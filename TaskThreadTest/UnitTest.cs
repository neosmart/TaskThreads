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
        public void ValidateThreadWork()
        {
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
    }
}
