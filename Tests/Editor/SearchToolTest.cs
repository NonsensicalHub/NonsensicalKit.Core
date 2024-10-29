using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Nonsensicalkit.Tools.EazyTool.Tests
{
    public class SearchToolTest : MonoBehaviour
    {
        [Test]
        public void StringSearcherTest()
        {
            List<string> source = new List<string>()
            {
                "asdfasfdsaagagsas", "iij发撒打发是的地方阿斯弗爱的色放", "多噶苟富贵撒地方旦阿斯弗阿斯蒂芬说的发",
                "啊发放的顺丰aaaaaa", "测试用字符串"
            };
            StringSearcher s = new StringSearcher(source);
            CollectionAssert.AreEqual(s.SearchIndex("a"), new int[] { 0, 3 });
            CollectionAssert.AreEqual(s.SearchIndex("aaaaa"), new int[] { 3 });
            CollectionAssert.AreEqual(s.SearchIndex("aaaaaaaaa"), Array.Empty<int>());
            CollectionAssert.AreEqual(s.SearchIndex("放"), new int[] { 1, 3 });
            CollectionAssert.AreEqual(s.SearchIndex("地方"), new int[] { 1, 2 });
            CollectionAssert.AreEqual(s.SearchIndex("地的发放方"), Array.Empty<int>());
            CollectionAssert.AreEqual(s.SearchIndex("地方啊大苏打撒旦"), Array.Empty<int>());
            CollectionAssert.AreEqual(s.SearchIndex("顺丰a"), new int[] { 3 });
        }

        [Test]
        public void TimeSearcherTest()
        {
            List<DateTime> sourceTime = new List<DateTime>()
            {
                new DateTime(2022, 1, 1), new DateTime(1, 10, 2), new DateTime(2021, 1, 1),
                new DateTime(2023, 5, 1), new DateTime(2021, 2, 1), new DateTime(2023, 5, 8),
                new DateTime(2021, 5, 8), new DateTime(2028, 8, 8)
            };
            TimeSearcher ts = new TimeSearcher(sourceTime);
            CollectionAssert.AreEqual(ts.SearchIndex(new DateTime(2021, 1, 1), new DateTime(2022, 1, 1)),
                new int[] { 0, 2, 4, 6 });
        }
    }
}