using UnityEditor;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    public static class ConfigValidator
    {
        public static void Validate(AdManagerConfig cfg)
        {
            if (cfg == null) { Debug.LogError("[Rinval.MobileAdsIap] No config to validate"); return; }

            int errors = 0, warnings = 0;
            void Err(string m) { errors++; Debug.LogError($"[Rinval.MobileAdsIap] {m}"); }
            void Warn(string m) { warnings++; Debug.LogWarning($"[Rinval.MobileAdsIap] {m}"); }

            if (cfg.Mediator == MediatorKind.None) Warn("Mediator is None - only stub ads will serve in builds");

            if (cfg.AdsEnabled)
            {
                if (cfg.BannersEnabled && string.IsNullOrEmpty(cfg.GetBannerId())) Warn("Banner ID empty for current platform");
                if (cfg.InterstitialsEnabled && string.IsNullOrEmpty(cfg.GetInterstitialId())) Warn("Interstitial ID empty for current platform");
                if (cfg.RewardedEnabled && string.IsNullOrEmpty(cfg.GetRewardedId())) Warn("Rewarded ID empty for current platform");
                if (cfg.AppOpenEnabled && string.IsNullOrEmpty(cfg.GetAppOpenId())) Warn("App Open ID empty for current platform");
            }

            if (cfg.InterstitialMinIntervalSeconds < 0) Err("InterstitialMinIntervalSeconds must be >= 0");
            if (cfg.InterstitialMaxPerWindow < 0) Err("InterstitialMaxPerWindow must be >= 0");
            if (cfg.InterstitialWindowSeconds <= 0) Err("InterstitialWindowSeconds must be > 0");
            if (cfg.AppOpenCooldownSeconds < 0) Err("AppOpenCooldownSeconds must be >= 0");
            if (cfg.AppOpenMinAwaySeconds < 0) Err("AppOpenMinAwaySeconds must be >= 0");

            Debug.Log($"[Rinval.MobileAdsIap] Validation: {errors} error(s), {warnings} warning(s).");
            if (errors == 0 && warnings == 0) Debug.Log("[Rinval.MobileAdsIap] Config OK ✓");
        }
    }
}
