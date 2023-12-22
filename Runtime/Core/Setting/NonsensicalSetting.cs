using NonsensicalKit.Core.Log;
using NonsensicalKit.Core.Service;
using NonsensicalKit.Tools;
using UnityEngine;

namespace NonsensicalKit.Core.Setting
{
    public class NonsensicalSetting : ScriptableObject
    {
        private const string SETTING_NAME = "NonsensicalSetting";

        [SerializeField, TypeQualifiedString(typeof(ILog))]
        protected string _runningLogger;
        public string RunningLogger { get { return _runningLogger; } }

        [SerializeField, TypeQualifiedString(typeof(IService))]
        protected string[] _runningServices;
        public string[] RunningServices { get { return _runningServices; } }

        private static NonsensicalSetting _crtSetting;
        public static NonsensicalSetting CurrentSetting
        {
            get
            {
                if (_crtSetting == null)
                {
                    _crtSetting = Resources.Load<NonsensicalSetting>(SETTING_NAME);
                    if (_crtSetting == null)
                    {
                        _crtSetting = CreateDefaultSettings();
                    }
                }
                return _crtSetting;
            }
        }

        public static NonsensicalSetting DefaultSetting
        {
            get
            {
                return CreateDefaultSettings();
            }
        }

        public static NonsensicalSetting LoadSetting()
        {
            var setting = Resources.Load<NonsensicalSetting>(SETTING_NAME);
            if (setting == null)
            {
                Debug.LogError($"未检测到配置文件:{SETTING_NAME},请使用\"NonsensicalKit/Init NonsensicalSetting|初始化配置文件\"生成配置文件");
            }
            return setting;
        }


        private static NonsensicalSetting CreateDefaultSettings()
        {
            var defaultAsset = CreateInstance<NonsensicalSetting>();

            defaultAsset._runningLogger =nameof(DefaultLog);

            var services = ReflectionTool.GetConcreteTypesString<IService>().ToArray();
            defaultAsset._runningServices = services;

            return defaultAsset;
        }
    }
}
