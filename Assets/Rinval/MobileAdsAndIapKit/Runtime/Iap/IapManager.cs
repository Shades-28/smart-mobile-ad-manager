using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Static facade for in-app purchases. Mirrors AdManager's static-API style. Adapter is pluggable; default implementation uses Unity IAP via UnityIapAdapter (when UNITY_PURCHASING is defined). EditorIapAdapter simulates buys for dev builds.</summary>
    public static class IapManager
    {
        private static IIapAdapter _adapter;
        private static IReceiptValidator _validator;
        private static readonly Dictionary<string, IapProduct> _products = new Dictionary<string, IapProduct>();

        public static event Action<PurchaseResult> Purchased;
        public static event Action<string> RefundDetected;        // productId
        public static event Action<string> SubscriptionActivated; // productId
        public static event Action<string> SubscriptionDeactivated;

        public static bool IsInitialized => _adapter != null && _adapter.IsInitialized;

        /// <summary>Initialize from a ScriptableObject catalog - preferred for indie use.</summary>
        public static void Initialize(IapProductCatalog catalog, IIapAdapter adapter = null,
            IReceiptValidator validator = null, Action<bool> onReady = null)
        {
            if (catalog == null)
            {
                AdLogger.Error("IapManager.Initialize: null catalog");
                onReady?.Invoke(false);
                return;
            }
            Initialize(catalog.AsList(), adapter, validator, onReady);
        }

        public static void Initialize(IList<IapProduct> products, IIapAdapter adapter = null,
            IReceiptValidator validator = null, Action<bool> onReady = null)
        {
            if (_adapter != null) { AdLogger.Warn("IapManager already initialized"); onReady?.Invoke(true); return; }
            if (products == null || products.Count == 0)
            {
                AdLogger.Error("IapManager.Initialize: empty product list");
                onReady?.Invoke(false);
                return;
            }
            _products.Clear();
            foreach (var p in products) if (p != null && !string.IsNullOrEmpty(p.ProductId)) _products[p.ProductId] = p;

            _adapter = adapter ?? new EditorIapAdapter();
            _validator = validator;
            _adapter.Initialize(products, ok =>
            {
                MainThreadDispatcher.Enqueue(() => onReady?.Invoke(ok));
            });
        }

        public static void SetReceiptValidator(IReceiptValidator validator) => _validator = validator;

        public static void Shutdown()
        {
            _adapter = null;
            _validator = null;
            _products.Clear();
            Purchased = null;
            RefundDetected = null;
            SubscriptionActivated = null;
            SubscriptionDeactivated = null;
        }

        public static bool TryGetProduct(string productId, out IapProduct product) =>
            _products.TryGetValue(productId, out product);

        public static decimal GetPrice(string productId) =>
            _adapter?.GetLocalizedPrice(productId) ?? 0m;

        public static string GetCurrency(string productId) =>
            _adapter?.GetLocalizedCurrencyCode(productId) ?? "USD";

        public static bool IsAvailable(string productId) =>
            _adapter != null && _adapter.IsAvailable(productId);

        public static bool HasNonConsumable(string productId) =>
            _adapter != null && _adapter.HasNonConsumable(productId);

        public static bool IsSubscriptionActive(string productId) =>
            _adapter != null && _adapter.IsSubscriptionActive(productId);

        /// <summary>Async purchase. Awaits the full result (validation included).</summary>
        public static Task<PurchaseResult> PurchaseAsync(string productId)
        {
            var tcs = new TaskCompletionSource<PurchaseResult>();
            Purchase(productId, result => tcs.TrySetResult(result));
            return tcs.Task;
        }

        /// <summary>Async restore. True = at least one purchase restored or no error.</summary>
        public static Task<bool> RestorePurchasesAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            RestorePurchases(ok => tcs.TrySetResult(ok));
            return tcs.Task;
        }

        public static void Purchase(string productId, Action<PurchaseResult> callback = null)
        {
            if (_adapter == null) { AdLogger.Error("IapManager not initialized"); callback?.Invoke(new PurchaseResult { ProductId = productId, Code = PurchaseResultCode.Unknown }); return; }
            _adapter.Purchase(productId, result =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    if (result.Successful && _validator != null)
                    {
                        _validator.Validate(result, granted =>
                        {
                            MainThreadDispatcher.Enqueue(() =>
                            {
                                if (!granted)
                                {
                                    AdLogger.Warn($"IAP receipt validation denied for {result.ProductId}");
                                    result.Code = PurchaseResultCode.SignatureInvalid;
                                }
                                FirePurchase(result);
                                callback?.Invoke(result);
                            });
                        });
                    }
                    else
                    {
                        FirePurchase(result);
                        callback?.Invoke(result);
                    }
                });
            });
        }

        public static void RestorePurchases(Action<bool> callback = null)
        {
            if (_adapter == null) { callback?.Invoke(false); return; }
            _adapter.RestorePurchases(ok => MainThreadDispatcher.Enqueue(() => callback?.Invoke(ok)));
        }

        // - Internal hooks for adapters -
        public static void RaiseRefundDetected(string productId)
        {
            try { RefundDetected?.Invoke(productId); }
            catch (Exception e) { AdLogger.Error($"IapManager.RefundDetected listener threw: {e}"); }
        }

        public static void RaiseSubscriptionActivated(string productId)
        {
            try { SubscriptionActivated?.Invoke(productId); }
            catch (Exception e) { AdLogger.Error($"IapManager.SubscriptionActivated listener threw: {e}"); }
        }

        public static void RaiseSubscriptionDeactivated(string productId)
        {
            try { SubscriptionDeactivated?.Invoke(productId); }
            catch (Exception e) { AdLogger.Error($"IapManager.SubscriptionDeactivated listener threw: {e}"); }
        }

        private static void FirePurchase(PurchaseResult result)
        {
            try { Purchased?.Invoke(result); }
            catch (Exception e) { AdLogger.Error($"IapManager.Purchased listener threw: {e}"); }
        }
    }
}
