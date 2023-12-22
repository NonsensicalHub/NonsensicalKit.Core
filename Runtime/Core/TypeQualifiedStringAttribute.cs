using System;
using UnityEngine;

namespace NonsensicalKit.Core
{
    /// <summary>
    /// 用于在unity中绘制String选择下拉框，限定只能选择继承某个类型的类
    /// </summary>
    public class TypeQualifiedStringAttribute : PropertyAttribute
    {
        public Type Type;
        public TypeQualifiedStringAttribute(Type type)
        {
            Type = type;
        }
    }
}