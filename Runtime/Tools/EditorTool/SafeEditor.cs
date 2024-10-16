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
    }
}
