using System;
using UnityEngine.Serialization;

namespace NonsensicalKit.Core
{
    [Serializable]
    public class SerializableArray<T>
    {
        [FormerlySerializedAs("Array")] public T[] m_Array;
    }
}
