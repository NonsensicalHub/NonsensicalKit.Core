using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.Core.Editor.Tools
{
    /// <summary>
    /// 参考：https://blog.csdn.net/zcaixzy5211314/article/details/79549149
    /// 设置创建text时的默认字体
    /// </summary>
    public class SetDefaultFont : EditorWindow
    {
        private static Font font;
        private static EditorWindow window;

        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorApplication.hierarchyChanged += ChangeDefaultFont;
        }

        [MenuItem("NonsensicalKit/设置默认字体")]
        public static void OpenWindow()
        {
            window = GetWindow(typeof(SetDefaultFont));
            window.minSize = new Vector2(500, 300);
            font = GetFont();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("选择默认字体");
            EditorGUILayout.Space();
            font = (Font)EditorGUILayout.ObjectField(font, typeof(Font), true);
            EditorGUILayout.Space();
            if (GUILayout.Button("确定"))
            {
                SetFont(font);
                window.Close();
            }
        }

        private static void ChangeDefaultFont()
        {
            Font f = GetFont();

            if (f != null && Selection.activeGameObject != null)
            {
                var v = Selection.activeGameObject.GetComponentsInChildren<Text>();
                foreach (var item in v)
                {
                    item.font = f;
                }
            }
        }

        private static Font GetFont()
        {
            string path = PlayerPrefs.GetString("nk_setDefaultFont_defaultFontPath", "");
            return AssetDatabase.LoadAssetAtPath<Font>(path);
        }

        private static void SetFont(Font f)
        {
            string path = AssetDatabase.GetAssetPath(f);
            PlayerPrefs.SetString("nk_setDefaultFont_defaultFontPath", path);
        }
    }
}
