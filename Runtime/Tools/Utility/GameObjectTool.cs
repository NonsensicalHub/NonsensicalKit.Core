using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NonsensicalKit.Tools
{
    /// <summary>
    /// GameObject工具类
    /// </summary>
    public static class GameObjectTool
    {
        /// <summary>
        /// 根据路径获取对应的子节点
        /// </summary>
        /// <param name="root"></param>
        /// <param name="path">使用'|'分割，like"1|2|3|5"</param>
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

        public static List<Transform> GetChildren(this Transform tsf)
        {
            return  (from Transform child in tsf select child).ToList();
        }
        
        public static List<GameObject> GetChildrenGo(this Transform tsf)
        {
            return  (from Transform child in tsf select child.gameObject).ToList();
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

            bounds.center = root.transform.InverseTransformPoint(bounds.center);
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
            bounds.center = root.transform.InverseTransformPoint(bounds.center);
            return bounds;
        }

        /// <summary>
        /// 获取安全的聚焦距离，对象长宽高差距较大时
        /// </summary>
        /// <param name="tsf"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static (Vector3, float) GetSafeFocus(this Transform tsf, Camera camera = null)
        {
            if (camera == null)
            {
                camera = Camera.main;
            }

            if (camera == null)
            {
                return (Vector3.zero, 0f);
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

            return (bounds.center, distance);
        }

        public static (Vector3, float) GetFocus(this Transform tsf, Camera camera = null)
        {
            if (camera == null)
            {
                camera = Camera.main;
            }

            if (camera == null)
            {
                return (Vector3.zero, 0f);
            }

            Bounds bounds = tsf.BoundingBoxGlobal();
            var boundsHw = GetBoundsHw(camera, tsf.position, bounds);
            var xScale = 1 / boundsHw.x;
            var yScale = 1 / boundsHw.y;

            var minScale = Mathf.Min(xScale, yScale);

            var distance = boundsHw.z / minScale * 1.2f;

            return (bounds.center, distance);
        }

        public static Vector3 GetBoundsHw(Camera camera, Vector3 basePos, Bounds bounds)
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

        public static List<TComp> GetComponentsInChildrenWithout<TComp, TWithout>(this Transform tsf)
            where TWithout : Component where TComp : Component
        {
            Queue<Transform> queues = new Queue<Transform>();

            List<TComp> list = new List<TComp>();
            queues.Enqueue(tsf);

            while (queues.Count > 0)
            {
                Transform crt = queues.Dequeue();

                if (crt.TryGetComponent<TComp>(out var v))
                {
                    list.Add(v);
                }

                foreach (Transform child in crt)
                {
                    if (child.GetComponent<TWithout>() == null)
                    {
                        queues.Enqueue(child);
                    }
                }
            }

            return list;
        }
        
        public static void SetChildren(this Transform t, IEnumerable<GameObject> children)
        {
            if (children != null)
            {
                foreach (var item in children)
                {
                    item.transform.SetParent(t);
                }
            }
        }

        public static void SetChildren(this Transform t, IEnumerable<Transform> children)
        {
            if (children != null)
            {
                foreach (var item in children)
                {
                    item.SetParent(t);
                }
            }
        }

        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            T o;
            if (go.TryGetComponent<T>(out o))
            {
                return o;
            }
            else
            {
                o = go.AddComponent<T>();
                return o;
            }
        }

        public static T GetOrAddComponent<T>(this Transform t) where T : Component
        {
            return GetOrAddComponent<T>(t.gameObject);
        }

        public static T GetNearestObject<T>(this IEnumerable<T> list, Vector3 pos) where T : Component
        {
            T nearest = null;
            float nearestDistance = float.MaxValue;
            foreach (var item in list)
            {
                var distance = Vector3.Distance(pos, item.transform.position);
                if (distance < nearestDistance)
                {
                    nearest = item;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        /// <summary>
        /// 获取对象上的所有组件名称
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string[] GetComponentsName(Transform t)
        {
            string[] defaultComponent = new string[] { "GameObject", "Transform" };
            var coms = t.GetComponents<Component>();

            List<string> types = new List<string>();

            foreach (var t1 in coms)
            {
                if (t1 == null) continue;
                types.Add(t1.GetType().Name);
            }

            string[] otherComponent = types.ToArray();

            return defaultComponent.Concat(otherComponent).ToArray();
        }

        /// <summary>
        /// 销毁未激活部分
        /// </summary>
        /// <param name="tsf"></param>
        public static void DestroyInactivePart(Transform tsf)
        {
            Queue<Transform> nodes = new Queue<Transform>();
            nodes.Enqueue(tsf);

            while (nodes.Count > 0)
            {
                Transform crtNode = nodes.Dequeue();
                if (crtNode.gameObject.activeSelf == false)
                {
                    Object.Destroy(crtNode.gameObject);
                }
                else
                {
                    foreach (Transform item in crtNode)
                    {
                        nodes.Enqueue(item);
                    }
                }
            }
        }

        public static string GetNodePath(Transform root, Transform target)
        {
            if (root == target || target.IsChildOf(root) == false)
            {
                return string.Empty;
            }

            Stack<Transform> stack = new Stack<Transform>();
            while (target != root)
            {
                stack.Push(target);
                target = target.parent;
            }

            StringBuilder nodePath = new StringBuilder();

            while (stack.Count > 0)
            {
                Transform crt = stack.Pop();

                nodePath.Append("|");
                nodePath.Append(crt.GetSiblingIndex());
            }

            return nodePath.ToString();
        }

        public static Transform GetNode(Transform root, string path)
        {
            if (path == null)
            {
                return root;
            }

            string[] index = path.Split('|', StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in index)
            {
                root = root.GetChild(int.Parse(t));
            }

            return root;
        }


        /// <summary>
        /// 创建一个刚好包住所有子物体的盒子碰撞器
        /// </summary>
        /// <param name="go"></param>
        public static void CreateBoxColliderFitToChildren(GameObject go)
        {
            Quaternion qn = go.transform.rotation;
            go.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));

            bool hasBounds = false;
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

            Renderer[] childRenderers = go.transform.GetComponentsInChildren<Renderer>();

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

            BoxCollider collider;
            if ((collider = go.GetComponent<BoxCollider>()) == null)
            {
                collider = go.AddComponent<BoxCollider>();
            }

            collider.isTrigger = true;
            collider.center = bounds.center - go.transform.position;

            collider.size = new Vector3(bounds.size.x / go.transform.lossyScale.x, bounds.size.y / go.transform.lossyScale.y,
                bounds.size.z / go.transform.lossyScale.z);

            go.transform.rotation = qn;
        }
    }
}
