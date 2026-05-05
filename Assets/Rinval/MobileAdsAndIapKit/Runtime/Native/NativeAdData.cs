using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Snapshot of a native ad's user-facing fields. Dev binds these to their own Unity prefab (Image / Text / Button) - no native overlay rendering involved at this layer.</summary>
    public class NativeAdData
    {
        public string Headline;
        public string Body;
        public string CallToAction;
        public string Advertiser;
        public Texture2D IconTexture;
        public Texture2D ImageTexture;
        public float StarRating; // 0-5; 0 if not provided
        public string Network;

        /// <summary>Opaque per-network handle. Adapters use this to track impression / click.</summary>
        public object Handle;
    }
}
