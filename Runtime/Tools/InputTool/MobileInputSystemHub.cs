using NonsensicalKit.Core;
using UnityEngine;

namespace NonsensicalKit.Tools.InputTool
{
    public partial class MobileInputHub
    {
        private Vector2 _oneStartPosition;
        private Vector2 _twoStartPosition;
        private Vector2 _twoDistance;
        private float _distanceDelta;
        private float _startDistance;

        private void Update()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        OnOneFingerDown?.Invoke();
                        _oneStartPosition = touch.position;

                        break;

                    case TouchPhase.Moved:
                        TheOneFingerMove = touch.position - _oneStartPosition;
                        TheOneFingerPosChanged?.Invoke(touch.position);
                        TheOneFingerMoveChanged?.Invoke(touch.position - _oneStartPosition);
                        break;

                    case TouchPhase.Stationary:
                        IsOneFingerHold = true;
                        _oneStartPosition = touch.position;
                        TheOneFingerMove = Vector2.zero;
                        break;
                    case TouchPhase.Ended:
                        OnOneFingerUp?.Invoke();
                        break;
                }

                TheOneFingerPos = touch.position;

                if (Input.touchCount > 1)
                {
                    Touch touch2 = Input.GetTouch(1);
                    switch (touch2.phase)
                    {
                        case TouchPhase.Began:
                            OnTwoFingerDown?.Invoke();
                            _twoStartPosition = touch2.position;

                            break;
                        case TouchPhase.Moved:
                            _startDistance = Vector2.Distance(_twoStartPosition, touch.position);
                            _distanceDelta = Vector2.Distance(touch2.position, touch.position) - _startDistance;

                            if (!Mathf.Approximately(TwoFingerDistance, _distanceDelta))
                            {
                                OnTwoFingerDistanceChanged?.Invoke(_distanceDelta);
                                TwoFingerDistance = _distanceDelta;
                            }

                            _twoDistance = (touch2.position - touch.position) - (_twoStartPosition - touch.position);
                            if (TwoFingerMove != _twoDistance)
                            {
                                TwoFingerMove = _twoDistance;
                            }


                            break;
                        case TouchPhase.Stationary:
                            IsTwoFingerHold = true;
                            _twoStartPosition = touch2.position;

                            break;
                        case TouchPhase.Ended:
                            OnTwoFingerUp?.Invoke();
                            break;
                        case TouchPhase.Canceled:
                            break;
                    }
                }
                else
                {
                    IsTwoFingerHold = false;
                }
            }
        }
    }
}
