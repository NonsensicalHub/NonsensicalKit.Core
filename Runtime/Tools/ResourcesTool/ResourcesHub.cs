using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NonsensicalKit.Core;

namespace NonsensicalKit.Tools.ResourcesTool
{
    /// <summary>
    /// 聚合多种资源加载方式
    /// </summary>
    public class ResourcesHub : MonoSingleton<ResourcesHub>
    {
        public void Get<T>(string path, Action<T> callback)
        {

        }
        //private IEnumerator GetSprite(string path)
        //{
        //    var handle = Addressables.LoadAssetAsync<Sprite>(path);

        //    yield return handle;

        //    if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
        //    {
        //        m_img_icon.sprite = handle.Result;
        //    }
        //}
    }
}