using NonsensicalKit.Core;
using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.Tools
{
    /// <summary>
    /// 仿DOTween，作为简单需求时的替代品
    /// </summary>
    public static class DoTweenTool
    {
        private static NonsensicalInstance Hub
        {
            get
            {
                if (_hubBuffer == null)
                {
                    _hubBuffer = NonsensicalInstance.Instance;
                }

                return _hubBuffer;
            }
        }

        private static NonsensicalInstance _hubBuffer;

        public static Tweener DoFade(this CanvasGroup canvasGroup, float endValue, float value)
        {
            CanvasGroupTweener newTweener = new CanvasGroupTweener(canvasGroup, endValue, value);

            Hub.Tweeners.Add(newTweener);

            return newTweener;
        }

        public static Tweener DoMove(this Transform transform, Vector3 endValue, float value)
        {
            TransformMoveTweener newTweener = new TransformMoveTweener(transform, endValue, value);

            Hub.Tweeners.Add(newTweener);

            return newTweener;
        }

        public static Tweener DoMove(this RectTransform transform, Vector2 endValue, float value)
        {
            RectTransformMoveTweener newTweener = new RectTransformMoveTweener(transform, endValue, value);

            Hub.Tweeners.Add(newTweener);

            return newTweener;
        }

        public static Tweener DoRotate(this Transform transform, Vector3 endValue, float value)
        {
            TransformRotateTweener newTweener = new TransformRotateTweener(transform, endValue, value);

            Hub.Tweeners.Add(newTweener);

            return newTweener;
        }

        public static Tweener DoRotate(this Transform transform, Quaternion endValue, float value)
        {
            TransformQuaternionRotateTweener newTweener = new TransformQuaternionRotateTweener(transform, endValue, value);

            Hub.Tweeners.Add(newTweener);

            return newTweener;
        }

        public static Tweener DoLocalMove(this Transform transform, Vector3 endValue, float value)
        {
            TransformLocalMoveTweener newTweener = new TransformLocalMoveTweener(transform, endValue, value);

            Hub.Tweeners.Add(newTweener);

            return newTweener;
        }

        public static Tweener DoLocalMoveX(this Transform transform, float endValue, float value)
        {
            TransformLocalMoveXTweener newTweener = new TransformLocalMoveXTweener(transform, endValue, value);

            Hub.Tweeners.Add(newTweener);

            return newTweener;
        }

        public static Tweener DoLocalRotate(this Transform transform, Vector3 endValue, float value)
        {
            TransformLocalRotateTweener newTweener = new TransformLocalRotateTweener(transform, endValue, value);

            Hub.Tweeners.Add(newTweener);

            return newTweener;
        }

        public static Tweener DoLocalRotate(this Transform transform, Quaternion endValue, float value)
        {
            TransformQuaternionLocalRotateTweener newTweener = new TransformQuaternionLocalRotateTweener(transform, endValue, value);

            Hub.Tweeners.Add(newTweener);

            return newTweener;
        }

        public static Tweener DoLocalScale(this Transform transform, Vector3 endValue, float value)
        {
            TransformLocalScaleTweener newTweener = new TransformLocalScaleTweener(transform, endValue, value);

            Hub.Tweeners.Add(newTweener);

            return newTweener;
        }

        public static Tweener DoScrollTo(this ScrollRect scrollRect, Vector2 endValue, float value)
        {
            ScrollRectScrollToTweener newTweener = new ScrollRectScrollToTweener(scrollRect, endValue, value);

            Hub.Tweeners.Add(newTweener);

            return newTweener;
        }
    }

    public abstract class Tweener
    {
        public bool IsOver => _isOver;

        /// <summary>
        /// 默认是运动的总用时，依据速度运动时是速度值
        /// </summary>
        private readonly float _value;

        /// <summary>
        /// 总运动量，用于以速度运动时的进度依据
        /// </summary>
        protected float TotalValue;

        /// <summary>
        /// 延迟多久后执行
        /// </summary>
        private float _delay;

        /// <summary>
        /// 累积时间
        /// </summary>
        protected float AccumulateTime;

        /// <summary>
        /// 是否已经结束运动
        /// </summary>
        private bool _isOver;

        /// <summary>
        /// 是否暂停
        /// </summary>
        private bool _isPause;

        /// <summary>
        /// _value是否代表速度，为否时_value代表总用时
        /// </summary>
        private bool _speedBase;

        public delegate void OnCompleteHandle();

        public OnCompleteHandle OnCompleteEvent;

        protected Tweener(float value)
        {
            _value = value;
            AccumulateTime = 0;
            _delay = 0;
            _isPause = false;
            _isOver = false;
            DoSpecificBySchedule(0);
        }

        /// <summary>
        /// 由NonsensicalUnityInstance每帧调用的方法
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public bool DoIt(float deltaTime)
        {
            if (_isPause)
            {
                return false;
            }

            float schedule;
            AccumulateTime += deltaTime;
            if (_speedBase)
            {
                if (_value <= 0 || TotalValue == 0)
                {
                    schedule = 1;
                }
                else
                {
                    schedule = (AccumulateTime - _delay) * _value / TotalValue;
                }
            }
            else
            {
                if (_value <= 0)
                {
                    schedule = 1;
                }
                else
                {
                    schedule = (AccumulateTime - _delay) / _value;
                }
            }

            if (schedule >= 1)
            {
                _isOver = DoSpecificBySchedule(1);
                OnCompleteEvent?.Invoke();
                return true;
            }
            else
            {
                _isOver = DoSpecificBySchedule(schedule);
                return false;
            }
        }

        public void Pause()
        {
            _isPause = true;
        }

        public void Resume()
        {
            _isPause = false;
        }

        public void Abort()
        {
            _isOver = true;
        }

        /// <summary>
        /// 某一次调用后以进度为依据执行的具体行为
        /// </summary>
        /// <param name="schedule">区间为0到1的进度值</param>
        /// <returns>是否已经完成运动</returns>
        public abstract bool DoSpecificBySchedule(float schedule);

        public Tweener SetDelay(float time)
        {
            _delay = time;
            return this;
        }

        /// <summary>
        /// 将传入的第二个参数作为速度使用，其中位移的速度单位是m/s，旋转的速度单位是°/s
        /// </summary>
        /// <returns></returns>
        public Tweener SetSpeedBased()
        {
            _speedBase = true;
            return this;
        }

        public Tweener OnComplete(OnCompleteHandle func)
        {
            OnCompleteEvent += func;
            return this;
        }
    }

    public class CanvasGroupTweener : Tweener
    {
        private readonly CanvasGroup _canvasGroup;
        private readonly float _startValue;
        private readonly float _endValue;

        public CanvasGroupTweener(CanvasGroup canvasGroup, float endValue, float value) : base(value)
        {
            this._canvasGroup = canvasGroup;
            _startValue = canvasGroup.alpha;
            this._endValue = endValue;
            TotalValue = Mathf.Abs(endValue - _startValue);
        }

        public override bool DoSpecificBySchedule(float schedule)
        {
            if (_canvasGroup == null)
            {
                return true;
            }

            _canvasGroup.alpha = _startValue + (_endValue - _startValue) * schedule;

            return false;
        }
    }

    public class TransformRotateTweener : Tweener
    {
        private readonly Transform _transform;
        private readonly Vector3 _startValue;
        private readonly Vector3 _endValue;

        public TransformRotateTweener(Transform transform, Vector3 endValue, float value) : base(value)
        {
            this._transform = transform;
            _startValue = transform.eulerAngles;
            this._endValue = endValue.AngleNear(_startValue);
            TotalValue = Quaternion.Angle(Quaternion.Euler(_startValue), Quaternion.Euler(endValue));
        }

        public override bool DoSpecificBySchedule(float schedule)
        {
            if (_transform == null)
            {
                return true;
            }

            _transform.eulerAngles = Vector3.Lerp(_startValue, _endValue, schedule);

            return false;
        }
    }

    public class TransformQuaternionRotateTweener : Tweener
    {
        private readonly Transform _transform;
        private readonly Quaternion _startValue;
        private readonly Quaternion _endValue;

        public TransformQuaternionRotateTweener(Transform transform, Quaternion endValue, float value) : base(value)
        {
            this._transform = transform;
            _startValue = transform.rotation;
            this._endValue = endValue;
            TotalValue = Quaternion.Angle(_startValue, endValue);
        }

        public override bool DoSpecificBySchedule(float schedule)
        {
            if (_transform == null)
            {
                return true;
            }

            _transform.rotation = Quaternion.Lerp(_startValue, _endValue, schedule);

            return false;
        }
    }

    public class TransformQuaternionLocalRotateTweener : Tweener
    {
        private readonly Transform _transform;
        private readonly Quaternion _startValue;
        private readonly Quaternion _endValue;

        public TransformQuaternionLocalRotateTweener(Transform transform, Quaternion endValue, float value) : base(value)
        {
            this._transform = transform;
            _startValue = transform.localRotation;
            this._endValue = endValue;
            TotalValue = Quaternion.Angle(_startValue, endValue);
        }

        public override bool DoSpecificBySchedule(float schedule)
        {
            if (_transform == null)
            {
                return true;
            }

            _transform.localRotation = Quaternion.Lerp(_startValue, _endValue, schedule);

            return false;
        }
    }

    public class TransformMoveTweener : Tweener
    {
        private readonly Transform _transform;
        private readonly Vector3 _startValue;
        private readonly Vector3 _endValue;

        public TransformMoveTweener(Transform transform, Vector3 endValue, float value) : base(value)
        {
            this._transform = transform;
            _startValue = transform.position;
            this._endValue = endValue;
            TotalValue = Vector3.Distance(_startValue, endValue);
        }

        public override bool DoSpecificBySchedule(float schedule)
        {
            if (_transform == null)
            {
                return true;
            }

            _transform.position = Vector3.Lerp(_startValue, _endValue, schedule);

            return false;
        }
    }

    public class RectTransformMoveTweener : Tweener
    {
        private readonly RectTransform _rectTransform;
        private readonly Vector2 _startValue;
        private readonly Vector2 _endValue;

        public RectTransformMoveTweener(RectTransform rectTransform, Vector2 endValue, float value) : base(value)
        {
            this._rectTransform = rectTransform;
            _startValue = rectTransform.anchoredPosition;
            this._endValue = endValue;
            TotalValue = Vector2.Distance(_startValue, endValue);
        }

        public override bool DoSpecificBySchedule(float schedule)
        {
            if (_rectTransform == null)
            {
                return true;
            }

            _rectTransform.anchoredPosition = Vector2.Lerp(_startValue, _endValue, schedule);

            return false;
        }
    }

    public class TransformLocalMoveTweener : Tweener
    {
        private readonly Transform _transform;
        private readonly Vector3 _startValue;
        private readonly Vector3 _endValue;

        public TransformLocalMoveTweener(Transform transform, Vector3 endValue, float value) : base(value)
        {
            this._transform = transform;
            _startValue = transform.localPosition;
            this._endValue = endValue;
            TotalValue = Vector3.Distance(_startValue, endValue);
        }

        public override bool DoSpecificBySchedule(float schedule)
        {
            if (_transform == null)
            {
                return true;
            }

            _transform.localPosition = Vector3.Lerp(_startValue, _endValue, schedule);

            return false;
        }
    }

    public class TransformLocalRotateTweener : Tweener
    {
        private readonly Transform _transform;
        private readonly Vector3 _startValue;
        private readonly Vector3 _endValue;

        public TransformLocalRotateTweener(Transform transform, Vector3 endValue, float value) : base(value)
        {
            this._transform = transform;
            _startValue = transform.localEulerAngles;
            this._endValue = endValue.AngleNear(_startValue);
            TotalValue = Quaternion.Angle(Quaternion.Euler(_startValue), Quaternion.Euler(endValue));
        }

        public override bool DoSpecificBySchedule(float schedule)
        {
            if (_transform == null)
            {
                return true;
            }

            _transform.localEulerAngles = Vector3.Lerp(_startValue, _endValue, schedule);
            return false;
        }
    }

    public class TransformLocalScaleTweener : Tweener
    {
        private readonly Transform _transform;
        private readonly Vector3 _startValue;
        private readonly Vector3 _endValue;

        public TransformLocalScaleTweener(Transform transform, Vector3 endValue, float value) : base(value)
        {
            this._transform = transform;
            _startValue = transform.localScale;
            this._endValue = endValue;
            TotalValue = Vector3.Distance(_startValue, endValue);
        }

        public override bool DoSpecificBySchedule(float schedule)
        {
            if (_transform == null)
            {
                return true;
            }

            _transform.localScale = Vector3.Lerp(_startValue, _endValue, schedule);
            return false;
        }
    }

    public class TransformLocalMoveXTweener : Tweener
    {
        private readonly Transform _transform;
        private readonly float _startValue;
        private readonly float _endValue;

        public TransformLocalMoveXTweener(Transform transform, float endValue, float value) : base(value)
        {
            this._transform = transform;
            _startValue = transform.localPosition.x;
            this._endValue = endValue;
            TotalValue = Mathf.Abs(endValue - _startValue);
        }

        public override bool DoSpecificBySchedule(float schedule)
        {
            if (_transform == null)
            {
                return true;
            }

            Vector3 temp = _transform.localPosition;
            temp.x = _startValue + (_endValue - _startValue) * schedule;
            _transform.localPosition = temp;
            return false;
        }
    }

    public class ScrollRectScrollToTweener : Tweener
    {
        private readonly ScrollRect _scrollRect;
        private readonly Vector2 _startValue;
        private readonly Vector2 _endValue;

        public ScrollRectScrollToTweener(ScrollRect scrollRect, Vector2 endValue, float value) : base(value)
        {
            this._scrollRect = scrollRect;
            _startValue = scrollRect.normalizedPosition;
            this._endValue = endValue;
            TotalValue = Vector2.Distance(_startValue, endValue);
        }

        public override bool DoSpecificBySchedule(float schedule)
        {
            if (_scrollRect == null)
            {
                return true;
            }

            _scrollRect.normalizedPosition = Vector2.Lerp(_startValue, _endValue, schedule);
            return false;
        }
    }
}
