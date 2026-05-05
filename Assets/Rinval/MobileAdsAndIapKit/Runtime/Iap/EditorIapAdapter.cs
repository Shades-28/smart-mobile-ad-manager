using System;
using System.Collections.Generic;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Simulates IAP in the editor. Every purchase succeeds unless the publisher overrides NextResult. Tracks owned non-consumables in memory so HasNonConsumable returns the right state across calls within a session.</summary>
    public class EditorIapAdapter : IIapAdapter
    {
        public static PurchaseResultCode NextResult { get; set; } = PurchaseResultCode.Success;
        public static decimal SimulatedPrice { get; set; } = 0.99m;
        public static string SimulatedCurrency { get; set; } = "USD";

        private readonly Dictionary<string, IapProduct> _products = new Dictionary<string, IapProduct>();
        private readonly HashSet<string> _ownedNonCons = new HashSet<string>();
        private readonly HashSet<string> _activeSubs = new HashSet<string>();

        public bool IsInitialized { get; private set; }

        public void Initialize(IList<IapProduct> products, Action<bool> onReady)
        {
            _products.Clear();
            foreach (var p in products) if (p != null && !string.IsNullOrEmpty(p.ProductId)) _products[p.ProductId] = p;
            IsInitialized = true;
            AdLogger.Lifecycle("EditorIapAdapter.Initialize", $"{_products.Count} products");
            onReady?.Invoke(true);
        }

        public bool IsAvailable(string productId) => _products.ContainsKey(productId);

        public void Purchase(string productId, Action<PurchaseResult> callback)
        {
            if (!_products.TryGetValue(productId, out var product))
            {
                callback?.Invoke(new PurchaseResult { ProductId = productId, Code = PurchaseResultCode.ProductUnavailable });
                return;
            }
            var code = NextResult;
            NextResult = PurchaseResultCode.Success;
            var result = new PurchaseResult
            {
                ProductId = productId,
                Code = code,
                Receipt = $"editor-receipt-{productId}-{Guid.NewGuid():N}",
                TransactionId = Guid.NewGuid().ToString("N"),
                Currency = SimulatedCurrency,
                LocalizedPrice = SimulatedPrice,
            };
            if (code == PurchaseResultCode.Success)
            {
                if (product.Kind == ProductKind.NonConsumable) _ownedNonCons.Add(productId);
                if (product.Kind == ProductKind.Subscription)
                {
                    _activeSubs.Add(productId);
                    IapManager.RaiseSubscriptionActivated(productId);
                }
                AdLogger.Tag("IAP", $"editor purchase OK: {productId}");
            }
            else AdLogger.Tag("IAP", $"editor purchase {code}: {productId}");
            callback?.Invoke(result);
        }

        public void RestorePurchases(Action<bool> callback)
        {
            AdLogger.Tag("IAP", $"editor restore: {_ownedNonCons.Count} non-cons, {_activeSubs.Count} subs");
            callback?.Invoke(true);
        }

        public bool HasNonConsumable(string productId) => _ownedNonCons.Contains(productId);
        public bool IsSubscriptionActive(string productId) => _activeSubs.Contains(productId);
        public decimal GetLocalizedPrice(string productId) => SimulatedPrice;
        public string GetLocalizedCurrencyCode(string productId) => SimulatedCurrency;

        // Editor-only test hooks
        public void SimulateRefund(string productId)
        {
            _ownedNonCons.Remove(productId);
            _activeSubs.Remove(productId);
            IapManager.RaiseRefundDetected(productId);
            if (_products.TryGetValue(productId, out var p) && p.Kind == ProductKind.Subscription)
                IapManager.RaiseSubscriptionDeactivated(productId);
        }
    }
}
