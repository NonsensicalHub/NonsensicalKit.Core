using System;

namespace NonsensicalKit.Core.Log.NonsensicalLog
{
    /// <summary>
    /// 日志上下文
    /// </summary>
    public class LogContext
    {
        public LogLevel LogLevel;
        public object Obj;
        public string[] Tags;
        public DateTime Time;
        public string MemberName;
        public string FilePath;
        public int LineNumber;

        public LogContext(LogLevel logLevel,object obj, string[] tags, string memberName, string filePath, int lineNumber)
        {
            LogLevel = logLevel;
            Obj = obj;
            Tags = tags;
            Time = DateTime.UtcNow;
            MemberName = memberName;
            FilePath = filePath;
            LineNumber = lineNumber;
        }
    }
}
