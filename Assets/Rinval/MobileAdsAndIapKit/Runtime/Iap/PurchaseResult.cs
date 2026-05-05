namespace Rinval.MobileAdsAndIapKit
{
    public enum PurchaseResultCode
    {
        Success,
        UserCancelled,
        PaymentDeclined,
        ProductUnavailable,
        DuplicateTransaction,
        SignatureInvalid,
        NetworkError,
        Unknown,
    }

    public struct PurchaseResult
    {
        public string ProductId;
        public PurchaseResultCode Code;
        public string Receipt;
        public string TransactionId;
        public string Currency;
        public decimal LocalizedPrice;

        public bool Successful => Code == PurchaseResultCode.Success;

        public override string ToString() =>
            $"PurchaseResult({ProductId} {Code} {LocalizedPrice} {Currency} txn={TransactionId})";
    }
}
