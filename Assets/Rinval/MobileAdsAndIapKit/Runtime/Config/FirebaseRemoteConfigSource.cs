#if RINVAL_FIREBASE_REMOTECONFIG
using System.Collections.Generic;
using Firebase.RemoteConfig;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Reference IConfigSource backed by Firebase Remote Config. Wraps an AdManagerConfig for defaults; overrides each field if the matching remote key exists.</summary>
    public class FirebaseRemoteConfigSource : IConfigSource
    {
        private readonly AdManagerConfig _fallback;

        public FirebaseRemoteConfigSource(AdManagerConfig fallback)
        {
            _fallback = fallback;
        }

        private static FirebaseRemoteConfig RC => FirebaseRemoteConfig.DefaultInstance;

        private bool BoolKey(string key, bool fallback)
        {
            try { var v = RC.GetValue(key); return v.Source == ValueSource.RemoteValue ? v.BooleanValue : fallback; }
            catch { return fallback; }
        }

        private int IntKey(string key, int fallback)
        {
            try { var v = RC.GetValue(key); return v.Source == ValueSource.RemoteValue ? (int)v.LongValue : fallback; }
            catch { return fallback; }
        }

        public bool AdsEnabled => BoolKey("ads_enabled", _fallback.AdsEnabled);
        public bool InterstitialsEnabled => BoolKey("interstitials_enabled", _fallback.InterstitialsEnabled);
        public bool RewardedEnabled => BoolKey("rewarded_enabled", _fallback.RewardedEnabled);
        public bool BannersEnabled => BoolKey("banners_enabled", _fallback.BannersEnabled);
        public bool AppOpenEnabled => BoolKey("app_open_enabled", _fallback.AppOpenEnabled);

        public int InterstitialMinIntervalSeconds => IntKey("interstitial_min_interval_seconds", _fallback.InterstitialMinIntervalSeconds);
        public int InterstitialMinStage => IntKey("interstitial_min_stage", _fallback.InterstitialMinStage);
        public int InterstitialMaxPerWindow => IntKey("interstitial_max_per_window", _fallback.InterstitialMaxPerWindow);
        public int InterstitialWindowSeconds => IntKey("interstitial_window_seconds", _fallback.InterstitialWindowSeconds);
        public bool SkipFirstInterstitial => BoolKey("skip_first_interstitial", _fallback.SkipFirstInterstitial);

        public int AppOpenCooldownSeconds => IntKey("app_open_cooldown_seconds", _fallback.AppOpenCooldownSeconds);
        public int AppOpenMinAwaySeconds => IntKey("app_open_min_away_seconds", _fallback.AppOpenMinAwaySeconds);
        public bool AppOpenSkipFirstLaunch => BoolKey("app_open_skip_first_launch", _fallback.AppOpenSkipFirstLaunch);

        public bool VerboseLogging => _fallback.VerboseLogging;
        public bool TestMode => _fallback.TestMode;
        public IList<PlacementRule> PlacementRules => _fallback.PlacementRules;
    }
}
#endif
