#if AD_USE_APPLOVIN
using System;

namespace Rinval.MobileAdsAndIapKit
{
    public class AppLovinAppOpenAdapter : IAppOpenAdapter
    {
        public MediatorKind Kind => MediatorKind.AppLovinMax;
        public bool IsInitialized { get; private set; }

        private AdManagerConfig _config;
        private Action<AdRevenueInfo> _onRevenuePaid;
        private Action<AdResultCode> _showCallback;
        private string _adUnitId;

        public void Initialize(AdManagerConfig config, Action<AdRevenueInfo> onRevenuePaid)
        {
            _config = config;
            _onRevenuePaid = onRevenuePaid;
            _adUnitId = _config.GetAppOpenId();
            if (string.IsNullOrEmpty(_adUnitId))
            {
                AdLogger.Warn("AppLovin app-open ID empty");
                return;
            }
            WireCallbacks();
            IsInitialized = true;
            AdLogger.Lifecycle("AppLovinAppOpenAdapter.Initialize", "ready");
        }

        private void WireCallbacks()
        {
            MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += (id, info) =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    var cb = _showCallback; _showCallback = null;
                    cb?.Invoke(AdResultCode.Closed);
                });
            };
            MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += (id, err, info) =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    var cb = _showCallback; _showCallback = null;
                    cb?.Invoke(AdResultCode.Failed);
                });
            };
            MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += (id, info) =>
            {
                try
                {
                    var rev = new AdRevenueInfo(AdFormat.AppOpen, info.AdUnitIdentifier, "", info.NetworkName, "USD", info.Revenue);
                    MainThreadDispatcher.Enqueue(() => _onRevenuePaid?.Invoke(rev));
                }
                catch (Exception e) { AdLogger.Error($"AppLovinAppOpenAdapter.OnAdRevenuePaid threw: {e}"); }
            };
        }

        public void Load()
        {
            try
            {
                if (string.IsNullOrEmpty(_adUnitId)) return;
                MaxSdk.LoadAppOpenAd(_adUnitId);
            }
            catch (Exception e) { AdLogger.Error($"AppLovinAppOpenAdapter.Load threw: {e}"); }
        }

        public bool IsReady()
        {
            return !string.IsNullOrEmpty(_adUnitId) && MaxSdk.IsAppOpenAdReady(_adUnitId);
        }

        public void Show(Action<AdResultCode> callback)
        {
            try
            {
                if (!IsReady()) { callback?.Invoke(AdResultCode.NotReady); return; }
                _showCallback = callback;
                MaxSdk.ShowAppOpenAd(_adUnitId);
            }
            catch (Exception e)
            {
                AdLogger.Error($"AppLovinAppOpenAdapter.Show threw: {e}");
                callback?.Invoke(AdResultCode.Failed);
            }
        }
    }
}
#endif
