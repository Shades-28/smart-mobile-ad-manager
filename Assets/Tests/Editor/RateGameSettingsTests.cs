using NUnit.Framework;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    public class RateGameSettingsTests
    {
        [SetUp]
        public void Setup() => RateGameManager.ResetForTests();
        [TearDown]
        public void TearDown() => RateGameManager.ResetForTests();

        [Test]
        public void Configure_AppliesAppOpensThreshold()
        {
            var settings = ScriptableObject.CreateInstance<RateGameSettings>();
            try
            {
                // Force defaults: 90 days / 3 per year. Tweak via reflection-free path is impossible
                // without exposing setters, so test via a settings asset with public-shaped defaults.
                RateGameManager.Configure(settings);

                // Default _appOpensBeforeFirst on the asset is 5. CanShow should be false until
                // we record 5 app-opens (no prompt has been shown yet so the cooldown is satisfied).
                Assert.IsFalse(RateGameManager.CanShow(), "Should be gated by AppOpensBeforeFirst");
                for (int i = 0; i < 5; i++) RateGameManager.RecordAppOpen();
                // Sessions threshold is 3 by default; record those too
                for (int i = 0; i < 3; i++) RateGameManager.RecordSessionEnd();
                Assert.IsTrue(RateGameManager.CanShow(), "After thresholds met, prompt should be allowed");
            }
            finally { Object.DestroyImmediate(settings); }
        }

        [Test]
        public void Configure_NullRestoresDefaults()
        {
            var settings = ScriptableObject.CreateInstance<RateGameSettings>();
            try
            {
                RateGameManager.Configure(settings);
                RateGameManager.Configure(null);
                // After null: thresholds = 0, so CanShow only depends on cooldown (no prior shows -> true)
                Assert.IsTrue(RateGameManager.CanShow());
            }
            finally { Object.DestroyImmediate(settings); }
        }
    }
}
