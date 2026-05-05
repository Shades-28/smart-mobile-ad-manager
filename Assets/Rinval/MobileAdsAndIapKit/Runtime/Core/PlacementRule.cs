using System;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Per-placement frequency cap rule. Optional override of the global interstitial caps for a specific placement key (e.g. "level_complete", "shop", "game_over").</summary>
    [Serializable]
    public class PlacementRule
    {
        [SerializeField] private string _placement;
        [Min(0)][SerializeField] private int _minIntervalSeconds = 30;
        [Min(0)][SerializeField] private int _maxPerWindow = 3;
        [Min(1)][SerializeField] private int _windowSeconds = 300;

        public string Placement => _placement ?? string.Empty;
        public int MinIntervalSeconds => _minIntervalSeconds;
        public int MaxPerWindow => _maxPerWindow;
        public int WindowSeconds => _windowSeconds;

        public PlacementRule() { }

        public PlacementRule(string placement, int minIntervalSeconds, int maxPerWindow, int windowSeconds)
        {
            _placement = placement;
            _minIntervalSeconds = minIntervalSeconds;
            _maxPerWindow = maxPerWindow;
            _windowSeconds = windowSeconds;
        }
    }
}
