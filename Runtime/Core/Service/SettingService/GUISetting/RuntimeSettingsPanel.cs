using System.Collections.Generic;
using NonsensicalKit.Core.Service.Setting;
using UnityEngine;

namespace Core.Service.SettingService.GUISetting
{
    /// <summary>
    /// 运行时设置面板，使用 Unity 的 IMGUI 绘制可拖动的窗口。
    /// 依赖于 RuntimeSettingsManager 来获取和设置数据。
    /// </summary>
    public class RuntimeSettingsPanel : MonoBehaviour
    {
        [Header("UI Settings")]
        // m_toggleKey: 私有序列化字段，用于 UI 切换
        [SerializeField]
        private KeyCode m_toggleKey = KeyCode.F1;

        [Header("GUI Skin")] [SerializeField] private GUISkin m_customSkin;

        [SerializeField, Tooltip("窗口高度使用百分比")] private bool m_usePercentage;

        [SerializeField] private float m_windowHeightPercentage = 0.8f;

        // m_windowRect: 私有序列化字段，存储窗口位置和大小
        [SerializeField] private Rect m_windowRect = new Rect(50, 50, 400, 600);

        private int _windowID = 123456;
        private Vector2 _scrollPosition = Vector2.zero;

        [Header("References")] public NonsensicalKit.Core.Service.Setting.SettingService settingService;

        private bool m_showPanel = false;
        private bool _initData = false;

        // 使用属性来获取设置数据，确保每次都取到最新数据
        private List<GUISettingItem> CurrentSettingsDataWrapper => settingService?.GetSettingsDataWrapper();

        private void Start()
        {
            if (settingService == null)
            {
                Debug.LogError("RuntimeSettingsManager not assigned or found!", this);
            }
            if (_windowID == 0) _windowID = GetInstanceID();
        }

        public void SetSettingService(NonsensicalKit.Core.Service.Setting.SettingService service) => settingService = service;
 
        public void Init(GUIPanelConfig config)
        {
            m_toggleKey = config.m_ToggleKey;
            m_customSkin = config.m_CustomSkin;
            m_usePercentage = config.m_UsePercentage;
            m_windowHeightPercentage = config.m_WindowHeightPercentage;
            m_windowRect = config.m_WindowRect;
        }

        private void Update()
        {
            if (settingService == null) return;
            if (Input.GetKeyDown(m_toggleKey))
            {
                m_showPanel = !m_showPanel;
                if (m_showPanel)
                {
                    settingService.RefreshGUIData();
                    _initData = true;
                }
            }
        }

        private void OnGUI()
        {
            if (!m_showPanel || CurrentSettingsDataWrapper == null || settingService == null) return;
            // 如果 m_customSkin 不为 null，则应用它。否则，GUI.skin 保持为默认的 Unity skin。
            if (m_customSkin)
            {
                GUI.skin = m_customSkin;
            }

            m_windowRect = GUI.Window(_windowID, m_windowRect, DrawWindow, "Game Settings");
            m_windowRect.x = Mathf.Clamp(m_windowRect.x, 0, Screen.width - m_windowRect.width);
            m_windowRect.y = Mathf.Clamp(m_windowRect.y, 0, Screen.height - m_windowRect.height);

            if (m_usePercentage)
            {
                m_windowRect.height = Screen.height * m_windowHeightPercentage;
            }
        }

        // --- GUI 绘制方法 ---

        private void DrawWindow(int id)
        {
            GUILayout.Space(10);

            // 使用 m_scrollPosition
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));

            foreach (var setting in CurrentSettingsDataWrapper)
            {
                if (_initData)
                {
                    setting.SetInitialValue(setting.value); //初始化数据,清空未被应用的数据
                    _initData = false;
                }

                if (!string.IsNullOrEmpty(setting.description))
                {
                    GUIStyle descStyle = GUI.skin.FindStyle("Description") ?? GUI.skin.label;
                    GUILayout.Label(setting.description, descStyle);

                    // 为了通用性，这里直接使用默认 Label 样式
                    // GUILayout.Label(setting.description);
                }

                GUILayout.BeginHorizontal();
                switch (setting.InternalType)
                {
                    case SettingItemTypeInternal.Bool:
                        // Toggle 将使用 GUISkin 中的 Toggle 样式
                        var newBoolValue = GUILayout.Toggle(setting.boolValue, setting.label);
                        if (newBoolValue != setting.boolValue)
                        {
                            settingService.SetValue(setting.key, newBoolValue);
                        }

                        break;
                    case SettingItemTypeInternal.String:
                        // Label 和 TextField 将使用 GUISkin 中的对应样式
                        GUILayout.Label(setting.label);
                        string newStringValue = GUILayout.TextField(setting.stringValue);
                        if (newStringValue != setting.stringValue)
                        {
                            settingService.SetValue(setting.key, newStringValue);
                        }

                        break;
                    case SettingItemTypeInternal.Float:
                        // Label 和 HorizontalSlider 将使用 GUISkin 中的对应样式
                        GUILayout.Label($"{setting.label} ({setting.floatValue:F2})");
                        float newFloatValue = GUILayout.HorizontalSlider(setting.floatValue, setting.range.I1, setting.range.I2, GUILayout.ExpandWidth(true));
                        if (Mathf.Abs(newFloatValue - setting.floatValue) > 0.001f)
                        {
                            settingService.SetValue(setting.key, newFloatValue);
                        }

                        break;
                    case SettingItemTypeInternal.Enum:
                        // var tempSetting = setting.Clone();//深拷贝对象
                        GUILayout.Label(setting.label);
                        if (setting.options is { Count: > 0 })
                        {
                            int newIndex = GUILayout.SelectionGrid(setting.SelectedOptionIndex, setting.options.ToArray(), 1, GUILayout.ExpandWidth(true));
                            if (newIndex != setting.SelectedOptionIndex)
                            {
                                //正常来说赋值逻辑应在SetValue处处理
                                //但此处需要将改动提前应用到UI面板上,所以需要提前赋值,意在及时更新视觉效果
                                setting.SelectedOptionIndex = newIndex;
                                setting.stringValue = setting.options[newIndex];
                                settingService.SetValue(setting.key, setting.stringValue);
                            }
                        }
                        else
                        {
                            GUILayout.Label("(No options defined)");
                        }

                        break;
                    case SettingItemTypeInternal.Button:
                        // Button 将使用 GUISkin 中的 Button 样式
                        if (GUILayout.Button(setting.label))
                        {
                            settingService.HandleButtonAction(setting.buttonAction);
                        }

                        break;
                }

                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }

            GUILayout.EndScrollView();
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            // 按钮全部使用 GUISkin 中的 Button 样式
            if (GUILayout.Button("应用"))
            {
                settingService.HandleButtonAction("applySettings");
            }

            if (GUILayout.Button("确认"))
            {
                settingService.HandleButtonAction("applySettings");
                m_showPanel = false;
            }

            if (GUILayout.Button("取消"))
            {
                settingService.HandleButtonAction("cancel");
                m_showPanel = false;
            }


            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // 拖动区域，将使用 GUISkin 中的 Window 样式进行渲染
            GUI.DragWindow(new Rect(0, 0, m_windowRect.width, 20));
        }
    }
}