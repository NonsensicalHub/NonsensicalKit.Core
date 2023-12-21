using UnityEngine;

namespace NonsensicalKit.Tools
{
    /// <summary>
    /// 贝塞尔曲线工具类
    /// </summary>
    public class BezierTool
    {
        /// <summary>
        /// 二次贝塞尔曲线，根据T值，计算贝塞尔曲线上面相对应的点
        /// </summary>
        /// <param name="t">T值</param>
        /// <param name="p0">起始点</param>
        /// <param name="p1">控制点</param>
        /// <param name="p2">目标点</param>
        /// <returns>根据T值计算出来的贝赛尔曲线点</returns>
        public static Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
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
        /// 三次贝塞尔曲线，根据T值，计算贝塞尔曲线上面相对应的点
        /// </summary>
        /// <param name="t">插量值</param>
        /// <param name="p0">起点</param>
        /// <param name="p1">控制点1</param>
        /// <param name="p2">控制点2</param>
        /// <param name="p3">终点</param>
        /// <returns></returns>
        public static Vector3 CalculateThreePowerBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
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
        public static Vector3 CalculateThreePowerBezierDerivative(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float tt = t * t;

            Vector3 p = (-3 * tt + 6 * t - 3) * p0;
            p += (9 * tt - 12 * t + 3) * p1;
            p += (-9 * tt + 6 * t) * p2;
            p += 3 * tt * p3;

            return p;
        }

        /// <summary>
        /// 获取二次贝塞尔曲线点的数组
        /// </summary>
        /// <param name="startPoint"></param>起始点
        /// <param name="controlPoint"></param>控制点
        /// <param name="endPoint"></param>目标点
        /// <param name="segmentNum">采样点的数量，包含起点终点</param>采样点的数量
        /// <returns></returns>存储贝塞尔曲线点的数组
        public static Vector3[] GetCubicBeizerList(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint, int segmentNum)
        {
            Vector3[] path = new Vector3[segmentNum];
            for (int i = 0; i < segmentNum; i++)
            {
                float t = i / ((float)segmentNum - 1);
                Vector3 pixel = CalculateCubicBezierPoint(t, startPoint,
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
        /// <returns></returns>存储贝塞尔曲线点的数组
        public static Vector3[] GetThreePowerBeizerList(Vector3 startPoint, Vector3 controlPoint1, Vector3 controlPoint2, Vector3 endPoint, int segmentNum)
        {
            Vector3[] path = new Vector3[segmentNum];
            for (int i = 0; i < segmentNum; i++)
            {
                float t = i / ((float)segmentNum - 1);
                Vector3 pixel = CalculateThreePowerBezierPoint(t, startPoint,
                    controlPoint1, controlPoint2, endPoint);
                path[i] = pixel;
            }
            return path;
        }

        /// <summary>
        /// 获取三次贝塞尔曲线点和斜率
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="controlPoint1"></param>
        /// <param name="controlPoint2"></param>
        /// <param name="endPoint"></param>
        /// <param name="segmentNum">采样点的数量，包含起点终点</param>
        /// <returns></returns>
        public static Vector3[][] GetThreePowerBeizerListWithSlope(Vector3 startPoint, Vector3 controlPoint1, Vector3 controlPoint2, Vector3 endPoint, int segmentNum)
        {
            Vector3[] path = new Vector3[segmentNum];
            Vector3[] slopes = new Vector3[segmentNum];
            for (int i = 0; i < segmentNum; i++)
            {
                float t = i / ((float)segmentNum - 1);
                Vector3 pixel = CalculateThreePowerBezierPoint(t, startPoint,
                    controlPoint1, controlPoint2, endPoint);
                Vector3 slope = CalculateThreePowerBezierDerivative(t, startPoint,
                    controlPoint1, controlPoint2, endPoint);
                path[i] = pixel;
                slopes[i] = slope;
            }
            return new Vector3[][] { path, slopes };
        }
    }
}
