using System;
using System.Linq.Expressions;

namespace NonsensicalKit.Tools
{
    /// <summary>
    /// 表达式辅助工具
    /// </summary>
    public static class LinqTool
    {
        /// <summary>
        /// 获取变量名
        /// 用法：
        /// string a = "Value";
        /// string s = GetVarName(() => a);
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string GetVarName<T>(Expression<Func<T>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            Expression body = expression.Body;
            if (body is UnaryExpression unaryExpression &&
                unaryExpression.Operand is MemberExpression unaryMemberExpression)
            {
                return unaryMemberExpression.Member.Name;
            }

            if (body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }

            // 显式抛错，避免调用方在表达式不合法时拿到静默错误结果。
            throw new ArgumentException("Expression body must be a member expression.", nameof(expression));
        }

        [Obsolete("Use GetVarName<T>(Expression<Func<T>>) instead.")]
        public static string GetVarName(Expression<Func<string, string>> exp)
        {
            if (exp == null)
            {
                throw new ArgumentNullException(nameof(exp));
            }

            if (exp.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }

            throw new ArgumentException("Expression body must be a member expression.", nameof(exp));
        }
    }
}
