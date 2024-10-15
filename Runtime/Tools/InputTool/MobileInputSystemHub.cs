using NonsensicalKit.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonsensicalKit.Tools.InputTool
{
    public partial class MobileInputHub : MonoSingleton<MobileInputHub>
    {

        private Vector2 _oneStartPosition;
        private Vector2 _twoDistance;
        private Vector2 _twoLastDistance;
        private float _distance;
        private void Update()
        {

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        OnOneFingerDowm?.Invoke();
                        _oneStartPosition = touch.position;

                        break;

                    case TouchPhase.Moved:
                        TheOneFingerMove = touch.position - _oneStartPosition;
                        TheOneFingerPosChanged?.Invoke(touch.position);
                        TheOneFingerMoveChanged?.Invoke(touch.position - _oneStartPosition);
                        break;

                    case TouchPhase.Stationary:
                        ISOneFingerHold = true;
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
                            OnTwoFingerDowm?.Invoke();
                            break;
                        case TouchPhase.Moved:
                            _distance = Vector2.Distance(touch2.position, touch.position);

                            _twoDistance = touch2.position - touch.position;
                            TwoFingerMove = _twoDistance - _twoLastDistance;
                            _twoLastDistance = _twoDistance;

                            if (TwoFingerDistance != _distance)
                            {
                                OnTwoFingerDistanceChanged?.Invoke(_distance);
                                TwoFingerDistance = _distance;
                            }

                            break;
                        case TouchPhase.Stationary:
                            ISTwoFingerHold = true;
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
                    ISTwoFingerHold = false;
                }

            }

        }
    }
}