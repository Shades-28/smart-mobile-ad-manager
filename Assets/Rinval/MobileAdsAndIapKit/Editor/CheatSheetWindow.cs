using UnityEditor;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>In-editor cheat sheet - the 5-line bootstrap, common calls, and the no-code component reference. Saves the dev from hunting through Documentation/QuickStart.md.</summary>
    public class CheatSheetWindow : EditorWindow
    {
        private Vector2 _scroll;

        [MenuItem("Rinval/Mobile Ads & IAP Kit/Cheat Sheet", priority = 0)]
        public static void Open()
        {
            var w = GetWindow<CheatSheetWindow>("Mobile Ads & IAP Kit");
            w.minSize = new Vector2(560, 600);
            w.Show();
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            Header("Mobile Ads & IAP Kit - Cheat Sheet");
            EditorGUILayout.LabelField("Drop, type, click. Five lines or fewer.", EditorStyles.miniLabel);
            EditorGUILayout.Space(8);

            Section("1. The fastest path (no code)");
            Body("• Create an empty GameObject, add component MobileKitBootstrap.\n" +
                 "• Drag in your AdManagerConfig (Create → Rinval → Ads → Ad Manager Config).\n" +
                 "• Drag in your IapProductCatalog if you have IAP.\n" +
                 "• Rinval → Mobile Ads & IAP Kit → Create Consent Popup Prefab, then drop into scene.\n" +
                 "• Press Play. Done.");
            EditorGUILayout.Space(8);

            Section("2. The 5-line bootstrap (with code)");
            Code(@"using Rinval.MobileAdsAndIapKit;

await MobileKit.InitializeAsync(new KitConfig {
    AdConfig = myAdManagerConfig,
    IapCatalog = myIapCatalog,    // optional
    UseAppOpen = true,
});");
            EditorGUILayout.Space(8);

            Section("3. Most common calls");
            Code(@"// Show ads
AdManager.LoadInterstitial();
var result = await AdManager.ShowRewardedAsync(""level_complete"");
if (result.WasRewarded) GiveCoins(10);

// Banners
AdManager.LoadBanner(BannerAnchor.Bottom, BannerSize.Adaptive);
AdManager.ShowBanner();

// Buy a product
var purchase = await IapManager.PurchaseAsync(""coins_100"");
if (purchase.Successful) GiveCoins(100);

// Restore (Apple required)
await IapManager.RestorePurchasesAsync();");
            EditorGUILayout.Space(8);

            Section("4. No-code components (Add Component → Mobile Ads & IAP Kit)");
            Body("• IapBuyButton - buy a product when clicked\n" +
                 "• RestorePurchasesButton - Apple-required restore\n" +
                 "• RewardedAdButton - watch-ad-for-reward\n" +
                 "• InterstitialAdButton - show interstitial then continue\n" +
                 "• BannerToggleButton - show / hide / toggle banner\n" +
                 "• RateGameButton - store rate prompt\n" +
                 "• ConsentManageButton - re-open consent popup\n" +
                 "• CrossPromoButton - show one of your other apps\n" +
                 "• AdEventListener - drop on any object, react to global events");
            EditorGUILayout.Space(8);

            Section("5. Hooking events (code)");
            Code(@"AdManager.RevenuePaid += info => Debug.Log($""${info.Amount} from {info.NetworkName}"");
AdManager.RewardedClosed += code => { /* code is AdResultCode */ };
IapManager.Purchased += result => { /* result.Successful, result.ProductId */ };
IapManager.RefundDetected += pid => RevokeContent(pid);");
            EditorGUILayout.Space(8);

            Section("6. Editor tooling");
            Body("• Pre-flight Setup Wizard - full project audit + auto-fix on first install\n" +
                 "• Live Diagnostics - Play-mode timeline of every ad / IAP / consent / connectivity event\n" +
                 "• Run Health Check - pass/fail report on every subsystem\n" +
                 "• Placement Graph - visual map of placements, rules, last verdict\n" +
                 "• Recipes - pre-tuned configs (Hyper-Casual / Match-3 / Idle)\n" +
                 "• Capture Marketing Screenshots - auto-snapshots for the Asset Store listing");

            EditorGUILayout.Space(8);
            Section("7. Test mode & debugging");
            Body("• Editor: simulator overlay shows in Game view automatically\n" +
                 "• Press F2 in Play mode to toggle the runtime debug overlay\n" +
                 "• AdLogger.SetVerbose(true) for noisy logs\n" +
                 "• AdManager.LastShowVerdict - explicit reason for the most recent gate decision");
            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("Full reference: Documentation/QuickStart.md and API_Reference.md.", EditorStyles.miniLabel);
            EditorGUILayout.EndScrollView();
        }

        private static void Header(string txt)
        {
            EditorGUILayout.LabelField(txt, new GUIStyle(EditorStyles.boldLabel) { fontSize = 16 });
        }

        private static void Section(string txt)
        {
            EditorGUILayout.LabelField(txt, EditorStyles.boldLabel);
        }

        private static void Body(string txt)
        {
            EditorGUILayout.LabelField(txt, new GUIStyle(EditorStyles.label) { wordWrap = true });
        }

        private static void Code(string txt)
        {
            var style = new GUIStyle(EditorStyles.textArea) { font = EditorStyles.miniFont, wordWrap = false };
            EditorGUILayout.SelectableLabel(txt, style, GUILayout.Height(GetCodeHeight(txt)));
        }

        private static float GetCodeHeight(string txt)
        {
            int lines = 1;
            foreach (var c in txt) if (c == '\n') lines++;
            return lines * 14f + 12f;
        }
    }
}
