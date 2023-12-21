using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NonsensicalKit.Tools
{
    /// <summary>
    /// GameObject工具类
    /// </summary>
    public static class GameObjectTool
    {
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

            for (int i = 0; i < coms.Length; i++)
            {
                if (coms[i] == null) continue;
                types.Add(coms[i].GetType().Name);
            }

            string[] otherComponent = types.ToArray();

            return defaultComponent.Concat(otherComponent).ToArray();
        }

        /// <summary>
        /// 销毁未激活部分
        /// </summary>
        /// <param name="tsf"></param>
        public static void DestroyUnactivePart(Transform tsf)
        {
            Queue<Transform> nodes = new Queue<Transform>();
            nodes.Enqueue(tsf);

            while (nodes.Count > 0)
            {
                Transform crtNode = nodes.Dequeue();
                if (crtNode.gameObject.activeSelf == false)
                {
                    GameObject.Destroy(crtNode.gameObject);
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
            for (int i = 0; i < index.Length; i++)
            {
                root = root.GetChild(int.Parse(index[i]));
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

            collider.size = new Vector3(bounds.size.x / go.transform.lossyScale.x, bounds.size.y / go.transform.lossyScale.y, bounds.size.z / go.transform.lossyScale.z);

            go.transform.rotation = qn;
        }
    }
}
