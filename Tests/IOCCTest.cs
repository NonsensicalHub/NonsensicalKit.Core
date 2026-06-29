using System.Reflection;
using NUnit.Framework;

namespace NonsensicalKit.Core.Editor.Tests
{
    public class IOCCTest
    {
        [Test]
        // Start is called before the first frame update
        public void Test()
        {
            // 原本使用的地方现在不使用反射了
            
            // var messageAggregator = typeof(MessageAggregator);
            // var fieldInfo = messageAggregator.GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
            // Assert.IsNotNull(fieldInfo);
            // var instance = fieldInfo.GetValue(null);
            // Assert.IsNotNull(instance);
        }
    }
}
