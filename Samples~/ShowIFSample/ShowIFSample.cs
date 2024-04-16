using NonsensicalKit.Tools.EditorTool;
using System.Collections.Generic;
using UnityEngine;

namespace NonsensicalKit.Tools.Samples.EditorTool
{
    public class ShowIFSample : MonoBehaviour
    {
        public bool Bool1 = true;
        public bool Bool2 => Bool1;
        [ShowIF("Bool1", true)]
        public int A, B;
        [ShowIF("Bool2", false)]
        public int C, D;

        public enum ThisIsEnum
        {
            Alpha,
            Beta,
            Gamma,
        }
        public ThisIsEnum EnumValue;

        [ShowIF("EnumValue", ThisIsEnum.Alpha)]
        public int Alpha;
        [ShowIF("EnumValue", ThisIsEnum.Beta)]
        public int Beta;
        [ShowIF("EnumValue", ThisIsEnum.Gamma)]
        public int Gamma;

        [System.Serializable]
        public class ThisIsClass
        {
            public ThisIsEnum EnumValue2 = ThisIsEnum.Alpha;

            [ShowIF("EnumValue2", ThisIsEnum.Alpha)]
            public ThisIsClass2 Class;

            [ShowIF("EnumValue2", ThisIsEnum.Beta)]
            public int IntValue;

            [System.Serializable]
            public class ThisIsClass2
            {
                [ShowIF("EnumValue2", ThisIsEnum.Alpha)]
                public List<string> ListValue = new();
            }

        }
        public ThisIsClass ClassValue;
    }
}
