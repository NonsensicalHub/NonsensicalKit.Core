using System;
using UnityEngine;

namespace NonsensicalKit.Tools
{
    /// <summary>
    /// 贝塞尔曲线工具类
    /// </summary>
    public static class BezierTool
    {
        private static int ValidatePointCount(int pointCount, string paramName)
        {
            if (pointCount < 2)
            {
                throw new ArgumentOutOfRangeException(paramName, pointCount,
                    "pointCount must be greater than or equal to 2.");
            }

            return pointCount;
        }

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
            Vector3 diff1 = c - p0;
            Vector3 diff2 = p1 - c;

            return 2 * ((1 - t) * diff1 + t * diff2);
        }

        /// <summary>获取二次贝塞尔曲线点的数组</summary>
        public static Vector3[] GetQuadraticBezierList(Vector3 p0, Vector3 c, Vector3 p1, int pointNum)
        {
            int pointCount = ValidatePointCount(pointNum, nameof(pointNum));
            Vector3[] path = new Vector3[pointCount];
            float denominator = pointCount - 1f;
            for (int i = 0; i < pointCount; i++)
            {
                float t = i / denominator;
                path[i] = CalculateQuadraticBezierPoint(t, p0, c, p1);
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
            int pointCount = ValidatePointCount(pointNum, nameof(pointNum));
            Vector3[] path = new Vector3[pointCount];
            float denominator = pointCount - 1f;
            for (int i = 0; i < pointCount; i++)
            {
                float t = i / denominator;
                path[i] = CalculateCubicBezierPoint(t, p0, c0, c1, p1);
            }

            return path;
        }

        /// <summary>获取三次贝塞尔曲线总弧长</summary>
        public static float GetCubicBezierLength(Vector3 p0, Vector3 c0, Vector3 c1, Vector3 p1, int pointNum)
        {
            int pointCount = ValidatePointCount(pointNum, nameof(pointNum));
            float length = 0;
            Vector3 lastPoint = p0;
            float denominator = pointCount - 1f;
            for (int i = 1; i < pointCount; i++)
            {
                float t = i / denominator;
                Vector3 point = CalculateCubicBezierPoint(t, p0, c0, c1, p1);
                length += Vector3.Distance(lastPoint, point);
                lastPoint = point;
            }

            return length;
        }

        /// <summary>获取三次贝塞尔曲线点和切线</summary>
        public static Vector3[][] GetCubicBezierListWithSlope(Vector3 p0, Vector3 c0, Vector3 c1, Vector3 p1,
            int pointNum)
        {
            int pointCount = ValidatePointCount(pointNum, nameof(pointNum));
            Vector3[] path = new Vector3[pointCount];
            Vector3[] slopes = new Vector3[pointCount];
            float denominator = pointCount - 1f;
            for (int i = 0; i < pointCount; i++)
            {
                float t = i / denominator;
                path[i] = CalculateCubicBezierPoint(t, p0, c0, c1, p1);
                slopes[i] = CalculateCubicBezierDerivative(t, p0, c0, c1, p1);
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
            if (sampleCount < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleCount), sampleCount,
                    "sampleCount must be greater than or equal to 2.");
            }

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
        [Obsolete("Use GetPointByArcLengthRatio instead.")]
        public Vector3 GetPointByArcLengthRadio(float radio)
        {
            // 兼容旧命名，内部统一转发到 Ratio 语义接口。
            return GetPointByArcLengthRatio(radio);
        }

        /// <summary>根据弧长比例（0到1）获取曲线上的点位</summary>
        public Vector3 GetPointByArcLengthRatio(float ratio)
        {
            float t = GetTByArcLengthRatio(ratio);
            return BezierTool.CalculateQuadraticBezierPoint(t, _p0, _c, _p2);
        }

        /// <summary>根据弧长比例（0到1）获取曲线上的点位和切线</summary>
        [Obsolete("Use GetPointAndTangentByArcLengthRatio instead.")]
        public (Vector3, Vector3) GetPointAndTangentByArcLengthRadio(float radio)
        {
            return GetPointAndTangentByArcLengthRatio(radio);
        }

        /// <summary>根据弧长比例（0到1）获取曲线上的点位和切线</summary>
        public (Vector3 point, Vector3 derivative) GetPointAndTangentByArcLengthRatio(float ratio)
        {
            float t = GetTByArcLengthRatio(ratio);
            return (BezierTool.CalculateQuadraticBezierPoint(t, _p0, _c, _p2),
                BezierTool.CalculateQuadraticBezierDerivative(t, _p0, _c, _p2));
        }

        /// <summary>根据弧长比例（0到1）获取曲线上的对应的t</summary>
        [Obsolete("Use GetTByArcLengthRatio instead.")]
        public float GetTByArcLengthRadio(float radio)
        {
            return GetTByArcLengthRatio(radio);
        }

        /// <summary>根据弧长比例（0到1）获取曲线上的对应的t</summary>
        public float GetTByArcLengthRatio(float ratio)
        {
            ratio = Mathf.Clamp01(ratio);
            if (_arcLength <= Mathf.Epsilon)
            {
                return 0f;
            }

            float targetLength = ratio * _arcLength;

            // 二分查找目标弧长对应的采样区间。
            int low = 0;
            int high = _sampleArcLengths.Length - 1;
            while (low < high)
            {
                int mid = (low + high) / 2;
                if (_sampleArcLengths[mid] < targetLength)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid;
                }
            }

            int index = low;
            if (index <= 0)
            {
                return 0f;
            }

            if (index >= _sampleArcLengths.Length)
            {
                return 1f;
            }

            // 基于采样弧长做一次线性反插值，得到足够稳定且高效的近似 t。
            float segmentStart = _sampleArcLengths[index - 1];
            float segmentEnd = _sampleArcLengths[index];
            float segmentLength = segmentEnd - segmentStart;
            float segmentFraction = segmentLength > Mathf.Epsilon ? (targetLength - segmentStart) / segmentLength : 0f;

            float denominator = _samples.Length - 1f;
            float tStart = (index - 1) / denominator;
            float tEnd = index / denominator;
            return Mathf.Lerp(tStart, tEnd, segmentFraction);
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
            if (sampleCount < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleCount), sampleCount,
                    "sampleCount must be greater than or equal to 2.");
            }

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
        [Obsolete("Use GetPointByArcLengthRatio instead.")]
        public Vector3 GetPointByArcLengthRadio(float radio)
        {
            return GetPointByArcLengthRatio(radio);
        }

        /// <summary>根据弧长比例（0到1）获取曲线上的点位</summary>
        public Vector3 GetPointByArcLengthRatio(float ratio)
        {
            float t = GetTByArcLengthRatio(ratio);
            return BezierTool.CalculateCubicBezierPoint(t, _p0, _c0, _c1, _p1);
        }

        /// <summary>根据弧长比例（0到1）获取曲线上的点位和切线</summary>
        [Obsolete("Use GetPointAndTangentByArcLengthRatio instead.")]
        public (Vector3 point, Vector3 derivative) GetPointAndTangentByArcLengthRadio(float radio)
        {
            return GetPointAndTangentByArcLengthRatio(radio);
        }

        /// <summary>根据弧长比例（0到1）获取曲线上的点位和切线</summary>
        public (Vector3 point, Vector3 derivative) GetPointAndTangentByArcLengthRatio(float ratio)
        {
            float t = GetTByArcLengthRatio(ratio);
            return (BezierTool.CalculateCubicBezierPoint(t, _p0, _c0, _c1, _p1),
                BezierTool.CalculateCubicBezierDerivative(t, _p0, _c0, _c1, _p1));
        }

        public (Vector3 point, Vector3 derivative) GetPointAndTangentByArcLength(float length)
        {
            float ratio = _arcLength > Mathf.Epsilon ? Mathf.Clamp01(length / _arcLength) : 0f;
            return GetPointAndTangentByArcLengthRatio(ratio);
        }

        public (Vector3 point, Vector3 derivative) GetPointAndTangentByT(float t)
        {
            return (BezierTool.CalculateCubicBezierPoint(t, _p0, _c0, _c1, _p1),
                BezierTool.CalculateCubicBezierDerivative(t, _p0, _c0, _c1, _p1));
        }

        /// <summary>根据 t 获取对应位置的弧长比例（0..1）</summary>
        [Obsolete("Use GetRatioByT instead.")]
        public float GetRadioByT(float t)
        {
            return GetRatioByT(t);
        }

        /// <summary>根据 t 获取对应位置的弧长比例（0..1）</summary>
        public float GetRatioByT(float t)
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
            float ratio = _arcLength > Mathf.Epsilon ? Mathf.Clamp01(length / _arcLength) : 0f;
            return GetTByArcLengthRatio(ratio);
        }

        /// <summary>根据弧长比例（0到1）获取曲线上的点位对应的t</summary>
        [Obsolete("Use GetTByArcLengthRatio instead.")]
        public float GetTByArcLengthRadio(float radio)
        {
            return GetTByArcLengthRatio(radio);
        }

        /// <summary>根据弧长比例（0到1）获取曲线上的点位对应的t</summary>
        public float GetTByArcLengthRatio(float ratio)
        {
            ratio = Mathf.Clamp01(ratio);
            if (_arcLength <= Mathf.Epsilon)
            {
                return 0f;
            }

            float targetLength = ratio * _arcLength;

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
                float s = GetArcLengthByT(t);
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
        public (Vector3 point, float t, float distSqr) GetClosestInfo(Vector3 point, int refineSteps = 4,
            int refineSamples = 16)
        {
            if (refineSteps < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(refineSteps), refineSteps,
                    "refineSteps must be greater than or equal to 0.");
            }

            if (refineSamples < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(refineSamples), refineSamples,
                    "refineSamples must be greater than or equal to 1.");
            }

            float bestT = 0f;
            float bestDistSqr = float.PositiveInfinity;
            float denominator = _samples.Length - 1f;
            for (int i = 0; i < _samples.Length; i++)
            {
                Vector3 pos = _samples[i];
                float d2 = (pos - point).sqrMagnitude;
                if (d2 < bestDistSqr)
                {
                    bestDistSqr = d2;
                    bestT = i / denominator;
                }
            }

            float interval = 1f / denominator;
            for (int r = 0; r < refineSteps; r++)
            {
                float tMin = Mathf.Max(0f, bestT - interval);
                float tMax = Mathf.Min(1f, bestT + interval);

                for (int i = 0; i <= refineSamples; i++)
                {
                    float t = Mathf.Lerp(tMin, tMax, i / (float)refineSamples);
                    Vector3 pos = BezierTool.CalculateCubicBezierPoint(t, _p0, _c0, _c1, _p1);
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
