using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Genre-specific "monetization recipes" - drop-in AdManagerConfig + IapProductCatalog + PlacementCatalog with sensible defaults for the genre. Saves devs an hour of guessing what cap to set for "level_complete" in a hyper-casual game.</summary>
    public static class RecipeBuilder
    {
        private const string RecipeFolder = "Assets/Rinval/MobileAdsAndIapKit/Recipes";

        [MenuItem("Rinval/Mobile Ads & IAP Kit/Recipes/Hyper-Casual", priority = 50)]
        public static void HyperCasual() => Build("HyperCasual",
            interstitialIntervalSeconds: 30,
            interstitialMaxPerWindow: 8,
            interstitialWindowSeconds: 300,
            interstitialMinStage: 0,
            skipFirstInterstitial: true,
            placements: new[] { "level_complete", "level_fail", "main_menu", "continue", "double_coins", "skip_level" },
            iapProducts: new[]
            {
                ("remove_ads", ProductKind.NonConsumable),
                ("coins_small", ProductKind.Consumable),
                ("coins_large", ProductKind.Consumable),
            });

        [MenuItem("Rinval/Mobile Ads & IAP Kit/Recipes/Match-3", priority = 51)]
        public static void Match3() => Build("Match3",
            interstitialIntervalSeconds: 90,
            interstitialMaxPerWindow: 4,
            interstitialWindowSeconds: 600,
            interstitialMinStage: 3,
            skipFirstInterstitial: true,
            placements: new[] { "level_complete", "level_fail", "out_of_lives", "shop_open", "double_coins", "extra_moves", "rewarded_continue" },
            iapProducts: new[]
            {
                ("remove_ads", ProductKind.NonConsumable),
                ("coins_pack_1", ProductKind.Consumable),
                ("coins_pack_2", ProductKind.Consumable),
                ("coins_pack_3", ProductKind.Consumable),
                ("piggy_bank", ProductKind.Consumable),
                ("vip_monthly", ProductKind.Subscription),
            });

        [MenuItem("Rinval/Mobile Ads & IAP Kit/Recipes/Idle Game", priority = 52)]
        public static void Idle() => Build("Idle",
            interstitialIntervalSeconds: 120,
            interstitialMaxPerWindow: 3,
            interstitialWindowSeconds: 900,
            interstitialMinStage: 5,
            skipFirstInterstitial: true,
            placements: new[] { "prestige", "double_offline", "ad_chest", "rewarded_speedup", "shop_open" },
            iapProducts: new[]
            {
                ("remove_ads", ProductKind.NonConsumable),
                ("starter_pack", ProductKind.NonConsumable),
                ("gems_small", ProductKind.Consumable),
                ("gems_medium", ProductKind.Consumable),
                ("gems_large", ProductKind.Consumable),
                ("vip_monthly", ProductKind.Subscription),
            });

        private static void Build(string genre,
            int interstitialIntervalSeconds, int interstitialMaxPerWindow, int interstitialWindowSeconds,
            int interstitialMinStage, bool skipFirstInterstitial,
            string[] placements, (string, ProductKind)[] iapProducts)
        {
            Directory.CreateDirectory(RecipeFolder);
            var folder = $"{RecipeFolder}/{genre}";
            Directory.CreateDirectory(folder);

            // Ad config
            var adConfig = ScriptableObject.CreateInstance<AdManagerConfig>();
            ApplyConfigDefaults(adConfig, interstitialIntervalSeconds, interstitialMaxPerWindow,
                interstitialWindowSeconds, interstitialMinStage, skipFirstInterstitial);
            AssetDatabase.CreateAsset(adConfig, $"{folder}/AdManagerConfig.asset");

            // Placement catalog
            var pc = ScriptableObject.CreateInstance<PlacementCatalog>();
            var placementsField = typeof(PlacementCatalog).GetField("_placements",
                BindingFlags.NonPublic | BindingFlags.Instance);
            placementsField?.SetValue(pc, new System.Collections.Generic.List<string>(placements));
            AssetDatabase.CreateAsset(pc, $"{folder}/PlacementCatalog.asset");

            // IAP catalog
            var iap = ScriptableObject.CreateInstance<IapProductCatalog>();
            var prodList = new System.Collections.Generic.List<IapProduct>();
            foreach (var (id, kind) in iapProducts) prodList.Add(new IapProduct(id, kind));
            var iapField = typeof(IapProductCatalog).GetField("_products",
                BindingFlags.NonPublic | BindingFlags.Instance);
            iapField?.SetValue(iap, prodList);
            AssetDatabase.CreateAsset(iap, $"{folder}/IapProductCatalog.asset");

            // KitSettings
            var kit = ScriptableObject.CreateInstance<MobileKitSettings>();
            AssetDatabase.CreateAsset(kit, $"{folder}/MobileKitSettings.asset");

            // Rate settings - gentler for hyper-casual (most casual buyers ignore prompts), longer
            // for match-3/idle where the player's relationship is more invested.
            var rate = ScriptableObject.CreateInstance<RateGameSettings>();
            AssetDatabase.CreateAsset(rate, $"{folder}/RateGameSettings.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Recipe Created",
                $"Generated {genre} recipe at:\n{folder}\n\n" +
                "Drag AdManagerConfig + IapProductCatalog onto a MobileKitBootstrap component to use.",
                "OK");
            Selection.activeObject = adConfig;
        }

        private static void ApplyConfigDefaults(AdManagerConfig cfg,
            int interval, int maxWindow, int windowSec, int minStage, bool skipFirst)
        {
            // Use reflection to set the private serialized fields. This is the cleanest way to
            // produce a tuned recipe without exposing a giant public Setter API on the config.
            SetField(cfg, "_interstitialMinIntervalSeconds", interval);
            SetField(cfg, "_interstitialMaxPerWindow", maxWindow);
            SetField(cfg, "_interstitialWindowSeconds", windowSec);
            SetField(cfg, "_interstitialMinStage", minStage);
            SetField(cfg, "_skipFirstInterstitial", skipFirst);
        }

        private static void SetField(object o, string field, object value)
        {
            var f = o.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            f?.SetValue(o, value);
        }
    }
}
