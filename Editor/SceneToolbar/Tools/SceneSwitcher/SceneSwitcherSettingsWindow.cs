using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Frame.Editor.SceneToolbar.Tools.SceneSwitcher
{
    /// <summary>
    /// 场景切换列表过滤设置窗口。
    /// </summary>
    public sealed class SceneSwitcherSettingsWindow : EditorWindow
    {
        Vector2 _folderScroll;
        Vector2 _sceneScroll;
        ReorderableList _folderList;
        List<SceneRow> _sceneRows;
        string _search = string.Empty;

        class SceneRow
        {
            public string Path;
            public string Display;
            public bool InFilterScope;
        }

        public static void Open()
        {
            var window = GetWindow<SceneSwitcherSettingsWindow>(true, "场景切换设置", true);
            window.minSize = new Vector2(420, 480);
            window.Show();
        }

        void OnEnable()
        {
            SceneSwitcherSettings.Reload();
            RebuildFolderList();
            RebuildSceneRows();
        }

        void RebuildFolderList()
        {
            var data = SceneSwitcherSettings.Data;
            _folderList = new ReorderableList(data.includedFolders, typeof(string), true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "包含目录（仅显示这些目录下的场景）"),
                drawElementCallback = (rect, index, active, focused) =>
                {
                    if (index < 0 || index >= data.includedFolders.Count)
                        return;

                    rect.y += 2;
                    rect.height = EditorGUIUtility.singleLineHeight;
                    data.includedFolders[index] = EditorGUI.TextField(rect, data.includedFolders[index]);
                },
                onAddCallback = _ =>
                {
                    var folder = PickAssetsFolder();
                    if (string.IsNullOrEmpty(folder))
                        return;

                    if (!data.includedFolders.Exists(f =>
                            string.Equals(
                                SceneSwitcherSettings.NormalizePath(f),
                                folder,
                                System.StringComparison.OrdinalIgnoreCase)))
                    {
                        data.includedFolders.Add(folder);
                    }
                },
                onChangedCallback = _ =>
                {
                    SceneSwitcherSettings.Save();
                    RebuildSceneRows();
                }
            };
        }

        void RebuildSceneRows()
        {
            var all = FindAllScenePaths();
            _sceneRows = new List<SceneRow>(all.Count);

            foreach (var path in all)
            {
                // 用临时逻辑判断「是否在目录范围内」，忽略 hidden，便于在设置里勾选
                var inScope = IsInIncludeScope(path);
                _sceneRows.Add(new SceneRow
                {
                    Path = path,
                    Display = path.StartsWith("Assets/", System.StringComparison.OrdinalIgnoreCase)
                        ? path.Substring("Assets/".Length)
                        : path,
                    InFilterScope = inScope
                });
            }
        }

        static bool IsInIncludeScope(string scenePath)
        {
            var data = SceneSwitcherSettings.Data;
            if (data.showAllScenes)
                return true;

            if (data.includedFolders == null || data.includedFolders.Count == 0)
                return false;

            var normalized = SceneSwitcherSettings.NormalizePath(scenePath);
            foreach (var folder in data.includedFolders)
            {
                if (string.IsNullOrEmpty(folder))
                    continue;

                var prefix = SceneSwitcherSettings.NormalizePath(folder);
                if (normalized.Equals(prefix, System.StringComparison.OrdinalIgnoreCase)
                    || normalized.StartsWith(prefix + "/", System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        static List<string> FindAllScenePaths()
        {
            var guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            var paths = new List<string>(guids.Length);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path) && path.EndsWith(".unity", System.StringComparison.OrdinalIgnoreCase))
                    paths.Add(path);
            }

            paths.Sort(System.StringComparer.OrdinalIgnoreCase);
            return paths;
        }

        static string PickAssetsFolder()
        {
            var abs = EditorUtility.OpenFolderPanel("选择要包含的场景目录", Application.dataPath, string.Empty);
            if (string.IsNullOrEmpty(abs))
                return null;

            abs = abs.Replace('\\', '/');
            var dataPath = Application.dataPath.Replace('\\', '/');
            if (!abs.StartsWith(dataPath, System.StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayDialog("无效目录", "请选择项目 Assets 目录下的文件夹。", "确定");
                return null;
            }

            var relative = "Assets" + abs.Substring(dataPath.Length);
            return SceneSwitcherSettings.NormalizePath(relative);
        }

        void OnGUI()
        {
            var data = SceneSwitcherSettings.Data;

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("场景列表过滤", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "默认只显示 Assets/Scenes，避免插件 Demo 场景刷屏。可在下方调整目录，或单独勾选隐藏某些场景。",
                MessageType.Info);

            EditorGUI.BeginChangeCheck();
            data.showAllScenes = EditorGUILayout.ToggleLeft("显示全部场景（忽略目录过滤）", data.showAllScenes);
            if (EditorGUI.EndChangeCheck())
            {
                SceneSwitcherSettings.Save();
                RebuildSceneRows();
            }

            using (new EditorGUI.DisabledScope(data.showAllScenes))
            {
                _folderList?.DoLayoutList();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("添加 Assets/Scenes", GUILayout.Width(140)))
                {
                    const string scenesFolder = "Assets/Scenes";
                    if (!data.includedFolders.Exists(f =>
                            string.Equals(
                                SceneSwitcherSettings.NormalizePath(f),
                                scenesFolder,
                                System.StringComparison.OrdinalIgnoreCase)))
                    {
                        data.includedFolders.Add(scenesFolder);
                        SceneSwitcherSettings.Save();
                        RebuildSceneRows();
                    }
                }

                if (GUILayout.Button("浏览添加目录…", GUILayout.Width(120)))
                {
                    var folder = PickAssetsFolder();
                    if (!string.IsNullOrEmpty(folder)
                        && !data.includedFolders.Exists(f =>
                            string.Equals(
                                SceneSwitcherSettings.NormalizePath(f),
                                folder,
                                System.StringComparison.OrdinalIgnoreCase)))
                    {
                        data.includedFolders.Add(folder);
                        SceneSwitcherSettings.Save();
                        RebuildSceneRows();
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("场景可见性（勾选 = 在菜单中显示）", EditorStyles.boldLabel);

            _search = EditorGUILayout.TextField("搜索", _search);

            _sceneScroll = EditorGUILayout.BeginScrollView(_sceneScroll);
            if (_sceneRows != null)
            {
                IEnumerable<SceneRow> rows = _sceneRows;
                if (!string.IsNullOrWhiteSpace(_search))
                {
                    var key = _search.Trim();
                    rows = rows.Where(r =>
                        r.Display.IndexOf(key, System.StringComparison.OrdinalIgnoreCase) >= 0
                        || r.Path.IndexOf(key, System.StringComparison.OrdinalIgnoreCase) >= 0);
                }

                // 优先显示当前过滤范围内的场景
                rows = rows.OrderByDescending(r => r.InFilterScope).ThenBy(r => r.Display);

                foreach (var row in rows)
                {
                    var inScope = row.InFilterScope || data.showAllScenes;
                    var shown = inScope && !SceneSwitcherSettings.IsHidden(row.Path);

                    using (new EditorGUI.DisabledScope(!inScope && !data.showAllScenes))
                    {
                        EditorGUI.BeginChangeCheck();
                        var label = inScope ? row.Display : row.Display + "  （不在包含目录内）";
                        var next = EditorGUILayout.ToggleLeft(label, shown);
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (inScope || data.showAllScenes)
                            {
                                SceneSwitcherSettings.SetHidden(row.Path, !next);
                                SceneSwitcherSettings.Save();
                            }
                        }
                    }
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("清除全部隐藏标记"))
            {
                data.hiddenScenes.Clear();
                SceneSwitcherSettings.Save();
            }

            if (GUILayout.Button("保存并关闭"))
            {
                SceneSwitcherSettings.Save();
                Close();
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
