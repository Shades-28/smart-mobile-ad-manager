using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    [Serializable]
    public class AbExperiment
    {
        [Tooltip("Stable experiment key used to derive deterministic bucketing.")]
        [SerializeField] private string _key;
        [Tooltip("Optional: tie this experiment to a specific placement key. Empty = applies globally.")]
        [SerializeField] private string _placement = "";

        [SerializeField] private List<AbVariant> _variants = new List<AbVariant>
        {
            new AbVariant(),
        };

        public string Key => _key ?? string.Empty;
        public string Placement => _placement ?? string.Empty;
        public IReadOnlyList<AbVariant> Variants => _variants;
    }

    [CreateAssetMenu(
        fileName = "AbExperimentCatalog",
        menuName = "Rinval/Mobile Ads & IAP/A-B Experiment Catalog",
        order = 160)]
    public class AbExperimentCatalog : ScriptableObject
    {
        [SerializeField] private List<AbExperiment> _experiments = new List<AbExperiment>();

        public IReadOnlyList<AbExperiment> Experiments => _experiments;

        public AbExperiment Find(string key)
        {
            foreach (var e in _experiments) if (e.Key == key) return e;
            return null;
        }
    }
}
