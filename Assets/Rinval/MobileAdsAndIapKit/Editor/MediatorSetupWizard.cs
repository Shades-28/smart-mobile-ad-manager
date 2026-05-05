using UnityEditor;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    public static class MediatorSetupWizard
    {
        private static readonly string[] ManagedDefines = {
            "AD_MONETIZATION", "AD_USE_APPLOVIN", "AD_USE_ADMOB", "AD_USE_LEVELPLAY"
        };

        public static void SetMediator(MediatorKind kind)
        {
            ApplyToGroup(BuildTargetGroup.Android, kind);
            ApplyToGroup(BuildTargetGroup.iOS, kind);
            ApplyToGroup(BuildTargetGroup.Standalone, kind);
            Debug.Log($"[Rinval.MobileAdsIap] Mediator set to {kind}. Restart Unity if compile errors persist.");
        }

        private static void ApplyToGroup(BuildTargetGroup group, MediatorKind kind)
        {
            var current = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            var defines = new System.Collections.Generic.List<string>(
                current.Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
            foreach (var d in ManagedDefines) defines.Remove(d);

            if (kind != MediatorKind.None)
            {
                defines.Add("AD_MONETIZATION");
                switch (kind)
                {
                    case MediatorKind.AppLovinMax: defines.Add("AD_USE_APPLOVIN"); break;
                    case MediatorKind.GoogleAdMob: defines.Add("AD_USE_ADMOB"); break;
                    case MediatorKind.UnityLevelPlay: defines.Add("AD_USE_LEVELPLAY"); break;
                }
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defines));
        }
    }
}
