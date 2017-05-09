#if NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2 || NETSTANDARD1_3 || NETSTANDARD1_4 || NETSTANDARD1_5 || NETSTANDARD1_6
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace System.Threading
{
    public delegate void ParameterizedThreadStart(object obj);
    public delegate void ThreadStart();

    public class Thread
    {
        private Task _task;

        public string Name { get; set; } = string.Empty;
        private bool _isBackground = false;
        public bool IsBackground
        {
            get => _isBackground;
            set
            {
                if (ThreadState != ThreadState.Unstarted)
                {
                    throw new ThreadStateException("Cannot change IsBackground property after thread has been started!");
                }
                _isBackground = value;
            }
        }
        public bool IsAlive { get; private set; } = false;
        public CultureInfo CurrentCulture { get; private set; }
        private ManualResetEventSlim _taskStarted = new ManualResetEventSlim(false);
        private static SemaphoreSlim _unavailable = new SemaphoreSlim(0, 1);
        private SemaphoreSlim _threadSuspend = null;
        public ThreadPriority Priority { get; set; } = ThreadPriority.Normal;
        public ThreadState ThreadState { get; private set; } = ThreadState.Unstarted;

        [ThreadStatic]
        private static Thread _currentThread;
        public static Thread CurrentThread
        {
            get
            {
                if (_currentThread == null)
                {
                    _currentThread = new Thread()
                    {
                        CurrentCulture = CultureInfo.CurrentCulture,
                        _managedThreadId = Interlocked.Increment(ref _globalThreadId),
                        ThreadState = ThreadState.Running,
                        IsAlive = true,
                    };
                }
                return _currentThread;
            }
        }

        private static int _globalThreadId = 0;
        private int _managedThreadId = -42;
        public int ManagedThreadId
        {
            get
            {
                if (_managedThreadId == -42)
                {
                    throw new ThreadStateException("Cannot get managed thread id of unstarted thread!");
                }
                return _managedThreadId;
            }
        }

        private enum StartType
        {
            Standard,
            Parameterized
        };

        StartType _startType;
        ParameterizedThreadStart _parameterizedStart;
        ThreadStart _start;

        private Thread() { }

        public Thread(ParameterizedThreadStart threadStart, int maxStackSize = 0)
        {
            _startType = StartType.Parameterized;
            _parameterizedStart = threadStart;
        }

        public Thread(ThreadStart threadStart, int maxStackSize = 0)
        {
            _startType = StartType.Standard;
            _start = threadStart;
        }

        private void InnerStart(Action action)
        {
            _currentThread = this;
            CurrentCulture = CultureInfo.CurrentCulture;
            _managedThreadId = Interlocked.Increment(ref _globalThreadId);
            IsAlive = true;
            ThreadState = IsBackground ? ThreadState.Background : ThreadState.Running;
            _taskStarted.Set();
            //free up the reference to _taskStarted, we'll only wait on it if it's not null
            //since we do this after _task has been initialized, callers will see either _task or _taskStarted
            _taskStarted = null;

            try
            {
                action();
            }
            catch
            {
                //absorb all exceptions
            }
            finally
            {
                IsAlive = false;
                ThreadState = ThreadState.Stopped;
            }
        }

        public void Start()
        {
            if (_startType == StartType.Parameterized)
            {
                throw new InvalidOperationException("Must supply argument for ParameterizedThreadStart!");
            }

            if (_task != null)
            {
                throw new ThreadStateException("Thread already started!");
            }

            _task = new Task(() => InnerStart(() => _start()), TaskCreationOptions.LongRunning);
            _task.Start();
        }

        public void Start(object obj)
        {
            if (_startType == StartType.Standard)
            {
                throw new InvalidOperationException("Must use parameterless Start() method instead!");
            }

            if (_task != null)
            {
                throw new ThreadStateException("Thread already started!");
            }

            _task = new Task(() => InnerStart(() => _parameterizedStart(obj)), TaskCreationOptions.LongRunning);
            _task.Start();
        }

        public void Join()
        {
            if (this == CurrentThread)
            {
                //stop caller from doing something stupid
                return;
            }
            if (_task == null)
            {
                throw new ThreadStateException("Cannot join an unstarted thread!");
            }

            CurrentThread.ThreadState = ThreadState.WaitSleepJoin;
            _taskStarted?.Wait();
            _task.Wait();
            CurrentThread.ThreadState = IsBackground ? ThreadState.Background : ThreadState.Running;
        }

        public bool Join(Int32 milliseconds)
        {
            if (this == CurrentThread)
            {
                //stop caller from doing something stupid
                return true;
            }
            if (_task == null)
            {
                throw new ThreadStateException("Cannot join an unstarted thread!");
            }

            CurrentThread.ThreadState = ThreadState.WaitSleepJoin;
            _taskStarted?.Wait();
            bool waitResult = _task.Wait(milliseconds);
            CurrentThread.ThreadState = IsBackground ? ThreadState.Background : ThreadState.Running;

            return waitResult;
        }

        public bool Join(TimeSpan timeout)
        {
            if (this == CurrentThread)
            {
                //stop caller from doing something stupid
                return true;
            }
            if (_task == null)
            {
                throw new ThreadStateException("Cannot join an unstarted thread!");
            }

            CurrentThread.ThreadState = ThreadState.WaitSleepJoin;
            _taskStarted?.Wait();
            bool waitResult = _task.Wait(timeout);
            CurrentThread.ThreadState = CurrentThread.IsBackground ? ThreadState.Background : ThreadState.Running;

            return waitResult;
        }

        public static void Sleep(int milliseconds)
        {
            CurrentThread.ThreadState = ThreadState.WaitSleepJoin;
            _unavailable.Wait(milliseconds);
            CurrentThread.ThreadState = CurrentThread.IsBackground ? ThreadState.Background : ThreadState.Running;
        }

        public static void Sleep(TimeSpan duration)
        {
            CurrentThread.ThreadState = ThreadState.WaitSleepJoin;
            _unavailable.Wait(duration);
            CurrentThread.ThreadState = CurrentThread.IsBackground ? ThreadState.Background : ThreadState.Running;
        }

        public void Abort()
        {
            //throw new NotImplementedException();
            if (IsAlive)
            {
                ThreadState = ThreadState.AbortRequested;
            }

            if (this == CurrentThread)
            {
                ThreadState = ThreadState.Aborted;
                throw new ThreadAbortException();
            }
        }

        public void Suspend()
        {
            //throw new NotImplementedException();
            if (ThreadState == ThreadState.Running || ThreadState == ThreadState.Background)
            {
                ThreadState = ThreadState.SuspendRequested;

                if (this == CurrentThread)
                {
                    ThreadState = ThreadState.Suspended;
                    _threadSuspend = new SemaphoreSlim(0, 1);
                    _threadSuspend.Wait();
                    ThreadState = IsBackground ? ThreadState.Background : ThreadState.Running;
                }
            }
        }

        public void Resume()
        {
            if (_threadSuspend != null)
            {
                _threadSuspend.Release();
            }
            else if (ThreadState == ThreadState.Suspended || ThreadState == ThreadState.SuspendRequested)
            {
                ThreadState = IsBackground ? ThreadState.Background : ThreadState.Running;
            }
        }
    }
}
#endif