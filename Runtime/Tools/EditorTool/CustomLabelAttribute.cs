using UnityEngine;

namespace NonsensicalKit.Tools.EditorTool
{
    /// <summary>
    /// 使字段在Inspector中显示自定义的名称。
    /// </summary>
    public class CustomLabelAttribute : PropertyAttribute
    {
        public string name;

        public CustomLabelAttribute(string name)
        {
            this.name = name;
        }
    }
}
