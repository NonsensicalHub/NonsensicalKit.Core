using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NonsensicalKit.Core.Log
{
    public class DefaultLog : ILog
    {
        private readonly StringBuilder _sb;

        private readonly HashSet<string> _ignoreTags;
        public DefaultLog()
        {
            if (PlatformInfo.IsEditor)
            {
                _sb = new StringBuilder();
                
                var ignoreStr = PlayerPrefs.GetString("NonsensicalKit_Editor_Ignore_Log_Tag_List", "");
                
                _ignoreTags = ignoreStr.Split("|", StringSplitOptions.RemoveEmptyEntries).ToHashSet();
                
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
            if (PlatformInfo.IsEditor&&CheckTags(tags))
            {
                UnityEngine.Debug.Log(BuildString("Debug: ", obj, tags, callerMemberName, callerFilePath, callerLineNumber), context);
            }
        }

        public void Info(object obj, Object context = null, string[] tags = null, [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (PlatformInfo.IsEditor&&CheckTags(tags))
            {
                UnityEngine.Debug.Log(BuildString("Info: ", obj, tags, callerMemberName, callerFilePath, callerLineNumber), context);
            }
        }

        public void Warning(object obj, Object context = null, string[] tags = null, [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (PlatformInfo.IsEditor&&CheckTags(tags))
            {
                UnityEngine.Debug.LogWarning(BuildString("Warning: ", obj, tags, callerMemberName, callerFilePath, callerLineNumber), context);
            }
        }

        public void Error(object obj, Object context = null, string[] tags = null, [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (PlatformInfo.IsEditor&&CheckTags(tags))
            {
                UnityEngine.Debug.LogError(BuildString("Error: ", obj, tags, callerMemberName, callerFilePath, callerLineNumber), context);
            }
        }

        public void Fatal(object obj, Object context = null, string[] tags = null, [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (PlatformInfo.IsEditor&&CheckTags(tags))
            {
                UnityEngine.Debug.LogError(BuildString("Fatal: ", obj, tags, callerMemberName, callerFilePath, callerLineNumber), context);
            }
        }

        private bool CheckTags(string[] tags)
        {
            if (tags==null||tags.Length==0)
            {
                return true;
            }

            foreach (var tag in tags)
            {
                if (_ignoreTags.Contains(tag))
                {
                    return false;
                }
            }

            return true;
        }
        
        private string BuildString(string head, object obj, string[] tags, string callerMemberName, string callerFilePath, int callerLineNumber)
        {
            _sb.Clear();
            _sb.Append(head);
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
            _sb.Append($"{callerMemberName}( at {callerFilePath} :{callerLineNumber})");

            var logStr = _sb.ToString();

            var bs = Encoding.UTF8.GetBytes(logStr);
            if (bs.Length > 15000)
            {
                Array.Resize(ref bs, 15000);
                logStr = Encoding.UTF8.GetString(bs) + "<Cutoff>";
            }

            return logStr;
        }
    }
}
