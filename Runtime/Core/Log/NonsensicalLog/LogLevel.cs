namespace NonsensicalKit.Core.Log.NonsensicalLog
{
    /// <summary>
    /// 每条消息都有一个等级（不可以为OFF）,设定Log等级后，仅会Log大于等于设定等级的消息
    /// </summary>
    public enum LogLevel
    {
        Debug = 0, //显示所有消息,包括用于调试的Debug消息
        Info, //用于展示当前正处于什么状态或者正在做什么长时间的事情的消息
        Warning, //不一定是错误，但是应当进行注意时的消息
        Error, //发生了错误，但是不影响其他模块继续运行时的消息
        Fatal, //发生了可能会导致程序无法正常运行的错误时的消息
        Off, //不显示任何消息
    }
}
