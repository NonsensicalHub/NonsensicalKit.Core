using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using NonsensicalKit.Core.Log;
using NonsensicalKit.Core.Service;
using NonsensicalKit.Tools;
using UnityEngine;

namespace NonsensicalKit.Core.Save
{
    /// <summary>
    /// 存档管理器：聚合 Provider，执行统一的保存与读取流程。
    /// </summary>
    public sealed class SaveService : IClassService
    {
        private const int MaxModuleCount = 1024;
        private const int MaxModulePayloadBytes = 8 * 1024 * 1024;
        private const long MaxTotalPayloadBytes = 64L * 1024 * 1024;

        public bool IsReady => true;
        public Action InitCompleted { get; set; }

        private readonly List<ISaveProvider> _providers = new();

        /// <summary>
        /// 注册存档提供方。
        /// </summary>
        public bool RegisterProvider(ISaveProvider provider)
        {
            if (provider == null || string.IsNullOrWhiteSpace(provider.SaveKey))
            {
                return false;
            }

            foreach (var item in _providers)
            {
                if (string.Equals(item.SaveKey, provider.SaveKey, StringComparison.Ordinal))
                {
                    LogCore.Warning($"重复的存档键：{provider.SaveKey}");
                    return false;
                }
            }

            _providers.Add(provider);
            return true;
        }

        /// <summary>
        /// 反注册存档提供方。
        /// </summary>
        public void UnregisterProvider(ISaveProvider provider)
        {
            if (provider == null)
            {
                return;
            }

            _providers.Remove(provider);
        }

        /// <summary>
        /// 从已注册的 Provider 采集当前存档快照。
        /// </summary>
        public SaveGameData Capture(string slotId)
        {
            var data = new SaveGameData
            {
                SlotId = string.IsNullOrWhiteSpace(slotId) ? "default" : slotId,
                TimestampUtcSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Modules = new List<SaveModuleRecord>(_providers.Count),
            };

            foreach (var provider in _providers)
            {
                try
                {
                    var payload = provider.CaptureAsBytes();
                    data.Modules.Add(new SaveModuleRecord
                    {
                        Key = provider.SaveKey,
                        SchemaVersion = provider.SchemaVersion,
                        Payload = payload ?? Array.Empty<byte>(),
                    });
                }
                catch (Exception e)
                {
                    LogCore.Warning($"采集存档失败：{provider.SaveKey}，{e.Message}");
                }
            }

            LogCore.Info($"存档完毕，共有：{_providers.Count}个模块");

            return data;
        }

        /// <summary>
        /// 保存指定槽位。
        /// </summary>
        public void Save(string slotId)
        {
            var snapshot = Capture(slotId);
            var raw = Serialize(snapshot);
            Write(snapshot.SlotId, raw);
        }

        /// <summary>
        /// 异步保存指定槽位，避免阻塞主线程。
        /// </summary>
        public async UniTask SaveAsync(string slotId)
        {
            var snapshot = Capture(slotId);
            var raw = Serialize(snapshot);
            await WriteAsync(snapshot.SlotId, raw);
        }

        /// <summary>
        /// 尝试读取并恢复指定槽位。
        /// </summary>
        public bool Load(string slotId)
        {
            SaveGameData data = null;
            if (Exists(slotId) == false)
            {
                return false;
            }

            try
            {
                var raw = Read(slotId);
                data = Deserialize(raw);
                if (data == null)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                LogCore.Warning($"读取存档失败：{slotId}，{e.Message}");
                return false;
            }

            var lookup = new Dictionary<string, byte[]>(StringComparer.Ordinal);
            if (data.Modules != null)
            {
                foreach (var module in data.Modules)
                {
                    if (module == null || string.IsNullOrWhiteSpace(module.Key))
                    {
                        continue;
                    }

                    lookup[module.Key] = module.Payload ?? Array.Empty<byte>();
                }
            }

            foreach (var provider in _providers)
            {
                if (lookup.TryGetValue(provider.SaveKey, out var payload))
                {
                    try
                    {
                        provider.RestoreFromBytes(payload);
                    }
                    catch (Exception e)
                    {
                        LogCore.Warning($"恢复存档失败：{provider.SaveKey}，{e.Message}");
                    }
                }
                else
                {
                    LogCore.Warning($"未找到对应存档模块：{provider.SaveKey}");
                }
            }

            return true;
        }

        /// <summary>
        /// 异步尝试读取并恢复指定槽位，避免阻塞主线程。
        /// </summary>
        public async UniTask<bool> LoadAsync(string slotId)
        {
            SaveGameData data = null;
            if (Exists(slotId) == false)
            {
                return false;
            }

            try
            {
                var raw = await ReadAsync(slotId);
                data = Deserialize(raw);
                if (data == null)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                LogCore.Warning($"读取存档失败：{slotId}，{e.Message}");
                return false;
            }

            var lookup = new Dictionary<string, byte[]>(StringComparer.Ordinal);
            if (data.Modules != null)
            {
                foreach (var module in data.Modules)
                {
                    if (module == null || string.IsNullOrWhiteSpace(module.Key))
                    {
                        continue;
                    }

                    lookup[module.Key] = module.Payload ?? Array.Empty<byte>();
                }
            }

            foreach (var provider in _providers)
            {
                if (lookup.TryGetValue(provider.SaveKey, out var payload))
                {
                    try
                    {
                        provider.RestoreFromBytes(payload);
                    }
                    catch (Exception e)
                    {
                        LogCore.Warning($"恢复存档失败：{provider.SaveKey}，{e.Message}");
                    }
                }
                else
                {
                    LogCore.Warning($"未找到对应存档模块：{provider.SaveKey}");
                }
            }

            return true;
        }

        /// <summary>
        /// 将存档数据写为稳定结构的二进制数据。
        /// </summary>
        private byte[] Serialize(SaveGameData data)
        {
            if (data == null)
            {
                return Array.Empty<byte>();
            }

            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            writer.Write(data.SlotId ?? string.Empty);
            writer.Write(data.TimestampUtcSeconds);

            var modules = data.Modules ?? new List<SaveModuleRecord>();
            writer.Write(modules.Count);
            foreach (var module in modules)
            {
                writer.Write(module?.Key ?? string.Empty);
                writer.Write(module?.SchemaVersion ?? 1);
                var payload = module?.Payload ?? Array.Empty<byte>();
                writer.Write(payload.Length);
                writer.Write(payload);
            }

            writer.Flush();
            return stream.ToArray();
        }

        /// <summary>
        /// 从二进制数据读取存档数据。
        /// </summary>
        private SaveGameData Deserialize(byte[] raw)
        {
            if (raw == null || raw.Length == 0)
            {
                return null;
            }

            using var stream = new MemoryStream(raw);
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);

            var data = new SaveGameData
            {
                SlotId = reader.ReadString(),
                TimestampUtcSeconds = reader.ReadInt64(),
            };

            var moduleCount = reader.ReadInt32();
            if (moduleCount < 0)
            {
                throw new InvalidDataException($"无效的模块数量：{moduleCount}");
            }
            if (moduleCount > MaxModuleCount)
            {
                throw new InvalidDataException($"模块数量超过上限：{moduleCount}/{MaxModuleCount}");
            }

            data.Modules = new List<SaveModuleRecord>(moduleCount);
            long totalPayloadBytes = 0;
            for (var i = 0; i < moduleCount; i++)
            {
                var key = reader.ReadString();
                var version = reader.ReadInt32();
                var length = reader.ReadInt32();
                if (length < 0)
                {
                    throw new InvalidDataException($"无效的负载长度：{length}");
                }
                if (length > MaxModulePayloadBytes)
                {
                    throw new InvalidDataException($"模块负载超过上限：{length}/{MaxModulePayloadBytes}");
                }
                if (length > stream.Length - stream.Position)
                {
                    throw new EndOfStreamException($"读取负载长度不足，期望：{length}，剩余：{stream.Length - stream.Position}");
                }

                var payload = length > 0 ? reader.ReadBytes(length) : Array.Empty<byte>();
                if (payload.Length != length)
                {
                    throw new EndOfStreamException($"读取负载长度不足，期望：{length}，实际：{payload.Length}");
                }

                totalPayloadBytes += length;
                if (totalPayloadBytes > MaxTotalPayloadBytes)
                {
                    throw new InvalidDataException($"存档总负载超过上限：{totalPayloadBytes}/{MaxTotalPayloadBytes}");
                }

                data.Modules.Add(new SaveModuleRecord
                {
                    Key = key,
                    SchemaVersion = version,
                    Payload = payload,
                });
            }

            return data;
        }

        /// <summary>
        /// 生成指定槽位的完整文件路径。
        /// </summary>
        private string BuildSavePath(string slotId)
        {
            var safeSlot = string.IsNullOrWhiteSpace(slotId) ? "default" : slotId.Trim();
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                safeSlot = safeSlot.Replace(invalidChar, '_');
            }

            return Path.Combine(Application.persistentDataPath, safeSlot);
        }

        /// <summary>
        /// 判断指定槽位文件是否存在。
        /// </summary>
        private bool Exists(string slotId)
        {
            return File.Exists(BuildSavePath(slotId));
        }

        /// <summary>
        /// 写入指定槽位文件的二进制内容。
        /// </summary>
        private void Write(string slotId, byte[] raw)
        {
            var path = BuildSavePath(slotId);
            FileTool.EnsureFileDir(path);
            File.WriteAllBytes(path, raw ?? Array.Empty<byte>());
        }

        /// <summary>
        /// 异步写入指定槽位文件的二进制内容。
        /// </summary>
        private UniTask WriteAsync(string slotId, byte[] raw)
        {
            return UniTask.RunOnThreadPool(() =>
            {
                var path = BuildSavePath(slotId);
                FileTool.EnsureFileDir(path);
                File.WriteAllBytes(path, raw ?? Array.Empty<byte>());
            });
        }

        /// <summary>
        /// 读取指定槽位文件的二进制内容。
        /// </summary>
        private byte[] Read(string slotId)
        {
            var path = BuildSavePath(slotId);
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        /// <summary>
        /// 异步读取指定槽位文件的二进制内容。
        /// </summary>
        private UniTask<byte[]> ReadAsync(string slotId)
        {
            return UniTask.RunOnThreadPool(() =>
            {
                var path = BuildSavePath(slotId);
                return File.Exists(path) ? File.ReadAllBytes(path) : null;
            });
        }
    }
}
