using System;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Polls Application.internetReachability and raises Online/Offline transition events. Lighter than a full reachability library; sufficient for "show offline banner" UX.</summary>
    public class ConnectivityWatcher : MonoBehaviour
    {
        private static ConnectivityWatcher _instance;
        public static event Action<bool> OnlineChanged; // true = online

        public static bool IsOnline { get; private set; } = true;
        private float _pollInterval = 2f;
        private float _nextPoll;

        public static void EnsureRunning()
        {
            if (_instance != null) return;
            var go = new GameObject("Rinval.MobileAdsIap.ConnectivityWatcher");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<ConnectivityWatcher>();
        }

        public static void SetPollInterval(float seconds)
        {
            if (_instance != null) _instance._pollInterval = Mathf.Max(0.5f, seconds);
        }

        private void Start()
        {
            IsOnline = ConnectivityGuard.IsOnline();
            _nextPoll = Time.unscaledTime + _pollInterval;
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextPoll) return;
            _nextPoll = Time.unscaledTime + _pollInterval;
            var nowOnline = ConnectivityGuard.IsOnline();
            if (nowOnline == IsOnline) return;
            IsOnline = nowOnline;
            try { OnlineChanged?.Invoke(nowOnline); }
            catch (Exception e) { AdLogger.Error($"ConnectivityWatcher.OnlineChanged listener threw: {e}"); }
        }
    }
}
