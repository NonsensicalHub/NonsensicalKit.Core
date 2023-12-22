using NonsensicalKit.Core.Service.Config;
using UnityEngine;

namespace NonsensicalKit.Core.Log.NonsensicalLog
{
    [CreateAssetMenu(fileName = "New NonsensicalLogConfig", menuName = "ScriptableObjects/NonsensicalLogConfig")]
    public class NonsensicalLogConfig : ConfigObject
    {
        public NonsensicalLogConfigData data;

        public override ConfigData GetData()
        {
            return data;
        }

        public override void SetData(ConfigData cd)
        {
            data = cd as NonsensicalLogConfigData;
        }
    }

    [System.Serializable]
    public class NonsensicalLogConfigData:ConfigData
    {
        public LogLevel EditorLogLevel = LogLevel.DEBUG;
        public LogLevel RuntimeLogLevel = LogLevel.OFF;
        public LogStrategy[] EditorLogStrategy = new LogStrategy[] { LogStrategy .Console};
        public LogStrategy[] RuntimeLogStrategy = new LogStrategy[] { };
        public bool EditorLogDateTime = false;
        public bool RuntimeLogDateTime = false;
        public bool EditorLogCallerInfo = false;
        public bool RuntimeLogCallerInfo = false;
    }
}
