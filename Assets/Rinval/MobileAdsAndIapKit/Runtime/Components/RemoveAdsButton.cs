using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Drop-on-button "Remove Ads" trigger. Buys the remove-ads non-consumable and auto-disables itself once active. UnityEvents fire on success / fail.</summary>
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("Mobile Ads & IAP Kit/Remove Ads Button")]
    public class RemoveAdsButton : MonoBehaviour
    {
        [Tooltip("Product ID. Defaults to RemoveAdsManager.DefaultProductId (\"remove_ads\").")]
        [SerializeField] private string _productId = RemoveAdsManager.DefaultProductId;

        [Tooltip("Hide the button entirely once the user has removed ads.")]
        [SerializeField] private bool _hideWhenActive = true;

        [Header("Events")]
        public UnityEvent OnRemoved;
        public UnityEvent OnFailed;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(Buy);
            RemoveAdsManager.Activated += RefreshUi;
        }

        private void OnEnable() => RefreshUi();

        public void Buy()
        {
            if (RemoveAdsManager.IsActive) { RefreshUi(); return; }
            if (!IapManager.IsInitialized) { OnFailed?.Invoke(); return; }
            IapManager.Purchase(_productId, result =>
            {
                if (result.Successful) OnRemoved?.Invoke();
                else OnFailed?.Invoke();
                RefreshUi();
            });
        }

        public void RefreshUi()
        {
            if (RemoveAdsManager.IsActive)
            {
                if (_hideWhenActive) gameObject.SetActive(false);
                else if (_button != null) _button.interactable = false;
            }
            else
            {
                gameObject.SetActive(true);
                if (_button != null) _button.interactable = true;
            }
        }
    }
}
