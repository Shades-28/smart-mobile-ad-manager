using NUnit.Framework;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    public class MobileKitSettingsTests
    {
        [TearDown]
        public void TearDown() => RateGameManager.ResetForTests();

        [Test]
        public void Apply_ConfiguresRateGame()
        {
            var settings = ScriptableObject.CreateInstance<MobileKitSettings>();
            try
            {
                // Apply with no rate-settings: should clear any prior config without throwing.
                Assert.DoesNotThrow(() => settings.Apply());
            }
            finally { Object.DestroyImmediate(settings); }
        }

        [Test]
        public void RemoveAdsProductId_FallsBackToDefault_WhenEmpty()
        {
            var settings = ScriptableObject.CreateInstance<MobileKitSettings>();
            try
            {
                Assert.AreEqual(RemoveAdsManager.DefaultProductId, settings.RemoveAdsProductId);
            }
            finally { Object.DestroyImmediate(settings); }
        }
    }
}
