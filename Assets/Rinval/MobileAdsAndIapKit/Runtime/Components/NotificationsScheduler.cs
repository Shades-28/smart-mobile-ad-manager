using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Drop-on-GameObject scheduler. Holds an array of NotificationDefinitions, fires them on the chosen trigger. Designer drags assets into the array, picks trigger, never writes scheduling code.</summary>
    [AddComponentMenu("Mobile Ads & IAP Kit/Notifications Scheduler")]
    public class NotificationsScheduler : MonoBehaviour
    {
        public enum Trigger
        {
            OnAppStart,    // Awake() - fire as soon as the scheduler loads
            OnAppQuit,     // OnApplicationQuit / pause(true) - retention notifications
            Manual,        // call ScheduleAll() / Schedule(index) from code or a UnityEvent
        }

        [Serializable]
        public class Slot
        {
            [SerializeField] public NotificationDefinition Definition;
            [SerializeField] public Trigger TriggerOn = Trigger.OnAppQuit;
            [SerializeField] public bool CancelBeforeSchedule = true;
        }

        [Tooltip("Designer fills these. Each slot has its own trigger.")]
        [SerializeField] private List<Slot> _slots = new List<Slot>();

        [Tooltip("If true, requests notification permission on Awake (no-op in editor).")]
        [SerializeField] private bool _requestPermissionOnStart = true;

        public IReadOnlyList<Slot> Slots => _slots;

        private void Awake()
        {
            if (_requestPermissionOnStart) NotificationsManager.RequestPermission(null);
            FireMatching(Trigger.OnAppStart);
        }

        private void OnApplicationQuit() => FireMatching(Trigger.OnAppQuit);
        private void OnApplicationPause(bool paused)
        {
            if (paused) FireMatching(Trigger.OnAppQuit);
        }

        public void ScheduleAll()
        {
            foreach (var s in _slots) ScheduleSlot(s);
        }

        public void Schedule(int index)
        {
            if (index < 0 || index >= _slots.Count) return;
            ScheduleSlot(_slots[index]);
        }

        public void CancelAll()
        {
            foreach (var s in _slots)
                if (s != null && s.Definition != null) NotificationsManager.Cancel(s.Definition.Id);
        }

        private void FireMatching(Trigger t)
        {
            foreach (var s in _slots)
                if (s != null && s.TriggerOn == t) ScheduleSlot(s);
        }

        private static void ScheduleSlot(Slot s)
        {
            if (s == null || s.Definition == null) return;
            if (s.CancelBeforeSchedule) NotificationsManager.Cancel(s.Definition.Id);
            NotificationsManager.Schedule(s.Definition);
        }
    }
}
