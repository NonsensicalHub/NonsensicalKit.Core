namespace NonsensicalKit.Core.Save
{
    /// <summary>
    /// 存档数据提供方。每个系统实现自己的采集与恢复逻辑。
    /// </summary>
    public interface ISaveProvider
    {
        /// <summary>
        /// 当前模块在存档中的唯一键。
        /// </summary>
        string SaveKey { get; }

        /// <summary>
        /// 存档版本
        /// </summary>
        int SchemaVersion { get; }
        
        /// <summary>
        /// 采集模块数据并输出为二进制内容。
        /// </summary>
        byte[] CaptureAsBytes();

        /// <summary>
        /// 使用二进制内容恢复模块数据。
        /// </summary>
        void RestoreFromBytes(byte[] bytes);
    }
}
