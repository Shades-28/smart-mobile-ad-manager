#if AD_USE_APPLOVIN
using System;
using UnityEngine;
namespace Rinval.MobileAdsAndIapKit
{
    public class AppLovinAdapter : INetworkAdapter
    {
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterFactory()
        {
            AdManager.RegisterAdapter(() => new AppLovinAdapter());
        }

        public MediatorKind Kind => MediatorKind.AppLovinMax;
        public string DisplayName => "AppLovin MAX";
        public bool IsInitialized { get; private set; }

        private AdManagerConfig _config;
        private Action<AdRevenueInfo> _onRevenuePaid;
        private Action<AdResultCode> _interstitialCallback;
        private Action<AdResultCode> _rewardedCallback;
        private string _interstitialPlacement;
        private string _rewardedPlacement;

        private Action<bool> _interstitialLoadCallback;
        private Action<bool> _rewardedLoadCallback;
        private Action<bool> _mrecLoadCallback;

        public void Initialize(AdManagerConfig config, Action<AdRevenueInfo> onRevenuePaid)
        {
            try
            {
                _config = config;
                _onRevenuePaid = onRevenuePaid;

                if (TestDeviceRegistry.Ids.Count > 0)
                {
                    var ids = new System.Collections.Generic.List<string>(TestDeviceRegistry.Ids);
                    MaxSdk.SetTestDeviceAdvertisingIdentifiers(ids.ToArray());
                }

                MaxSdkCallbacks.OnSdkInitializedEvent += _ =>
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        try
                        {
                            MaxSdk.SetCreativeDebuggerEnabled(_config.TestMode);
                            MaxSdk.SetVerboseLogging(_config.VerboseLogging);

                            WireInterstitialCallbacks();
                            WireRewardedCallbacks();
                            WireBannerCallbacks();
                            WireMrecCallbacks();

                            IsInitialized = true;
                            AdLogger.Lifecycle("AppLovinAdapter.Initialize", "MAX SDK ready");
                        }
                        catch (Exception inner) { AdLogger.Error($"AppLovinAdapter post-init threw: {inner}"); }
                    });
                };
                MaxSdk.InitializeSdk();
            }
            catch (Exception e) { AdLogger.Error($"AppLovinAdapter.Initialize threw: {e}"); }
        }

        public void OnApplicationPause(bool paused) { }

        // Banner -
        private string _bannerId;
        public void LoadBanner(BannerAnchor anchor) => LoadBanner(anchor, BannerSize.Standard);

        public void LoadBanner(BannerAnchor anchor, BannerSize size)
        {
            try
            {
                _bannerId = _config.GetBannerId();
                if (string.IsNullOrEmpty(_bannerId)) { AdLogger.Warn("AppLovin banner ID is empty"); return; }
                MaxSdk.CreateBanner(_bannerId, ToMaxBannerPosition(anchor));
                if (size == BannerSize.Adaptive)
                    MaxSdk.SetBannerExtraParameter(_bannerId, "adaptive_banner", "true");
            }
            catch (Exception e) { AdLogger.Error($"AppLovinAdapter.LoadBanner threw: {e}"); }
        }

        public void ShowBanner()
        {
            try { if (!string.IsNullOrEmpty(_bannerId)) MaxSdk.ShowBanner(_bannerId); }
            catch (Exception e) { AdLogger.Error($"AppLovinAdapter.ShowBanner threw: {e}"); }
        }

        public void HideBanner()
        {
            try { if (!string.IsNullOrEmpty(_bannerId)) MaxSdk.HideBanner(_bannerId); }
            catch (Exception e) { AdLogger.Error($"AppLovinAdapter.HideBanner threw: {e}"); }
        }

        public void DestroyBanner()
        {
            try
            {
                if (string.IsNullOrEmpty(_bannerId)) return;
                MaxSdk.DestroyBanner(_bannerId);
                _bannerId = null;
            }
            catch (Exception e) { AdLogger.Error($"AppLovinAdapter.DestroyBanner threw: {e}"); }
        }

        public void SetBannerAutoRefresh(bool enabled)
        {
            try
            {
                if (string.IsNullOrEmpty(_bannerId)) return;
                if (enabled) MaxSdk.StartBannerAutoRefresh(_bannerId);
                else MaxSdk.StopBannerAutoRefresh(_bannerId);
            }
            catch (Exception e) { AdLogger.Error($"AppLovinAdapter.SetBannerAutoRefresh threw: {e}"); }
        }

        private MaxSdkBase.BannerPosition ToMaxBannerPosition(BannerAnchor a)
        {
            switch (a)
            {
                case BannerAnchor.Top: return MaxSdkBase.BannerPosition.TopCenter;
                case BannerAnchor.TopLeft: return MaxSdkBase.BannerPosition.TopLeft;
                case BannerAnchor.TopRight: return MaxSdkBase.BannerPosition.TopRight;
                case BannerAnchor.BottomLeft: return MaxSdkBase.BannerPosition.BottomLeft;
                case BannerAnchor.BottomRight: return MaxSdkBase.BannerPosition.BottomRight;
                default: return MaxSdkBase.BannerPosition.BottomCenter;
            }
        }

        // Interstitial -
        public void LoadInterstitial(Action<bool> onLoaded)
        {
            var id = _config.GetInterstitialId();
            if (string.IsNullOrEmpty(id)) { AdLogger.Warn("AppLovin interstitial ID empty"); onLoaded?.Invoke(false); return; }
            _interstitialLoadCallback = onLoaded;
            MaxSdk.LoadInterstitial(id);
        }

        public bool IsInterstitialReady()
        {
            var id = _config.GetInterstitialId();
            return !string.IsNullOrEmpty(id) && MaxSdk.IsInterstitialReady(id);
        }

        public void ShowInterstitial(string placement, Action<AdResultCode> callback)
        {
            try
            {
                var id = _config.GetInterstitialId();
                if (string.IsNullOrEmpty(id) || !MaxSdk.IsInterstitialReady(id))
                {
                    callback?.Invoke(AdResultCode.NotReady);
                    return;
                }
                _interstitialCallback = callback;
                _interstitialPlacement = placement;
                MaxSdk.ShowInterstitial(id, placement);
            }
            catch (Exception e)
            {
                AdLogger.Error($"AppLovinAdapter.ShowInterstitial threw: {e}");
                callback?.Invoke(AdResultCode.Failed);
            }
        }

        // Rewarded -
        public void LoadRewarded(Action<bool> onLoaded)
        {
            var id = _config.GetRewardedId();
            if (string.IsNullOrEmpty(id)) { AdLogger.Warn("AppLovin rewarded ID empty"); onLoaded?.Invoke(false); return; }
            _rewardedLoadCallback = onLoaded;
            MaxSdk.LoadRewardedAd(id);
        }

        public bool IsRewardedReady()
        {
            var id = _config.GetRewardedId();
            return !string.IsNullOrEmpty(id) && MaxSdk.IsRewardedAdReady(id);
        }

        public void ShowRewarded(string placement, Action<AdResultCode> callback)
        {
            try
            {
                var id = _config.GetRewardedId();
                if (string.IsNullOrEmpty(id) || !MaxSdk.IsRewardedAdReady(id))
                {
                    callback?.Invoke(AdResultCode.NotReady);
                    return;
                }
                _rewardedCallback = callback;
                _rewardedPlacement = placement;
                MaxSdk.ShowRewardedAd(id, placement);
            }
            catch (Exception e)
            {
                AdLogger.Error($"AppLovinAdapter.ShowRewarded threw: {e}");
                callback?.Invoke(AdResultCode.Failed);
            }
        }

        // Rewarded Interstitial - AppLovin MAX has no distinct format; route to rewarded.
        public void LoadRewardedInterstitial(Action<bool> onLoaded) => LoadRewarded(onLoaded);
        public bool IsRewardedInterstitialReady() => IsRewardedReady();
        public void ShowRewardedInterstitial(string placement, Action<AdResultCode> callback) => ShowRewarded(placement, callback);

        // MREC -
        private string _mrecId;
        public void LoadMrec(Action<bool> onLoaded)
        {
            _mrecId = _config.GetMrecId();
            if (string.IsNullOrEmpty(_mrecId)) { onLoaded?.Invoke(false); return; }
            _mrecLoadCallback = onLoaded;
            MaxSdk.CreateMRec(_mrecId, MaxSdkBase.AdViewPosition.Centered);
        }
        public bool IsMrecReady() => !string.IsNullOrEmpty(_mrecId);
        public void ShowMrec() { if (!string.IsNullOrEmpty(_mrecId)) MaxSdk.ShowMRec(_mrecId); }
        public void HideMrec() { if (!string.IsNullOrEmpty(_mrecId)) MaxSdk.HideMRec(_mrecId); }

        // Callback wiring -
        private void WireInterstitialCallbacks()
        {
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += (id, info) =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    var cb = _interstitialLoadCallback; _interstitialLoadCallback = null;
                    cb?.Invoke(true);
                });
            };
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += (id, err) =>
            {
                AdLogger.Network(DisplayName, $"interstitial load failed: {err.Message}");
                MainThreadDispatcher.Enqueue(() =>
                {
                    var cb = _interstitialLoadCallback; _interstitialLoadCallback = null;
                    cb?.Invoke(false);
                });
            };
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += (id, info) =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    var cb = _interstitialCallback; _interstitialCallback = null;
                    cb?.Invoke(AdResultCode.Closed);
                });
            };
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += (id, info) => FireRevenue(AdFormat.Interstitial, _interstitialPlacement, info);
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += (id, err, info) =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    var cb = _interstitialCallback; _interstitialCallback = null;
                    cb?.Invoke(AdResultCode.Failed);
                });
            };
        }

        private void WireRewardedCallbacks()
        {
            bool earned = false;
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += (id, info) =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    var cb = _rewardedLoadCallback; _rewardedLoadCallback = null;
                    cb?.Invoke(true);
                });
            };
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += (id, err) =>
            {
                AdLogger.Network(DisplayName, $"rewarded load failed: {err.Message}");
                MainThreadDispatcher.Enqueue(() =>
                {
                    var cb = _rewardedLoadCallback; _rewardedLoadCallback = null;
                    cb?.Invoke(false);
                });
            };
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += (id, reward, info) => earned = true;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += (id, info) =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    var cb = _rewardedCallback; _rewardedCallback = null;
                    cb?.Invoke(earned ? AdResultCode.Rewarded : AdResultCode.Cancelled);
                    earned = false;
                });
            };
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += (id, info) => FireRevenue(AdFormat.Rewarded, _rewardedPlacement, info);
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += (id, err, info) =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    var cb = _rewardedCallback; _rewardedCallback = null;
                    cb?.Invoke(AdResultCode.Failed);
                });
            };
        }

        private void WireBannerCallbacks()
        {
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += (id, info) => FireRevenue(AdFormat.Banner, "", info);
        }

        private void WireMrecCallbacks()
        {
            MaxSdkCallbacks.MRec.OnAdLoadedEvent += (id, info) =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    var cb = _mrecLoadCallback; _mrecLoadCallback = null;
                    cb?.Invoke(true);
                });
            };
            MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += (id, err) =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    var cb = _mrecLoadCallback; _mrecLoadCallback = null;
                    cb?.Invoke(false);
                });
            };
            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += (id, info) => FireRevenue(AdFormat.MediumRectangle, "", info);
        }

        private void FireRevenue(AdFormat fmt, string placement, MaxSdkBase.AdInfo info)
        {
            var rev = new AdRevenueInfo(fmt, info.AdUnitIdentifier, placement, info.NetworkName, "USD", info.Revenue);
            MainThreadDispatcher.Enqueue(() => _onRevenuePaid?.Invoke(rev));
        }
    }
}
#endif
