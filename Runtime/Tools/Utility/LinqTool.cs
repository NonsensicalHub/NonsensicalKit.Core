using System;
using System.Linq.Expressions;

namespace NonsensicalKit.Tools
{
    public static class LinqTool
    {
        /// <summary>
        /// 获取变量名
        /// 用法：
        /// string a = "Value";
        /// string s = GetVarName(p => a);
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public static string GetVarName(Expression<Func<string, string>> exp)
        {
            return ((MemberExpression)exp.Body).Member.Name;
        }
    }
}
