using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Drop-on-button interstitial trigger. Useful for "Continue" or "Next Level" buttons where you want to show an interstitial before proceeding. UnityEvents fire AFTER the ad closes.</summary>
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("Mobile Ads & IAP Kit/Interstitial Ad Button")]
    public class InterstitialAdButton : MonoBehaviour
    {
        [Tooltip("Placement key passed to AdManager.ShowInterstitial (e.g. \"level_complete\").")]
        [SerializeField] private string _placement = "default";

        [Tooltip("If true, the OnContinue event fires even if the ad was gated by frequency caps.")]
        [SerializeField] private bool _continueIfGated = true;

        [Header("Events (fire after ad closes - or immediately if gated and ContinueIfGated)")]
        public UnityEvent OnContinue;
        public UnityEvent OnAdShown;

        private Button _button;

        public string Placement
        {
            get => _placement;
            set => _placement = value;
        }

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(Trigger);
        }

        public void Trigger()
        {
            if (!AdManager.IsInitialized)
            {
                AdLogger.Warn("InterstitialAdButton clicked before AdManager.Initialize - continuing anyway");
                OnContinue?.Invoke();
                return;
            }
            bool attempted = AdManager.ShowInterstitial(_placement, code =>
            {
                if (code == AdResultCode.Closed || code == AdResultCode.Shown) OnAdShown?.Invoke();
                OnContinue?.Invoke();
            });
            if (!attempted && _continueIfGated)
            {
                // Gated by frequency caps or not initialized - let game flow continue.
                OnContinue?.Invoke();
            }
        }
    }
}
