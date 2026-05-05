using System;

namespace Rinval.MobileAdsAndIapKit
{
    public static class ConsentManager
    {
        public static event Action<ConsentStatus> ConsentChanged;

        public static ConsentStatus GdprStatus => UmpHelper.Status;
        public static AttStatus AttStatus => AttHelper.CurrentStatus;

        public static void RequestAll(string privacyPolicyUrl, Action onComplete = null)
        {
            UmpHelper.RequestConsent(privacyPolicyUrl, gdpr =>
            {
                ConsentChanged?.Invoke(gdpr);
                if (AttHelper.IsSupported)
                {
                    AttHelper.RequestAuthorization(_ => onComplete?.Invoke());
                }
                else
                {
                    onComplete?.Invoke();
                }
            });
        }

        public static void Grant()
        {
            UmpHelper.SetStatus(ConsentStatus.Obtained);
            ConsentChanged?.Invoke(ConsentStatus.Obtained);
        }

        public static void Deny()
        {
            UmpHelper.SetStatus(ConsentStatus.Denied);
            ConsentChanged?.Invoke(ConsentStatus.Denied);
        }

        public static void Reset()
        {
            UmpHelper.Reset();
            ConsentChanged = null;
        }

        public static bool AdsAllowed
        {
            get
            {
                var s = UmpHelper.Status;
                if (s == ConsentStatus.Denied) return false;
                var att = AttHelper.CurrentStatus;
                if (att == AttStatus.Denied) return true;
                return true;
            }
        }
    }
}
