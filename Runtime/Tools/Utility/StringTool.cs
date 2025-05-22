using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NonsensicalKit.Tools
{
    /// <summary>
    /// 字符串工具类
    /// </summary>
    public static class StringTool
    {
        private const string NoBreakingSpace = "\u00A0";

        #region PublicMethod

        public static string GetNumAlphabet(int num)
        {
            string result = string.Empty;
            while (num > 0)
            {
                num--; // 调整num，使余数0对应'A'
                int remainder = num % 26;
                result = (char)(65 + remainder) + result;
                num /= 26;
            }

            return result;
        }

        ///<summary>
        /// 判断输入的字符串是否只包含数字和英文字母  
        /// </summary>
        /// <returns></returns>
        public static bool JustNumAndEng(this string str)
        {
            foreach (var c in str)
            {
                if (c < 48
                    || (57 < c && c < 65)
                    || (90 < c && c < 97)
                    || 122 < c)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 获取格式化时间，自动判断顶级单位
        /// </summary>
        /// <param name="totalSecond"></param>
        /// <returns></returns>
        public static string GetFormatTime(float totalSecond)
        {
            var hour = (int)totalSecond / 3600;
            var minute = (int)(totalSecond - hour * 3600) / 60;
            var second = (int)(totalSecond - hour * 3600 - minute * 60);

            if (hour > 0)
            {
                return $"{hour:D2}:{minute:D2}:{second:D2}";
            }
            else
            {
                return $"{minute:D2}:{second:D2}";
            }
        }


        /// <summary>
        /// 获取格式化时间，顶级单位为小时
        /// </summary>
        /// <param name="totalSecond"></param>
        /// <returns></returns>
        public static string GetFormatHourTime(float totalSecond)
        {
            var hour = (int)totalSecond / 3600;
            var minute = (int)(totalSecond - hour * 3600) / 60;
            var second = (int)(totalSecond - hour * 3600 - minute * 60);

            return $"{hour:D2}:{minute:D2}:{second:D2}";
        }

        /// <summary>
        /// 获取格式化时间，顶级单位为分钟
        /// </summary>
        /// <param name="totalSecond"></param>
        /// <returns></returns>
        public static string GetFormatMinuteTime(float totalSecond)
        {
            var minute = (int)(totalSecond) / 60;
            var second = (int)(totalSecond - minute * 60);

            return $"{minute:D2}:{second:D2}";
        }

        /// <summary>
        /// 从Url中获取文件后缀
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetFileExtensionFromUrl(string url)
        {
            url = url.Split('?')[0];
            url = url.Split('/').Last();
            return url.Contains('.') ? url.Substring(url.LastIndexOf('.')) : "";
        }

        /// <summary>
        /// 将普通空格替换成utf8中的空格，避免unity文本组件因空格换行
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ReplaceNoBreakingSpace(string str)
        {
            return str.Replace(" ", NoBreakingSpace);
        }

        /// <summary>
        /// 匿名函数的正则匹配
        /// </summary>
        /// <returns></returns>
        public static string AnonymousFunctionMatch()
        {
            return "[(].*[)] *= *>";
        }

        /// <summary>
        /// 获取新副本名称
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetDuplicateName(string name)
        {
            int oldIndex = GetDuplicateIndex(name);
            if (oldIndex > 0)
            {
                name = name.RemoveEnd(oldIndex.ToString().Length + 2);
            }

            name += $"({oldIndex + 1})";
            return name;
        }

        /// <summary>
        /// 获取副本索引
        /// </summary>
        /// <param name="name"></param>
        /// <returns>返回0代表无有效索引</returns>
        public static int GetDuplicateIndex(string name)
        {
            int leftIndex = name.LastIndexOf('(');
            if (leftIndex == -1) return 0;

            int rightIndex = name.LastIndexOf(')');
            int length = rightIndex - leftIndex;
            if (length <= 0) return 0;

            string indexStr = name.Substring(leftIndex + 1, length - 1);
            if (int.TryParse(indexStr, out var index) == false) return 0;
            if (index < 0) return 0;
            return index;
        }

        /// <summary>
        /// 去除字符串尾部字符
        /// </summary>
        /// <param name="str"></param>
        /// <param name="removeLength"></param>
        /// <returns></returns>
        public static string RemoveEnd(this string str, int removeLength)
        {
            int length = str.Length;
            if (str.Length < removeLength) return str;
            return str.Substring(0, length - removeLength);
        }

        /// <summary>
        /// 将单行字符串分割成多行，同时保证单词不会跨行
        /// </summary> 
        /// <param name="stringToSplit"></param>
        /// <param name="maximumLineLength"></param>
        /// <returns></returns>
        public static IEnumerable<string> SplitToLines(string stringToSplit, int maximumLineLength)
        {
            var words = stringToSplit.Split(' ').Concat(new[] { "" });
            var enumerable = words as string[] ?? words.ToArray();
            return
                enumerable
                    .Skip(1)
                    .Aggregate(
                        enumerable.Take(1).ToList(),
                        (a, w) =>
                        {
                            var last = a.Last();
                            while (last.Length > maximumLineLength)
                            {
                                a[a.Count() - 1] = last.Substring(0, maximumLineLength);
                                last = last.Substring(maximumLineLength);
                                a.Add(last);
                            }

                            var test = last + " " + w;
                            if (test.Length > maximumLineLength)
                            {
                                a.Add(w);
                            }
                            else
                            {
                                a[a.Count() - 1] = test;
                            }

                            return a;
                        });
        }

        public static string ToRealString(this Vector2 v2)
        {
            return $"({v2.x},{v2.y})";
        }

        public static string ToRealString(this Vector3 v3)
        {
            return $"({v3.x},{v3.y},{v3.z})";
        }

        public static bool CheckBom(byte[] data)
        {
            if (data.Length < 3)
            {
                return false;
            }

            return data[0] == 239 && data[1] == 187 && data[2] == 191;
        }

        public static bool CheckBom(string str)
        {
            if (str.Length < 1)
            {
                return false;
            }

            return str[0] == 65279;
        }

        public static string TrimBom(this string str)
        {
            if (CheckBom(str))
            {
                return str.Substring(1);
            }

            return str;
        }

        public static string TrimClone(string str)
        {
            int length = str.Length;
            if (length < 7)
            {
                return str;
            }

            if (str.Substring(length - 7) != "(Clone)")
            {
                return str;
            }

            return str.Substring(0, length - 7);
        }

        /// <summary>
        /// 算式运算(仅支持加减乘除)
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static double? Calculation(string s)
        {
            List<string> ls = Incision(s);

            if (ls == null || ls.Count == 0)
            {
                return null;
            }

            ls = Brackets(ls);
            if ((ls = Exclude(ls)) == null)
            {
                return null;
            }

            ls = MultiplicationAndDivision(ls);
            return AdditionAndSubtraction(ls);
        }

        /// <summary>
        /// 获取集合的可读字符串
        /// </summary>
        /// <param name="ienumerable"></param>
        /// <returns></returns>
        public static string GetSetString(IEnumerable ienumerable)
        {
            if (ienumerable == null)
            {
                return "not a set";
            }

            StringBuilder sb = new StringBuilder();

            sb.Append("[");
            foreach (var item in ienumerable)
            {
                if (item is Vector2 vector2)
                {
                    sb.Append($"({vector2.x},{vector2.y})");
                }
                else if (item is Vector3 vector3)
                {
                    sb.Append($"({vector3.x},{vector3.y},{vector3.z})");
                }
                else if (item is Vector4 temp)
                {
                    sb.Append($"({temp.x},{temp.y},{temp.z},{temp.w})");
                }
                else
                {
                    sb.Append(item);
                }

                sb.Append(",");
            }

            if (sb[^1] != '[')
            {
                sb.Remove(sb.Length - 1, 1);
            }

            sb.Append("]");

            return sb.ToString();
        }

        /// <summary>
        /// 根据路径字符串获取文件名
        /// </summary>
        /// <param name="path">文件全路径</param>
        /// <param name="withSuffix">返回的结果是否携带后缀</param>
        /// <returns></returns>
        public static string GetFileNameByPath(string path, bool withSuffix = false)
        {
            if (path == null)
            {
                return string.Empty;
            }

            string[] fullNameTemp = path.Split(new[] { '/', '\\' });

            string nameWithSuffix = fullNameTemp[^1];

            string filename = nameWithSuffix;

            if (withSuffix == false)
            {
                string[] nameWithSuffixTemp = nameWithSuffix.Split(new[] { '.' });

                filename = nameWithSuffixTemp[0];
            }

            return filename;
        }

        /// <summary>
        /// 根据路径字符串获取文件夹字符串
        /// </summary>
        /// <param name="path">文件全路径</param>
        /// <returns></returns>
        public static string GetDirPathByPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            string[] fullNameTemp = path.Split(new[] { '/', '\\' });

            int fileNameLength = fullNameTemp[^1].Length;


            return path.Substring(0, path.Length - fileNameLength - 1);
        }

        /// <summary>
        /// 根据长度切分字符串
        /// </summary>
        /// <param name="target"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static string[] SplitStringByLength(string target, int limit = 10000)
        {
            if (target.Length < limit) return new[] { target };
            int count = target.Length / limit;
            var array = new string[count + 1];
            for (int i = 0; i < count; i++)
            {
                array[i] = target.Substring(i * limit, limit) + "\n";
            }

            array[count] = target.Substring(count * limit, target.Length % limit);
            return array;
        }

        /// <summary>
        /// 判断字符串数组中是否包含目标字符串
        /// </summary>
        /// <param name="stringArray"></param>
        /// <param name="check"></param>
        /// <returns></returns>
        public static bool Contains(this string[] stringArray, string check)
        {
            if (stringArray != null)
            {
                foreach (var item in stringArray)
                {
                    if (item == check)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 获取当前日期的字符串
        /// </summary>
        /// <returns>当前日期的字符串</returns>
        public static string GetDateString(string divider = "_")
        {
            return DateTime.Today.ToString($"yyyy{divider}MM{divider}dd");
        }

        /// <summary>
        /// 获取当前日期时间的字符串
        /// </summary>
        /// <returns>当前日期的字符串</returns>
        public static string GetDateTimeString(string divider = "_")
        {
            DateTime dt = DateTime.Now;
            return DateTime.Now.ToString($"yyyy{divider}MM{divider}dd {dt.Hour}{divider}mm{divider}ss");
        }

        /// <summary>
        /// 将秒数转换成 小时:分钟:秒数
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string ToHms(int time)
        {
            int hour = time / 3600;
            int minute = (time - hour * 3600) / 60;
            int second = time % 60;
            return $"{hour:D2}:{minute:D2}:{second:D2}";
        }

        /// <summary>
        /// 将秒数转换成 分钟:秒数
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string ToMS(int time)
        {
            int minute = time / 60;
            int second = time % 60;
            return $"{minute:D2}:{second:D2}";
        }

        #endregion

        #region PrivateMethod

        /// <summary>
        ///  将传入的字符串以算数符号为界切开，并放入链表中
        /// </summary>
        /// <param name="s">要切分的字符串</param>
        /// <returns></returns>
        private static List<string> Incision(string s)
        {
            List<string> ls = new List<string>();
            bool flag = false;

            while (true)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] == '+' || s[i] == '-' || s[i] == '*' || s[i] == '/')
                    {
                        string temp1 = s[i].ToString();
                        char[] temp2 = { s[i] };
                        string[] ss = s.Split(temp2, 2);
                        if (ss[0] != "")
                        {
                            ls.Add(ss[0]);
                        }

                        ls.Add(temp1);
                        if (ss[1] == "")
                        {
                            return null;
                        }

                        s = ss[1];
                        flag = true;
                        break;
                    }
                    else if (s[i] == '(' || s[i] == '（')
                    {
                        if (i == 0)
                        {
                            if (!char.IsNumber(s[i + 1]))
                            {
                                return null;
                            }
                        }
                        else
                        {
                            if ((s[i - 1] != '+' && s[i - 1] != '-' && s[i - 1] != '*' && s[i - 1] != '/') || !char.IsNumber(s[i + 1]))
                            {
                                return null;
                            }
                        }

                        ls.Add(s[i].ToString());
                        s = s.Substring(1);
                        flag = true;
                        break;
                    }
                    else if (s[i] == ')' || s[i] == '）')
                    {
                        if (i == s.Length - 1)
                        {
                            if (!char.IsNumber(s[i - 1]))
                            {
                                return null;
                            }
                        }
                        else
                        {
                            if (!char.IsNumber(s[i - 1]) || (s[i + 1] != '+' && s[i + 1] != '-' && s[i + 1] != '*' && s[i + 1] != '/'))
                            {
                                return null;
                            }
                        }

                        string temp1 = s[i].ToString();
                        char[] temp2 = { s[i] };
                        string[] ss = s.Split(temp2, 2);
                        ls.Add(ss[0]);
                        ls.Add(temp1);
                        s = ss[1];
                        flag = true;
                        break;
                    }
                }

                if (flag)
                {
                    flag = false;
                    continue;
                }

                if (!string.IsNullOrEmpty(s))
                {
                    ls.Add(s);
                }

                break;
            }

            return ls;
        }

        /// <summary>
        /// 将所有非数字或运算符号的字符排除
        /// </summary>
        /// <param name="ls"></param>
        /// <returns></returns>
        private static List<string> Exclude(List<string> ls)
        {
            for (int i = 0; i < ls.Count; i++)
            {
                if (ls[i] != "+" && ls[i] != "-" && ls[i] != "*" && ls[i] != "/" && ls[i] != "(" && ls[i] != ")" && ls[i] != "（" &&
                    ls[i] != "）")
                {
                    for (int j = 0; j < ls[i].Length; j++)
                    {
                        if (!char.IsNumber(ls[i][j]) && ls[i][j] != '.')
                        {
                            ls[i] = ls[i].Replace(ls[i][j].ToString(), "");
                            j = -1;
                        }
                    }

                    if (ls[i] == "")
                    {
                        return null;
                    }
                }
            }

            return ls;
        }

        /// <summary>
        /// 括弧运算
        /// </summary>
        /// <param name="ls"></param>
        /// <returns></returns>
        private static List<string> Brackets(List<string> ls)
        {
            for (int i = 0; i < ls.Count; i++)
            {
                if (ls[i] == "(" || ls[i] == "（")
                {
                    int count = 1;
                    for (int j = i + 1; j < ls.Count; j++)
                    {
                        if (ls[j] == "(" || ls[j] == "（")
                        {
                            count++;
                        }
                        else if ((ls[j] == ")" || ls[j] == "）"))
                        {
                            count--;
                            if (count == 0)
                            {
                                ls[i] = Calculation(string.Join("", ls.GetRange(i + 1, j - i - 1))).ToString();
                                ls.RemoveRange(i + 1, j - i);
                                break;
                            }
                        }
                    }
                }
            }

            return ls;
        }

        /// <summary>
        /// 进行乘除运算
        /// </summary>
        /// <param name="ls"></param>
        /// <returns></returns>
        private static List<string> MultiplicationAndDivision(List<string> ls)
        {
            int i = 0;
            bool flag = false;
            double d = 0;

            while (true)
            {
                for (; i < ls.Count; i++)
                {
                    if (ls[i] == "*")
                    {
                        d = double.Parse(ls[i - 1]) * double.Parse(ls[i + 1]);
                        flag = true;
                        break;
                    }
                    else if (ls[i] == "/")
                    {
                        d = double.Parse(ls[i - 1]) / double.Parse(ls[i + 1]);
                        flag = true;
                        break;
                    }
                }

                if (flag)
                {
                    ls[i - 1] = d.ToString(CultureInfo.InvariantCulture);
                    ls.RemoveRange(i, 2);
                    flag = false;
                }
                else
                {
                    return ls;
                }
            }
        }

        /// <summary>
        /// 进行加减运算
        /// </summary>
        /// <param name="ls"></param>
        /// <returns></returns>
        private static double AdditionAndSubtraction(List<string> ls)
        {
            double result = double.Parse(ls[0]);

            for (int i = 1; i < ls.Count; i += 2)
            {
                if (ls[i] == "+")
                {
                    result += double.Parse(ls[i + 1]);
                }
                else if (ls[i] == "-")
                {
                    result -= double.Parse(ls[i + 1]);
                }
            }

            return result;
        }

        #endregion
    }
}
