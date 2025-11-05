using System;
using UnityEngine;

namespace NonsensicalKit.Tools.EasyTool
{
    /// <summary>
    /// 简单移动逻辑封装
    /// </summary>
    public class NonsensicalMover
    {
        public Action<NonsensicalMover> OnArrived;
        public float Speed { get => _speed;  set => _speed = value; }
        public Transform Obj { get => _obj; set => _obj = value; }
        public float Distance => _distance;
        public bool Moving => _moving;

        private float _speed;
        private Transform _obj;
        private readonly bool _localMode;
        private float _distance;

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
             _distance = Vector3.Distance(current, _target);

            if (max > _distance)
            {
                _distance = 0;
                Current = _target;
                _moving = false;
                OnArrived?.Invoke(this);
            }
            else
            {
                var dir = (_target - current).normalized;
                var next = max * dir + current;
                Current = next;
            }
        }
    }
}
