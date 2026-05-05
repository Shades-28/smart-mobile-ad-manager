using System;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>First-class "remove ads" handling. Once active, all interstitial / banner / MREC / app-open calls become no-ops. Rewarded ads are still allowed (industry standard - rewarded is player-initiated value, not interruption advertising). Wired automatically when an IAP product matching the configured ProductId is purchased (default: "remove_ads"). Persisted in PlayerPrefs so it survives reinstalls? - no, only in-session; restore-purchases re-applies it via the IapManager.HasNonConsumable check.</summary>
    public static class RemoveAdsManager
    {
        private const string Key = "Rinval.MobileAdsIap.RemoveAdsActive";
        public const string DefaultProductId = "remove_ads";

        public static event Action Activated;
        public static event Action Deactivated;

        private static string _productId = DefaultProductId;
        private static bool _initialized;

        public static string ProductId => _productId;

        /// <summary>Returns true when ads have been removed via IAP. Source of truth is the IAP layer (HasNonConsumable) - PlayerPrefs is a cache for fast queries before IAP is initialized.</summary>
        public static bool IsActive
        {
            get
            {
                if (IapManager.IsInitialized) return IapManager.HasNonConsumable(_productId);
                return PlayerPrefs.GetInt(Key, 0) == 1;
            }
        }

        /// <summary>Wires events. Call once at app start (or use MobileKitBootstrap which does it).</summary>
        public static void Initialize(string productId = DefaultProductId)
        {
            if (_initialized) return;
            _productId = string.IsNullOrEmpty(productId) ? DefaultProductId : productId;
            _initialized = true;

            IapManager.Purchased += OnPurchased;
            IapManager.RefundDetected += OnRefund;

            // If the IAP layer is already initialized and the product is owned, sync PlayerPrefs.
            if (IapManager.IsInitialized && IapManager.HasNonConsumable(_productId))
                PlayerPrefs.SetInt(Key, 1);
        }

        public static void Shutdown()
        {
            IapManager.Purchased -= OnPurchased;
            IapManager.RefundDetected -= OnRefund;
            Activated = null;
            Deactivated = null;
            _initialized = false;
        }

        private static void OnPurchased(PurchaseResult r)
        {
            if (!r.Successful || r.ProductId != _productId) return;
            PlayerPrefs.SetInt(Key, 1);
            PlayerPrefs.Save();
            try { Activated?.Invoke(); }
            catch (Exception e) { AdLogger.Error($"RemoveAdsManager.Activated listener threw: {e}"); }
        }

        private static void OnRefund(string productId)
        {
            if (productId != _productId) return;
            PlayerPrefs.SetInt(Key, 0);
            PlayerPrefs.Save();
            try { Deactivated?.Invoke(); }
            catch (Exception e) { AdLogger.Error($"RemoveAdsManager.Deactivated listener threw: {e}"); }
        }

        public static void ResetForTests()
        {
            PlayerPrefs.DeleteKey(Key);
            _initialized = false;
            Activated = null;
            Deactivated = null;
        }
    }
}
