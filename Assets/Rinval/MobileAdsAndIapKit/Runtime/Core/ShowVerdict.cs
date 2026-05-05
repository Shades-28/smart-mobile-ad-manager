namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Diagnostic snapshot of a show-attempt decision. Surfaces "why didn't this work?" without forcing callers to use `out string` parameters.</summary>
    public readonly struct ShowVerdict
    {
        public readonly bool Allowed;
        public readonly string Reason;
        public readonly string Placement;
        public readonly AdFormat Format;

        public ShowVerdict(bool allowed, string reason, string placement, AdFormat format)
        {
            Allowed = allowed;
            Reason = reason ?? string.Empty;
            Placement = placement ?? string.Empty;
            Format = format;
        }

        public static ShowVerdict Ok(string placement, AdFormat format) =>
            new ShowVerdict(true, string.Empty, placement, format);

        public static ShowVerdict Blocked(string reason, string placement, AdFormat format) =>
            new ShowVerdict(false, reason, placement, format);

        public override string ToString() => Allowed ? $"{Format} OK" : $"{Format} blocked: {Reason}";
    }
}
