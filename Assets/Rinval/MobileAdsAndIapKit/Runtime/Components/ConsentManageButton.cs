using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Drop-on-button "Manage Privacy / Consent" trigger. Re-opens the consent popup so users can change their GDPR / personalization choice. Required for AdMob policy compliance - every game with personalized ads must surface a way to revisit consent.</summary>
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("Mobile Ads & IAP Kit/Consent Manage Button")]
    public class ConsentManageButton : MonoBehaviour
    {
        [Tooltip("Optional reference to a ConsentPopupController in the scene. If null, a search is performed.")]
        [SerializeField] private ConsentPopupController _popup;

        [Header("Events")]
        public UnityEvent OnPopupOpened;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(Trigger);
        }

        public void Trigger()
        {
            if (_popup == null) _popup = FindObjectOfType<ConsentPopupController>(true);
            if (_popup == null)
            {
                AdLogger.Warn("ConsentManageButton: no ConsentPopupController found in scene");
                return;
            }
            _popup.Show();
            OnPopupOpened?.Invoke();
        }
    }
}
