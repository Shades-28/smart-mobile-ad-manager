using System;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Defers ad loads when the device is offline. Schedulers consult IsOnline() before each load attempt; if offline, they skip the kick and rely on Tick to retry once back online.</summary>
    public static class ConnectivityGuard
    {
        private static Func<bool> _override;

        /// <summary>Tests can substitute the connectivity check. Pass null to restore default behavior (Application.internetReachability).</summary>
        public static void OverrideForTests(Func<bool> isOnline)
        {
            _override = isOnline;
        }

        public static bool IsOnline()
        {
            if (_override != null) return _override();
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
    }
}
