using System;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Designer-authored notification. Drag into NotificationsManager.Schedule(def) instead of passing strings/durations at call sites. Lets non-coders adjust copy, timing, and visuals without touching C#.</summary>
    [CreateAssetMenu(
        fileName = "NotificationDefinition",
        menuName = "Rinval/Mobile Ads & IAP/Notification Definition",
        order = 130)]
    public class NotificationDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Stable key used for cancellation and dedup. Keep unique across the whole game.")]
        [SerializeField] private string _id;

        [Header("Copy")]
        [SerializeField] private string _title = "Come back!";
        [TextArea, SerializeField] private string _body = "Your daily reward is waiting.";
        [Tooltip("Optional iOS subtitle. Ignored on Android.")]
        [SerializeField] private string _subtitleIos;

        [Header("Visuals")]
        [Tooltip("Optional Android small icon resource name (must be in res/drawable).")]
        [SerializeField] private string _smallIconAndroid = "icon_0";
        [Tooltip("Optional Android large icon resource name.")]
        [SerializeField] private string _largeIconAndroid;

        [Header("Schedule")]
        [Tooltip("Minutes from Schedule() call until the notification fires. Minimum 1 minute.")]
        [Min(1), SerializeField] private int _delayMinutes = 60;

        [Tooltip("Number of repeat cycles after the first fire. 0 = no repeat. Range 0-3.")]
        [Range(0, 3), SerializeField] private int _repeatCycles = 0;

        [Tooltip("Minutes between repeat fires. Ignored if RepeatCycles == 0.")]
        [Min(1), SerializeField] private int _repeatIntervalMinutes = 60;

        [Tooltip("If true, this notification is cancelled and re-scheduled every time the game starts.")]
        [SerializeField] private bool _resetOnGameStart = true;

        [Header("Channel (Android)")]
        [Tooltip("Notification channel ID. Auto-registered with high importance.")]
        [SerializeField] private string _channelId = "default";
        [SerializeField] private string _channelName = "Default";
        [TextArea, SerializeField] private string _channelDescription = "Game notifications";

        [Header("Payload")]
        [Tooltip("Optional payload string passed to NotificationOpened event handlers (e.g. \"daily_reward\").")]
        [SerializeField] private string _payload;

        public string Id => string.IsNullOrEmpty(_id) ? name : _id;
        public string Title => _title ?? string.Empty;
        public string Body => _body ?? string.Empty;
        public string SubtitleIos => _subtitleIos ?? string.Empty;
        public string SmallIconAndroid => _smallIconAndroid ?? string.Empty;
        public string LargeIconAndroid => _largeIconAndroid ?? string.Empty;
        public TimeSpan Delay => TimeSpan.FromMinutes(_delayMinutes);
        public int RepeatCycles => _repeatCycles;
        public TimeSpan RepeatInterval => TimeSpan.FromMinutes(_repeatIntervalMinutes);
        public bool ResetOnGameStart => _resetOnGameStart;
        public string ChannelId => _channelId ?? "default";
        public string ChannelName => _channelName ?? "Default";
        public string ChannelDescription => _channelDescription ?? string.Empty;
        public string Payload => _payload ?? string.Empty;
    }
}
