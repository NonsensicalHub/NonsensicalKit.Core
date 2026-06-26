using System;
using System.Collections.Generic;
using UnityEngine;

namespace NonsensicalKit.Tools.GUITool
{
    public partial class NonsensicalConsole
    {
        private sealed class LogPanel
        {
            private struct LogRow
            {
                public string Text;
                public Color Color;
                public float Height;
            }

            private static readonly Dictionary<LogType, Color> TypeColors = new Dictionary<LogType, Color>
            {
                { LogType.Assert, Color.white },
                { LogType.Error, Color.red },
                { LogType.Exception, Color.red },
                { LogType.Log, Color.white },
                { LogType.Warning, Color.yellow }
            };

            private readonly List<LogInfo> _logs = new List<LogInfo>();
            private readonly List<LogInfo> _pendingLogs = new List<LogInfo>();
            private readonly object _pendingLock = new object();
            private readonly List<LogRow> _rows = new List<LogRow>();

            private Vector2 _scrollPosition;
            private bool _layoutDirty = true;
            private float _totalContentHeight;
            private float _lastLayoutContentWidth = -1f;
            private bool _lastCollapse;
            private bool _lastShowStackTrace;
            private int _lastMaxLogChars;

            public void Bind()
            {
                Application.logMessageReceived += HandleLogMessageReceived;
            }

            public void Unbind()
            {
                Application.logMessageReceived -= HandleLogMessageReceived;
                lock (_pendingLock)
                {
                    _pendingLogs.Clear();
                }
            }

            public void FlushPendingLogs(bool restrictLogCount, int maxLogs)
            {
                List<LogInfo> batch = null;
                lock (_pendingLock)
                {
                    if (_pendingLogs.Count > 0)
                    {
                        batch = new List<LogInfo>(_pendingLogs);
                        _pendingLogs.Clear();
                    }
                }

                if (batch != null && batch.Count > 0)
                {
                    for (int i = 0; i < batch.Count; i++)
                    {
                        _logs.Add(batch[i]);
                    }

                    _layoutDirty = true;
                }

                if (restrictLogCount)
                {
                    TrimLogs(maxLogs);
                }
            }

            public void Draw(
                ref bool collapse,
                ref bool showStackTrace,
                bool restrictLogCount,
                int maxLogs,
                int maxLogCharsPerLine,
                ConsoleStyles styles)
            {
                bool collapseChanged = collapse != _lastCollapse;
                bool stackChanged = showStackTrace != _lastShowStackTrace;
                bool charsChanged = maxLogCharsPerLine != _lastMaxLogChars;
                _lastCollapse = collapse;
                _lastShowStackTrace = showStackTrace;
                _lastMaxLogChars = maxLogCharsPerLine;

                DrawToolbar(ref collapse, ref showStackTrace, styles);

                Rect viewport = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                if (viewport.height < 2f)
                {
                    viewport.height = 200f;
                }

                float scrollbarReserve = 20f;
                GUISkin skin = GUI.skin;
                if (skin != null && skin.verticalScrollbar != null)
                {
                    scrollbarReserve = skin.verticalScrollbar.fixedWidth + skin.verticalScrollbar.margin.left +
                                        skin.verticalScrollbar.margin.right;
                }

                float contentWidth = Mathf.Max(40f, viewport.width - scrollbarReserve);

                if (_layoutDirty || collapseChanged || stackChanged || charsChanged ||
                    Mathf.Abs(contentWidth - _lastLayoutContentWidth) > 2f)
                {
                    RebuildVirtualRows(collapse, showStackTrace, maxLogCharsPerLine, contentWidth, styles.Label);
                    _lastLayoutContentWidth = contentWidth;
                    _layoutDirty = false;
                }

                float contentHeight = Mathf.Max(_totalContentHeight, 1f);
                _scrollPosition = GUI.BeginScrollView(
                    viewport,
                    _scrollPosition,
                    new Rect(0f, 0f, contentWidth, contentHeight),
                    false,
                    true);

                float y = 0f;
                float viewTop = _scrollPosition.y;
                float viewBottom = _scrollPosition.y + viewport.height;
                const float overscan = 32f;

                for (int r = 0; r < _rows.Count; r++)
                {
                    LogRow row = _rows[r];
                    float rowBottom = y + row.Height;
                    if (rowBottom < viewTop - overscan)
                    {
                        y = rowBottom;
                        continue;
                    }

                    if (y > viewBottom + overscan)
                    {
                        break;
                    }

                    Color prev = GUI.contentColor;
                    GUI.contentColor = row.Color;
                    GUI.Label(new Rect(0f, y, contentWidth, row.Height), row.Text, styles.Label);
                    GUI.contentColor = prev;
                    y = rowBottom;
                }

                GUI.EndScrollView();
            }

            private void DrawToolbar(ref bool collapse, ref bool showStackTrace, ConsoleStyles styles)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Clear", styles.Button, GUILayout.Width(70)))
                {
                    _logs.Clear();
                    _rows.Clear();
                    _totalContentHeight = 0f;
                    _layoutDirty = true;
                }

                collapse = GUILayout.Toggle(collapse, "Collapse", styles.Toggle, GUILayout.Width(90));
                showStackTrace = GUILayout.Toggle(showStackTrace, "StackTrace", styles.Toggle, GUILayout.Width(110));
                GUILayout.Label($"Count: {_logs.Count}", styles.Label, GUILayout.Width(120));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            private void RebuildVirtualRows(
                bool collapse,
                bool showStackTrace,
                int maxLogCharsPerLine,
                float contentWidth,
                GUIStyle labelStyle)
            {
                _rows.Clear();
                float y = 0f;

                for (int i = 0; i < _logs.Count; i++)
                {
                    LogInfo log = _logs[i];
                    if (collapse && i > 0 && _logs[i - 1].Message == log.Message)
                    {
                        continue;
                    }

                    string message = BuildDisplayMessage(log, maxLogCharsPerLine);
                    Color color = TypeColors.TryGetValue(log.LogType, out Color c) ? c : Color.white;
                    float h = Mathf.Max(
                        labelStyle.CalcHeight(new GUIContent(message), contentWidth),
                        labelStyle.lineHeight > 0f ? labelStyle.lineHeight : labelStyle.fontSize + 6f);

                    _rows.Add(new LogRow { Text = message, Color = color, Height = h });
                    y += h;

                    if (showStackTrace && string.IsNullOrEmpty(log.StackTrace) == false)
                    {
                        string st = log.StackTrace;
                        float hs = Mathf.Max(
                            labelStyle.CalcHeight(new GUIContent(st), contentWidth),
                            labelStyle.lineHeight > 0f ? labelStyle.lineHeight : labelStyle.fontSize + 6f);
                        _rows.Add(new LogRow { Text = st, Color = color, Height = hs });
                        y += hs;
                    }
                }

                _totalContentHeight = y;
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
                var entry = new LogInfo
                {
                    Message = message,
                    StackTrace = stackTrace,
                    LogType = type,
                    Time = DateTime.Now,
                };

                lock (_pendingLock)
                {
                    _pendingLogs.Add(entry);
                }
            }

            private void TrimLogs(int maxLogs)
            {
                int cap = Mathf.Max(1, maxLogs);
                int amountToRemove = Mathf.Max(_logs.Count - cap, 0);
                if (amountToRemove > 0)
                {
                    _logs.RemoveRange(0, amountToRemove);
                    _layoutDirty = true;
                }
            }
        }
    }
}
