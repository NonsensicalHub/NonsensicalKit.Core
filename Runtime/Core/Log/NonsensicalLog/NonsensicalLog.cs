using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using NonsensicalKit.Core.Service;
using NonsensicalKit.Core.Service.Config;
using NonsensicalKit.Tools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NonsensicalKit.Core.Log.NonsensicalLog
{
    /// <summary>
    /// 可以在一定程度上进行配置的日志类
    /// 需要NonsensicalKit.Core.Service.Config.ConfigService服务
    /// </summary>
    public class NonsensicalLog : ILog
    {
        private List<LogStrategyContext> _strategies;
        private readonly StringBuilder _sb;
        private readonly Queue<LogContext> _buffer;

        private bool _isReady;

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

            foreach (var item in _strategies)
            {
                item.Writer?.Flush();
                item.FileStream?.Flush();
                item.Writer?.Close();
                item.FileStream?.Close();
            }
        }

        public void Debug(object obj, Object context = null, string[] tags = null, [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            TryLog(new LogContext(LogLevel.DEBUG, obj, context, tags, callerMemberName, callerFilePath, callerLineNumber));
        }

        public void Info(object obj, Object context = null, string[] tags = null, [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            TryLog(new LogContext(LogLevel.INFO, obj, context, tags, callerMemberName, callerFilePath, callerLineNumber));
        }

        public void Warning(object obj, Object context = null, string[] tags = null, [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            TryLog(new LogContext(LogLevel.WARNING, obj, context, tags, callerMemberName, callerFilePath, callerLineNumber));
        }

        public void Error(object obj, Object context = null, string[] tags = null, [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            TryLog(new LogContext(LogLevel.ERROR, obj, context, tags, callerMemberName, callerFilePath, callerLineNumber));
        }

        public void Fatal(object obj, Object context = null, string[] tags = null, [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            TryLog(new LogContext(LogLevel.FATAL, obj, context, tags, callerMemberName, callerFilePath, callerLineNumber));
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
            foreach (var strategy in _strategies)
            {
                if (info.LogLevel < strategy.LogLevel)
                {
                    continue;
                }

                if (strategy.TagCheck)
                {
                    bool flag = false;
                    foreach (var item in info.Tags)
                    {
                        if (strategy.ExcludeTags.Contains(item))
                        {
                            flag = true;
                            break;
                        }

                        if (strategy.LimitedTags.Length > 0 && (strategy.LimitedTags.Contains(item) == false))
                        {
                            flag = true;
                            break;
                        }
                    }

                    if (flag)
                    {
                        continue;
                    }
                }

                _sb.Clear();
                _sb.Append(info.LogLevel);
                _sb.Append(": ");
                _sb.AppendLine(info.Obj != null ? info.Obj.ToString() : "null");

                if (info.Tags != null && info.Tags.Length > 0)
                {
                    _sb.Append("Tags:[");
                    foreach (var tag in info.Tags)
                    {
                        _sb.Append($"{tag},");
                    }

                    _sb.Remove(_sb.Length - 1, 1); //去掉最后一个逗号
                    _sb.AppendLine("]");
                }

                if (strategy.LogDateTime)
                {
                    _sb.AppendLine($"DateTime:{info.Time.ToString("yyyy-MM-dd HH:mm:ss")}");
                }

                if (strategy.LogClassInfo)
                {
                    _sb.AppendLine($"{info.MemberName}(at {info.FilePath} :{info.LineNumber})");
                }

                switch (strategy.LogStrategy)
                {
                    case LogPathway.Console:
                        switch (info.LogLevel)
                        {
                            case LogLevel.DEBUG:
                            case LogLevel.INFO:
                                UnityEngine.Debug.Log(_sb.ToString(), info.Context);
                                break;
                            case LogLevel.WARNING:
                                UnityEngine.Debug.LogWarning(_sb.ToString(), info.Context);
                                break;
                            case LogLevel.ERROR:
                            case LogLevel.FATAL:
                                UnityEngine.Debug.LogError(_sb.ToString(), info.Context);
                                break;
                            case LogLevel.OFF:
                                break;
                        }

                        break;
                    case LogPathway.PersistentFile:
                    case LogPathway.CustomPathFile:
                        try
                        {
                            strategy.Writer.Write(_sb.ToString());
                            strategy.Writer.Write("\r\n");
                            strategy.Writer.Flush();
                            strategy.FileStream.Flush();
                        }
                        catch (Exception)
                        {
                            UnityEngine.Debug.LogError("日志文件无法写入");
                        }

                        break;
                }
            }
        }

        private void Flush()
        {
            while (_buffer.Count > 0)
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
                _strategies = new List<LogStrategyContext>();
                foreach (var strategyConfig in data.m_Strategies)
                {
                    if ((PlatformInfo.IsEditor && !strategyConfig.WorkInEditor)
                        || (!PlatformInfo.IsEditor && !strategyConfig.WorkInRuntime))
                    {
                        continue;
                    }

                    LogStrategyContext strategy = new LogStrategyContext();
                    strategy.LogLevel = strategyConfig.LogLevel;
                    strategy.LogStrategy = strategyConfig.LogStrategy;
                    strategy.LogArgument = strategyConfig.LogArgument;
                    strategy.LogDateTime = strategyConfig.LogDateTime;
                    strategy.LogClassInfo = strategyConfig.LogCallerInfo;
                    strategy.TagCheck = (strategyConfig.ExcludeTags.Length > 0) || (strategyConfig.LimitedTags.Length > 0);
                    strategy.ExcludeTags = strategyConfig.ExcludeTags;
                    strategy.LimitedTags = strategyConfig.LimitedTags;

                    switch (strategy.LogStrategy)
                    {
                        case LogPathway.Console:
                            break;
                        case LogPathway.PersistentFile:
                            try
                            {
                                var logFilePath = Path.Combine(Application.persistentDataPath, "NonsensicalLog",
                                    $"Log{DateTime.UtcNow:yyyy_MM_dd_HH}.txt");
                                FileTool.EnsureFileDir(logFilePath);
                                strategy.FileStream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.Write);
                                strategy.Writer = new StreamWriter(strategy.FileStream, Encoding.UTF8);
                            }
                            catch (Exception)
                            {
                                UnityEngine.Debug.LogError("无法创建日志文件");
                                continue;
                            }

                            break;
                        case LogPathway.CustomPathFile:
                            try
                            {
                                var logFilePath = Path.Combine(strategy.LogArgument);
                                FileTool.EnsureFileDir(logFilePath);
                                strategy.FileStream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.Write);
                                strategy.Writer = new StreamWriter(strategy.FileStream, Encoding.UTF8);
                            }
                            catch (Exception)
                            {
                                UnityEngine.Debug.LogError("无法创建日志文件");
                                continue;
                            }

                            break;
                    }

                    _strategies.Add(strategy);
                }

                UnityEngine.Debug.Log($"NonsensicalLog Ready");

                Flush();

                _isReady = true;
            }
            else
            {
                UnityEngine.Debug.LogError($"未配置{nameof(NonsensicalLogConfig)}");
            }
        }
    }
}
