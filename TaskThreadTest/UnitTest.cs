using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TaskThreadTest
{
    [TestClass]
    public class BasicTests
    {
        [TestMethod]
        public void InitializeTest()
        {

        }

        [TestMethod]
        public void ThreadCreateTest()
        {
            var thread = new Thread(() =>
            {
                Debug.WriteLine("Thread started");
            });
            thread.Start();
        }

        [TestMethod]
        public void ParameterizedThreadCreateTest()
        {
            Object passedObject = null;
            ParameterizedThreadStart pStart = delegate(object obj)
            {
                passedObject = obj;
            };

            var thread = new Thread(pStart);

            var pObject = new Object();
            thread.Start(pObject);
            thread.Join();

            Assert.AreEqual(pObject, passedObject);
        }

        [TestMethod]
        public void ValidateThreadWork()
        {
            //purposely using a separate event instead of relying on thread.Wait() to make this test independent
            var workDone = new ManualResetEventSlim(false);

            int x = 0;
            var thread = new Thread(() =>
            {
                x = 42;
                workDone.Set();
            });

            Assert.AreNotEqual(42, x);
            thread.Start();
            workDone.Wait();
            Assert.AreEqual(42, x);
        }

        [TestMethod]
        public void CurrentThreadNotNull()
        {
            Assert.IsNotNull(Thread.CurrentThread);
        }

        [TestMethod]
        public void ValidateSleep()
        {
            var start = DateTime.UtcNow;
            Thread.Sleep(600);
            Assert.IsTrue(DateTime.UtcNow > start + TimeSpan.FromMilliseconds(600));
        }

        [TestMethod]
        public void TestThreadJoin()
        {
            int x = 0;
            var thread = new Thread(() =>
            {
                Thread.Sleep(200);
                x = 42;
            });

            Assert.AreNotEqual(42, x);
            thread.Start();
            thread.Join();
            Assert.AreEqual(42, x);
        }

        [TestMethod]
        public void ThreadIdChanges()
        {
            var threadId1 = Thread.CurrentThread.ManagedThreadId;
            var threadId2 = 0;

            var thread = new Thread(() =>
            {
                threadId2 = Thread.CurrentThread.ManagedThreadId;
            });

            thread.Start();
            thread.Join();
            Assert.AreNotEqual(threadId1, threadId2);
        }

        [TestMethod]
        public void ThreadIdStaysSame()
        {
            var threadRef1 = Thread.CurrentThread;
            var threadRef2 = Thread.CurrentThread;

            Assert.AreEqual(threadRef1, threadRef2);
        }

        [TestMethod]
        public void ThreadEqualityOverriden()
        {
            Thread innerThread = null;
            var outerThread = new Thread(() =>
            {
                innerThread = Thread.CurrentThread;
            });

            outerThread.Start();
            outerThread.Join();

            Assert.AreEqual(innerThread, outerThread);
        }

        [TestMethod]
        public void CurrentThreadPreserved()
        {
            Thread thisThread = Thread.CurrentThread;
            Thread innerThread = null;
            var outerThread = new Thread(() =>
            {
                innerThread = Thread.CurrentThread;
            });

            outerThread.Start();
            outerThread.Join();

            Assert.AreNotEqual(Thread.CurrentThread, innerThread);
            Assert.AreEqual(thisThread, Thread.CurrentThread);
        }

        [TestMethod]
        public void RunsInSeparateThread()
        {
            Thread thisThread = Thread.CurrentThread;
            Thread innerThread = null;
            var outerThread = new Thread(() =>
            {
                innerThread = Thread.CurrentThread;
            });

            outerThread.Start();
            outerThread.Join();

            Assert.AreNotEqual(innerThread.ManagedThreadId, thisThread.ManagedThreadId);
        }

        [TestMethod]
        public void ExceptionsNotBubbled()
        {
            bool exceptionThrown = false;

            try
            {
                var thread = new Thread(() =>
                {
                    throw new Exception();
                });
                thread.Start();
                thread.Join();
            }
            catch
            {
                exceptionThrown = true;
            }

            Assert.IsFalse(exceptionThrown);
        }
    }
}
