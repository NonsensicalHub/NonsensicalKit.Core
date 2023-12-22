using NonsensicalKit.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NonsensicalKit.Core.Editor.Tools
{
    /// <summary>
    /// 编辑器总管理类，用于管理一些通用但使用较消耗性能的属性
    /// </summary>
    public static class NonsensicalEditorManager
    {
        public static GameObject[] SelectGameobjects;
        public static Transform SelectTransform;
        public static Action OnSelectChanged;
        public static Action OnEditorUpdate;

        [InitializeOnLoadMethod]
        private static void App()
        {
            EditorApplication.update += OnUpdate;

            Selection.selectionChanged += OnSelectionChanged;
        }

        private static void OnUpdate()
        {
            OnEditorUpdate?.Invoke();
        }

        private static void OnSelectionChanged()
        {
            if (Selection.gameObjects.Length < 1)
            {
                SelectGameobjects = new GameObject[0];
                SelectTransform = null;
            }
            else
            {
                SelectGameobjects = Selection.gameObjects;
                SelectTransform = SelectGameobjects[0].transform;
            }

            OnSelectChanged?.Invoke();
        }

        /// <summary>
        /// 检测Resources文件夹内是否存在同名文件（无视后缀名）
        /// </summary>
        /// <returns></returns>
        [MenuItem("NonsensicalKit/Items/检测资源重名")]
        private static bool CheckResoureDuplicateName()
        {
            List<string> duplicateNameInfo = new List<string>();

            HashSet<string> vs = new HashSet<string>();

            Queue<DirectoryInfo> directoryInfos = new Queue<DirectoryInfo>();

            DirectoryInfo di = new DirectoryInfo(Application.dataPath + @"/Resources");

            directoryInfos.Enqueue(di);

            int leftCount = 1;

            while (leftCount > 0)
            {
                DirectoryInfo directoryInfo = directoryInfos.Dequeue();
                leftCount--;

                foreach (FileInfo item in directoryInfo.GetFiles())
                {
                    if (vs.Add(item.Name) == false)
                    {
                        duplicateNameInfo.Add(item.FullName);
                    }
                }

                foreach (DirectoryInfo item in directoryInfo.GetDirectories())
                {
                    directoryInfos.Enqueue(item);
                    leftCount++;
                }
            }

            foreach (var item in duplicateNameInfo)
            {
                Debug.Log($"资源重名：{item}");
            }

            if (duplicateNameInfo.Count == 0)
            {
                Debug.Log("无资源重名");
                return false;
            }

            return true;
        }

        [MenuItem("NonsensicalKit/Items/刷新项目文件")]
        private static void RefeshAsset()
        {
            AssetDatabase.Refresh();
            Debug.Log("刷新完成");
        }

        [MenuItem("NonsensicalKit/Items/开启持久存储文件夹")]
        private static void OpenPersistentDataPath()
        {
            string path = Application.persistentDataPath;
            System.Diagnostics.Process.Start(path);
        }

        /// <summary>
        /// 根据名称排序场景内对象
        /// </summary>
        [MenuItem("NonsensicalKit/Items/根据名称排序")]
        private static void NameSort()
        {
            GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

            for (int i = 0; i < roots.Length - 1; i++)
            {
                for (int j = i + 1; j < roots.Length; j++)
                {
                    if (string.Compare(roots[i].name, roots[j].name) > 0)
                    {
                        GameObject temp = roots[i];
                        roots[i] = roots[j];
                        roots[j] = temp;
                    }
                }
            }

            foreach (var item in roots)
            {
                item.transform.SetAsLastSibling();
            }

            Debug.Log("排序完成");
        }

        [MenuItem("NonsensicalKit/检测聚合器枚举值重复")]
        private static void AggregatorEnumChecker()
        {
            Debug.Log("开始检测枚举值重复");
            int errorCount = 0;
            Dictionary<int,string> keyValuePairs = new Dictionary<int,string>();
            var v = ReflectionTool.GetEnumByAttribute<AggregatorEnumAttribute>();
            foreach (var item in v)
            {
                Array values = Enum.GetValues(item);
                foreach (var value in values)
                {
                    var intValue = (int)value ;
                    if (keyValuePairs.ContainsKey(intValue))
                    {
                        errorCount++;
                        Debug.Log($"枚举{item.Name}与枚举{keyValuePairs[intValue]}存在相同的值索引{intValue}");
                    }
                    else
                    {
                        keyValuePairs.Add(intValue, item.Name);
                    }
                }
            }
            Debug.Log($"枚举值重复检测完毕,共发现{errorCount}个重复");
        }
    }
}
