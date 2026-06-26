using System;
using System.Collections.Generic;
using UnityEngine;

namespace NonsensicalKit.Tools.GUITool
{
    public partial class NonsensicalConsole
    {
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
    }
}
