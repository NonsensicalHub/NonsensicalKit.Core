using System;
using System.Runtime.CompilerServices;
using NonsensicalKit.Core.Setting;
using NonsensicalKit.Tools;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace NonsensicalKit.Core.Log
{
    public static class LogCore
    {
        private static ILog _logger;

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
                    _logger = Activator.CreateInstance(type) as ILog;
                    break;
                }
            }

            _logger ??= new DefaultLog();
        }

        public static void Debug(object obj, string tag, UnityObject context = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (_logger == null)
            {
                UnityEngine.Debug.Log(obj, context);
            }
            else
            {
                _logger.Debug(obj, context, new[] { tag }, callerMemberName, callerFilePath, callerLineNumber);
            }
        }

        public static void Debug(object obj, UnityObject context = null, string[] tags = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (_logger == null)
            {
                UnityEngine.Debug.Log(obj, context);
            }
            else
            {
                _logger.Debug(obj, context, tags, callerMemberName, callerFilePath, callerLineNumber);
            }
        }

        public static void Info(object obj, string tag, UnityObject context = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (_logger == null)
            {
                UnityEngine.Debug.Log(obj, context);
            }
            else
            {
                _logger.Info(obj, context, new[] { tag }, callerMemberName, callerFilePath, callerLineNumber);
            }
        }

        public static void Info(object obj, UnityObject context = null, string[] tags = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (_logger == null)
            {
                UnityEngine.Debug.Log(obj, context);
            }
            else
            {
                _logger.Info(obj, context, tags, callerMemberName, callerFilePath, callerLineNumber);
            }
        }

        public static void Warning(object obj, string tag, UnityObject context = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (_logger == null)
            {
                UnityEngine.Debug.LogWarning(obj, context);
            }
            else
            {
                _logger.Warning(obj, context, new[] { tag }, callerMemberName, callerFilePath, callerLineNumber);
            }
        }

        public static void Warning(object obj, UnityObject context = null, string[] tags = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if (_logger == null)
            {
                UnityEngine.Debug.LogWarning(obj, context);
            }
            else
            {
                _logger.Warning(obj, context, tags, callerMemberName, callerFilePath, callerLineNumber);
            }
        }

        public static void Error(object obj, string tag, UnityObject context = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (_logger == null)
            {
                UnityEngine.Debug.LogError(obj, context);
            }
            else
            {
                _logger.Error(obj, context, new[] { tag }, callerMemberName, callerFilePath, callerLineNumber);
            }
        }

        public static void Error(object obj, UnityObject context = null, string[] tags = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (_logger == null)
            {
                UnityEngine.Debug.LogError(obj, context);
            }
            else
            {
                _logger.Error(obj, context, tags, callerMemberName, callerFilePath, callerLineNumber);
            }
        }

        public static void Fatal(object obj, string tag, UnityObject context = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (_logger == null)
            {
                UnityEngine.Debug.LogError(obj, context);
            }
            else
            {
                _logger.Fatal(obj, context, new[] { tag }, callerMemberName, callerFilePath, callerLineNumber);
            }
        }

        public static void Fatal(object obj, UnityObject context = null, string[] tags = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (_logger == null)
            {
                UnityEngine.Debug.LogError(obj, context);
            }
            else
            {
                _logger.Fatal(obj, context, tags, callerMemberName, callerFilePath, callerLineNumber);
            }
        }
    }
}
