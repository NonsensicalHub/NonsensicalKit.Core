using System;
using System.Runtime.CompilerServices;
using NonsensicalKit.Core.Setting;
using NonsensicalKit.Tools;
using UnityEngine;
using Object = UnityEngine.Object;

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

        public static void Debug(object obj, Object context = null, string[] tags = null, [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (Logger == null)
            {
                UnityEngine.Debug.Log(obj, context);
            }
            else
            {
                Logger.Debug(obj, context, tags, callerMemberName, callerFilePath, callerLineNumber);
            }
        }

        public static void Info(object obj, Object context = null, string[] tags = null, [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (Logger == null)
            {
                UnityEngine.Debug.Log(obj, context);
            }
            else
            {
                Logger.Info(obj, context, tags, callerMemberName, callerFilePath, callerLineNumber);
            }
        }

        public static void Warning(object obj, Object context = null, string[] tags = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (Logger == null)
            {
                UnityEngine.Debug.LogWarning(obj, context);
            }
            else
            {
                Logger.Warning(obj, context, tags, callerMemberName, callerFilePath, callerLineNumber);
            }
        }

        public static void Error(object obj, Object context = null, string[] tags = null, [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (Logger == null)
            {
                UnityEngine.Debug.LogError(obj, context);
            }
            else
            {
                Logger.Error(obj, context, tags, callerMemberName, callerFilePath, callerLineNumber);
            }
        }

        public static void Fatal(object obj, Object context = null, string[] tags = null, [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (Logger == null)
            {
                UnityEngine.Debug.LogError(obj, context);
            }
            else
            {
                Logger.Fatal(obj, context, tags, callerMemberName, callerFilePath, callerLineNumber);
            }
        }
    }
}
