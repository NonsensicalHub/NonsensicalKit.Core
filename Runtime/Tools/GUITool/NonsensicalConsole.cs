using System;
using System.Collections.Generic;
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
    /// 调试面板控制器（OnGUI）：负责 F1 显隐、调试命令和日志显示。
    /// </summary>
    public class NonsensicalConsole : NonsensicalMono
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
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
            {
                TogglePanel();
            }
#else
            if (Input.GetKeyDown(KeyCode.F1))
            {
                TogglePanel();
            }
#endif
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

            _styles.Update(Mathf.Max(10, m_consoleFontSize));
            _windowRect = GUILayout.Window(GetInstanceID(), _windowRect, DrawWindow, WindowTitle, _styles.Window);
            HandleWindowResize();
        }

        private void DrawWindow(int windowId)
        {
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

        private struct LogInfo
        {
            public string Message;
            public string StackTrace;
            public LogType LogType;
            public DateTime Time;
        }

        private sealed class CommandPanel
        {
            private const string CommandInputControlName = "NonseniscalCommandInput";
            private const float PromptLabelWidth = 16f;
            private const float SendButtonWidth = 70f;
            private static readonly char[] CommandSeparator = { ' ' };

            private string _commandInput = string.Empty;
            private readonly List<string> _commandHistory = new List<string>();
            private int _historyIndex = -1;
            private string _historyDraft = string.Empty;

            public void Draw(Action<string, string[]> onSubmit, ConsoleStyles styles)
            {
                if (ShouldSubmitOnEnter(Event.current))
                {
                    Submit(onSubmit);
                    Event.current.Use();
                }
                else if (TryNavigateHistory(Event.current))
                {
                    Event.current.Use();
                }

                GUILayout.Label("Command", styles.Label);
                GUILayout.BeginHorizontal();
                GUILayout.Label(">", styles.Label, GUILayout.Width(PromptLabelWidth));
                GUI.SetNextControlName(CommandInputControlName);
                _commandInput = GUILayout.TextField(_commandInput, styles.TextField, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Send", styles.Button, GUILayout.Width(SendButtonWidth)))
                {
                    Submit(onSubmit);
                }

                GUILayout.EndHorizontal();
                GUILayout.Label("格式: commandId [argument]，按 Enter 发送", styles.Label);
            }

            private bool ShouldSubmitOnEnter(Event current)
            {
                if (GUI.GetNameOfFocusedControl() != CommandInputControlName || current.type != EventType.KeyDown)
                {
                    return false;
                }

                return current.keyCode == KeyCode.Return ||
                       current.keyCode == KeyCode.KeypadEnter ||
                       current.character == '\n' ||
                       current.character == '\r';
            }

            private bool TryNavigateHistory(Event current)
            {
                if (GUI.GetNameOfFocusedControl() != CommandInputControlName || current.type != EventType.KeyDown)
                {
                    return false;
                }

                if (current.keyCode == KeyCode.UpArrow)
                {
                    return NavigateToOlderCommand();
                }

                if (current.keyCode == KeyCode.DownArrow)
                {
                    return NavigateToNewerCommand();
                }

                return false;
            }

            private bool NavigateToOlderCommand()
            {
                if (_commandHistory.Count == 0)
                {
                    return false;
                }

                if (_historyIndex < 0)
                {
                    _historyDraft = _commandInput;
                    _historyIndex = _commandHistory.Count - 1;
                }
                else if (_historyIndex > 0)
                {
                    _historyIndex--;
                }

                _commandInput = _commandHistory[_historyIndex];
                return true;
            }

            private bool NavigateToNewerCommand()
            {
                if (_historyIndex < 0)
                {
                    return false;
                }

                if (_historyIndex < _commandHistory.Count - 1)
                {
                    _historyIndex++;
                    _commandInput = _commandHistory[_historyIndex];
                }
                else
                {
                    _historyIndex = -1;
                    _commandInput = _historyDraft;
                }

                return true;
            }

            private void Submit(Action<string, string[]> onSubmit)
            {
                string line = _commandInput?.Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    return;
                }

                string[] tokens = line.Split(CommandSeparator, StringSplitOptions.RemoveEmptyEntries);
                string commandId = tokens[0];
                string[] param = new string[Mathf.Max(0, tokens.Length - 1)];
                if (tokens.Length > 1)
                {
                    Array.Copy(tokens, 1, param, 0, param.Length);
                }

                onSubmit(commandId, param);
                if (_commandHistory.Count == 0 || _commandHistory[_commandHistory.Count - 1] != line)
                {
                    _commandHistory.Add(line);
                }

                _historyIndex = -1;
                _historyDraft = string.Empty;
                _commandInput = string.Empty;
            }
        }

        private sealed class ConsoleStyles
        {
            private int _fontSize = -1;
            private GUISkin _skin;

            public GUIStyle Window { get; private set; }
            public GUIStyle Label { get; private set; }
            public GUIStyle Button { get; private set; }
            public GUIStyle TextField { get; private set; }
            public GUIStyle Toggle { get; private set; }

            public void Update(int fontSize)
            {
                if (GUI.skin == null)
                {
                    return;
                }

                if (_fontSize == fontSize && ReferenceEquals(_skin, GUI.skin))
                {
                    return;
                }

                _fontSize = fontSize;
                _skin = GUI.skin;

                Window = Clone(_skin.window, fontSize);
                Label = Clone(_skin.label, fontSize);
                Button = Clone(_skin.button, fontSize);
                TextField = Clone(_skin.textField, fontSize);
                Toggle = Clone(_skin.toggle, fontSize);
            }

            private static GUIStyle Clone(GUIStyle source, int fontSize)
            {
                GUIStyle style = source == null ? new GUIStyle() : new GUIStyle(source);
                style.fontSize = fontSize;
                return style;
            }
        }

        private sealed class LogPanel
        {
            private static readonly Dictionary<LogType, Color> TypeColors = new Dictionary<LogType, Color>
            {
                { LogType.Assert, Color.white },
                { LogType.Error, Color.red },
                { LogType.Exception, Color.red },
                { LogType.Log, Color.white },
                { LogType.Warning, Color.yellow }
            };

            private readonly List<LogInfo> _logs = new List<LogInfo>();
            private Vector2 _scrollPosition;

            public void Bind()
            {
                Application.logMessageReceived += HandleLogMessageReceived;
            }

            public void Unbind()
            {
                Application.logMessageReceived -= HandleLogMessageReceived;
            }

            public void Draw(
                ref bool collapse,
                ref bool showStackTrace,
                bool restrictLogCount,
                int maxLogs,
                int maxLogCharsPerLine,
                ConsoleStyles styles)
            {
                DrawToolbar(ref collapse, ref showStackTrace, styles);

                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));
                for (int i = 0; i < _logs.Count; i++)
                {
                    LogInfo log = _logs[i];
                    if (collapse && i > 0 && _logs[i - 1].Message == log.Message)
                    {
                        continue;
                    }

                    GUI.contentColor = TypeColors.TryGetValue(log.LogType, out Color color) ? color : Color.white;

                    GUILayout.Label(BuildDisplayMessage(log, maxLogCharsPerLine), styles.Label);

                    if (showStackTrace && string.IsNullOrEmpty(log.StackTrace) == false)
                    {
                        GUILayout.Label(log.StackTrace, styles.Label);
                    }
                }

                GUI.contentColor = Color.white;
                GUILayout.EndScrollView();

                if (restrictLogCount)
                {
                    TrimLogs(maxLogs);
                }
            }

            private void DrawToolbar(ref bool collapse, ref bool showStackTrace, ConsoleStyles styles)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Clear", styles.Button, GUILayout.Width(70)))
                {
                    _logs.Clear();
                }

                collapse = GUILayout.Toggle(collapse, "Collapse", styles.Toggle, GUILayout.Width(90));
                showStackTrace = GUILayout.Toggle(showStackTrace, "StackTrace", styles.Toggle, GUILayout.Width(110));
                GUILayout.Label($"Count: {_logs.Count}", styles.Label, GUILayout.Width(120));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            private static string BuildDisplayMessage(LogInfo log, int maxLogCharsPerLine)
            {
                string message = $"{log.Time:HH:mm:ss} [{log.LogType}] {log.Message}";
                if (message.Length <= maxLogCharsPerLine)
                {
                    return message;
                }

                return message.Substring(0, maxLogCharsPerLine) + "...";
            }

            private void HandleLogMessageReceived(string message, string stackTrace, LogType type)
            {
                _logs.Add(new LogInfo
                {
                    Message = message,
                    StackTrace = stackTrace,
                    LogType = type,
                    Time = DateTime.Now,
                });
            }

            private void TrimLogs(int maxLogs)
            {
                int amountToRemove = Mathf.Max(_logs.Count - Mathf.Max(1, maxLogs), 0);
                if (amountToRemove > 0)
                {
                    _logs.RemoveRange(0, amountToRemove);
                }
            }
        }
    }
}
