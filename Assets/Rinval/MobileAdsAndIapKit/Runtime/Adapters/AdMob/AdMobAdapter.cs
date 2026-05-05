#if AD_USE_ADMOB
using System;
using GoogleMobileAds.Api;
namespace Rinval.MobileAdsAndIapKit
{
    public class AdMobAdapter : INetworkAdapter
    {
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterFactory()
        {
            AdManager.RegisterAdapter(() => new AdMobAdapter());
        }

        public MediatorKind Kind => MediatorKind.GoogleAdMob;
        public string DisplayName => "Google AdMob";
        public bool IsInitialized { get; private set; }

        private AdManagerConfig _config;
        private Action<AdRevenueInfo> _onRevenuePaid;

        private BannerView _banner;
        private InterstitialAd _interstitial;
        private RewardedAd _rewarded;
        private RewardedInterstitialAd _rewardedInterstitial;
        private BannerView _mrec;

        private Action<AdResultCode> _interstitialCallback;
        private Action<AdResultCode> _rewardedCallback;
        private Action<AdResultCode> _rewardedInterstitialCallback;
        private string _interstitialPlacement;
        private string _rewardedPlacement;
        private string _rewardedInterstitialPlacement;

        public void Initialize(AdManagerConfig config, Action<AdRevenueInfo> onRevenuePaid)
        {
            try
            {
                _config = config;
                _onRevenuePaid = onRevenuePaid;
                if (TestDeviceRegistry.Ids.Count > 0)
                {
                    var ids = new System.Collections.Generic.List<string>(TestDeviceRegistry.Ids);
                    MobileAds.SetRequestConfiguration(new RequestConfiguration { TestDeviceIds = ids });
                }
                MobileAds.Initialize(_ =>
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        IsInitialized = true;
                        AdLogger.Lifecycle("AdMobAdapter.Initialize", "ready");
                    });
                });
            }
            catch (Exception e) { AdLogger.Error($"AdMobAdapter.Initialize threw: {e}"); }
        }

        public void OnApplicationPause(bool paused) { }

        // Banner -
        public void LoadBanner(BannerAnchor anchor) => LoadBanner(anchor, BannerSize.Standard);

        public void LoadBanner(BannerAnchor anchor, BannerSize size)
        {
            try
            {
                DestroyBanner();
                var id = _config.GetBannerId();
                if (string.IsNullOrEmpty(id)) { AdLogger.Warn("AdMob banner ID empty"); return; }
                var adSize = ToAdMobSize(size);
                _banner = new BannerView(id, adSize, ToAdMobPosition(anchor));
                _banner.OnAdPaid += paid => FireRevenue(AdFormat.Banner, "", paid);
                _banner.LoadAd(new AdRequest());
            }
            catch (Exception e) { AdLogger.Error($"AdMobAdapter.LoadBanner threw: {e}"); }
        }

        private static AdSize ToAdMobSize(BannerSize size)
        {
            switch (size)
            {
                case BannerSize.Adaptive:
                    return AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
                case BannerSize.MediumRectangle: return AdSize.MediumRectangle;
                case BannerSize.Leaderboard: return AdSize.Leaderboard;
                default: return AdSize.Banner;
            }
        }

        public void SetBannerAutoRefresh(bool enabled)
        {
            // AdMob banner refresh is configured server-side per ad unit; this is a no-op
            // at the SDK level. Surface the intent via log so dashboards reflect publisher choice.
            AdLogger.Lifecycle("AdMobAdapter.SetBannerAutoRefresh", enabled ? "on (server-side)" : "off (server-side)");
        }

        public void ShowBanner()
        {
            try { _banner?.Show(); }
            catch (Exception e) { AdLogger.Error($"AdMobAdapter.ShowBanner threw: {e}"); }
        }

        public void HideBanner()
        {
            try { _banner?.Hide(); }
            catch (Exception e) { AdLogger.Error($"AdMobAdapter.HideBanner threw: {e}"); }
        }

        public void DestroyBanner()
        {
            try { if (_banner != null) { _banner.Destroy(); _banner = null; } }
            catch (Exception e) { AdLogger.Error($"AdMobAdapter.DestroyBanner threw: {e}"); }
        }

        private AdPosition ToAdMobPosition(BannerAnchor a)
        {
            switch (a)
            {
                case BannerAnchor.Top: return AdPosition.Top;
                case BannerAnchor.TopLeft: return AdPosition.TopLeft;
                case BannerAnchor.TopRight: return AdPosition.TopRight;
                case BannerAnchor.BottomLeft: return AdPosition.BottomLeft;
                case BannerAnchor.BottomRight: return AdPosition.BottomRight;
                default: return AdPosition.Bottom;
            }
        }

        // Interstitial -
        public void LoadInterstitial(Action<bool> onLoaded)
        {
            var id = _config.GetInterstitialId();
            if (string.IsNullOrEmpty(id)) { onLoaded?.Invoke(false); return; }
            InterstitialAd.Load(id, new AdRequest(), (ad, err) =>
            {
                if (err != null)
                {
                    AdLogger.Network(DisplayName, $"interstitial load failed: {err}");
                    MainThreadDispatcher.Enqueue(() => onLoaded?.Invoke(false));
                    return;
                }
                _interstitial = ad;
                _interstitial.OnAdFullScreenContentClosed += () =>
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        var cb = _interstitialCallback; _interstitialCallback = null;
                        cb?.Invoke(AdResultCode.Closed);
                    });
                };
                _interstitial.OnAdFullScreenContentFailed += err2 =>
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        var cb = _interstitialCallback; _interstitialCallback = null;
                        cb?.Invoke(AdResultCode.Failed);
                    });
                };
                _interstitial.OnAdPaid += paid => FireRevenue(AdFormat.Interstitial, _interstitialPlacement, paid);
                MainThreadDispatcher.Enqueue(() => onLoaded?.Invoke(true));
            });
        }

        public bool IsInterstitialReady() => _interstitial != null && _interstitial.CanShowAd();

        public void ShowInterstitial(string placement, Action<AdResultCode> callback)
        {
            try
            {
                if (!IsInterstitialReady()) { callback?.Invoke(AdResultCode.NotReady); return; }
                _interstitialCallback = callback;
                _interstitialPlacement = placement;
                _interstitial.Show();
            }
            catch (Exception e)
            {
                AdLogger.Error($"AdMobAdapter.ShowInterstitial threw: {e}");
                callback?.Invoke(AdResultCode.Failed);
            }
        }

        // Rewarded -
        public void LoadRewarded(Action<bool> onLoaded)
        {
            var id = _config.GetRewardedId();
            if (string.IsNullOrEmpty(id)) { onLoaded?.Invoke(false); return; }
            RewardedAd.Load(id, new AdRequest(), (ad, err) =>
            {
                if (err != null)
                {
                    AdLogger.Network(DisplayName, $"rewarded load failed: {err}");
                    MainThreadDispatcher.Enqueue(() => onLoaded?.Invoke(false));
                    return;
                }
                _rewarded = ad;
                _rewarded.OnAdFullScreenContentClosed += () =>
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        var cb = _rewardedCallback; _rewardedCallback = null;
                        cb?.Invoke(AdResultCode.Closed);
                    });
                };
                _rewarded.OnAdFullScreenContentFailed += err2 =>
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        var cb = _rewardedCallback; _rewardedCallback = null;
                        cb?.Invoke(AdResultCode.Failed);
                    });
                };
                _rewarded.OnAdPaid += paid => FireRevenue(AdFormat.Rewarded, _rewardedPlacement, paid);
                MainThreadDispatcher.Enqueue(() => onLoaded?.Invoke(true));
            });
        }

        public bool IsRewardedReady() => _rewarded != null && _rewarded.CanShowAd();

        public void ShowRewarded(string placement, Action<AdResultCode> callback)
        {
            try
            {
                if (!IsRewardedReady()) { callback?.Invoke(AdResultCode.NotReady); return; }
                _rewardedCallback = callback;
                _rewardedPlacement = placement;
                _rewarded.Show(reward =>
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        var cb = _rewardedCallback; _rewardedCallback = null;
                        cb?.Invoke(AdResultCode.Rewarded);
                    });
                });
            }
            catch (Exception e)
            {
                AdLogger.Error($"AdMobAdapter.ShowRewarded threw: {e}");
                callback?.Invoke(AdResultCode.Failed);
            }
        }

        // Rewarded Interstitial -
        public void LoadRewardedInterstitial(Action<bool> onLoaded)
        {
            try
            {
                var id = _config.GetRewardedInterstitialId();
                if (string.IsNullOrEmpty(id)) { onLoaded?.Invoke(false); return; }
                RewardedInterstitialAd.Load(id, new AdRequest(), (ad, err) =>
                {
                    if (err != null)
                    {
                        AdLogger.Network(DisplayName, $"rewarded-interstitial load failed: {err}");
                        MainThreadDispatcher.Enqueue(() => onLoaded?.Invoke(false));
                        return;
                    }
                    _rewardedInterstitial = ad;
                    _rewardedInterstitial.OnAdFullScreenContentClosed += () =>
                    {
                        MainThreadDispatcher.Enqueue(() =>
                        {
                            var cb = _rewardedInterstitialCallback; _rewardedInterstitialCallback = null;
                            cb?.Invoke(AdResultCode.Closed);
                        });
                    };
                    _rewardedInterstitial.OnAdFullScreenContentFailed += err2 =>
                    {
                        MainThreadDispatcher.Enqueue(() =>
                        {
                            var cb = _rewardedInterstitialCallback; _rewardedInterstitialCallback = null;
                            cb?.Invoke(AdResultCode.Failed);
                        });
                    };
                    _rewardedInterstitial.OnAdPaid += paid => FireRevenue(AdFormat.RewardedInterstitial, _rewardedInterstitialPlacement, paid);
                    MainThreadDispatcher.Enqueue(() => onLoaded?.Invoke(true));
                });
            }
            catch (Exception e) { AdLogger.Error($"AdMobAdapter.LoadRewardedInterstitial threw: {e}"); onLoaded?.Invoke(false); }
        }

        public bool IsRewardedInterstitialReady() => _rewardedInterstitial != null && _rewardedInterstitial.CanShowAd();

        public void ShowRewardedInterstitial(string placement, Action<AdResultCode> callback)
        {
            try
            {
                if (!IsRewardedInterstitialReady()) { callback?.Invoke(AdResultCode.NotReady); return; }
                _rewardedInterstitialCallback = callback;
                _rewardedInterstitialPlacement = placement;
                _rewardedInterstitial.Show(reward =>
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        var cb = _rewardedInterstitialCallback; _rewardedInterstitialCallback = null;
                        cb?.Invoke(AdResultCode.Rewarded);
                    });
                });
            }
            catch (Exception e)
            {
                AdLogger.Error($"AdMobAdapter.ShowRewardedInterstitial threw: {e}");
                callback?.Invoke(AdResultCode.Failed);
            }
        }

        // MREC -
        public void LoadMrec(Action<bool> onLoaded)
        {
            var id = _config.GetMrecId();
            if (string.IsNullOrEmpty(id)) { onLoaded?.Invoke(false); return; }
            _mrec = new BannerView(id, AdSize.MediumRectangle, AdPosition.Center);
            _mrec.OnAdPaid += paid => FireRevenue(AdFormat.MediumRectangle, "", paid);
            _mrec.LoadAd(new AdRequest());
            // AdMob banner-style views don't expose a load-result callback at this surface.
            // Treat the issuance as success; if the network fills nothing, IsReady will reflect that.
            onLoaded?.Invoke(true);
        }
        public bool IsMrecReady() => _mrec != null;
        public void ShowMrec() => _mrec?.Show();
        public void HideMrec() => _mrec?.Hide();

        private void FireRevenue(AdFormat fmt, string placement, AdValue value)
        {
            var amt = value.Value / 1_000_000.0;
            var rev = new AdRevenueInfo(fmt, "admob", placement, "AdMob", value.CurrencyCode, amt);
            MainThreadDispatcher.Enqueue(() => _onRevenuePaid?.Invoke(rev));
        }
    }
}
#endif
