using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NonsensicalKit.Tools.ObjectPool
{
    [Serializable]
    public class SerializableGameObjectPool
    {
        [SerializeField] private GameObject m_prefab; //预制体
        [SerializeField] private Transform m_birthPoint; //出生点
        [SerializeField] private List<GameObject> m_using = new List<GameObject>(); //使用中对象
        [SerializeField] private List<GameObject> m_storage = new List<GameObject>(); //存储对象

        public GameObject Prefab
        {
            get => m_prefab;
            set => m_prefab = value;
        }

        private List<GameObject> _cache;
        private int _catchIndex;

        public void Init(GameObject prefab, Transform birthPoint)
        {
            m_prefab = prefab;
            m_birthPoint = birthPoint;
        }

        /// <summary>
        /// 取出对象
        /// </summary>
        /// <returns></returns>
        public GameObject New()
        {
            GameObject newGo;
            if ((_cache != null) && (_catchIndex < _cache.Count))
            {
                newGo = _cache[_catchIndex];
                _catchIndex++;
                m_using.Add(newGo);
            }
            else if (m_storage.Count > 0)
            {
                newGo = m_storage[0];
                m_storage.RemoveAt(0);
                newGo.SetActive(true);
                m_using.Add(newGo);
            }
            else
            {
                newGo = Object.Instantiate(m_prefab, m_birthPoint);
                newGo.SetActive(true);
                m_using.Add(newGo);
            }

            return newGo;
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        /// <param name="go"></param>
        public void Store(GameObject go)
        {
            if (m_using.Contains(go) && (m_storage.Contains(go) == false))
            {
                m_using.Remove(go);
                m_storage.Add(go);
                go.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 和Flush配套使用
        /// 缓存当前使用的对象，在取出对象时优先使用缓存的对象，在需要全部回收后重新配置时使用，避免回收后马上取用造成的不必要性能消耗
        /// </summary>
        public void Cache()
        {
            _cache = m_using;
            _catchIndex = 0;
            m_using = new List<GameObject>();
        }

        /// <summary>
        /// 和Catch配套使用，回收未取用的缓存
        /// </summary>
        public void Flush()
        {
            if (_cache == null)
            {
                return;
            }

            for (; _catchIndex < _cache.Count; _catchIndex++)
            {
                _cache[_catchIndex].SetActive(false);
                m_storage.Add(_cache[_catchIndex]);
            }

            _cache.Clear();
            _cache = null;
        }

        /// <summary>
        /// 把使用中的对象全部回收
        /// </summary>
        public void Clear()
        {
            foreach (var item in m_using)
            {
                item.SetActive(false);
                m_storage.Add(item);
            }

            m_using.Clear();
        }

        /// <summary>
        /// 销毁所有对象
        /// </summary>
        public void Clean()
        {
            foreach (var item in m_using)
            {
                item.Destroy();
            }

            foreach (var item in m_storage)
            {
                item.Destroy();
            }

            m_using.Clear();
            m_storage.Clear();
            _cache = null;
        }
    }
}
