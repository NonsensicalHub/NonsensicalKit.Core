using NonsensicalKit.Core;
using NonsensicalKit.Tools;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nonsensicalkit.Core.Editor.Tests
{
    class AggregatorEnumTest
    {
		[Test]
		public void AggregatorEnumChecker() 
		{
            StringBuilder sb = new StringBuilder();
            int errorCount = 0;
            Dictionary<int, string> keyValuePairs = new Dictionary<int, string>();
            var v = ReflectionTool.GetEnumByAttribute<AggregatorEnumAttribute>();
            foreach (var item in v)
            {
                Array values = Enum.GetValues(item);
                foreach (var value in values)
                {
                    var intValue = (int)value;
                    if (keyValuePairs.ContainsKey(intValue))
                    {
                        errorCount++;
                        sb.AppendLine($"枚举{item.Name}与枚举{keyValuePairs[intValue]}存在相同的值索引{intValue}");
                    }
                    else
                    {
                        keyValuePairs.Add(intValue, item.Name);
                    }
                }
            }
            if (errorCount>0)
            {
                sb.Insert(0, $"枚举值重复检测完毕,共发现{errorCount}个重复");
            }
            Assert.That(errorCount ==0, sb.ToString()) ;
        }
    }
}