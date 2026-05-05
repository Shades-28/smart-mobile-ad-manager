using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Lightweight client-side A/B test framework. Sticky-buckets a user into a variant via a stable hash of (userId, experimentKey). No backend required; the kit makes the assignment once per session and remembers it. Usage: AbTestManager.SetCatalog(myCatalog); var variant = AbTestManager.GetVariantKey("interstitial_cap_v2"); if (variant == "aggressive") AdManager.SetCurrentStage(0);</summary>
    public static class AbTestManager
    {
        private const string KeyUserId = "Rinval.MobileAdsIap.AbUserId";
        private static AbExperimentCatalog _catalog;
        private static readonly Dictionary<string, string> _assignments = new Dictionary<string, string>();
        private static string _userId;

        public static event Action<string, string> VariantAssigned; // (experimentKey, variantKey)

        public static void SetCatalog(AbExperimentCatalog catalog)
        {
            _catalog = catalog;
            _assignments.Clear();
        }

        public static AbExperimentCatalog Catalog => _catalog;

        public static string UserId
        {
            get
            {
                if (!string.IsNullOrEmpty(_userId)) return _userId;
                _userId = PlayerPrefs.GetString(KeyUserId, null);
                if (string.IsNullOrEmpty(_userId))
                {
                    _userId = Guid.NewGuid().ToString("N");
                    PlayerPrefs.SetString(KeyUserId, _userId);
                    PlayerPrefs.Save();
                }
                return _userId;
            }
        }

        /// <summary>Returns the variant key the user is bucketed into for the given experiment.</summary>
        public static string GetVariantKey(string experimentKey)
        {
            if (_catalog == null || string.IsNullOrEmpty(experimentKey)) return "control";
            if (_assignments.TryGetValue(experimentKey, out var cached)) return cached;
            var exp = _catalog.Find(experimentKey);
            if (exp == null || exp.Variants.Count == 0) { _assignments[experimentKey] = "control"; return "control"; }

            int totalWeight = 0;
            foreach (var v in exp.Variants) totalWeight += v.Weight;
            if (totalWeight <= 0) { _assignments[experimentKey] = exp.Variants[0].Key; return exp.Variants[0].Key; }

            uint hash = StableHash(UserId + ":" + experimentKey);
            int bucket = (int)(hash % (uint)totalWeight);

            int accum = 0;
            foreach (var v in exp.Variants)
            {
                accum += v.Weight;
                if (bucket < accum)
                {
                    _assignments[experimentKey] = v.Key;
                    try { VariantAssigned?.Invoke(experimentKey, v.Key); }
                    catch (Exception e) { AdLogger.Error($"AbTestManager.VariantAssigned listener threw: {e}"); }
                    AdLogger.Tag("AB", $"{experimentKey} → {v.Key} (user {UserId.Substring(0, 8)})");
                    return v.Key;
                }
            }
            _assignments[experimentKey] = exp.Variants[0].Key;
            return exp.Variants[0].Key;
        }

        /// <summary>Returns the assigned variant object (or null) for inspection.</summary>
        public static AbVariant GetVariant(string experimentKey)
        {
            if (_catalog == null) return null;
            var exp = _catalog.Find(experimentKey);
            if (exp == null) return null;
            var key = GetVariantKey(experimentKey);
            foreach (var v in exp.Variants) if (v.Key == key) return v;
            return null;
        }

        public static IReadOnlyDictionary<string, string> AllAssignments => _assignments;

        public static void ResetForTests()
        {
            _assignments.Clear();
            _userId = null;
            PlayerPrefs.DeleteKey(KeyUserId);
        }

        // FNV-1a 32-bit. Stable across runs and platforms - does NOT use String.GetHashCode
        // because that's randomized per process in modern .NET.
        private static uint StableHash(string s)
        {
            unchecked
            {
                uint h = 2166136261u;
                foreach (var c in s) { h ^= c; h *= 16777619u; }
                return h;
            }
        }
    }
}
