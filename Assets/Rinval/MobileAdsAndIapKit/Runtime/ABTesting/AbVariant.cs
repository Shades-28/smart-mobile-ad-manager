using System;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    [Serializable]
    public class AbVariant
    {
        [SerializeField] private string _key = "control";
        [Tooltip("Bucket weight 0-100. The kit normalizes weights across all variants in an experiment.")]
        [Range(0, 100)][SerializeField] private int _weight = 50;

        [Header("Frequency overrides (optional)")]
        [Tooltip("If > 0, overrides the placement's MinIntervalSeconds for users in this variant.")]
        [Min(0)][SerializeField] private int _minIntervalSecondsOverride = 0;
        [Tooltip("If > 0, overrides the placement's MaxPerWindow for users in this variant.")]
        [Min(0)][SerializeField] private int _maxPerWindowOverride = 0;

        public string Key => _key ?? "control";
        public int Weight => Mathf.Max(0, _weight);
        public int MinIntervalSecondsOverride => _minIntervalSecondsOverride;
        public int MaxPerWindowOverride => _maxPerWindowOverride;
    }
}
