using System;
using NonsensicalKit.Core.Service.Config;
using UnityEngine;
using UnityEngine.Serialization;

namespace NonsensicalKit.Core.Log.NonsensicalLog
{
    [CreateAssetMenu(fileName = "New NonsensicalLogConfig", menuName = "ScriptableObjects/NonsensicalLogConfig")]
    public class NonsensicalLogConfig : ConfigObject
    {
        [FormerlySerializedAs("data")] public NonsensicalLogConfigData m_Data;

        public override ConfigData GetData()
        {
            return m_Data;
        }

        public override void SetData(ConfigData cd)
        {
            m_Data = cd as NonsensicalLogConfigData;
        }
    }

    [Serializable]
    public class NonsensicalLogConfigData : ConfigData
    {
        [FormerlySerializedAs("Strategys")] public NonsensicalLogStrategyConfig[] m_Strategies;
    }

    [Serializable]
    public class NonsensicalLogStrategyConfig
    {
        public bool WorkInEditor;
        public bool WorkInRuntime;

        public LogLevel LogLevel = LogLevel.Debug;
        public string[] ExcludeTags;
        public string[] LimitedTags;
        public LogPathway LogStrategy = LogPathway.Console;
        public string LogArgument;
        public bool LogDateTime = false;
        public bool LogCallerInfo = false;
    }
}
