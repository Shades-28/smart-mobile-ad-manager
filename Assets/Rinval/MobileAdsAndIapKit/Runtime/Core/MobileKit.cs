using System;
using System.Threading.Tasks;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>One-line facade that boots the kit. Power users can still call AdManager / IapManager / AppOpenAdManager directly.</summary>
    public static class MobileKit
    {
        public static bool IsReady { get; private set; }
        public static event Action Ready;

        public static void Initialize(KitConfig config, Action<bool> onReady = null)
        {
            if (IsReady) { onReady?.Invoke(true); return; }
            if (config == null)
            {
                AdLogger.Error("MobileKit.Initialize: null config");
                onReady?.Invoke(false);
                return;
            }
            if (config.AdConfig == null)
            {
                AdLogger.Error("MobileKit.Initialize: KitConfig.AdConfig is required");
                onReady?.Invoke(false);
                return;
            }

            // 1. Ads core
            AdManager.Initialize(config.AdConfig);
            if (config.SsvValidator != null) AdManager.SetSsvValidator(config.SsvValidator);

            // 2. App Open ads (optional)
            if (config.UseAppOpen)
                AppOpenAdManager.Initialize(config.AdConfig);

            // 3. Connectivity watcher (optional, but cheap and useful)
            if (config.UseConnectivityWatcher)
                ConnectivityWatcher.EnsureRunning();

            // Apply kit-wide settings if supplied (poll interval, rate-game rules, etc.).
            if (config.KitSettings != null && config.KitSettings.AutoApply)
                config.KitSettings.Apply();

            // A/B test catalog (sticky-buckets the user; experiments evaluated on demand).
            if (config.AbCatalog != null)
                AbTestManager.SetCatalog(config.AbCatalog);

            // 4. IAP (optional). If a catalog is supplied, we wait for it to init before reporting ready.
            if (config.IapCatalog != null && config.IapCatalog.Products.Count > 0)
            {
                // Pre-arm RemoveAdsManager before IAP init so its event handlers are wired in time.
                RemoveAdsManager.Initialize(config.RemoveAdsProductId);
                IapManager.Initialize(config.IapCatalog, validator: config.ReceiptValidator, onReady: ok =>
                {
                    IsReady = ok;
                    if (ok)
                    {
                        try { Ready?.Invoke(); }
                        catch (Exception e) { AdLogger.Error($"MobileKit.Ready listener threw: {e}"); }
                    }
                    onReady?.Invoke(ok);
                });
            }
            else
            {
                IsReady = true;
                try { Ready?.Invoke(); }
                catch (Exception e) { AdLogger.Error($"MobileKit.Ready listener threw: {e}"); }
                onReady?.Invoke(true);
            }
        }

        /// <summary>Async one-liner. Returns true on success.</summary>
        public static Task<bool> InitializeAsync(KitConfig config)
        {
            var tcs = new TaskCompletionSource<bool>();
            Initialize(config, ok => tcs.TrySetResult(ok));
            return tcs.Task;
        }

        public static void Shutdown()
        {
            AdManager.Shutdown();
            AppOpenAdManager.Shutdown();
            IapManager.Shutdown();
            RemoveAdsManager.Shutdown();
            IsReady = false;
            Ready = null;
        }
    }

    /// <summary>Configuration object passed to MobileKit.Initialize.</summary>
    public class KitConfig
    {
        public AdManagerConfig AdConfig;
        public IapProductCatalog IapCatalog;
        public bool UseAppOpen = false;
        public bool UseConnectivityWatcher = true;
        public IRewardedSsvValidator SsvValidator;
        public IReceiptValidator ReceiptValidator;

        /// <summary>Product ID used to remove ads. Default: "remove_ads".</summary>
        public string RemoveAdsProductId = RemoveAdsManager.DefaultProductId;

        /// <summary>Optional kit-wide tuning knobs (connectivity poll, rate-game rules, etc.).</summary>
        public MobileKitSettings KitSettings;

        /// <summary>Optional A/B-test experiment catalog. Sticky-buckets the user on init.</summary>
        public AbExperimentCatalog AbCatalog;
    }
}
