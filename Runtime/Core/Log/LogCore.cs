using NonsensicalKit.Core.Setting;
using NonsensicalKit.Tools;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace NonsensicalKit.Core.Log
{
    public static class LogCore
    {
        public static ILog Logger;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void CreateLog()
        {
            var setting = NonsensicalSetting.LoadSetting();
            if (setting == null)
            {
                return;
            }
            var runningLog = setting.RunningLogger;

            var allLogTypes = ReflectionTool.GetConcreteTypes<ILog>();

            foreach (Type type in allLogTypes)
            {
                if (runningLog.Equals(type.Name))
                {
                    Logger = Activator.CreateInstance(type) as ILog;

                    break;
                }
            }

            if (Logger == null)
            {
                Logger = new DefaultLog();
            }
        }

        public static void Debug(object obj, string[] tags = null, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            Logger.Debug(obj, tags, callerMemberName, callerFilePath, callerLineNumber);
        }

        public static void Info(object obj, string[] tags = null, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            Logger.Info(obj, tags, callerMemberName, callerFilePath, callerLineNumber);
        }

        public static void Warning(object obj, string[] tags = null, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            Logger.Warning(obj, tags, callerMemberName, callerFilePath, callerLineNumber);
        }

        public static void Error(object obj, string[] tags = null, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            Logger.Error(obj, tags, callerMemberName, callerFilePath, callerLineNumber);
        }

        public static void Fatal(object obj, string[] tags = null, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            Logger.Fatal(obj, tags, callerMemberName, callerFilePath, callerLineNumber);
        }
    }
}
