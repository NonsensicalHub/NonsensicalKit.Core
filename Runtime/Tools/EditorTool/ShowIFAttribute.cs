using System;
using UnityEngine;

namespace NonsensicalKit.Tools.EditorTool
{
    /// <summary>
    /// 限定在属性面板上的显示，需要注意的是，当给数组或链表添加此属性时，实际上控制的是数组或链表的子对象
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class ShowIFAttribute : PropertyAttribute
    {
        public string MemberName;
        public object Value;

        public ShowIFAttribute(string memberName)
        {
            MemberName = memberName;
            Value = true;
        }
        public ShowIFAttribute(string memberName, object value)
        {
            MemberName = memberName;
            Value = value;
        }
    }
}
