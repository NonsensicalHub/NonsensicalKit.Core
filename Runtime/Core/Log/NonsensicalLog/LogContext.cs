using System;
using Object = UnityEngine.Object;

namespace NonsensicalKit.Core.Log.NonsensicalLog
{
    /// <summary>
    /// 日志上下文
    /// </summary>
    public class LogContext
    {
        public readonly LogLevel LogLevel;
        public readonly object Obj;
        public readonly Object Context;
        public readonly string[] Tags;
        public readonly DateTime Time;
        public readonly string MemberName;
        public readonly string FilePath;
        public readonly int LineNumber;

        public LogContext(LogLevel logLevel, object obj, Object context, string[] tags, string memberName, string filePath,
            int lineNumber)
        {
            LogLevel = logLevel;
            Context = context;
            Obj = obj;
            Tags = tags;
            Time = DateTime.UtcNow;
            MemberName = memberName;
            FilePath = filePath;
            LineNumber = lineNumber;
        }
    }
}
