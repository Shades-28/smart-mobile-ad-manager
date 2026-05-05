using System;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    public class EditorAdapter : INetworkAdapter
    {
        public MediatorKind Kind => MediatorKind.Editor;
        public string DisplayName => "Editor (Simulated)";
        public bool IsInitialized { get; private set; }

        private AdManagerConfig _config;
        private Action<AdRevenueInfo> _onRevenuePaid;
        private SimulatedAdOverlay _overlay;

        public static AdResultCode NextRewardedResult { get; set; } = AdResultCode.Rewarded;
        public static AdResultCode NextInterstitialResult { get; set; } = AdResultCode.Closed;
        public static bool NextLoadFails { get; set; } = false;
        public static double SimulatedRevenue { get; set; } = 0.0085;

        public void Initialize(AdManagerConfig config, Action<AdRevenueInfo> onRevenuePaid)
        {
            _config = config;
            _onRevenuePaid = onRevenuePaid;
            EnsureOverlay();
            IsInitialized = true;
            AdLogger.Lifecycle("EditorAdapter.Initialize", "ready");
        }

        public void OnApplicationPause(bool paused) { }

        // Banner -
        public void LoadBanner(BannerAnchor anchor) => LoadBanner(anchor, BannerSize.Standard);

        public void LoadBanner(BannerAnchor anchor, BannerSize size)
        {
            EnsureOverlay();
            _overlay.SetBanner(anchor, true);
            AdLogger.Network(DisplayName, $"banner loaded @ {anchor} ({size})");
        }

        public void ShowBanner()
        {
            EnsureOverlay();
            _overlay.SetBannerVisible(true);
            FireRevenue(AdFormat.Banner);
        }

        public void HideBanner()
        {
            if (_overlay != null) _overlay.SetBannerVisible(false);
        }

        public void DestroyBanner()
        {
            if (_overlay != null) _overlay.SetBanner(BannerAnchor.Bottom, false);
        }

        public void SetBannerAutoRefresh(bool enabled) { /* simulator: no-op */ }

        // Interstitial -
        private bool _interstitialLoaded;

        public void LoadInterstitial(Action<bool> onLoaded)
        {
            _interstitialLoaded = !NextLoadFails;
            AdLogger.Network(DisplayName, $"interstitial load => {(_interstitialLoaded ? "ready" : "failed")}");
            onLoaded?.Invoke(_interstitialLoaded);
        }

        public bool IsInterstitialReady() => _interstitialLoaded;

        public void ShowInterstitial(string placement, Action<AdResultCode> callback)
        {
            EnsureOverlay();
            if (!_interstitialLoaded)
            {
                callback?.Invoke(AdResultCode.NotReady);
                return;
            }
            _interstitialLoaded = false;
            _overlay.ShowFullscreen(
                title: "Simulated Interstitial",
                subtitle: $"placement: {placement}",
                isRewarded: false,
                onClose: result =>
                {
                    var code = NextInterstitialResult == AdResultCode.None ? result : NextInterstitialResult;
                    NextInterstitialResult = AdResultCode.Closed;
                    if (code == AdResultCode.Closed || code == AdResultCode.Shown)
                        FireRevenue(AdFormat.Interstitial, placement);
                    callback?.Invoke(code);
                });
        }

        // Rewarded -
        private bool _rewardedLoaded;

        public void LoadRewarded(Action<bool> onLoaded)
        {
            _rewardedLoaded = !NextLoadFails;
            AdLogger.Network(DisplayName, $"rewarded load => {(_rewardedLoaded ? "ready" : "failed")}");
            onLoaded?.Invoke(_rewardedLoaded);
        }

        public bool IsRewardedReady() => _rewardedLoaded;

        public void ShowRewarded(string placement, Action<AdResultCode> callback)
        {
            EnsureOverlay();
            if (!_rewardedLoaded)
            {
                callback?.Invoke(AdResultCode.NotReady);
                return;
            }
            _rewardedLoaded = false;
            _overlay.ShowFullscreen(
                title: "Simulated Rewarded",
                subtitle: $"placement: {placement} - wait or skip",
                isRewarded: true,
                onClose: result =>
                {
                    var code = NextRewardedResult == AdResultCode.None ? result : NextRewardedResult;
                    NextRewardedResult = AdResultCode.Rewarded;
                    if (code == AdResultCode.Rewarded)
                        FireRevenue(AdFormat.Rewarded, placement);
                    callback?.Invoke(code);
                });
        }

        // Rewarded Interstitial -
        // In editor we treat rewarded-interstitial as a rewarded variant with the same simulator
        // surface, so devs can wire both code paths and verify them without device builds.
        private bool _rewardedInterstitialLoaded;

        public void LoadRewardedInterstitial(Action<bool> onLoaded)
        {
            _rewardedInterstitialLoaded = !NextLoadFails;
            AdLogger.Network(DisplayName, $"rewarded-interstitial load => {(_rewardedInterstitialLoaded ? "ready" : "failed")}");
            onLoaded?.Invoke(_rewardedInterstitialLoaded);
        }

        public bool IsRewardedInterstitialReady() => _rewardedInterstitialLoaded;

        public void ShowRewardedInterstitial(string placement, Action<AdResultCode> callback)
        {
            EnsureOverlay();
            if (!_rewardedInterstitialLoaded) { callback?.Invoke(AdResultCode.NotReady); return; }
            _rewardedInterstitialLoaded = false;
            _overlay.ShowFullscreen(
                title: "Simulated Rewarded Interstitial",
                subtitle: $"placement: {placement} - wait or skip",
                isRewarded: true,
                onClose: result =>
                {
                    var code = NextRewardedResult == AdResultCode.None ? result : NextRewardedResult;
                    NextRewardedResult = AdResultCode.Rewarded;
                    if (code == AdResultCode.Rewarded)
                        FireRevenue(AdFormat.RewardedInterstitial, placement);
                    callback?.Invoke(code);
                });
        }

        // MREC -
        private bool _mrecLoaded;

        public void LoadMrec(Action<bool> onLoaded)
        {
            _mrecLoaded = !NextLoadFails;
            onLoaded?.Invoke(_mrecLoaded);
        }

        public bool IsMrecReady() => _mrecLoaded;

        public void ShowMrec()
        {
            EnsureOverlay();
            _overlay.SetMrecVisible(true);
            FireRevenue(AdFormat.MediumRectangle);
        }

        public void HideMrec()
        {
            if (_overlay != null) _overlay.SetMrecVisible(false);
        }

        // -
        private void EnsureOverlay()
        {
            if (_overlay != null) return;
            _overlay = SimulatedAdOverlay.GetOrCreate();
        }

        private void FireRevenue(AdFormat format, string placement = "")
        {
            var info = new AdRevenueInfo(format, $"editor-{format}", placement, DisplayName, "USD", SimulatedRevenue);
            _onRevenuePaid?.Invoke(info);
        }
    }
}
