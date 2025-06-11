using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NaughtyAttributes;
using Newtonsoft.Json.Linq;
using NonsensicalKit.Tools;
using NonsensicalKit.Tools.NetworkTool;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
#endif

namespace NonsensicalKit.Core.Service.Config
{
    [ServicePrefab("Services/ConfigService")]
    public class ConfigService : NonsensicalMono, IMonoService
    {
        [Tooltip("每次运行或打包前，自动搜索所有Config并设置")] [SerializeField]
        private bool m_autoMode = true;

        [Tooltip("生成和读取json文件")] [SerializeField]
        private bool m_jsonMode = true;
        
        [Tooltip("将所有配置json合并到一个文件，减少文件请求次数")] [SerializeField]
        private bool m_singleJsonMode = true;

        [Tooltip("所有管理的配置文件，可以通过右键-Set All Config来快速设置项目内所有配置文件")] [SerializeField]
        private ConfigObject[] m_configDatas = Array.Empty<ConfigObject>();

        private Dictionary<string, ConfigData> _configs;

        private int _count;

        public bool IsReady { get; private set; }

        public Action InitCompleted { get; set; }

        #region Init

        private void Awake()
        {
            _configs = new Dictionary<string, ConfigData>();
            StartCoroutine(OnInitStart());
        }

        protected IEnumerator OnInitStart()
        {
            if (PlatformInfo.IsEditor || (!m_jsonMode))
            {
                foreach (var item in m_configDatas)
                {
                    var data = item.GetData();
                    string crtID = data.ConfigID;
                    if (!_configs.TryAdd(crtID, data))
                    {
                        Debug.LogError("相同类型配置的ID重复:" + crtID);
                        break;
                    }
                }
            }
            else
            {
                //发布出去后读取json文件，而不是直接使用序列化对象

                if (m_singleJsonMode)
                {
                    yield return GetSingleConfig();
                }
                else
                {
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
        /// <param name="id">想要获取的ID</param>
        /// <param name="t">out值，获取不到时为默认值</param>
        /// <returns>是否获取成功</returns>
        public bool TryGetConfig<T>(string id, out T t) where T : ConfigData
        {
            t = null;

            if (_configs.TryGetValue(id, out var config))
            {
                t = config as T;
            }

            return t != null;
        }

        public T GetConfig<T>(string id) where T : ConfigData
        {
            if (_configs.TryGetValue(id, out var config))
            {
                return config as T;
            }

            return null;
        }

        /// <summary>
        /// 使用类型来获取配置信息，返回所有匹配的对象
        /// </summary>
        /// <typeparam name="T">配置类的类型</typeparam>
        /// <param name="values">out值，获取不到时为默认值</param>
        /// <returns>是否获取成功</returns>
        public bool TryGetConfigs<T>(out List<T> values) where T : ConfigData
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
        /// <typeparam name="TConfig">配置类的类型</typeparam>
        /// <typeparam name="T">字段的类型</typeparam>
        /// <param name="filedName">字段的名称</param>
        /// <param name="t">out值，获取不到时为默认值</param>
        /// <returns>是否获取成功</returns>
        public bool TryGetConfigValue<TConfig, T>(string filedName, out T t) where TConfig : ConfigData
        {
            t = default(T);
            if (TryGetConfig(out TConfig config))
            {
                Type type = typeof(TConfig);

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

        public T GetConfigValue<TConfig, T>(string filedName) where TConfig : ConfigData
        {
            if (TryGetConfig(out TConfig config))
            {
                Type type = typeof(TConfig);

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

        private IEnumerator GetSingleConfig()
        {
            string configFilePath = Path.Combine(Application.streamingAssetsPath, "Configs.json");
            
            UnityWebRequest unityWebRequest = new UnityWebRequest();
            yield return unityWebRequest.Get(configFilePath);

            if (unityWebRequest.result == UnityWebRequest.Result.Success)
            {
                string str = unityWebRequest.downloadHandler.text;
                
                JArray arr = JArray.Parse(str);

                if (arr.Count != m_configDatas.Length)
                {
                    Debug.LogError("获取的配置文件不匹配：" + configFilePath);
                    yield break;
                }

                for (int i = 0; i < arr.Count; i++)
                {
                    ConfigObject obj = m_configDatas[i];
                    var data= arr[i].ToObject( obj.GetData().GetType()) as ConfigData;
                    
                    obj.BeforeSetData();
                    obj.SetData(data);
                    obj.AfterSetData();
                    string crtID = data.ConfigID;
                    _configs[crtID] = obj.GetData();
                }
            }
            else
            {
                Debug.LogError("获取配置文件失败：" + configFilePath);
            }
        }

        
        private IEnumerator StartGet(ConfigObject obj)
        {
            var data = obj.GetData();
            string path = GetFilePath(data);
            UnityWebRequest unityWebRequest = new UnityWebRequest();
            yield return unityWebRequest.Get(path);

            if (unityWebRequest.result == UnityWebRequest.Result.Success)
            {
                string str = unityWebRequest.downloadHandler.text;
                SetData(str, data.GetType(), obj);
                string crtID = data.ConfigID;
                _configs[crtID] = obj.GetData();
            }
            else
            {
                Debug.LogError("获取配置文件失败：" + path);
            }

            _count--;
        }

        /// <summary>
        /// 通过类型和id获取文件路径
        /// </summary>
        /// <param name="configData"></param>
        /// <returns></returns>
        private string GetFilePath(ConfigData configData)
        {
            string configFilePath = Path.Combine(Application.streamingAssetsPath, "Configs",
                configData.GetType() + "_" + configData.ConfigID + ".json");
            return configFilePath;
        }

        /// <summary>
        /// 赋值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="type"></param>
        /// <param name="configData"></param>
        private void SetData(string str, Type type, ConfigObject configData)
        {
            MethodInfo deserializeMethod = JsonTool.DESERIALIZE_METHOD.MakeGenericMethod(new[] { type });
            object deserializeData;
            try
            {
                deserializeData = deserializeMethod.Invoke(null, new object[] { str });
            }
            catch (Exception e)
            {
                Debug.LogError("NonsensicalAppConfig文件反序列化出错" + "\r\n" + e);
                return;
            }

            configData.BeforeSetData();
            configData.SetData(deserializeData as ConfigData);
            configData.AfterSetData();
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
            EditorUtility.SetDirty(gameObject);
        }

        private T[] GetAllInstances<T>() where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name); //FindAssets uses tags check documentation for more info
            T[] a = new T[guids.Length];
            for (int i = 0; i < guids.Length; i++) //probably could get optimized 
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return a;
        }

        /// <summary>
        /// 用于编辑器环境中的反向读取json文件
        /// </summary>
        [ContextMenu("Load Json")]
        private void LoadJson()
        {
            if (m_jsonMode)
            {
                if (m_configDatas == null)
                {
                    Debug.LogError("尚未进行配置");
                    return;
                }

                if (m_singleJsonMode)
                {
                    
                    string configFilePath = Path.Combine(Application.streamingAssetsPath, "Configs.json");

                    string str = FileTool.ReadAllText(configFilePath);
                    
                    JArray arr = JArray.Parse(str);

                    if (arr.Count != m_configDatas.Length)
                    {
                        Debug.LogError("获取的配置文件不匹配：" + configFilePath);
                        return;
                    }

                    for (int i = 0; i < arr.Count; i++)
                    {
                        ConfigObject obj = m_configDatas[i];
                        var data= arr[i].ToObject( obj.GetData().GetType()) as ConfigData;
                        obj.BeforeSetData();
                        obj.SetData(data);
                        obj.AfterSetData();
                        EditorUtility.SetDirty(m_configDatas[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < m_configDatas.Length; i++)
                    {
                        var data = m_configDatas[i].GetData();
                        string path = GetFilePath(data);
                        string str = FileTool.ReadAllText(path);
                        SetData(str, data.GetType(), m_configDatas[i]);
                        EditorUtility.SetDirty(m_configDatas[i]);
                    }
                }
            }
        }

        /// <summary>
        /// 打包前调用
        /// </summary>
        [ContextMenu("Create Json")]
        public void OnBeforeBuild()
        {
            if (m_autoMode)
            {
                FindAndSetAllConfig();
            }

            if (m_jsonMode)
            {
                WriteConfigs();
            }
        }

        /// <summary>
        /// 编辑器内运行前调用
        /// </summary>
        public void OnBeforePlay()
        {
            if (m_autoMode)
            {
                FindAndSetAllConfig();
            }
        }

        /// <summary>
        /// Build时会自动调用此方法，将序列化序列化对象写入StreamingAssets文件夹里的json文件
        /// </summary>
        private void WriteConfigs()
        {
            if (m_singleJsonMode)
            {
                JArray arr = new JArray();
                
                string configFilePath = Path.Combine(Application.streamingAssetsPath, "Configs.json");
                foreach (var t in m_configDatas)
                {
                    arr.Add(JObject.FromObject(t.GetData()));
                }
                
                FileTool.WriteTxt(configFilePath, arr.ToString());
            }
            else
            {
                var configs = new HashSet<string>();
                foreach (var t in m_configDatas)
                {
                    var data = t.GetData();
                    string crtID = data.ConfigID;
                    if (configs.Add(crtID))
                    {
                        try
                        {
                            string json = JsonTool.SerializeObject(data);
                            FileTool.WriteTxt(GetFilePath(data), json);
                        }
                        catch (Exception e)
                        {
                            throw new BuildFailedException($"配置{data.ConfigID}json文件写入{GetFilePath(data)}失败\r\n{e.Message}");
                        }
                    }
                    else
                    {
                        throw new BuildFailedException($"相同类型配置的ID重复: {crtID}");
                    }
                }
            }
        }
#endif

        #endregion
    }
}
