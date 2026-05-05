using System.Collections.Generic;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    [CreateAssetMenu(
        fileName = "AdManagerConfig",
        menuName = "Rinval/Ads/Ad Manager Config",
        order = 100)]
    public class AdManagerConfig : ScriptableObject, IConfigSource
    {
        [Header("Master Switches")]
        [Tooltip("Master enable. When off, all ad calls become no-ops.")]
        [SerializeField] private bool _adsEnabled = true;

        [SerializeField] private bool _interstitialsEnabled = true;
        [SerializeField] private bool _rewardedEnabled = true;
        [SerializeField] private bool _bannersEnabled = true;
        [SerializeField] private bool _appOpenEnabled = true;

        [Header("Mediator")]
        [SerializeField] private MediatorKind _mediator = MediatorKind.AppLovinMax;

        [Header("Ad Unit IDs - Android")]
        [SerializeField] private string _bannerIdAndroid;
        [SerializeField] private string _interstitialIdAndroid;
        [SerializeField] private string _rewardedIdAndroid;
        [SerializeField] private string _rewardedInterstitialIdAndroid;
        [SerializeField] private string _mrecIdAndroid;
        [SerializeField] private string _nativeIdAndroid;
        [SerializeField] private string _appOpenIdAndroid;

        [Header("Ad Unit IDs - iOS")]
        [SerializeField] private string _bannerIdIos;
        [SerializeField] private string _interstitialIdIos;
        [SerializeField] private string _rewardedIdIos;
        [SerializeField] private string _rewardedInterstitialIdIos;
        [SerializeField] private string _mrecIdIos;
        [SerializeField] private string _nativeIdIos;
        [SerializeField] private string _appOpenIdIos;

        [Header("Banner")]
        [SerializeField] private BannerAnchor _defaultBannerAnchor = BannerAnchor.Bottom;

        [Header("Interstitial Frequency Caps")]
        [Min(0)] [SerializeField] private int _interstitialMinIntervalSeconds = 60;
        [Min(0)] [SerializeField] private int _interstitialMinStage = 0;
        [Min(0)] [SerializeField] private int _interstitialMaxPerWindow = 5;
        [Min(1)] [SerializeField] private int _interstitialWindowSeconds = 300;
        [SerializeField] private bool _skipFirstInterstitial = true;

        [Header("App Open")]
        [Min(0)] [SerializeField] private int _appOpenCooldownSeconds = 60;
        [Min(0)] [SerializeField] private int _appOpenMinAwaySeconds = 30;
        [SerializeField] private bool _appOpenSkipFirstLaunch = true;

        [Header("Per-Placement Caps (optional)")]
        [Tooltip("Optional per-placement frequency caps. Empty list = global caps only.")]
        [SerializeField] private List<PlacementRule> _placementRules = new List<PlacementRule>();

        [Header("Diagnostics")]
        [SerializeField] private bool _verboseLogging = true;
        [Tooltip("When on, the active mediator is forced to serve test ads.")]
        [SerializeField] private bool _testMode = true;

        public bool AdsEnabled => _adsEnabled;
        public bool InterstitialsEnabled => _interstitialsEnabled;
        public bool RewardedEnabled => _rewardedEnabled;
        public bool BannersEnabled => _bannersEnabled;
        public bool AppOpenEnabled => _appOpenEnabled;

        public MediatorKind Mediator => _mediator;
        public BannerAnchor DefaultBannerAnchor => _defaultBannerAnchor;

        public int InterstitialMinIntervalSeconds => _interstitialMinIntervalSeconds;
        public int InterstitialMinStage => _interstitialMinStage;
        public int InterstitialMaxPerWindow => _interstitialMaxPerWindow;
        public int InterstitialWindowSeconds => _interstitialWindowSeconds;
        public bool SkipFirstInterstitial => _skipFirstInterstitial;

        public int AppOpenCooldownSeconds => _appOpenCooldownSeconds;
        public int AppOpenMinAwaySeconds => _appOpenMinAwaySeconds;
        public bool AppOpenSkipFirstLaunch => _appOpenSkipFirstLaunch;

        public bool VerboseLogging => _verboseLogging;
        public bool TestMode => _testMode;
        public IList<PlacementRule> PlacementRules => _placementRules ?? (_placementRules = new List<PlacementRule>());

        public string GetBannerId() => Pick(_bannerIdAndroid, _bannerIdIos);
        public string GetInterstitialId() => Pick(_interstitialIdAndroid, _interstitialIdIos);
        public string GetRewardedId() => Pick(_rewardedIdAndroid, _rewardedIdIos);
        public string GetRewardedInterstitialId() => Pick(_rewardedInterstitialIdAndroid, _rewardedInterstitialIdIos);
        public string GetMrecId() => Pick(_mrecIdAndroid, _mrecIdIos);
        public string GetNativeId() => Pick(_nativeIdAndroid, _nativeIdIos);
        public string GetAppOpenId() => Pick(_appOpenIdAndroid, _appOpenIdIos);

        public string GetIdFor(AdFormat format)
        {
            switch (format)
            {
                case AdFormat.Banner: return GetBannerId();
                case AdFormat.Interstitial: return GetInterstitialId();
                case AdFormat.Rewarded: return GetRewardedId();
                case AdFormat.RewardedInterstitial: return GetRewardedInterstitialId();
                case AdFormat.MediumRectangle: return GetMrecId();
                case AdFormat.Native: return GetNativeId();
                case AdFormat.AppOpen: return GetAppOpenId();
                default: return string.Empty;
            }
        }

        private static string Pick(string android, string ios)
        {
#if UNITY_ANDROID
            return android ?? string.Empty;
#elif UNITY_IOS
            return ios ?? string.Empty;
#else
            return android ?? ios ?? string.Empty;
#endif
        }

        public void SetTestMode(bool value) => _testMode = value;
        public void SetVerbose(bool value) => _verboseLogging = value;
        public void SetAdsEnabled(bool value) => _adsEnabled = value;
        public void SetMediator(MediatorKind value) => _mediator = value;
    }
}
