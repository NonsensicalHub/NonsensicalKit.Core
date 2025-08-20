using UnityEngine;

namespace NonsensicalKit.Tools
{
    /// <summary>
    /// 贝塞尔曲线工具类
    /// </summary>
    public static class BezierTool
    {
        #region QuadraticBezierMethod

        /// <summary>二次贝塞尔曲线，根据T值，计算贝塞尔曲线上面相对应的点</summary>
        public static Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 c, Vector3 p1)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;

            Vector3 p = uu * p0;
            p += 2 * u * t * c;
            p += tt * p1;

            return p;
        }

        /// <summary>二次贝塞尔曲线导数</summary>
        public static Vector3 CalculateQuadraticBezierDerivative(float t, Vector3 p0, Vector3 c, Vector3 p1)
        {
            Vector3 diff1 = c - p0; // First segment direction
            Vector3 diff2 = p1 - c; // Second segment direction

            return 2 * ((1 - t) * diff1 + t * diff2);
        }

        /// <summary>获取二次贝塞尔曲线点的数组</summary>
        public static Vector3[] GetQuadraticBezierList(Vector3 p0, Vector3 c, Vector3 p1, int pointNum)
        {
            Vector3[] path = new Vector3[pointNum];
            for (int i = 0; i < pointNum; i++)
            {
                float t = i / ((float)pointNum - 1);
                Vector3 pixel = CalculateQuadraticBezierPoint(t, p0,
                    c, p1);
                path[i] = pixel;
            }

            return path;
        }

        #endregion

        #region CubicBezierMethod

        /// <summary>三次贝塞尔曲线，根据T值，计算贝塞尔曲线上面相对应的点</summary>
        public static Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 c0, Vector3 c1, Vector3 p1)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float ttt = tt * t;
            float uuu = uu * u;

            Vector3 p = uuu * p0;
            p += 3 * t * uu * c0;
            p += 3 * tt * u * c1;
            p += ttt * p1;

            return p;
        }

        /// <summary>三次贝塞尔曲线导数</summary>
        public static Vector3 CalculateCubicBezierDerivative(float t, Vector3 p0, Vector3 c0, Vector3 c1, Vector3 p1)
        {
            float tt3 = t * t * 3;
            float t6 = t * 6;

            Vector3 p = (-tt3 + t6 - 3) * p0
                        + (3 * tt3 - 2 * t6 + 3) * c0
                        + (-3 * tt3 + t6) * c1
                        + tt3 * p1;

            return p;

            // 原计算公式
            // Vector3 diff1 = c0 - p0;
            // Vector3 diff2 = c1 - c0;
            // Vector3 diff3 = p1 - c1;
            //
            // float u = 1 - t;
            // float w1 = 3 * u * u; 
            // float w2 = 6 * u * t;        
            // float w3 = 3 * t * t;                
            //
            // return w1 * diff1 + w2 * diff2 + w3 * diff3;
        }


        /// <summary>获取三次贝塞尔曲线点的数组</summary>
        public static Vector3[] GetCubicBezierList(Vector3 p0, Vector3 c0, Vector3 c1, Vector3 p1, int pointNum)
        {
            Vector3[] path = new Vector3[pointNum];
            for (int i = 0; i < pointNum; i++)
            {
                float t = i / ((float)pointNum - 1);
                Vector3 pixel = CalculateCubicBezierPoint(t, p0,
                    c0, c1, p1);
                path[i] = pixel;
            }

            return path;
        }

        /// <summary>获取三次贝塞尔曲线总弧长</summary>
        public static float GetCubicBezierLength(Vector3 p0, Vector3 c0, Vector3 c1, Vector3 p1, int pointNum)
        {
            float length = 0;
            Vector3 lastPoint = p0;
            for (int i = 1; i < pointNum; i++)
            {
                float t = i / ((float)pointNum - 1);
                Vector3 pixel = CalculateCubicBezierPoint(t, p0,
                    c0, c1, p1);
                length += Vector3.Distance(lastPoint, pixel);
                lastPoint = pixel;
            }

            return length;
        }

        /// <summary>获取三次贝塞尔曲线点和切线</summary>
        public static Vector3[][] GetCubicBezierListWithSlope(Vector3 p0, Vector3 c0, Vector3 c1, Vector3 p2, int pointNum)
        {
            Vector3[] path = new Vector3[pointNum];
            Vector3[] slopes = new Vector3[pointNum];
            for (int i = 0; i < pointNum; i++)
            {
                float t = i / ((float)pointNum - 1);
                Vector3 pixel = CalculateCubicBezierPoint(t, p0,
                    c0, c1, p2);
                Vector3 slope = CalculateCubicBezierDerivative(t, p0,
                    c0, c1, p2);
                path[i] = pixel;
                slopes[i] = slope;
            }

            return new[] { path, slopes };
        }

        #endregion
    }

    /// <summary>
    /// 根据弧长比例求点时，需要计算大量的采样点，每次计算都重新采样过于浪费，应使用类对象管理
    /// </summary>
    public class QuadraticBezierCurve
    {
        public float ArcLength => _arcLength;

        private readonly Vector3 _p0;
        private readonly Vector3 _c;
        private readonly Vector3 _p2;
        private readonly Vector3[] _samples;
        private readonly float[] _sampleArcLengths;
        private readonly float _arcLength;

        public QuadraticBezierCurve(Vector3 p0, Vector3 c, Vector3 p2, int sampleCount = 100)
        {
            _p0 = p0;
            _c = c;
            _p2 = p2;
            _samples = BezierTool.GetQuadraticBezierList(p0, c, p2, sampleCount);
            _sampleArcLengths = new float[_samples.Length];
            for (int i = 1; i < _samples.Length; i++)
            {
                _sampleArcLengths[i] = _sampleArcLengths[i - 1] + Vector3.Distance(_samples[i], _samples[i - 1]);
            }

            _arcLength = _sampleArcLengths[_samples.Length - 1];
        }

        /// <summary>根据弧长比例（0到1）获取曲线上的点位</summary>
        public Vector3 GetPointByArcLengthRadio(float radio)
        {
            var t = GetTByArcLengthRadio(radio);
            return BezierTool.CalculateQuadraticBezierPoint(t, _p0, _c, _p2);
        }

        /// <summary>根据弧长比例（0到1）获取曲线上的点位和切线</summary>
        public (Vector3, Vector3) GetPointAndTangentByArcLengthRadio(float radio)
        {
            var t = GetTByArcLengthRadio(radio);

            return (BezierTool.CalculateQuadraticBezierPoint(t, _p0, _c, _p2), BezierTool.CalculateQuadraticBezierDerivative(t, _p0, _c, _p2));
        }

        /// <summary>根据弧长比例（0到1）获取曲线上的对应的t</summary>
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
    /// 所有坐标都是世界坐标
    /// </summary>
    public class CubicBezierCurve
    {
        public float ArcLength => _arcLength;
        public Vector3[] Samples => _samples;

        private readonly Vector3 _p0;
        private readonly Vector3 _c0;
        private readonly Vector3 _c1;
        private readonly Vector3 _p1;
        private readonly Vector3[] _samples;
        private readonly float[] _sampleArcLengths;
        private readonly float _arcLength;

        public CubicBezierCurve(Vector3 p0, Vector3 c0, Vector3 c1, Vector3 p1, int sampleCount = 100)
        {
            _p0 = p0;
            _c0 = c0;
            _c1 = c1;
            _p1 = p1;
            _samples = BezierTool.GetCubicBezierList(p0, c0, c1, p1, sampleCount);
            _sampleArcLengths = new float[_samples.Length];
            for (int i = 1; i < _samples.Length; i++)
            {
                _sampleArcLengths[i] = _sampleArcLengths[i - 1] + Vector3.Distance(_samples[i], _samples[i - 1]);
            }

            _arcLength = _sampleArcLengths[_samples.Length - 1];
        }

        public Vector3 GetPointByT(float t)
        {
            return BezierTool.CalculateCubicBezierPoint(t, _p0, _c0, _c1, _p1);
        }

        /// <summary>根据弧长比例（0到1）获取曲线上的点位</summary>
        public Vector3 GetPointByArcLengthRadio(float radio)
        {
            var t = GetTByArcLengthRadio(radio);
            return BezierTool.CalculateCubicBezierPoint(t, _p0, _c0, _c1, _p1);
        }

        /// <summary>根据弧长比例（0到1）获取曲线上的点位和切线</summary>
        public (Vector3 point, Vector3 derivative) GetPointAndTangentByArcLengthRadio(float radio)
        {
            var t = GetTByArcLengthRadio(radio);

            return (BezierTool.CalculateCubicBezierPoint(t, _p0, _c0, _c1, _p1), BezierTool.CalculateCubicBezierDerivative(t, _p0, _c0, _c1, _p1));
        }

        public (Vector3 point, Vector3 derivative) GetPointAndTangentByArcLength(float length)
        {
            var radio = Mathf.Clamp01(length / _arcLength);

            return GetPointAndTangentByArcLengthRadio(radio);
        }

        public (Vector3 point, Vector3 derivative) GetPointAndTangentByT(float t)
        {
            return (BezierTool.CalculateCubicBezierPoint(t, _p0, _c0, _c1, _p1), BezierTool.CalculateCubicBezierDerivative(t, _p0, _c0, _c1, _p1));
        }

        /// <summary>根据 t 获取对应位置的弧长比例（0..1）</summary>
        public float GetRadioByT(float t)
        {
            // 限制 t 范围
            t = Mathf.Clamp01(t);

            // 找到 t 对应的采样区间
            float exactIndex = t * (_samples.Length - 1);
            int index = Mathf.FloorToInt(exactIndex);
            float frac = exactIndex - index;

            if (index <= 0) return 0f;
            if (index >= _sampleArcLengths.Length - 1) return 1f;

            // 区间弧长插值
            float startLength = _sampleArcLengths[index];
            float endLength = _sampleArcLengths[index + 1];
            float lengthAtT = Mathf.Lerp(startLength, endLength, frac);

            return _arcLength > 0f ? lengthAtT / _arcLength : 0f;
        }


        public float GetTByArcLength(float length)
        {
            var radio = Mathf.Clamp01(length / _arcLength);
            return GetTByArcLengthRadio(radio);
        }

        /// <summary>根据弧长比例（0到1）获取曲线上的点位对应的t</summary>
        public float GetTByArcLengthRadio(float radio)
        {
            radio = Mathf.Clamp01(radio);
            float targetLength = radio * _arcLength;

            int low = 0;
            int high = _sampleArcLengths.Length - 1;
            while (low < high)
            {
                int mid = (low + high) / 2;
                if (_sampleArcLengths[mid] < targetLength) low = mid + 1;
                else high = mid;
            }

            int index = low;
            if (index <= 0) return 0f;
            if (index >= _sampleArcLengths.Length) return 1f;

            float segStart = _sampleArcLengths[index - 1];
            float segEnd = _sampleArcLengths[index];
            float segFrac = (segEnd - segStart) > 1e-9f ? (targetLength - segStart) / (segEnd - segStart) : 0f;

            float denom = (_samples.Length - 1);
            float tStart = (index - 1) / denom;
            float tEnd = index / denom;
            float t = Mathf.Lerp(tStart, tEnd, segFrac);

            float tLow = tStart;
            float tHigh = tEnd;
            const int maxIter = 10;
            const float lengthTol = 1e-5f;
            const float tTol = 1e-7f;

            for (int iter = 0; iter < maxIter; iter++)
            {
                // 注意：如果 Cubic 类没有实现 GetArcLengthByT，可直接
                // 复制 Quadratic 的 GetArcLengthByT 实现（基于 _sampleArcLengths 插值）
                float s = /* call an equivalent GetArcLengthByT(t) for cubic */ GetArcLengthByT(t);
                float f = s - targetLength;
                if (Mathf.Abs(f) <= lengthTol) break;

                Vector3 deriv = BezierTool.CalculateCubicBezierDerivative(t, _p0, _c0, _c1, _p1);
                float speed = deriv.magnitude;

                float tNew;
                if (speed > 1e-8f)
                {
                    tNew = t - f / speed;
                    if (float.IsNaN(tNew) || tNew <= tLow || tNew >= tHigh) tNew = 0.5f * (tLow + tHigh);
                }
                else
                {
                    tNew = 0.5f * (tLow + tHigh);
                }

                float sNew = GetArcLengthByT(Mathf.Clamp01(tNew));
                if (sNew > targetLength) tHigh = tNew;
                else tLow = tNew;

                t = Mathf.Clamp01(tNew);
                if (tHigh - tLow < tTol)
                {
                    t = 0.5f * (tLow + tHigh);
                    break;
                }
            }

            return Mathf.Clamp01(t);
        }


        /// <summary>根据 t 获取该位置的弧长</summary>
        public float GetArcLengthByT(float t)
        {
            // 限制 t 范围
            t = Mathf.Clamp01(t);

            // 找到 t 对应的采样区间
            float exactIndex = t * (_samples.Length - 1);
            int index = Mathf.FloorToInt(exactIndex);
            float frac = exactIndex - index;

            if (index <= 0) return 0f;
            if (index >= _sampleArcLengths.Length - 1) return _arcLength;

            // 区间弧长插值
            float startLength = _sampleArcLengths[index];
            float endLength = _sampleArcLengths[index + 1];
            return Mathf.Lerp(startLength, endLength, frac);
        }

        /// <summary>获取曲线上最近的点和此时的t以及距离的平方</summary>
        public (Vector3 point, float t, float distSqr) GetClosestInfo(Vector3 point, int refineSteps = 4, int refineSamples = 16)
        {
            float bestT = 0f;
            float bestDistSqr = float.PositiveInfinity;
            for (int i = 0; i < _samples.Length; i++)
            {
                Vector3 pos = _samples[i];
                float d2 = (pos - point).sqrMagnitude;
                if (d2 < bestDistSqr)
                {
                    bestDistSqr = d2;
                    bestT = i / (float)_samples.Length;
                }
            }

            float interval = 1f / _samples.Length;
            for (int r = 0; r < refineSteps; r++)
            {
                float tMin = Mathf.Max(0f, bestT - interval);
                float tMax = Mathf.Min(1f, bestT + interval);

                for (int i = 0; i <= refineSamples; i++)
                {
                    float t = Mathf.Lerp(tMin, tMax, i / (float)refineSamples);
                    Vector3 pos = BezierTool.CalculateCubicBezierPoint(bestT, _p0, _c0, _c1, _p1);
                    float d2 = (pos - point).sqrMagnitude;
                    if (d2 < bestDistSqr)
                    {
                        bestDistSqr = d2;
                        bestT = t;
                    }
                }

                interval *= 0.5f;
            }

            return (BezierTool.CalculateCubicBezierPoint(bestT, _p0, _c0, _c1, _p1), bestT, bestDistSqr);
        }
    }
}
