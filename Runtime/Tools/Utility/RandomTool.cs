using System;
using System.Security.Cryptography;
using UnityEngine;

namespace NonsensicalKit.Tools
{
    /// <summary>
    /// 随机数工具类
    /// </summary>
    public static class RandomTool
    {
        /// <summary>
        /// 创建一个足够随机的token
        /// </summary>
        /// <returns></returns>
        public static string CreateToken()
        {
            var deviceId = SystemInfo.deviceUniqueIdentifier;
            var day = DateTime.Now.Day;
            var month = DateTime.Now.Month;
            var last2DigitsofYear = DateTime.Now.Year % 100;
            //依照规则可以添加其他字段，确保足够复杂
            var source = ((day * 10) + (month * 100) + (last2DigitsofYear) * 1000) + deviceId;
            //创建md5
            using (var md5Hash = MD5.Create())
            {
                return MD5Tool.GetMd5Hash(md5Hash, source);
            }
        }

        /// <summary>
        /// 获取使用Guid作为种子返回的随机数
        /// </summary>
        /// <param name="max">返回值的绝对值小于max</param>
        /// <returns></returns>
        public static int GetRandomInt(int max)
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            int iSeed = BitConverter.ToInt32(buffer, 0);
            System.Random random = new System.Random(iSeed);
            int temp = random.Next(max * 2 - 1);
            temp = temp - max + 1;

            return temp;
        }

        public static float GetRandomFloat(float max)
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            int iSeed = BitConverter.ToInt32(buffer, 0);
            System.Random random = new System.Random(iSeed);
            float temp = (float)random.NextDouble() % max;
            return temp;
        }

        /// <summary>
        /// Returns a random long from min (inclusive) to max (exclusive)
        /// 用例（ming=long.MinValue,max=long.MaxValue)中有误
        /// </summary>
        /// <param name="random">The given random instance</param>
        /// <param name="min">The inclusive minimum bound</param>
        /// <param name="max">The exclusive maximum bound.  Must be greater than min</param>
        public static long NextLong(this System.Random random, long min = 0, long max = long.MaxValue)
        {
            if (max <= min)
                return min;

            //Working with ulong so that modulo works correctly with values > long.MaxValue
            ulong uRange = (ulong)(max - min);

            //Prevent a modolo bias; see https://stackoverflow.com/a/10984975/238419
            //for more information.
            //In the worst case, the expected number of calls is 2 (though usually it's
            //much closer to 1) so this loop doesn't really hurt performance at all.
            ulong ulongRand;
            do
            {
                byte[] buf = new byte[8];
                random.NextBytes(buf);
                ulongRand = (ulong)BitConverter.ToInt64(buf, 0);
            } while (ulongRand > ulong.MaxValue - ((ulong.MaxValue % uRange) + 1) % uRange);

            return (long)(ulongRand % uRange) + min;
        }
    }
}
