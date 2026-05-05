using System;

namespace Rinval.MobileAdsAndIapKit
{
    public static class AttHelper
    {
        private static AttStatus? _override;

        public static AttStatus CurrentStatus
        {
            get
            {
                if (_override.HasValue) return _override.Value;
#if UNITY_IOS && !UNITY_EDITOR
                return AttStatus.NotDetermined;
#else
                return AttStatus.Unsupported;
#endif
            }
        }

        public static bool IsSupported
        {
            get
            {
                if (_override.HasValue) return _override.Value != AttStatus.Unsupported;
#if UNITY_IOS
                return true;
#else
                return false;
#endif
            }
        }

        public static void RequestAuthorization(Action<AttStatus> callback)
        {
            if (!IsSupported)
            {
                callback?.Invoke(AttStatus.Unsupported);
                return;
            }
#if UNITY_IOS && !UNITY_EDITOR
            // Real implementation would call iOS ATTrackingManager via plugin.
            // The buyer integrates their preferred ATT plugin and replaces this stub.
            callback?.Invoke(AttStatus.NotDetermined);
#else
            callback?.Invoke(_override ?? AttStatus.Authorized);
#endif
        }

        public static void OverrideForTests(AttStatus? value) => _override = value;
    }
}
