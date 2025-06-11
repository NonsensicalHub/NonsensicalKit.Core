using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class DrawArrow
{
    public static void ForHandle(in Vector3 pos, in Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
#if UNITY_EDITOR
        Arrow(TargetType.Handle, pos, direction, Handles.color, arrowHeadLength, arrowHeadAngle);
#endif
    }

    public static void ForHandle(in Vector3 pos, in Vector3 direction, in Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
#if UNITY_EDITOR
        Arrow(TargetType.Handle, pos, direction, color, arrowHeadLength, arrowHeadAngle);
#endif
    }

    public static void ForGizmo(in Vector3 pos, in Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Arrow(TargetType.Gizmo, pos, direction, Gizmos.color, arrowHeadLength, arrowHeadAngle);
    }

    public static void ForGizmo(in Vector3 pos, in Vector3 direction, in Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Arrow(TargetType.Gizmo, pos, direction, color, arrowHeadLength, arrowHeadAngle);
    }

    public static void ForDebug(in Vector3 pos, in Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Arrow(TargetType.Debug, pos, direction, Gizmos.color, arrowHeadLength, arrowHeadAngle);
    }

    public static void ForDebug(in Vector3 pos, in Vector3 direction, in Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Arrow(TargetType.Debug, pos, direction, color, arrowHeadLength, arrowHeadAngle);
    }

    private static void Arrow(TargetType targetType, in Vector3 pos, in Vector3 direction, in Color color, float arrowHeadLength = 0.25f,
        float arrowHeadAngle = 20.0f)
    {
        Camera c = Camera.current;
        if (c == null) return;

        var up = Quaternion.LookRotation(direction, c.transform.forward) * Quaternion.Euler(0, arrowHeadAngle, 0) * Vector3.back * arrowHeadLength;
        var down = Quaternion.LookRotation(direction, c.transform.forward) * Quaternion.Euler(0, -arrowHeadAngle, 0) * Vector3.back * arrowHeadLength;
        var end = pos + direction*0.8f;
        Color colorPrew;


        switch (targetType)
        {
            case TargetType.Gizmo:

                Mesh triangle = new Mesh
                {
                    vertices = new Vector3[3]
                    {
                        end,
                        end + up,
                        end + down,
                    },
                    triangles = new int[]
                    {
                        0, 1, 2
                    }
                };

                triangle.RecalculateNormals();
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

#if UNITY_EDITOR
            case TargetType.Handle:
                colorPrew = Handles.color;
                Handles.color = color;
                Handles.DrawLine(pos, end);
                Handles.DrawLine(end, end + up);
                Handles.DrawLine(end, end + down);
                Handles.color = colorPrew;
                break;
#endif
        }
    }

    private enum TargetType
    {
        Gizmo, Debug, Handle
    }
}
