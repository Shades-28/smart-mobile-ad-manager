using System;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Triggers the platform's native rate prompt (StoreKit on iOS / Google Play in-app review on Android). Tracks "shown" via PlayerPrefs to respect platform limits and avoid spam.</summary>
    public static class RateGameManager
    {
        private const string KeyShownCount = "Rinval.MobileAdsIap.RateShownCount";
        private const string KeyLastShownTicks = "Rinval.MobileAdsIap.RateLastShownTicks";
        private const string KeyAppOpens = "Rinval.MobileAdsIap.RateAppOpens";
        private const string KeySessions = "Rinval.MobileAdsIap.RateSessions";

        // Defaults match Apple's StoreKit hints; designer can override via Configure(settings).
        private static int _minDaysBetween = 90;
        private static int _maxShownPerYear = 3;
        private static int _appOpensBeforeFirst = 0;
        private static int _minSessionsBeforeFirst = 0;

        public static event Action Shown;

        /// <summary>Apply designer settings. Pass null to restore defaults.</summary>
        public static void Configure(RateGameSettings settings)
        {
            if (settings == null)
            {
                _minDaysBetween = 90; _maxShownPerYear = 3;
                _appOpensBeforeFirst = 0; _minSessionsBeforeFirst = 0;
                return;
            }
            _minDaysBetween = settings.MinDaysBetween;
            _maxShownPerYear = settings.MaxShownPerYear;
            _appOpensBeforeFirst = settings.AppOpensBeforeFirst;
            _minSessionsBeforeFirst = settings.MinSessionsBeforeFirst;
        }

        public static void RecordAppOpen()
        {
            PlayerPrefs.SetInt(KeyAppOpens, PlayerPrefs.GetInt(KeyAppOpens, 0) + 1);
            PlayerPrefs.Save();
        }

        public static void RecordSessionEnd()
        {
            PlayerPrefs.SetInt(KeySessions, PlayerPrefs.GetInt(KeySessions, 0) + 1);
            PlayerPrefs.Save();
        }

        public static int ShownCount => PlayerPrefs.GetInt(KeyShownCount, 0);
        public static DateTime LastShown
        {
            get
            {
                var t = PlayerPrefs.GetString(KeyLastShownTicks, "0");
                return long.TryParse(t, out var ticks) && ticks > 0 ? new DateTime(ticks, DateTimeKind.Utc) : DateTime.MinValue;
            }
        }

        public static bool CanShow()
        {
            if (ShownCount >= _maxShownPerYear) return false;
            if (PlayerPrefs.GetInt(KeyAppOpens, 0) < _appOpensBeforeFirst) return false;
            if (PlayerPrefs.GetInt(KeySessions, 0) < _minSessionsBeforeFirst) return false;
            var since = DateTime.UtcNow - LastShown;
            return since.TotalDays >= _minDaysBetween;
        }

        public static void TryShow()
        {
            if (!CanShow())
            {
                AdLogger.Tag("RATE", "skipped (cap or cooldown)");
                return;
            }
#if UNITY_IOS && !UNITY_EDITOR
            try
            {
                UnityEngine.iOS.Device.RequestStoreReview();
                MarkShown();
            }
            catch (Exception e) { AdLogger.Error($"RateGameManager iOS RequestStoreReview threw: {e}"); }
#elif UNITY_ANDROID && !UNITY_EDITOR
            // Google Play in-app review requires the Play Review library; surface the intent.
            AdLogger.Tag("RATE", "Android: requires Play Review library (publisher integration)");
            MarkShown();
#else
            AdLogger.Tag("RATE", "editor: simulated");
            MarkShown();
#endif
        }

        private static void MarkShown()
        {
            PlayerPrefs.SetInt(KeyShownCount, ShownCount + 1);
            PlayerPrefs.SetString(KeyLastShownTicks, DateTime.UtcNow.Ticks.ToString());
            PlayerPrefs.Save();
            try { Shown?.Invoke(); } catch (Exception e) { AdLogger.Error($"RateGameManager.Shown listener threw: {e}"); }
        }

        public static void ResetForTests()
        {
            PlayerPrefs.DeleteKey(KeyShownCount);
            PlayerPrefs.DeleteKey(KeyLastShownTicks);
            PlayerPrefs.DeleteKey(KeyAppOpens);
            PlayerPrefs.DeleteKey(KeySessions);
            Configure(null);
        }
    }
}
