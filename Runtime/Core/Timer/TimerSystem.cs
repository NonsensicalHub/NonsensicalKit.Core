using System;
using UnityEngine;

namespace NonsensicalKit.Core.Timer
{
    public class TimerSystem : MonoBehaviour
    {
        private static TimerSystem instance;

        public static TimerSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject("TimerSystem").AddComponent<TimerSystem>();
                }

                return instance;
            }
        }

        private NonsensicalTimer _nonsensicalTimer;
        private bool start = false;

        private void Awake()
        {
            _nonsensicalTimer = new NonsensicalTimer();
            _nonsensicalTimer.GetNow = GetNow;
            _nonsensicalTimer.SetLog((str, level) =>
            {
                switch (level)
                {
                    case NonsensicalTimer.LogLevel.Info:
                        Debug.Log(str);
                        break;
                    case NonsensicalTimer.LogLevel.Log:
                        Debug.Log(str);
                        break;
                    case NonsensicalTimer.LogLevel.Warning:
                        Debug.LogWarning(str);
                        break;
                    case NonsensicalTimer.LogLevel.Error:
                        Debug.LogError(str);
                        break;
                }
            });
            start = true;
        }

        private void Update()
        {
            if (start) _nonsensicalTimer.Tick();
        }

        public void SetLog(NonsensicalTimer.TaskLog log)
        {
            _nonsensicalTimer.SetLog(log);
        }

        public void ResetTimer()
        {
            _nonsensicalTimer.ResetTimer();
        }

        #region TimeTask

        public IDPack AddTimerTask(Action<int> callBack, double delay, int count = 1,
            TimeUnit unit = TimeUnit.Millisecound, bool initialcall = false)
        {
            if (initialcall)
            {
                callBack.Invoke(0);
            }

            return _nonsensicalTimer.AddTimerTask(callBack, delay, count, unit);
        }

        public void ResetTimeTask(int id)
        {
            _nonsensicalTimer.ResetTimeTask(id);
        }

        public void DeleteTimeTask(int id)
        {
            _nonsensicalTimer.DeleteTimeTask(id);
        }

        public bool ReplaceTimeTask(int id, Action<int> callBack, double delay, int count = 1,
            TimeUnit unit = TimeUnit.Millisecound)
        {
            return _nonsensicalTimer.ReplaceTimeTask(id, callBack, delay, count, unit);
        }

        #endregion

        #region FrameTask

        public IDPack AddFrameTask(Action<int> callBack, int delay, int count = 1)
        {
            return _nonsensicalTimer.AddFrameTask(callBack, delay, count);
        }

        public void DeleteFrameTask(int id)
        {
            _nonsensicalTimer.DeleteFrameTask(id);
        }

        public bool ReplaceFrameTask(int id, Action<int> callBack, int delay, int count = 1)
        {
            return _nonsensicalTimer.ReplaceFrameTask(id, callBack, delay, count);
        }

        #endregion

        #region Tools

        public double GetMillisecondsTime()
        {
            return _nonsensicalTimer.GetMillisecondsTime();
        }

        public DateTime GetLocalDateTime()
        {
            return _nonsensicalTimer.GetLocalDateTime();
        }

        public int GetYear()
        {
            return _nonsensicalTimer.GetYear();
        }

        public int GetMonth()
        {
            return _nonsensicalTimer.GetMonth();
        }

        public int GetDay()
        {
            return _nonsensicalTimer.GetDay();
        }

        public int GetWeek()
        {
            return _nonsensicalTimer.GetWeek();
        }

        public string GetLocalTimeStr()
        {
            return _nonsensicalTimer.GetLocalTimeStr();
        }

        #endregion

        private double GetNow()
        {
            return Time.time * 1000;
        }
    }
}
