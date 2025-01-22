using System;
using System.Collections.Generic;
using UnityEngine;

namespace NonsensicalKit.Tools
{
    /// <summary>
    /// Tansform工具类
    /// </summary>
    public static class TransformTool
    {
        /// <summary>
        /// 根据路径获取对应的子节点
        /// </summary>
        /// <param name="t"></param>
        /// <param name="s">使用'|'分割，like"1|2|3|5"</param>
        /// <returns></returns>
        public static Transform GetTransformByNodePath(Transform root, string path)
        {
            Transform crt = root;

            string[] pathNode = path.Split('|', StringSplitOptions.RemoveEmptyEntries);

            foreach (var node in pathNode)
            {
                int num;
                if (int.TryParse(node, out num))
                {
                    if (crt.childCount > num)
                    {
                        crt = crt.GetChild(num);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }

            return crt;
        }

        public static Bounds BoundingBox(this Transform root, IEnumerable<Renderer> renderers)
        {
            Quaternion qn = root.rotation;
            root.rotation = Quaternion.Euler(new Vector3(0, 0, 0));

            bool hasBounds = false;

            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

            foreach (var item in renderers)
            {
                if (hasBounds)
                {
                    bounds.Encapsulate(item.bounds);
                }
                else
                {
                    bounds = item.bounds;
                    hasBounds = true;
                }
            }

            bounds.center = root.transform.InverseTransformPoint(bounds.center);;
            root.rotation = qn;
            return bounds;
        }
        
        
        public static Bounds BoundingBoxGlobal(this Transform root, bool includeInactive = false)
        {
            bool hasBounds = false;

            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

            Renderer[] childRenderers = root.GetComponentsInChildren<Renderer>(includeInactive);

            foreach (var item in childRenderers)
            {
                if (hasBounds)
                {
                    bounds.Encapsulate(item.bounds);
                }
                else
                {
                    bounds = item.bounds;
                    hasBounds = true;
                }
            }

            return bounds;
        }

        public static Bounds BoundingBox(this Transform root, bool includeInactive = false)
        {
            Quaternion qn = root.rotation;
            root.rotation = Quaternion.Euler(new Vector3(0, 0, 0));

            bool hasBounds = false;

            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

            Renderer[] childRenderers = root.GetComponentsInChildren<Renderer>(includeInactive);

            foreach (var item in childRenderers)
            {
                if (hasBounds)
                {
                    bounds.Encapsulate(item.bounds);
                }
                else
                {
                    bounds = item.bounds;
                    hasBounds = true;
                }
            }

            root.rotation = qn;
            bounds.center = root.transform.InverseTransformPoint(bounds.center);;
            return bounds;
        }

        /// <summary>
        /// 获取安全的聚焦距离，对象长宽高差距较大时
        /// </summary>
        /// <param name="tsf"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static (Vector3 ,float)GetSafeFocus(this Transform tsf, Camera camera = null)
        {
            if (camera == null)
            {
                camera = Camera.main;
            }

            if (camera == null)
            {
                return (Vector3.zero,0f);
            }

            Bounds bounds = tsf.BoundingBoxGlobal();

            float fov = camera.fieldOfView * Mathf.Deg2Rad;
            if (Screen.width < Screen.height)
            {
                fov = Camera.VerticalToHorizontalFieldOfView(fov, camera.aspect);
            }

            float diagonal = Mathf.Sqrt(bounds.size.x * bounds.size.x + bounds.size.y * bounds.size.y + bounds.size.z * bounds.size.z);

            //tan(fov/2)=diagonal*0.5f/distance
            //distance=diagonal*0.5f/tan(fov/2)
            //safeDistance=distance* 6/5
            float distance = (0.6f * diagonal) / Mathf.Tan(fov * 0.5f);

            return (bounds.center,distance);
        }

        public static (Vector3 ,float) GetFocus(this Transform tsf, Camera camera = null)
        {
            if (camera == null)
            {
                camera = Camera.main;
            }

            if (camera == null)
            {
                return (Vector3.zero,0f);
            }

            Bounds bounds = tsf.BoundingBoxGlobal();
            var boundsHW = GetBoundsHW(camera, tsf.position, bounds);
            var xScale = 1/ boundsHW.x;
            var yScale = 1/ boundsHW.y;

            var minScale = Mathf.Min(xScale, yScale);

            var distance = boundsHW.z / minScale*1.2f;

            return (bounds.center,distance);
        }

        public static Vector3 GetBoundsHW(Camera camera, Vector3 basePos, Bounds bounds)
        {
            Vector3[] corners = new Vector3[8];
            corners[0] = basePos + bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z);
            corners[1] = basePos + bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, -bounds.extents.z);
            corners[2] = basePos + bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, bounds.extents.z);
            corners[3] = basePos + bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, -bounds.extents.z);
            corners[4] = basePos + bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, bounds.extents.z);
            corners[5] = basePos + bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, -bounds.extents.z);
            corners[6] = basePos + bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, bounds.extents.z);
            corners[7] = basePos + bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, -bounds.extents.z);

            Vector2 xMinMax = new Vector2(float.MaxValue, float.MinValue);
            Vector2 yMinMax = new Vector2(float.MaxValue, float.MinValue);
            Vector2 zMinMax = new Vector2(float.MaxValue, float.MinValue);

            for (int i = 0; i < corners.Length; i++)
            {
                var point = camera.WorldToViewportPoint(corners[i]);
                xMinMax.x = Mathf.Min(point.x, xMinMax.x);
                xMinMax.y = Mathf.Max(point.x, xMinMax.y);
                yMinMax.x = Mathf.Min(point.y, yMinMax.x);
                yMinMax.y = Mathf.Max(point.y, yMinMax.y);
                zMinMax.x = Mathf.Min(point.z, zMinMax.x);
                zMinMax.y = Mathf.Max(point.z, zMinMax.y);
            }

            return new Vector3(xMinMax.y - xMinMax.x, yMinMax.y - yMinMax.x, (zMinMax.x + zMinMax.y) * 0.5f);
        }

        public static List<Transform> GetChildsWithout<T>(this Transform tsf) where T : Component
        {
            Queue<Transform> queues = new Queue<Transform>();

            List<Transform> list = new List<Transform>();
            queues.Enqueue(tsf);

            while (queues.Count > 0)
            {
                Transform crt = queues.Dequeue();

                list.Add(crt);

                foreach (Transform child in crt)
                {
                    if (child.GetComponent<T>() == null)
                    {
                        queues.Enqueue(child);
                    }
                }
            }

            return list;
        }

        public static List<Comp> GetComponentsInChildrenWithout<Comp, Without>(this Transform tsf) where Without : Component where Comp : Component
        {
            Queue<Transform> queues = new Queue<Transform>();

            List<Comp> list = new List<Comp>();
            queues.Enqueue(tsf);

            while (queues.Count > 0)
            {
                Transform crt = queues.Dequeue();

                if (crt.TryGetComponent<Comp>(out var v))
                {
                    list.Add(v);
                }

                foreach (Transform child in crt)
                {
                    if (child.GetComponent<Without>() == null)
                    {
                        queues.Enqueue(child);
                    }
                }
            }

            return list;
        }
    }
}
