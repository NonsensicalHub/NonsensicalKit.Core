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
