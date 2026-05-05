using System;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Native ads have a data-driven surface separate from full-screen / banner ads. Adapters implementing this interface load a NativeAdData and report back via callback.</summary>
    public interface INativeAdAdapter
    {
        bool IsInitialized { get; }
        void Initialize(AdManagerConfig config, Action<AdRevenueInfo> onRevenuePaid);

        /// <param name="onLoaded">Receives the data on success, null on failure.</param>
        void Load(string placement, Action<NativeAdData> onLoaded);

        /// <summary>Call when the user clicks/taps the bound prefab; lets the adapter record a click.</summary>
        void RecordClick(NativeAdData data);

        /// <summary>Call when the prefab becomes visible to the user.</summary>
        void RecordImpression(NativeAdData data);

        /// <summary>Call when the dev is done with this ad (prefab destroyed / scene unload).</summary>
        void Destroy(NativeAdData data);
    }
}
