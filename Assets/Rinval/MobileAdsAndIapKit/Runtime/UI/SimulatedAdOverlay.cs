using System;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    public class SimulatedAdOverlay : MonoBehaviour
    {
        private static SimulatedAdOverlay _instance;

        public static SimulatedAdOverlay GetOrCreate()
        {
            if (_instance != null) return _instance;
            var go = new GameObject("Rinval.MobileAdsIap.SimulatedAdOverlay");
            _instance = go.AddComponent<SimulatedAdOverlay>();
            DontDestroyOnLoad(go);
            return _instance;
        }

        public bool BannerActive { get; private set; }
        public bool BannerVisible { get; private set; }
        public bool MrecVisible { get; private set; }
        public BannerAnchor BannerAnchor { get; private set; } = BannerAnchor.Bottom;

        private bool _fullscreenActive;
        private string _fullscreenTitle;
        private string _fullscreenSubtitle;
        private bool _fullscreenIsRewarded;
        private float _fullscreenStartTime;
        private float _rewardedThresholdSeconds = 3f;
        private Action<AdResultCode> _fullscreenCallback;

        private GUIStyle _bannerStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _btnStyle;

        public void SetBanner(BannerAnchor anchor, bool active)
        {
            BannerAnchor = anchor;
            BannerActive = active;
            if (!active) BannerVisible = false;
        }

        public void SetBannerVisible(bool visible) => BannerVisible = visible && BannerActive;

        public void SetMrecVisible(bool visible) => MrecVisible = visible;

        public void ShowFullscreen(string title, string subtitle, bool isRewarded, Action<AdResultCode> onClose)
        {
            _fullscreenActive = true;
            _fullscreenTitle = title;
            _fullscreenSubtitle = subtitle;
            _fullscreenIsRewarded = isRewarded;
            _fullscreenStartTime = Time.unscaledTime;
            _fullscreenCallback = onClose;
        }

        public bool IsFullscreenActive => _fullscreenActive;

        private void OnGUI()
        {
            EnsureStyles();
            DrawBanner();
            DrawMrec();
            if (_fullscreenActive) DrawFullscreen();
        }

        private void EnsureStyles()
        {
            if (_bannerStyle != null) return;
            _bannerStyle = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter, fontSize = 18 };
            _bannerStyle.normal.textColor = Color.white;
            _bannerStyle.normal.background = MakeColorTex(new Color(0.20f, 0.30f, 0.55f, 0.95f));

            _titleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 30, fontStyle = FontStyle.Bold };
            _titleStyle.normal.textColor = Color.white;

            _bodyStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 18 };
            _bodyStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);

            _btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 18, fontStyle = FontStyle.Bold };
        }

        private static Texture2D MakeColorTex(Color c)
        {
            var t = new Texture2D(2, 2);
            var px = new Color[4]; for (int i = 0; i < 4; i++) px[i] = c;
            t.SetPixels(px); t.Apply();
            return t;
        }

        private void DrawBanner()
        {
            if (!BannerActive || !BannerVisible) return;
            const float w = 320f, h = 50f;
            float x, y;
            switch (BannerAnchor)
            {
                case BannerAnchor.Top:         x = (Screen.width - w) / 2f; y = 0f; break;
                case BannerAnchor.TopLeft:     x = 0f;                       y = 0f; break;
                case BannerAnchor.TopRight:    x = Screen.width - w;         y = 0f; break;
                case BannerAnchor.BottomLeft:  x = 0f;                       y = Screen.height - h; break;
                case BannerAnchor.BottomRight: x = Screen.width - w;         y = Screen.height - h; break;
                default:                       x = (Screen.width - w) / 2f; y = Screen.height - h; break;
            }
            GUI.Box(new Rect(x, y, w, h), "Simulated Banner - Rinval Ads", _bannerStyle);
        }

        private void DrawMrec()
        {
            if (!MrecVisible) return;
            const float w = 300f, h = 250f;
            float x = (Screen.width - w) / 2f;
            float y = (Screen.height - h) / 2f;
            GUI.Box(new Rect(x, y, w, h), "Simulated MREC\n300×250", _bannerStyle);
        }

        private void DrawFullscreen()
        {
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none, _bannerStyle);

            float cx = Screen.width / 2f;
            float cy = Screen.height / 2f;

            GUI.Label(new Rect(cx - 300, cy - 120, 600, 50), _fullscreenTitle, _titleStyle);
            GUI.Label(new Rect(cx - 300, cy - 70, 600, 30), _fullscreenSubtitle, _bodyStyle);

            float elapsed = Time.unscaledTime - _fullscreenStartTime;
            if (_fullscreenIsRewarded)
            {
                float remaining = Mathf.Max(0f, _rewardedThresholdSeconds - elapsed);
                GUI.Label(
                    new Rect(cx - 300, cy - 30, 600, 30),
                    remaining > 0f
                        ? $"Reward unlocks in {remaining:0.0}s"
                        : "Reward unlocked - close to claim",
                    _bodyStyle);

                if (remaining <= 0f && GUI.Button(new Rect(cx - 110, cy + 20, 220, 50), "Close (Earn Reward)", _btnStyle))
                    Close(AdResultCode.Rewarded);

                if (GUI.Button(new Rect(cx - 110, cy + 80, 220, 40), "Skip (No Reward)", _btnStyle))
                    Close(AdResultCode.Cancelled);
            }
            else
            {
                if (GUI.Button(new Rect(cx - 110, cy + 20, 220, 50), "Close", _btnStyle))
                    Close(AdResultCode.Closed);
            }
        }

        private void Close(AdResultCode result)
        {
            _fullscreenActive = false;
            var cb = _fullscreenCallback;
            _fullscreenCallback = null;
            cb?.Invoke(result);
        }

        public void ForceCloseFullscreen(AdResultCode result)
        {
            if (_fullscreenActive) Close(result);
        }
    }
}
