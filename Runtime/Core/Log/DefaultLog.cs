using System;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NonsensicalKit.Core.Log
{
    public class DefaultLog : ILog
    {
        private readonly StringBuilder _sb;

        public DefaultLog()
        {
            if (PlatformInfo.IsEditor)
            {
                _sb = new StringBuilder();
                UnityEngine.Debug.Log($"StartDefaultLog\r\n" +
                                      $"DateTime:{DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n" +
                                      $"Device Model:{SystemInfo.deviceModel}\r\n" +
                                      $"Device Name:{SystemInfo.deviceName}\r\n" +
                                      $"Operating System:{SystemInfo.operatingSystem}");
            }
        }

        public void Debug(object obj, Object context = null, string[] tags = null, [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (PlatformInfo.IsEditor)
            {
                _sb.Clear();
                _sb.Append("Debug: ");
                BuildString(obj, tags, callerMemberName, callerFilePath, callerLineNumber);
                UnityEngine.Debug.Log(_sb.ToString(), context);
            }
        }

        public void Info(object obj, Object context = null, string[] tags = null, [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (PlatformInfo.IsEditor)
            {
                _sb.Clear();
                _sb.Append("Info: ");
                BuildString(obj, tags, callerMemberName, callerFilePath, callerLineNumber);
                UnityEngine.Debug.Log(_sb.ToString(), context);
            }
        }

        public void Warning(object obj, Object context = null, string[] tags = null, [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (PlatformInfo.IsEditor)
            {
                _sb.Clear();
                _sb.Append("Warning: ");
                BuildString(obj, tags, callerMemberName, callerFilePath, callerLineNumber);
                UnityEngine.Debug.LogWarning(_sb.ToString(), context);
            }
        }

        public void Error(object obj, Object context = null, string[] tags = null, [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (PlatformInfo.IsEditor)
            {
                _sb.Clear();
                _sb.Append("Error: ");
                BuildString(obj, tags, callerMemberName, callerFilePath, callerLineNumber);
                UnityEngine.Debug.LogError(_sb.ToString(), context);
            }
        }

        public void Fatal(object obj, Object context = null, string[] tags = null, [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (PlatformInfo.IsEditor)
            {
                _sb.Clear();
                _sb.Append("Fatal: ");
                BuildString(obj, tags, callerMemberName, callerFilePath, callerLineNumber);
                UnityEngine.Debug.LogError(_sb.ToString(), context);
            }
        }

        private void BuildString(object obj, string[] tags, string callerMemberName, string callerFilePath, int callerLineNumber)
        {
            _sb.AppendLine(obj != null ? obj.ToString() : "null");

            if (tags is { Length: > 0 })
            {
                _sb.Append("Tags:[");
                foreach (var tag in tags)
                {
                    _sb.Append($"{tag},");
                }

                _sb.Remove(_sb.Length - 1, 1); //去掉最后一个逗号
                _sb.AppendLine("]");
            }

            _sb.AppendLine($"DateTime:{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            _sb.Append($"{callerMemberName}(at {callerFilePath} :{callerLineNumber})");
        }
    }
}
