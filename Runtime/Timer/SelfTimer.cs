using System;
using NonsensicalKit.Core.Service;
using UnityEngine;

namespace NonsensicalKit.Core.Timer
{
    /// <summary>
    /// 将 <see cref="TimerSystem"/> 注册为可运行的 Mono 服务，并通过 IOCC 暴露给业务侧。
    /// </summary>
    [ServicePrefab("Services/Timer")]
    public class SelfTimer : MonoBehaviour, IMonoService
    {
        private TimerSystem _timeSys;

        private void Awake()
        {
            IsReady = false;

            _timeSys = TimerSystem.Instance;
            IOCC.Set<TimerSystem>("Timer", _timeSys);

            IsReady = true;
            InitCompleted?.Invoke();
        }

        private void OnDestroy()
        {
            _timeSys?.ResetTimer();
        }

        public bool IsReady { get; private set; }
        public Action InitCompleted { get; set; }
    }
}