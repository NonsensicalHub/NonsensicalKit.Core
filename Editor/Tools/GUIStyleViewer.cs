using UnityEditor;
using UnityEngine;

namespace NonsensicalKit.Core.Editor.Tools
{
    /// <summary>
    /// 查看内置GUI，方便设置编辑器工具格式
    /// https://blog.csdn.net/u011428080/article/details/106676213
    /// </summary>
    public class GUIStyleViewer : EditorWindow
    {
        private Vector2 _scrollPosition = new Vector2(0, 0);
        private string _search = string.Empty;
        private GUIStyle _textStyle;

        [MenuItem("NonsensicalKit/GUIStyleViewer", false, 10)]
        private static void OpenStyleViewer()
        {
            GetWindow<GUIStyleViewer>(false, "内置GUIStyle");
        }

        private void OnGUI()
        {
            if (_textStyle == null)
            {
                _textStyle = new GUIStyle("HeaderLabel");
                _textStyle.fontSize = 25;
            }

            GUILayout.BeginHorizontal("HelpBox");
            GUILayout.Label("结果如下：", _textStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Search:");
            _search = EditorGUILayout.TextField(_search);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("PopupCurveSwatchBackground");
            GUILayout.Label("样式展示", _textStyle, GUILayout.Width(300));
            GUILayout.Label("名字", _textStyle, GUILayout.Width(300));
            GUILayout.EndHorizontal();

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            foreach (var style in GUI.skin.customStyles)
            {
                if (style.name.ToLower().Contains(_search.ToLower()))
                {
                    GUILayout.Space(15);
                    GUILayout.BeginHorizontal("PopupCurveSwatchBackground");
                    if (GUILayout.Button(style.name, style, GUILayout.Width(300)))
                    {
                        EditorGUIUtility.systemCopyBuffer = style.name;
                        Debug.LogError(style.name);
                    }

                    EditorGUILayout.SelectableLabel(style.name, GUILayout.Width(300));
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndScrollView();
        }
    }
}
