#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Post-build hook that patches Info.plist with the AdMob App ID and ATT usage description. Driven by the values that AutoConfigWizard wrote to EditorPrefs.</summary>
    public static class IosPlistPostProcess
    {
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS) return;

            var plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            if (!File.Exists(plistPath))
            {
                Debug.LogWarning("[Rinval.MobileAdsIap] Info.plist not found; skipping iOS plist patch.");
                return;
            }
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            var admobId = EditorPrefs.GetString("Rinval.MobileAdsIap.AdMobAppIdIos", "");
            if (!string.IsNullOrEmpty(admobId))
                plist.root.SetString("GADApplicationIdentifier", admobId);

            var attDesc = EditorPrefs.GetString("Rinval.MobileAdsIap.AttDescription", "");
            if (!string.IsNullOrEmpty(attDesc))
                plist.root.SetString("NSUserTrackingUsageDescription", attDesc);

            plist.WriteToFile(plistPath);
            Debug.Log("[Rinval.MobileAdsIap] Info.plist patched with AdMob ID + ATT description.");
        }
    }
}
#endif
