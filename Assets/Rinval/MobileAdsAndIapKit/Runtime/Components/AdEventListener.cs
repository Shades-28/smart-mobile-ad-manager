using UnityEngine;
using UnityEngine.Events;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Drop on any GameObject to react to AdManager / IapManager events without writing code. Each UnityEvent is wired to one of the kit's static events; assign the methods you want to call in the inspector.</summary>
    [AddComponentMenu("Mobile Ads & IAP Kit/Ad Event Listener")]
    public class AdEventListener : MonoBehaviour
    {
        [Header("Ad Events")]
        public UnityEvent OnInterstitialClosed;
        public UnityEvent OnRewardedClosed;
        public UnityEvent OnRewardEarned;
        public UnityEvent OnAdLoaded;
        public UnityEvent OnAdLoadFailed;

        [Header("IAP Events")]
        public UnityEvent OnAnyPurchaseSuccess;
        public UnityEvent OnAnyPurchaseFailed;
        public UnityEvent OnRefundDetected;
        public UnityEvent OnSubscriptionActivated;
        public UnityEvent OnSubscriptionDeactivated;

        [Header("Lifecycle")]
        public UnityEvent OnConnectivityOnline;
        public UnityEvent OnConnectivityOffline;

        private void OnEnable()
        {
            AdManager.InterstitialClosed += HandleInterstitialClosed;
            AdManager.RewardedClosed += HandleRewardedClosed;
            AdManager.Loaded += HandleLoaded;
            AdManager.LoadFailed += HandleLoadFailed;

            IapManager.Purchased += HandlePurchased;
            IapManager.RefundDetected += HandleRefund;
            IapManager.SubscriptionActivated += HandleSubActivated;
            IapManager.SubscriptionDeactivated += HandleSubDeactivated;

            ConnectivityWatcher.OnlineChanged += HandleConnectivity;
            ConnectivityWatcher.EnsureRunning();
        }

        private void OnDisable()
        {
            AdManager.InterstitialClosed -= HandleInterstitialClosed;
            AdManager.RewardedClosed -= HandleRewardedClosed;
            AdManager.Loaded -= HandleLoaded;
            AdManager.LoadFailed -= HandleLoadFailed;

            IapManager.Purchased -= HandlePurchased;
            IapManager.RefundDetected -= HandleRefund;
            IapManager.SubscriptionActivated -= HandleSubActivated;
            IapManager.SubscriptionDeactivated -= HandleSubDeactivated;

            ConnectivityWatcher.OnlineChanged -= HandleConnectivity;
        }

        private void HandleInterstitialClosed(AdResultCode code) => OnInterstitialClosed?.Invoke();
        private void HandleRewardedClosed(AdResultCode code)
        {
            OnRewardedClosed?.Invoke();
            if (code == AdResultCode.Rewarded) OnRewardEarned?.Invoke();
        }
        private void HandleLoaded(AdFormat _) => OnAdLoaded?.Invoke();
        private void HandleLoadFailed(AdFormat _) => OnAdLoadFailed?.Invoke();

        private void HandlePurchased(PurchaseResult r)
        {
            if (r.Successful) OnAnyPurchaseSuccess?.Invoke();
            else OnAnyPurchaseFailed?.Invoke();
        }
        private void HandleRefund(string _) => OnRefundDetected?.Invoke();
        private void HandleSubActivated(string _) => OnSubscriptionActivated?.Invoke();
        private void HandleSubDeactivated(string _) => OnSubscriptionDeactivated?.Invoke();

        private void HandleConnectivity(bool online)
        {
            if (online) OnConnectivityOnline?.Invoke();
            else OnConnectivityOffline?.Invoke();
        }
    }
}
