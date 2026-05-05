#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Runtime debug overlay for ad-state inspection. Dev/editor builds only. Activated by 4-finger tap (mobile) or pressing F2 (editor/standalone).</summary>
    public class AdDebugOverlay : MonoBehaviour
    {
        private static AdDebugOverlay _instance;
        private bool _visible;
        private Vector2 _scroll;
        private readonly List<string> _eventLog = new List<string>();
        private const int LogCap = 30;

        public static void EnsureRunning()
        {
            if (_instance != null) return;
            var go = new GameObject("Rinval.MobileAdsIap.DebugOverlay");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<AdDebugOverlay>();
        }

        private void Awake()
        {
            AdManager.Loaded += fmt => Append($"loaded: {fmt}");
            AdManager.LoadFailed += fmt => Append($"load-failed: {fmt}");
            AdManager.RevenuePaid += info => Append($"revenue: {info.Format} ${info.Amount:0.000000}");
            AdManager.InterstitialClosed += code => Append($"interstitial closed: {code}");
            AdManager.RewardedClosed += code => Append($"rewarded closed: {code}");
        }

        private void Update()
        {
            // 4-finger tap toggles visibility
            if (Input.touchCount == 4 && Input.touches[0].phase == TouchPhase.Began)
                _visible = !_visible;
            // Editor / standalone: F2 toggles
            if (Input.GetKeyDown(KeyCode.F2)) _visible = !_visible;
        }

        private void OnGUI()
        {
            if (!_visible) return;
            const float W = 420f, H = 480f;
            float x = Screen.width - W - 20f;
            float y = 20f;
            GUI.color = new Color(0, 0, 0, 0.85f);
            GUI.DrawTexture(new Rect(x, y, W, H), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUILayout.BeginArea(new Rect(x + 10, y + 10, W - 20, H - 20));
            GUILayout.Label("Mobile Ads & IAP Kit - Debug", BoldStyle());
            GUILayout.Label($"Initialized: {AdManager.IsInitialized}");
            GUILayout.Label($"Active Mediator: {AdManager.ActiveMediatorName}");
            GUILayout.Label($"Interstitials shown: {AdManager.InterstitialsShownTotal}");
            GUILayout.Label($"Online: {ConnectivityGuard.IsOnline()}");
            GUILayout.Space(8);
            GUILayout.Label("Event log:", BoldStyle());
            _scroll = GUILayout.BeginScrollView(_scroll);
            for (int i = _eventLog.Count - 1; i >= 0; i--)
                GUILayout.Label(_eventLog[i]);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void Append(string line)
        {
            _eventLog.Add($"[{Time.realtimeSinceStartup:0.0}] {line}");
            if (_eventLog.Count > LogCap) _eventLog.RemoveAt(0);
        }

        private static GUIStyle _bold;
        private static GUIStyle BoldStyle()
        {
            if (_bold == null)
            {
                _bold = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            }
            return _bold;
        }
    }
}
#endif
