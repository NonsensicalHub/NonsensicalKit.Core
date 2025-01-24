using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NonsensicalKit.Core.Editor.Tools
{
    public class AddMeshCollider
    {
        [MenuItem("NonsensicalKit/批量修改/批量添加MeshCollider")]
        private static void AggregatorEnumChecker()
        {
            var tArray = GetSelectComponent<Transform>();
            for (int i = 0; i < tArray.Count; i++)
            {
                Transform temp = tArray[i];
                if (temp.GetComponent<MeshFilter>() != null)
                {
                    if (temp.GetComponent<MeshCollider>() == null)
                    {
                        Undo.RecordObject(temp, temp.gameObject.name);
                        temp.gameObject.AddComponent<MeshCollider>();
                    }
                }
            }

            Debug.Log($"MeshCollider组件添加完成");
        }

        private static List<T> GetSelectComponent<T>()
        {
            var v = NonsensicalEditorManager.SelectGameObjects;
            List<T> components = new List<T>();

            foreach (var item in v)
            {
                components.AddRange(item.GetComponentsInChildren<T>());
            }

            return components;
        }
    }
}
