using System;
using NonsensicalKit.Core.Log;
using NonsensicalKit.Core.Service;
using NonsensicalKit.Tools;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NonsensicalKit.Core.Setting
{
    public class NonsensicalSetting : ScriptableObject
    {
        private const string SettingName = "NonsensicalSetting";

        [FormerlySerializedAs("_runningLogger")] [SerializeField, TypeQualifiedString(typeof(ILog))]
        private string m_runningLogger;

        public string RunningLogger => m_runningLogger;

        [FormerlySerializedAs("_runningServices")] [SerializeField, TypeQualifiedString(typeof(IService))]
        private string[] m_runningServices;

        public string[] RunningServices => m_runningServices;

        private static NonsensicalSetting _crtSetting;

        public static NonsensicalSetting CurrentSetting
        {
            get
            {
                if (_crtSetting == null)
                {
                    _crtSetting = Resources.Load<NonsensicalSetting>(SettingName);
                    if (_crtSetting == null)
                    {
                        _crtSetting = CreateDefaultSettings();
                    }
                }

                return _crtSetting;
            }
        }

        public static NonsensicalSetting DefaultSetting => CreateDefaultSettings();

        public static NonsensicalSetting LoadSetting()
        {
            var setting = Resources.Load<NonsensicalSetting>(SettingName);
            if (setting == null)
            {
                if (PlatformInfo.IsEditor)
                {
#if UNITY_EDITOR
                    setting = CreateDefaultSettings();
                    FileTool.EnsureDir(Application.dataPath + "/Resources");
                    AssetDatabase.CreateAsset(setting, $"Assets/Resources/{SettingName}.asset");
                    Debug.Log($"自动创建NonsensicalSetting,路径为：Assets/Resources/{SettingName}.asset", setting);
#endif
                }
                else
                {
                    Debug.LogError($"未检测到配置文件:{SettingName},请使用\"NonsensicalKit/Init NonsensicalSetting|初始化配置文件\"生成配置文件");
                }
            }

            return setting;
        }


        private static NonsensicalSetting CreateDefaultSettings()
        {
            var defaultAsset = CreateInstance<NonsensicalSetting>();

            defaultAsset.m_runningLogger = nameof(DefaultLog);

            //var services = ReflectionTool.GetConcreteTypesString<IService>().ToArray();
            //defaultAsset._runningServices = services;
            defaultAsset.m_runningServices = Array.Empty<string>();

            return defaultAsset;
        }
    }
}
