using System;

namespace Rinval.MobileAdsAndIapKit
{
    public interface INetworkAdapter
    {
        MediatorKind Kind { get; }
        string DisplayName { get; }
        bool IsInitialized { get; }

        void Initialize(AdManagerConfig config, Action<AdRevenueInfo> onRevenuePaid);
        void OnApplicationPause(bool paused);

        void LoadBanner(BannerAnchor anchor);
        void LoadBanner(BannerAnchor anchor, BannerSize size);
        void ShowBanner();
        void HideBanner();
        void DestroyBanner();
        void SetBannerAutoRefresh(bool enabled);

        void LoadInterstitial(Action<bool> onLoaded);
        bool IsInterstitialReady();
        void ShowInterstitial(string placement, Action<AdResultCode> callback);

        void LoadRewarded(Action<bool> onLoaded);
        bool IsRewardedReady();
        void ShowRewarded(string placement, Action<AdResultCode> callback);

        void LoadRewardedInterstitial(Action<bool> onLoaded);
        bool IsRewardedInterstitialReady();
        void ShowRewardedInterstitial(string placement, Action<AdResultCode> callback);

        void LoadMrec(Action<bool> onLoaded);
        bool IsMrecReady();
        void ShowMrec();
        void HideMrec();
    }
}
