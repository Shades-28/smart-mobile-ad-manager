using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    public class MainThreadDispatcher : MonoBehaviour
    {
        private static MainThreadDispatcher _instance;
        private static readonly object _lock = new object();
        private static int _mainThreadId = -1;
        private readonly Queue<Action> _queue = new Queue<Action>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void CaptureMainThread()
        {
            // Runs very early on the main thread, before any user code or native ad callbacks.
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public static MainThreadDispatcher Instance
        {
            get
            {
                if (_instance != null) return _instance;
                // Lazy GameObject construction must happen on the main thread; if a native
                // ad-network callback hits this first from a worker thread, Unity will throw
                // a cryptic Internal_CreateGameObject error. Detect and surface a clear one.
                if (_mainThreadId != -1 && Thread.CurrentThread.ManagedThreadId != _mainThreadId)
                {
                    throw new InvalidOperationException(
                        "MainThreadDispatcher.Instance was accessed from a background thread before " +
                        "it was initialized on the main thread. Call AdManager.Initialize(...) or " +
                        "touch MainThreadDispatcher.Instance once on the main thread at startup.");
                }
                lock (_lock)
                {
                    if (_instance != null) return _instance;
                    var go = new GameObject("Rinval.MobileAdsIap.MainThreadDispatcher");
                    _instance = go.AddComponent<MainThreadDispatcher>();
                    _mainThreadId = Thread.CurrentThread.ManagedThreadId;
                    DontDestroyOnLoad(go);
                    return _instance;
                }
            }
        }

        public static void Enqueue(Action action)
        {
            if (action == null) return;
            var inst = Instance;
            lock (inst._queue)
            {
                inst._queue.Enqueue(action);
            }
        }

        public int PendingCount
        {
            get { lock (_queue) return _queue.Count; }
        }

        private void Update()
        {
            while (true)
            {
                Action next;
                lock (_queue)
                {
                    if (_queue.Count == 0) break;
                    next = _queue.Dequeue();
                }
                try { next?.Invoke(); }
                catch (Exception e) { AdLogger.Error($"MainThreadDispatcher action threw: {e}"); }
            }
        }

        public static void ResetForTests()
        {
            if (_instance == null) return;
            lock (_instance._queue) { _instance._queue.Clear(); }
        }
    }
}
