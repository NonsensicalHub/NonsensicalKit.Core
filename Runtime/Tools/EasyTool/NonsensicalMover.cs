using System;
using UnityEngine;

namespace NonsensicalKit.Tools.EasyTool
{
    /// <summary>
    /// 简单移动逻辑封装，
    /// </summary>
    public class NonsensicalMover
    {
        public Action<NonsensicalMover> OnArrived;
        public Transform Obj => _obj;

        private float Speed { set => _speed = value; }

        private float _speed;
        private readonly Transform _obj;
        private readonly bool _localMode;

        private Vector3 Current
        {
            get => _localMode ? _obj.localPosition : _obj.position;
            set
            {
                if (_localMode)
                {
                    _obj.localPosition = value;
                }
                else
                {
                    _obj.position = value;
                }
            }
        }

        private bool _moving;
        private Vector3 _target;

        public NonsensicalMover(Transform obj, float speed, bool localMode)
        {
            _obj = obj;
            _speed = speed;
            _localMode = localMode;
        }

        public void SetTarget(Vector3 target)
        {
            _moving = true;
            _target = target;
        }

        public void UpdateMove()
        {
            UpdateMove(Time.deltaTime);
        }

        public void UpdateMove(float deltaTime)
        {
            if (!_moving) return;
            var current = Current;
            var max = deltaTime * _speed;
            var distance = Vector3.Distance(current, _target);

            if (max > distance)
            {
                Current = _target;
                _moving = false;
                OnArrived?.Invoke(this);
            }
            else
            {
                var dir = _target - current;
                var next = max * dir + current;
                Current = next;
            }
        }
    }
}
