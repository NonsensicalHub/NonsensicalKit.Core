using NonsensicalKit.Core.Service;
using UnityEngine;

namespace NonsensicalKit.Core.Save
{
    /// <summary>
    /// 方便场景中自动发现的基类。
    /// </summary>
    public abstract class SaveProviderBehaviour : NonsensicalMono, ISaveProvider
    {
        [SerializeField] private string m_saveKey="SaveKey";
        [SerializeField] private int m_schemaVersion=1;

        public string SaveKey => m_saveKey;

        public int SchemaVersion => m_schemaVersion;

        protected virtual void Awake()
        {
            ServiceCore.Get<SaveService>().RegisterProvider(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ServiceCore.Get<SaveService>().UnregisterProvider(this);
        }

        /// <summary>
        /// 采集当前组件状态并返回二进制数据。
        /// </summary>
        public abstract byte[] CaptureAsBytes();

        /// <summary>
        /// 从二进制数据恢复组件状态。
        /// </summary>
        public abstract void RestoreFromBytes(byte[] bytes);
    }
}
