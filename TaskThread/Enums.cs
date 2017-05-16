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
        WaitSleepJoin,
        Suspended,
        AbortRequested,
        Aborted
    }

    enum StartType
    {
        Standard,
        Parameterized
    };
}
