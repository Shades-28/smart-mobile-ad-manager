using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Designer-tunable rate-prompt rules. Drop into RateGameManager.Configure(settings) at startup or assign on MobileKitBootstrap. Replaces hardcoded 90-day / 3-per-year defaults.</summary>
    [CreateAssetMenu(
        fileName = "RateGameSettings",
        menuName = "Rinval/Mobile Ads & IAP/Rate Game Settings",
        order = 140)]
    public class RateGameSettings : ScriptableObject
    {
        [Tooltip("Minimum days between successive prompts (Apple StoreKit hint = 90).")]
        [Min(0)][SerializeField] private int _minDaysBetween = 90;

        [Tooltip("Maximum number of prompts to show in a single year (Apple cap = 3).")]
        [Min(1)][SerializeField] private int _maxShownPerYear = 3;

        [Tooltip("Number of app-opens before the first prompt is allowed.")]
        [Min(0)][SerializeField] private int _appOpensBeforeFirst = 5;

        [Tooltip("Optional minimum session count (if you call RateGameManager.RecordSessionEnd elsewhere).")]
        [Min(0)][SerializeField] private int _minSessionsBeforeFirst = 3;

        public int MinDaysBetween => _minDaysBetween;
        public int MaxShownPerYear => _maxShownPerYear;
        public int AppOpensBeforeFirst => _appOpensBeforeFirst;
        public int MinSessionsBeforeFirst => _minSessionsBeforeFirst;
    }
}
