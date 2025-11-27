using UnityEngine;

namespace NonsensicalKit.Core.Timer
{
    /// <summary>
    /// 周期调用PeriodicTask方法
    /// </summary>
    public abstract class PeriodicTaskBase : MonoBehaviour
    {
        [SerializeField] protected bool m_log;
        [SerializeField, Tooltip("调用间隔")] private float m_interval;
        [SerializeField, Tooltip("调用次数")] private int m_requestCount;
        [SerializeField, Tooltip("初始调用")] private bool m_initialCall;
        [SerializeField] private bool m_enableOnAwake;

        private IDPack _timerID;

        protected virtual void Awake()
        {
            if (m_enableOnAwake)
            {
                _timerID = TimerSystem.Instance.AddTimerTask(PeriodicTask, m_interval, m_requestCount, TimeUnit.Secound,
                    m_initialCall);
            }
        }

        protected virtual void OnDestroy()
        {
            TimerSystem.Instance.DeleteTimeTask(_timerID.id);
        }

        public virtual void SetStatus(bool enable)
        {
            if (enable)
            {
                _timerID = TimerSystem.Instance.AddTimerTask(PeriodicTask, m_interval, m_requestCount, TimeUnit.Secound,
                    m_initialCall);
            }
            else
            {
                TimerSystem.Instance.DeleteTimeTask(_timerID.id);
            }
        }

        protected virtual void ResetTimer()
        {
            TimerSystem.Instance.ResetTimeTask(_timerID.id);
        }

        private void PeriodicTask(int _)
        {
            PeriodicTask();
        }

        protected abstract void PeriodicTask();

        [ContextMenu("test")]
        public virtual void Test()
        {
            PeriodicTask();
        }
    }
}
