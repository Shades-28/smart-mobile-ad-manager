using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Kit-wide knobs that don't fit on AdManagerConfig (which is ads-only) or any single subsystem SO. Drag onto MobileKitBootstrap and the kit will apply them at startup.</summary>
    [CreateAssetMenu(
        fileName = "MobileKitSettings",
        menuName = "Rinval/Mobile Ads & IAP/Mobile Kit Settings",
        order = 100)]
    public class MobileKitSettings : ScriptableObject
    {
        [Header("Connectivity")]
        [Tooltip("ConnectivityWatcher poll interval in seconds. Lower = faster reaction, higher = less work.")]
        [Min(0.5f)][SerializeField] private float _connectivityPollSeconds = 2f;

        [Header("Remove Ads")]
        [Tooltip("Product ID treated as the 'remove ads' non-consumable. Default: \"remove_ads\".")]
        [SerializeField] private string _removeAdsProductId = RemoveAdsManager.DefaultProductId;

        [Header("Consent")]
        [Tooltip("Optional ConsentSettings asset; passed to the ConsentPopupController found in the scene.")]
        [SerializeField] private ConsentSettings _consent;

        [Header("Rate Game")]
        [Tooltip("Optional RateGameSettings asset; applied to RateGameManager at startup.")]
        [SerializeField] private RateGameSettings _rate;

        [Header("Auto-Initialize")]
        [Tooltip("If true, MobileKit auto-applies these settings during Initialize.")]
        [SerializeField] private bool _autoApply = true;

        public float ConnectivityPollSeconds => _connectivityPollSeconds;
        public string RemoveAdsProductId => string.IsNullOrEmpty(_removeAdsProductId) ? RemoveAdsManager.DefaultProductId : _removeAdsProductId;
        public ConsentSettings Consent => _consent;
        public RateGameSettings Rate => _rate;
        public bool AutoApply => _autoApply;

        /// <summary>Apply this settings asset to the active subsystems.</summary>
        public void Apply()
        {
            ConnectivityWatcher.SetPollInterval(_connectivityPollSeconds);
            RateGameManager.Configure(_rate);
            // Consent is applied to the popup controller by MobileKitBootstrap when found in scene.
        }
    }
}
