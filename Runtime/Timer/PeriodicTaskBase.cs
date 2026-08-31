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
        private bool _isRunning;

        protected virtual void Awake()
        {
            if (m_enableOnAwake)
            {
                SetStatus(true);
            }
        }

        protected virtual void OnDestroy()
        {
            if (!_isRunning) return;

            // 退出时勿通过 Instance 重建 TimerSystem
            if (TimerSystem.HasInstance)
            {
                TimerSystem.Instance.DeleteTimeTask(_timerID.id);
            }

            _isRunning = false;
        }

        public virtual void SetStatus(bool enable)
        {
            if (enable)
            {
                if (_isRunning) return;

                _timerID = TimerSystem.Instance.AddTimerTask(PeriodicTask, m_interval, m_requestCount, TimeUnit.Second,
                    m_initialCall);
                _isRunning = true;
            }
            else
            {
                if (!_isRunning) return;

                TimerSystem.Instance.DeleteTimeTask(_timerID.id);
                _isRunning = false;
            }
        }

        protected virtual void ResetTimer()
        {
            if (!_isRunning) return;

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
