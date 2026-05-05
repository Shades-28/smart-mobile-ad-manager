using System.Collections.Generic;

namespace Rinval.MobileAdsAndIapKit
{
    public interface IConfigSource
    {
        bool AdsEnabled { get; }
        bool InterstitialsEnabled { get; }
        bool RewardedEnabled { get; }
        bool BannersEnabled { get; }
        bool AppOpenEnabled { get; }

        int InterstitialMinIntervalSeconds { get; }
        int InterstitialMinStage { get; }
        int InterstitialMaxPerWindow { get; }
        int InterstitialWindowSeconds { get; }
        bool SkipFirstInterstitial { get; }

        int AppOpenCooldownSeconds { get; }
        int AppOpenMinAwaySeconds { get; }
        bool AppOpenSkipFirstLaunch { get; }

        bool VerboseLogging { get; }
        bool TestMode { get; }

        /// <summary>Per-placement override rules. Empty list = no placement-level gating.</summary>
        IList<PlacementRule> PlacementRules { get; }
    }
}
