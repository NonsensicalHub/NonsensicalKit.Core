using NonsensicalKit.Editor.Service;
using NonsensicalKit.Editor.Service.Config;
using NonsensicalKit.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace NonsensicalKit.Editor.Log.NonsensicalLog
{
    /// <summary>
    /// 可以在一定程度上进行配置的日志类
    /// 需要NonsensicalKit.Core.Service.Config.ConfigService服务
    /// </summary>
    public class NonsensicalLog : ILog
    {
        private StringBuilder _sb;
        private Queue<LogContext> _buffer;

        private bool _isReady;

        private LogLevel _logLevel;
        private LogStrategy[] _logStrategy;
        private bool _logDateTime;
        private bool _logClassInfo;

        private bool _logConsole;
        private bool _logPersistentFile;

        private FileStream _fs;
        private StreamWriter _sw;

        public NonsensicalLog()
        {
            _sb = new StringBuilder();
            _buffer = new Queue<LogContext>();
            UnityEngine.Debug.Log($"NonsensicalLog Init\r\n" +
                $"DateTime:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}\r\n" +
                $"Device Model:{SystemInfo.deviceModel}\r\n" +
                $"Device Name:{SystemInfo.deviceName}\r\n" +
                $"Operating System:{SystemInfo.operatingSystem}");

            ServiceCore.AfterServiceCoreInit += OnServiceCoreInit;
        }

         ~NonsensicalLog()
        {
            _sb.Clear();
            _sw?.Flush();
            _fs?.Flush();
            _sw?.Close();
            _fs?.Close();
        }

        public void Debug(object obj, string[] tags = null, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            TryLog(new LogContext(LogLevel.DEBUG, obj, tags, callerMemberName, callerFilePath, callerLineNumber));
        }

        public void Info(object obj, string[] tags = null, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            TryLog(new LogContext(LogLevel.INFO, obj, tags, callerMemberName, callerFilePath, callerLineNumber));
        }

        public void Warning(object obj, string[] tags = null, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            TryLog(new LogContext(LogLevel.WARNING, obj, tags, callerMemberName, callerFilePath, callerLineNumber));
        }

        public void Error(object obj, string[] tags = null, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            TryLog(new LogContext(LogLevel.ERROR, obj, tags, callerMemberName, callerFilePath, callerLineNumber));
        }

        public void Fatal(object obj, string[] tags = null, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            TryLog(new LogContext(LogLevel.FATAL, obj, tags, callerMemberName, callerFilePath, callerLineNumber));
        }

        private void TryLog(LogContext info)
        {
            if (!_isReady)
            {
                _buffer.Enqueue(info);
            }
            else
            {
                Log(info);
            }
        }

        private void Log(LogContext info)
        {
            if (info.LogLevel< _logLevel)
            {
                return;
            }
            _sb.Clear();
            _sb.Append(info.LogLevel);
            _sb.Append(": ");
            if (info.Obj != null)
            {
                _sb.AppendLine(info.Obj.ToString());
            }
            else
            {
                _sb.AppendLine("null");
            }
            if (info.Tags != null && info.Tags.Length > 0)
            {
                _sb.Append("Tags:[");
                foreach (var tag in info.Tags)
                {
                    _sb.Append($"{tag},");
                }
                _sb.Remove(_sb.Length - 1, 1);  //去掉最后一个逗号
                _sb.AppendLine("]");
            }
            if (_logDateTime)
            {
                _sb.AppendLine($"DateTime:{info.Time.ToString("yyyy-MM-dd HH:mm:ss")}");
            }
            if (_logClassInfo)
            {
                _sb.AppendLine($"{info.MemberName}(at {info.FilePath} :{info.LineNumber})");
            }

            if (_logConsole)
            {
                switch (info.LogLevel)
                {
                    case LogLevel.DEBUG:
                    case LogLevel.INFO:
                        UnityEngine.Debug.Log(_sb.ToString());
                        break;
                    case LogLevel.WARNING:
                        UnityEngine.Debug.LogWarning(_sb.ToString());
                        break;
                    case LogLevel.ERROR:
                    case LogLevel.FATAL:
                        UnityEngine.Debug.LogError(_sb.ToString());
                        break;
                    case LogLevel.OFF:
                        break;
                }
            }

            if (_logPersistentFile)
            {
                try
                {
                    _sw.Write(_sb.ToString());
                    _sw.Write("\r\n");
                    _sw.Flush();
                    _fs.Flush();
                }
                catch (Exception)
                {
                    _logPersistentFile = false;
                    UnityEngine.Debug.LogError("日志文件无法写入");
                }
            }

        }

        private void Flush()
        {
            while (_buffer.Count>0)
            {
                Log(_buffer.Dequeue());
            }
        }

        private void OnServiceCoreInit()
        {
            ServiceCore.SafeGet<ConfigService>(OnGetConfigService);
        }

        private void OnGetConfigService(ConfigService configService)
        {
            if (configService.TryGetConfig<NonsensicalLogConfigData>(out var data))
            {
                if (PlatformInfo.IsEditor)
                {
                    _logLevel = data.EditorLogLevel;
                    _logStrategy = data.EditorLogStrategy;
                    _logDateTime = data.EditorLogDateTime;
                    _logClassInfo = data.EditorLogCallerInfo;
                }
                else
                {
                    _logLevel = data.RuntimeLogLevel;
                    _logStrategy = data.RuntimeLogStrategy;
                    _logDateTime = data.RuntimeLogDateTime;
                    _logClassInfo = data.RuntimeLogCallerInfo;
                }
                if (_logStrategy.Length==0)
                {
                    _logLevel = LogLevel.OFF;
                }

                foreach (var item in _logStrategy)
                {
                    switch (item)
                    {
                        case LogStrategy.Console:
                            _logConsole =true;
                            break;
                        case LogStrategy.PersistentFile:
                            _logPersistentFile = true;
                            break;
                        default:
                            break;
                    }
                }

                if (_logPersistentFile)
                {
                    try
                    {
                        var logFilePath = Path.Combine(Application.persistentDataPath, "NonsensicalLog", $"Log{DateTime.UtcNow.ToString("yyyy_MM_dd_HH")}.txt");
                        FileTool.EnsureDir(logFilePath);
                        _fs = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.Write);
                        _sw = new StreamWriter(_fs, Encoding.UTF8);
                    }
                    catch (Exception)
                    {
                        _logPersistentFile = false;
                        UnityEngine.Debug.LogError("无法创建日志文件");
                    }
                }

                Flush();
                UnityEngine.Debug.Log($"NonsensicalLog Ready");
                _isReady = true;
            }
            else
            {
                UnityEngine.Debug.LogError($"未配置{nameof(NonsensicalLogConfig)}");
            }
        }
    }
}
