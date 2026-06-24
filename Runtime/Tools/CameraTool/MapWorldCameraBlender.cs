using System;
using System.Reflection;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.Tools.CameraTool

{
    /// <summary>
    /// 地图相机与世界（3D）相机之间的画面渐变混合；不驱动位姿，运动由 Cinemachine 等外部系统负责。
    /// </summary>
    [DisallowMultipleComponent]
    public class MapWorldCameraBlender : MonoBehaviour
    {
        public enum ViewBlendState

        {
            MapOnly,

            Blending,

            WorldOnly
        }


        /// <summary>
        /// 混合目标：<see cref="World"/> 权重 0→1（最终仅世界相机），<see cref="Map"/> 权重 1→0（最终仅地图相机）。
        /// </summary>
        public enum ViewBlendTarget

        {
            World = 0,

            Map = 1
        }

        private const string DefaultMapTextureShaderProperty = "_MapTex";
        private const string DefaultWorldTextureShaderProperty = "_WorldTex";
        private const string DefaultBlendShaderProperty = "_BlendFactor";


        [Header("Cameras")]
        [SerializeField] private Camera m_mapCamera;


        [SerializeField] private Camera m_worldCamera;


        [Header("Blend Timing")]
        [SerializeField] private float m_blendStartDelay = 0.05f;


        [SerializeField] private float m_blendDuration = 0.35f;


        [SerializeField] private AnimationCurve m_blendCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);


        [Header("Render Texture")]
        [SerializeField] private bool m_useRenderTextureBlend = true;


        [SerializeField] [Range(0.25f, 1f)] private float m_renderTextureScale = 0.75f;


        [SerializeField] private int m_renderTextureDepth = 16;


        [Tooltip("计算 RT 尺寸用的参考相机；为空则用地图相机，再退回世界相机。")]
        [SerializeField] private Camera m_sizeReferenceCamera;


        [SerializeField] private Material m_blendMaterial;


        [SerializeField] private string m_mapTextureShaderProperty = "_MapTex";


        [SerializeField] private string m_worldTextureShaderProperty = "_WorldTex";


        [SerializeField] private string m_blendShaderProperty = "_BlendFactor";


        [Header("Overlay UI")]
        [Tooltip("全屏混合 RawImage；过渡期间显示，结束后隐藏。")]
        [SerializeField] private RawImage m_blendOverlay;


        [SerializeField] private bool m_autoShowHideOverlay = true;


        [Tooltip("显示混合层时关闭 Raycast，避免挡住地图点击。")]
        [SerializeField] private bool m_overlayIgnoreRaycastWhenVisible = true;


        [Header("Behavior")]
        [SerializeField] private bool m_applyMapCameraOptimizations = true;


        [SerializeField] private bool m_disableMapCameraInWorldView = true;


        [SerializeField] private bool m_autoCompleteBlend = true;


        public ViewBlendState CurrentState { get; private set; } = ViewBlendState.MapOnly;


        /// <summary>0 = 纯地图，1 = 纯世界。</summary>

        public float BlendFactor { get; private set; }


        /// <summary>当前混合目标（仅在 <see cref="ViewBlendState.Blending"/> 时有效）。</summary>

        public ViewBlendTarget ActiveTarget { get; private set; }


        public Camera MapCamera => m_mapCamera;


        /// <summary>世界/场景相机（挂 CinemachineBrain 的物理相机）。</summary>

        public Camera WorldCamera => m_worldCamera;


        private RenderTexture _mapRenderTexture;

        private RenderTexture _worldRenderTexture;

        private int _referenceWidth;

        private int _referenceHeight;

        private float _blendElapsed;

        private float _blendTotalDuration;

        private bool _blendTowardsMap;

        private ViewBlendState _stateBeforeBlend;

        private Material _blendMaterialInstance;

        private bool _ownsBlendMaterialInstance;

        private float _lastBlendFactorSentToMaterial = float.NaN;

        private bool _worldCameraAllowHdrCached;

        private bool _hasWorldCameraAllowHdrCache;

        private string _resolvedMapTextureShaderProperty;

        private string _resolvedWorldTextureShaderProperty;

        private string _resolvedBlendShaderProperty;


        private void Awake()

        {
            if (m_applyMapCameraOptimizations)

            {
                ApplyMapCameraOptimizations();
            }


            SetupBlendMaterialInstance();

            ResolveBlendShaderPropertyNames();
        }


        private void Start()

        {
            EnterMapOnlyState();
        }


        private void Update()

        {
            if (CurrentState != ViewBlendState.Blending)

            {
                return;
            }


            TickBlend(Time.deltaTime);
        }


        private void OnDisable()

        {
            ReleaseRenderTextures();

            ClearBlendMaterialTextures();

            SetOverlayVisible(false);
        }


        private void OnDestroy()

        {
            ReleaseRenderTextures();

            ClearBlendMaterialTextures();

            if (_ownsBlendMaterialInstance && _blendMaterialInstance != null)

            {
                Destroy(_blendMaterialInstance);

                _blendMaterialInstance = null;
            }
        }


        [Button]
        public void BlendToWorld()

        {
            StartBlend(ViewBlendTarget.World);
        }


        [Button]
        public void BlendToMap()

        {
            StartBlend(ViewBlendTarget.Map);
        }


        /// <summary>开始地图 ↔ 世界画面混合。</summary>
        public void StartBlend(ViewBlendTarget target)
        {
            if (!ValidateCameraReferences())

            {
                return;
            }


            if (target == ViewBlendTarget.Map && CurrentState == ViewBlendState.MapOnly)
            {
                Debug.LogWarning(
                    $"{nameof(MapWorldCameraBlender)}: 混合到地图应在非「仅地图」状态下调用（当前已是 MapOnly）。",
                    this);

                return;
            }

            _stateBeforeBlend = CurrentState;

            ActiveTarget = target;

            _blendTowardsMap = target == ViewBlendTarget.Map;

            _blendElapsed = 0f;

            _blendTotalDuration = Mathf.Max(m_blendStartDelay + m_blendDuration, 0.01f);

            EnableBothCameras();

            CacheReferenceDimensions();

            EnsureRenderTextures();

            ApplyBlendFactor(_blendTowardsMap ? 1f : 0f);

            SetOverlayVisible(true);


            CurrentState = ViewBlendState.Blending;
        }


        /// <summary>取消混合并恢复到本次混合前的稳定状态。</summary>
        public void CancelBlend()

        {
            if (CurrentState != ViewBlendState.Blending)

            {
                return;
            }


            _blendTowardsMap = false;


            if (_stateBeforeBlend == ViewBlendState.WorldOnly)

            {
                EnterWorldOnlyState();
            }

            else

            {
                EnterMapOnlyState();
            }
        }


        /// <summary>立即完成混合并进入目标稳定状态。</summary>
        public void CompleteBlend()

        {
            if (CurrentState != ViewBlendState.Blending)

            {
                return;
            }


            FinishBlendToTarget();
        }


        /// <summary>混合过程中刷新 RT 基准尺寸，必要时重建 RT。</summary>
        public void RefreshRenderTextureSize(bool useScreenPixelSize)

        {
            if (useScreenPixelSize)

            {
                _referenceWidth = Screen.width;

                _referenceHeight = Screen.height;
            }

            else

            {
                CacheReferenceDimensions();
            }


            if (CurrentState == ViewBlendState.Blending && m_useRenderTextureBlend)

            {
                EnsureRenderTextures();
            }
        }


        /// <summary>对地图相机应用低开销渲染建议。</summary>
        public void ApplyMapCameraOptimizations()

        {
            if (m_mapCamera == null)

            {
                return;
            }


            m_mapCamera.allowMSAA = false;

            m_mapCamera.allowHDR = false;

            m_mapCamera.useOcclusionCulling = false;

            TryApplyUrpCameraOptimizations(m_mapCamera);
        }


        private void TickBlend(float deltaTime)

        {
            _blendElapsed += Mathf.Max(0f, deltaTime);


            float t = m_blendDuration <= 0f
                ? 1f
                : Mathf.Clamp01((_blendElapsed - m_blendStartDelay) / m_blendDuration);

            float curveValue = m_blendCurve.Evaluate(t);

            ApplyBlendFactor(_blendTowardsMap ? 1f - curveValue : curveValue);


            if (m_autoCompleteBlend && _blendElapsed >= _blendTotalDuration)

            {
                FinishBlendToTarget();
            }
        }


        private void EnterMapOnlyState()

        {
            CurrentState = ViewBlendState.MapOnly;

            ApplyBlendFactor(0f);

            SetOverlayVisible(false);


            _referenceWidth = 0;

            _referenceHeight = 0;


            if (m_mapCamera != null)

            {
                m_mapCamera.enabled = true;

                m_mapCamera.targetTexture = null;
            }


            if (m_worldCamera != null)

            {
                m_worldCamera.enabled = false;

                m_worldCamera.targetTexture = null;

                RestoreWorldCameraHdrIfCached();
            }


            ReleaseRenderTextures();

            ClearBlendMaterialTextures();
        }


        private void EnterWorldOnlyState()

        {
            CurrentState = ViewBlendState.WorldOnly;

            ApplyBlendFactor(1f);

            SetOverlayVisible(false);


            _referenceWidth = 0;

            _referenceHeight = 0;


            if (m_worldCamera != null)

            {
                m_worldCamera.enabled = true;

                m_worldCamera.targetTexture = null;

                RestoreWorldCameraHdrIfCached();
            }


            if (m_mapCamera != null)

            {
                m_mapCamera.enabled = !m_disableMapCameraInWorldView;

                m_mapCamera.targetTexture = null;
            }


            ReleaseRenderTextures();

            ClearBlendMaterialTextures();
        }


        private void EnableBothCameras()

        {
            if (m_mapCamera != null)

            {
                m_mapCamera.enabled = true;
            }


            if (m_worldCamera != null)

            {
                m_worldCamera.enabled = true;
            }
        }


        private void FinishBlendToTarget()

        {
            _blendTowardsMap = false;


            if (ActiveTarget == ViewBlendTarget.Map)

            {
                ApplyBlendFactor(0f);

                EnterMapOnlyState();
            }

            else

            {
                ApplyBlendFactor(1f);

                EnterWorldOnlyState();
            }
        }


        private bool ValidateCameraReferences()

        {
            if (m_mapCamera == null || m_worldCamera == null)

            {
                Debug.LogWarning($"{nameof(MapWorldCameraBlender)}: 请指定地图相机与世界相机。", this);

                return false;
            }


            if (m_useRenderTextureBlend && m_blendOverlay == null)

            {
                Debug.LogWarning($"{nameof(MapWorldCameraBlender)}: 已启用 RT 混合但未指定混合 RawImage。", this);

                return false;
            }


            return true;
        }


        private void SetupBlendMaterialInstance()

        {
            _blendMaterialInstance = m_blendMaterial;

            _ownsBlendMaterialInstance = false;


            if (m_blendOverlay == null)

            {
                return;
            }


            if (m_blendMaterial != null)

            {
                var instance = new Material(m_blendMaterial);

                m_blendOverlay.material = instance;

                _blendMaterialInstance = instance;

                _ownsBlendMaterialInstance = true;
            }

            else if (m_blendOverlay.material != null)

            {
                _blendMaterialInstance = m_blendOverlay.material;
            }


            if (m_autoShowHideOverlay)

            {
                m_blendOverlay.gameObject.SetActive(false);
            }
        }

        private void ResolveBlendShaderPropertyNames()

        {
            _resolvedMapTextureShaderProperty = ResolveShaderPropertyName(
                m_mapTextureShaderProperty,
                DefaultMapTextureShaderProperty);

            _resolvedWorldTextureShaderProperty = ResolveShaderPropertyName(
                m_worldTextureShaderProperty,
                DefaultWorldTextureShaderProperty);

            _resolvedBlendShaderProperty = ResolveShaderPropertyName(
                m_blendShaderProperty,
                DefaultBlendShaderProperty);
        }


        private string ResolveShaderPropertyName(string configuredPropertyName, string fallbackPropertyName)

        {
            if (_blendMaterialInstance == null)

            {
                return string.IsNullOrEmpty(configuredPropertyName) ? fallbackPropertyName : configuredPropertyName;
            }


            if (!string.IsNullOrEmpty(configuredPropertyName) &&
                _blendMaterialInstance.HasProperty(configuredPropertyName))

            {
                return configuredPropertyName;
            }


            if (_blendMaterialInstance.HasProperty(fallbackPropertyName))

            {
                return fallbackPropertyName;
            }


            return configuredPropertyName;
        }


        private void SetOverlayVisible(bool visible)

        {
            if (!m_autoShowHideOverlay || m_blendOverlay == null)

            {
                return;
            }


            m_blendOverlay.gameObject.SetActive(visible);

            if (visible && m_overlayIgnoreRaycastWhenVisible)

            {
                m_blendOverlay.raycastTarget = false;
            }
        }


        private void ClearBlendMaterialTextures()

        {
            if (_blendMaterialInstance == null)

            {
                return;
            }


            if (!string.IsNullOrEmpty(_resolvedMapTextureShaderProperty))

            {
                _blendMaterialInstance.SetTexture(_resolvedMapTextureShaderProperty, null);
            }


            if (!string.IsNullOrEmpty(_resolvedWorldTextureShaderProperty))

            {
                _blendMaterialInstance.SetTexture(_resolvedWorldTextureShaderProperty, null);
            }
        }


        private Camera GetSizeReferenceCamera()

        {
            if (m_sizeReferenceCamera != null)

            {
                return m_sizeReferenceCamera;
            }


            if (m_mapCamera != null)

            {
                return m_mapCamera;
            }


            return m_worldCamera;
        }


        private void CacheReferenceDimensions()

        {
            Camera refCam = GetSizeReferenceCamera();

            if (refCam != null)

            {
                _referenceWidth = refCam.pixelWidth;

                _referenceHeight = refCam.pixelHeight;
            }

            else

            {
                _referenceWidth = Screen.width;

                _referenceHeight = Screen.height;
            }
        }


        private void GetRenderTextureDimensions(out int width, out int height)

        {
            int bw = _referenceWidth;

            int bh = _referenceHeight;

            if (bw <= 0 || bh <= 0)

            {
                Camera refCam = GetSizeReferenceCamera();

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


            width = Mathf.Max(64, Mathf.RoundToInt(bw * m_renderTextureScale));

            height = Mathf.Max(64, Mathf.RoundToInt(bh * m_renderTextureScale));
        }


        private void EnsureRenderTextures()

        {
            if (!m_useRenderTextureBlend || m_mapCamera == null || m_worldCamera == null)

            {
                return;
            }


            GetRenderTextureDimensions(out int width, out int height);


            bool dimensionsMatch = _mapRenderTexture != null && _worldRenderTexture != null &&
                                   _mapRenderTexture.IsCreated() && _worldRenderTexture.IsCreated() &&
                                   _mapRenderTexture.width == width && _mapRenderTexture.height == height &&
                                   _worldRenderTexture.width == width && _worldRenderTexture.height == height;

            if (dimensionsMatch)

            {
                BindCamerasAndMaterial();

                return;
            }


            ReleaseRenderTextures();


            _mapRenderTexture = new RenderTexture(width, height, m_renderTextureDepth, RenderTextureFormat.Default)

            {
                name = "MapWorldBlend_MapRT"
            };

            _worldRenderTexture = new RenderTexture(width, height, m_renderTextureDepth, RenderTextureFormat.Default)

            {
                name = "MapWorldBlend_WorldRT"
            };


            _mapRenderTexture.Create();

            _worldRenderTexture.Create();


            BindCamerasAndMaterial();
        }


        private void BindCamerasAndMaterial()

        {
            if (m_mapCamera != null && m_mapCamera.targetTexture != _mapRenderTexture)

            {
                m_mapCamera.targetTexture = _mapRenderTexture;
            }


            if (m_worldCamera != null)

            {
                if (!_hasWorldCameraAllowHdrCache)

                {
                    _worldCameraAllowHdrCached = m_worldCamera.allowHDR;

                    _hasWorldCameraAllowHdrCache = true;
                }


                // URP 下 HDR 相机写入 LDR RT 时世界画面可能全黑

                m_worldCamera.allowHDR = false;


                if (m_worldCamera.targetTexture != _worldRenderTexture)

                {
                    m_worldCamera.targetTexture = _worldRenderTexture;
                }
            }


            ApplyBlendTexturesToMaterial();
        }


        private void ApplyBlendTexturesToMaterial()

        {
            if (_blendMaterialInstance == null || _mapRenderTexture == null || _worldRenderTexture == null)

            {
                return;
            }


            if (!string.IsNullOrEmpty(_resolvedMapTextureShaderProperty))

            {
                _blendMaterialInstance.SetTexture(_resolvedMapTextureShaderProperty, _mapRenderTexture);
            }


            if (!string.IsNullOrEmpty(_resolvedWorldTextureShaderProperty))

            {
                _blendMaterialInstance.SetTexture(_resolvedWorldTextureShaderProperty, _worldRenderTexture);
            }


            if (m_blendOverlay == null)

            {
                return;
            }


            // RawImage 的 CanvasRenderer 依赖 mainTexture；仅 SetMaterial 自定义属性在部分 URP/UI 路径下会全黑

            m_blendOverlay.texture = _mapRenderTexture;

            m_blendOverlay.SetMaterialDirty();
        }


        private void RestoreWorldCameraHdrIfCached()

        {
            if (m_worldCamera == null || !_hasWorldCameraAllowHdrCache)

            {
                return;
            }


            m_worldCamera.allowHDR = _worldCameraAllowHdrCached;
        }


        private void ReleaseRenderTextures()

        {
            if (m_mapCamera != null && m_mapCamera.targetTexture == _mapRenderTexture)

            {
                m_mapCamera.targetTexture = null;
            }


            if (m_worldCamera != null && m_worldCamera.targetTexture == _worldRenderTexture)

            {
                m_worldCamera.targetTexture = null;
            }


            if (_mapRenderTexture != null)

            {
                _mapRenderTexture.Release();

                Destroy(_mapRenderTexture);

                _mapRenderTexture = null;
            }


            if (_worldRenderTexture != null)

            {
                _worldRenderTexture.Release();

                Destroy(_worldRenderTexture);

                _worldRenderTexture = null;
            }
        }


        private void ApplyBlendFactor(float factor)

        {
            BlendFactor = Mathf.Clamp01(factor);

            if (_blendMaterialInstance == null)

            {
                return;
            }


            if (Mathf.Approximately(BlendFactor, _lastBlendFactorSentToMaterial))

            {
                return;
            }


            _lastBlendFactorSentToMaterial = BlendFactor;

            if (!string.IsNullOrEmpty(_resolvedBlendShaderProperty))

            {
                _blendMaterialInstance.SetFloat(_resolvedBlendShaderProperty, BlendFactor);
            }
        }


        private static void TryApplyUrpCameraOptimizations(Camera mapCamera)

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
