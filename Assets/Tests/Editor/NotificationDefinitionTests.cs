using NUnit.Framework;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    public class NotificationDefinitionTests
    {
        [Test]
        public void Defaults_AreUsable()
        {
            var def = ScriptableObject.CreateInstance<NotificationDefinition>();
            try
            {
                Assert.IsFalse(string.IsNullOrEmpty(def.Title));
                Assert.IsFalse(string.IsNullOrEmpty(def.Body));
                Assert.AreEqual(0, def.RepeatCycles);
                Assert.IsTrue(def.Delay.TotalMinutes >= 1);
                Assert.IsTrue(def.ResetOnGameStart);
                Assert.IsFalse(string.IsNullOrEmpty(def.ChannelId));
            }
            finally { Object.DestroyImmediate(def); }
        }

        [Test]
        public void Id_FallsBackToAssetName_WhenIdEmpty()
        {
            var def = ScriptableObject.CreateInstance<NotificationDefinition>();
            try
            {
                def.name = "DailyReward";
                Assert.AreEqual("DailyReward", def.Id);
            }
            finally { Object.DestroyImmediate(def); }
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator Schedule_FiresEvent()
        {
            var def = ScriptableObject.CreateInstance<NotificationDefinition>();
            try
            {
                NotificationDefinition received = null;
                NotificationsManager.ScheduleDefinitionRequested += d => received = d;
                NotificationsManager.Schedule(def);
                yield return null;
                Assert.AreSame(def, received);
            }
            finally { Object.DestroyImmediate(def); }
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator Cancel_FiresEvent_WithId()
        {
            string received = null;
            NotificationsManager.CancelRequested += id => received = id;
            NotificationsManager.Cancel("daily_reward");
            yield return null;
            Assert.AreEqual("daily_reward", received);
        }
    }
}
