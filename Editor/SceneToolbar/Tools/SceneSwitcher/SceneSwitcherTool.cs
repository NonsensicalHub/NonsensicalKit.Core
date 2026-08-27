using System.Collections.Generic;
using Frame.Editor.SceneToolbar;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Frame.Editor.SceneToolbar.Tools.SceneSwitcher
{
    /// <summary>
    /// 场景切换工具：点击后弹出二级菜单，列出过滤后的场景，点击即可打开。
    /// 菜单末尾提供「设置…」入口。
    /// </summary>
    public sealed class SceneSwitcherTool : ISceneToolbarTool
    {
        public const string ToolId = "frame.scene-toolbar.scene-switcher";

        public string Id => ToolId;
        public string Tooltip => "切换场景";
        public int Order => 0;

        public Texture Icon
        {
            get
            {
                var content = EditorGUIUtility.IconContent("d_SceneAsset Icon");
                if (content != null && content.image != null)
                    return content.image;

                content = EditorGUIUtility.IconContent("SceneAsset Icon");
                return content != null ? content.image : null;
            }
        }

        public void OnClick(Rect buttonScreenRect)
        {
            var menu = new GenericMenu();
            var scenes = FindVisibleScenePaths();

            if (scenes.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("没有可显示的场景（请打开设置调整过滤）"));
            }
            else
            {
                var activePath = EditorSceneManager.GetActiveScene().path;

                foreach (var path in scenes)
                {
                    var scenePath = path;
                    var displayName = BuildDisplayName(scenePath);
                    var isActive = string.Equals(scenePath, activePath, System.StringComparison.OrdinalIgnoreCase);

                    menu.AddItem(
                        new GUIContent(displayName),
                        isActive,
                        () => OpenScene(scenePath));
                }
            }

            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("设置…"), false, SceneSwitcherSettingsWindow.Open);

            if (buttonScreenRect.width > 0f || buttonScreenRect.height > 0f)
                menu.DropDown(buttonScreenRect);
            else
                menu.ShowAsContext();
        }

        static List<string> FindVisibleScenePaths()
        {
            var all = FindAllScenePaths();
            var visible = new List<string>(all.Count);
            foreach (var path in all)
            {
                if (SceneSwitcherSettings.IsSceneVisible(path))
                    visible.Add(path);
            }

            return visible;
        }

        static List<string> FindAllScenePaths()
        {
            var guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            var paths = new List<string>(guids.Length);

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path) || !path.EndsWith(".unity", System.StringComparison.OrdinalIgnoreCase))
                    continue;

                paths.Add(path);
            }

            paths.Sort(System.StringComparer.OrdinalIgnoreCase);
            return paths;
        }

        static string BuildDisplayName(string scenePath)
        {
            var relative = scenePath.StartsWith("Assets/", System.StringComparison.OrdinalIgnoreCase)
                ? scenePath.Substring("Assets/".Length)
                : scenePath;

            return relative.Replace('/', '\\').Replace(".unity", string.Empty);
        }

        static void OpenScene(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath))
                return;

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                Debug.LogWarning($"[SceneToolbar] 场景不存在: {scenePath}");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }
    }
}
