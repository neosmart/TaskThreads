using System;
using System.Collections.Generic;
using System.Text;

namespace TaskThread
{
    class Exceptions
    {
        public class ThreadStateException : Exception
        {
            public ThreadStateException(string message = "", Exception innerException = null)
                : base(message, innerException)
            { }
        }
    }
}
