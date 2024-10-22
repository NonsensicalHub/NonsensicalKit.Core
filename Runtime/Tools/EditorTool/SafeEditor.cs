using UnityEngine;

namespace NonsensicalKit.Tools.EditorTool
{
    public static class SafeEditor
    {
        public static void SetDirty(this GameObject go)
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(go);
#endif
        }
        public static void Destroy(this GameObject go)
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
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
                return UnityEditor.EditorApplication.isPlaying;
#else
                return true;
#endif
            }
        }
    }
}
