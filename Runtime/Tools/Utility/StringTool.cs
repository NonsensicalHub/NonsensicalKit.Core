using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace NonsensicalKit.Tools
{
    /// <summary>
    /// 字符串工具类
    /// </summary>
    public static class StringTool
    {
        private static readonly string no_breaking_space = "\u00A0";

        #region PublicMethod

        ///<summary>
        /// 判断输入的字符串是否只包含数字和英文字母  
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool JustNumAndEng(this string str)
        {
            foreach (var c in str)
            {
                if (c<48
                    ||(57<c&&c<65)
                    ||(90<c&&c<97)
                    ||122<c)
                {
                    return false;
                }
            }
            return true;
        }

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
            return str.Replace(" ", no_breaking_space);
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
            if (oldIndex>0)
            {
                name = name.RemoveEnd(oldIndex.ToString().Length + 2);
            }
            name+=$"({oldIndex+1})";
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
            if (length<=0) return 0;

            string indexStr=name.Substring(leftIndex+1, length-1);
            int index;
            if (int.TryParse(indexStr, out index) == false) return 0;
            if (index < 0) return 0;
            return index;
        }

        /// <summary>
        /// 去除字符串尾部字符
        /// </summary>
        /// <param name="str"></param>
        /// <param name="removeLength"></param>
        /// <returns></returns>
        public static string RemoveEnd(this string str,int removeLength)
        {
            int length = str.Length;
            if (str.Length< removeLength) return str;
            return str.Substring(0, length-removeLength);
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
            return
                words
                    .Skip(1)
                    .Aggregate(
                        words.Take(1).ToList(),
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

        // Note that Color32 and Color implictly convert to each other. You may pass a Color object to this method without first casting it.
        public static string ColorToHex(Color32 color)
        {
            string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString("X2");
            return hex;
        }

        public static Color HexToColor(string hex)
        {
            hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
            hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
            byte a = 255;//assume fully visible unless specified in hex
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            //Only use alpha if the string has enough characters
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return new Color32(r, g, b, a);
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

        public static string TrimBOM(this string str)
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
            if (str.Substring(length-7)!="(Clone)")
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
                if (item.GetType() == typeof(Vector2))
                {
                    Vector2 temp = (Vector2)item;
                    sb.Append($"({temp.x},{temp.y})");
                }
                else if (item.GetType() == typeof(Vector3))
                {
                    Vector3 temp = (Vector3)item;
                    sb.Append($"({temp.x},{temp.y},{temp.z})");
                }
                else if (item.GetType() == typeof(Vector4))
                {
                    Vector4 temp = (Vector4)item;
                    sb.Append($"({temp.x},{temp.y},{temp.z},{temp.w})");
                }
                else
                {
                    sb.Append(item.ToString());
                }
                sb.Append(",");
            }
            if (sb[sb.Length - 1] != '[')
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

            string[] fullNameTemp = path.Split(new char[] { '/', '\\' });

            string nameWithSuffix = fullNameTemp[fullNameTemp.Length - 1];

            string filename = nameWithSuffix;

            if (withSuffix == false)
            {
                string[] nameWithSuffixTemp = nameWithSuffix.Split(new char[] { '.' });

                filename = nameWithSuffixTemp[0];
            }

            return filename;
        }

        /// <summary>
        /// 根据路径字符串获取文件夹字符串
        /// </summary>
        /// <param name="path">文件全路径</param>
        /// <returns></returns>
        public static string GetDirpathByPath(string path)
        {
            if (path == null)
            {
                return string.Empty;
            }

            string[] fullNameTemp = path.Split(new char[] { '/', '\\' });

            int fileNameLength = fullNameTemp[fullNameTemp.Length - 1].Length;


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
            if (target.Length < limit) return new string[] { target };
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
        public static string ToHMS(int time)
        {
            int hour = time / 3600;
            int minute = (time - hour * 3600) / 60;
            int second = time % 60;
            return string.Format("{0:D2}:{1:D2}:{2:D2}", hour, minute, second);
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
            return string.Format("{0:D2}:{1:D2}", minute, second);
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
                        if (ss[0] != null && ss[0] != "")
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
                if (s != null && s != "")
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
        /// <param name="_ls"></param>
        /// <returns></returns>
        private static List<string> Exclude(List<string> _ls)
        {
            for (int i = 0; i < _ls.Count; i++)
            {
                if (_ls[i] != "+" && _ls[i] != "-" && _ls[i] != "*" && _ls[i] != "/" && _ls[i] != "(" && _ls[i] != ")" && _ls[i] != "（" && _ls[i] != "）")
                {
                    for (int j = 0; j < _ls[i].Length; j++)
                    {
                        if (!char.IsNumber(_ls[i][j]) && _ls[i][j] != '.')
                        {
                            _ls[i] = _ls[i].Replace(_ls[i][j].ToString(), "");
                            j = -1;
                        }
                    }
                    if (_ls[i] == "")
                    {
                        return null;
                    }
                }
            }

            return _ls;
        }

        /// <summary>
        /// 括弧运算
        /// </summary>
        /// <param name="_ls"></param>
        /// <returns></returns>
        private static List<string> Brackets(List<string> _ls)
        {
            for (int i = 0; i < _ls.Count; i++)
            {
                if (_ls[i] == "(" || _ls[i] == "（")
                {
                    int count = 1;
                    for (int j = i + 1; j < _ls.Count; j++)
                    {
                        if (_ls[j] == "(" || _ls[j] == "（")
                        {
                            count++;
                        }
                        else if ((_ls[j] == ")" || _ls[j] == "）"))
                        {
                            count--;
                            if (count == 0)
                            {
                                _ls[i] = Calculation(string.Join("", _ls.GetRange(i + 1, j - i - 1))).ToString();
                                _ls.RemoveRange(i + 1, j - i);
                                break;
                            }

                        }
                    }
                }
            }
            return _ls;
        }

        /// <summary>
        /// 进行乘除运算
        /// </summary>
        /// <param name="_ls"></param>
        /// <returns></returns>
        private static List<string> MultiplicationAndDivision(List<string> _ls)
        {
            int i = 0;
            bool flag = false;
            double d = 0;

            while (true)
            {
                for (; i < _ls.Count; i++)
                {
                    if (_ls[i] == "*")
                    {
                        d = double.Parse(_ls[i - 1]) * double.Parse(_ls[i + 1]);
                        flag = true;
                        break;
                    }
                    else if (_ls[i] == "/")
                    {
                        d = double.Parse(_ls[i - 1]) / double.Parse(_ls[i + 1]);
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    _ls[i - 1] = d.ToString();
                    _ls.RemoveRange(i, 2);
                    flag = false;
                }
                else
                {
                    return _ls;
                }
            }
        }

        /// <summary>
        /// 进行加减运算
        /// </summary>
        /// <param name="_ls"></param>
        /// <returns></returns>
        private static double AdditionAndSubtraction(List<string> _ls)
        {
            double result = double.Parse(_ls[0]);

            for (int i = 1; i < _ls.Count; i += 2)
            {
                if (_ls[i] == "+")
                {
                    result += double.Parse(_ls[i + 1]);
                }
                else if (_ls[i] == "-")
                {
                    result -= double.Parse(_ls[i + 1]);
                }
            }
            return result;
        }
        #endregion
    }
}
