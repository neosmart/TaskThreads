namespace System.Threading
{
    public enum ThreadPriority
    {
        Lowest,
        BelowNormal,
        Normal,
        AboveNormal,
        Highest
    }

    [FlagsAttribute]
    public enum ThreadState
    {
        Running,
        StopRequested,
        SuspendRequested,
        Background,
        Unstarted,
        Stopped,
        Suspended,
        AbortRequested,
        Aborted
    }
}
