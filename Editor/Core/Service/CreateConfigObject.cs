using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace NonsensicalKit.Core.Editor.Service.Config
{
    public class CreateConfigObject : EndNameEditAction
    {
        [MenuItem("Assets/Create/NonsensicalKit/CreateConfigObject", false, -100)]
        private static void ShowWindow()
        {
            Create();
        }

        public static void Create()
        {
            //参数为传递给CreateEventCSScriptAsset类action方法的参数
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                CreateInstance<CreateConfigObject>(),
                GetSelectPathOrFallback() + $"/NewConfigObject.cs", null, "");
        }

        /// <summary>
        /// 取得要创建文件的路径
        /// </summary>
        /// <returns></returns>
        public static string GetSelectPathOrFallback()
        {
            string path = "Assets";
            //遍历选中的资源以获得路径
            //Selection.GetFiltered是过滤选择文件或文件夹下的物体，assets表示只返回选择对象本身
            foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }

            return path;
        }


        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            Object obj = CreateScriptAssetFromTemplate(pathName, resourceFile); //创建资源
            ProjectWindowUtil.ShowCreatedAsset(obj); //高亮显示资源
        }

        private Object CreateScriptAssetFromTemplate(string pathName, string resourceFile)
        {
            string fullPath = Path.GetFullPath(pathName); //获取要创建资源的绝对路径
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathName); //获取文件名，不含扩展名


            string resourceFileText =
                @"using NonsensicalKit.Core.Service.Config;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = ""#CLASSNAME#"", menuName = ""ScriptableObjects/#CLASSNAME#"")]
public class #CLASSNAME# : ConfigObject
{
    public #CLASSNAME#Data data;
    public override ConfigData GetData()
    {
        return data;
    }

    public override void SetData(ConfigData cd)
    {
        data = cd as #CLASSNAME#Data;
    }
}

[System.Serializable]
public class #CLASSNAME#Data : ConfigData
{

}
 ";
            string temp = Regex.Replace(resourceFileText, "#CLASSNAME#", fileNameWithoutExtension);

            bool encoderShouldEmitUTF8Identifier = true; //参数指定是否提供 Unicode 字节顺序标记
            bool throwOnInvalidBytes = false; //是否在检测到无效的编码时引发异常
            UTF8Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier, throwOnInvalidBytes);
            bool append = false;
            StreamWriter streamWriter = new StreamWriter(fullPath, append, encoding); //写入文件
            streamWriter.Write(temp);
            streamWriter.Close();
            AssetDatabase.ImportAsset(pathName); //刷新资源管理器
            AssetDatabase.Refresh();
            return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
        }
    }
}
