using System;
using UnityEngine;
using UnityEngine.UI;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Drop-in GDPR / ATT consent popup. Wire the buttons + texts in the prefab inspector and call Show() at app start. ConsentManager + AttHelper are updated automatically.</summary>
    public class ConsentPopupController : MonoBehaviour
    {
        [Header("UI Bindings")]
        public GameObject panel;
        public Text titleText;
        public Text bodyText;
        public Text acceptButtonLabel;
        public Text declineButtonLabel;
        public Text privacyPolicyButtonLabel;
        public Button acceptButton;
        public Button denyButton;
        public Button privacyPolicyButton;

        [Header("Settings (preferred - overrides inline copy)")]
        [Tooltip("ConsentSettings asset. If set, its values override the inline copy below.")]
        [SerializeField] private ConsentSettings _settings;

        [Header("Inline Copy (fallback when no Settings asset is assigned)")]
        [SerializeField, TextArea] private string _title = "Privacy & Personalization";
        [SerializeField, TextArea] private string _body = "We use ads to keep this game free. Tap Accept to allow personalized ads, or Decline for non-personalized ads only. You can change this anytime in Settings.";
        [SerializeField] private string _privacyPolicyUrl = "https://example.com/privacy";

        public event Action<ConsentStatus> Resolved;

        private void Awake()
        {
            ApplyCopy();
            if (acceptButton != null) acceptButton.onClick.AddListener(OnAccept);
            if (denyButton != null) denyButton.onClick.AddListener(OnDeny);
            if (privacyPolicyButton != null) privacyPolicyButton.onClick.AddListener(OnPrivacy);
            HidePanel();
        }

        private void ApplyCopy()
        {
            string title = _settings != null ? _settings.Title : _title;
            string body = _settings != null ? _settings.Body : _body;
            string acceptLabel = _settings != null ? _settings.AcceptLabel : null;
            string declineLabel = _settings != null ? _settings.DeclineLabel : null;
            string policyLabel = _settings != null ? _settings.PrivacyPolicyLabel : null;

            if (titleText != null) titleText.text = title;
            if (bodyText != null) bodyText.text = body;
            if (acceptButtonLabel != null && !string.IsNullOrEmpty(acceptLabel)) acceptButtonLabel.text = acceptLabel;
            if (declineButtonLabel != null && !string.IsNullOrEmpty(declineLabel)) declineButtonLabel.text = declineLabel;
            if (privacyPolicyButtonLabel != null && !string.IsNullOrEmpty(policyLabel)) privacyPolicyButtonLabel.text = policyLabel;
        }

        public void SetSettings(ConsentSettings settings)
        {
            _settings = settings;
            ApplyCopy();
        }

        public void Show()
        {
            if (panel != null) panel.SetActive(true);
            else gameObject.SetActive(true);
        }

        public bool ShowIfNeeded()
        {
            if (ConsentManager.GdprStatus != ConsentStatus.Unknown) return false;
            Show();
            return true;
        }

        private void OnAccept()
        {
            ConsentManager.Grant();
            HidePanel();
            Resolved?.Invoke(ConsentStatus.Obtained);
        }

        private void OnDeny()
        {
            ConsentManager.Deny();
            HidePanel();
            Resolved?.Invoke(ConsentStatus.Denied);
        }

        private void OnPrivacy()
        {
            string url = _settings != null ? _settings.PrivacyPolicyUrl : _privacyPolicyUrl;
            if (!string.IsNullOrEmpty(url)) Application.OpenURL(url);
        }

        private void HidePanel()
        {
            if (panel != null) panel.SetActive(false);
        }
    }
}
