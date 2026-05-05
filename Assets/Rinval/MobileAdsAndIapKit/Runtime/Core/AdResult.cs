namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>High-level result wrapper around AdResultCode. Hides the 10-enum surface for the 95% of call sites that only care about "did the reward happen / did the ad show?".</summary>
    public readonly struct AdResult
    {
        public readonly AdResultCode RawCode;

        public AdResult(AdResultCode code) { RawCode = code; }

        /// <summary>True only when a rewarded ad granted reward.</summary>
        public bool WasRewarded => RawCode == AdResultCode.Rewarded;

        /// <summary>True when the ad reached the user's screen (Closed or Shown).</summary>
        public bool WasShown => RawCode == AdResultCode.Closed || RawCode == AdResultCode.Shown;

        /// <summary>User cancelled or skipped without earning reward.</summary>
        public bool WasCancelled => RawCode == AdResultCode.Cancelled;

        /// <summary>Show attempt failed (network error, display failed, no fill).</summary>
        public bool Failed =>
            RawCode == AdResultCode.Failed
            || RawCode == AdResultCode.LoadFailed
            || RawCode == AdResultCode.TimedOut;

        /// <summary>Ad system was off (master switch, format disabled, test lab).</summary>
        public bool WasDisabled => RawCode == AdResultCode.Disabled;

        /// <summary>Ad wasn't ready when Show was called (gated, no inventory, not loaded).</summary>
        public bool WasNotReady => RawCode == AdResultCode.NotReady;

        public static implicit operator AdResultCode(AdResult r) => r.RawCode;
        public static implicit operator AdResult(AdResultCode c) => new AdResult(c);

        public override string ToString() => RawCode.ToString();
    }
}
