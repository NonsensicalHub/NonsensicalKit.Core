#if USE_UniTask
using Cysharp.Threading.Tasks;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace BJTimer
{
    public class BTimer
    {
        private Timer serTime;
        private TaskLog taskLog;
        private Action<Action<int>, int> taskHandle;

        public Func<double> GetNow;

        private static readonly string lockId = "lockId";
        private static readonly string lockTime = "lockTime";
        private static readonly string lockFrame = "lockFrame";
        //private static readonly string lockDelteId = "lockDelteId";

        private int id;
        private Dictionary<int, TaskFlag> idDic = new Dictionary<int, TaskFlag>();
        private List<int> delIds = new List<int>();

        private DateTime startDateTime = new DateTime(1970, 1, 1, 0, 0, 0);
        private double nowTime;

        private List<TimeTask> tempTimeTaskList = new List<TimeTask>();
        private List<UniTimeTask> tempTimeUniTaskList = new List<UniTimeTask>();
        private List<TimeTask> timeTaskList = new List<TimeTask>();
        private List<UniTimeTask> timeUniTaskList = new List<UniTimeTask>();
        private List<int> tempDelTimeList = new List<int>();


        private int sinceframeCount;
        private List<FrameTask> tempFrameTaskList = new List<FrameTask>();
        private List<FrameTask> frameTaskList = new List<FrameTask>();
        private List<int> tempDelFrameList = new List<int>();

        public void ResetTimer()
        {
            if (serTime != null) serTime.Stop();
            idDic.Clear();
            tempTimeTaskList.Clear();
            tempTimeUniTaskList.Clear();
            tempTimeUniTaskList.Clear();
            timeTaskList.Clear();
            tempFrameTaskList.Clear();
            frameTaskList.Clear();
            id = 0;
        }

        public void StartSeverTimer(int interval)
        {
            if (interval != 0 && taskHandle != null)
            {
                if (serTime != null) serTime.Dispose();

                serTime = new Timer(interval);
                serTime.AutoReset = true;
                serTime.Elapsed += (sender, arg) =>
                {
                    Tick();
                };
                serTime.Start();
            }
        }

        public void Tick()
        {
            TimeTaskTick();
#if USE_UniTask
            UniTimeTaskTick();
#endif
            FrameTaskTick();

            RecDelTimeTask();
#if USE_UniTask
            RecDelUniTimeTask();
#endif
            RecDelFrameTask();
            RecDelId();
        }

        public void SetLog(TaskLog log)
        {
            taskLog = log;
        }

        private void LogInfo(string info, LogLevel level = LogLevel.Log)
        {
            if (taskLog != null) taskLog.Invoke(info, level);
        }

        public void SetHandle(Action<Action<int>, int> handle)
        {
            taskHandle = handle;
        }

        #region Tool

        public double GetMillisecondsTime()
        {
            return nowTime;
        }

        public DateTime GetLocalDateTime()
        {
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;
            DateTime dt = TimeZoneInfo.ConvertTimeFromUtc(startDateTime.AddMilliseconds(nowTime), timeZoneInfo);
            return dt;
        }

        public int GetYear()
        {
            return GetLocalDateTime().Year;
        }

        public int GetMonth()
        {
            return GetLocalDateTime().Month;
        }

        public int GetDay()
        {
            return GetLocalDateTime().Day;
        }

        public int GetWeek()
        {
            return (int)GetLocalDateTime().DayOfWeek;
        }

        public string GetLocalTimeStr()
        {
            DateTime dt = GetLocalDateTime();
            string str = GetTimeStr(dt.Hour) + ":" + GetTimeStr(dt.Minute) + ":" + GetTimeStr(dt.Second);
            return str;
        }

        #endregion

        #region TimeTask

        private void TimeTaskTick()
        {
            if (tempTimeTaskList.Count > 0)
            {
                lock (lockTime)
                {
                    for (int i = 0; i < tempTimeTaskList.Count; i++)
                    {
                        AddTimeListItem(tempTimeTaskList[i]);
                    }

                    tempTimeTaskList.Clear();
                }
            }

            nowTime = GetNowTime();
            for (int i = 0; i < timeTaskList.Count; i++)
            {
                TimeTask task = timeTaskList[i];
                if (nowTime.CompareTo(task.destTime) < 0) continue;
                else
                {
                    Action<int> cb = task.callBack;
                    try
                    {
                        if (cb != null && taskHandle != null) taskHandle.Invoke(cb, task.id);
                        else if (cb != null) cb.Invoke(task.id);
                    }
                    catch (Exception e)
                    {
                        LogInfo(e.ToString(), LogLevel.Error);
                    }
                }

                if (task.count == 1)
                {
                    RemoveTimeListItem(i);
                    ///*lock (lockTime) */deleteIds.Add(task.id);
                }
                else
                {
                    if (task.count > 0)
                    {
                        --task.count;
                    }

                    task.destTime += task.delay;
                    timeTaskList[i] = task;
                }
            }
        }

        public IDPack AddTimerTask(Action<int> callBack, double delay, int count = 1, TimeUnit unit = TimeUnit.Millisecound)
        {
            switch (unit)
            {
                case TimeUnit.Millisecound:
                    break;
                case TimeUnit.Secound:
                    delay *= 1000;
                    break;
                case TimeUnit.Minute:
                    delay *= 1000 * 60;
                    break;
                case TimeUnit.Hour:
                    delay *= 1000 * 60 * 60;
                    break;
                case TimeUnit.Day:
                    delay *= 1000 * 60 * 60 * 24; // 最大支持 24天
                    break;
            }

            nowTime = GetNowTime();
            double destTime = nowTime + delay;

            int id = GetId();
            if (id == -1) return new IDPack(id, TaskType.TimeTask);

            idDic[id] = new TaskFlag(idDic[id], TaskType.TimeTask);
            TimeTask task = new TimeTask
            {
                id = id,
                delay = delay,
                destTime = destTime,
                callBack = callBack,
                count = count
            };
            lock (lockTime)
            {
                tempTimeTaskList.Add(task);
            }

            return new IDPack(id, TaskType.TimeTask);
        }

        public void DeleteTimeTask(int id)
        {
            lock (lockTime) tempDelTimeList.Add(id);
        }

        private bool DealDeleteTimeTask(int id)
        {
            bool exit = false;

            if (idDic.ContainsKey(id) && idDic[id].active)
            {
                exit = true;
                RemoveTimeListItem(idDic[id].index);
            }

            if (!exit)
            {
                for (int i = 0; i < tempTimeTaskList.Count; i++)
                {
                    if (tempTimeTaskList[i].id == id)
                    {
                        exit = true;
                        RemoveListItem_TimeTask(tempTimeTaskList, i);
                        delIds.Add(id);
                        //lock(lockTime) idDic.Remove(id);
                        break;
                    }
                }
            }

            return exit;
        }

        public bool ReplaceTimeTask(int id, Action<int> callBack, double delay, int count = 1, TimeUnit unit = TimeUnit.Millisecound)
        {
            switch (unit)
            {
                case TimeUnit.Millisecound:
                    break;
                case TimeUnit.Secound:
                    delay *= 1000;
                    break;
                case TimeUnit.Minute:
                    delay *= 1000 * 60;
                    break;
                case TimeUnit.Hour:
                    delay *= 1000 * 60 * 60;
                    break;
                case TimeUnit.Day:
                    delay *= 1000 * 60 * 60 * 24; // 最大支持 24天
                    break;
            }

            nowTime = GetNowTime();
            double destTime = nowTime + delay;
            TimeTask task = new TimeTask
            {
                id = id,
                delay = delay,
                destTime = destTime,
                callBack = callBack,
                count = count,
            };

            if (idDic.ContainsKey(id) && idDic[id].active)
            {
                timeTaskList[idDic[id].index] = task;
                return true;
            }
            else
            {
                for (int i = 0; i < tempTimeTaskList.Count; i++)
                {
                    if (tempTimeTaskList[i].id == id)
                    {
                        tempTimeTaskList[i] = task;
                        return true;
                    }
                }
            }

            return false;
        }

        private void RemoveTimeListItem(int index)
        {
            if (timeTaskList.Count == 0 && tempTimeTaskList.Count == 0) return;

            TimeTask task = timeTaskList[index];

            RemoveListItem_TimeTask(timeTaskList, index);

            if (index < timeTaskList.Count)
            {
                TimeTask indexTask = timeTaskList[index];
                TaskFlag flag = new TaskFlag
                {
                    id = indexTask.id,
                    index = index,
                    active = true
                };
                idDic[indexTask.id] = flag;
            }

            delIds.Add(task.id);
            //lock(lockId) idDic.Remove(task.id);
        }

        private void AddTimeListItem(TimeTask task)
        {
            TaskFlag flag = new TaskFlag
            {
                id = task.id,
                index = timeTaskList.Count,
                active = true
            };
            idDic[task.id] = flag;
            timeTaskList.Add(task);
        }

        private void RemoveListItem_TimeTask(List<TimeTask> list, int index)
        {
            int last = list.Count - 1;
            TimeTask temp = list[index];
            list[index] = list[last];
            list[last] = temp;
            list.RemoveAt(last);
        }

        private void RecDelTimeTask()
        {
            if (tempDelTimeList.Count > 0)
            {
                lock (lockTime)
                {
                    for (int i = 0; i < tempDelTimeList.Count; i++)
                    {
                        DealDeleteTimeTask(tempDelTimeList[i]);
                    }

                    tempDelTimeList.Clear();
                }
            }
        }

        #endregion

        #region UniTimeTask

#if USE_UniTask
        private void UniTimeTaskTick()
        {
            if (tempTimeUniTaskList.Count > 0)
            {
                lock (lockTime)
                {
                    for (int i = 0; i < tempTimeUniTaskList.Count; i++)
                    {
                        AddUniTimeListItem(tempTimeUniTaskList[i]);
                    }
                    tempTimeUniTaskList.Clear();
                }
            }

            nowTime = GetNowTime();
            for (int i = 0; i < timeUniTaskList.Count; i++)
            {
                UniTimeTask task = timeUniTaskList[i];
                if (nowTime.CompareTo(task.destTime) < 0) continue;
                else
                {
                    UniTask cb = task.callBack;
                    try
                    {
                        /*      if (cb != null && taskHandle != null) taskHandle.Invoke(cb, task.id);
                              else if (cb != null) cb.Invoke(task.id);*/
                        Debug.Log("invoke" + cb.ToString());
                        //                        UnityEngine.Debug.Log();
                        cb.Forget();
                    }
                    catch (Exception e)
                    {
                        LogInfo(e.ToString(), LogLevel.Error);
                    }
                }
                Debug.Log(task.count);
                if (task.count == 1)
                {
                    RemoveUniTimeListItem(i);
                    ///*lock (lockTime) */deleteIds.Add(task.id);
                }
                else
                {
                    if (task.count > 0)
                    {
                        --task.count;
                    }
                    task.destTime += task.delay;
                    timeUniTaskList[i] = task;
                }
            }
        }

        public IDPack AddUniTimerTask(UniTask callBack, double delay, int count = 1, TimeUnit unit = TimeUnit.Millisecound)
        {
            switch (unit)
            {
                case TimeUnit.Millisecound:
                    break;
                case TimeUnit.Secound:
                    delay *= 1000;
                    break;
                case TimeUnit.Minute:
                    delay *= 1000 * 60;
                    break;
                case TimeUnit.Hour:
                    delay *= 1000 * 60 * 60;
                    break;
                case TimeUnit.Day:
                    delay *= 1000 * 60 * 60 * 24; // 最大支持 24天
                    break;
            }
            nowTime = GetNowTime();
            double destTime = nowTime + delay;

            int id = GetId();
            if (id == -1) return new IDPack(id, TaskType.TimeTask);

            idDic[id] = new TaskFlag(idDic[id], TaskType.TimeTask);
            UniTimeTask task = new UniTimeTask
            {
                id = id,
                delay = delay,
                destTime = destTime,
                callBack = callBack,
                count = count
            };
            lock (lockTime)
            {
                tempTimeUniTaskList.Add(task);
            }

            return new IDPack(id, TaskType.TimeTask);
        }

        public void DeleteUniTimeTask(int id)
        {
            lock (lockTime) tempDelTimeList.Add(id);
        }

        private bool DealDeleteUniTimeTask(int id)
        {
            bool exit = false;

            if (idDic.ContainsKey(id) && idDic[id].active)
            {
                exit = true;
                RemoveUniTimeListItem(idDic[id].index);
            }

            if (!exit)
            {
                for (int i = 0; i < tempTimeUniTaskList.Count; i++)
                {
                    if (tempTimeUniTaskList[i].id == id)
                    {
                        exit = true;
                        RemoveListItem_UniTimeTask(tempTimeUniTaskList, i);
                        delIds.Add(id);
                        //lock(lockTime) idDic.Remove(id);
                        break;
                    }
                }
            }

            return exit;
        }

        public bool ReplaceUniTimeTask(int id, UniTask callBack, double delay, int count = 1, TimeUnit unit = TimeUnit.Millisecound)
        {
            switch (unit)
            {
                case TimeUnit.Millisecound:
                    break;
                case TimeUnit.Secound:
                    delay *= 1000;
                    break;
                case TimeUnit.Minute:
                    delay *= 1000 * 60;
                    break;
                case TimeUnit.Hour:
                    delay *= 1000 * 60 * 60;
                    break;
                case TimeUnit.Day:
                    delay *= 1000 * 60 * 60 * 24; // 最大支持 24天
                    break;
            }
            nowTime = GetNowTime();
            double destTime = nowTime + delay;
            UniTimeTask task = new UniTimeTask
            {
                id = id,
                delay = delay,
                destTime = destTime,
                callBack = callBack,
                count = count,
            };

            if (idDic.ContainsKey(id) && idDic[id].active)
            {
                timeUniTaskList[idDic[id].index] = task;
                return true;
            }
            else
            {
                for (int i = 0; i < tempTimeUniTaskList.Count; i++)
                {
                    if (tempTimeUniTaskList[i].id == id)
                    {
                        tempTimeUniTaskList[i] = task;
                        return true;
                    }
                }
            }

            return false;
        }

        private void RemoveUniTimeListItem(int index)
        {
            if (timeUniTaskList.Count == 0 && tempTimeUniTaskList.Count == 0) return;

            UniTimeTask task = timeUniTaskList[index];

            RemoveListItem_UniTimeTask(timeUniTaskList, index);

            if (index < timeTaskList.Count)
            {
                UniTimeTask indexTask = timeUniTaskList[index];
                TaskFlag flag = new TaskFlag
                {
                    id = indexTask.id,
                    index = index,
                    active = true
                };
                idDic[indexTask.id] = flag;
            }

            delIds.Add(task.id);
            //lock(lockId) idDic.Remove(task.id);
        }
        private void AddUniTimeListItem(UniTimeTask task)
        {
            TaskFlag flag = new TaskFlag
            {
                id = task.id,
                index = timeTaskList.Count,
                active = true
            };
            idDic[task.id] = flag;
            timeUniTaskList.Add(task);
        }
        private void RemoveListItem_UniTimeTask(List<UniTimeTask> list, int index)
        {
            int last = list.Count - 1;
            UniTimeTask temp = list[index];
            list[index] = list[last];
            list[last] = temp;
            list.RemoveAt(last);
        }
        private void RecDelUniTimeTask()
        {
            if (tempDelTimeList.Count > 0)
            {
                lock (lockTime)
                {
                    for (int i = 0; i < tempDelTimeList.Count; i++)
                    {
                        DealDeleteUniTimeTask(tempDelTimeList[i]);
                    }

                    tempDelTimeList.Clear();
                }
            }
        }

#endif

        #endregion

        #region FrameTask

        private void FrameTaskTick()
        {
            if (tempFrameTaskList.Count > 0)
            {
                lock (lockFrame)
                {
                    for (int i = 0; i < tempFrameTaskList.Count; i++)
                    {
                        AddFrameListItem(tempFrameTaskList[i]);
                    }

                    tempFrameTaskList.Clear();
                }
            }


            for (int i = 0; i < frameTaskList.Count; i++)
            {
                FrameTask task = frameTaskList[i];
                if (sinceframeCount < task.destFrame) continue;
                else
                {
                    Action<int> cb = task.callBack;
                    try
                    {
                        if (cb != null && taskHandle != null) taskHandle.Invoke(cb, task.id);
                        else if (cb != null) cb.Invoke(task.id);
                    }
                    catch (Exception e)
                    {
                        LogInfo(e.ToString(), LogLevel.Error);
                    }
                }

                if (task.count == 1)
                {
                    RemoveFrameListItem(i);
                    //deleteIds.Add(task.id);
                }
                else
                {
                    if (task.count > 0)
                    {
                        --task.count;
                    }

                    task.destFrame += task.delay;
                    frameTaskList[i] = task;
                }
            }

            ++sinceframeCount;
        }

        public IDPack AddFrameTask(Action<int> callBack, int delay, int count = 1)
        {
            int destFrame = sinceframeCount + delay;

            int id = GetId();
            if (id == -1) return new IDPack(id, TaskType.FrameTask);

            idDic[id] = new TaskFlag(idDic[id], TaskType.FrameTask);
            FrameTask task = new FrameTask
            {
                id = id,
                delay = delay,
                destFrame = destFrame,
                callBack = callBack,
                count = count
            };
            lock (lockFrame)
            {
                tempFrameTaskList.Add(task);
            }

            return new IDPack(id, TaskType.FrameTask);
        }

        public void DeleteFrameTask(int id)
        {
            lock (lockFrame) tempDelFrameList.Add(id);
        }

        private bool DealDeleteFrameTask(int id)
        {
            bool exit = false;
            if (idDic.ContainsKey(id) && idDic[id].active)
            {
                exit = true;
                RemoveFrameListItem(idDic[id].index);
            }

            if (!exit)
            {
                for (int i = 0; i < tempFrameTaskList.Count; i++)
                {
                    if (tempFrameTaskList[i].id == id)
                    {
                        exit = true;
                        RemoveListItem_FrameTask(tempFrameTaskList, i);
                        delIds.Add(id);
                        ///*lock (obj)*/ idDic.Remove(id);
                        break;
                    }
                }
            }

            return exit;
        }

        public bool ReplaceFrameTask(int id, Action<int> callBack, int delay, int count = 1)
        {
            int destFrame = sinceframeCount + delay;
            FrameTask task = new FrameTask
            {
                id = id,
                delay = delay,
                destFrame = destFrame,
                callBack = callBack,
                count = count
            };

            if (idDic.ContainsKey(id) && idDic[id].active)
            {
                frameTaskList[idDic[id].index] = task;
                return true;
            }
            else
            {
                for (int i = 0; i < tempFrameTaskList.Count; i++)
                {
                    if (tempFrameTaskList[i].id == id)
                    {
                        tempFrameTaskList[i] = task;
                        return true;
                    }
                }
            }

            return false;
        }

        private void RemoveFrameListItem(int index)
        {
            if (frameTaskList.Count == 0 && tempFrameTaskList.Count == 0) return;

            FrameTask task = frameTaskList[index];

            RemoveListItem_FrameTask(frameTaskList, index);
            if (index < frameTaskList.Count)
            {
                FrameTask indexTask = frameTaskList[index];
                TaskFlag flag = new TaskFlag
                {
                    id = indexTask.id,
                    index = index,
                    active = true
                };
                idDic[indexTask.id] = flag;
            }

            delIds.Add(task.id);
            //idDic.Remove(task.id);
        }

        private void AddFrameListItem(FrameTask task)
        {
            TaskFlag flag = new TaskFlag
            {
                id = task.id,
                index = frameTaskList.Count,
                active = true
            };
            idDic[task.id] = flag;
            frameTaskList.Add(task);
        }

        private void RemoveListItem_FrameTask(List<FrameTask> list, int index)
        {
            int last = list.Count - 1;
            FrameTask temp = list[index];
            list[index] = list[last];
            list[last] = temp;
            list.RemoveAt(last);
        }

        private void RecDelFrameTask()
        {
            if (tempDelFrameList.Count > 0)
            {
                lock (lockFrame)
                {
                    for (int i = 0; i < tempDelFrameList.Count; i++)
                    {
                        DealDeleteFrameTask(tempDelFrameList[i]);
                    }

                    tempDelFrameList.Clear();
                }
            }
        }

        #endregion

        #region Common

        private int GetId()
        {
            lock (lockId)
            {
                id += 1;

                int len = 0;
                while (true)
                {
                    if (id == int.MaxValue) id = 0;

                    if (idDic.ContainsKey(id)) id++;
                    else break;

                    len++;
                    if (len == int.MaxValue)
                    {
                        LogInfo("计时任务已满，无法添加任务", LogLevel.Error);
                        return -1;
                    }
                }

                TaskFlag flag = new TaskFlag
                {
                    id = id,
                    active = false
                };
                idDic.Add(id, flag);
            }

            return id;
        }

        private void RecDelId()
        {
            if (delIds.Count > 0)
            {
                lock (lockId)
                {
                    for (int i = 0; i < delIds.Count; i++)
                    {
                        idDic.Remove(delIds[i]);
                    }
                }
            }
        }

        private double GetNowTime()
        {
            if (GetNow == null)
            {
                return GetUTCMilliseconds();
            }
            else
            {
                return GetNow();
            }
        }

        private double GetUTCMilliseconds()
        {
            TimeSpan ts = DateTime.UtcNow - startDateTime;
            return ts.TotalMilliseconds;
        }

        private string GetTimeStr(int time)
        {
            if (time < 10) return "0" + time.ToString();
            else return time.ToString();
        }

        #endregion

        public delegate void TaskLog(string str, LogLevel logLevel = LogLevel.Log);

        public enum LogLevel
        {
            Info,
            Log,
            Warning,
            Error
        }
    }
}
