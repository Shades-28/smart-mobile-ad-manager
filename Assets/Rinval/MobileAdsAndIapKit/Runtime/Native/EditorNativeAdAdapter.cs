using System;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Editor / no-mediator stub. Returns a placeholder NativeAdData so devs can wire their prefab and verify the layout without device builds.</summary>
    public class EditorNativeAdAdapter : INativeAdAdapter
    {
        public bool IsInitialized { get; private set; }

        public void Initialize(AdManagerConfig config, Action<AdRevenueInfo> onRevenuePaid)
        {
            IsInitialized = true;
            AdLogger.Lifecycle("EditorNativeAdAdapter.Initialize", "ready (simulated)");
        }

        public void Load(string placement, Action<NativeAdData> onLoaded)
        {
            var data = new NativeAdData
            {
                Headline = "Try Rinval Games",
                Body = "Production-grade Unity tooling for mobile devs.",
                CallToAction = "Install",
                Advertiser = "Rinval Games",
                IconTexture = Texture2D.whiteTexture,
                ImageTexture = Texture2D.whiteTexture,
                StarRating = 4.7f,
                Network = "Editor (Simulated)",
                Handle = null,
            };
            onLoaded?.Invoke(data);
        }

        public void RecordImpression(NativeAdData data) => AdLogger.Tag("NATIVE", "impression (sim)");
        public void RecordClick(NativeAdData data) => AdLogger.Tag("NATIVE", "click (sim)");
        public void Destroy(NativeAdData data) { /* no-op */ }
    }
}
