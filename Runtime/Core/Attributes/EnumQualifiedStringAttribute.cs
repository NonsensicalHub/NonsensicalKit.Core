using System;
using UnityEngine;

namespace NonsensicalKit.Core
{
    /// <summary>
    /// 用于在unity中绘制String选择下拉框，限定只能选择某个枚举值
    /// </summary>
    public class EnumQualifiedStringAttribute : PropertyAttribute
    {
        public Type EnumType;
        public EnumQualifiedStringAttribute(Type type)
        {
            EnumType = type;
        }
    }
}
