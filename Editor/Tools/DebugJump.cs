using NonsensicalKit.Core.Log;
using NonsensicalKit.Core.Log.NonsensicalLog;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace NonsensicalKit.Core.Editor.Tools
{
    /// <summary>
    /// 用于改变双击console中信息后的跳转,跳过无效条目
    /// </summary>
    public class DebugJump
    {
        public static string[] JumpString = new string[] { nameof(DefaultLog), nameof(NonsensicalLog) ,nameof(LogCore) };

        [UnityEditor.Callbacks.OnOpenAsset(0)]
        private static bool OnOpenAsset(int instanceID, int line)
        {
            string instanceName = EditorUtility.InstanceIDToObject(instanceID).name;

            bool flag=false;
            //只处理需要处理的信息
            foreach (var item in JumpString)
            {
                if (item.Equals(instanceName))
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                return false;
            }

            string stackTrace = GetStackTrace();
            if (!string.IsNullOrEmpty(stackTrace))
            {
                Match matches = Regex.Match(stackTrace, @"\(at (.+)\)", RegexOptions.IgnoreCase);
                while (matches.Success)
                {
                    //索引为1的部分就是at后面的字符串
                    string pathline = matches.Groups[1].Value;

                    bool flag2 = false;
                    //判断是否需要跳过
                    foreach (var item in JumpString)
                    {
                        if (pathline.Contains(item))
                        {
                            flag2 = true;
                            break;
                        }
                    }
                    if (flag2)
                    {
                        matches = matches.NextMatch();
                        continue;
                    }
                    int splitIndex = pathline.LastIndexOf(":");
                    string path = pathline.Substring(0, splitIndex);
                    line = System.Convert.ToInt32(pathline.Substring(splitIndex + 1));
                    string fullPath ;
                    if (path.Contains("Assets"))
                    {
                        //当路径为Assets内时，Unity会省略前面的部分，需要自行补全
                        fullPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets"));
                        fullPath += path;
                    }
                    else
                    {
                        fullPath = path;
                    }

                    return UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(fullPath.Replace('/', '\\'), line); ;
                }

            }
            return false;
        }
        /// <summary>
        /// 获取Cosole窗口的详细信息部分，双击跳转时详细信息部分显示的就是选中的那条信息
        /// </summary>
        /// <returns></returns>
        private static string GetStackTrace()
        {
            var ConsoleWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
            if (ConsoleWindowType != null && EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.titleContent.text == "Console")
            {
                var activeTextField = ConsoleWindowType.GetField("m_ActiveText", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                string activeText = activeTextField.GetValue(EditorWindow.focusedWindow).ToString();
                return activeText;
            }
            return null;
        }
    }
}
