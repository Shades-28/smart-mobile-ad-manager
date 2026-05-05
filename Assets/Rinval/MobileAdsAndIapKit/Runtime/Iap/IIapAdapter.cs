using System;
using System.Collections.Generic;

namespace Rinval.MobileAdsAndIapKit
{
    public interface IIapAdapter
    {
        bool IsInitialized { get; }
        void Initialize(IList<IapProduct> products, Action<bool> onReady);
        bool IsAvailable(string productId);
        void Purchase(string productId, Action<PurchaseResult> callback);
        void RestorePurchases(Action<bool> callback);
        bool HasNonConsumable(string productId);
        bool IsSubscriptionActive(string productId);
        decimal GetLocalizedPrice(string productId);
        string GetLocalizedCurrencyCode(string productId);
    }

    public interface IReceiptValidator
    {
        /// <summary>Validate a purchase receipt against your server. onResult(true) = grant, (false) = deny.</summary>
        void Validate(PurchaseResult purchase, Action<bool> onResult);
    }
}
