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

            Assert.AreNotEqual(x, 42);
            thread.Start();
            workDone.Wait();
            Assert.AreEqual(x, 42);
        }

        [TestMethod]
        public void TestThreadJoin()
        {
            int x = 0;
            var thread = new Thread(() =>
            {
                //await Task.Delay(5000);
                x = 42;
            });

            Assert.AreNotEqual(x, 42);
            thread.Start();
            thread.Join();
            Assert.AreEqual(x, 42);
        }
    }
}
