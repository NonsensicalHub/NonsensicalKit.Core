using System;
using System.Collections;
using System.Collections.Generic;
using NonsensicalKit.Tools;
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
                    GameObject instanceGameObject = new GameObject("Nonsensical Instance");
                    DontDestroyOnLoad(instanceGameObject);
                    _instance = instanceGameObject.AddComponent<NonsensicalInstance>();
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

        private Queue<string> _messages;

        public List<Tweener> Tweenners;

        private void Awake()
        {
            _messages = new Queue<string>();
            Tweenners = new List<Tweener>();

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
                    if (keyValuePairs.TryGetValue(intValue, out var pair))
                    {
                        errorCount++;
                        Debug.Log($"枚举{item.Name}与枚举{pair}存在相同的值索引{intValue}");
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
            while (_messages.Count > 0)
            {
                Debug.Log(_messages.Dequeue());
            }

            for (int i = 0; i < Tweenners.Count; i++)
            {
                if (Tweenners[i].IsOver)
                {
                    Tweenners.RemoveAt(i);
                    i--;
                }
                else if (Tweenners[i].DoIt(Time.deltaTime))
                {
                    Tweenners.RemoveAt(i);
                    i--;
                }
            }
        }

        public void DelayDoIt(float delayTime, Action action)
        {
            StartCoroutine(DelayDoItCoroutine(delayTime, action));
        }

        public void AddComponent<T>() where T : MonoBehaviour
        {
            gameObject.AddComponent<T>();
        }

        public void Log(string msg)
        {
            _messages.Enqueue(msg);
        }

        private IEnumerator DelayDoItCoroutine(float delayTime, Action action)
        {
            yield return new WaitForSeconds(delayTime);

            action?.Invoke();
        }
    }
}
