using System;
using System.Reflection;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.Tools.CameraTool
{
    /// <summary>
    /// 双相机切换控制器：
    /// 1. 非切换期仅激活一个相机；
    /// 2. 过渡期短时启用双相机并写入 RT；
    /// 3. 通过材质参数驱动屏幕空间混合；
    /// 4. 位姿由两相机公共父节点 <see cref="m_cameraRig"/>（或自动解析的同级父节点）统一插值，不再单独驱动某一台相机。
    /// </summary>
    [DisallowMultipleComponent]
    public class MapCameraTransitionController : MonoBehaviour
    {
        public enum TransitionState
        {
            IdleMapOnly,
            Transitioning,
            Idle3DOnly
        }

        /// <summary>
        /// 混合结束态：<see cref="TowardsDive"/> 以 3D 为主（混合 0→1），<see cref="TowardsMap"/> 以地图为主（混合 1→0）。
        /// </summary>
        public enum MapCameraTransitionBlend
        {
            TowardsDive = 0,
            TowardsMap = 1
        }

        [Header("Camera References")]
        [Tooltip("两台相机的公共父节点；过渡时只移动该节点的世界位姿。未指定且两相机 parent 相同时会自动使用该 parent。")]
        [SerializeField] private Transform m_cameraRig;

        [SerializeField] private Camera m_topMapCamera;

        [SerializeField] private Camera m_dive3DCamera;

        [Header("Transition Timing")]
        [SerializeField] private float m_sinkDuration = 0.8f;

        [SerializeField] private float m_blendDelay = 0.05f;
        [SerializeField] private float m_blendDuration = 0.35f;
        [SerializeField] private AnimationCurve m_sinkCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve m_blendCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("RenderTexture Blend")]
        [SerializeField] private bool m_enableRenderTextureBlend = true;

        [SerializeField] [Range(0.25f, 1f)] private float m_rtResolutionScale = 0.75f;
        [SerializeField] private int m_rtDepthBuffer = 16;

        [Tooltip(
            "用于计算 RT 宽高；为空则用俯视地图相机（再退回下沉相机）。在 BeginTransition 时读取 Camera.pixelWidth/Height（绑定 RT 之前），与 URP 渲染缩放后的实际输出一致。")]
        [SerializeField] private Camera m_rtSizeReferenceCamera;

        [SerializeField] private Material m_blendMaterial;
        [SerializeField] private string m_mapTextureProperty = "_MapTex";
        [SerializeField] private string m_diveTextureProperty = "_DiveTex";
        [SerializeField] private string m_blendProperty = "_Blend";

        [Header("Blend UI (optional)")]
        [Tooltip("全屏混合用 RawImage；过渡期间自动显示，结束后自动隐藏，避免空 RT 时挡住主相机。")]
        [SerializeField] private RawImage m_blendRawImage;

        [SerializeField] private bool m_autoManageBlendUi = true;

        [Tooltip("显示混合层时关闭 Raycast，避免挡住地图点击（按需关闭本项）。")]
        [SerializeField] private bool m_blendUiDisableRaycastWhenVisible = true;

        [Header("Behavior")]
        [SerializeField] private bool m_autoApplyMapCameraLowCostConfig = true;

        [SerializeField] private bool m_mapCameraActiveOnlyInTransition = true;
        [SerializeField] private bool m_autoCompleteTransition = true;

        public TransitionState State { get; private set; } = TransitionState.IdleMapOnly;
        public float BlendAlpha { get; private set; }

        /// <summary>当前或最近一次过渡的目标世界位置。</summary>
        public Vector3 TransitionTargetPosition { get; private set; }

        /// <summary>当前或最近一次过渡的目标世界旋转。</summary>
        public Quaternion TransitionTargetRotation { get; private set; }

        /// <summary>当前过渡的混合方向（仅在 Transitioning 时有效）。</summary>
        public MapCameraTransitionBlend ActiveBlendMode { get; private set; }

        private RenderTexture _mapRT;
        private RenderTexture _diveRT;

        /// <summary>
        /// 绑定 RT 之前缓存的参考视口像素（避免绑定后 Camera.pixelWidth 变成 RT 宽高导致无法与真实输出对齐）。
        /// </summary>
        private int _blendRtBaseWidth;

        private int _blendRtBaseHeight;

        private float _elapsed;
        private float _totalDuration;

        private Vector3 _rigStartPos;
        private Quaternion _rigStartRot;

        private bool _hasTransitionSnapshot;

        /// <summary>
        /// true 表示混合从 1→0（TowardsMap）；false 为 0→1（TowardsDive）。
        /// </summary>
        private bool _reverseTransition;

        /// <summary>
        /// 进入本次过渡前的状态（用于 Cancel 时恢复正确 Idle）。
        /// </summary>
        private TransitionState _stateBeforeThisTransition;

        /// <summary>
        /// 实际写入 RT 与混合参数的材质（与 RawImage 共用同一份运行时实例）。
        /// </summary>
        private Material m_blendMaterialRuntime;

        private bool _ownsBlendMaterialInstance;

        /// <summary>
        /// 本次过渡开始时解析的 rig，避免每帧重复 <see cref="ResolveCameraRigTransform"/>。
        /// </summary>
        private Transform _cachedRigForTransition;

        /// <summary>避免对材质重复写入相同混合值。</summary>
        private float _lastBlendSentToMaterial = float.NaN;

        private void Awake()
        {
            if (m_autoApplyMapCameraLowCostConfig)
            {
                ApplyMapCameraLowCostConfig();
            }

            SetupBlendMaterialRuntime();
        }

        private void Start()
        {
            EnterIdleMapOnly();
        }

        private void Update()
        {
            if (State != TransitionState.Transitioning)
            {
                return;
            }

            TickTransition(Time.deltaTime);
        }

        private void OnDisable()
        {
            ReleaseRT();
            ClearBlendMaterialTextures();
            SetBlendUiVisible(false);
        }

        private void OnDestroy()
        {
            ReleaseRT();
            ClearBlendMaterialTextures();
            if (_ownsBlendMaterialInstance && m_blendMaterialRuntime != null)
            {
                Destroy(m_blendMaterialRuntime);
                m_blendMaterialRuntime = null;
            }
        }

        [Button]
        public void TestTowardsDive()
        {
            BeginTransition(new Vector3(-9.90924549f, 37.1648788f, -21.0386181f),
                Quaternion.Euler(new Vector3(51.4297676f, 20.3860226f, -0.000260182656f)),
                MapCameraTransitionBlend.TowardsDive);
        }

        [Button]
        public void TestTowardsMap()
        {
            BeginTransition(new Vector3(0,10,0),
                Quaternion.Euler(new Vector3(90,0,0)),
                MapCameraTransitionBlend.TowardsMap);
        }

        /// <summary>
        /// 使用场景中 Transform 的世界位姿作为过渡终点。
        /// </summary>
        public void BeginTransition(Transform targetWorldPose, MapCameraTransitionBlend blendMode)
        {
            if (targetWorldPose == null)
            {
                Debug.LogWarning(
                    $"{nameof(MapCameraTransitionController)}.{nameof(BeginTransition)}: targetWorldPose 为空。");
                return;
            }

            BeginTransition(targetWorldPose.position, targetWorldPose.rotation, blendMode);
        }

        /// <summary>
        /// 开始过渡：通过 <see cref="m_cameraRig"/>（或两相机公共父节点）插值到目标世界位姿。
        /// </summary>
        public void BeginTransition(Vector3 targetWorldPosition, Quaternion targetWorldRotation,
            MapCameraTransitionBlend blendMode)
        {
            if (!ValidateCameraReferences())
            {
                return;
            }

            if (blendMode == MapCameraTransitionBlend.TowardsMap && State == TransitionState.IdleMapOnly)
            {
                Debug.LogWarning($"{nameof(MapCameraTransitionController)}: TowardsMap 应在非「仅地图」状态下调用。");
                return;
            }

            _stateBeforeThisTransition = State;
            ActiveBlendMode = blendMode;
            _reverseTransition = blendMode == MapCameraTransitionBlend.TowardsMap;

            TransitionTargetPosition = targetWorldPosition;
            TransitionTargetRotation = targetWorldRotation;

            _elapsed = 0f;
            _totalDuration = Mathf.Max(m_sinkDuration, m_blendDelay + m_blendDuration);
            _totalDuration = Mathf.Max(_totalDuration, 0.01f);

            CacheTransitionSnapshot();
            EnableTransitionCameras();
            CacheBlendRtBaseDimensions();
            EnsureRenderTexture();
            ApplyBlend(_reverseTransition ? 1f : 0f);
            SetBlendUiVisible(true);

            State = TransitionState.Transitioning;
        }

        /// <summary>
        /// 取消当前过渡并恢复到进入本次过渡前的 Idle 状态。
        /// </summary>
        public void CancelTransition()
        {
            if (_hasTransitionSnapshot)
            {
                RestoreTransitionSnapshot();
            }

            _reverseTransition = false;

            if (_stateBeforeThisTransition == TransitionState.Idle3DOnly)
            {
                EnterIdle3DOnly();
                ApplyBlend(1f);
            }
            else
            {
                EnterIdleMapOnly();
                ApplyBlend(0f);
            }
        }

        /// <summary>
        /// 立即完成当前过渡（与自动结束效果一致：按当前 <see cref="ActiveBlendMode"/> 进入地图或 3D Idle）。
        /// </summary>
        public void CompleteTransition()
        {
            if (State != TransitionState.Transitioning)
            {
                return;
            }

            CompleteTransitionToEndState();
        }

        /// <summary>
        /// 过渡中按当前策略刷新 RT 基准尺寸并必要时重建 RT。
        /// </summary>
        /// <param name="useScreenPixelSize">为 true 时使用 <see cref="Screen"/> 宽高；为 false 时使用参考相机 <see cref="GetRtReferenceCamera"/> 的像素（须在相机未指向本脚本 RT 时调用才准确）。</param>
        public void RefreshBlendRtDimensions(bool useScreenPixelSize)
        {
            if (useScreenPixelSize)
            {
                _blendRtBaseWidth = Screen.width;
                _blendRtBaseHeight = Screen.height;
            }
            else
            {
                CacheBlendRtBaseDimensions();
            }

            if (State == TransitionState.Transitioning && m_enableRenderTextureBlend)
            {
                EnsureRenderTexture();
            }
        }

        /// <summary>
        /// 仅对二维地图相机应用低开销参数建议。
        /// </summary>
        public void ApplyMapCameraLowCostConfig()
        {
            if (m_topMapCamera == null)
            {
                return;
            }

            // 低开销建议（二维地图相机）：
            // 1) 关闭MSAA：屏幕空间混合阶段可减少显著带宽消耗；
            // 2) 关闭HDR：地图层通常不依赖高动态范围；
            // 3) 关闭遮挡剔除：俯视地图常为规则区域，可避免额外CPU剔除成本。
            // 注意：这些设置只建议用于二维地图相机，不建议直接套用到3D主相机。
            m_topMapCamera.allowMSAA = false;
            m_topMapCamera.allowHDR = false;
            m_topMapCamera.useOcclusionCulling = false;

            // URP 可选优化（反射安全处理，不依赖包编译）：
            // - 关闭后处理 renderPostProcessing
            // - 关闭阴影 renderShadows
            // - 抗锯齿模式设为None antialiasing
            TryApplyUrpCameraLowCostConfig(m_topMapCamera);
        }

        private void TickTransition(float deltaTime)
        {
            _elapsed += Mathf.Max(0f, deltaTime);

            float sinkT = m_sinkDuration <= 0f ? 1f : Mathf.Clamp01(_elapsed / m_sinkDuration);
            float sinkValue = m_sinkCurve.Evaluate(sinkT);
            UpdateCameraPose(sinkValue);

            float blendT = m_blendDuration <= 0f
                ? 1f
                : Mathf.Clamp01((_elapsed - m_blendDelay) / m_blendDuration);
            float blendValue = m_blendCurve.Evaluate(blendT);
            if (_reverseTransition)
            {
                ApplyBlend(1f - blendValue);
            }
            else
            {
                ApplyBlend(blendValue);
            }

            if (m_autoCompleteTransition && _elapsed >= _totalDuration)
            {
                CompleteTransitionToEndState();
            }
        }

        private void UpdateCameraPose(float t)
        {
            Transform rig = _cachedRigForTransition != null ? _cachedRigForTransition : ResolveCameraRigTransform();
            if (rig == null)
            {
                return;
            }

            rig.position = Vector3.Lerp(_rigStartPos, TransitionTargetPosition, t);
            rig.rotation = Quaternion.Slerp(_rigStartRot, TransitionTargetRotation, t);
        }

        private void EnterIdleMapOnly()
        {
            State = TransitionState.IdleMapOnly;
            _cachedRigForTransition = null;
            ApplyBlend(0f);
            SetBlendUiVisible(false);

            _blendRtBaseWidth = 0;
            _blendRtBaseHeight = 0;

            if (m_topMapCamera != null)
            {
                m_topMapCamera.enabled = true;
                m_topMapCamera.targetTexture = null;
            }

            if (m_dive3DCamera != null)
            {
                m_dive3DCamera.enabled = false;
                m_dive3DCamera.targetTexture = null;
            }

            ReleaseRT();
            ClearBlendMaterialTextures();
        }

        private void EnterIdle3DOnly()
        {
            State = TransitionState.Idle3DOnly;
            _cachedRigForTransition = null;
            ApplyBlend(1f);
            SetBlendUiVisible(false);

            _blendRtBaseWidth = 0;
            _blendRtBaseHeight = 0;

            if (m_dive3DCamera != null)
            {
                m_dive3DCamera.enabled = true;
                m_dive3DCamera.targetTexture = null;
            }

            if (m_topMapCamera != null)
            {
                m_topMapCamera.enabled = !m_mapCameraActiveOnlyInTransition;
                m_topMapCamera.targetTexture = null;
            }

            ReleaseRT();
            ClearBlendMaterialTextures();
        }

        private void EnableTransitionCameras()
        {
            if (m_topMapCamera != null)
            {
                m_topMapCamera.enabled = true;
            }

            if (m_dive3DCamera != null)
            {
                m_dive3DCamera.enabled = true;
            }
        }

        private void CacheTransitionSnapshot()
        {
            Transform rig = ResolveCameraRigTransform();
            if (rig == null)
            {
                return;
            }

            _cachedRigForTransition = rig;
            _rigStartPos = rig.position;
            _rigStartRot = rig.rotation;
            _hasTransitionSnapshot = true;
        }

        private void RestoreTransitionSnapshot()
        {
            Transform rig = _cachedRigForTransition != null ? _cachedRigForTransition : ResolveCameraRigTransform();
            if (rig == null)
            {
                _hasTransitionSnapshot = false;
                return;
            }

            rig.SetPositionAndRotation(_rigStartPos, _rigStartRot);
            _hasTransitionSnapshot = false;
        }

        /// <summary>
        /// 解析用于插值位姿的父节点：优先 <see cref="m_cameraRig"/>；否则若两相机 parent 相同且非空则使用该 parent。
        /// </summary>
        private Transform ResolveCameraRigTransform()
        {
            if (m_cameraRig != null)
            {
                return m_cameraRig;
            }

            if (m_topMapCamera == null || m_dive3DCamera == null)
            {
                return null;
            }

            Transform p1 = m_topMapCamera.transform.parent;
            Transform p2 = m_dive3DCamera.transform.parent;
            if (p1 != null && p1 == p2)
            {
                return p1;
            }

            return null;
        }

        private void CompleteTransitionToEndState()
        {
            bool towardsMap = ActiveBlendMode == MapCameraTransitionBlend.TowardsMap;
            _reverseTransition = false;

            if (towardsMap)
            {
                ApplyBlend(0f);
                EnterIdleMapOnly();
            }
            else
            {
                ApplyBlend(1f);
                EnterIdle3DOnly();
            }
        }

        private bool ValidateCameraReferences()
        {
            if (m_topMapCamera == null || m_dive3DCamera == null)
            {
                Debug.LogWarning($"{nameof(MapCameraTransitionController)} camera references are missing.");
                return false;
            }

            if (ResolveCameraRigTransform() == null)
            {
                Debug.LogWarning(
                    $"{nameof(MapCameraTransitionController)}: 请指定 {nameof(m_cameraRig)}，或确保两台相机挂在同一 Transform 父节点下。");
                return false;
            }

            return true;
        }

        private void SetupBlendMaterialRuntime()
        {
            m_blendMaterialRuntime = m_blendMaterial;
            _ownsBlendMaterialInstance = false;

            if (m_blendRawImage == null)
            {
                return;
            }

            if (m_blendMaterial != null)
            {
                var instance = new Material(m_blendMaterial);
                m_blendRawImage.material = instance;
                m_blendMaterialRuntime = instance;
                _ownsBlendMaterialInstance = true;
            }
            else if (m_blendRawImage.material != null)
            {
                m_blendMaterialRuntime = m_blendRawImage.material;
            }

            if (m_autoManageBlendUi)
            {
                m_blendRawImage.gameObject.SetActive(false);
            }
        }

        private void SetBlendUiVisible(bool visible)
        {
            if (!m_autoManageBlendUi || m_blendRawImage == null)
            {
                return;
            }

            m_blendRawImage.gameObject.SetActive(visible);
            if (visible && m_blendUiDisableRaycastWhenVisible)
            {
                m_blendRawImage.raycastTarget = false;
            }
        }

        private void ClearBlendMaterialTextures()
        {
            if (m_blendMaterialRuntime == null)
            {
                return;
            }

            m_blendMaterialRuntime.SetTexture(m_mapTextureProperty, null);
            m_blendMaterialRuntime.SetTexture(m_diveTextureProperty, null);
        }

        private Camera GetRtReferenceCamera()
        {
            if (m_rtSizeReferenceCamera != null)
            {
                return m_rtSizeReferenceCamera;
            }

            if (m_topMapCamera != null)
            {
                return m_topMapCamera;
            }

            return m_dive3DCamera;
        }

        private void CacheBlendRtBaseDimensions()
        {
            Camera refCam = GetRtReferenceCamera();
            if (refCam != null)
            {
                _blendRtBaseWidth = refCam.pixelWidth;
                _blendRtBaseHeight = refCam.pixelHeight;
            }
            else
            {
                _blendRtBaseWidth = Screen.width;
                _blendRtBaseHeight = Screen.height;
            }
        }

        /// <summary>
        /// RT 尺寸与「绑定 RT 前」参考相机视口像素一致（含 URP Render Scale），再乘 m_rtResolutionScale。
        /// </summary>
        private void GetBlendRenderTextureDimensions(out int width, out int height)
        {
            int bw = _blendRtBaseWidth;
            int bh = _blendRtBaseHeight;
            if (bw <= 0 || bh <= 0)
            {
                Camera refCam = GetRtReferenceCamera();
                if (refCam != null)
                {
                    bw = refCam.pixelWidth;
                    bh = refCam.pixelHeight;
                }
                else
                {
                    bw = Screen.width;
                    bh = Screen.height;
                }
            }

            width = Mathf.Max(64, Mathf.RoundToInt(bw * m_rtResolutionScale));
            height = Mathf.Max(64, Mathf.RoundToInt(bh * m_rtResolutionScale));
        }

        private void EnsureRenderTexture()
        {
            if (!m_enableRenderTextureBlend)
            {
                return;
            }

            if (m_topMapCamera == null || m_dive3DCamera == null)
            {
                return;
            }

            GetBlendRenderTextureDimensions(out int width, out int height);

            bool sameRtDimensions = _mapRT != null && _diveRT != null && _mapRT.IsCreated() && _diveRT.IsCreated() &&
                                    _mapRT.width == width && _mapRT.height == height && _diveRT.width == width &&
                                    _diveRT.height == height;
            if (sameRtDimensions)
            {
                BindCamerasAndBlendMaterialToRenderTextures();
                return;
            }

            ReleaseRT();

            _mapRT = new RenderTexture(width, height, m_rtDepthBuffer, RenderTextureFormat.Default)
            {
                name = "TopMapTransitionRT"
            };
            _diveRT = new RenderTexture(width, height, m_rtDepthBuffer, RenderTextureFormat.Default)
            {
                name = "Dive3DTransitionRT"
            };

            _mapRT.Create();
            _diveRT.Create();

            BindCamerasAndBlendMaterialToRenderTextures();
        }

        /// <summary>将两台相机的 targetTexture 与混合材质贴图槽绑定到当前 _mapRT/_diveRT。</summary>
        private void BindCamerasAndBlendMaterialToRenderTextures()
        {
            if (m_topMapCamera != null && m_topMapCamera.targetTexture != _mapRT)
            {
                m_topMapCamera.targetTexture = _mapRT;
            }

            if (m_dive3DCamera != null && m_dive3DCamera.targetTexture != _diveRT)
            {
                m_dive3DCamera.targetTexture = _diveRT;
            }

            if (m_blendMaterialRuntime != null && _mapRT != null && _diveRT != null)
            {
                m_blendMaterialRuntime.SetTexture(m_mapTextureProperty, _mapRT);
                m_blendMaterialRuntime.SetTexture(m_diveTextureProperty, _diveRT);
            }
        }

        private void ReleaseRT()
        {
            if (m_topMapCamera != null && m_topMapCamera.targetTexture == _mapRT)
            {
                m_topMapCamera.targetTexture = null;
            }

            if (m_dive3DCamera != null && m_dive3DCamera.targetTexture == _diveRT)
            {
                m_dive3DCamera.targetTexture = null;
            }

            if (_mapRT != null)
            {
                _mapRT.Release();
                Destroy(_mapRT);
                _mapRT = null;
            }

            if (_diveRT != null)
            {
                _diveRT.Release();
                Destroy(_diveRT);
                _diveRT = null;
            }
        }

        private void ApplyBlend(float alpha)
        {
            BlendAlpha = Mathf.Clamp01(alpha);
            if (m_blendMaterialRuntime == null)
            {
                return;
            }

            if (Mathf.Approximately(BlendAlpha, _lastBlendSentToMaterial))
            {
                return;
            }

            _lastBlendSentToMaterial = BlendAlpha;
            m_blendMaterialRuntime.SetFloat(m_blendProperty, BlendAlpha);
        }

        private static void TryApplyUrpCameraLowCostConfig(Camera mapCamera)
        {
            Type cameraDataType =
                Type.GetType(
                    "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
            if (cameraDataType == null)
            {
                return;
            }

            Component urpData = mapCamera.GetComponent(cameraDataType);
            if (urpData == null)
            {
                return;
            }

            SetBoolProperty(cameraDataType, urpData, "renderPostProcessing", false);
            SetBoolProperty(cameraDataType, urpData, "renderShadows", false);
            SetEnumToZero(cameraDataType, urpData, "antialiasing");
        }

        private static void SetBoolProperty(Type type, object target, string propertyName, bool value)
        {
            PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null || property.PropertyType != typeof(bool) || !property.CanWrite)
            {
                return;
            }

            property.SetValue(target, value);
        }

        private static void SetEnumToZero(Type type, object target, string propertyName)
        {
            PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null || !property.CanWrite || !property.PropertyType.IsEnum)
            {
                return;
            }

            object value = Enum.ToObject(property.PropertyType, 0);
            property.SetValue(target, value);
        }
    }
}
