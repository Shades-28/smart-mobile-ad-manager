using System.Collections.Generic;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Authored list of placement keys used in the game. Pair with PlacementCatalog.Get(key) or generated PlacementIds (from the Editor menu) to eliminate string-typo bugs at AdManager.Show* call sites.</summary>
    [CreateAssetMenu(
        fileName = "PlacementCatalog",
        menuName = "Rinval/Mobile Ads & IAP/Placement Catalog",
        order = 120)]
    public class PlacementCatalog : ScriptableObject
    {
        [SerializeField] private List<string> _placements = new List<string>
        {
            "default",
            "level_complete",
            "level_fail",
            "shop_open",
            "double_coins",
            "continue",
            "main_menu",
        };

        public IReadOnlyList<string> Placements => _placements;

        public bool Contains(string key) => !string.IsNullOrEmpty(key) && _placements.Contains(key);
    }
}
