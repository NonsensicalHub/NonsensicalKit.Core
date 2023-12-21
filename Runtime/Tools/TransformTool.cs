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

            string[] pathNode = path.Split('|', System.StringSplitOptions.RemoveEmptyEntries);

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

        public static Bounds CalculateBox(this Transform root, IEnumerable<Renderer> renderers)
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

            root.rotation = qn;
            bounds.center -= root.position;
            return bounds;
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
