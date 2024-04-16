using NonsensicalKit.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonsensicalKit.Core
{
    /// <summary>
    /// 场景中的实例对象，用于执行协程、线程Debug
    /// </summary>
    public class NonsensicalInstance : MonoBehaviour
    {
        public static NonsensicalInstance Instance
        {
            get
            {
                if (_instance == null)
                {
                    Application.quitting += OnQuitting;
                    GameObject instanceGameobject = new GameObject("Nonsensical Instance");
                    DontDestroyOnLoad(instanceGameobject);
                    _instance = instanceGameobject.AddComponent<NonsensicalInstance>();
                }
                return _instance;
            }
        }

        public static bool ApplicationIsQuitting = false;

        private static NonsensicalInstance _instance;

        private static void OnQuitting()
        {
            ApplicationIsQuitting = true;
        }

        public Queue<string> Messages;

        public List<Tweenner> Tweenners;

        private void Awake()
        {
            Messages = new Queue<string>();
            Tweenners = new List<Tweenner>();

#if UNITY_EDITOR
            int errorCount = 0;
            Dictionary<int, string> keyValuePairs = new Dictionary<int, string>();
            var v = ReflectionTool.GetEnumByAttribute<AggregatorEnumAttribute>();
            foreach (var item in v)
            {
                Array values = Enum.GetValues(item);
                foreach (var value in values)
                {
                    var intValue = (int)value;
                    if (keyValuePairs.ContainsKey(intValue))
                    {
                        errorCount++;
                        Debug.Log($"枚举{item.Name}与枚举{keyValuePairs[intValue]}存在相同的值索引{intValue}");
                    }
                    else
                    {
                        keyValuePairs.Add(intValue, item.Name);
                    }
                }
            }
            if (errorCount > 0)
            {
                Debug.Log($"枚举值检测到重复,共发现{errorCount}个重复");
            }
#endif
        }

        private void Update()
        {
            while (Messages.Count > 0)
            {
                Debug.Log(Messages.Dequeue());
            }

            for (int i = 0; i < Tweenners.Count; i++)
            {
                if (Tweenners[i].IsOver)
                {
                    Tweenners.RemoveAt(i);
                    i--;
                }
                else if (Tweenners[i].DoIt(Time.deltaTime) == true)
                {
                    Tweenners.RemoveAt(i);
                    i--;
                }
            }
        }

        public void DelayDoIt(float _delayTime, Action _action)
        {
            StartCoroutine(DelayDoItCoroutine(_delayTime, _action));
        }

        public void AddComponent<T>() where T : MonoBehaviour
        {
            gameObject.AddComponent<T>();
        }

        private IEnumerator DelayDoItCoroutine(float _delayTime, Action _action)
        {
            yield return new WaitForSeconds(_delayTime);

            _action?.Invoke();
        }
    }
}
