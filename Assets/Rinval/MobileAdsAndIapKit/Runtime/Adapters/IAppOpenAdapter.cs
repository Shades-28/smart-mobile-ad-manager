using System;

namespace Rinval.MobileAdsAndIapKit
{
    public interface IAppOpenAdapter
    {
        MediatorKind Kind { get; }
        bool IsInitialized { get; }

        void Initialize(AdManagerConfig config, Action<AdRevenueInfo> onRevenuePaid);
        void Load();
        bool IsReady();
        void Show(Action<AdResultCode> callback);
    }
}
