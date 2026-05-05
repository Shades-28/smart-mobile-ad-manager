using UnityEditor;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    public class AdManagerDashboard : EditorWindow
    {
        private AdManagerConfig _config;
        private Vector2 _scroll;

        [MenuItem("Rinval/Mobile Ads & IAP Kit/Ad Manager Dashboard", priority = 0)]
        public static void Open()
        {
            var w = GetWindow<AdManagerDashboard>("Ad Manager");
            w.minSize = new Vector2(420, 540);
            w.Show();
        }

        private void OnEnable()
        {
            if (_config == null)
                _config = AssetDatabase.FindAssets("t:AdManagerConfig").Length > 0
                    ? AssetDatabase.LoadAssetAtPath<AdManagerConfig>(
                        AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:AdManagerConfig")[0]))
                    : null;
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawHeader();
            DrawConfigSection();
            DrawDefinesSection();
            EditorGUILayout.Space(10);
            DrawRuntimeSection();
            EditorGUILayout.Space(10);
            DrawTestSection();
            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Mobile Ads & IAP Kit", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("by Rinval Games", EditorStyles.miniLabel);
            EditorGUILayout.Space(8);
        }

        private void DrawConfigSection()
        {
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
            _config = (AdManagerConfig)EditorGUILayout.ObjectField("Config Asset", _config, typeof(AdManagerConfig), false);
            if (_config == null)
            {
                EditorGUILayout.HelpBox("No AdManagerConfig found. Click 'Create Config' below.", MessageType.Warning);
                if (GUILayout.Button("Create AdManagerConfig"))
                {
                    var asset = ScriptableObject.CreateInstance<AdManagerConfig>();
                    AssetDatabase.CreateFolder("Assets", "Resources");
                    AssetDatabase.CreateAsset(asset, "Assets/Resources/AdManagerConfig.asset");
                    AssetDatabase.SaveAssets();
                    _config = asset;
                }
                return;
            }
            if (GUILayout.Button("Validate Configuration"))
                ConfigValidator.Validate(_config);
        }

        private void DrawDefinesSection()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Active Mediator Define", EditorStyles.boldLabel);
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            EditorGUILayout.LabelField("Build Target", group.ToString());
            EditorGUILayout.LabelField("Defines", defines, EditorStyles.wordWrappedLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Use AppLovin")) MediatorSetupWizard.SetMediator(MediatorKind.AppLovinMax);
            if (GUILayout.Button("Use AdMob")) MediatorSetupWizard.SetMediator(MediatorKind.GoogleAdMob);
            if (GUILayout.Button("Use LevelPlay")) MediatorSetupWizard.SetMediator(MediatorKind.UnityLevelPlay);
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Disable Ads (clear all)")) MediatorSetupWizard.SetMediator(MediatorKind.None);
        }

        private void DrawRuntimeSection()
        {
            EditorGUILayout.LabelField("Runtime State", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Initialized", AdManager.IsInitialized.ToString());
            EditorGUILayout.LabelField("Active Mediator", AdManager.ActiveMediatorName);
            EditorGUILayout.LabelField("Interstitials shown", AdManager.InterstitialsShownTotal.ToString());
        }

        private void DrawTestSection()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use the test buttons.", MessageType.Info);
                return;
            }
            EditorGUILayout.LabelField("Live Test Buttons", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Banner");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Top")) AdManager.LoadBanner(BannerAnchor.Top);
            if (GUILayout.Button("Load Bottom")) AdManager.LoadBanner(BannerAnchor.Bottom);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Show")) AdManager.ShowBanner();
            if (GUILayout.Button("Hide")) AdManager.HideBanner();
            if (GUILayout.Button("Destroy")) AdManager.DestroyBanner();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Interstitial");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Load")) AdManager.LoadInterstitial();
            if (GUILayout.Button("Show (Closed)"))
            {
                EditorAdapter.NextInterstitialResult = AdResultCode.Closed;
                AdManager.ShowInterstitial("editor_dashboard");
            }
            if (GUILayout.Button("Show (Failed)"))
            {
                EditorAdapter.NextInterstitialResult = AdResultCode.Failed;
                AdManager.ShowInterstitial("editor_dashboard");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Rewarded");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Load")) AdManager.LoadRewarded();
            if (GUILayout.Button("Show (Reward)"))
            {
                EditorAdapter.NextRewardedResult = AdResultCode.Rewarded;
                AdManager.ShowRewarded("editor_dashboard");
            }
            if (GUILayout.Button("Show (Cancel)"))
            {
                EditorAdapter.NextRewardedResult = AdResultCode.Cancelled;
                AdManager.ShowRewarded("editor_dashboard");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("MREC");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Load")) AdManager.LoadMrec();
            if (GUILayout.Button("Show")) AdManager.ShowMrec();
            if (GUILayout.Button("Hide")) AdManager.HideMrec();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Failure simulation");
            EditorAdapter.NextLoadFails = EditorGUILayout.Toggle("Force next load to fail", EditorAdapter.NextLoadFails);
        }
    }
}
