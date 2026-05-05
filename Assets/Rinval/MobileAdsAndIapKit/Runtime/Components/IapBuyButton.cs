using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Drop-on-button IAP purchase trigger. Type the product ID in the inspector, wire UnityEvents for success / failure / cancel, and the button works without any code.</summary>
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("Mobile Ads & IAP Kit/IAP Buy Button")]
    public class IapBuyButton : MonoBehaviour
    {
        [Tooltip("Product ID exactly as registered in IapProductCatalog and the store consoles.")]
        [SerializeField] private string _productId;

        [Tooltip("If true, button is disabled while the purchase is in flight to prevent double-taps.")]
        [SerializeField] private bool _disableWhileBuying = true;

        [Tooltip("If true and product is a non-consumable already owned, the button is disabled at runtime.")]
        [SerializeField] private bool _disableIfOwned = true;

        [Tooltip("Optional Text/TMP_Text component to show the localized price (e.g. \"$0.99\").")]
        [SerializeField] private Text _priceLabel;

        [Header("Events")]
        public UnityEvent OnPurchaseSuccess;
        public UnityEvent OnPurchaseCancelled;
        public UnityEvent OnPurchaseFailed;

        private Button _button;
        private bool _purchaseInFlight;

        public string ProductId
        {
            get => _productId;
            set => _productId = value;
        }

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(Buy);
        }

        private void Start() => RefreshUi();

        private void OnEnable() => RefreshUi();

        public void Buy()
        {
            if (string.IsNullOrEmpty(_productId))
            {
                AdLogger.Error($"IapBuyButton on '{name}' has no product ID set.");
                OnPurchaseFailed?.Invoke();
                return;
            }
            if (!IapManager.IsInitialized)
            {
                AdLogger.Error("IapBuyButton clicked before IapManager.Initialize");
                OnPurchaseFailed?.Invoke();
                return;
            }
            if (_purchaseInFlight) return;
            _purchaseInFlight = true;
            if (_disableWhileBuying && _button != null) _button.interactable = false;

            IapManager.Purchase(_productId, result =>
            {
                _purchaseInFlight = false;
                RefreshUi();
                switch (result.Code)
                {
                    case PurchaseResultCode.Success:
                        OnPurchaseSuccess?.Invoke();
                        break;
                    case PurchaseResultCode.UserCancelled:
                        OnPurchaseCancelled?.Invoke();
                        break;
                    default:
                        OnPurchaseFailed?.Invoke();
                        break;
                }
            });
        }

        public void RefreshUi()
        {
            if (_button == null) _button = GetComponent<Button>();
            bool ownedNonCons = _disableIfOwned && IapManager.IsInitialized && IapManager.HasNonConsumable(_productId);
            _button.interactable = !_purchaseInFlight && !ownedNonCons;

            if (_priceLabel != null && IapManager.IsInitialized)
            {
                var price = IapManager.GetPrice(_productId);
                var ccy = IapManager.GetCurrency(_productId);
                if (price > 0m) _priceLabel.text = $"{price:0.00} {ccy}";
            }
        }
    }
}
