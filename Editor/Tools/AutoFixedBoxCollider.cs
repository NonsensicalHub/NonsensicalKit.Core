using System.Collections.Generic;
using NonsensicalKit.Tools;
using UnityEditor;
using UnityEngine;

namespace NonsensicalKit.Core.Editor.Tools
{
    /// <summary>
    /// 添加或修改成自适应大小的盒子碰撞体
    /// </summary>
    public class AutoFixedBoxCollider : EditorWindow
    {
        private static string _showText; //显示给用户的文本

        [MenuItem("NonsensicalKit/批量修改/清除无实体对象盒子碰撞体")]
        private static void ClearBoxCollider()
        {
            if (Selection.gameObjects.Length == 0)
            {
                Debug.Log("未选中任何对象");
            }
            else
            {
                foreach (var t in Selection.gameObjects)
                {
                    Clear(t.transform);
                }

                Debug.Log("盒子碰撞器自适应完成");
            }
        }

        [MenuItem("NonsensicalKit/批量修改/自动添加自适应大小盒子碰撞器包括子物体")]
        private static void AddComponentToCrtTargetWithChilds()
        {
            if (Selection.gameObjects.Length == 0)
            {
                Debug.Log("未选中任何对象");
            }
            else
            {
                foreach (var t in Selection.gameObjects)
                {
                    AutoFixed(t.transform);
                }

                Debug.Log("盒子碰撞器自适应完成");
            }
        }

        [MenuItem("NonsensicalKit/批量修改/自动添加自适应大小盒子碰撞器")]
        private static void AddComponentToCrtTarget()
        {
            if (Selection.gameObjects.Length == 0)
            {
                Debug.Log("未选中任何对象");
            }
            else
            {
                foreach (var t in Selection.gameObjects)
                {
                    FitToChildren(t);
                }

                Debug.Log("盒子碰撞器自适应完成");
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(_showText, MessageType.Info);
        }

        private static void Clear(Transform tran)
        {
            var ts = tran.GetComponentsInChildren<BoxCollider>();

            foreach (var item in ts)
            {
                if (item.GetComponent<MeshRenderer>() == false)
                {
                    DestroyImmediate(item);
                }
            }
        }

        private static void AutoFixed(Transform tran)
        {
            Stack<Transform> crtTargets = new Stack<Transform>();
            crtTargets.Push(tran);

            while (crtTargets.Count > 0)
            {
                Transform crt = crtTargets.Pop();

                Renderer renderer = crt.gameObject.transform.GetComponent<Renderer>();

                if (renderer == null)
                {
                    FitToChildren(crt.gameObject);
                }
                else
                {
                    FitCollider(crt.gameObject);
                }

                foreach (Transform item in crt)
                {
                    crtTargets.Push(item);
                }
            }
        }

        /// <summary>
        /// 添加盒子碰撞器并且自适应其大小
        /// </summary>
        /// <param name="go"></param>
        private static void FitCollider(GameObject go)
        {
            Renderer renderer = go.transform.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            BoxCollider bc;
            if ((bc = go.GetComponent<BoxCollider>()) == null)
            {
                bc = go.AddComponent<BoxCollider>();

                bc.isTrigger = true;
            }

            Quaternion qn = go.transform.rotation;
            go.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            Bounds bounds = renderer.bounds;
            go.transform.rotation = qn;

            bc.size = new Vector3(bounds.size.x / go.transform.lossyScale.x, bounds.size.y / go.transform.lossyScale.y,
                bounds.size.z / go.transform.lossyScale.z);
        }


        /// <summary>
        /// 创建一个刚好包住所有子物体的盒子碰撞器
        /// </summary>
        /// <param name="go"></param>
        private static void FitToChildren(GameObject go)
        {
            Quaternion qn = go.transform.rotation;
            go.transform.rotation = Quaternion.identity;

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
                collider.isTrigger = true;
            }

            collider.center = go.transform.InverseTransformPoint(bounds.center);

            collider.size = bounds.size.Division(go.transform.lossyScale);

            EditorUtility.SetDirty(collider);
            go.transform.rotation = qn;
        }
    }
}
