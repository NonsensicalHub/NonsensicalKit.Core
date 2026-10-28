using UnityEditor;
using UnityEngine;

namespace NonsensicalKit.Setting.Editor
{
    /// <summary>
    /// 退出 Play 模式时还原系统设置快照，避免污染 ProjectSettings / URP 资源。
    /// </summary>
    [InitializeOnLoad]
    public static class SettingPlayModeRestore
    {
        static SettingPlayModeRestore()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode)
            {
                return;
            }

            SettingManager manager = SettingManager.Instance;
            if (manager == null || !manager.IsReady)
            {
                return;
            }

            try
            {
                manager.RestoreSystemSnapshotForEditor();
                Debug.Log("[Setting] 已还原系统设置快照（退出 Play 模式）");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Setting] 还原系统快照失败: " + e.Message);
            }
        }
    }
}
