#if AD_USE_ADMOB
using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Production AdMob native ad adapter. Loads a single native ad per Load() call, surfaces headline/body/icon/CTA via NativeAdData. The opaque Handle is the underlying NativeAd.</summary>
    public class AdMobNativeAdAdapter : INativeAdAdapter
    {
        public bool IsInitialized { get; private set; }

        private AdManagerConfig _config;
        private Action<AdRevenueInfo> _onRevenuePaid;
        private readonly Dictionary<NativeAdData, NativeAd> _bound = new Dictionary<NativeAdData, NativeAd>();

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterFactory()
        {
            // Auto-registers if AdMob mediation is selected; harmless otherwise.
            NativeAdLoader.RegisterAdapter(new AdMobNativeAdAdapter());
        }

        public void Initialize(AdManagerConfig config, Action<AdRevenueInfo> onRevenuePaid)
        {
            _config = config;
            _onRevenuePaid = onRevenuePaid;
            IsInitialized = true;
            AdLogger.Lifecycle("AdMobNativeAdAdapter.Initialize", "ready");
        }

        public void Load(string placement, Action<NativeAdData> onLoaded)
        {
            try
            {
                var id = _config?.GetNativeId();
                if (string.IsNullOrEmpty(id)) { AdLogger.Warn("AdMob native ID empty"); onLoaded?.Invoke(null); return; }

                var loader = new AdLoader.Builder(id).ForNativeAd().Build();
                loader.OnNativeAdLoaded += (s, args) =>
                {
                    var ad = args.nativeAd;
                    var data = new NativeAdData
                    {
                        Headline = ad.GetHeadlineText(),
                        Body = ad.GetBodyText(),
                        CallToAction = ad.GetCallToActionText(),
                        Advertiser = ad.GetAdvertiserText(),
                        IconTexture = ad.GetIconTexture(),
                        ImageTexture = ad.GetImageTextures()?.Count > 0 ? ad.GetImageTextures()[0] : null,
                        StarRating = (float)(ad.GetStarRating() ?? 0.0),
                        Network = "AdMob",
                        Handle = ad,
                    };
                    _bound[data] = ad;
                    ad.OnPaidEvent += (sender, paid) =>
                    {
                        try
                        {
                            var amt = paid.AdValue.Value / 1_000_000.0;
                            var info = new AdRevenueInfo(AdFormat.Native, "admob-native", placement, "AdMob", paid.AdValue.CurrencyCode, amt);
                            MainThreadDispatcher.Enqueue(() => _onRevenuePaid?.Invoke(info));
                        }
                        catch (Exception e) { AdLogger.Error($"AdMobNativeAdAdapter.OnPaidEvent threw: {e}"); }
                    };
                    onLoaded?.Invoke(data);
                };
                loader.OnAdFailedToLoad += (s, args) =>
                {
                    AdLogger.Network("AdMob", $"native load failed: {args.LoadAdError}");
                    onLoaded?.Invoke(null);
                };
                loader.LoadAd(new AdRequest());
            }
            catch (Exception e) { AdLogger.Error($"AdMobNativeAdAdapter.Load threw: {e}"); onLoaded?.Invoke(null); }
        }

        public void RecordImpression(NativeAdData data)
        {
            // AdMob's NativeAd records impressions automatically via the bound view; no-op here.
        }

        public void RecordClick(NativeAdData data)
        {
            // AdMob's NativeAd handles click via its own button; no-op.
        }

        public void Destroy(NativeAdData data)
        {
            try
            {
                if (_bound.TryGetValue(data, out var ad)) { ad.Destroy(); _bound.Remove(data); }
            }
            catch (Exception e) { AdLogger.Error($"AdMobNativeAdAdapter.Destroy threw: {e}"); }
        }
    }
}
#endif
