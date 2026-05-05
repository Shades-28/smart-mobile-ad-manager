using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Editor menu: select a PlacementCatalog asset, then run this to emit a PlacementIds.cs file with one const-string per placement. Devs use AdManager.ShowRewarded(PlacementIds.LevelComplete) instead of magic strings → typos caught at compile time.</summary>
    public static class PlacementIdsGenerator
    {
        private const string OutputPath = "Assets/Rinval/MobileAdsAndIapKit/Runtime/Core/PlacementIds.Generated.cs";

        [MenuItem("Rinval/Mobile Ads & IAP Kit/Generate PlacementIds.cs from selected catalog", priority = 20)]
        public static void Generate()
        {
            var catalog = Selection.activeObject as PlacementCatalog;
            if (catalog == null)
            {
                EditorUtility.DisplayDialog("Select a PlacementCatalog",
                    "Click a PlacementCatalog asset in the Project window first, then run this menu.", "OK");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("// Auto-generated from PlacementCatalog. Do not edit by hand.");
            sb.AppendLine("// Regenerate via Rinval → Mobile Ads & IAP Kit → Generate PlacementIds.cs.");
            sb.AppendLine("namespace Rinval.MobileAdsAndIapKit");
            sb.AppendLine("{");
            sb.AppendLine("    public static class PlacementIds");
            sb.AppendLine("    {");
            foreach (var p in catalog.Placements)
            {
                if (string.IsNullOrEmpty(p)) continue;
                sb.AppendLine($"        public const string {ToConstName(p)} = \"{p}\";");
            }
            sb.AppendLine("    }");
            sb.AppendLine("}");

            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
            File.WriteAllText(OutputPath, sb.ToString());
            AssetDatabase.ImportAsset(OutputPath);
            EditorUtility.DisplayDialog("PlacementIds Generated",
                $"Wrote {catalog.Placements.Count} const(s) to:\n{OutputPath}", "OK");
        }

        private static string ToConstName(string raw)
        {
            var sb = new StringBuilder();
            bool capitalizeNext = true;
            foreach (var ch in raw)
            {
                if (ch == '_' || ch == '-' || ch == ' ') { capitalizeNext = true; continue; }
                if (!char.IsLetterOrDigit(ch)) continue;
                sb.Append(capitalizeNext ? char.ToUpperInvariant(ch) : ch);
                capitalizeNext = false;
            }
            if (sb.Length == 0 || char.IsDigit(sb[0])) sb.Insert(0, "P");
            return sb.ToString();
        }
    }
}
