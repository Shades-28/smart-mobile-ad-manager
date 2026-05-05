#if AD_USE_ADMOB
using System;
using GoogleMobileAds.Api;

namespace Rinval.MobileAdsAndIapKit
{
    public class AdMobAppOpenAdapter : IAppOpenAdapter
    {
        public MediatorKind Kind => MediatorKind.GoogleAdMob;
        public bool IsInitialized { get; private set; }

        private AdManagerConfig _config;
        private Action<AdRevenueInfo> _onRevenuePaid;
        private AppOpenAd _ad;
        private Action<AdResultCode> _showCallback;
        private DateTime _loadedAt;

        // App Open ads expire after ~4 hours per AdMob policy.
        private static readonly TimeSpan ExpirationWindow = TimeSpan.FromHours(4);

        public void Initialize(AdManagerConfig config, Action<AdRevenueInfo> onRevenuePaid)
        {
            _config = config;
            _onRevenuePaid = onRevenuePaid;
            // MobileAds.Initialize is called by the main AdMobAdapter; if used standalone,
            // the user must call MobileAds.Initialize separately before this adapter's first Load.
            IsInitialized = true;
            AdLogger.Lifecycle("AdMobAppOpenAdapter.Initialize", "ready");
        }

        public void Load()
        {
            try
            {
                var id = _config?.GetAppOpenId();
                if (string.IsNullOrEmpty(id)) { AdLogger.Warn("AdMob app-open ID empty"); return; }
                AppOpenAd.Load(id, new AdRequest(), (ad, err) =>
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        if (err != null)
                        {
                            AdLogger.Network("AdMob", $"app-open load failed: {err}");
                            return;
                        }
                        _ad = ad;
                        _loadedAt = DateTime.UtcNow;
                        WireAdEvents();
                    });
                });
            }
            catch (Exception e) { AdLogger.Error($"AdMobAppOpenAdapter.Load threw: {e}"); }
        }

        private void WireAdEvents()
        {
            if (_ad == null) return;
            _ad.OnAdFullScreenContentClosed += () =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    var cb = _showCallback; _showCallback = null;
                    _ad = null;
                    cb?.Invoke(AdResultCode.Closed);
                });
            };
            _ad.OnAdFullScreenContentFailed += err =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    var cb = _showCallback; _showCallback = null;
                    _ad = null;
                    cb?.Invoke(AdResultCode.Failed);
                });
            };
            _ad.OnAdPaid += paid =>
            {
                try
                {
                    var amt = paid.Value / 1_000_000.0;
                    var info = new AdRevenueInfo(AdFormat.AppOpen, "admob-appopen", "", "AdMob", paid.CurrencyCode, amt);
                    MainThreadDispatcher.Enqueue(() => _onRevenuePaid?.Invoke(info));
                }
                catch (Exception e) { AdLogger.Error($"AdMobAppOpenAdapter.OnAdPaid threw: {e}"); }
            };
        }

        public bool IsReady()
        {
            if (_ad == null) return false;
            if (DateTime.UtcNow - _loadedAt > ExpirationWindow)
            {
                _ad = null;
                return false;
            }
            return _ad.CanShowAd();
        }

        public void Show(Action<AdResultCode> callback)
        {
            try
            {
                if (!IsReady()) { callback?.Invoke(AdResultCode.NotReady); return; }
                _showCallback = callback;
                _ad.Show();
            }
            catch (Exception e)
            {
                AdLogger.Error($"AdMobAppOpenAdapter.Show threw: {e}");
                callback?.Invoke(AdResultCode.Failed);
            }
        }
    }
}
#endif
