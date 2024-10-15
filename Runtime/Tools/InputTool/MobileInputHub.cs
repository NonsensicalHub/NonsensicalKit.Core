using NonsensicalKit.Core;
using System;
using UnityEngine;

namespace NonsensicalKit.Tools.InputTool
{
    public partial class MobileInputHub : MonoSingleton<MobileInputHub>
    {
        public Action<Vector2> TheOneFingerPosChanged { get; set; }
        public Action<Vector2> TheOneFingerMoveChanged { get; set; }
        public Action<float> OnTwoFingerDistanceChanged { get; set; }


        public Action OnOneFingerDowm { get; set; }
        public Action OnOneFingerUp { get; set; }
        public Action OnTwoFingerDowm { get; set; }
        public Action OnTwoFingerUp { get; set; }

        public bool ISOneFingerHold { get; private set; }
        public bool ISTwoFingerHold { get; private set; }


        public Vector2 TheOneFingerMove { get; private set; }
        public Vector2 TheOneFingerPos { get; private set; }


        public Vector2 TwoFingerMove { get; private set; }
        public float TwoFingerDistance { get; private set; }
    }
}
