using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rinval.MobileAdsAndIapKit
{
    public class MobileKitTests
    {
        private AdManagerConfig _adConfig;

        [SetUp]
        public void Setup()
        {
            _ = MainThreadDispatcher.Instance; // warm
            MobileKit.Shutdown();
            _adConfig = ScriptableObject.CreateInstance<AdManagerConfig>();
            TestLabDetector.OverrideForTests(false);
        }

        [TearDown]
        public void TearDown()
        {
            MobileKit.Shutdown();
            Object.DestroyImmediate(_adConfig);
            TestLabDetector.OverrideForTests(null);
        }

        [UnityTest]
        public IEnumerator Initialize_NullConfig_FailsCleanly()
        {
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(@"\[ADS:ERROR\].*null config"));
            bool? ok = null;
            MobileKit.Initialize(null, r => ok = r);
            yield return null;
            Assert.IsFalse(ok ?? true);
            Assert.IsFalse(MobileKit.IsReady);
        }

        [UnityTest]
        public IEnumerator Initialize_RequiresAdConfig()
        {
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(@"AdConfig is required"));
            bool? ok = null;
            MobileKit.Initialize(new KitConfig(), r => ok = r);
            yield return null;
            Assert.IsFalse(ok ?? true);
        }

        [UnityTest]
        public IEnumerator Initialize_AdsOnly_Succeeds()
        {
            bool? ok = null;
            MobileKit.Initialize(new KitConfig { AdConfig = _adConfig }, r => ok = r);
            yield return null;
            yield return null;
            Assert.IsTrue(ok ?? false);
            Assert.IsTrue(MobileKit.IsReady);
            Assert.IsTrue(AdManager.IsInitialized);
        }

        [UnityTest]
        public IEnumerator Initialize_FiresReadyEvent()
        {
            bool fired = false;
            MobileKit.Ready += () => fired = true;
            MobileKit.Initialize(new KitConfig { AdConfig = _adConfig });
            yield return null;
            Assert.IsTrue(fired);
        }
    }
}
