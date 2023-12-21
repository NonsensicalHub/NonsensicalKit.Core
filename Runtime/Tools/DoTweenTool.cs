using NonsensicalKit.Editor;
using UnityEngine;

namespace NonsensicalKit.Tools
{
    /// <summary>
    /// 仿DOTween，作为简单需求时的替代品
    /// </summary>
    public static class DoTweenTool
    {
        private static NonsensicalInstance _hub
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

        public static Tweenner DoFade(this CanvasGroup canvasGroup, float endValue, float value)
        {
            CanvasGroupTweener newTweener = new CanvasGroupTweener(canvasGroup, endValue, value);

            _hub.Tweenners.Add(newTweener);

            return newTweener;
        }

        public static Tweenner DoMove(this Transform _transform, Vector3 endValue, float value)
        {
            TransformMoveTweener newTweener = new TransformMoveTweener(_transform, endValue, value);

            _hub.Tweenners.Add(newTweener);

            return newTweener;
        }
        public static Tweenner DoMove(this RectTransform _transform, Vector2 endValue, float value)
        {
            RectTransformMoveTweener newTweener = new RectTransformMoveTweener(_transform, endValue, value);

            _hub.Tweenners.Add(newTweener);

            return newTweener;
        }

        public static Tweenner DoRotate(this Transform _transform, Vector3 endValue, float value)
        {
            TransformRotateTweener newTweener = new TransformRotateTweener(_transform, endValue, value);

            _hub.Tweenners.Add(newTweener);

            return newTweener;
        }

        public static Tweenner DoRotate(this Transform _transform, Quaternion endValue, float value)
        {
            TransformQuaternionRotateTweener newTweener = new TransformQuaternionRotateTweener(_transform, endValue, value);

            _hub.Tweenners.Add(newTweener);

            return newTweener;
        }

        public static Tweenner DoLocalMove(this Transform _transform, Vector3 endValue, float value)
        {
            TransformLocalMoveTweener newTweener = new TransformLocalMoveTweener(_transform, endValue, value);

            _hub.Tweenners.Add(newTweener);

            return newTweener;
        }

        public static Tweenner DoLocalMoveX(this Transform _transform, float endValue, float value)
        {
            TransformLocalMoveXTweener newTweener = new TransformLocalMoveXTweener(_transform, endValue, value);

            _hub.Tweenners.Add(newTweener);

            return newTweener;
        }

        public static Tweenner DoLocalRotate(this Transform _transform, Vector3 endValue, float value)
        {
            TransformLocalRotateTweener newTweener = new TransformLocalRotateTweener(_transform, endValue, value);

            _hub.Tweenners.Add(newTweener);

            return newTweener;
        }

        public static Tweenner DoLocalRotate(this Transform _transform, Quaternion endValue, float value)
        {
            TransformQuaternionLocalRotateTweener newTweener = new TransformQuaternionLocalRotateTweener(_transform, endValue, value);

            _hub.Tweenners.Add(newTweener);

            return newTweener;
        }

        public static Tweenner DoLocalScale(this Transform _transform, Vector3 endValue, float value)
        {
            TransformLocalScaleTweener newTweener = new TransformLocalScaleTweener(_transform, endValue, value);

            _hub.Tweenners.Add(newTweener);

            return newTweener;
        }
    }

    public abstract class Tweenner
    {
        public bool IsOver => _isOver;

        /// <summary>
        /// 默认是运动的总用时，依据速度运动时是速度值
        /// </summary>
        private readonly float _value;
        /// <summary>
        /// 总运动量，用于以速度运动时的进度依据
        /// </summary>
        protected float _totalValue;
        /// <summary>
        /// 延迟多久后执行
        /// </summary>
        private float _delay;
        /// <summary>
        /// 累积时间
        /// </summary>
        protected float _accumulateTime;
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

        protected Tweenner(float value)
        {
            _value = value;
            _accumulateTime = 0;
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
            _accumulateTime += deltaTime;
            if (_speedBase)
            {
                if (_value <= 0 || _totalValue == 0)
                {
                    schedule = 1;
                }
                else
                {
                    schedule = (_accumulateTime - _delay) * _value / _totalValue;
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
                    schedule = (_accumulateTime - _delay) / _value;
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

        public Tweenner SetDelay(float time)
        {
            _delay = time;
            return this;
        }

        /// <summary>
        /// 将传入的第二个参数作为速度使用，其中位移的速度单位是m/s，旋转的速度单位是°/s
        /// </summary>
        /// <returns></returns>
        public Tweenner SetSpeedBased()
        {
            _speedBase = true;
            return this;
        }

        public Tweenner OnComplete(OnCompleteHandle func)
        {
            OnCompleteEvent += func;
            return this;
        }

    }

    public class CanvasGroupTweener : Tweenner
    {
        private readonly CanvasGroup _canvasGroup;
        private readonly float _startValue;
        private readonly float _endValue;

        public CanvasGroupTweener(CanvasGroup canvasGroup, float endValue, float value) : base(value)
        {
            this._canvasGroup = canvasGroup;
            _startValue = canvasGroup.alpha;
            this._endValue = endValue;
            _totalValue = Mathf.Abs(endValue - _startValue);
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

    public class TransformRotateTweener : Tweenner
    {
        private readonly Transform _transform;
        private readonly Vector3 _startValue;
        private readonly Vector3 _endValue;

        public TransformRotateTweener(Transform transform, Vector3 endValue, float value) : base(value)
        {
            this._transform = transform;
            _startValue = transform.eulerAngles;
            this._endValue = endValue.AngleNear(_startValue);
            _totalValue = Quaternion.Angle(Quaternion.Euler(_startValue), Quaternion.Euler(endValue));
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

    public class TransformQuaternionRotateTweener : Tweenner
    {
        private readonly Transform _transform;
        private readonly Quaternion _startValue;
        private readonly Quaternion _endValue;

        public TransformQuaternionRotateTweener(Transform transform, Quaternion endValue, float value) : base(value)
        {
            this._transform = transform;
            _startValue = transform.rotation;
            this._endValue = endValue;
            _totalValue = Quaternion.Angle(_startValue, endValue);
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

    public class TransformQuaternionLocalRotateTweener : Tweenner
    {
        private readonly Transform _transform;
        private readonly Quaternion _startValue;
        private readonly Quaternion _endValue;

        public TransformQuaternionLocalRotateTweener(Transform transform, Quaternion endValue, float value) : base(value)
        {
            this._transform = transform;
            _startValue = transform.localRotation;
            this._endValue = endValue;
            _totalValue = Quaternion.Angle(_startValue, endValue);
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

    public class TransformMoveTweener : Tweenner
    {
        private readonly Transform _transform;
        private readonly Vector3 _startValue;
        private readonly Vector3 _endValue;

        public TransformMoveTweener(Transform transform, Vector3 endValue, float value) : base(value)
        {
            this._transform = transform;
            _startValue = transform.position;
            this._endValue = endValue;
            _totalValue = Vector3.Distance(_startValue, endValue);
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
    public class RectTransformMoveTweener : Tweenner
    {
        private readonly RectTransform _rectTransform;
        private readonly Vector2 _startValue;
        private readonly Vector2 _endValue;

        public RectTransformMoveTweener(RectTransform rectTransform, Vector2 endValue, float value) : base(value)
        {
            this._rectTransform = rectTransform;
            _startValue = rectTransform.anchoredPosition;
            this._endValue = endValue;
            _totalValue = Vector2.Distance(_startValue, endValue);
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

    public class TransformLocalMoveTweener : Tweenner
    {
        private readonly Transform _transform;
        private readonly Vector3 _startValue;
        private readonly Vector3 _endValue;

        public TransformLocalMoveTweener(Transform transform, Vector3 endValue, float value) : base(value)
        {
            this._transform = transform;
            _startValue = transform.localPosition;
            this._endValue = endValue;
            _totalValue = Vector3.Distance(_startValue, endValue);
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

    public class TransformLocalRotateTweener : Tweenner
    {
        private readonly Transform _transform;
        private readonly Vector3 _startValue;
        private readonly Vector3 _endValue;

        public TransformLocalRotateTweener(Transform transform, Vector3 endValue, float value) : base(value)
        {
            this._transform = transform;
            _startValue = transform.localEulerAngles;
            this._endValue = endValue.AngleNear(_startValue);
            _totalValue = Quaternion.Angle(Quaternion.Euler(_startValue), Quaternion.Euler(endValue));
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

    public class TransformLocalScaleTweener : Tweenner
    {
        private readonly Transform _transform;
        private readonly Vector3 _startValue;
        private readonly Vector3 _endValue;

        public TransformLocalScaleTweener(Transform transform, Vector3 endValue, float value) : base(value)
        {
            this._transform = transform;
            _startValue = transform.localScale;
            this._endValue = endValue;
            _totalValue = Vector3.Distance(_startValue, endValue);
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

    public class TransformLocalMoveXTweener : Tweenner
    {
        private readonly Transform _transform;
        private readonly float _startValue;
        private readonly float _endValue;

        public TransformLocalMoveXTweener(Transform transform, float endValue, float value) : base(value)
        {
            this._transform = transform;
            _startValue = transform.localPosition.x;
            this._endValue = endValue;
            _totalValue = Mathf.Abs(endValue - _startValue);
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
}
