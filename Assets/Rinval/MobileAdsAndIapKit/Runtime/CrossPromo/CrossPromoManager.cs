using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    [Serializable]
    public class CrossPromoEntry
    {
        [SerializeField] private string _appName;
        [SerializeField] private string _storeUrlAndroid;
        [SerializeField] private string _storeUrlIos;
        [SerializeField] private Sprite _icon;
        [SerializeField] private string _tagline;

        public string AppName => _appName ?? string.Empty;
        public string StoreUrl
        {
            get
            {
#if UNITY_ANDROID
                return _storeUrlAndroid;
#elif UNITY_IOS
                return _storeUrlIos;
#else
                return _storeUrlAndroid ?? _storeUrlIos;
#endif
            }
        }
        public Sprite Icon => _icon;
        public string Tagline => _tagline ?? string.Empty;
    }

    [CreateAssetMenu(fileName = "CrossPromoCatalog", menuName = "Rinval/Mobile Ads & IAP/Cross-Promo Catalog", order = 200)]
    public class CrossPromoCatalog : ScriptableObject
    {
        [SerializeField] private List<CrossPromoEntry> _entries = new List<CrossPromoEntry>();

        [Header("Frequency")]
        [Min(0)][SerializeField] private int _minIntervalSeconds = 180;
        [Min(1)][SerializeField] private int _appOpensBeforeFirst = 2;

        public IReadOnlyList<CrossPromoEntry> Entries => _entries;
        public int MinIntervalSeconds => _minIntervalSeconds;
        public int AppOpensBeforeFirst => _appOpensBeforeFirst;
    }

    /// <summary>Local cross-promotion of your other apps. Picks an entry from the catalog according to frequency limits; the publisher renders it however they want via OnEntryReady. Click handling is deliberately simple: opens the platform store URL.</summary>
    public static class CrossPromoManager
    {
        private const string KeyAppOpens = "Rinval.MobileAdsIap.CrossPromoAppOpens";
        private const string KeyLastShownTicks = "Rinval.MobileAdsIap.CrossPromoLastShown";
        private const string KeyRotation = "Rinval.MobileAdsIap.CrossPromoIndex";

        public static event Action<CrossPromoEntry> OnEntryReady;

        public static void RecordAppOpen()
        {
            var n = PlayerPrefs.GetInt(KeyAppOpens, 0) + 1;
            PlayerPrefs.SetInt(KeyAppOpens, n);
            PlayerPrefs.Save();
        }

        public static bool CanShow(CrossPromoCatalog catalog, out string reason)
        {
            reason = string.Empty;
            if (catalog == null || catalog.Entries.Count == 0) { reason = "empty catalog"; return false; }
            if (PlayerPrefs.GetInt(KeyAppOpens, 0) < catalog.AppOpensBeforeFirst) { reason = "app-opens threshold"; return false; }
            var ticks = long.TryParse(PlayerPrefs.GetString(KeyLastShownTicks, "0"), out var t) ? t : 0;
            var lastShown = ticks > 0 ? new DateTime(ticks, DateTimeKind.Utc) : DateTime.MinValue;
            if ((DateTime.UtcNow - lastShown).TotalSeconds < catalog.MinIntervalSeconds) { reason = "min-interval cap"; return false; }
            return true;
        }

        public static bool TryShow(CrossPromoCatalog catalog)
        {
            if (!CanShow(catalog, out var reason)) { AdLogger.Tag("XPROMO", $"skip: {reason}"); return false; }
            var idx = PlayerPrefs.GetInt(KeyRotation, 0) % catalog.Entries.Count;
            var entry = catalog.Entries[idx];
            PlayerPrefs.SetInt(KeyRotation, idx + 1);
            PlayerPrefs.SetString(KeyLastShownTicks, DateTime.UtcNow.Ticks.ToString());
            PlayerPrefs.Save();
            try { OnEntryReady?.Invoke(entry); }
            catch (Exception e) { AdLogger.Error($"CrossPromoManager.OnEntryReady listener threw: {e}"); }
            return true;
        }

        public static void OpenStore(CrossPromoEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.StoreUrl)) return;
            try { Application.OpenURL(entry.StoreUrl); }
            catch (Exception e) { AdLogger.Error($"CrossPromoManager.OpenStore threw: {e}"); }
        }

        public static void ResetForTests()
        {
            PlayerPrefs.DeleteKey(KeyAppOpens);
            PlayerPrefs.DeleteKey(KeyLastShownTicks);
            PlayerPrefs.DeleteKey(KeyRotation);
        }
    }
}
