using System.Runtime.CompilerServices;
using UnityEngine;

namespace NonsensicalKit.Core.Log
{
    public interface ILog
    {
        public void Debug(object obj,
            Object context = null,
            string[] tags = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0);

        public void Info(object obj,
            Object context = null,
            string[] tags = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0);

        public void Warning(object obj,
            Object context = null,
            string[] tags = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0);

        public void Error(object obj,
            Object context = null,
            string[] tags = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0);

        public void Fatal(object obj,
            Object context = null,
            string[] tags = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0);
    }
}
