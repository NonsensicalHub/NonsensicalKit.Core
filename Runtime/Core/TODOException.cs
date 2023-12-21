using System;

namespace NonsensicalKit.Editor
{
    /// <summary>
    /// 待处理异常
    /// 这种异常用于标记那些在开发阶段尚未做完的部分
    /// 或者是可以在开发阶段完全修复的错误
    /// </summary>
    public class TODOException : Exception
    {
        private readonly string _message;
        
        public TODOException()
        {
            _message = "待处理";
        }
        public TODOException(string message)
        {
            _message = message;
        }
        public override string Message
        {
            get
            {
                return "TODO:" + _message;
            }
        }
    }
}
