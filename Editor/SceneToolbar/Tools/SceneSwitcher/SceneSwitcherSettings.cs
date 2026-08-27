using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Frame.Editor.SceneToolbar.Tools.SceneSwitcher
{
    /// <summary>
    /// 场景切换菜单的过滤设置（项目级，存于 ProjectSettings）。
    /// </summary>
    [Serializable]
    public sealed class SceneSwitcherSettingsData
    {
        /// <summary>为 true 时忽略目录过滤，显示 Assets 下全部场景（仍会应用 hiddenScenes）。</summary>
        public bool showAllScenes;

        /// <summary>仅显示这些目录及其子目录下的场景（Unity 资源路径，如 Assets/Scenes）。</summary>
        public List<string> includedFolders = new List<string> { "Assets/Scenes" };

        /// <summary>即使匹配目录也强制隐藏的场景路径。</summary>
        public List<string> hiddenScenes = new List<string>();
    }

    public static class SceneSwitcherSettings
    {
        const string FilePath = "ProjectSettings/SceneSwitcherSettings.json";

        static SceneSwitcherSettingsData s_Cache;

        public static SceneSwitcherSettingsData Data
        {
            get
            {
                if (s_Cache == null)
                    s_Cache = Load();
                return s_Cache;
            }
        }

        public static void Reload()
        {
            s_Cache = Load();
        }

        public static void Save()
        {
            if (s_Cache == null)
                s_Cache = CreateDefault();

            Normalize(s_Cache);

            var dir = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonUtility.ToJson(s_Cache, true);
            File.WriteAllText(FilePath, json);
        }

        public static bool IsSceneVisible(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath))
                return false;

            var data = Data;
            if (IsHidden(scenePath, data))
                return false;

            if (data.showAllScenes)
                return true;

            if (data.includedFolders == null || data.includedFolders.Count == 0)
                return false;

            var normalized = NormalizePath(scenePath);
            foreach (var folder in data.includedFolders)
            {
                if (string.IsNullOrEmpty(folder))
                    continue;

                var prefix = NormalizePath(folder).TrimEnd('/');
                if (normalized.Equals(prefix, StringComparison.OrdinalIgnoreCase)
                    || normalized.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public static void SetHidden(string scenePath, bool hidden)
        {
            if (string.IsNullOrEmpty(scenePath))
                return;

            var data = Data;
            if (data.hiddenScenes == null)
                data.hiddenScenes = new List<string>();

            var normalized = NormalizePath(scenePath);
            var index = data.hiddenScenes.FindIndex(p =>
                string.Equals(NormalizePath(p), normalized, StringComparison.OrdinalIgnoreCase));

            if (hidden && index < 0)
                data.hiddenScenes.Add(scenePath);
            else if (!hidden && index >= 0)
                data.hiddenScenes.RemoveAt(index);
        }

        public static bool IsHidden(string scenePath)
        {
            return IsHidden(scenePath, Data);
        }

        static bool IsHidden(string scenePath, SceneSwitcherSettingsData data)
        {
            if (data.hiddenScenes == null || data.hiddenScenes.Count == 0)
                return false;

            var normalized = NormalizePath(scenePath);
            foreach (var hidden in data.hiddenScenes)
            {
                if (string.Equals(NormalizePath(hidden), normalized, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        static SceneSwitcherSettingsData Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    var data = JsonUtility.FromJson<SceneSwitcherSettingsData>(json);
                    if (data != null)
                    {
                        Normalize(data);
                        return data;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SceneToolbar] 读取场景过滤设置失败: {e.Message}");
            }

            return CreateDefault();
        }

        static SceneSwitcherSettingsData CreateDefault()
        {
            // 默认只显示 Assets/Scenes，避免 Plugins/ThirdParty 演示场景刷屏
            return new SceneSwitcherSettingsData
            {
                showAllScenes = false,
                includedFolders = new List<string> { "Assets/Scenes" },
                hiddenScenes = new List<string>()
            };
        }

        static void Normalize(SceneSwitcherSettingsData data)
        {
            if (data.includedFolders == null)
                data.includedFolders = new List<string>();
            if (data.hiddenScenes == null)
                data.hiddenScenes = new List<string>();

            for (var i = 0; i < data.includedFolders.Count; i++)
                data.includedFolders[i] = NormalizePath(data.includedFolders[i]);

            for (var i = 0; i < data.hiddenScenes.Count; i++)
                data.hiddenScenes[i] = NormalizePath(data.hiddenScenes[i]);

            // 去重
            data.includedFolders = DistinctIgnoreCase(data.includedFolders);
            data.hiddenScenes = DistinctIgnoreCase(data.hiddenScenes);
        }

        static List<string> DistinctIgnoreCase(List<string> source)
        {
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in source)
            {
                if (string.IsNullOrEmpty(item) || !seen.Add(item))
                    continue;
                result.Add(item);
            }

            return result;
        }

        public static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            return path.Replace('\\', '/').TrimEnd('/');
        }
    }
}
