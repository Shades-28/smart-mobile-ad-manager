#if UNITY_PURCHASING
using System;
using System.Collections.Generic;
using UnityEngine.Purchasing;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Production IAP adapter backed by Unity IAP (UnityEngine.Purchasing). Compiles only when UNITY_PURCHASING is defined (i.e. com.unity.purchasing is installed).</summary>
    public class UnityIapAdapter : IIapAdapter, IStoreListener
    {
        public bool IsInitialized { get; private set; }

        private IStoreController _store;
        private IExtensionProvider _extensions;
        private Action<bool> _onReady;
        private Action<PurchaseResult> _pendingPurchaseCallback;
        private Action<bool> _restoreCallback;
        private readonly Dictionary<string, IapProduct> _products = new Dictionary<string, IapProduct>();

        public void Initialize(IList<IapProduct> products, Action<bool> onReady)
        {
            try
            {
                _products.Clear();
                _onReady = onReady;
                var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
                foreach (var p in products)
                {
                    if (p == null || string.IsNullOrEmpty(p.ProductId)) continue;
                    _products[p.ProductId] = p;
                    builder.AddProduct(p.ProductId, ToUnityType(p.Kind),
                        new IDs { { p.PlatformSku, GooglePlay.Name }, { p.PlatformSku, AppleAppStore.Name } });
                }
                UnityPurchasing.Initialize(this, builder);
            }
            catch (Exception e)
            {
                AdLogger.Error($"UnityIapAdapter.Initialize threw: {e}");
                onReady?.Invoke(false);
            }
        }

        private static ProductType ToUnityType(ProductKind k) => k switch
        {
            ProductKind.Consumable => ProductType.Consumable,
            ProductKind.NonConsumable => ProductType.NonConsumable,
            ProductKind.Subscription => ProductType.Subscription,
            _ => ProductType.Consumable,
        };

        // IStoreListener -
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _store = controller;
            _extensions = extensions;
            IsInitialized = true;
            AdLogger.Lifecycle("UnityIapAdapter", "Unity IAP ready");
            _onReady?.Invoke(true);
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            AdLogger.Error($"UnityIapAdapter init failed: {error}");
            _onReady?.Invoke(false);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            AdLogger.Error($"UnityIapAdapter init failed: {error} - {message}");
            _onReady?.Invoke(false);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {
            try
            {
                var product = e.purchasedProduct;
                var result = new PurchaseResult
                {
                    ProductId = product.definition.id,
                    Code = PurchaseResultCode.Success,
                    Receipt = product.receipt,
                    TransactionId = product.transactionID,
                    Currency = product.metadata.isoCurrencyCode,
                    LocalizedPrice = product.metadata.localizedPrice,
                };
                if (_products.TryGetValue(result.ProductId, out var meta) && meta.Kind == ProductKind.Subscription)
                    IapManager.RaiseSubscriptionActivated(result.ProductId);
                _pendingPurchaseCallback?.Invoke(result);
                _pendingPurchaseCallback = null;
            }
            catch (Exception ex) { AdLogger.Error($"UnityIapAdapter.ProcessPurchase threw: {ex}"); }
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            var result = new PurchaseResult
            {
                ProductId = product.definition.id,
                Code = MapFailure(reason),
            };
            _pendingPurchaseCallback?.Invoke(result);
            _pendingPurchaseCallback = null;
        }

        private static PurchaseResultCode MapFailure(PurchaseFailureReason reason) => reason switch
        {
            PurchaseFailureReason.UserCancelled => PurchaseResultCode.UserCancelled,
            PurchaseFailureReason.PaymentDeclined => PurchaseResultCode.PaymentDeclined,
            PurchaseFailureReason.ProductUnavailable => PurchaseResultCode.ProductUnavailable,
            PurchaseFailureReason.DuplicateTransaction => PurchaseResultCode.DuplicateTransaction,
            PurchaseFailureReason.SignatureInvalid => PurchaseResultCode.SignatureInvalid,
            PurchaseFailureReason.ExistingPurchasePending => PurchaseResultCode.DuplicateTransaction,
            _ => PurchaseResultCode.Unknown,
        };

        // IIapAdapter -
        public bool IsAvailable(string productId) =>
            _store != null && _store.products.WithID(productId)?.availableToPurchase == true;

        public void Purchase(string productId, Action<PurchaseResult> callback)
        {
            try
            {
                if (_store == null) { callback?.Invoke(new PurchaseResult { ProductId = productId, Code = PurchaseResultCode.Unknown }); return; }
                _pendingPurchaseCallback = callback;
                _store.InitiatePurchase(productId);
            }
            catch (Exception e)
            {
                AdLogger.Error($"UnityIapAdapter.Purchase threw: {e}");
                callback?.Invoke(new PurchaseResult { ProductId = productId, Code = PurchaseResultCode.Unknown });
            }
        }

        public void RestorePurchases(Action<bool> callback)
        {
            try
            {
#if UNITY_IOS
                var apple = _extensions?.GetExtension<IAppleExtensions>();
                _restoreCallback = callback;
                apple?.RestoreTransactions(ok => { _restoreCallback?.Invoke(ok); _restoreCallback = null; });
#else
                // Google Play restores automatically on init; just report success.
                callback?.Invoke(true);
#endif
            }
            catch (Exception e) { AdLogger.Error($"UnityIapAdapter.RestorePurchases threw: {e}"); callback?.Invoke(false); }
        }

        public bool HasNonConsumable(string productId)
        {
            var p = _store?.products.WithID(productId);
            return p != null && p.hasReceipt && p.definition.type == ProductType.NonConsumable;
        }

        public bool IsSubscriptionActive(string productId)
        {
            // Detailed subscription validation requires platform-specific extensions; this is
            // a coarse "we have a receipt" check. Pair with IReceiptValidator for real backend SSV.
            var p = _store?.products.WithID(productId);
            return p != null && p.hasReceipt && p.definition.type == ProductType.Subscription;
        }

        public decimal GetLocalizedPrice(string productId) =>
            _store?.products.WithID(productId)?.metadata.localizedPrice ?? 0m;

        public string GetLocalizedCurrencyCode(string productId) =>
            _store?.products.WithID(productId)?.metadata.isoCurrencyCode ?? "USD";
    }
}
#endif
