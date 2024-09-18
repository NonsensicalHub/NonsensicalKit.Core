using System.IO;

namespace NonsensicalKit.Core.Log.NonsensicalLog
{
    public class LogStrategyContext
    {
        public LogLevel LogLevel;
        public LogPathway LogStrategy;
        public bool LogDateTime;
        public bool LogClassInfo;
        public string LogArgument;

        public bool TagCheck;
        public string[] ExcludeTags;
        public string[] LimitedTags;

        public FileStream FileStream;
        public StreamWriter _sw;
    }
}
