using System;
using System.Collections.Generic;

namespace NonsensicalKit.Core.Save
{
    /// <summary>
    /// 单个模块的存档记录。
    /// </summary>
    [Serializable]
    public sealed class SaveModuleRecord
    {
        
        /// <summary>
        /// 模块唯一键。
        /// </summary>
        public string Key;

        /// <summary>
        /// 版本号，用于升级兼容。
        /// </summary>
        public int SchemaVersion = 1;
        
        /// <summary>
        /// 模块序列化后的二进制负载。
        /// </summary>
        public byte[] Payload;
    }

    /// <summary>
    /// 完整游戏存档快照。
    /// </summary>
    [Serializable]
    public sealed class SaveGameData
    {
        /// <summary>
        /// 存档槽位 ID。
        /// </summary>
        public string SlotId = "default";

        /// <summary>
        /// UTC 秒级时间戳。
        /// </summary>
        public long TimestampUtcSeconds;

        /// <summary>
        /// 所有模块的存档记录。
        /// </summary>
        public List<SaveModuleRecord> Modules = new();
    }
}
