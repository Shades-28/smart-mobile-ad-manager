using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Like the Unity Profiler, but for monetization. Captures every ad / IAP / consent / connectivity event with a timestamp and surfaces the active runtime state. Works in Play mode and Edit mode.</summary>
    public class LiveDiagnosticsWindow : EditorWindow
    {
        private struct Entry
        {
            public float TimeRealtime;
            public string Category;
            public string Detail;
            public Color Tint;
        }

        private static readonly List<Entry> _log = new List<Entry>();
        private const int MaxEntries = 500;
        private static bool _wired;

        private Vector2 _scroll;
        private string _filter = "";
        private bool _autoScroll = true;
        private readonly HashSet<string> _hiddenCategories = new HashSet<string>();

        [MenuItem("Rinval/Mobile Ads & IAP Kit/Live Diagnostics", priority = 2)]
        public static void Open()
        {
            var w = GetWindow<LiveDiagnosticsWindow>("Live Diagnostics");
            w.minSize = new Vector2(720, 480);
            w.Show();
            EnsureWired();
        }

        private static void EnsureWired()
        {
            if (_wired) return;
            _wired = true;
            AdManager.RevenuePaid += info => Append("Revenue", $"{info.Format} {info.NetworkName} {info.FormatLocalized()} placement={info.Placement}", new Color(0.4f, 0.85f, 0.4f));
            AdManager.InterstitialClosed += code => Append("Interstitial", $"closed: {code}", new Color(0.6f, 0.7f, 1f));
            AdManager.RewardedClosed += code => Append("Rewarded", $"closed: {code}", new Color(0.7f, 0.5f, 1f));
            AdManager.Loaded += fmt => Append("Load", $"{fmt} loaded", new Color(0.4f, 0.85f, 0.4f));
            AdManager.LoadFailed += fmt => Append("Load", $"{fmt} FAILED", new Color(1f, 0.5f, 0.5f));
            IapManager.Purchased += r => Append("IAP", $"{r.ProductId}: {r.Code} ({r.LocalizedPrice} {r.Currency})", r.Successful ? new Color(0.4f, 0.85f, 0.4f) : new Color(1f, 0.5f, 0.5f));
            IapManager.RefundDetected += pid => Append("IAP", $"REFUND for {pid}", new Color(1f, 0.5f, 0.5f));
            IapManager.SubscriptionActivated += pid => Append("IAP", $"sub activated: {pid}", new Color(0.4f, 0.85f, 0.4f));
            IapManager.SubscriptionDeactivated += pid => Append("IAP", $"sub deactivated: {pid}", new Color(1f, 0.7f, 0.4f));
            ConnectivityWatcher.OnlineChanged += online => Append("Net", online ? "ONLINE" : "OFFLINE", online ? new Color(0.4f, 0.85f, 0.4f) : new Color(1f, 0.5f, 0.5f));
            RemoveAdsManager.Activated += () => Append("RemoveAds", "activated", new Color(0.4f, 0.85f, 0.4f));
            RemoveAdsManager.Deactivated += () => Append("RemoveAds", "deactivated (refund)", new Color(1f, 0.5f, 0.5f));
        }

        private static void Append(string cat, string detail, Color tint)
        {
            _log.Add(new Entry { TimeRealtime = Time.realtimeSinceStartup, Category = cat, Detail = detail, Tint = tint });
            if (_log.Count > MaxEntries) _log.RemoveAt(0);
        }

        private void OnGUI()
        {
            EnsureWired();

            // Toolbar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(60))) _log.Clear();
            _autoScroll = GUILayout.Toggle(_autoScroll, "Auto-scroll", EditorStyles.toolbarButton, GUILayout.Width(90));
            GUILayout.Space(8);
            EditorGUILayout.LabelField("Filter:", GUILayout.Width(40));
            _filter = EditorGUILayout.TextField(_filter, EditorStyles.toolbarTextField, GUILayout.MinWidth(150));
            GUILayout.FlexibleSpace();
            DrawCategoryToggle("Revenue");
            DrawCategoryToggle("Interstitial");
            DrawCategoryToggle("Rewarded");
            DrawCategoryToggle("Load");
            DrawCategoryToggle("IAP");
            DrawCategoryToggle("Net");
            DrawCategoryToggle("RemoveAds");
            EditorGUILayout.EndHorizontal();

            // Runtime state strip
            DrawStateStrip();

            // Log
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            for (int i = 0; i < _log.Count; i++)
            {
                var e = _log[i];
                if (_hiddenCategories.Contains(e.Category)) continue;
                if (!string.IsNullOrEmpty(_filter) && e.Detail.IndexOf(_filter, System.StringComparison.OrdinalIgnoreCase) < 0
                    && e.Category.IndexOf(_filter, System.StringComparison.OrdinalIgnoreCase) < 0) continue;
                DrawEntry(e);
            }
            if (_autoScroll && Event.current.type == EventType.Repaint) _scroll.y = float.MaxValue;
            EditorGUILayout.EndScrollView();

            if (Application.isPlaying) Repaint();
        }

        private void DrawCategoryToggle(string cat)
        {
            bool on = !_hiddenCategories.Contains(cat);
            bool nv = GUILayout.Toggle(on, cat, EditorStyles.toolbarButton, GUILayout.Width(80));
            if (nv != on) { if (nv) _hiddenCategories.Remove(cat); else _hiddenCategories.Add(cat); }
        }

        private void DrawStateStrip()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Runtime State", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"AdManager initialized: {AdManager.IsInitialized}   |   Mediator: {AdManager.ActiveMediatorName}   |   Online: {ConnectivityGuard.IsOnline()}");
            EditorGUILayout.LabelField($"Interstitial ready: {SafeBool(() => AdManager.IsInterstitialReady())}   |   Rewarded ready: {SafeBool(() => AdManager.IsRewardedReady())}   |   Interstitials shown: {AdManager.InterstitialsShownTotal}");
            EditorGUILayout.LabelField($"IAP initialized: {IapManager.IsInitialized}   |   Remove ads: {RemoveAdsManager.IsActive}   |   Last verdict: {AdManager.LastShowVerdict}");
            EditorGUILayout.EndVertical();
        }

        private void DrawEntry(Entry e)
        {
            var color = GUI.backgroundColor;
            GUI.backgroundColor = e.Tint * 0.5f + Color.white * 0.5f;
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"{e.TimeRealtime:0.00}s", GUILayout.Width(60));
            EditorGUILayout.LabelField(e.Category, EditorStyles.boldLabel, GUILayout.Width(90));
            EditorGUILayout.LabelField(e.Detail);
            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = color;
        }

        private static string SafeBool(System.Func<bool> f)
        {
            try { return f().ToString(); } catch { return "n/a"; }
        }
    }
}
