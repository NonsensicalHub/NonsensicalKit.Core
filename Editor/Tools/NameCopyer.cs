using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NonsensicalKit.Editor.Core.Tools
{
    /// <summary>
    /// 快速命名工具，可以将一个节点树的所有名称复制到另一个相同结构的节点树上
    /// </summary>
    public class NameCopyer : EditorWindow
    {
        [MenuItem("NonsensicalKit/快速命名工具")]
        private static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(NameCopyer));
        }

        private NameTree _copyBuffer;

        private void OnGUI()
        {
            if (GUILayout.Button("复制"))
            {
                _copyBuffer = new NameTree();

                Copy(NonsensicalEditorManager.SelectTransform, _copyBuffer);

                Debug.Log("复制成功");
            }

            if (GUILayout.Button("粘贴"))
            {
                Paste(NonsensicalEditorManager.SelectTransform, _copyBuffer);
                Undo.RecordObject(NonsensicalEditorManager.SelectTransform, "PasteName");
                Debug.Log("粘贴成功");
            }
        }

        private void Copy(Transform node, NameTree nameTree)
        {
            nameTree.Name = node.name;
            nameTree.Childs = new List<NameTree>();
            foreach (Transform item in node)
            {
                NameTree newChild = new NameTree();
                nameTree.Childs.Add(newChild);
                Copy(item, newChild);
            }
        }

        private void Paste(Transform node, NameTree nameTree)
        {
            node.name = nameTree.Name;

            int min = Mathf.Min(node.childCount, nameTree.Childs.Count);
            for (int i = 0; i < min; i++)
            {
                Paste(node.GetChild(i), nameTree.Childs[i]);
            }
        }

        class NameTree
        {
            public string Name;
            public List<NameTree> Childs;
        }
    }
}
