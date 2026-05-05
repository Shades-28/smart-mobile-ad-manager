#if RINVAL_FCM
using System;
using Firebase.Messaging;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Firebase Cloud Messaging push adapter. Compiles only when RINVAL_FCM is defined and the Firebase Messaging package is installed. Plug in via NotificationsManager.RegisterPushAdapter.</summary>
    public class FcmPushAdapter : IPushAdapter
    {
        public void Initialize(Action<string> onTokenReceived)
        {
            try
            {
                FirebaseMessaging.TokenReceived += (sender, args) => onTokenReceived?.Invoke(args.Token);
                FirebaseMessaging.MessageReceived += (sender, args) =>
                {
                    var payload = args.Message?.Data != null ? string.Join(",", args.Message.Data.Keys) : "";
                    NotificationsManager.RaiseOpened(payload);
                };
            }
            catch (Exception e) { AdLogger.Error($"FcmPushAdapter.Initialize threw: {e}"); }
        }
    }
}
#endif
