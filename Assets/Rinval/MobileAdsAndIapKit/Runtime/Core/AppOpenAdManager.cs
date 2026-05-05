using System;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    public class AppOpenAdManager : MonoBehaviour
    {
        private static AppOpenAdManager _instance;
        private static IAppOpenAdapter _adapter;
        private static IConfigSource _configSource;

        private static float _lastShownTimeUnscaled = -9999f;
        private static float _appPausedAtTimeUnscaled = -1f;
        private static bool _firstLaunchHandled;

        public static event Action<AdResultCode> AppOpenClosed;

        public static bool IsInitialized => _instance != null && _adapter != null && _adapter.IsInitialized;
        public static MediatorKind ActiveMediator => _adapter?.Kind ?? MediatorKind.None;

        public static void Initialize(AdManagerConfig config, IConfigSource configSource = null, IAppOpenAdapter adapter = null)
        {
            if (_instance != null) { AdLogger.Warn("AppOpenAdManager already initialized"); return; }
            if (config == null) { AdLogger.Error("AppOpenAdManager.Initialize: null config"); return; }

            _configSource = configSource ?? config;
            _adapter = adapter ?? new EditorAppOpenAdapter();

            var go = new GameObject("Rinval.MobileAdsIap.AppOpenAdManager");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<AppOpenAdManager>();

            _adapter.Initialize(config, _ => { });
            _firstLaunchHandled = !_configSource.AppOpenSkipFirstLaunch;
            AdLogger.Lifecycle("AppOpenAdManager.Initialize");
        }

        public static void Shutdown()
        {
            if (_instance != null) Destroy(_instance.gameObject);
            _instance = null;
            _adapter = null;
            _configSource = null;
            AppOpenClosed = null;
            _lastShownTimeUnscaled = -9999f;
            _appPausedAtTimeUnscaled = -1f;
            _firstLaunchHandled = false;
        }

        public static void Load()
        {
            if (!Guard()) return;
            _adapter.Load();
        }

        public static bool IsReady() =>
            Guard(silent: true) && _adapter.IsReady();

        public static bool TryShow(Action<AdResultCode> callback = null)
        {
            if (!Guard()) { callback?.Invoke(AdResultCode.Disabled); return false; }
            if (!CanShow(out var reason))
            {
                AdLogger.Lifecycle("AppOpenAdManager.TryShow", $"blocked: {reason}");
                callback?.Invoke(AdResultCode.NotReady);
                return false;
            }
            if (!_adapter.IsReady()) { callback?.Invoke(AdResultCode.NotReady); return false; }
            _adapter.Show(code =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    if (code == AdResultCode.Closed || code == AdResultCode.Shown)
                        _lastShownTimeUnscaled = Time.unscaledTime;
                    AppOpenClosed?.Invoke(code);
                    callback?.Invoke(code);
                });
            });
            return true;
        }

        public static bool CanShow(out string reason)
        {
            reason = string.Empty;
            if (RemoveAdsManager.IsActive) { reason = "ads removed via IAP"; return false; }
            if (!_configSource.AppOpenEnabled) { reason = "app-open disabled"; return false; }
            if (!_firstLaunchHandled) { reason = "skip first launch"; return false; }
            float now = Time.unscaledTime;
            if (now - _lastShownTimeUnscaled < _configSource.AppOpenCooldownSeconds)
            { reason = $"cooldown {now - _lastShownTimeUnscaled:0}s < {_configSource.AppOpenCooldownSeconds}"; return false; }
            if (_appPausedAtTimeUnscaled > 0f && now - _appPausedAtTimeUnscaled < _configSource.AppOpenMinAwaySeconds)
            { reason = $"away {now - _appPausedAtTimeUnscaled:0}s < {_configSource.AppOpenMinAwaySeconds}"; return false; }
            return true;
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                _appPausedAtTimeUnscaled = Time.unscaledTime;
            }
            else
            {
                _firstLaunchHandled = true;
                if (CanShow(out _) && _adapter != null && _adapter.IsReady())
                    TryShow();
            }
        }

        private static bool Guard(bool silent = false)
        {
            if (_adapter == null || _configSource == null)
            {
                if (!silent) AdLogger.Error("AppOpenAdManager not initialized");
                return false;
            }
            if (!_configSource.AdsEnabled) return false;
            return true;
        }
    }

    internal class EditorAppOpenAdapter : IAppOpenAdapter
    {
        public MediatorKind Kind => MediatorKind.Editor;
        public bool IsInitialized { get; private set; }
        private bool _loaded;
        public void Initialize(AdManagerConfig config, Action<AdRevenueInfo> onRevenuePaid) { IsInitialized = true; }
        public void Load() => _loaded = true;
        public bool IsReady() => _loaded;
        public void Show(Action<AdResultCode> callback)
        {
            if (!_loaded) { callback?.Invoke(AdResultCode.NotReady); return; }
            _loaded = false;
            callback?.Invoke(AdResultCode.Closed);
        }
    }
}
