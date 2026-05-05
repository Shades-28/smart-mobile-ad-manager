using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Drop-on-button "Restore Purchases" trigger. Required by Apple for any app with non-consumable or subscription products. No ID needed - restores everything the user previously bought.</summary>
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("Mobile Ads & IAP Kit/Restore Purchases Button")]
    public class RestorePurchasesButton : MonoBehaviour
    {
        [Tooltip("If true, button is disabled while the restore is in flight.")]
        [SerializeField] private bool _disableWhileRestoring = true;

        [Header("Events")]
        public UnityEvent OnRestoreSuccess;
        public UnityEvent OnRestoreFailed;

        private Button _button;
        private bool _inFlight;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(Restore);
        }

        public void Restore()
        {
            if (!IapManager.IsInitialized)
            {
                AdLogger.Error("RestorePurchasesButton clicked before IapManager.Initialize");
                OnRestoreFailed?.Invoke();
                return;
            }
            if (_inFlight) return;
            _inFlight = true;
            if (_disableWhileRestoring && _button != null) _button.interactable = false;

            IapManager.RestorePurchases(ok =>
            {
                _inFlight = false;
                if (_button != null) _button.interactable = true;
                if (ok) OnRestoreSuccess?.Invoke();
                else OnRestoreFailed?.Invoke();
            });
        }
    }
}
