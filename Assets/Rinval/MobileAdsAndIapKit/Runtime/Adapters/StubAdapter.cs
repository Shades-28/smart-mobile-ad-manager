using System;

namespace Rinval.MobileAdsAndIapKit
{
    public class StubAdapter : INetworkAdapter
    {
        public MediatorKind Kind => MediatorKind.None;
        public string DisplayName => "Stub (Disabled)";
        public bool IsInitialized { get; private set; }

        public void Initialize(AdManagerConfig config, Action<AdRevenueInfo> onRevenuePaid)
        {
            IsInitialized = true;
            AdLogger.Lifecycle("StubAdapter.Initialize", "ads disabled - no mediator selected");
        }

        public void OnApplicationPause(bool paused) { }

        public void LoadBanner(BannerAnchor anchor) { }
        public void LoadBanner(BannerAnchor anchor, BannerSize size) { }
        public void ShowBanner() { }
        public void HideBanner() { }
        public void DestroyBanner() { }
        public void SetBannerAutoRefresh(bool enabled) { }

        // Stub treats every load as "completed" (no-op) so callers don't retry into a void.
        // IsReady stays false so Show() falls through to Disabled.
        public void LoadInterstitial(Action<bool> onLoaded) => onLoaded?.Invoke(true);
        public bool IsInterstitialReady() => false;
        public void ShowInterstitial(string placement, Action<AdResultCode> callback)
            => callback?.Invoke(AdResultCode.Disabled);

        public void LoadRewarded(Action<bool> onLoaded) => onLoaded?.Invoke(true);
        public bool IsRewardedReady() => false;
        public void ShowRewarded(string placement, Action<AdResultCode> callback)
            => callback?.Invoke(AdResultCode.Disabled);

        public void LoadRewardedInterstitial(Action<bool> onLoaded) => onLoaded?.Invoke(true);
        public bool IsRewardedInterstitialReady() => false;
        public void ShowRewardedInterstitial(string placement, Action<AdResultCode> callback)
            => callback?.Invoke(AdResultCode.Disabled);

        public void LoadMrec(Action<bool> onLoaded) => onLoaded?.Invoke(true);
        public bool IsMrecReady() => false;
        public void ShowMrec() { }
        public void HideMrec() { }
    }
}
