using UnityEngine;

namespace NonsensicalKit.Setting
{
    /// <summary>
    /// 按 Unity 版本选择面板渲染器：
    /// 2021.2+ → UI Toolkit；更低版本 → IMGUI。
    /// </summary>
    public static class SettingPanelFactory
    {
        public static bool PreferUiToolkit
        {
            get
            {
#if UNITY_2021_2_OR_NEWER
                return true;
#else
                return false;
#endif
            }
        }

        public static string ActiveRendererName
        {
            get { return PreferUiToolkit ? "UIToolkit" : "IMGUI"; }
        }

        /// <summary>
        /// 可通过启动参数强制指定：-nk-setting-ui=imgui | uitk
        /// </summary>
        public static ISettingPanelView Create(GameObject host)
        {
            string force = GetForceRenderer();
            if (force == "imgui")
            {
                Debug.Log("[Setting] 强制使用 IMGUI 渲染器");
                return host.AddComponent<ImguiSettingPanel>();
            }

#if UNITY_2021_2_OR_NEWER
            if (force == "uitk" || PreferUiToolkit)
            {
                try
                {
                    // 预检主题，没有主题绝不用 UITK（否则空白页无法验收）
                    var theme = Resources.Load<UnityEngine.UIElements.ThemeStyleSheet>(
                        "UIToolkit/UnityDefaultRuntimeTheme");
                    var panel = Resources.Load<UnityEngine.UIElements.PanelSettings>(
                        "UIToolkit/SettingPanelSettings");
                    bool themeReady = (theme != null) ||
                                      (panel != null && panel.themeStyleSheet != null);
#if UNITY_EDITOR
                    if (!themeReady)
                    {
                        themeReady =
                            UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.ThemeStyleSheet>(
                                "Assets/UI Toolkit/UnityThemes/UnityDefaultRuntimeTheme.tss") != null;
                    }
#endif
                    if (!themeReady)
                    {
                        Debug.LogWarning("[Setting] UITK 主题资源缺失，回退 IMGUI");
                    }
                    else
                    {
                        UitkSettingPanel uitk = host.AddComponent<UitkSettingPanel>();
                        if (uitk != null)
                        {
                            return uitk;
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("[Setting] UI Toolkit 创建失败，回退 IMGUI: " + e.Message);
                }
            }
#endif
            return host.AddComponent<ImguiSettingPanel>();
        }

        private static string GetForceRenderer()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] != null && args[i].StartsWith("-nk-setting-ui="))
                {
                    return args[i].Substring("-nk-setting-ui=".Length).Trim().ToLowerInvariant();
                }
            }

            return PlayerPrefs.GetString("NK_SETTING_UI", string.Empty).Trim().ToLowerInvariant();
        }
    }
}
