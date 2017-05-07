using System;
using System.Collections.Generic;
using System.Text;

namespace System.Threading
{
    public class ThreadStateException : Exception
    {
        public ThreadStateException(string message = "", Exception innerException = null)
            : base(message, innerException)
        { }
    }
}
