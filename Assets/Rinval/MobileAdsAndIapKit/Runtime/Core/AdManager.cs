using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    public class AdManager : MonoBehaviour
    {
        private static AdManager _instance;
        private static INetworkAdapter _adapter;
        private static AdManagerConfig _config;
        private static IConfigSource _configSource;

        private static AdLoadScheduler _interstitialScheduler;
        private static AdLoadScheduler _rewardedScheduler;
        private static AdLoadScheduler _mrecScheduler;

        private static float _lastInterstitialTimeUnscaled = -9999f;
        private static int _interstitialsInWindow;
        private static float _windowStartTimeUnscaled;
        private static int _interstitialsShownTotal;
        private static int _currentStage;

        public static event Action<AdRevenueInfo> RevenuePaid;
        public static event Action<AdResultCode> InterstitialClosed;
        public static event Action<AdResultCode> RewardedClosed;
        public static event Action<AdFormat> Loaded;
        public static event Action<AdFormat> LoadFailed;

        private static readonly PlacementCapTracker _placementTracker = new PlacementCapTracker();
        private static IRewardedSsvValidator _ssvValidator;

        /// <summary>The most recent decision a Show* call made. Useful for "why didn't anything happen?" diagnostics.</summary>
        public static ShowVerdict LastShowVerdict { get; private set; }

        /// <summary>Registers a server-side reward validator. Pass null to remove.</summary>
        public static void SetSsvValidator(IRewardedSsvValidator validator) => _ssvValidator = validator;

        public static bool IsInitialized => _adapter != null && _adapter.IsInitialized;
        public static MediatorKind ActiveMediator => _adapter?.Kind ?? MediatorKind.None;
        public static string ActiveMediatorName => _adapter?.DisplayName ?? "(none)";
        public static int InterstitialsShownTotal => _interstitialsShownTotal;
        public static AdManagerConfig Config => _config;
        public static IConfigSource ConfigSource => _configSource;

        public static void Initialize(AdManagerConfig config, IConfigSource configSource = null)
        {
            if (_instance != null)
            {
                AdLogger.Warn("AdManager.Initialize called twice - ignoring");
                return;
            }
            if (config == null)
            {
                AdLogger.Error("AdManager.Initialize called with null config");
                return;
            }

            _config = config;
            _configSource = configSource ?? config;

            AdLogger.SetVerbose(_configSource.VerboseLogging);
            AdLogger.Lifecycle("AdManager.Initialize", $"mediator={config.Mediator}");

            var go = new GameObject("Rinval.MobileAdsIap.AdManager");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<AdManager>();

            _adapter = SelectAdapter(config);
            _adapter.Initialize(config, OnRevenuePaidInternal);

#if UNITY_EDITOR
            // In editor, auto-register a simulated native adapter so devs can preview their UI
            // without device builds. Production adapters self-register via RuntimeInitializeOnLoad.
            if (NativeAdLoader.Adapter == null)
            {
                var nativeAdapter = new EditorNativeAdAdapter();
                nativeAdapter.Initialize(config, OnRevenuePaidInternal);
                NativeAdLoader.RegisterAdapter(nativeAdapter);
            }
#endif

            _windowStartTimeUnscaled = Time.unscaledTime;
            _ = MainThreadDispatcher.Instance;

            _placementTracker.SetRules(_configSource.PlacementRules);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            AdDebugOverlay.EnsureRunning();
#endif

            // Schedulers handle retry+backoff and auto-preload. StubAdapter (ads disabled)
            // is excluded - its loads are no-ops and we don't want a useless retry loop.
            if (!(_adapter is StubAdapter))
            {
                _interstitialScheduler = new AdLoadScheduler(
                    AdFormat.Interstitial,
                    onLoaded => _adapter.LoadInterstitial(onLoaded),
                    () => _adapter.IsInterstitialReady());
                _interstitialScheduler.OnAttemptCompleted += success => RaiseLoadEvent(AdFormat.Interstitial, success);

                _rewardedScheduler = new AdLoadScheduler(
                    AdFormat.Rewarded,
                    onLoaded => _adapter.LoadRewarded(onLoaded),
                    () => _adapter.IsRewardedReady());
                _rewardedScheduler.OnAttemptCompleted += success => RaiseLoadEvent(AdFormat.Rewarded, success);

                _mrecScheduler = new AdLoadScheduler(
                    AdFormat.MediumRectangle,
                    onLoaded => _adapter.LoadMrec(onLoaded),
                    () => _adapter.IsMrecReady());
                _mrecScheduler.OnAttemptCompleted += success => RaiseLoadEvent(AdFormat.MediumRectangle, success);
            }
        }

        private static void RaiseLoadEvent(AdFormat fmt, bool success)
        {
            try
            {
                if (success) Loaded?.Invoke(fmt);
                else LoadFailed?.Invoke(fmt);
            }
            catch (Exception e) { AdLogger.Error($"AdManager.{(success ? "Loaded" : "LoadFailed")} listener threw: {e}"); }
        }

        private static System.Func<INetworkAdapter> _adapterFactory;

        public static void RegisterAdapter(System.Func<INetworkAdapter> factory)
        {
            _adapterFactory = factory;
        }

        private static INetworkAdapter SelectAdapter(AdManagerConfig config)
        {
            if (TestLabDetector.IsTestLab())
            {
                AdLogger.Warn("Test Lab detected - using StubAdapter (ads disabled)");
                return new StubAdapter();
            }

#if UNITY_EDITOR
            return new EditorAdapter();
#else
            if (_adapterFactory != null)
            {
                var adapter = _adapterFactory();
                if (adapter != null) return adapter;
            }
            AdLogger.Warn("No mediator adapter registered - using StubAdapter");
            return new StubAdapter();
#endif
        }

        public static void Shutdown()
        {
            if (_instance != null) Destroy(_instance.gameObject);
            _instance = null;
            _adapter = null;
            _config = null;
            _configSource = null;
            _interstitialScheduler = null;
            _rewardedScheduler = null;
            _mrecScheduler = null;
            RevenuePaid = null;
            InterstitialClosed = null;
            RewardedClosed = null;
            Loaded = null;
            LoadFailed = null;
            _placementTracker.Reset();
            _lastInterstitialTimeUnscaled = -9999f;
            _interstitialsInWindow = 0;
            _interstitialsShownTotal = 0;
            _currentStage = 0;
            // Adapter factory is intentionally preserved across Shutdown - registered once at app start
        }

        public static void SetCurrentStage(int stage) => _currentStage = stage;

        // - Async overloads -
        /// <summary>Async show-rewarded. Returns AdResult wrapping the raw code.</summary>
        public static Task<AdResult> ShowRewardedAsync(string placement)
        {
            var tcs = new TaskCompletionSource<AdResult>();
            ShowRewarded(placement, code => tcs.TrySetResult(new AdResult(code)));
            return tcs.Task;
        }

        /// <summary>Async show-interstitial. Returns AdResult wrapping the raw code.</summary>
        public static Task<AdResult> ShowInterstitialAsync(string placement)
        {
            var tcs = new TaskCompletionSource<AdResult>();
            bool attempted = ShowInterstitial(placement, code => tcs.TrySetResult(new AdResult(code)));
            if (!attempted && !tcs.Task.IsCompleted)
                tcs.TrySetResult(new AdResult(AdResultCode.NotReady));
            return tcs.Task;
        }

        // - Banner -
        public static void LoadBanner(BannerAnchor anchor)
        {
            if (!GuardEnabled() || !_configSource.BannersEnabled) return;
            _adapter.LoadBanner(anchor);
        }

        public static void LoadBanner(BannerAnchor anchor, BannerSize size)
        {
            if (!GuardEnabled() || !_configSource.BannersEnabled) return;
            _adapter.LoadBanner(anchor, size);
        }

        public static void SetBannerAutoRefresh(bool enabled)
        {
            if (_adapter == null) return;
            _adapter.SetBannerAutoRefresh(enabled);
        }

        public static void ShowBanner()
        {
            if (!GuardEnabled() || !_configSource.BannersEnabled) return;
            if (RemoveAdsManager.IsActive) { AdLogger.Lifecycle("AdManager.ShowBanner", "skipped: ads removed via IAP"); return; }
            _adapter.ShowBanner();
        }

        public static void HideBanner()
        {
            if (_adapter == null) return;
            _adapter.HideBanner();
        }

        public static void DestroyBanner()
        {
            if (_adapter == null) return;
            _adapter.DestroyBanner();
        }

        // - Interstitial -
        public static void LoadInterstitial()
        {
            if (!GuardEnabled() || !_configSource.InterstitialsEnabled) return;
            if (_interstitialScheduler != null) _interstitialScheduler.Enable();
            else _adapter.LoadInterstitial(null);
        }

        public static bool IsInterstitialReady() =>
            GuardEnabled() && _configSource.InterstitialsEnabled && _adapter.IsInterstitialReady();

        public static bool ShowInterstitial(string placement, Action<AdResultCode> callback = null)
        {
            if (!GuardEnabled() || !_configSource.InterstitialsEnabled)
            {
                LastShowVerdict = ShowVerdict.Blocked("interstitials disabled", placement, AdFormat.Interstitial);
                callback?.Invoke(AdResultCode.Disabled);
                return false;
            }
            if (RemoveAdsManager.IsActive)
            {
                LastShowVerdict = ShowVerdict.Blocked("ads removed via IAP", placement, AdFormat.Interstitial);
                callback?.Invoke(AdResultCode.Disabled);
                return false;
            }
            if (!CanShowInterstitial(out var reason))
            {
                LastShowVerdict = ShowVerdict.Blocked(reason, placement, AdFormat.Interstitial);
                AdLogger.Network(_adapter.DisplayName, $"interstitial gated: {reason}");
                callback?.Invoke(AdResultCode.NotReady);
                return false;
            }
            if (!_placementTracker.CanShow(placement, Time.unscaledTime, out var placementReason))
            {
                LastShowVerdict = ShowVerdict.Blocked(placementReason, placement, AdFormat.Interstitial);
                AdLogger.Network(_adapter.DisplayName, $"interstitial gated: {placementReason}");
                callback?.Invoke(AdResultCode.NotReady);
                return false;
            }
            LastShowVerdict = ShowVerdict.Ok(placement, AdFormat.Interstitial);
            _adapter.ShowInterstitial(placement, code =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    if (code == AdResultCode.Closed || code == AdResultCode.Shown)
                    {
                        _lastInterstitialTimeUnscaled = Time.unscaledTime;
                        _interstitialsInWindow++;
                        _interstitialsShownTotal++;
                        _placementTracker.RecordShown(placement, Time.unscaledTime);
                        _interstitialScheduler?.OnAdConsumed();
                    }
                    InterstitialClosed?.Invoke(code);
                    callback?.Invoke(code);
                });
            });
            return true;
        }

        public static bool CanShowInterstitial(out string reason)
        {
            reason = string.Empty;
            if (!_configSource.InterstitialsEnabled) { reason = "interstitials disabled in config"; return false; }
            if (_currentStage < _configSource.InterstitialMinStage) { reason = $"stage {_currentStage} < min {_configSource.InterstitialMinStage}"; return false; }
            if (_interstitialsShownTotal == 0 && _configSource.SkipFirstInterstitial) { reason = "skip first interstitial"; return false; }

            float now = Time.unscaledTime;
            if (now - _lastInterstitialTimeUnscaled < _configSource.InterstitialMinIntervalSeconds)
            {
                reason = $"interval cap: {now - _lastInterstitialTimeUnscaled:0.0}s < {_configSource.InterstitialMinIntervalSeconds}s";
                return false;
            }

            if (now - _windowStartTimeUnscaled > _configSource.InterstitialWindowSeconds)
            {
                _windowStartTimeUnscaled = now;
                _interstitialsInWindow = 0;
            }
            if (_interstitialsInWindow >= _configSource.InterstitialMaxPerWindow)
            {
                reason = $"window cap: {_interstitialsInWindow} >= {_configSource.InterstitialMaxPerWindow}";
                return false;
            }
            return true;
        }

        // - Rewarded -
        public static void LoadRewarded()
        {
            if (!GuardEnabled() || !_configSource.RewardedEnabled) return;
            if (_rewardedScheduler != null) _rewardedScheduler.Enable();
            else _adapter.LoadRewarded(null);
        }

        public static bool IsRewardedReady() =>
            GuardEnabled() && _configSource.RewardedEnabled && _adapter.IsRewardedReady();

        public static void ShowRewarded(string placement, Action<AdResultCode> callback = null)
        {
            if (!GuardEnabled() || !_configSource.RewardedEnabled)
            {
                callback?.Invoke(AdResultCode.Disabled);
                return;
            }
            if (!_adapter.IsRewardedReady())
            {
                callback?.Invoke(AdResultCode.NotReady);
                return;
            }
            _adapter.ShowRewarded(placement, code =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    if (code == AdResultCode.Rewarded || code == AdResultCode.Cancelled || code == AdResultCode.Closed)
                    {
                        _placementTracker.RecordShown(placement, Time.unscaledTime);
                        _rewardedScheduler?.OnAdConsumed();
                    }
                    if (code == AdResultCode.Rewarded && _ssvValidator != null)
                    {
                        _ssvValidator.Validate(placement, granted =>
                        {
                            MainThreadDispatcher.Enqueue(() =>
                            {
                                var finalCode = granted ? AdResultCode.Rewarded : AdResultCode.Cancelled;
                                if (!granted)
                                    AdLogger.Warn($"SSV denied reward for placement '{placement}'");
                                RewardedClosed?.Invoke(finalCode);
                                callback?.Invoke(finalCode);
                            });
                        });
                    }
                    else
                    {
                        RewardedClosed?.Invoke(code);
                        callback?.Invoke(code);
                    }
                });
            });
        }

        // - Rewarded Interstitial -
        // Distinct on AdMob; on AppLovin/LevelPlay routes to standard rewarded.
        public static void LoadRewardedInterstitial()
        {
            if (!GuardEnabled() || !_configSource.RewardedEnabled) return;
            _adapter.LoadRewardedInterstitial(null);
        }

        public static bool IsRewardedInterstitialReady() =>
            GuardEnabled() && _configSource.RewardedEnabled && _adapter.IsRewardedInterstitialReady();

        public static void ShowRewardedInterstitial(string placement, Action<AdResultCode> callback = null)
        {
            if (!GuardEnabled() || !_configSource.RewardedEnabled)
            {
                callback?.Invoke(AdResultCode.Disabled);
                return;
            }
            if (!_adapter.IsRewardedInterstitialReady())
            {
                callback?.Invoke(AdResultCode.NotReady);
                return;
            }
            _adapter.ShowRewardedInterstitial(placement, code =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    if (code == AdResultCode.Rewarded || code == AdResultCode.Cancelled || code == AdResultCode.Closed)
                        _placementTracker.RecordShown(placement, Time.unscaledTime);
                    RewardedClosed?.Invoke(code);
                    callback?.Invoke(code);
                });
            });
        }

        public static Task<AdResult> ShowRewardedInterstitialAsync(string placement)
        {
            var tcs = new TaskCompletionSource<AdResult>();
            ShowRewardedInterstitial(placement, code => tcs.TrySetResult(new AdResult(code)));
            return tcs.Task;
        }

        // - MREC -
        public static void LoadMrec()
        {
            if (!GuardEnabled() || !_configSource.BannersEnabled) return;
            if (_mrecScheduler != null) _mrecScheduler.Enable();
            else _adapter.LoadMrec(null);
        }

        public static bool IsMrecReady() =>
            GuardEnabled() && _configSource.BannersEnabled && _adapter.IsMrecReady();

        public static void ShowMrec()
        {
            if (!GuardEnabled() || !_configSource.BannersEnabled) return;
            if (RemoveAdsManager.IsActive) { AdLogger.Lifecycle("AdManager.ShowMrec", "skipped: ads removed via IAP"); return; }
            _adapter.ShowMrec();
        }

        public static void HideMrec()
        {
            if (_adapter == null) return;
            _adapter.HideMrec();
        }

        // -
        private void OnApplicationPause(bool paused) => _adapter?.OnApplicationPause(paused);

        private void Update()
        {
            float now = Time.unscaledTime;
            _interstitialScheduler?.Tick(now);
            _rewardedScheduler?.Tick(now);
            _mrecScheduler?.Tick(now);
        }

        private static bool GuardEnabled()
        {
            if (_adapter == null || _configSource == null)
            {
                AdLogger.Error("AdManager not initialized - call AdManager.Initialize first");
                return false;
            }
            if (!_configSource.AdsEnabled)
            {
                AdLogger.Lifecycle("AdManager", "ads master switch is OFF");
                return false;
            }
            return true;
        }

        private static void OnRevenuePaidInternal(AdRevenueInfo info)
        {
            MainThreadDispatcher.Enqueue(() =>
            {
                AdLogger.Network(info.NetworkName, $"revenue: {info}");
                RevenuePaid?.Invoke(info);
            });
        }
    }
}
