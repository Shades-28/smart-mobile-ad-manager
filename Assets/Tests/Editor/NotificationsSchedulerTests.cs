using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    public class NotificationsSchedulerTests
    {
        [Test]
        public void ScheduleAll_FiresEventForEverySlot()
        {
            var go = new GameObject("scheduler", typeof(NotificationsScheduler));
            try
            {
                var scheduler = go.GetComponent<NotificationsScheduler>();
                var slot1 = new NotificationsScheduler.Slot { Definition = ScriptableObject.CreateInstance<NotificationDefinition>() };
                var slot2 = new NotificationsScheduler.Slot { Definition = ScriptableObject.CreateInstance<NotificationDefinition>() };
                slot1.Definition.name = "DailyReward";
                slot2.Definition.name = "EnergyFull";
                var slots = new List<NotificationsScheduler.Slot> { slot1, slot2 };
                var f = typeof(NotificationsScheduler).GetField("_slots", BindingFlags.NonPublic | BindingFlags.Instance);
                f.SetValue(scheduler, slots);

                int fired = 0;
                NotificationsManager.ScheduleDefinitionRequested += _ => fired++;
                scheduler.ScheduleAll();
                Assert.AreEqual(2, fired);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void Schedule_IndexOutOfRange_DoesNotThrow()
        {
            var go = new GameObject("scheduler", typeof(NotificationsScheduler));
            try
            {
                var scheduler = go.GetComponent<NotificationsScheduler>();
                Assert.DoesNotThrow(() => scheduler.Schedule(99));
                Assert.DoesNotThrow(() => scheduler.Schedule(-1));
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
