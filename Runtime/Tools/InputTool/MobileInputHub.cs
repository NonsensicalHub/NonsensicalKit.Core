using System;
using NonsensicalKit.Core;
using UnityEngine;

namespace NonsensicalKit.Tools.InputTool
{
    public partial class MobileInputHub : MonoSingleton<MobileInputHub>
    {
        public Action<Vector2> TheOneFingerPosChanged { get; set; }
        public Action<Vector2> TheOneFingerMoveChanged { get; set; }
        public Action<float> OnTwoFingerDistanceChanged { get; set; }


        public Action OnOneFingerDown { get; set; }
        public Action OnOneFingerUp { get; set; }
        public Action OnTwoFingerDown { get; set; }
        public Action OnTwoFingerUp { get; set; }

        public bool IsOneFingerHold { get; private set; }
        public bool IsTwoFingerHold { get; private set; }


        public Vector2 TheOneFingerMove { get; private set; }
        public Vector2 TheOneFingerPos { get; private set; }


        public Vector2 TwoFingerMove { get; private set; }
        public float TwoFingerDistance { get; private set; }
    }
}
