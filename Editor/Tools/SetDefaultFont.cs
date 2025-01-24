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
        private static Font _font;
        private static EditorWindow _window;

        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorApplication.hierarchyChanged += ChangeDefaultFont;
        }

        [MenuItem("NonsensicalKit/设置默认字体")]
        public static void OpenWindow()
        {
            _window = GetWindow(typeof(SetDefaultFont));
            _window.minSize = new Vector2(500, 300);
            _font = GetFont();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("选择默认字体");
            EditorGUILayout.Space();
            _font = (Font)EditorGUILayout.ObjectField(_font, typeof(Font), true);
            EditorGUILayout.Space();
            if (GUILayout.Button("确定"))
            {
                SetFont(_font);
                _window.Close();
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
