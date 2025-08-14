using System.Collections.Generic;
using NonsensicalKit.Tools;
using NonsensicalKit.Tools.ObjectPool;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Callbacks;
using UnityEditor;
#endif

namespace NonsensicalKit.Core
{
    public static class EditorDrawTool
    {
        public static void DrawArrowHandle(in Vector3 pos, in Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
#if UNITY_EDITOR
            DrawArrow(TargetType.Handle, pos, direction, Handles.color, arrowHeadLength, arrowHeadAngle);
#endif
        }

        public static void DrawArrowHandle(in Vector3 pos, in Vector3 direction, in Color color, float arrowHeadLength = 0.25f,
            float arrowHeadAngle = 20.0f)
        {
#if UNITY_EDITOR
            DrawArrow(TargetType.Handle, pos, direction, color, arrowHeadLength, arrowHeadAngle);
#endif
        }
        public static void DrawBezierArrowHandle(in Vector3 p0, in Vector3 p1, in Vector3 p2, in Vector3 p3, int smoothness, in Color color,
            float arrowHeadLength = 0.25f,
            float arrowHeadAngle = 20.0f)
        {
#if UNITY_EDITOR
            DrawBezierArrow(TargetType.Handle, p0, p1, p2, p3, smoothness, color, arrowHeadLength, arrowHeadAngle);
#endif
        }

        public static void DrawArrowGizmo(in Vector3 pos, in Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
#if UNITY_EDITOR
            DrawArrow(TargetType.Gizmo, pos, direction, Gizmos.color, arrowHeadLength, arrowHeadAngle);
#endif
        }

        public static void DrawArrowGizmo(in Vector3 pos, in Vector3 direction, in Color color, float arrowHeadLength = 0.25f,
            float arrowHeadAngle = 20.0f)
        {
#if UNITY_EDITOR
            DrawArrow(TargetType.Gizmo, pos, direction, color, arrowHeadLength, arrowHeadAngle);
#endif
        }

        public static void DrawBezierArrowGizmo(in Vector3 p0, in Vector3 p1, in Vector3 p2, in Vector3 p3, int smoothness, in Color color,
            float arrowHeadLength = 0.25f,
            float arrowHeadAngle = 20.0f)
        {
#if UNITY_EDITOR
            DrawBezierArrow(TargetType.Gizmo, p0, p1, p2, p3, smoothness, color, arrowHeadLength, arrowHeadAngle);
#endif
        }

        public static void DrawArrowDebug(in Vector3 pos, in Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
#if UNITY_EDITOR
            DrawArrow(TargetType.Debug, pos, direction, Gizmos.color, arrowHeadLength, arrowHeadAngle);
#endif
        }

        public static void DrawArrowDebug(in Vector3 pos, in Vector3 direction, in Color color, float arrowHeadLength = 0.25f,
            float arrowHeadAngle = 20.0f)
        {
#if UNITY_EDITOR
            DrawArrow(TargetType.Debug, pos, direction, color, arrowHeadLength, arrowHeadAngle);
#endif
        }

#if UNITY_EDITOR
        private static readonly ObjectPool<Mesh> Triangles = new(16, null, InitTriangle);
        private static readonly List<Mesh> Used = new();

        private static bool _flag = false;

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            Triangles.Clear();
            Used.Clear();
        }

        private static void InitTriangle(Mesh mesh)
        {
            mesh.SetVertices(new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero });
            mesh.SetTriangles(new int[] { 0, 1, 2 }, 0);
        }

        private static void Listener()
        {
            if (!_flag)
            {
                _flag = true;
                EditorApplication.update += UnListener;
            }
        }

        private static void UnListener()
        {
            EditorApplication.update -= UnListener;
            _flag = false;

            foreach (Mesh mesh in Used)
            {
                Triangles.Store(mesh);
            }

            Used.Clear();
        }

        private static void DrawBezierArrow(TargetType targetType, in Vector3 p0, in Vector3 p1, in Vector3 p2, in Vector3 p3, int smoothness,
            in Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Camera c = Camera.current;
            if (c == null) return;

            var points = BezierTool.GetCubicBezierList(p0, p1, p2, p3, smoothness);

            int point1 = (int)(smoothness * 0.7);
            int point2 = point1 + 1;
            var pos = points[point1];
            var direction = points[point2] - points[point1];

            var up = Quaternion.LookRotation(direction, c.transform.forward) * Quaternion.Euler(0, arrowHeadAngle, 0) * Vector3.back *
                     arrowHeadLength;
            var down = Quaternion.LookRotation(direction, c.transform.forward) * Quaternion.Euler(0, -arrowHeadAngle, 0) * Vector3.back *
                       arrowHeadLength;
            var end = pos + direction * 0.8f;

            Color colorPrew;

            switch (targetType)
            {
                case TargetType.Gizmo:
                    Mesh triangle = Triangles.New();
                    while (triangle == null) //每次构建后，已经创建好的网格会被unity销毁，此时需要清理这些已销毁网格
                    {
                        Object.DestroyImmediate(triangle);
                        triangle = Triangles.New();
                    }

                    triangle.SetVertices(new Vector3[] { end, end + up, end + down });
                    triangle.RecalculateNormals();
                    Used.Add(triangle);
                    Listener();

                    colorPrew = Gizmos.color;
                    Gizmos.color = color;
                    Gizmos.DrawMesh(triangle);
                    for (int i = 1; i < points.Length; i++)
                    {
                        Gizmos.DrawRay(points[i - 1], points[i] - points[i - 1]);
                    }

                    Gizmos.color = colorPrew;
                    break;

                case TargetType.Debug:
                    //TODO
                    break;
                case TargetType.Handle:
                    //TODO
                    break;
            }
        }

        private static void DrawArrow(TargetType targetType, in Vector3 pos, in Vector3 direction, in Color color, float arrowHeadLength = 0.25f,
            float arrowHeadAngle = 20.0f)
        {
            if (direction==Vector3.zero)
            {
                return;
            }
            Camera c = Camera.current;
            if (c == null) return;

            var up = Quaternion.LookRotation(direction, c.transform.forward) * Quaternion.Euler(0, arrowHeadAngle, 0) * Vector3.back *
                     arrowHeadLength;
            var down = Quaternion.LookRotation(direction, c.transform.forward) * Quaternion.Euler(0, -arrowHeadAngle, 0) * Vector3.back *
                       arrowHeadLength;
            var end = pos + direction * 0.8f;
            Color colorPrew;


            switch (targetType)
            {
                case TargetType.Gizmo:

                    Mesh triangle = Triangles.New();
                    while (triangle == null) //每次构建后，已经创建好的网格会被unity销毁，此时需要清理这些已销毁网格
                    {
                        Object.DestroyImmediate(triangle);
                        triangle = Triangles.New();
                    }

                    triangle.SetVertices(new Vector3[] { end, end + up, end + down });
                    triangle.RecalculateNormals();
                    Used.Add(triangle);
                    Listener();

                    colorPrew = Gizmos.color;
                    Gizmos.color = color;
                    Gizmos.DrawRay(pos, direction);
                    Gizmos.DrawMesh(triangle);
                    Gizmos.color = colorPrew;
                    break;

                case TargetType.Debug:
                    Debug.DrawRay(pos, direction, color);
                    Debug.DrawRay(end, up, color);
                    Debug.DrawRay(end, down, color);
                    break;

                case TargetType.Handle:
                    colorPrew = Handles.color;
                    Handles.color = color;
                    Handles.DrawLine(pos, end);
                    Handles.DrawLine(end, end + up);
                    Handles.DrawLine(end, end + down);
                    Handles.color = colorPrew;
                    break;
            }
        }

        private enum TargetType
        {
            Gizmo, Debug, Handle
        }
#endif
    }
}
