using UnityEngine;
using UnityEngine.UI;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Drop-on-button trigger that schedules a NotificationDefinition. Designer drags an asset in, ticks "Cancel On Schedule" if they want to dedup. No code needed.</summary>
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("Mobile Ads & IAP Kit/Schedule Notification Button")]
    public class ScheduleNotificationButton : MonoBehaviour
    {
        [SerializeField] private NotificationDefinition _definition;

        [Tooltip("If true, cancels any prior notification with the same Id before re-scheduling.")]
        [SerializeField] private bool _cancelBeforeSchedule = true;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(Trigger);
        }

        public void Trigger()
        {
            if (_definition == null) { AdLogger.Warn($"ScheduleNotificationButton on '{name}' has no definition"); return; }
            if (_cancelBeforeSchedule) NotificationsManager.Cancel(_definition.Id);
            NotificationsManager.Schedule(_definition);
        }
    }
}
