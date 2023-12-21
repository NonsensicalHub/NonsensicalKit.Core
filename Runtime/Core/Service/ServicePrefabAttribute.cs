using System;

namespace NonsensicalKit.Editor.Service
{
    /// <summary>
    /// 配置服务类预制体路径
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ServicePrefabAttribute : Attribute
    {
        public string PrefabPath => _prefabPath;
        private string _prefabPath;

        public ServicePrefabAttribute(string prefabPath)
        {
            _prefabPath = prefabPath;
        }
    }
}
