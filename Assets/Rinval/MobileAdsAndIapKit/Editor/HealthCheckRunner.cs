using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>One-shot diagnostic. Runs every subsystem in sequence and prints a clear pass/fail report to the console + a summary dialog. Trust signal: a buyer with a broken setup runs this and learns exactly what's wrong without reading 200 pages of docs.</summary>
    public static class HealthCheckRunner
    {
        private struct Check { public string Name; public bool Pass; public string Detail; }

        [MenuItem("Rinval/Mobile Ads & IAP Kit/Run Health Check", priority = 3)]
        public static void Run()
        {
            var checks = new List<Check>();

            // 1. AdManagerConfig discoverable
            var cfgGuids = AssetDatabase.FindAssets("t:AdManagerConfig");
            var cfg = cfgGuids.Length > 0 ? AssetDatabase.LoadAssetAtPath<AdManagerConfig>(AssetDatabase.GUIDToAssetPath(cfgGuids[0])) : null;
            checks.Add(new Check
            {
                Name = "AdManagerConfig present",
                Pass = cfg != null,
                Detail = cfg != null ? AssetDatabase.GetAssetPath(cfg) : "No AdManagerConfig found in project."
            });

            // 2. Mediator selected
            checks.Add(new Check
            {
                Name = "Mediator selected",
                Pass = cfg != null && cfg.Mediator != MediatorKind.None,
                Detail = cfg == null ? "n/a" : cfg.Mediator.ToString()
            });

            // 3. Ad unit IDs
            if (cfg != null)
            {
                var missing = new List<string>();
                if (string.IsNullOrEmpty(cfg.GetBannerId())) missing.Add("banner");
                if (string.IsNullOrEmpty(cfg.GetInterstitialId())) missing.Add("interstitial");
                if (string.IsNullOrEmpty(cfg.GetRewardedId())) missing.Add("rewarded");
                checks.Add(new Check
                {
                    Name = "Ad unit IDs",
                    Pass = missing.Count == 0,
                    Detail = missing.Count == 0 ? "All set" : "Missing: " + string.Join(", ", missing)
                });
            }

            // 4. Scripting define matches mediator
            if (cfg != null && cfg.Mediator != MediatorKind.None)
            {
                var group = EditorUserBuildSettings.selectedBuildTargetGroup;
                var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                string expected = cfg.Mediator switch
                {
                    MediatorKind.AppLovinMax => "AD_USE_APPLOVIN",
                    MediatorKind.GoogleAdMob => "AD_USE_ADMOB",
                    MediatorKind.UnityLevelPlay => "AD_USE_LEVELPLAY",
                    _ => ""
                };
                checks.Add(new Check
                {
                    Name = "Scripting define matches mediator",
                    Pass = defines.Contains(expected),
                    Detail = defines.Contains(expected) ? expected + " set" : $"Expected {expected}, not in defines for {group}"
                });
            }

            // 5. AdManager static surface compiles
            checks.Add(SafeRun("AdManager API reachable", () =>
            {
                var _ = AdManager.IsInitialized;
                var __ = AdManager.ActiveMediatorName;
                var ___ = AdManager.LastShowVerdict;
                return "ok";
            }));

            // 6. IapManager API reachable
            checks.Add(SafeRun("IapManager API reachable", () =>
            {
                var _ = IapManager.IsInitialized;
                return "ok";
            }));

            // 7. MainThreadDispatcher safe
            checks.Add(SafeRun("MainThreadDispatcher reachable", () =>
            {
                var _ = MainThreadDispatcher.Instance;
                return "instance acquired";
            }));

            // 8. ConsentManager API reachable
            checks.Add(SafeRun("ConsentManager API reachable", () =>
            {
                var _ = ConsentManager.GdprStatus;
                return "ok";
            }));

            // 9. AssetDatabase: PlacementCatalog optional but checked
            var pcGuids = AssetDatabase.FindAssets("t:PlacementCatalog");
            checks.Add(new Check { Name = "PlacementCatalog (optional)", Pass = true, Detail = pcGuids.Length > 0 ? "found" : "none (optional)" });

            // Print report
            PrintReport(checks);
        }

        private static Check SafeRun(string name, System.Func<string> body)
        {
            try { return new Check { Name = name, Pass = true, Detail = body() }; }
            catch (System.Exception e) { return new Check { Name = name, Pass = false, Detail = e.Message }; }
        }

        private static void PrintReport(List<Check> checks)
        {
            int passed = 0, failed = 0;
            var sb = new StringBuilder();
            sb.AppendLine("=== Mobile Ads & IAP Kit - Health Check ===");
            foreach (var c in checks)
            {
                if (c.Pass) passed++; else failed++;
                sb.AppendLine($"[{(c.Pass ? "PASS" : "FAIL")}] {c.Name} - {c.Detail}");
            }
            sb.AppendLine($"\nTotal: {passed} passed · {failed} failed");
            Debug.Log(sb.ToString());
            EditorUtility.DisplayDialog(
                "Health Check Result",
                $"{passed} passed, {failed} failed.\n\nFull report in the Console.",
                "OK");
        }
    }
}
