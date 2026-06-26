using UnityEngine;

namespace NonsensicalKit.Core.DagLogicNode
{
    public class DagNodeIdAttribute : PropertyAttribute
    {
        public readonly bool AllowEmpty;

        public DagNodeIdAttribute(bool allowEmpty = true)
        {
            AllowEmpty = allowEmpty;
        }
    }
}
