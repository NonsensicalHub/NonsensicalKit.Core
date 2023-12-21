namespace NonsensicalKit.Editor.Log.NonsensicalLog
{
    /// <summary>
    /// 每条消息都有一个等级（不可以为OFF）,设定Log等级后，仅会Log大于等于设定等级的消息
    /// </summary>
    public enum LogLevel
    {
        DEBUG = 1,  //显示所有消息,包括用于调试的Debug消息
        INFO,       //用于展示当前正处于什么状态或者正在做什么长时间的事情的消息
        WARNING,    //不一定是错误，但是应当进行注意时的消息
        ERROR,      //发生了错误，但是不影响其他模块继续运行时的消息
        FATAL,      //发生了会导致程序无法正常运行的错误时的消息
        OFF,        //不显示任何消息
    }
}