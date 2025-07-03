using UnityEngine;

namespace NonsensicalKit.Tools
{
    /// <summary>
    /// 贝塞尔曲线工具类
    /// </summary>
    public static class BezierTool
    {
        /// <summary>
        /// 二次贝塞尔曲线，根据T值，计算贝塞尔曲线上面相对应的点
        /// </summary>
        /// <param name="t">T值</param>
        /// <param name="p0">起始点</param>
        /// <param name="p1">控制点</param>
        /// <param name="p2">目标点</param>
        /// <returns>根据T值计算出来的贝赛尔曲线点</returns>
        public static Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;

            Vector3 p = uu * p0;
            p += 2 * u * t * p1;
            p += tt * p2;

            return p;
        }


        /// <summary>
        /// 三次贝塞尔曲线导数
        /// </summary>
        /// <param name="t"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public static Vector3 CalculateQuadraticBezierDerivative(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            Vector3 diff1 = p1 - p0; // First segment direction
            Vector3 diff2 = p2 - p1; // Second segment direction

            return 2 * ((1 - t) * diff1 + t * diff2);
        }

        /// <summary>
        /// 三次贝塞尔曲线，根据T值，计算贝塞尔曲线上面相对应的点
        /// </summary>
        /// <param name="t">插量值</param>
        /// <param name="p0">起点</param>
        /// <param name="p1">控制点1</param>
        /// <param name="p2">控制点2</param>
        /// <param name="p3">终点</param>
        /// <returns></returns>
        public static Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float ttt = tt * t;
            float uuu = uu * u;

            Vector3 p = uuu * p0;
            p += 3 * t * uu * p1;
            p += 3 * tt * u * p2;
            p += ttt * p3;

            return p;
        }

        /// <summary>
        /// 三次贝塞尔曲线导数
        /// </summary>
        /// <param name="t"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public static Vector3 CalculateCubicBezierDerivative(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float tt3 = t * t * 3;
            float t6 = t * 6;

            Vector3 p = (-tt3 + t6 - 3) * p0
                        + (3 * tt3 - 2 * t6 + 3) * p1
                        + (-3 * tt3 + t6) * p2
                        + tt3 * p3;

            return p;

            // 原计算公式
            // Vector3 diff1 = p1 - p0; // P1 to P0 vector
            // Vector3 diff2 = p2 - p1; // P2 to P1 vector
            // Vector3 diff3 = p3 - p2; // P3 to P2 vector
            //
            // float u = 1 - t;
            // float w1 = 3 * u * u; 
            // float w2 = 6 * u * t;        
            // float w3 = 3 * t * t;                
            //
            // return w1 * diff1 + w2 * diff2 + w3 * diff3;
        }

        /// <summary>
        /// 获取二次贝塞尔曲线点的数组
        /// </summary>
        /// <param name="startPoint">起始点</param>
        /// <param name="controlPoint">控制点</param>
        /// <param name="endPoint">目标点</param>
        /// <param name="segmentNum">采样点的数量，包含起点终点</param>
        /// <returns>存储贝塞尔曲线点的数组，包含首尾</returns>
        public static Vector3[] GetQuadraticBezierList(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint, int segmentNum)
        {
            Vector3[] path = new Vector3[segmentNum];
            for (int i = 0; i < segmentNum; i++)
            {
                float t = i / ((float)segmentNum - 1);
                Vector3 pixel = CalculateQuadraticBezierPoint(t, startPoint,
                    controlPoint, endPoint);
                path[i] = pixel;
            }

            return path;
        }

        /// <summary>
        /// 获取三次贝塞尔曲线点的数组
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="controlPoint1"></param>
        /// <param name="controlPoint2"></param>
        /// <param name="endPoint"></param>
        /// <param name="segmentNum">采样点的数量，包含起点终点</param>
        /// <returns>存储贝塞尔曲线点的数组，包含首尾</returns>
        public static Vector3[] GetCubicBezierList(Vector3 startPoint, Vector3 controlPoint1, Vector3 controlPoint2, Vector3 endPoint,
            int segmentNum)
        {
            Vector3[] path = new Vector3[segmentNum];
            for (int i = 0; i < segmentNum; i++)
            {
                float t = i / ((float)segmentNum - 1);
                Vector3 pixel = CalculateCubicBezierPoint(t, startPoint,
                    controlPoint1, controlPoint2, endPoint);
                path[i] = pixel;
            }

            return path;
        }

        /// <summary>
        /// 获取三次贝塞尔曲线总弧长
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="controlPoint1"></param>
        /// <param name="controlPoint2"></param>
        /// <param name="endPoint"></param>
        /// <param name="segmentNum"></param>
        /// <returns></returns>
        public static float GetCubicBezierLength(Vector3 startPoint, Vector3 controlPoint1, Vector3 controlPoint2, Vector3 endPoint,
            int segmentNum)
        {
            float length = 0;
            Vector3 lastPoint = startPoint;
            for (int i = 1; i < segmentNum; i++)
            {
                float t = i / ((float)segmentNum - 1);
                Vector3 pixel = CalculateCubicBezierPoint(t, startPoint,
                    controlPoint1, controlPoint2, endPoint);
                length += Vector3.Distance(lastPoint, pixel);
                lastPoint = pixel;
            }

            return length;
        }

        /// <summary>
        /// 获取三次贝塞尔曲线点和切线
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="controlPoint1"></param>
        /// <param name="controlPoint2"></param>
        /// <param name="endPoint"></param>
        /// <param name="segmentNum">采样点的数量，包含起点终点</param>
        /// <returns></returns>
        public static Vector3[][] GetCubicBezierListWithSlope(Vector3 startPoint, Vector3 controlPoint1, Vector3 controlPoint2, Vector3 endPoint,
            int segmentNum)
        {
            Vector3[] path = new Vector3[segmentNum];
            Vector3[] slopes = new Vector3[segmentNum];
            for (int i = 0; i < segmentNum; i++)
            {
                float t = i / ((float)segmentNum - 1);
                Vector3 pixel = CalculateCubicBezierPoint(t, startPoint,
                    controlPoint1, controlPoint2, endPoint);
                Vector3 slope = CalculateCubicBezierDerivative(t, startPoint,
                    controlPoint1, controlPoint2, endPoint);
                path[i] = pixel;
                slopes[i] = slope;
            }

            return new[] { path, slopes };
        }
    }

    /// <summary>
    /// 根据弧长比例求点时，需要计算大量的采样点，每次计算都重新采样过于浪费，应使用类对象管理
    /// </summary>
    public class QuadraticBezierCurve
    {
        public float ArcLength => _arcLength;

        private readonly Vector3 _p0;
        private readonly Vector3 _p1;
        private readonly Vector3 _p2;
        private readonly Vector3[] _samples;
        private readonly float[] _sampleArcLengths;
        private readonly float _arcLength;

        public QuadraticBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, int sampleCount = 100)
        {
            _p0 = p0;
            _p1 = p1;
            _p2 = p2;
            _samples = BezierTool.GetQuadraticBezierList(p0, p1, p2, sampleCount);
            _sampleArcLengths = new float[_samples.Length];
            for (int i = 1; i < _samples.Length; i++)
            {
                _sampleArcLengths[i] = _sampleArcLengths[i - 1] + Vector3.Distance(_samples[i], _samples[i - 1]);
            }

            _arcLength = _sampleArcLengths[_samples.Length - 1];
        }

        public Vector3 GetPointByArcLengthRadio(float radio)
        {
            var t = GetTByArcLengthRadio(radio);
            return BezierTool.CalculateQuadraticBezierPoint(t, _p0, _p1, _p2);
        }

        public (Vector3, Vector3) GetPointAndTangentByArcLengthRadio(float radio)
        {
            var t = GetTByArcLengthRadio(radio);

            return (BezierTool.CalculateQuadraticBezierPoint(t, _p0, _p1, _p2), BezierTool.CalculateQuadraticBezierDerivative(t, _p0, _p1, _p2));
        }

        public float GetTByArcLengthRadio(float radio)
        {
            float targetLength = radio * _arcLength;

            // 二分查找找到目标弧长所在的区间
            int low = 0;
            int high = _samples.Length;
            int index = 0;

            while (low < high)
            {
                index = (low + high) / 2;

                if (_sampleArcLengths[index] < targetLength)
                {
                    low = index + 1;
                }
                else
                {
                    high = index;
                }
            }

            float t;
            // 处理边界情况
            if (index <= 0)
            {
                t = 0;
            }
            else if (index >= _samples.Length)
            {
                t = 1;
            }
            else
            {
                // 线性插值计算精确的t值
                float segmentStart = _sampleArcLengths[index - 1];
                float segmentEnd = _sampleArcLengths[index];
                float segmentFraction = (targetLength - segmentStart) / (segmentEnd - segmentStart);

                float tStart = (index - 1) / (float)_samples.Length;
                float tEnd = index / (float)_samples.Length;
                t = Mathf.Lerp(tStart, tEnd, segmentFraction);
            }

            return t;
        }
    }

    /// <summary>
    /// 根据弧长比例求点时，需要计算大量的采样点，每次计算都重新采样过于浪费，应使用类对象管理
    /// </summary>
    public class CubicBezierCurve
    {
        public float ArcLength => _arcLength;

        private readonly Vector3 _p0;
        private readonly Vector3 _p1;
        private readonly Vector3 _p2;
        private readonly Vector3 _p3;
        private readonly Vector3[] _samples;
        private readonly float[] _sampleArcLengths;
        private readonly float _arcLength;

        public CubicBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int sampleCount = 100)
        {
            _p0 = p0;
            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _samples = BezierTool.GetCubicBezierList(p0, p1, p2, p3, sampleCount);
            _sampleArcLengths = new float[_samples.Length];
            for (int i = 1; i < _samples.Length; i++)
            {
                _sampleArcLengths[i] = _sampleArcLengths[i - 1] + Vector3.Distance(_samples[i], _samples[i - 1]);
            }

            _arcLength = _sampleArcLengths[_samples.Length - 1];
        }

        public Vector3 GetPointByArcLengthRadio(float radio)
        {
            var t = GetTByArcLengthRadio(radio);
            return BezierTool.CalculateCubicBezierPoint(t, _p0, _p1, _p2, _p3);
        }

        public (Vector3, Vector3) GetPointAndTangentByArcLengthRadio(float radio)
        {
            var t = GetTByArcLengthRadio(radio);

            return (BezierTool.CalculateCubicBezierPoint(t, _p0, _p1, _p2, _p3), BezierTool.CalculateCubicBezierDerivative(t, _p0, _p1, _p2, _p3));
        }

        public float GetTByArcLengthRadio(float radio)
        {
            float targetLength = radio * _arcLength;

            // 二分查找找到目标弧长所在的区间
            int low = 0;
            int high = _samples.Length;
            int index = 0;

            while (low < high)
            {
                index = (low + high) / 2;

                if (_sampleArcLengths[index] < targetLength)
                {
                    low = index + 1;
                }
                else
                {
                    high = index;
                }
            }

            float t;
            // 处理边界情况
            if (index <= 0)
            {
                t = 0;
            }
            else if (index >= _samples.Length)
            {
                t = 1;
            }
            else
            {
                // 线性插值计算精确的t值
                float segmentStart = _sampleArcLengths[index - 1];
                float segmentEnd = _sampleArcLengths[index];
                float segmentFraction = (targetLength - segmentStart) / (segmentEnd - segmentStart);

                float tStart = (index - 1) / (float)_samples.Length;
                float tEnd = index / (float)_samples.Length;
                t = Mathf.Lerp(tStart, tEnd, segmentFraction);
            }

            return t;
        }
    }
}
