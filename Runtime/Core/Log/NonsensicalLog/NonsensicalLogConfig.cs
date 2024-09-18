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
    public class NonsensicalLogConfigData : ConfigData
    {
        public NonsensicalLogStrategyConfig[] Strategys;
    }

    [System.Serializable]
    public class NonsensicalLogStrategyConfig
    {
        public bool WorkInEditor;
        public bool WorkInRuntime;

        public LogLevel LogLevel = LogLevel.DEBUG;
        public string[] ExcludeTags;
        public string[] LimitedTags;
        public LogPathway LogStrategy = LogPathway.Console;
        public string LogArgument;
        public bool LogDateTime = false;
        public bool LogCallerInfo = false;
    }
}
