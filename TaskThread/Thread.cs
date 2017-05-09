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
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public string Name { get; set; } = string.Empty;
        public bool IsBackground { get; set; } = false;
        public bool IsAlive => _task != null && !(_task.IsCanceled || _task.IsCompleted || _task.IsFaulted);
        public CultureInfo CurrentCulture => throw new NotImplementedException();
        private static SemaphoreSlim _unavailable = new SemaphoreSlim(0, 1);
        private static SemaphoreSlim _threadSuspend = new SemaphoreSlim(0, 1);
        public ThreadPriority Priority { get; set; } = ThreadPriority.Normal;
        public ThreadState ThreadState { get; private set; } = ThreadState.Unstarted;

        [ThreadStatic]
        public static Thread CurrentThread = new Thread()
        {
            ThreadState = ThreadState.Running
        };

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
            ThreadState = IsBackground ? ThreadState.Background : ThreadState.Running;
            action();
            ThreadState = ThreadState.Stopped;
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

            _task = new Task(() => InnerStart(() => _start()), _tokenSource.Token, TaskCreationOptions.LongRunning);
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

            _task = new Task(() => InnerStart(() => _parameterizedStart(obj)), _tokenSource.Token, TaskCreationOptions.LongRunning);
            _task.Start();
        }

        public void Join()
        {
            _task.Wait();
        }

        public bool Join(Int32 milliseconds)
        {
            return _task.Wait(milliseconds);
        }

        public bool Join(TimeSpan timeout)
        {
            return _task.Wait(timeout);
        }

        public static void Sleep(int milliseconds)
        {
            _unavailable.Wait(milliseconds);
        }

        public static void Sleep(TimeSpan duration)
        {
            _unavailable.Wait(duration);
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
                    _threadSuspend.Wait();
                    ThreadState = IsBackground ? ThreadState.Background : ThreadState.Running;
                }
            }
        }

        public void Resume()
        {
            if (this == CurrentThread && ThreadState == ThreadState.Suspended)
            {
                _threadSuspend.Release();
            }
            else if (ThreadState == ThreadState.Suspended || ThreadState == ThreadState.SuspendRequested)
            {
                ThreadState = IsBackground ? ThreadState.Background : ThreadState.Running;
            }
        }

        public static void GetDomain()
        {
            throw new NotImplementedException();
        }
    }
}
#endif