using System;
using BJTimer;
using UnityEngine;

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

    private BTimer bTimer;
    private bool start = false;

    public void Init()
    {
        bTimer = new BTimer();
        bTimer.GetNow = GetNow;
        bTimer.SetLog((str, level) =>
        {
            switch (level)
            {
                case BTimer.LogLevel.Info:
                    Debug.Log(str);
                    break;
                case BTimer.LogLevel.Log:
                    Debug.Log(str);
                    break;
                case BTimer.LogLevel.Warning:
                    Debug.LogWarning(str);
                    break;
                case BTimer.LogLevel.Error:
                    Debug.LogError(str);
                    break;
            }
        });
    }

    public void StartTimer()
    {
        start = true;
    }

    public void SetLog(BTimer.TaskLog log)
    {
        bTimer.SetLog(log);
    }

    private void Update()
    {
        if (start) bTimer.Tick();
    }

    public void ResetTimer()
    {
        bTimer.ResetTimer();
    }

    #region TimeTask
    public IDPack AddTimerTask(Action<int> callBack, double delay, int count = 1, TimeUnit unit = TimeUnit.Millisecound, bool initialcall = false)
    {
        if (initialcall)
        {
            callBack.Invoke(0);
        }
        return bTimer.AddTimerTask(callBack, delay, count, unit);
    }
    public void DeleteTimeTask(int id)
    {
        bTimer.DeleteTimeTask(id);
    }

    public bool ReplaceTimeTask(int id, Action<int> callBack, double delay, int count = 1, TimeUnit unit = TimeUnit.Millisecound)
    {
        return bTimer.ReplaceTimeTask(id, callBack, delay, count, unit);
    }

    #endregion

    #region FrameTask
    public IDPack AddFrameTask(Action<int> callBack, int delay, int count = 1)
    {
        return bTimer.AddFrameTask(callBack, delay, count);
    }

    public void DeleteFrameTask(int id)
    {
        bTimer.DeleteFrameTask(id);
    }

    public bool ReplaceFrameTask(int id, Action<int> callBack, int delay, int count = 1)
    {
        return bTimer.ReplaceFrameTask(id, callBack, delay, count);
    }
    #endregion

    #region Tools
    public double GetMillisecondsTime()
    {
        return bTimer.GetMillisecondsTime();
    }

    public DateTime GetLocalDateTime()
    {
        return bTimer.GetLocalDateTime();
    }

    public int GetYear()
    {
        return bTimer.GetYear();
    }
    public int GetMonth()
    {
        return bTimer.GetMonth();
    }
    public int GetDay()
    {
        return bTimer.GetDay();
    }
    public int GetWeek()
    {
        return bTimer.GetWeek();
    }

    public string GetLocalTimeStr()
    {
        return bTimer.GetLocalTimeStr();
    }
    #endregion

    private double GetNow()
    {
        return Time.unscaledTime *1000;
    }
}
