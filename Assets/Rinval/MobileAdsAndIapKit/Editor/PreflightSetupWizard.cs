using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Full pre-flight project audit. Inspects every dimension a buyer typically gets wrong on first install - Android Manifest, scripting defines, ad-unit IDs, GDPR/ATT config, icons, Unity version - then offers preview-then-confirm fixes for the auto-fixable ones.</summary>
    public class PreflightSetupWizard : EditorWindow
    {
        private const string AndroidManifestPath = "Assets/Plugins/Android/AndroidManifest.xml";
        private const string MinSupportedUnity = "2021.3";

        private AdManagerConfig _config;
        private string _admobAppIdAndroid = "";
        private string _admobAppIdIos = "";
        private string _attUserDescription = "We use this identifier to deliver personalized ads.";
        private List<Finding> _findings;
        private Vector2 _scroll;

        public enum Severity { Ok, Warning, Error }

        public class Finding
        {
            public string Title;
            public string Detail;
            public Severity Level;
            public bool AutoFixable;
            public System.Action<PreflightSetupWizard> Fix;
            public Finding(string t, string d, Severity s, bool fixable = false, System.Action<PreflightSetupWizard> fix = null)
            { Title = t; Detail = d; Level = s; AutoFixable = fixable; Fix = fix; }
        }

        [MenuItem("Rinval/Mobile Ads & IAP Kit/Pre-flight Setup Wizard", priority = 1)]
        public static void Open()
        {
            var w = GetWindow<PreflightSetupWizard>("Pre-flight Setup");
            w.minSize = new Vector2(720, 600);
            w.Show();
        }

        private void OnEnable()
        {
            if (_config == null)
            {
                var guids = AssetDatabase.FindAssets("t:AdManagerConfig");
                if (guids.Length > 0)
                    _config = AssetDatabase.LoadAssetAtPath<AdManagerConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            _admobAppIdAndroid = EditorPrefs.GetString("Rinval.MobileAdsIap.AdMobAppIdAndroid", "");
            _admobAppIdIos = EditorPrefs.GetString("Rinval.MobileAdsIap.AdMobAppIdIos", "");
            _attUserDescription = EditorPrefs.GetString("Rinval.MobileAdsIap.AttDescription", _attUserDescription);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Mobile Ads & IAP Kit - Pre-flight Setup", new GUIStyle(EditorStyles.boldLabel) { fontSize = 16 });
            EditorGUILayout.LabelField(
                "Audits every dimension a fresh project typically gets wrong on first install. " +
                "Click Run to scan; review the findings; auto-fix what can be fixed.",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(8);

            _config = (AdManagerConfig)EditorGUILayout.ObjectField("Ad Config", _config, typeof(AdManagerConfig), false);
            _admobAppIdAndroid = EditorGUILayout.TextField("AdMob App ID (Android)", _admobAppIdAndroid);
            _admobAppIdIos = EditorGUILayout.TextField("AdMob App ID (iOS)", _admobAppIdIos);
            _attUserDescription = EditorGUILayout.TextField("ATT Description (iOS)", _attUserDescription);

            EditorGUILayout.Space(12);
            if (GUILayout.Button("Run Pre-flight Check", GUILayout.Height(36)))
            {
                EditorPrefs.SetString("Rinval.MobileAdsIap.AdMobAppIdAndroid", _admobAppIdAndroid);
                EditorPrefs.SetString("Rinval.MobileAdsIap.AdMobAppIdIos", _admobAppIdIos);
                EditorPrefs.SetString("Rinval.MobileAdsIap.AttDescription", _attUserDescription);
                _findings = RunAudit();
            }

            if (_findings != null) DrawFindings();
        }

        private void DrawFindings()
        {
            EditorGUILayout.Space(12);
            int errors = 0, warnings = 0, oks = 0;
            foreach (var f in _findings)
            {
                if (f.Level == Severity.Error) errors++;
                else if (f.Level == Severity.Warning) warnings++;
                else oks++;
            }
            EditorGUILayout.LabelField(
                $"{errors} error · {warnings} warning · {oks} ok",
                new GUIStyle(EditorStyles.boldLabel) { fontSize = 13 });

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var f in _findings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                var color = GUI.backgroundColor;
                GUI.backgroundColor = ColorFor(f.Level);
                EditorGUILayout.LabelField($"{IconFor(f.Level)}  {f.Title}", EditorStyles.boldLabel);
                GUI.backgroundColor = color;
                EditorGUILayout.LabelField(f.Detail, EditorStyles.wordWrappedLabel);
                if (f.AutoFixable && f.Fix != null && f.Level != Severity.Ok)
                {
                    if (GUILayout.Button("Fix", GUILayout.Width(80)))
                    {
                        f.Fix(this);
                        _findings = RunAudit();
                        return;
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(4);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(8);
            using (new EditorGUI.DisabledScope(errors == 0 && warnings == 0))
            {
                var color = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
                if (GUILayout.Button($"Apply ALL Auto-fixes ({CountAutoFixable()})", GUILayout.Height(32)))
                {
                    if (EditorUtility.DisplayDialog("Apply Auto-fixes",
                        $"This will modify your project to fix {CountAutoFixable()} issue(s). Continue?",
                        "Apply", "Cancel"))
                    {
                        foreach (var f in _findings)
                            if (f.AutoFixable && f.Level != Severity.Ok && f.Fix != null) f.Fix(this);
                        AssetDatabase.Refresh();
                        _findings = RunAudit();
                    }
                }
                GUI.backgroundColor = color;
            }
        }

        private int CountAutoFixable()
        {
            int n = 0;
            if (_findings == null) return 0;
            foreach (var f in _findings) if (f.AutoFixable && f.Level != Severity.Ok) n++;
            return n;
        }

        private static Color ColorFor(Severity s) => s switch
        {
            Severity.Error => new Color(1f, 0.4f, 0.4f),
            Severity.Warning => new Color(1f, 0.85f, 0.3f),
            _ => new Color(0.4f, 0.85f, 0.4f),
        };
        private static string IconFor(Severity s) => s switch
        {
            Severity.Error => "[ERR]",
            Severity.Warning => "[WARN]",
            _ => "[OK]",
        };

        // - Audit checks -
        private List<Finding> RunAudit()
        {
            var findings = new List<Finding>();
            CheckUnityVersion(findings);
            CheckAdConfigPresent(findings);
            CheckAdUnitIds(findings);
            CheckScriptingDefines(findings);
            CheckAndroidManifest(findings);
            CheckAndroidIcons(findings);
            CheckGdprConsent(findings);
            CheckIapCatalog(findings);
            CheckPlacementCatalog(findings);
            CheckResourcesFolder(findings);
            return findings;
        }

        private void CheckUnityVersion(List<Finding> f)
        {
            var ver = Application.unityVersion;
            if (CompareVersion(ver, MinSupportedUnity) < 0)
                f.Add(new Finding("Unity version too old",
                    $"Unity {ver} is below the supported minimum {MinSupportedUnity}. Upgrade Unity.",
                    Severity.Error));
            else
                f.Add(new Finding("Unity version", $"{ver} is supported.", Severity.Ok));
        }

        private void CheckAdConfigPresent(List<Finding> f)
        {
            if (_config == null)
                f.Add(new Finding("AdManagerConfig missing",
                    "No AdManagerConfig assigned. Create one via Create → Rinval → Ads → Ad Manager Config and assign above.",
                    Severity.Error, true, w => w.CreateAdConfig()));
            else
                f.Add(new Finding("AdManagerConfig", $"Found at {AssetDatabase.GetAssetPath(_config)}.", Severity.Ok));
        }

        private void CheckAdUnitIds(List<Finding> f)
        {
            if (_config == null) return;
            var missing = new List<string>();
            if (string.IsNullOrEmpty(_config.GetBannerId())) missing.Add("banner");
            if (string.IsNullOrEmpty(_config.GetInterstitialId())) missing.Add("interstitial");
            if (string.IsNullOrEmpty(_config.GetRewardedId())) missing.Add("rewarded");
            if (missing.Count > 0)
                f.Add(new Finding("Ad unit IDs incomplete",
                    $"Missing for: {string.Join(", ", missing)}. Open the AdManagerConfig and paste your IDs from your mediator dashboard.",
                    Severity.Warning));
            else
                f.Add(new Finding("Ad unit IDs", "Banner / interstitial / rewarded all set.", Severity.Ok));
        }

        private void CheckScriptingDefines(List<Finding> f)
        {
            if (_config == null) return;
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            string expected = _config.Mediator switch
            {
                MediatorKind.AppLovinMax => "AD_USE_APPLOVIN",
                MediatorKind.GoogleAdMob => "AD_USE_ADMOB",
                MediatorKind.UnityLevelPlay => "AD_USE_LEVELPLAY",
                _ => null,
            };
            if (expected == null)
            {
                f.Add(new Finding("Mediator not selected", "Pick a mediator on the AdManagerConfig.", Severity.Warning));
                return;
            }
            if (!defines.Contains(expected))
                f.Add(new Finding("Scripting define missing",
                    $"Mediator is {_config.Mediator} but the {expected} scripting define is not set for {group}. The matching adapter won't compile.",
                    Severity.Error, true, w => w.SetDefine(expected)));
            else
                f.Add(new Finding("Scripting define", $"{expected} set for {group}.", Severity.Ok));
        }

        private void CheckAndroidManifest(List<Finding> f)
        {
            if (!File.Exists(AndroidManifestPath))
            {
                f.Add(new Finding("AndroidManifest.xml missing",
                    $"{AndroidManifestPath} does not exist. Required for AdMob app ID and permissions.",
                    Severity.Warning, true, w => w.CreateManifestSkeleton()));
                return;
            }
            var doc = new XmlDocument();
            try { doc.Load(AndroidManifestPath); }
            catch (System.Exception e) { f.Add(new Finding("AndroidManifest.xml unreadable", e.Message, Severity.Error)); return; }

            bool hasInternet = ManifestHasPermission(doc, "android.permission.INTERNET");
            bool hasNetState = ManifestHasPermission(doc, "android.permission.ACCESS_NETWORK_STATE");
            bool hasAdMobMeta = !string.IsNullOrEmpty(_admobAppIdAndroid) && ManifestHasMetaData(doc, "com.google.android.gms.ads.APPLICATION_ID");

            if (!hasInternet)
                f.Add(new Finding("Missing INTERNET permission",
                    "AndroidManifest.xml needs <uses-permission android:name=\"android.permission.INTERNET\"/>.",
                    Severity.Error, true, w => w.AddManifestPermission("android.permission.INTERNET")));
            else f.Add(new Finding("INTERNET permission", "set in manifest.", Severity.Ok));

            if (!hasNetState)
                f.Add(new Finding("Missing ACCESS_NETWORK_STATE permission",
                    "Recommended for connectivity checks.",
                    Severity.Warning, true, w => w.AddManifestPermission("android.permission.ACCESS_NETWORK_STATE")));

            if (_config != null && _config.Mediator == MediatorKind.GoogleAdMob && !hasAdMobMeta && !string.IsNullOrEmpty(_admobAppIdAndroid))
                f.Add(new Finding("AdMob APPLICATION_ID missing in manifest",
                    "AdMob will fail to initialize without this meta-data tag.",
                    Severity.Error, true, w => w.AddManifestAdMobAppId()));
        }

        private void CheckAndroidIcons(List<Finding> f)
        {
            var icons = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Android, IconKind.Application);
            bool hasAny = icons != null && icons.Length > 0;
            bool allEmpty = true;
            if (hasAny)
                foreach (var i in icons) if (i != null) { allEmpty = false; break; }
            if (!hasAny || allEmpty)
                f.Add(new Finding("App icon not set",
                    "Player Settings → Android → Icon: no icon is assigned. The app will ship with the default Unity icon.",
                    Severity.Warning));
            else
                f.Add(new Finding("App icon", "set in Player Settings.", Severity.Ok));
        }

        private void CheckGdprConsent(List<Finding> f)
        {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
            bool hasUmp = defines.Contains("RINVAL_UMP");
            if (!hasUmp)
                f.Add(new Finding("GDPR consent (UMP) not enabled",
                    "RINVAL_UMP scripting define is not set. Without UMP integration, GDPR users in the EEA may not see a consent form. Optional but recommended.",
                    Severity.Warning, true, w => w.SetDefine("RINVAL_UMP")));
            else
                f.Add(new Finding("GDPR / UMP", "RINVAL_UMP define set.", Severity.Ok));
        }

        private void CheckIapCatalog(List<Finding> f)
        {
            var guids = AssetDatabase.FindAssets("t:IapProductCatalog");
            if (guids.Length == 0)
                f.Add(new Finding("No IapProductCatalog found",
                    "If your game has IAP, create one via Create → Rinval → Mobile Ads & IAP → IAP Product Catalog. Skip if your game has no IAP.",
                    Severity.Warning));
            else
            {
                var catalog = AssetDatabase.LoadAssetAtPath<IapProductCatalog>(AssetDatabase.GUIDToAssetPath(guids[0]));
                if (catalog == null || catalog.Products.Count == 0)
                    f.Add(new Finding("IapProductCatalog empty",
                        $"{AssetDatabase.GUIDToAssetPath(guids[0])} has no products defined.",
                        Severity.Warning));
                else
                    f.Add(new Finding("IapProductCatalog", $"{catalog.Products.Count} products defined.", Severity.Ok));
            }
        }

        private void CheckPlacementCatalog(List<Finding> f)
        {
            var guids = AssetDatabase.FindAssets("t:PlacementCatalog");
            if (guids.Length == 0)
                f.Add(new Finding("No PlacementCatalog found",
                    "Optional. Authoring placement keys as a SO + generating PlacementIds.cs eliminates magic-string typos.",
                    Severity.Warning));
            else
                f.Add(new Finding("PlacementCatalog", "found.", Severity.Ok));
        }

        private void CheckResourcesFolder(List<Finding> f)
        {
            if (_config != null && AssetDatabase.GetAssetPath(_config).StartsWith("Assets/Resources/"))
                f.Add(new Finding("AdManagerConfig in Resources/", "Loadable via Resources.Load - good.", Severity.Ok));
            else if (_config != null)
                f.Add(new Finding("AdManagerConfig not in Resources/",
                    "Move to Assets/Resources/ if you want to load it via Resources.Load(\"AdManagerConfig\").",
                    Severity.Warning));
        }

        // - Fix actions -
        private void CreateAdConfig()
        {
            Directory.CreateDirectory("Assets/Resources");
            var asset = ScriptableObject.CreateInstance<AdManagerConfig>();
            AssetDatabase.CreateAsset(asset, "Assets/Resources/AdManagerConfig.asset");
            AssetDatabase.SaveAssets();
            _config = asset;
        }

        private void SetDefine(string symbol)
        {
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            var current = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            if (current.Contains(symbol)) return;
            current = string.IsNullOrEmpty(current) ? symbol : current + ";" + symbol;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, current);
        }

        private void CreateManifestSkeleton()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(AndroidManifestPath));
            const string skeleton =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<manifest xmlns:android=\"http://schemas.android.com/apk/res/android\">\n" +
                "  <application>\n" +
                "  </application>\n" +
                "</manifest>\n";
            File.WriteAllText(AndroidManifestPath, skeleton, new UTF8Encoding(false));
            AssetDatabase.Refresh();
        }

        private void AddManifestPermission(string permission)
        {
            var doc = new XmlDocument(); doc.Load(AndroidManifestPath);
            if (ManifestHasPermission(doc, permission)) return;
            var ns = "http://schemas.android.com/apk/res/android";
            var perm = doc.CreateElement("uses-permission");
            perm.SetAttribute("name", ns, permission);
            doc.DocumentElement.AppendChild(perm);
            SaveManifest(doc);
        }

        private void AddManifestAdMobAppId()
        {
            if (string.IsNullOrEmpty(_admobAppIdAndroid)) return;
            var doc = new XmlDocument(); doc.Load(AndroidManifestPath);
            if (ManifestHasMetaData(doc, "com.google.android.gms.ads.APPLICATION_ID")) return;
            var app = doc.GetElementsByTagName("application").Item(0) as XmlElement;
            if (app == null) return;
            var ns = "http://schemas.android.com/apk/res/android";
            var meta = doc.CreateElement("meta-data");
            meta.SetAttribute("name", ns, "com.google.android.gms.ads.APPLICATION_ID");
            meta.SetAttribute("value", ns, _admobAppIdAndroid);
            app.AppendChild(meta);
            SaveManifest(doc);
        }

        private static bool ManifestHasPermission(XmlDocument d, string name)
        {
            foreach (XmlNode n in d.GetElementsByTagName("uses-permission"))
                if (n.Attributes?["android:name"]?.Value == name) return true;
            return false;
        }

        private static bool ManifestHasMetaData(XmlDocument d, string name)
        {
            foreach (XmlNode n in d.GetElementsByTagName("meta-data"))
                if (n.Attributes?["android:name"]?.Value == name) return true;
            return false;
        }

        private static void SaveManifest(XmlDocument d)
        {
            var settings = new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false) };
            using (var w = XmlWriter.Create(AndroidManifestPath, settings)) d.Save(w);
        }

        private static int CompareVersion(string a, string b)
        {
            var pa = a.Split('.', 'f', 'a', 'b'); var pb = b.Split('.');
            int n = System.Math.Min(pa.Length, pb.Length);
            for (int i = 0; i < n; i++)
            {
                int.TryParse(pa[i], out int ai);
                int.TryParse(pb[i], out int bi);
                if (ai != bi) return ai - bi;
            }
            return 0;
        }
    }
}
