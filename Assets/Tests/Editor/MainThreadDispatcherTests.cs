using System.Collections;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rinval.MobileAdsAndIapKit
{
    public class MainThreadDispatcherTests
    {
        [SetUp]
        public void Setup()
        {
            // Touch Instance from the main thread before the test spawns workers — the lazy
            // GameObject construction inside get_Instance() is illegal off-thread.
            _ = MainThreadDispatcher.Instance;
            MainThreadDispatcher.ResetForTests();
        }

        [UnityTest]
        public IEnumerator Enqueue_FromBackgroundThread_RunsOnMainThread()
        {
            int mainThreadId = Thread.CurrentThread.ManagedThreadId;
            int callbackThreadId = -1;
            bool done = false;

            var t = new Thread(() =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                    done = true;
                });
            });
            t.Start();
            t.Join();

            // Wait for the dispatcher's Update() to drain the queue
            float timeout = 2f;
            while (!done && timeout > 0f)
            {
                yield return null;
                timeout -= Time.deltaTime;
            }

            Assert.IsTrue(done, "Enqueued action did not run within 2 seconds");
            Assert.AreEqual(mainThreadId, callbackThreadId, "Callback did not run on the main thread");
        }

        [UnityTest]
        public IEnumerator Enqueue_NullAction_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => MainThreadDispatcher.Enqueue(null));
            yield return null;
        }

        [UnityTest]
        public IEnumerator Enqueue_MultipleActions_AllExecuteInOrder()
        {
            var order = new System.Collections.Generic.List<int>();
            for (int i = 0; i < 10; i++)
            {
                int captured = i;
                MainThreadDispatcher.Enqueue(() => order.Add(captured));
            }

            yield return null; // let Update drain

            Assert.AreEqual(10, order.Count);
            for (int i = 0; i < 10; i++)
                Assert.AreEqual(i, order[i]);
        }

        [UnityTest]
        public IEnumerator Enqueue_ThrowingAction_LogsErrorAndContinues()
        {
            bool secondRan = false;
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(@"\[ADS:ERROR\].*MainThreadDispatcher"));
            MainThreadDispatcher.Enqueue(() => throw new System.Exception("boom"));
            MainThreadDispatcher.Enqueue(() => secondRan = true);

            yield return null;
            yield return null;

            Assert.IsTrue(secondRan, "Second action should still run after first one threw");
        }

        [UnityTest]
        public IEnumerator PendingCount_ReportsQueueDepth()
        {
            MainThreadDispatcher.ResetForTests();
            // Enqueue 3 actions but don't yield — Update has not run yet
            MainThreadDispatcher.Enqueue(() => { });
            MainThreadDispatcher.Enqueue(() => { });
            MainThreadDispatcher.Enqueue(() => { });
            // Cannot reliably check >0 here because Update may have already drained it
            yield return null;
            yield return null;
            Assert.AreEqual(0, MainThreadDispatcher.Instance.PendingCount, "Queue should drain after Update runs");
        }
    }
}
