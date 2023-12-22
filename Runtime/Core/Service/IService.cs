using System;

namespace NonsensicalKit.Core.Service
{
    public interface IService
    {
        /// <summary>
        /// 是否已经准备好（初始化完成）
        /// </summary>
        public bool IsReady { get;}

        /// <summary>
        /// 初始化完成回调
        /// </summary>
        public Action InitCompleted { get; set; }
    }
}
