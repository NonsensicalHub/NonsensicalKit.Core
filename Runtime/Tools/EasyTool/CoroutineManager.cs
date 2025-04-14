using System.Collections;
using System.Collections.Generic;
using NonsensicalKit.Core;
using UnityEngine;

namespace NonsensicalKit
{
    /// <summary>
    /// 以Transform为键管理协程
    /// </summary>
    public static class CoroutineManager
    {
        private static readonly Dictionary<Transform, CoroutineInfo> Coroutines = new Dictionary<Transform, CoroutineInfo>();

        public static bool CheckPlaying(Transform target)
        {
            if (Coroutines.TryGetValue(target, out var coroutine))
            {
                return coroutine.IsPlaying;
            }
            else
            {
                return false;
            }
        }

        public static void PlayCoroutine(Transform target, IEnumerator coroutine)
        {
            if (Coroutines.ContainsKey(target) == false)
            {
                CoroutineInfo ci = new CoroutineInfo();
                Coroutines.Add(target, ci);
            }

            var v = NonsensicalInstance.Instance.StartCoroutine(RunIt(Coroutines[target], coroutine));
            Coroutines[target].Coroutine = v;
        }

        public static void Stop(Transform target)
        {
            if (Coroutines.ContainsKey(target) && Coroutines[target].IsPlaying)
            {
                Coroutines[target].IsPlaying = false;
                NonsensicalInstance.Instance.StopCoroutine(Coroutines[target].Coroutine);
            }
        }

        private static IEnumerator RunIt(CoroutineInfo ci, IEnumerator coroutine)
        {
            ci.IsPlaying = true;

            yield return coroutine;

            ci.IsPlaying = false;
        }
    }

    public class CoroutineInfo
    {
        public Coroutine Coroutine { get; set; }
        public bool IsPlaying = false;
    }
}
