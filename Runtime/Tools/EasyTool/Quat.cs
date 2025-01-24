using UnityEngine;

namespace NonsensicalKit.Tools.EasyTool
{
    /// <summary>
    /// 自定义Quaternion，用于求四元数差值
    /// https://stackoverflow.com/questions/22157435/difference-between-the-two-quaternions
    /// </summary>
    public struct Quat
    {
        private float _x;
        private float _y;
        private float _z;
        private float _w;

        public Quat(Quaternion q)
        {
            _x = q.x;
            _y = q.y;
            _z = q.z;
            _w = q.w;
        }

        public Quat(float x, float y, float z, float w)
        {
            this._x = x;
            this._y = y;
            this._z = z;
            this._w = w;
        }

        public Quaternion ToQuaternion()
        {
            return new Quaternion(_x, _y, _z, _w);
        }

        public static Quaternion Dif(Quaternion q1, Quaternion q2)
        {
            var v2 = q1.x * q1.x + q1.y * q1.y + q1.z * q1.z + q1.w + q1.w;
            var v3 = new Quaternion(-q1.x / v2, -q1.y / v2, -q1.z / v2, q1.w / v2);
            var v4 = v3 * q2;
            return v4;
        }

        public static Quaternion Diff(Quaternion q1, Quaternion q2)
        {
            Quat dif = Diff(new Quat(q1), new Quat(q2));
            return dif.ToQuaternion();
        }

        public static Quat Diff(Quat a, Quat b)
        {
            return a.Inverse() * b;
        }

        public Quat Inverse()
        {
            Quat q = this;
            q = q.Conjugate();
            return q / Dot(this, this);
        }

        public Quat Conjugate()
        {
            Quat q;
            q._x = -this._x;
            q._y = -this._y;
            q._z = -this._z;
            q._w = this._w;

            return q;
        }

        public static float Dot(Quat q1, Quat q2)
        {
            return q1._x * q2._x + q1._y * q2._y + q1._z * q2._z + q1._w * q2._w;
        }

        public static Quat operator *(Quat q1, Quat q2)
        {
            Quat qu = new Quat
            {
                _x = q1._w * q2._x + q1._x * q2._w + q1._y * q2._z - q1._z * q2._y,
                _y = q1._w * q2._y + q1._y * q2._w + q1._z * q2._x - q1._x * q2._z,
                _z = q1._w * q2._z + q1._z * q2._w + q1._x * q2._y - q1._y * q2._x,
                _w = q1._w * q2._w - q1._x * q2._x - q1._y * q2._y - q1._z * q2._z
            };
            return qu;
        }

        public static Quat operator /(Quat q, float s)
        {
            return new Quat(q._x / s, q._y / s, q._z / s, q._w / s);
        }
    }
}
