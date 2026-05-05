using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rinval.MobileAdsAndIapKit
{
    public class DemoController : MonoBehaviour
    {
        [Header("Config")]
        public AdManagerConfig config;

        [Header("Wiring (auto-filled by OneClickDemoSetup)")]
        public Button bannerLoadTopBtn;
        public Button bannerLoadBottomBtn;
        public Button bannerShowBtn;
        public Button bannerHideBtn;
        public Button bannerDestroyBtn;

        public Button interstitialLoadBtn;
        public Button interstitialShowClosedBtn;
        public Button interstitialShowFailBtn;

        public Button rewardedLoadBtn;
        public Button rewardedShowRewardBtn;
        public Button rewardedShowCancelBtn;

        public Button mrecLoadBtn;
        public Button mrecShowBtn;
        public Button mrecHideBtn;

        public Button consentGrantBtn;
        public Button consentDenyBtn;

        public Button initBtn;
        public Button shutdownBtn;

        public Text statusText;
        public Text logText;

        private readonly List<string> _logLines = new List<string>();
        private double _totalRevenue;
        private int _adsShown;

        private void Awake()
        {
            Wire(bannerLoadTopBtn, () => AdManager.LoadBanner(BannerAnchor.Top));
            Wire(bannerLoadBottomBtn, () => AdManager.LoadBanner(BannerAnchor.Bottom));
            Wire(bannerShowBtn, () => AdManager.ShowBanner());
            Wire(bannerHideBtn, () => AdManager.HideBanner());
            Wire(bannerDestroyBtn, () => AdManager.DestroyBanner());

            Wire(interstitialLoadBtn, () => AdManager.LoadInterstitial());
            Wire(interstitialShowClosedBtn, () => {
                EditorAdapter.NextInterstitialResult = AdResultCode.Closed;
                AdManager.ShowInterstitial("demo_interstitial", code => Append($"interstitial → {code}"));
            });
            Wire(interstitialShowFailBtn, () => {
                EditorAdapter.NextInterstitialResult = AdResultCode.Failed;
                AdManager.ShowInterstitial("demo_interstitial", code => Append($"interstitial → {code}"));
            });

            Wire(rewardedLoadBtn, () => AdManager.LoadRewarded());
            Wire(rewardedShowRewardBtn, () => {
                EditorAdapter.NextRewardedResult = AdResultCode.Rewarded;
                AdManager.ShowRewarded("demo_rewarded", code => Append($"rewarded → {code}"));
            });
            Wire(rewardedShowCancelBtn, () => {
                EditorAdapter.NextRewardedResult = AdResultCode.Cancelled;
                AdManager.ShowRewarded("demo_rewarded", code => Append($"rewarded → {code}"));
            });

            Wire(mrecLoadBtn, () => AdManager.LoadMrec());
            Wire(mrecShowBtn, () => AdManager.ShowMrec());
            Wire(mrecHideBtn, () => AdManager.HideMrec());

            Wire(consentGrantBtn, () => { ConsentManager.Grant(); Append("consent → granted"); });
            Wire(consentDenyBtn, () => { ConsentManager.Deny(); Append("consent → denied"); });

            Wire(initBtn, InitAds);
            Wire(shutdownBtn, () => { AdManager.Shutdown(); Append("AdManager shut down"); });

            AdManager.RevenuePaid += OnRevenue;
            AdManager.InterstitialClosed += code => { _adsShown++; };
            AdManager.RewardedClosed += code => { if (code == AdResultCode.Rewarded) _adsShown++; };
        }

        private void Start() => InitAds();

        private void OnDestroy()
        {
            AdManager.RevenuePaid -= OnRevenue;
        }

        private void InitAds()
        {
            if (config == null)
            {
                Append("ERROR: assign an AdManagerConfig on DemoController");
                return;
            }
            AdManager.Initialize(config);
            Append($"AdManager initialized - mediator: {AdManager.ActiveMediatorName}");
        }

        private void OnRevenue(AdRevenueInfo info)
        {
            _totalRevenue += info.Amount;
            Append($"revenue: {info}");
        }

        private void Update()
        {
            if (statusText != null)
            {
                statusText.text =
                    $"Initialized: {AdManager.IsInitialized}\n" +
                    $"Mediator: {AdManager.ActiveMediatorName}\n" +
                    $"Ads shown: {_adsShown}\n" +
                    $"Revenue (sim): ${_totalRevenue:0.0000}\n" +
                    $"Consent: {ConsentManager.GdprStatus}";
            }
            if (logText != null)
            {
                logText.text = string.Join("\n", _logLines);
            }
        }

        private void Append(string line)
        {
            _logLines.Add($"[{Time.realtimeSinceStartup:0.0}s] {line}");
            if (_logLines.Count > 12) _logLines.RemoveAt(0);
            AdLogger.Tag("DEMO", line);
        }

        private static void Wire(Button btn, System.Action action)
        {
            if (btn == null || action == null) return;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => action());
        }
    }
}
