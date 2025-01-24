using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NonsensicalKit.Tools.GUITool
{
    /// <summary>
    /// A console to display Unity's debug logs in-game.
    /// </summary>
    public class DebugConsole : MonoBehaviour
    {
        public static DebugConsole Instance;

        #region Inspector Settings

        /// <summary>
        /// Whether to open the window by shaking the device (mobile-only).
        /// </summary>
        [SerializeField] private bool m_shakeToOpen = true;

        /// <summary>
        /// The (squared) acceleration above which the window should open.
        /// </summary>
        [SerializeField] private float m_shakeAcceleration = 3f;

        /// <summary>
        /// Whether to only keep a certain number of logs.
        ///
        /// Setting this can be helpful if memory usage is a concern.
        /// </summary>
        [SerializeField] private bool m_restrictLogCount = false;

        /// <summary>
        /// Number of logs to keep before removing old ones.
        /// </summary>
        [SerializeField] private int m_maxLogs = 1000;

        #endregion

        private readonly List<LogInfo> _logs = new List<LogInfo>();
        private Vector2 _scrollPosition;
        private bool _visible;
        private bool _collapse;

        // Visual elements:

        private static readonly Dictionary<LogType, Color> _logTypeColors = new Dictionary<LogType, Color>
        {
            { LogType.Assert, Color.white },
            { LogType.Error, Color.red },
            { LogType.Exception, Color.red },
            { LogType.Log, Color.white },
            { LogType.Warning, Color.yellow },
        };

        private const string WINDOW_TITLE = "Console";
        private const int MARGIN = 20;
        private static readonly GUIContent _clearLabel = new GUIContent("Clear", "Clear the contents of the console.");
        private static readonly GUIContent _collapseLabel = new GUIContent("Collapse", "Hide repeated messages.");

        private readonly Rect _titleBarRect = new Rect(0, 0, 10000, 20);
        private Rect _windowRect = new Rect(MARGIN, MARGIN, Screen.width - (MARGIN * 2), Screen.height - (MARGIN * 2));


        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current.backquoteKey.wasPressedThisFrame)
            {
                SwitchVisible();
            }
#else
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                SwitchVisible();
            }
#endif

            if (m_shakeToOpen && Input.acceleration.sqrMagnitude > m_shakeAcceleration)
            {
                SwitchVisible();
            }
        }

        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void OnGUI()
        {
            if (!_visible)
            {
                return;
            }

            _windowRect = GUILayout.Window(123456, _windowRect, DrawConsoleWindow, WINDOW_TITLE);
        }

        public void SwitchVisible()
        {
            _visible = !_visible;
        }

        /// <summary>
        /// Displays a window that lists the recorded logs.
        /// </summary>
        /// <param name="windowID">Window ID.</param>
        private void DrawConsoleWindow(int windowID)
        {
            DrawLogsList();
            DrawToolbar();

            // Allow the window to be dragged by its title bar.
            GUI.DragWindow(_titleBarRect);
        }

        /// <summary>
        /// Displays a scrollable list of logs.
        /// </summary>
        private void DrawLogsList()
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            // Iterate through the recorded logs.
            for (var i = 0; i < _logs.Count; i++)
            {
                var log = _logs[i];

                // Combine identical messages if collapse option is chosen.
                if (_collapse && i > 0)
                {
                    var previousMessage = _logs[i - 1].Message;

                    if (log.Message == previousMessage)
                    {
                        continue;
                    }
                }

                GUI.contentColor = _logTypeColors[log.LogType];

                string logContent = log.ToString();

                if (logContent.Length > 1000)
                {
                    logContent = logContent.Substring(0, 1000);
                    logContent += "...";
                }

                GUILayout.Label(logContent);
            }

            GUILayout.EndScrollView();

            // Ensure GUI colour is reset before drawing other components.
            GUI.contentColor = Color.white;
        }

        /// <summary>
        /// Displays options for filtering and changing the logs list.
        /// </summary>
        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(_clearLabel))
            {
                _logs.Clear();
            }


            _collapse = GUILayout.Toggle(_collapse, _collapseLabel, GUILayout.ExpandWidth(false));

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Records a log from the log callback.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="stackTrace">Trace of where the message came from.</param>
        /// <param name="type">Type of message (error, exception, warning, assert).</param>
        private void HandleLog(string message, string stackTrace, LogType type)
        {
            _logs.Add(new LogInfo(message, stackTrace, type, DateTime.Now));
            TrimExcessLogs();
        }

        /// <summary>
        /// Removes old logs that exceed the maximum number allowed.
        /// </summary>
        private void TrimExcessLogs()
        {
            if (!m_restrictLogCount)
            {
                return;
            }

            var amountToRemove = Mathf.Max(_logs.Count - m_maxLogs, 0);

            if (amountToRemove == 0)
            {
                return;
            }

            _logs.RemoveRange(0, amountToRemove);
        }
    }


    struct LogInfo
    {
        public string Message { get; private set; }
        public string StackTrace { get; private set; }
        public LogType LogType { get; private set; }
        public DateTime DateTime { get; private set; }

        public LogInfo(string message, string stackTrace, LogType logType, DateTime dateTime)
        {
            Message = message;
            StackTrace = stackTrace;
            LogType = logType;
            DateTime = dateTime;
        }

        public bool CheckType(LogType logType)
        {
            return LogType == logType;
        }

        public override string ToString()
        {
            return DateTime + " , " + LogType.ToString() + "\r\n    " + Message + "\r\n    " + StackTrace;
        }
    }
}
