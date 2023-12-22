using System;
using UnityEngine;

namespace NonsensicalKit.Core.Service.Config
{
    /// <summary>
    /// Config对象类应继承此类
    /// </summary>
    public abstract class ConfigObject : ScriptableObject
    {
        public abstract ConfigData GetData();

        public virtual void BeforeSetData()
        {

        }

        public abstract void SetData(ConfigData cd);

        public virtual void AfterSetData()
        {

        }

        protected bool CheckType<T>(ConfigData cdb) where T : ConfigData
        {
            return cdb.GetType() == typeof(T);
        }
    }

    [System.Serializable]
    public abstract class ConfigData
    {
        public string ConfigID = "ID" + Guid.NewGuid().ToString().Substring(0, 8);
    }
}
