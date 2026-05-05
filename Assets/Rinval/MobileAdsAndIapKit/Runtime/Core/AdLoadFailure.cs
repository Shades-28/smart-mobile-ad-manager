namespace Rinval.MobileAdsAndIapKit
{
    public readonly struct AdLoadFailure
    {
        public readonly AdFormat Format;
        public readonly string NetworkName;
        public readonly int ErrorCode;
        public readonly string Message;

        public AdLoadFailure(AdFormat format, string networkName, int errorCode, string message)
        {
            Format = format;
            NetworkName = networkName ?? string.Empty;
            ErrorCode = errorCode;
            Message = message ?? string.Empty;
        }

        public override string ToString() =>
            $"{Format} load failed on {NetworkName}: [{ErrorCode}] {Message}";
    }
}
