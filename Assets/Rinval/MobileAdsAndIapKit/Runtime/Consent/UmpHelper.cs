using System;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Google User Messaging Platform (UMP) consent integration. The default in-package implementation assumes consent is not required (test-mode behavior). Define <c>RINVAL_UMP</c> and install the Google Mobile Ads Unity plugin (which bundles UMP) to use the real flow that surfaces the GDPR consent form.</summary>
    public static class UmpHelper
    {
        private static ConsentStatus _status = ConsentStatus.Unknown;

        public static ConsentStatus Status => _status;

        public static void RequestConsent(string privacyPolicyUrl, Action<ConsentStatus> callback)
        {
#if RINVAL_UMP && !UNITY_EDITOR
            try
            {
                RequestViaUmp(privacyPolicyUrl, callback);
            }
            catch (Exception e)
            {
                AdLogger.Error($"UmpHelper.RequestViaUmp threw: {e}");
                _status = ConsentStatus.Unknown;
                callback?.Invoke(_status);
            }
#else
            // Default / editor behavior: assume not required so the rest of the kit can flow.
            _ = privacyPolicyUrl;
            _status = ConsentStatus.NotRequired;
            callback?.Invoke(_status);
#endif
        }

#if RINVAL_UMP && !UNITY_EDITOR
        private static void RequestViaUmp(string privacyPolicyUrl, Action<ConsentStatus> callback)
        {
            // Wire to GoogleMobileAds.Ump.Api when present. We compile this against reflection-style
            // calls so the kit doesn't hard-fail if the package isn't installed despite the define.
            var paramsType = System.Type.GetType("GoogleMobileAds.Ump.Api.ConsentRequestParameters, GoogleMobileAds.Ump");
            var infoType = System.Type.GetType("GoogleMobileAds.Ump.Api.ConsentInformation, GoogleMobileAds.Ump");
            var formType = System.Type.GetType("GoogleMobileAds.Ump.Api.ConsentForm, GoogleMobileAds.Ump");
            if (paramsType == null || infoType == null || formType == null)
            {
                AdLogger.Warn("UmpHelper: RINVAL_UMP defined but UMP SDK types not found.");
                _status = ConsentStatus.Unknown;
                callback?.Invoke(_status);
                return;
            }
            var requestParams = System.Activator.CreateInstance(paramsType);
            var update = infoType.GetMethod("Update");
            update?.Invoke(null, new object[] {
                requestParams,
                (System.Action<object>)(err => {
                    var loadAndShow = formType.GetMethod("LoadAndShowConsentFormIfRequired");
                    loadAndShow?.Invoke(null, new object[] {
                        (System.Action<object>)(_ => {
                            _status = ConsentStatus.Obtained;
                            MainThreadDispatcher.Enqueue(() => callback?.Invoke(_status));
                        })
                    });
                })
            });
        }
#endif

        public static void SetStatus(ConsentStatus value) => _status = value;
        public static void Reset() => _status = ConsentStatus.Unknown;
    }
}
