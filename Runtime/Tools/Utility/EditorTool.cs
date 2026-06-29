using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NonsensicalKit.Tools
{
    /// <summary>
    /// 编辑器/运行时共用的对象工具。
    /// 在编辑器下会根据播放状态选择 Destroy 或 DestroyImmediate。
    /// </summary>
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
