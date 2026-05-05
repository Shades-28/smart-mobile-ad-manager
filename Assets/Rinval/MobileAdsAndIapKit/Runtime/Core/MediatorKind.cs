namespace Rinval.MobileAdsAndIapKit
{
    public enum MediatorKind
    {
        None,
        Editor,
        AppLovinMax,
        GoogleAdMob,
        UnityLevelPlay,
    }

    public static class MediatorKindExtensions
    {
        /// <summary>True when the mediator runs against a real ad network (not Stub or Editor sim).</summary>
        public static bool IsLive(this MediatorKind k) =>
            k == MediatorKind.AppLovinMax
            || k == MediatorKind.GoogleAdMob
            || k == MediatorKind.UnityLevelPlay;
    }
}
