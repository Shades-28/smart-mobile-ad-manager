using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Drop-on-button rewarded-ad trigger. Type the placement key in the inspector, wire the reward callback in the OnRewarded UnityEvent. The button auto-disables when no rewarded ad is loaded, optionally hides itself entirely.</summary>
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("Mobile Ads & IAP Kit/Rewarded Ad Button")]
    public class RewardedAdButton : MonoBehaviour
    {
        [Tooltip("Placement key passed to AdManager.ShowRewarded (e.g. \"double_coins\").")]
        [SerializeField] private string _placement = "rewarded";

        [Tooltip("Hide the button entirely while no rewarded ad is ready to show.")]
        [SerializeField] private bool _hideWhenNotReady = false;

        [Tooltip("Disable (greyed out) the button while no rewarded ad is ready. Ignored if Hide is on.")]
        [SerializeField] private bool _disableWhenNotReady = true;

        [Tooltip("How often to refresh the button state, in seconds.")]
        [SerializeField] private float _refreshIntervalSeconds = 0.5f;

        [Header("Events")]
        public UnityEvent OnRewarded;
        public UnityEvent OnSkipped;
        public UnityEvent OnFailed;

        private Button _button;
        private float _nextRefresh;

        public string Placement
        {
            get => _placement;
            set => _placement = value;
        }

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(Watch);
        }

        private void OnEnable() => RefreshUi();

        private void Update()
        {
            if (Time.unscaledTime < _nextRefresh) return;
            _nextRefresh = Time.unscaledTime + _refreshIntervalSeconds;
            RefreshUi();
        }

        public void Watch()
        {
            if (!AdManager.IsInitialized)
            {
                AdLogger.Error("RewardedAdButton clicked before AdManager.Initialize");
                OnFailed?.Invoke();
                return;
            }
            AdManager.ShowRewarded(_placement, code =>
            {
                if (code == AdResultCode.Rewarded) OnRewarded?.Invoke();
                else if (code == AdResultCode.Cancelled || code == AdResultCode.Closed) OnSkipped?.Invoke();
                else OnFailed?.Invoke();
                RefreshUi();
            });
        }

        public void RefreshUi()
        {
            if (_button == null) _button = GetComponent<Button>();
            bool ready = AdManager.IsInitialized && AdManager.IsRewardedReady();

            if (_hideWhenNotReady)
            {
                gameObject.SetActive(ready);
                return;
            }
            if (_disableWhenNotReady) _button.interactable = ready;
        }
    }
}
