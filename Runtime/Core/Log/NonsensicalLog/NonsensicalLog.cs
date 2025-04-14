using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NonsensicalKit.Core.Service;
using NonsensicalKit.Core.Service.Config;
using NonsensicalKit.Tools;
using UnityEngine;
using UnityObject = UnityEngine.Object;

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
        private readonly HashSet<string> _ignoreTags;
        private bool _isReady;

        public NonsensicalLog()
        {
            var ignoreStr = PlayerPrefs.GetString("NonsensicalKit_Editor_Ignore_Log_Tag_List", "");

            _ignoreTags = ignoreStr.Split("|", StringSplitOptions.RemoveEmptyEntries).ToHashSet();
            _sb = new StringBuilder();
            _buffer = new Queue<LogContext>();
            UnityEngine.Debug.Log($"NonsensicalLog Init\r\n" +
                                  $"DateTime:{DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n" +
                                  $"Device Model:{SystemInfo.deviceModel}\r\n" +
                                  $"Device Name:{SystemInfo.deviceName}\r\n" +
                                  $"Operating System:{SystemInfo.operatingSystem}");

            ServiceCore.AfterServiceCoreInit += OnServiceCoreInit; //LogCore在ServiceCore之前初始化，所以需要等待
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

        public void Debug(object obj, UnityObject context = null, string[] tags = null, string callerMemberName = "", string callerFilePath = "",
            int callerLineNumber = 0)
        {
            TryLog(new LogContext(LogLevel.Debug, obj, context, tags, callerMemberName, callerFilePath, callerLineNumber));
        }

        public void Info(object obj, UnityObject context = null, string[] tags = null, string callerMemberName = "", string callerFilePath = "",
            int callerLineNumber = 0)
        {
            TryLog(new LogContext(LogLevel.Info, obj, context, tags, callerMemberName, callerFilePath, callerLineNumber));
        }

        public void Warning(object obj, UnityObject context = null, string[] tags = null, string callerMemberName = "", string callerFilePath = "",
            int callerLineNumber = 0)
        {
            TryLog(new LogContext(LogLevel.Warning, obj, context, tags, callerMemberName, callerFilePath, callerLineNumber));
        }

        public void Error(object obj, UnityObject context = null, string[] tags = null, string callerMemberName = "", string callerFilePath = "",
            int callerLineNumber = 0)
        {
            TryLog(new LogContext(LogLevel.Error, obj, context, tags, callerMemberName, callerFilePath, callerLineNumber));
        }

        public void Fatal(object obj, UnityObject context = null, string[] tags = null, string callerMemberName = "", string callerFilePath = "",
            int callerLineNumber = 0)
        {
            TryLog(new LogContext(LogLevel.Fatal, obj, context, tags, callerMemberName, callerFilePath, callerLineNumber));
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

        private bool CheckTags(string[] tags)
        {
            if (tags == null || tags.Length == 0)
            {
                return true;
            }

            foreach (var tag in tags)
            {
                if (_ignoreTags.Contains(tag))
                {
                    return false;
                }
            }

            return true;
        }

        private void Log(LogContext info)
        {
            if (PlatformInfo.IsEditor && !CheckTags(info.Tags))
            {
                return;
            }

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

                if (info.Tags is { Length: > 0 })
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
                    _sb.AppendLine($"DateTime:{info.Time:yyyy-MM-dd HH:mm:ss}");
                }

                if (strategy.LogClassInfo)
                {
                    _sb.AppendLine($"{info.MemberName}( at {info.FilePath} :{info.LineNumber})");
                }

                switch (strategy.LogStrategy)
                {
                    case LogPathway.Console:
                        var logStr = _sb.ToString();
                        if (PlatformInfo.IsEditor)
                        {
                            var bs = Encoding.UTF8.GetBytes(logStr);
                            if (bs.Length > 15000)
                            {
                                Array.Resize(ref bs, 15000);
                                logStr = Encoding.UTF8.GetString(bs) + "<Cutoff>";
                            }
                        }

                        switch (info.LogLevel)
                        {
                            case LogLevel.Debug:
                            case LogLevel.Info:
                                UnityEngine.Debug.Log(logStr, info.Context);
                                break;
                            case LogLevel.Warning:
                                UnityEngine.Debug.LogWarning(logStr, info.Context);
                                break;
                            case LogLevel.Error:
                            case LogLevel.Fatal:
                                UnityEngine.Debug.LogError(logStr, info.Context);
                                break;
                            case LogLevel.Off:
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

                    LogStrategyContext strategy = new LogStrategyContext
                    {
                        LogLevel = strategyConfig.LogLevel,
                        LogStrategy = strategyConfig.LogStrategy,
                        LogArgument = strategyConfig.LogArgument,
                        LogDateTime = strategyConfig.LogDateTime,
                        LogClassInfo = strategyConfig.LogCallerInfo,
                        TagCheck = strategyConfig.ExcludeTags.Length > 0 || strategyConfig.LimitedTags.Length > 0,
                        ExcludeTags = strategyConfig.ExcludeTags,
                        LimitedTags = strategyConfig.LimitedTags
                    };

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
