using NonsensicalKit.Core;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NonsensicalKit.Tools.GUITool
{
    [AggregatorEnum]
    enum NonsensicalConsoleEnum
    {
        ExecuteCommand = 12306
    }

    /// <summary>
    /// 调试面板控制器（OnGUI）：负责快捷键显隐、调试命令和日志显示。
    /// </summary>
    public partial class NonsensicalConsole : NonsensicalMono
    {
        private const string WindowTitle = "Nonsensical Console";
        private const float WindowMargin = 40f;
        private const float MaxWindowWidth = 1100f;
        private const float MaxWindowHeight = 700f;
        private const float ResizeHandleSize = 18f;
        private const float ResizeHandleVisualSize = 16f;
        private const float ResizeHandleVisualOffset = 20f;

        [Header("Console Options")]
        [SerializeField] private bool m_restrictLogCount = true;

#if ENABLE_INPUT_SYSTEM
        [Tooltip("按下其中任意一键可切换控制台显隐（新输入系统使用 Key）。")]
        [SerializeField] private Key[] m_togglePanelKeys = { Key.Backquote };
#else
        [Tooltip("按下其中任意一键可切换控制台显隐（旧输入管理器使用 KeyCode）。")]
        [SerializeField] private KeyCode[] m_togglePanelKeyCodes = { KeyCode.BackQuote };
#endif

        [SerializeField] private int m_maxLogs = 100; //最大日志显示条数
        [SerializeField] private int m_maxLogCharsPerLine = 1000; //单条日志字符长度限制
        [SerializeField] private Vector2 m_minWindowSize = new Vector2(640f, 360f);
        [SerializeField] private int m_consoleFontSize = 18;

        private bool _collapse = false;
        private bool _showStackTrace = false;
        private bool _visible;
        private Rect _windowRect = new Rect(20, 20, 960, 640);
        private readonly Rect _titleBarRect = new Rect(0, 0, 10000, 20);
        private readonly CommandPanel _commandPanel = new CommandPanel();
        private readonly LogPanel _logPanel = new LogPanel();
        private readonly ConsoleStyles _styles = new ConsoleStyles();
        private bool _isResizing;
        private Vector2 _resizeStartMouse;
        private Vector2 _resizeStartSize;

        private void Awake()
        {
#if !(UNITY_EDITOR || DEVELOPMENT_BUILD)
            enabled = false;
            return;
#endif
            _visible = false;
            _windowRect.width = Mathf.Min(Screen.width - WindowMargin, MaxWindowWidth);
            _windowRect.height = Mathf.Min(Screen.height - WindowMargin, MaxWindowHeight);
        }

        private void OnEnable()
        {
            _logPanel.Bind();
        }

        private void OnDisable()
        {
            _logPanel.Unbind();
        }

        private void Update()
        {
            _logPanel.FlushPendingLogs(m_restrictLogCount, m_maxLogs);

            // 面板关闭时用 Update 打开；打开后仅用 IMGUI 处理热键，避免与 TextField 抢键、且防止同帧双切换。
            if (_visible == false && ShouldTogglePanelThisFrame())
            {
                TogglePanel();
            }
        }

        private void TogglePanel()
        {
            _visible = !_visible;
        }

        private void OnGUI()
        {
            if (!_visible)
            {
                return;
            }

            _logPanel.FlushPendingLogs(m_restrictLogCount, m_maxLogs);

            _styles.Update(Mathf.Max(10, m_consoleFontSize));
            _windowRect = GUILayout.Window(GetInstanceID(), _windowRect, DrawWindow, WindowTitle, _styles.Window);
            HandleWindowResize();
        }

        private void DrawWindow(int windowId)
        {
            Event evt = Event.current;
            if (TryConsumeToggleHotkeyImGui(evt))
            {
                TogglePanel();
                evt.Use();
                GUIUtility.ExitGUI();
            }

            _commandPanel.Draw(SendCommand, _styles);
            GUILayout.Space(8);
            _logPanel.Draw(
                ref _collapse,
                ref _showStackTrace,
                m_restrictLogCount,
                m_maxLogs,
                m_maxLogCharsPerLine,
                _styles);
            DrawResizeHandle(_styles);
            GUI.DragWindow(_titleBarRect);
        }

        private void HandleWindowResize()
        {
            Rect resizeHandleRect = new Rect(
                _windowRect.xMax - ResizeHandleSize,
                _windowRect.yMax - ResizeHandleSize,
                ResizeHandleSize,
                ResizeHandleSize);

            Event current = Event.current;
            if (current.type == EventType.MouseDown && resizeHandleRect.Contains(current.mousePosition))
            {
                _isResizing = true;
                _resizeStartMouse = current.mousePosition;
                _resizeStartSize = new Vector2(_windowRect.width, _windowRect.height);
                current.Use();
                return;
            }

            if (_isResizing && current.type == EventType.MouseDrag)
            {
                Vector2 delta = current.mousePosition - _resizeStartMouse;
                _windowRect.width = Mathf.Max(m_minWindowSize.x, _resizeStartSize.x + delta.x);
                _windowRect.height = Mathf.Max(m_minWindowSize.y, _resizeStartSize.y + delta.y);
                current.Use();
                return;
            }

            if (_isResizing && (current.type == EventType.MouseUp || current.rawType == EventType.MouseUp))
            {
                _isResizing = false;
                current.Use();
            }
        }

        private void DrawResizeHandle(ConsoleStyles styles)
        {
            Rect handle = new Rect(
                _windowRect.width - ResizeHandleVisualOffset,
                _windowRect.height - ResizeHandleVisualOffset,
                ResizeHandleVisualSize,
                ResizeHandleVisualSize);
            GUI.Label(handle, "///", styles.Label);
        }

        private void SendCommand(string commandID, string[] param)
        {
            if (string.IsNullOrWhiteSpace(commandID))
            {
                Debug.LogWarning("[DebugTool] Command id is empty.");
                return;
            }

            bool result = Execute<string[], bool>(NonsensicalConsoleEnum.ExecuteCommand, commandID, param);

            if (result)
            {
                Debug.Log($"执行命令 {commandID} 成功，参数为 {StringTool.GetSetString(param)}");
            }
            else
            {
                Debug.LogWarning($"执行命令 {commandID} 失败，参数为 {StringTool.GetSetString(param)}");
            }
        }
    }
}
