using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NonsensicalKit.Tools
{
    /// <summary>
    /// MD5工具类
    /// </summary>
    public static class MD5Tool
    {
        /// <summary>
        /// md5 16位加密
        /// </summary>
        /// <param name="convertString"></param>
        /// <param name="toLower"></param>
        /// <returns></returns>
        public static string GetMd5Str(string convertString, bool toLower)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            string t2 = BitConverter.ToString(md5.ComputeHash(Encoding.Default.GetBytes(convertString)), 4, 8);
            t2 = t2.Replace("-", "");
            if (toLower)
            {
                t2 = t2.ToLower();
            }

            return t2;
        }

        /// <summary>
        /// MD5　32位加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UserMd5(string str)
        {
            string cl = str;
            string pwd = "";
            MD5 md5 = MD5.Create(); //实例化一个md5对像
            // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
            // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
            foreach (var t in s)
            {
                // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符                 
                pwd = pwd + t.ToString("X");
            }

            return pwd;
        }

        public static string StringToMD5Hash(string inputString)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] encryptedBytes = md5.ComputeHash(Encoding.ASCII.GetBytes(inputString));
            StringBuilder sb = new StringBuilder();
            foreach (var t in encryptedBytes)
            {
                sb.AppendFormat("{0:x2}", t);
            }

            return sb.ToString();
        }

        public static string GetMd5Hash(MD5 md5Hash, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            foreach (var t in data)
            {
                sBuilder.Append(t.ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        // Verify a hash against a string.
        public static bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
        {
            // Hash the input.
            var hashOfInput = GetMd5Hash(md5Hash, input);

            // Create a StringComparer an compare the hashes.
            var comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取文件MD5值
        /// </summary>
        /// <param name="fileName">文件绝对路径</param>
        /// <param name="size"></param>
        /// <returns>MD5值</returns>
        public static string GetMD5HashFromFile(string fileName, int size = 0)
        {
            FileStream file;
            StringBuilder sb;
            try
            {
                file = new FileStream(fileName, FileMode.Open);
            }
            catch (Exception)
            {
                return null;
            }

            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal;
            if (size <= 0)
            {
                retVal = md5.ComputeHash(file);
            }
            else
            {
                var bytes = new byte[size];
                var l = file.Read(bytes, 0, size);
                if (l < size)
                {
                    Array.Resize(ref bytes, l);
                }

                retVal = md5.ComputeHash(bytes);
            }

            file.Close();

            sb = new StringBuilder();
            foreach (var t in retVal)
            {
                sb.Append(t.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
