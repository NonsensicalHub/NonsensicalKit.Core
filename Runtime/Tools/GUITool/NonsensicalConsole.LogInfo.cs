using System;
using UnityEngine;

namespace NonsensicalKit.Tools.GUITool
{
    public partial class NonsensicalConsole
    {
        private struct LogInfo
        {
            public string Message;
            public string StackTrace;
            public LogType LogType;
            public DateTime Time;
        }
    }
}
