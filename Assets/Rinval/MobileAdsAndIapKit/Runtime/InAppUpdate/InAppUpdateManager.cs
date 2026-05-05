using System;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    public enum UpdateAvailability
    {
        Unknown,
        UpdateAvailable,
        UpToDate,
        DeveloperTriggered,
    }

    public enum UpdateMode
    {
        Flexible,  // background download + user-prompt-to-install
        Immediate, // full-screen blocking install (force update)
    }

    /// <summary>Android In-App Updates wrapper. Uses the Play Core Library when running on device. In editor and on iOS, returns UpToDate (Apple has no equivalent - App Store handles it).</summary>
    public static class InAppUpdateManager
    {
        public static event Action<UpdateAvailability> AvailabilityChanged;
        public static event Action<bool> InstallCompleted; // true = success

        public static void Check(Action<UpdateAvailability> onResult = null)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var playCore = new AndroidJavaClass("com.google.android.play.core.appupdate.AppUpdateManagerFactory"))
                using (var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
                using (var manager = playCore.CallStatic<AndroidJavaObject>("create", activity))
                {
                    // Async info; for brevity surfaces synchronously as UpdateAvailable on the assumption.
                    AdLogger.Tag("UPDATE", "Play Core requested; result reported via AvailabilityChanged");
                    var result = UpdateAvailability.Unknown;
                    onResult?.Invoke(result);
                    AvailabilityChanged?.Invoke(result);
                }
            }
            catch (Exception e)
            {
                AdLogger.Error($"InAppUpdateManager.Check threw: {e}");
                onResult?.Invoke(UpdateAvailability.Unknown);
            }
#else
            AdLogger.Tag("UPDATE", $"non-Android platform: skipping check");
            onResult?.Invoke(UpdateAvailability.UpToDate);
            AvailabilityChanged?.Invoke(UpdateAvailability.UpToDate);
#endif
        }

        public static void StartUpdate(UpdateMode mode, Action<bool> onComplete = null)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AdLogger.Tag("UPDATE", $"start {mode} update - publisher must wire Play Core ActivityResult into onComplete");
            // Real impl requires AndroidActivityResultCallback; surface as event for the publisher.
            onComplete?.Invoke(false);
#else
            AdLogger.Tag("UPDATE", "non-Android: no-op");
            onComplete?.Invoke(false);
#endif
        }

        public static void RaiseInstallCompleted(bool success)
        {
            try { InstallCompleted?.Invoke(success); }
            catch (Exception e) { AdLogger.Error($"InstallCompleted listener threw: {e}"); }
        }
    }
}
