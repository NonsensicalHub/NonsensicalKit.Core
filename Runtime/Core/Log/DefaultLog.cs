using System;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace NonsensicalKit.Core.Log
{
    public class DefaultLog : ILog
    {
        private StringBuilder _sb;

        public DefaultLog()
        {
            if (PlatformInfo.IsEditor == false)
            {
                _sb = new StringBuilder();
                UnityEngine.Debug.Log($"StartDefaultLog\r\n" +
                    $"DateTime:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}\r\n" +
                    $"Device Model:{SystemInfo.deviceModel}\r\n" +
                    $"Device Name:{SystemInfo.deviceName}\r\n" +
                    $"Operating System:{SystemInfo.operatingSystem}");
            }
        }

        public void Debug(object obj, string[] tags = null, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (PlatformInfo.IsEditor == false)
            {
                _sb.Clear();
                _sb.Append("Debug: ");
                BuildString(obj, tags, callerMemberName, callerFilePath, callerLineNumber);
                UnityEngine.Debug.Log(_sb.ToString());
            }
        }

        public void Info(object obj, string[] tags = null, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (PlatformInfo.IsEditor == false)
            {
                _sb.Clear();
                _sb.Append("Info: ");
                BuildString(obj, tags, callerMemberName, callerFilePath, callerLineNumber);
                UnityEngine.Debug.Log(_sb.ToString());
            }
        }

        public void Warning(object obj, string[] tags = null, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (PlatformInfo.IsEditor == false)
            {
                _sb.Clear();
                _sb.Append("Warning: ");
                BuildString(obj, tags, callerMemberName, callerFilePath, callerLineNumber);
                UnityEngine.Debug.LogWarning(_sb.ToString());
            }
        }

        public void Error(object obj, string[] tags = null, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (PlatformInfo.IsEditor == false)
            {
                _sb.Clear();
                _sb.Append("Error: ");
                BuildString(obj, tags, callerMemberName, callerFilePath, callerLineNumber);
                UnityEngine.Debug.LogError(_sb.ToString());
            }
        }

        public void Fatal(object obj, string[] tags = null, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (PlatformInfo.IsEditor == false)
            {
                _sb.Clear();
                _sb.Append("Fatal: ");
                BuildString(obj, tags, callerMemberName, callerFilePath, callerLineNumber);
                UnityEngine.Debug.LogError(_sb.ToString());
            }
        }

        private void BuildString(object obj, string[] tags, string callerMemberName = "", string callerFilePath = "", int callerLineNumber = 0)
        {
            if (obj != null)
            {
                _sb.AppendLine(obj.ToString());
            }
            else
            {
                _sb.AppendLine("null");
            }
            if (tags != null && tags.Length > 0)
            {
                _sb.Append("Tags:[");
                foreach (var tag in tags)
                {
                    _sb.Append($"{tag},");
                }
                _sb.Remove(_sb.Length - 1, 1);  //去掉最后一个逗号
                _sb.AppendLine("]");
            }
            _sb.AppendLine($"DateTime:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            _sb.Append($"{callerMemberName}(at {callerFilePath} :{callerLineNumber})");
        }
    }
}
