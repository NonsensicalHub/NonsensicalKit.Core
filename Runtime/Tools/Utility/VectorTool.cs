using System.Collections.Generic;
using NonsensicalKit.Core;
using UnityEngine;

namespace NonsensicalKit.Tools
{
    /// <summary>
    /// 向量工具类
    /// </summary>
    public static class VectorTool
    {
        public delegate bool CheckRaycastHit(RaycastHit hit);

        /// <summary>
        /// 算出RaycastAll碰撞到的所有点位中最近的点并返回
        /// 用于不适合分层（layer）时的最近射线碰撞点获取
        /// </summary>
        /// <param name="array"></param>
        /// <param name="check"></param>
        /// <returns></returns>
        public static RaycastHit? GetClosest(this RaycastHit[] array, CheckRaycastHit check)
        {
            List<RaycastHit> hits = new List<RaycastHit>();
            foreach (var item in array)
            {
                if (check(item))
                {
                    hits.Add(item);
                }
            }

            if (hits.Count == 0)
            {
                return null;
            }

            float minDistance = hits[0].distance;
            int minIndex = 0;
            for (int i = 1; i < hits.Count; i++)
            {
                if (minDistance > hits[i].distance)
                {
                    minDistance = hits[i].distance;
                    minIndex = i;
                }
            }

            return hits[minIndex];
        }

        /// <summary>
        /// 判断两个线段是否在同一条直线上
        /// </summary>
        /// <param name="line1Point1"></param>
        /// <param name="line1Point2"></param>
        /// <param name="line2Point1"></param>
        /// <param name="line2Point2"></param>
        /// <returns></returns>
        public static bool IsOnSameStraightLine(Vector3 line1Point1, Vector3 line1Point2, Vector3 line2Point1, Vector3 line2Point2)
        {
            if (!IsNear(line2Point1, line1Point1) && !IsNear(line2Point1, line1Point2) &&
                !IsEnoughParallel(line2Point1 - line1Point1, line2Point1 - line1Point2))
            {
                return false;
            }

            if (!IsNear(line2Point2, line1Point1) && !IsNear(line2Point2, line1Point2) &&
                !IsEnoughParallel(line2Point2 - line1Point1, line2Point2 - line1Point2))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 判断两个线段是否在同一条直线上
        /// </summary>
        /// <param name="line1Point1"></param>
        /// <param name="line1Point2"></param>
        /// <param name="line2Point1"></param>
        /// <param name="line2Point2"></param>
        /// <returns></returns>
        public static bool IsOnSameStraightLineScale(Vector3 line1Point1, Vector3 line1Point2, Vector3 line2Point1, Vector3 line2Point2)
        {
            line1Point1 *= 1000000;
            line1Point2 *= 1000000;
            line2Point1 *= 1000000;
            line2Point2 *= 1000000;
            if (!IsNear(line2Point1, line1Point1) && !IsNear(line2Point1, line1Point2) &&
                !IsEnoughParallel(line2Point1 - line1Point1, line2Point1 - line1Point2))
            {
                return false;
            }

            if (!IsNear(line2Point2, line1Point1) && !IsNear(line2Point2, line1Point2) &&
                !IsEnoughParallel(line2Point2 - line1Point1, line2Point2 - line1Point2))
            {
                return false;
            }

            return true;
        }

        public static float StraightLineAngle(Vector3 dir1, Vector3 dir2)
        {
            var angle = Vector3.Angle(dir1, dir2);
            if (angle > 90)
            {
                return 180 - angle;
            }

            return angle;
        }

        /// <summary>
        ///  两个向量是否足够平行
        /// </summary>
        /// <param name="dir1"></param>
        /// <param name="dir2"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static bool IsEnoughParallel(Vector3 dir1, Vector3 dir2, float threshold = 0.999998f)
        {
            //return Mathf.Abs(Vector3.Dot(dir1.normalized, dir2.normalized)) > threshold;
            return Vector3.Dot(dir1.normalized, dir2.normalized) > threshold;
        }

        public static List<Vector2> GetLineCrossUprightRect(Vector2 p1, Vector2 p2, Vector2 min, Vector2 max)
        {
            List<Vector2> result = new List<Vector2>();

            var v1 = GetHorizonCross(min.y, p1, p2, min.x, max.x);
            if (v1 != null)
            {
                result.Add((Vector2)v1);
            }

            var v2 = GetHorizonCross(max.y, p1, p2, min.x, max.x);
            if (v2 != null)
            {
                result.Add((Vector2)v2);
            }

            var v3 = GetVerticalCross(min.x, p1, p2, min.y, max.y);
            if (v3 != null)
            {
                result.Add((Vector2)v3);
            }

            var v4 = GetVerticalCross(max.x, p1, p2, min.y, max.y);
            if (v4 != null)
            {
                result.Add((Vector2)v4);
            }

            return result;
        }

        private static Vector2? GetHorizonCross(float y, Vector2 p1, Vector2 p2, float rectMinX, float rectMaxX)
        {
            if (Mathf.Approximately(p1.y, p2.y))
            {
                return null;
            }

            if (Mathf.Approximately(p1.x, p2.x))
            {
                return new Vector2(p1.x, y);
            }

            float k = (p2.y - p1.y) / (p2.x - p1.x);
            float a = p1.y - k * p1.x;
            float x = (y - a) / k;
            float minX = Mathf.Min(p1.x, p2.x);
            float maxX = Mathf.Max(p1.x, p2.x);

            if (minX < x && x < maxX && rectMinX < x && x < rectMaxX)
            {
                return new Vector2(x, y);
            }
            else
            {
                return null;
            }
        }

        private static Vector2? GetVerticalCross(float x, Vector2 p1, Vector2 p2, float rectMinY, float rectMaxY)
        {
            if (Mathf.Approximately(p1.x, p2.x))
            {
                return null;
            }

            if (Mathf.Approximately(p1.y, p2.y))
            {
                return new Vector2(x, p1.y);
            }

            float k = (p2.y - p1.y) / (p2.x - p1.x);
            float a = p1.y - k * p1.x;
            float y = k * x + a;
            float minY = Mathf.Min(p1.y, p2.y);
            float maxY = Mathf.Max(p1.y, p2.y);
            if (minY < y && y < maxY && rectMinY < y && y < rectMaxY)
            {
                return new Vector2(x, y);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取直线与矩形的所有交点
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static List<Vector2> GetLineCrossRect(Vector2 p1, Vector2 p2, Vector2 min, Vector2 max)
        {
            List<Vector2> result = new List<Vector2>();

            var v1 = GetLineSegmentCrossPoint(p1, p2, min, new Vector2(min.x, max.y));
            if (v1 != null)
            {
                result.Add((Vector2)v1);
            }

            var v2 = GetLineSegmentCrossPoint(p1, p2, new Vector2(min.x, max.y), max);
            if (v2 != null)
            {
                result.Add((Vector2)v2);
            }

            var v3 = GetLineSegmentCrossPoint(p1, p2, new Vector2(max.x, min.y), max);
            if (v3 != null)
            {
                result.Add((Vector2)v3);
            }

            var v4 = GetLineSegmentCrossPoint(p1, p2, min, new Vector2(max.x, min.y));
            if (v4 != null)
            {
                result.Add((Vector2)v4);
            }

            return result;
        }

        /// <summary>
        /// 获取二维平面两线段交点
        /// </summary>
        /// <returns></returns>
        public static Vector2? GetLineSegmentCrossPoint(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            Vector2 point = GetCrossPoint(p1, p2, p3, p4);
            if (Vector2.Dot(p1 - point, p2 - point) < 0 && Vector2.Dot(p3 - point, p4 - point) < 0)
            {
                return point;
            }
            else
            {
                Debug.Log($"p1:{p1},p2:{p2},p3:{p3},p4:{p4},point:{point}");
                return null;
            }
        }

        /// <summary>
        /// 获取二维平面两直线交点
        /// 当直线平行或垂直与水平面时结果有误，因为此时k为NaN或无穷大
        /// </summary>
        /// <returns></returns>
        public static Vector2 GetCrossPoint(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            float k1 = (p2.y - p1.y) / (p2.x - p1.x);
            float k2 = (p4.y - p3.y) / (p4.x - p3.x);
            var x = (k1 * p1.x - p1.y - k2 * p3.x + p3.y) / (k1 - k2);
            var y = k1 * (x - p1.x) + p1.y;
            return new Vector2(x, y);
        }

        /// <summary>
        /// 获取向量在轴上的投影偏移量
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetProjectionOffset(Vector3 pointOffset, Vector3 axis)
        {
            return axis * (pointOffset.magnitude * Mathf.Cos(Vector3.Angle(pointOffset, axis) / 180f * Mathf.PI));
        }

        /// <summary>
        /// 获取向量在轴上的投影的距离
        /// 当向量方向与轴方向相反时（角度大于180度），返回负数
        /// </summary>
        /// <returns></returns>
        public static float GetProjectionDistance(Vector3 pointOffset, Vector3 axis)
        {
            Vector3 offset = GetProjectionOffset(pointOffset, axis);
            float distance = offset.magnitude;
            if (Vector3.Dot(pointOffset, axis) < 0)
            {
                distance = -distance;
            }

            return distance;
        }

        public static float NatureAngle(this float value)
        {
            while (true)
            {
                switch (value)
                {
                    case > 360: value -= 360; break;
                    case < 0: value += 360; break;
                    default: return value;
                }
            }
        }

        public static float AngleNear(this float value, float targetValue)
        {
            while (true)
            {
                if (Mathf.Approximately(value, targetValue))
                {
                    return value;
                }

                if (value > targetValue)
                {
                    if (value - 360 > targetValue)
                    {
                        value -= 360;
                    }
                    else if (Mathf.Abs(value - targetValue) < Mathf.Abs(value - 360 - targetValue))
                    {
                        return value;
                    }
                    else
                    {
                        value -= 360;
                        return value;
                    }
                }
                else
                {
                    if (value + 360 < targetValue)
                    {
                        value += 360;
                    }
                    else if (Mathf.Abs(value - targetValue) < Mathf.Abs(value + 360 - targetValue))
                    {
                        return value;
                    }
                    else
                    {
                        value += 360;
                        return value;
                    }
                }
            }
        }

        public static Vector3 AngleNear(this Vector3 value, Vector3 targetValue)
        {
            while (true)
            {
                if (Mathf.Approximately(value.x, targetValue.x))
                {
                    break;
                }

                if (value.x > targetValue.x)
                {
                    if (value.x - 360 > targetValue.x)
                    {
                        value.x -= 360;
                    }
                    else if (Mathf.Abs(value.x - targetValue.x) < Mathf.Abs(value.x - 360 - targetValue.x))
                    {
                        break;
                    }
                    else
                    {
                        value.x -= 360;
                        break;
                    }
                }
                else
                {
                    if (value.x + 360 < targetValue.x)
                    {
                        value.x += 360;
                    }
                    else if (Mathf.Abs(value.x - targetValue.x) < Mathf.Abs(value.x + 360 - targetValue.x))
                    {
                        break;
                    }
                    else
                    {
                        value.x += 360;
                        break;
                    }
                }
            }

            while (true)
            {
                if (Mathf.Approximately(value.y, targetValue.y))
                {
                    break;
                }
                else if (value.y > targetValue.y)
                {
                    if (value.y - 360 > targetValue.y)
                    {
                        value.y -= 360;
                    }
                    else if (Mathf.Abs(value.y - targetValue.y) < Mathf.Abs(value.y - 360 - targetValue.y))
                    {
                        break;
                    }
                    else
                    {
                        value.y -= 360;
                        break;
                    }
                }
                else
                {
                    if (value.y + 360 < targetValue.y)
                    {
                        value.y += 360;
                    }
                    else if (Mathf.Abs(value.y - targetValue.y) < Mathf.Abs(value.y + 360 - targetValue.y))
                    {
                        break;
                    }
                    else
                    {
                        value.y += 360;
                        break;
                    }
                }
            }

            while (true)
            {
                if (value.z == targetValue.z)
                {
                    break;
                }
                else if (value.z > targetValue.z)
                {
                    if (value.z - 360 > targetValue.z)
                    {
                        value.z -= 360;
                    }
                    else if (Mathf.Abs(value.z - targetValue.z) < Mathf.Abs(value.z - 360 - targetValue.z))
                    {
                        break;
                    }
                    else
                    {
                        value.z -= 360;
                        break;
                    }
                }
                else
                {
                    if (value.z + 360 < targetValue.z)
                    {
                        value.z += 360;
                    }
                    else if (Mathf.Abs(value.z - targetValue.z) < Mathf.Abs(value.z + 360 - targetValue.z))
                    {
                        break;
                    }
                    else
                    {
                        value.z += 360;
                        break;
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// 向量除法
        /// </summary>
        /// <param name="detailed"></param>
        /// <param name="divisor"></param>
        /// <returns></returns>
        public static Vector3 Division(this Vector3 detailed, Vector3 divisor)
        {
            return new Vector3(detailed.x / divisor.x, detailed.y / divisor.y, detailed.z / divisor.z);
        }

        public static Vector3 GetNearValue(Vector3 rawVector3, int magnification)
        {
            Vector3 temp = rawVector3 / magnification;

            Vector3 temp2 = new Vector3(
                Mathf.Round(temp.x),
                Mathf.Round(temp.y),
                Mathf.Round(temp.z));

            Vector3 temp3 = temp2 * magnification;

            return temp3;
        }

        /// <summary>
        /// 求出最近的整数位置
        /// </summary>
        /// <param name="rawVector3"></param>
        /// <returns></returns>
        public static Vector3 GetNearValue(Vector3 rawVector3)
        {
            return new Vector3(
                Mathf.Round(rawVector3.x),
                Mathf.Round(rawVector3.y),
                Mathf.Round(rawVector3.z));
        }

        public static Vector3 GetNearVector3(Vector3 rawVector3, int level)
        {
            return new Vector3(
                MathTool.GetNearValue(rawVector3.x, level),
                MathTool.GetNearValue(rawVector3.y, level),
                MathTool.GetNearValue(rawVector3.z, level));
        }

        public static Vector3 GetNearVector3Use2(Vector3 rawVector3, int level)
        {
            return new Vector3(
                MathTool.GetNearValueUse2(rawVector3.x, level),
                MathTool.GetNearValueUse2(rawVector3.y, level),
                MathTool.GetNearValueUse2(rawVector3.z, level));
        }

        /// <summary>
        /// 获取目标子物体与自己的相对坐标
        /// </summary>
        /// <param name="selfCenter"></param>
        /// <param name="targetCenter"></param>
        /// <param name="targetLocalPoint"></param>
        /// <returns></returns>
        public static Vector3 LocalPosTransform(this Vector3 selfCenter, Vector3 targetCenter, Vector3 targetLocalPoint)
        {
            Vector3 targetWorldPoint = targetCenter + targetLocalPoint;
            Vector3 selfLocalPoint = targetWorldPoint - selfCenter;
            return selfLocalPoint;
        }

        /// <summary>
        /// 求两向量角度，并根据法线方向判断是哪个象限，结果和Vector3.SignedAngle一致
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        public static float GetSignedangle(Vector3 a, Vector3 b, Vector3 normal)
        {
            Vector3 cross = Vector3.Cross(a, b);

            int dir = Vector3.Dot(cross, normal) < 0 ? -1 : 1;

            float ang = Vector3.Angle(a, b);

            return ang * dir;
        }

        /// <summary>
        /// 根据面的法向量求出向量在面上的投影向量
        /// https://blog.csdn.net/weixin_41485242/article/details/95066693
        /// 点的投影使用Plane.ClosestPointOnPlane(Vector3 point)
        /// </summary>
        /// <returns></returns>
        public static Vector3 VectorProjection(Vector3 a, Vector3 normal)
        {
            return a - (Vector3.Dot(a, normal) / normal.magnitude) * normal;
        }

        /// <summary>
        /// 获取在两个向量在面上投影的角度
        /// 尚未验证
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="normal"></param>
        /// <param name="centerPoint"></param>
        /// <returns></returns>
        public static float GetProjectionAngle(Vector3 a, Vector3 b, Vector3 normal, Vector3 centerPoint)
        {
            Vector3 vector1 = VectorProjection(a, normal) - centerPoint;
            Vector3 vector2 = VectorProjection(b, normal) - centerPoint;

            return GetSignedangle(vector1, vector2, normal);
        }

        /// <summary>
        /// 获取点在直线上的垂足
        /// https://blog.csdn.net/u011435933/article/details/106375017/
        /// </summary>
        /// <param name="singlePoint"></param>
        /// <param name="linePoint1"></param>
        /// <param name="linePoint2"></param>
        /// <returns></returns>
        public static Vector3 GetFootDrop(Vector3 singlePoint, Vector3 linePoint1, Vector3 linePoint2)
        {
            float denominator = (linePoint2.x - linePoint1.x) * (linePoint2.x - linePoint1.x) +
                                (linePoint2.y - linePoint1.y) * (linePoint2.y - linePoint1.y) +
                                (linePoint2.z - linePoint1.z) * (linePoint2.z - linePoint1.z);

            if (denominator < Mathf.Epsilon)
            {
                //Debug.LogWarning("直线点重合");
                return linePoint1;
            }

            float numerator = (singlePoint.x - linePoint1.x) * (linePoint2.x - linePoint1.x)
                              + (singlePoint.y - linePoint1.y) * (linePoint2.y - linePoint1.y)
                              + (singlePoint.z - linePoint1.z) * (linePoint2.z - linePoint1.z);
            
            float k = numerator / denominator;
            Vector3 result = new Vector3(k * (linePoint2.x - linePoint1.x) + linePoint1.x, k * (linePoint2.y - linePoint1.y) + linePoint1.y,
                k * (linePoint2.z - linePoint1.z) + linePoint1.z);

            return result;
        }

        /// <summary>
        /// 获取点与直线的距离
        /// </summary>
        /// <returns></returns>
        public static float CalculatePointToLineDistance(Vector3 singlePoint, Vector3 linePoint1, Vector3 linePoint2)
        {
            Vector3 bc = linePoint2 - linePoint1;
            float magnitudeBC = bc.magnitude;

            // 处理B和C重合的情况
            if (magnitudeBC < Mathf.Epsilon)
            {
                //Debug.LogWarning("点B和点C重合，直线无效。返回点A到点B的距离。");
                return Vector3.Distance(singlePoint, linePoint1);
            }

            Vector3 ba = singlePoint - linePoint1;
            Vector3 cross = Vector3.Cross(ba, bc); // 叉积
            return cross.magnitude / magnitudeBC; // 距离
        }

        /// <summary>
        /// 获取点在轴上的投影与轴对应原点的距离
        /// </summary>
        /// <param name="singlePoint"></param>
        /// <param name="axisCenterPoint"></param>
        /// <param name="axisPositivePoint"></param>
        /// <returns></returns>
        public static float GetAxisValue(Vector3 singlePoint, Vector3 axisCenterPoint, Vector3 axisPositivePoint)
        {
            Vector3 axisPoint = GetFootDrop(singlePoint, axisCenterPoint, axisPositivePoint);

            float value = (axisPoint - axisCenterPoint).magnitude;

            if (Vector3.Angle(axisPoint - axisCenterPoint, axisPositivePoint - axisCenterPoint) > 90)
            {
                value = -value;
            }

            return value;
        }

        /// <summary>
        /// Inverts a scale vector by dividing 1 by each component
        /// </summary>
        public static Vector3 Invert(this Vector3 vec)
        {
            return new Vector3(1 / vec.x, 1 / vec.y, 1 / vec.z);
        }

        public static void SetLossyScaleOne(this Transform transform)
        {
            Vector3 lossyScale = transform.lossyScale;
            Vector3 localScale = transform.localScale;

            Vector3 llScale = Vector3.Scale(lossyScale, localScale.Invert());
            Vector3 newScale = llScale.Invert();
            transform.localScale = newScale;
        }

        public static Vector3 TransformWithPos(this Matrix4x4 matrix4X4, float x, float y, float z)
        {
            return matrix4X4 * new Vector4(x, y, z, 1);
        }

        public static Vector3 TransformWithPos(this Matrix4x4 matrix4X4, Vector3 pos)
        {
            return matrix4X4 * new Vector4(pos.x, pos.y, pos.z, 1);
        }

        public static Float3 TransformWithPos(this Matrix4x4 matrix4X4, Float3 pos)
        {
            Vector3 newPos = matrix4X4 * new Vector4(pos.F1, pos.F2, pos.F3, 1);
            return new Float3(newPos);
        }

        /// <summary>
        /// 获取点在线段上的垂足
        /// https://blog.csdn.net/u011435933/article/details/106375017/
        /// </summary>
        /// <param name="singlePoint"></param>
        /// <param name="linePoint1"></param>
        /// <param name="linePoint2"></param>
        /// <returns>垂足不在线段上时返回null</returns>
        public static Vector3? GetFootDropInLineSegment(Vector3 singlePoint, Vector3 linePoint1, Vector3 linePoint2)
        {
            float denominator = (linePoint2.x - linePoint1.x) * (linePoint2.x - linePoint1.x) +
                                (linePoint2.y - linePoint1.y) * (linePoint2.y - linePoint1.y) +
                                (linePoint2.z - linePoint1.z) * (linePoint2.z - linePoint1.z);

            if (denominator < Mathf.Epsilon)
            {
                //Debug.LogWarning("直线点重合");
                return linePoint1;
            }

            float numerator = (singlePoint.x - linePoint1.x) * (linePoint2.x - linePoint1.x)
                              + (singlePoint.y - linePoint1.y) * (linePoint2.y - linePoint1.y)
                              + (singlePoint.z - linePoint1.z) * (linePoint2.z - linePoint1.z);

            float k = numerator / denominator;
            if (k < 0 || k > 1)
            {
                return null;
            }

            Vector3 result = new Vector3(k * (linePoint2.x - linePoint1.x) + linePoint1.x, k * (linePoint2.y - linePoint1.y) + linePoint1.y,
                k * (linePoint2.z - linePoint1.z) + linePoint1.z);

            return result;
        }

        /// <summary>
        /// 获取摄像机看向地平线的视点
        /// </summary>
        /// <param name="cameraPos">摄像机位置</param>
        /// <param name="cameraForwardPos">摄像机前方位置</param>
        /// <param name="horizontal">地平线高度</param>
        /// <returns>没有看向地平线时返回null,否则返回视点的位置</returns>
        public static Vector3? GetViewPoint(Vector3 cameraPos, Vector3 cameraForwardPos, float horizontal)
        {
            if ((cameraPos.y - horizontal) * (cameraForwardPos.y - horizontal) > 0 //当摄像机的点和摄像机的前方点没有在地平线两侧时
                && Mathf.Abs(cameraPos.y) - Mathf.Abs(cameraForwardPos.y) < 0) //且当摄像机前方的位置比摄像机的位置更加远离地平线时的位置
            {
                //此时代表没有看向地面
                return null;
            }
            else
            {
                float h1 = Mathf.Abs(cameraPos.y - horizontal);
                float h2 = Mathf.Abs(cameraForwardPos.y - horizontal);
                float l1 = Vector3.Distance(cameraPos, cameraForwardPos);
                float l2 = h1 * l1 / (h1 - h2);
                return cameraForwardPos + (cameraForwardPos - cameraPos).normalized * l2;
            }
        }

        /// <summary>
        /// 获取线面交点
        /// </summary>
        /// <param name="linePoint1"></param>
        /// <param name="linePoint2"></param>
        /// <param name="plane"></param>
        /// <returns>返回null意味着线面平行且不重合</returns>
        public static Vector3? GetLinePlaneCrossPoint(Vector3 linePoint1, Vector3 linePoint2, Plane plane)
        {
            Vector3 l = linePoint2 - linePoint1;
            Vector3 p0 = -plane.normal * plane.distance;
            Vector3 l0 = linePoint1;
            Vector3 n = plane.normal;

            //直线向量和法线向量垂直时（即直线和面平行）
            if (IsVertical(l, n))
            {
                //直线与平面重合时
                if (Vector3.Dot(p0 - l0, n) == 0)
                {
                    return Vector3.Lerp(linePoint1, linePoint2, 0.5f);
                }
                else
                {
                    return null;
                }
            }

            float d = Vector3.Dot((p0 - l0), n) / Vector3.Dot(l, n);

            Vector3 t = d * l + l0;

            return t;
        }

        /// <summary>
        /// 将物体移动至UGUI中对应的位置
        /// </summary>
        /// <param name="changeTarget">需要更改位置的对象</param>
        /// <param name="targetCamera">渲染对象的相机</param>
        /// <param name="posTarget">UGUI中需放置位置的UI对象</param>
        /// <param name="renderCanvas">渲染ui的相机</param>
        /// <param name="zOffset">对象移动目标位置的深度</param>
        public static void MovePosByUGUI(Transform changeTarget, Camera targetCamera, RectTransform posTarget, RectTransform renderCanvas,
            float zOffset = 10)
        {
            float x = (posTarget.localPosition.x + renderCanvas.rect.width * 0.5f) / renderCanvas.rect.width * Screen.width;
            float y = (posTarget.localPosition.y + renderCanvas.rect.height * 0.5f) / renderCanvas.rect.height * Screen.height;

            Vector3 screenPoint = new Vector3(x, y, zOffset);

            Vector3 worldPoint = targetCamera.ScreenToWorldPoint(screenPoint);

            changeTarget.position = worldPoint;
        }

        /// <summary>
        /// 两个向量是否平行
        /// </summary>
        /// <param name="dir1"></param>
        /// <param name="dir2"></param>
        /// <returns></returns>
        public static bool IsParallel(this Vector3 dir1, Vector3 dir2)
        {
            return Vector3.Cross(dir1, dir2) == Vector3.zero;
        }

        /// <summary>
        /// 是否垂直
        /// </summary>
        /// <param name="dir1"></param>
        /// <param name="dir2"></param>
        /// <returns></returns>
        public static bool IsVertical(this Vector3 dir1, Vector3 dir2)
        {
            return Vector3.Dot(dir1, dir2) == 0;
        }

        public static float GetSkewLinesDistance(Vector3 dir1, Vector3 dir2, Vector3 point1, Vector3 point2)
        {
            return CalculateLineToLineDistance(point1, dir1, point2, dir2);
        }

        /// <summary>
        /// 获取异面直线的距离
        /// </summary>
        /// <returns></returns>
        public static float CalculateLineToLineDistance(
            Vector3 point1, Vector3 dir1, Vector3 point2, Vector3 dir2)
        {
            // 处理方向向量为零的情况
            if (dir1.sqrMagnitude < Mathf.Epsilon || dir2.sqrMagnitude < Mathf.Epsilon)
            {
                //Debug.LogWarning("方向向量长度为零，使用点对点距离计算");
                return Vector3.Distance(point1, point2);
            }

            // 计算方向向量的叉积
            Vector3 crossDir = Vector3.Cross(dir1, dir2);
            float crossMagnitude = crossDir.magnitude;

            // 判断是否平行
            if (crossMagnitude < Mathf.Epsilon)
            {
                // 平行直线：使用点P到直线2的距离
                return CalculatePointToLineDistance(point1, point2, dir2);
            }

            // 异面直线：使用点积公式
            Vector3 link = point2 - point1;
            float dotProduct = Mathf.Abs(Vector3.Dot(link, crossDir));
            return dotProduct / crossMagnitude;
        }

        /// <summary>
        /// 求两个向量的公垂线，当两个向量平行时，随机返回一个与这两个向量垂直的向量
        /// </summary>
        /// <param name="dir1"></param>
        /// <param name="dir2"></param>
        /// <returns></returns>
        public static Vector3 GetCommonVerticalLine(Vector3 dir1, Vector3 dir2)
        {
            Vector3 normal = Vector3.Cross(dir1, dir2);


            //当两个向量平行时，Vector3.Cross求出来的公垂线为Vector3.Zero
            if (normal == Vector3.zero)
            {
                //随意取一个向量求公垂线（这里取Vector3.up）
                normal = Vector3.Cross(dir1, Vector3.up);

                //当仍然与随意取的向量平行时
                if (normal == Vector3.zero)
                {
                    //拿一个与之前向量不平行的向量求公垂线（这里取Vector3.forward）
                    normal = Vector3.Cross(dir1, Vector3.forward);
                }
            }

            return normal.normalized;
        }

        /// <summary>
        /// 求两个向量的公垂线，当两个向量平行时，随机返回一个与这两个向量垂直的向量
        /// </summary>
        /// <param name="dir1"></param>
        /// <param name="dir2"></param>
        /// <returns></returns>
        public static Vector3 GetCommonVerticalLineScale(Vector3 dir1, Vector3 dir2)
        {
            dir1 *= 100;
            dir2 *= 100;

            Vector3 normal = Vector3.Cross(dir1, dir2);


            //当两个向量平行时，Vector3.Cross求出来的公垂线为Vector3.Zero
            if (normal == Vector3.zero)
            {
                //随意取一个向量求公垂线（这里取Vector3.up）
                normal = Vector3.Cross(dir1, Vector3.up);

                //当仍然与随意取的向量平行时
                if (normal == Vector3.zero)
                {
                    //拿一个与之前向量不平行的向量求公垂线（这里取Vector3.forward）
                    normal = Vector3.Cross(dir1, Vector3.forward);
                }
            }

            return normal.normalized;
        }

        /// <summary>
        /// 根据子物体的旋转差值旋转
        /// </summary>
        /// <param name="rotateObject">需要旋转的对象</param>
        /// <param name="crtElementPoints">子物体面上不在一条直线上的三个点的数组</param>
        /// <param name="targetElementPoints">目标物体面上不在一条直线上的三个点的数组</param>
        public static void RotateByChildren(this Transform rotateObject, Vector3[] crtElementPoints, Vector3[] targetElementPoints)
        {
            Vector3 dir1 = crtElementPoints[0] - crtElementPoints[1];
            Vector3 dir2 = crtElementPoints[1] - crtElementPoints[2];
            Vector3 normal1 = GetCommonVerticalLine(dir1, dir2);

            Vector3 dir3 = targetElementPoints[0] - targetElementPoints[1];
            Vector3 dir4 = targetElementPoints[1] - targetElementPoints[2];
            Vector3 normal2 = GetCommonVerticalLine(dir3, dir4);

            float angle = Vector3.Angle(normal1, normal2);
            Vector3 axis = GetCommonVerticalLine(normal1, normal2);

            rotateObject.rotation = Quaternion.AngleAxis(angle, axis) * rotateObject.transform.rotation;
        }

        /// <summary>
        /// 获取鼠标位置的世界坐标（深度由所选物体决定）
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetWorldPos(Transform target)
        {
            //获取需要移动物体的世界转屏幕坐标
            Vector3 screenPos = Camera.main.WorldToScreenPoint(target.transform.position);
            //获取鼠标位置
            Vector3 mousePos = Input.mousePosition;
            //因为鼠标只有X，Y轴，所以要赋予给鼠标Z轴
            mousePos.z = screenPos.z;
            //把鼠标的屏幕坐标转换成世界坐标
            return Camera.main.ScreenToWorldPoint(mousePos);
        }

        /// <summary>
        /// 获取圆内随机一点
        /// </summary>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static Vector2 GetCirclePoint(float radius)
        {
            //随机获取弧度
            float radin = RandomTool.GetRandomFloat(2 * Mathf.PI);
            float distance = RandomTool.GetRandomFloat(radius);
            float x = distance * Mathf.Cos(radin);
            float y = distance * Mathf.Sin(radin);
            Vector2 endPoint = new Vector2(x, y);
            return endPoint;
        }

        /// <summary>
        /// 获取射线和三角形的交点
        /// 推导过程：https://www.cnblogs.com/graphics/archive/2010/08/09/1795348.html
        /// </summary>
        /// <returns></returns>
        public static Vector3? GetRayTriangleCrossPoint(Vector3 rayOrigin, Vector3 rayUnitVector, Vector3 trianglePoint1, Vector3 trianglePoint2,
            Vector3 trianglePoint3)
        {
            Vector3 e1 = trianglePoint2 - trianglePoint1;
            Vector3 e2 = trianglePoint3 - trianglePoint1;

            Vector3 p = Vector3.Cross(rayUnitVector, e2);

            Vector3 T;
            float det = Vector3.Dot(p, e1);
            if (det > 0)
            {
                T = rayOrigin - trianglePoint1;
            }
            else
            {
                det = -det;
                T = trianglePoint1 - rayOrigin;
            }

            //射线在面上
            if (det == 0)
            {
                //TODO:返回射线与任一边的交点
                return null;
            }


            Vector3 q = Vector3.Cross(T, e1);

            float t = Vector3.Dot(q, e2) / det;

            if (t < 0)
            {
                return null;
            }

            float u = Vector3.Dot(p, T) / det;

            if (u < 0)
            {
                return null;
            }

            float v = Vector3.Dot(q, rayUnitVector) / det;

            if (v < 0)
            {
                return null;
            }

            if ((u + v) > 1)
            {
                return null;
            }

            return rayOrigin + rayUnitVector * t;
        }

        /// <summary>
        /// 判断两个三维向量是否足够接近
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static bool IsNear(Vector3 vec1, Vector3 vec2)
        {
            if (MathTool.IsNear(vec1.x, vec2.x) == false)
            {
                return false;
            }

            if (MathTool.IsNear(vec1.y, vec2.y) == false)
            {
                return false;
            }

            if (MathTool.IsNear(vec1.z, vec2.z) == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 按照（x,y,z）的顺序比较三维坐标
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        private static int CompareVector3(Vector3 v1, Vector3 v2)
        {
            if (v1.x > v2.x)
            {
                return 1;
            }
            else if (v1.x < v2.x)
            {
                return -1;
            }
            else
            {
                if (v1.y > v2.y)
                {
                    return 1;
                }
                else if (v1.y < v2.y)
                {
                    return -1;
                }
                else
                {
                    if (v1.z > v2.z)
                    {
                        return 1;
                    }
                    else if (v1.z < v2.z)
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }

        /// <summary>
        /// 对三维坐标链表进行排序
        /// </summary>
        /// <param name="rawData"></param>
        public static void SortList(List<Vector3> rawData)
        {
            for (int i = 0; i < rawData.Count - 1; i++)
            {
                for (int j = 0; j < rawData.Count - 1 - i; j++)
                {
                    if (CompareVector3(rawData[j], rawData[j + 1]) == 1)
                    {
                        (rawData[j], rawData[j + 1]) = (rawData[j + 1], rawData[j]);
                    }
                }
            }
        }

        public static Vector3 GetEightPos(int pos, float size)
        {
            switch (pos)
            {
                case 0: return new Vector3(size, size, size);
                case 1: return new Vector3(size, size, -size);
                case 2: return new Vector3(size, -size, size);
                case 3: return new Vector3(size, -size, -size);
                case 4: return new Vector3(-size, size, size);
                case 5: return new Vector3(-size, size, -size);
                case 6: return new Vector3(-size, -size, size);
                case 7: return new Vector3(-size, -size, -size);
                default: return Vector3.zero;
            }
        }

        public static Float3 GetEightPosF3(int pos, float size)
        {
            switch (pos)
            {
                case 0: return new Float3(size, size, size);
                case 1: return new Float3(size, size, -size);
                case 2: return new Float3(size, -size, size);
                case 3: return new Float3(size, -size, -size);
                case 4: return new Float3(-size, size, size);
                case 5: return new Float3(-size, size, -size);
                case 6: return new Float3(-size, -size, size);
                case 7: return new Float3(-size, -size, -size);
                default: return Float3.Zero;
            }
        }

        //Returns the rotated Vector3 using a Quaterion
        public static Vector3 RotateAroundPivot(this Vector3 point, Vector3 pivot, Quaternion angle)
        {
            return angle * (point - pivot) + pivot;
        }

        //Returns the rotated Vector3 using Euler
        public static Vector3 RotateAroundPivot(this Vector3 point, Vector3 pivot, Vector3 euler)
        {
            return RotateAroundPivot(point, pivot, Quaternion.Euler(euler));
        }

        //Rotates the Transform's position using a Quaterion
        public static void RotateAroundPivot(this Transform me, Vector3 pivot, Quaternion angle)
        {
            me.position = me.position.RotateAroundPivot(pivot, angle);
        }

        //Rotates the Transform's position using Euler
        public static void RotateAroundPivot(this Transform me, Vector3 pivot, Vector3 euler)
        {
            me.position = me.position.RotateAroundPivot(pivot, Quaternion.Euler(euler));
        }
    }
}
