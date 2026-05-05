using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Designer-authored copy + URLs for the Consent popup. Drag onto ConsentPopupController to override the defaults. Lets non-coders localize and tweak the popup without touching the prefab or C#.</summary>
    [CreateAssetMenu(
        fileName = "ConsentSettings",
        menuName = "Rinval/Mobile Ads & IAP/Consent Settings",
        order = 150)]
    public class ConsentSettings : ScriptableObject
    {
        [Header("Copy")]
        [SerializeField] private string _title = "Privacy & Personalization";
        [TextArea, SerializeField] private string _body =
            "We use ads to keep this game free.\n\n" +
            "Tap Accept to allow personalized ads, or Decline for non-personalized ads only. " +
            "You can change this anytime in Settings.";

        [Header("Buttons")]
        [SerializeField] private string _acceptLabel = "Accept";
        [SerializeField] private string _declineLabel = "Decline";
        [SerializeField] private string _privacyPolicyLabel = "Privacy Policy";

        [Header("URLs")]
        [SerializeField] private string _privacyPolicyUrl = "https://example.com/privacy";

        public string Title => _title;
        public string Body => _body;
        public string AcceptLabel => _acceptLabel;
        public string DeclineLabel => _declineLabel;
        public string PrivacyPolicyLabel => _privacyPolicyLabel;
        public string PrivacyPolicyUrl => _privacyPolicyUrl;
    }
}
