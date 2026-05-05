using UnityEngine;
using UnityEngine.Events;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Drop on an empty GameObject in your first scene, assign your AdManagerConfig and (optionally) IapProductCatalog, hit Play. The kit initializes itself before any other Awake runs in the scene.</summary>
    [DefaultExecutionOrder(-1000)]
    [AddComponentMenu("Mobile Ads & IAP Kit/Mobile Kit Bootstrap")]
    public class MobileKitBootstrap : MonoBehaviour
    {
        [Header("Required")]
        [Tooltip("Your AdManagerConfig asset. Create one via Assets → Create → Rinval → Ads → Ad Manager Config.")]
        [SerializeField] private AdManagerConfig _adConfig;

        [Header("Optional")]
        [Tooltip("IAP product catalog. Leave empty if your game has no IAP.")]
        [SerializeField] private IapProductCatalog _iapCatalog;

        [Tooltip("Kit-wide tuning knobs (connectivity poll, rate-game rules, etc.).")]
        [SerializeField] private MobileKitSettings _kitSettings;

        [Tooltip("Optional ConsentSettings asset. Auto-applied to a ConsentPopupController in the scene.")]
        [SerializeField] private ConsentSettings _consentSettings;

        [Tooltip("Optional A/B-test experiment catalog.")]
        [SerializeField] private AbExperimentCatalog _abCatalog;

        [Tooltip("Enable App Open ads (shown when the user returns to the app).")]
        [SerializeField] private bool _useAppOpen = false;

        [Tooltip("Start a ConnectivityWatcher that emits Online/Offline events.")]
        [SerializeField] private bool _useConnectivityWatcher = true;

        [Tooltip("Don't destroy this GameObject between scene loads.")]
        [SerializeField] private bool _persistAcrossScenes = true;

        [Tooltip("Show the consent popup automatically on first launch (if a ConsentPopupController is in the scene).")]
        [SerializeField] private bool _showConsentOnFirstLaunch = true;

        [Header("Events")]
        public UnityEvent OnKitReady;
        public UnityEvent OnKitInitFailed;

        private void Awake()
        {
            if (_persistAcrossScenes) DontDestroyOnLoad(gameObject);
            if (_adConfig == null)
            {
                AdLogger.Error("MobileKitBootstrap: no AdManagerConfig assigned in inspector");
                OnKitInitFailed?.Invoke();
                return;
            }

            MobileKit.Initialize(new KitConfig
            {
                AdConfig = _adConfig,
                IapCatalog = _iapCatalog,
                KitSettings = _kitSettings,
                AbCatalog = _abCatalog,
                UseAppOpen = _useAppOpen,
                UseConnectivityWatcher = _useConnectivityWatcher,
            }, ok =>
            {
                if (ok)
                {
                    var popup = FindObjectOfType<ConsentPopupController>(true);
                    if (popup != null)
                    {
                        // Prefer per-bootstrap setting; fall back to whatever's on KitSettings.
                        var settings = _consentSettings ?? _kitSettings?.Consent;
                        if (settings != null) popup.SetSettings(settings);
                        if (_showConsentOnFirstLaunch) popup.ShowIfNeeded();
                    }
                    RateGameManager.RecordAppOpen();
                    SessionTicker.EnsureRunning();
                    OnKitReady?.Invoke();
                }
                else OnKitInitFailed?.Invoke();
            });
        }
    }
}
