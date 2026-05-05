#if AD_USE_LEVELPLAY
using System;
namespace Rinval.MobileAdsAndIapKit
{
    public class LevelPlayAdapter : INetworkAdapter
    {
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterFactory()
        {
            AdManager.RegisterAdapter(() => new LevelPlayAdapter());
        }

        public MediatorKind Kind => MediatorKind.UnityLevelPlay;
        public string DisplayName => "Unity LevelPlay";
        public bool IsInitialized { get; private set; }

        private AdManagerConfig _config;
        private Action<AdRevenueInfo> _onRevenuePaid;
        private Action<AdResultCode> _interstitialCallback;
        private Action<AdResultCode> _rewardedCallback;
        private string _interstitialPlacement;
        private string _rewardedPlacement;
        private bool _rewardEarned;

        private Action<bool> _interstitialLoadCallback;

        public void Initialize(AdManagerConfig config, Action<AdRevenueInfo> onRevenuePaid)
        {
            _config = config;
            _onRevenuePaid = onRevenuePaid;

            IronSourceEvents.onSdkInitializationCompletedEvent += () =>
            {
                IsInitialized = true;
                AdLogger.Lifecycle("LevelPlayAdapter.Initialize", "ready");
            };

            IronSourceInterstitialEvents.onAdReadyEvent += info =>
                MainThreadDispatcher.Enqueue(() =>
                {
                    var cb = _interstitialLoadCallback; _interstitialLoadCallback = null;
                    cb?.Invoke(true);
                });
            IronSourceInterstitialEvents.onAdLoadFailedEvent += err =>
                MainThreadDispatcher.Enqueue(() =>
                {
                    var cb = _interstitialLoadCallback; _interstitialLoadCallback = null;
                    cb?.Invoke(false);
                });
            IronSourceInterstitialEvents.onAdClosedEvent += info =>
                MainThreadDispatcher.Enqueue(() =>
                {
                    var cb = _interstitialCallback; _interstitialCallback = null;
                    cb?.Invoke(AdResultCode.Closed);
                });
            IronSourceInterstitialEvents.onAdShowFailedEvent += (err, info) =>
                MainThreadDispatcher.Enqueue(() =>
                {
                    var cb = _interstitialCallback; _interstitialCallback = null;
                    cb?.Invoke(AdResultCode.Failed);
                });
            IronSourceInterstitialEvents.onAdRevenuePaidEvent += info =>
                FireRevenue(AdFormat.Interstitial, _interstitialPlacement, info);

            IronSourceRewardedVideoEvents.onAdRewardedEvent += (place, info) => _rewardEarned = true;
            IronSourceRewardedVideoEvents.onAdClosedEvent += info =>
                MainThreadDispatcher.Enqueue(() =>
                {
                    var cb = _rewardedCallback; _rewardedCallback = null;
                    cb?.Invoke(_rewardEarned ? AdResultCode.Rewarded : AdResultCode.Cancelled);
                    _rewardEarned = false;
                });
            IronSourceRewardedVideoEvents.onAdRevenuePaidEvent += info =>
                FireRevenue(AdFormat.Rewarded, _rewardedPlacement, info);

            try { IronSource.Agent.init(/* app key set in dashboard via inspector */ ""); }
            catch (Exception e) { AdLogger.Error($"LevelPlayAdapter.IronSource.init threw: {e}"); }
        }

        public void OnApplicationPause(bool paused) => IronSource.Agent.onApplicationPause(paused);

        // Banner -
        public void LoadBanner(BannerAnchor anchor) => LoadBanner(anchor, BannerSize.Standard);

        public void LoadBanner(BannerAnchor anchor, BannerSize size)
        {
            try
            {
                var bannerSize = size == BannerSize.MediumRectangle
                    ? IronSourceBannerSize.RECTANGLE
                    : IronSourceBannerSize.SMART;
                IronSource.Agent.loadBanner(bannerSize, ToLevelPlayPosition(anchor));
            }
            catch (Exception e) { AdLogger.Error($"LevelPlayAdapter.LoadBanner threw: {e}"); }
        }

        public void ShowBanner()
        {
            try { IronSource.Agent.displayBanner(); }
            catch (Exception e) { AdLogger.Error($"LevelPlayAdapter.ShowBanner threw: {e}"); }
        }

        public void HideBanner()
        {
            try { IronSource.Agent.hideBanner(); }
            catch (Exception e) { AdLogger.Error($"LevelPlayAdapter.HideBanner threw: {e}"); }
        }

        public void DestroyBanner()
        {
            try { IronSource.Agent.destroyBanner(); }
            catch (Exception e) { AdLogger.Error($"LevelPlayAdapter.DestroyBanner threw: {e}"); }
        }

        public void SetBannerAutoRefresh(bool enabled)
        {
            // LevelPlay banner auto-refresh is configured in their dashboard; no SDK toggle.
            AdLogger.Lifecycle("LevelPlayAdapter.SetBannerAutoRefresh", enabled ? "on (dashboard)" : "off (dashboard)");
        }

        private IronSourceBannerPosition ToLevelPlayPosition(BannerAnchor a)
        {
            switch (a)
            {
                case BannerAnchor.Top:
                case BannerAnchor.TopLeft:
                case BannerAnchor.TopRight:
                    return IronSourceBannerPosition.TOP;
                default:
                    return IronSourceBannerPosition.BOTTOM;
            }
        }

        // Interstitial -
        public void LoadInterstitial(Action<bool> onLoaded)
        {
            _interstitialLoadCallback = onLoaded;
            IronSource.Agent.loadInterstitial();
        }
        public bool IsInterstitialReady() => IronSource.Agent.isInterstitialReady();

        public void ShowInterstitial(string placement, Action<AdResultCode> callback)
        {
            try
            {
                if (!IsInterstitialReady()) { callback?.Invoke(AdResultCode.NotReady); return; }
                _interstitialCallback = callback;
                _interstitialPlacement = placement;
                IronSource.Agent.showInterstitial(placement);
            }
            catch (Exception e)
            {
                AdLogger.Error($"LevelPlayAdapter.ShowInterstitial threw: {e}");
                callback?.Invoke(AdResultCode.Failed);
            }
        }

        // Rewarded -
        public void LoadRewarded(Action<bool> onLoaded)
        {
            // LevelPlay rewarded auto-loads - return current readiness immediately.
            onLoaded?.Invoke(IronSource.Agent.isRewardedVideoAvailable());
        }
        public bool IsRewardedReady() => IronSource.Agent.isRewardedVideoAvailable();

        public void ShowRewarded(string placement, Action<AdResultCode> callback)
        {
            try
            {
                if (!IsRewardedReady()) { callback?.Invoke(AdResultCode.NotReady); return; }
                _rewardedCallback = callback;
                _rewardedPlacement = placement;
                _rewardEarned = false;
                IronSource.Agent.showRewardedVideo(placement);
            }
            catch (Exception e)
            {
                AdLogger.Error($"LevelPlayAdapter.ShowRewarded threw: {e}");
                callback?.Invoke(AdResultCode.Failed);
            }
        }

        // Rewarded Interstitial - LevelPlay has no distinct format; route to rewarded.
        public void LoadRewardedInterstitial(Action<bool> onLoaded) => LoadRewarded(onLoaded);
        public bool IsRewardedInterstitialReady() => IsRewardedReady();
        public void ShowRewardedInterstitial(string placement, Action<AdResultCode> callback) => ShowRewarded(placement, callback);

        // MREC -
        public void LoadMrec(Action<bool> onLoaded)
        {
            // LevelPlay MREC routed via banner-with-rectangle size - not separately loadable.
            onLoaded?.Invoke(false);
        }
        public bool IsMrecReady() => false;
        public void ShowMrec() { }
        public void HideMrec() { }

        private void FireRevenue(AdFormat fmt, string placement, IronSourceImpressionData info)
        {
            var rev = new AdRevenueInfo(
                fmt, info.adUnit, placement, info.adNetwork ?? "LevelPlay", "USD", info.revenue ?? 0.0);
            MainThreadDispatcher.Enqueue(() => _onRevenuePaid?.Invoke(rev));
        }
    }
}
#endif
