namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Banner sizes. Adaptive maps to the network's anchored-adaptive size when supported.</summary>
    public enum BannerSize
    {
        Standard,        // 320x50
        Adaptive,        // anchored adaptive (taller, higher CPM where available)
        MediumRectangle, // 300x250 - see also LoadMrec
        Leaderboard,     // 728x90 (tablet)
    }
}
