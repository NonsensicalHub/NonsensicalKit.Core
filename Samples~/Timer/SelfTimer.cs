using BJTimer;
using NonsensicalKit.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfTimer : MonoBehaviour
{
    [SerializeField] private bool _eableTimer;
    private TimerSystem timeSys;
    private void Awake()
    {
      
        if (false == _eableTimer) return;

        timeSys = TimerSystem.Instance;
        timeSys.Init();
        timeSys.StartTimer();

        IOCC.Set<TimerSystem>("Timer", timeSys);
    }

    private void OnDestroy()
    {
        timeSys.ResetTimer();
    }
}
