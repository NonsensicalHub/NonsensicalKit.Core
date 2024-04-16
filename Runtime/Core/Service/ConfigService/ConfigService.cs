using NonsensicalKit.Tools;
using NonsensicalKit.Tools.NetworkTool;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace NonsensicalKit.Core.Service.Config
{
    [ServicePrefab("Services/ConfigService")]
    public class ConfigService : NonsensicalMono, IMonoService
    {
        [Tooltip("每次运行或打包前，自动搜索所有Config并设置")]
        [SerializeField] private bool m_autoMode = true;
        [Tooltip("所有管理的配置文件，可以通过右键-Set All Config来快速设置项目内所有配置文件")]
        [SerializeField] private ConfigObject[] m_configDatas;

        private Dictionary<string, ConfigData> _configs;

        private int _count = 0;

        public bool IsReady { get; set; }

        public Action InitCompleted { get; set; }

        #region Init
        private void Awake()
        {
            _configs = new Dictionary<string, ConfigData>();
            StartCoroutine(OnInitStart());
        }

        protected IEnumerator OnInitStart()
        {
            if (PlatformInfo.IsEditor)
            {
                foreach (var item in m_configDatas)
                {
                    var data = item.GetData();
                    string crtID = data.ConfigID;
                    if (!_configs.ContainsKey(crtID))
                    {
                        _configs[crtID] = data;
                    }
                    else
                    {
                        Debug.LogError("相同类型配置的ID重复:" + crtID);
                        break;
                    }
                }
            }
            else
            {
                //发布出去后读取json文件，而不是直接使用序列化对象
                _count = m_configDatas.Length;
                foreach (var item in m_configDatas)
                {
                    StartCoroutine(StartGet(item));
                }

                while (_count != 0)
                {
                    yield return null;
                }
            }
            IsReady = true;
            InitCompleted?.Invoke();
        }

        #endregion

        #region GetValue
        /// <summary>
        /// 使用类型来获取配置信息，返回匹配的第一个
        /// </summary>
        /// <typeparam name="T">配置类的类型</typeparam>
        /// <param name="t">out值，获取不到时为默认值</param>
        /// <returns>是否获取成功</returns>
        public bool TryGetConfig<T>(out T t) where T : ConfigData
        {
            t = null;
            foreach (var configData in _configs.Values)
            {
                if (configData.GetType() == typeof(T))
                {
                    t = configData as T;
                    break;
                }
            }
            return t != null;
        }

        public T GetConfig<T>() where T : ConfigData
        {
            foreach (var configData in _configs.Values)
            {
                if (configData.GetType() == typeof(T))
                {
                    return configData as T;
                }
            }
            return null;
        }

        /// <summary>
        /// 同时使用类型和ID来获取配置信息，返回匹配的第一个
        /// </summary>
        /// <typeparam name="T">配置类的类型</typeparam>
        /// <param name="ID">想要获取的ID</param>
        /// <param name="t">out值，获取不到时为默认值</param>
        /// <returns>是否获取成功</returns>
        public bool TryGetConfig<T>(string ID, out T t) where T : ConfigData
        {
            t = null;

            if (_configs.ContainsKey(ID))
            {
                t = _configs[ID] as T;
            }

            return t != null;
        }

        public T GetConfig<T>(string ID) where T : ConfigData
        {
            if (_configs.ContainsKey(ID))
            {
                return _configs[ID] as T;
            }

            return null;
        }

        /// <summary>
        /// 使用类型来获取配置信息，返回所有匹配的对象
        /// </summary>
        /// <typeparam name="T">配置类的类型</typeparam>
        /// <param name="t">out值，获取不到时为默认值</param>
        /// <returns>是否获取成功</returns>
        public bool TryGetConfigs<T>(out IList<T> values) where T : ConfigData
        {
            values = new List<T>();
            foreach (var configData in _configs.Values)
            {
                if (configData.GetType() == typeof(T))
                {
                    values.Add(configData as T);
                }
            }
            return values.Count > 0;
        }

        public List<T> GetConfigs<T>() where T : ConfigData
        {
            var values = new List<T>();
            foreach (var configData in _configs.Values)
            {
                if (configData.GetType() == typeof(T))
                {
                    values.Add(configData as T);
                }
            }
            return values;
        }

        /// <summary>
        /// 获取某个配置类的某个字段的值
        /// </summary>
        /// <typeparam name="Config">配置类的类型</typeparam>
        /// <typeparam name="T">字段的类型</typeparam>
        /// <param name="filedName">字段的名称</param>
        /// <param name="t">out值，获取不到时为默认值</param>
        /// <returns>是否获取成功</returns>
        public bool TryGetConfigValue<Config, T>(string filedName, out T t) where Config : ConfigData
        {
            t = default(T);
            if (TryGetConfig(out Config config))
            {
                Type type = typeof(Config);

                var f = type.GetField(filedName);
                if (f != null)
                {
                    var v = f.GetValue(config);
                    if (v.GetType() == typeof(T))
                    {
                        t = (T)v;
                        return true;
                    }
                }
            }
            return false;
        }

        public T GetConfigValue<Config, T>(string filedName) where Config : ConfigData
        {
            if (TryGetConfig(out Config config))
            {
                Type type = typeof(Config);

                var f = type.GetField(filedName);
                if (f != null)
                {
                    var v = f.GetValue(config);
                    if (v.GetType() == typeof(T))
                    {
                        return (T)v;
                    }
                }
            }
            return default;
        }
        #endregion

        #region private method

        private IEnumerator StartGet(ConfigObject obj)
        {
            var data = obj.GetData();
            string path = GetFilePath(data);
            UnityWebRequest unityWebRequest = new UnityWebRequest();
            yield return unityWebRequest.QuickGet(path);

            if (unityWebRequest != null && unityWebRequest.result == UnityWebRequest.Result.Success)
            {
                string str = unityWebRequest.downloadHandler.text;
                SetData(str, data.GetType(), obj);
                string crtID = data.ConfigID;
                _configs[crtID] = obj.GetData();
            }
            else
            {
                Debug.LogError("获取配置文件失败");
            }
            _count--;
        }

        /// <summary>
        /// 赋值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="type"></param>
        /// <param name="configData"></param>
        private void SetData(string str, Type type, ConfigObject configData)
        {
            MethodInfo deserializeMethod = Tools.JsonTool.DESERIALIZE_METHOD.MakeGenericMethod(new Type[] { type });
            object deserializeData = null;
            try
            {
                deserializeData = deserializeMethod.Invoke(null, new object[] { str });
            }
            catch (Exception e)
            {
                Debug.LogError("NonsensicalAppConfig文件反序列化出错" + "\r\n" + e.ToString());
                return;
            }

            configData.BeforeSetData();
            configData.SetData(deserializeData as ConfigData);
            configData.AfterSetData();
        }

        /// <summary>
        /// 通过类型和id获取文件路径
        /// </summary>
        /// <param name="configData"></param>
        /// <returns></returns>
        private string GetFilePath(ConfigData configData)
        {
            string configFilePath = Path.Combine(Application.streamingAssetsPath, "Configs", configData.GetType().ToString() + "_" + configData.ConfigID + ".json");
            return configFilePath;
        }
        #endregion

        #region Editor Method
#if UNITY_EDITOR
        /// <summary>
        /// 搜索项目内所有配置文件并赋值
        /// </summary>
        [ContextMenu("Set All Config")]
        public void FindAndSetAllConfig()
        {
            var v = GetAllInstances<ConfigObject>();
            m_configDatas = v;
            UnityEditor.EditorUtility.SetDirty(gameObject);
        }
        private T[] GetAllInstances<T>() where T : ScriptableObject
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(T).Name);  //FindAssets uses tags check documentation for more info
            T[] a = new T[guids.Length];
            for (int i = 0; i < guids.Length; i++)         //probably could get optimized 
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return a;
        }

        /// <summary>
        /// 用于编辑器环境中的反向读取json文件
        /// </summary>
        [ContextMenu("Load Json")]
        private void LoadJson()
        {
            for (int i = 0; i < m_configDatas.Length; i++)
            {
                var data = m_configDatas[i].GetData();
                string path = GetFilePath(data);
                string str = FileTool.ReadAllText(path);
                SetData(str, data.GetType(), m_configDatas[i]);
            }

            UnityEditor.EditorUtility.SetDirty(gameObject);
        }

        public void OnBeforeBuild()
        {
            if (m_autoMode)
            {
                FindAndSetAllConfig();
            }
            WriteConfigs();
        }

        public void OnBeforePlay()
        {
            if (m_autoMode)
            {
                FindAndSetAllConfig();
            }
        }

        /// <summary>
        /// Build时会调用此方法，将序列化序列化对象写入StreamingAssets文件夹里的json文件
        /// </summary>
        private void WriteConfigs()
        {
            _configs = new Dictionary<string, ConfigData>();
            for (int i = 0; i < m_configDatas.Length; i++)
            {
                var data = m_configDatas[i].GetData();
                string crtID = data.ConfigID;
                if (!_configs.ContainsKey(crtID))
                {
                    try
                    {
                        string json = Tools.JsonTool.SerializeObject(data);
                        FileTool.WriteTxt(GetFilePath(data), json);
                    }
                    catch (Exception)
                    {
                        Debug.Log("配置json文件写入失败");
                    }
                }
                else
                {
                    Debug.LogError("相同类型配置的ID重复:" + crtID);
                    break;
                }
            }
        }
#endif
        #endregion
    }
}
