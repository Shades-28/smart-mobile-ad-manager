using System;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Static facade for native ads. AdMob has a real adapter; AppLovin/LevelPlay can register their own.</summary>
    public static class NativeAdLoader
    {
        private static INativeAdAdapter _adapter;
        public static INativeAdAdapter Adapter => _adapter;

        public static void RegisterAdapter(INativeAdAdapter adapter) => _adapter = adapter;

        public static bool IsAvailable => _adapter != null && _adapter.IsInitialized;

        public static void Load(string placement, Action<NativeAdData> onLoaded)
        {
            if (_adapter == null) { AdLogger.Warn("No native-ad adapter registered"); onLoaded?.Invoke(null); return; }
            try { _adapter.Load(placement, data => MainThreadDispatcher.Enqueue(() => onLoaded?.Invoke(data))); }
            catch (Exception e) { AdLogger.Error($"NativeAdLoader.Load threw: {e}"); onLoaded?.Invoke(null); }
        }

        public static void RecordImpression(NativeAdData data)
        {
            if (_adapter == null || data == null) return;
            try { _adapter.RecordImpression(data); }
            catch (Exception e) { AdLogger.Error($"NativeAdLoader.RecordImpression threw: {e}"); }
        }

        public static void RecordClick(NativeAdData data)
        {
            if (_adapter == null || data == null) return;
            try { _adapter.RecordClick(data); }
            catch (Exception e) { AdLogger.Error($"NativeAdLoader.RecordClick threw: {e}"); }
        }

        public static void Destroy(NativeAdData data)
        {
            if (_adapter == null || data == null) return;
            try { _adapter.Destroy(data); }
            catch (Exception e) { AdLogger.Error($"NativeAdLoader.Destroy threw: {e}"); }
        }
    }
}
