using NonsensicalKit.Tools.MeshTool;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NonsensicalKit.Tools.EasyTool
{
    public class RenderBox : MonoBehaviour
    {
        [SerializeField] private Vector3 m_center;
        [SerializeField] private Vector3 m_rotation;
        [SerializeField] private Vector3 m_extent;

        public Vector3 Center
        {
            get => m_center;
            set => m_center = value;
        }

        public Vector3 Rotation
        {
            get => m_rotation;
            set => m_rotation = value;
        }

        public Vector3 Extent
        {
            get => m_extent;
            set => m_extent = value;
        }

        public Vector3 Size
        {
            get => m_extent * 2f;
            set => m_extent = value * 0.5f;
        }

        private void Reset()
        {
            CalculateBox();
        }

        public void CalculateBox()
        {
            Quaternion qn = transform.rotation;
            transform.rotation = Quaternion.identity;

            bool hasBounds = false;

            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

            Renderer[] childRenderers = transform.GetComponentsInChildren<Renderer>();

            foreach (var item in childRenderers)
            {
                if (hasBounds)
                {
                    bounds.Encapsulate(item.bounds);
                }
                else
                {
                    bounds = item.bounds;
                    hasBounds = true;
                }
            }

            bounds.center -= transform.position;

            m_center = bounds.center;
            m_extent = bounds.extents;

            transform.rotation = qn;
        }

        public bool Contains(Vector3 worldPoint)
        {
            Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
            var boundsPoint = Quaternion.Euler(Rotation) * localPoint;
            Bounds bounds = new Bounds(Center, Size);
            return bounds.Contains(boundsPoint);
        }

        /// <summary>
        /// 盒子内部最近的点位
        /// </summary>
        /// <param name="worldPoint"></param>
        /// <returns></returns>
        public Vector3 NearestPoint(Vector3 worldPoint)
        {
            Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
            var boundsPoint = Quaternion.Euler(Rotation) * localPoint;
            Bounds bounds = new Bounds(Center, Size);
            if (bounds.Contains(boundsPoint))
            {
                return worldPoint;
            }
            else
            {
                boundsPoint.x = Mathf.Clamp(boundsPoint.x, bounds.center.x - bounds.extents.x, bounds.center.x + bounds.extents.x);
                boundsPoint.y = Mathf.Clamp(boundsPoint.y, bounds.center.y - bounds.extents.y, bounds.center.y + bounds.extents.y);
                boundsPoint.z = Mathf.Clamp(boundsPoint.z, bounds.center.z - bounds.extents.z, bounds.center.z + bounds.extents.z);

                var resultLocal = Quaternion.Inverse(Quaternion.Euler(Rotation)) * boundsPoint;

                var resultWorld = transform.TransformPoint(resultLocal);
                return resultWorld;
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(RenderBox))]
    public class RenderBoxEditor : Editor
    {
        private static readonly int HandleColor = Shader.PropertyToID("_HandleColor");
        private static readonly int HandleSize = Shader.PropertyToID("_HandleSize");
        private static readonly int ObjectToWorld = Shader.PropertyToID("_ObjectToWorld");
        private static readonly int HandleZTest = Shader.PropertyToID("_HandleZTest");

        private Color RealHandleColor =>
            Handles.color * new Color(1, 1, 1, .5f) + (Handles.lighting ? new Color(0, 0, 0, .5f) : new Color(0, 0, 0, 0));

        private Quaternion _crtRotation;
        private bool _editBox;
        private RenderBox _crtRenderBox;

        public void OnSceneGUI()
        {
            _crtRenderBox = target as RenderBox;
            if (_crtRenderBox == null)
            {
                return;
            }

            _crtRotation = _crtRenderBox.transform.rotation;
            _crtRotation *= Quaternion.Euler(_crtRenderBox.Rotation);

            Vector3 pos = _crtRenderBox.transform.position;
            Vector3 boundsCenter = pos + _crtRenderBox.Center;
            Vector3 boundsExtent = _crtRenderBox.Extent;

            Handles.color = Color.blue;

            Vector3 p1 = boundsCenter + _crtRotation * Vector3.Scale(boundsExtent, new Vector3(1, 1, 1));
            Vector3 p2 = boundsCenter + _crtRotation * Vector3.Scale(boundsExtent, new Vector3(1, 1, -1));
            Vector3 p3 = boundsCenter + _crtRotation * Vector3.Scale(boundsExtent, new Vector3(1, -1, 1));
            Vector3 p4 = boundsCenter + _crtRotation * Vector3.Scale(boundsExtent, new Vector3(1, -1, -1));
            Vector3 p5 = boundsCenter + _crtRotation * Vector3.Scale(boundsExtent, new Vector3(-1, 1, 1));
            Vector3 p6 = boundsCenter + _crtRotation * Vector3.Scale(boundsExtent, new Vector3(-1, 1, -1));
            Vector3 p7 = boundsCenter + _crtRotation * Vector3.Scale(boundsExtent, new Vector3(-1, -1, 1));
            Vector3 p8 = boundsCenter + _crtRotation * Vector3.Scale(boundsExtent, new Vector3(-1, -1, -1));

            Handles.DrawLine(p1, p2);
            Handles.DrawLine(p1, p3);
            Handles.DrawLine(p2, p4);
            Handles.DrawLine(p3, p4);

            Handles.DrawLine(p5, p6);
            Handles.DrawLine(p5, p7);
            Handles.DrawLine(p6, p8);
            Handles.DrawLine(p7, p8);

            Handles.DrawLine(p1, p5);
            Handles.DrawLine(p2, p6);
            Handles.DrawLine(p3, p7);
            Handles.DrawLine(p4, p8);

            if (_editBox)
            {
                Vector3 p9 = boundsCenter + _crtRotation * new Vector3(boundsExtent.x, 0, 0);
                Vector3 p10 = boundsCenter + _crtRotation * new Vector3(-boundsExtent.x, 0, 0);
                Vector3 p11 = boundsCenter + _crtRotation * new Vector3(0, boundsExtent.y, 0);
                Vector3 p12 = boundsCenter + _crtRotation * new Vector3(0, -boundsExtent.y, 0);
                Vector3 p13 = boundsCenter + _crtRotation * new Vector3(0, 0, boundsExtent.z);
                Vector3 p14 = boundsCenter + _crtRotation * new Vector3(0, 0, -boundsExtent.z);
                Vector3 p15 = boundsCenter;

                CheckOnePoint(p9, p15, new Vector3(1, 0, 0));
                CheckOnePoint(p10, p15, new Vector3(-1, 0, 0));
                CheckOnePoint(p11, p15, new Vector3(0, 1, 0));
                CheckOnePoint(p12, p15, new Vector3(0, -1, 0));
                CheckOnePoint(p13, p15, new Vector3(0, 0, 1));
                CheckOnePoint(p14, p15, new Vector3(0, 0, -1));
            }
        }

        private void CheckOnePoint(Vector3 holdPoint, Vector3 centerPoint, Vector3 dir)
        {
            EditorGUI.BeginChangeCheck();

#if UNITY_6000_0_OR_NEWER
            Vector3 newHoldPoint = Handles.FreeMoveHandle(holdPoint, HandleUtility.GetHandleSize(holdPoint) * 0.03f,
                Vector3.one * 0.5f, Handles.DotHandleCap);
#else
            Vector3 newHoldPoint = Handles.FreeMoveHandle(holdPoint, Quaternion.identity, HandleUtility.GetHandleSize(holdPoint) * 0.03f,
                Vector3.one * 0.5f, Handles.DotHandleCap);
#endif
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_crtRenderBox, "Changed Box");
                var v = VectorTool.GetFootDrop(newHoldPoint, centerPoint, holdPoint);
                var v2 = (v - holdPoint).magnitude;

                if (Vector3.Angle(v - holdPoint, holdPoint - centerPoint) > 90)
                {
                    v2 = -v2;
                }

                _crtRenderBox.Size += Vector3.Scale(Vector3.Scale(Vector3.one * v2, dir), dir);
                _crtRenderBox.Center += Vector3.Scale(Vector3.one * (v2 * 0.5f), dir);
            }

            EditorGUI.BeginChangeCheck();
        }

        private void CustomHandle(int controlID, Vector3 position, Quaternion rotation, float size, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.Repaint:
                {
                    Vector3 cubeSize = Vector3.one * 0.5f;

                    Graphics.DrawMeshNow(ModelHelper.CreateCube(Vector3.zero, cubeSize), StartCapDraw(position, _crtRotation, size));
                }
                    break;
                case EventType.Layout:
                case EventType.MouseMove:
                    HandleUtility.AddControl(controlID, HandleUtility.DistanceToCube(position, rotation, size));
                    break;
            }
        }

        private Matrix4x4 StartCapDraw(Vector3 position, Quaternion rotation, float size)
        {
            Shader.SetGlobalColor(HandleColor, RealHandleColor);
            Shader.SetGlobalFloat(HandleSize, size);
            Matrix4x4 mat = Handles.matrix * Matrix4x4.TRS(position, rotation, Vector3.one);
            Shader.SetGlobalMatrix(ObjectToWorld, mat);
            HandleUtility.handleMaterial.SetInt(HandleZTest, (int)Handles.zTest);
            HandleUtility.handleMaterial.SetPass(0);
            return mat;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            RenderBox rb = (RenderBox)target;

            _editBox = GUILayout.Toggle(_editBox, "Edit Box");

            if (GUILayout.Button("ResetBox"))
            {
                Undo.RecordObject(_crtRenderBox, "Reset Box");
                rb.CalculateBox();
            }
        }
    }
#endif
}
