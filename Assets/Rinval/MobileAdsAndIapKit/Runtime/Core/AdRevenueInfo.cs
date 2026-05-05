namespace Rinval.MobileAdsAndIapKit
{
    public readonly struct AdRevenueInfo
    {
        public readonly AdFormat Format;
        public readonly string AdUnitId;
        public readonly string Placement;
        public readonly string NetworkName;
        public readonly string Currency;
        public readonly double Amount;

        public AdRevenueInfo(
            AdFormat format,
            string adUnitId,
            string placement,
            string networkName,
            string currency,
            double amount)
        {
            Format = format;
            AdUnitId = adUnitId ?? string.Empty;
            Placement = placement ?? string.Empty;
            NetworkName = networkName ?? string.Empty;
            Currency = string.IsNullOrEmpty(currency) ? "USD" : currency;
            Amount = amount;
        }

        public override string ToString()
        {
            return $"{Format} | {NetworkName} | {Placement} | {Amount:0.######} {Currency}";
        }

        /// <summary>Display-friendly amount, e.g. "$0.0085" for USD or "0.0085 EUR" otherwise.</summary>
        public string FormatLocalized()
        {
            if (Currency == "USD") return $"${Amount:0.0000}";
            return $"{Amount:0.0000} {Currency}";
        }
    }
}
