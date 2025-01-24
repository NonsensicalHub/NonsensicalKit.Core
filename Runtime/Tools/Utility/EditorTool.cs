using UnityEditor;
using UnityEngine;

namespace NonsensicalKit.Tools
{
    public static class EditorTool
    {
        public static void SetDirty(this GameObject go)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(go);
#endif
        }

        public static void Destroy(this GameObject go)
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                Object.Destroy(go);
            }
            else
            {
                Object.DestroyImmediate(go);
            }
#else
            Object.Destroy(go);
#endif
        }

        public static bool IsPlaying
        {
            get
            {
#if UNITY_EDITOR
                return EditorApplication.isPlaying;
#else
                return true;
#endif
            }
        }
    }
}
