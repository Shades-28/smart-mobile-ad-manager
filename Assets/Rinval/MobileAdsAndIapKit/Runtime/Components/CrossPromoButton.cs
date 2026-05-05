using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Drop-on-button cross-promo trigger. Picks an entry from the catalog and either opens its store URL directly (simplest case) or fires OnEntryReady so a custom popup can render it.</summary>
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("Mobile Ads & IAP Kit/Cross-Promo Button")]
    public class CrossPromoButton : MonoBehaviour
    {
        [SerializeField] private CrossPromoCatalog _catalog;

        [Tooltip("If true, immediately opens the picked entry's store URL when the button is clicked.")]
        [SerializeField] private bool _openStoreImmediately = true;

        [Header("Events")]
        public UnityEvent OnEntryShown;

        private Button _button;
        private CrossPromoEntry _lastPicked;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(Trigger);
            CrossPromoManager.OnEntryReady += entry =>
            {
                _lastPicked = entry;
                OnEntryShown?.Invoke();
                if (_openStoreImmediately) CrossPromoManager.OpenStore(entry);
            };
        }

        public void Trigger()
        {
            if (_catalog == null)
            {
                AdLogger.Warn("CrossPromoButton has no catalog assigned.");
                return;
            }
            CrossPromoManager.TryShow(_catalog);
        }

        public CrossPromoEntry LastPickedEntry => _lastPicked;
    }
}
