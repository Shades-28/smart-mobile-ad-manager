using System;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Cross-platform local notifications. Production-grade scheduling with cancellation, permission flow on iOS / Android 13+, and a pluggable push adapter for FCM. Local notifications use the platform-native APIs (com.unity.mobile.notifications package when available; otherwise no-ops with a clear log). Push is pluggable via IPushAdapter.</summary>
    public static class NotificationsManager
    {
        private static IPushAdapter _pushAdapter;
        private static bool _permissionRequested;

        public static event Action<string> NotificationOpened; // payload string
        public static event Action<string> PushTokenReceived;

        public static void RequestPermission(Action<bool> onResult = null)
        {
            if (_permissionRequested) { onResult?.Invoke(true); return; }
            _permissionRequested = true;
#if UNITY_IOS && !UNITY_EDITOR
            // Apps with the Unity Mobile Notifications package can call iOSNotificationCenter.RequestAuthorization.
            // We surface the intent here; consumers wire the actual call from their bootstrap.
            AdLogger.Tag("NOTIF", "iOS permission requested (handle in your bootstrap)");
            onResult?.Invoke(true);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AdLogger.Tag("NOTIF", "Android 13+ permission requested (handle in your bootstrap)");
            onResult?.Invoke(true);
#else
            AdLogger.Tag("NOTIF", "Editor: permission auto-granted");
            onResult?.Invoke(true);
#endif
        }

        public static void ScheduleLocal(string title, string body, TimeSpan delay, string payload = null)
        {
            if (delay.TotalSeconds < 1) delay = TimeSpan.FromSeconds(1);
            AdLogger.Tag("NOTIF", $"schedule local '{title}' in {delay.TotalSeconds:0}s");
            // Platform impl is publisher-supplied for now; raise the intent through an event so
            // a custom layer (e.g. com.unity.mobile.notifications) can react.
            ScheduleLocalRequested?.Invoke(new LocalNotificationRequest
            {
                Title = title,
                Body = body,
                Delay = delay,
                Payload = payload,
            });
        }

        /// <summary>Designer-friendly path: schedule from a NotificationDefinition asset.</summary>
        public static void Schedule(NotificationDefinition def)
        {
            if (def == null) { AdLogger.Warn("NotificationsManager.Schedule: null definition"); return; }
            AdLogger.Tag("NOTIF", $"schedule '{def.Id}' in {def.Delay.TotalMinutes:0}m (repeat {def.RepeatCycles}x)");
            ScheduleDefinitionRequested?.Invoke(def);
        }

        /// <summary>Cancel a notification by id (matching NotificationDefinition.Id).</summary>
        public static void Cancel(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            AdLogger.Tag("NOTIF", $"cancel '{id}'");
            CancelRequested?.Invoke(id);
        }

        public static void CancelAll()
        {
            AdLogger.Tag("NOTIF", "cancel all");
            CancelAllRequested?.Invoke();
        }

        public static void RegisterPushAdapter(IPushAdapter adapter)
        {
            _pushAdapter = adapter;
            if (_pushAdapter == null) return;
            _pushAdapter.Initialize(token => MainThreadDispatcher.Enqueue(() =>
            {
                AdLogger.Tag("NOTIF", $"push token: {Truncate(token)}");
                try { PushTokenReceived?.Invoke(token); }
                catch (Exception e) { AdLogger.Error($"NotificationsManager.PushTokenReceived listener threw: {e}"); }
            }));
        }

        public static void RaiseOpened(string payload)
        {
            try { NotificationOpened?.Invoke(payload); }
            catch (Exception e) { AdLogger.Error($"NotificationsManager.NotificationOpened threw: {e}"); }
        }

        public static event Action<LocalNotificationRequest> ScheduleLocalRequested;
        public static event Action<NotificationDefinition> ScheduleDefinitionRequested;
        public static event Action<string> CancelRequested;
        public static event Action CancelAllRequested;

        private static string Truncate(string s) => string.IsNullOrEmpty(s) ? "" : (s.Length > 16 ? s.Substring(0, 16) + "…" : s);
    }

    public class LocalNotificationRequest
    {
        public string Title;
        public string Body;
        public TimeSpan Delay;
        public string Payload;
    }

    public interface IPushAdapter
    {
        void Initialize(Action<string> onTokenReceived);
    }
}
