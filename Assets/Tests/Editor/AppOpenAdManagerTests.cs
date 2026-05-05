using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rinval.MobileAdsAndIapKit
{
    public class AppOpenAdManagerTests
    {
        private AdManagerConfig _cfg;
        private FakeConfigSource _src;

        [SetUp]
        public void Setup()
        {
            AppOpenAdManager.Shutdown();
            _cfg = ScriptableObject.CreateInstance<AdManagerConfig>();
            _src = new FakeConfigSource
            {
                AppOpenSkipFirstLaunch = false,
                AppOpenCooldownSeconds = 0,
                AppOpenMinAwaySeconds = 0,
            };
        }

        [TearDown]
        public void TearDown()
        {
            AppOpenAdManager.Shutdown();
            Object.DestroyImmediate(_cfg);
        }

        [Test]
        public void Initialize_SetsState()
        {
            AppOpenAdManager.Initialize(_cfg, _src);
            Assert.IsTrue(AppOpenAdManager.IsInitialized);
            Assert.AreEqual(MediatorKind.Editor, AppOpenAdManager.ActiveMediator);
        }

        [Test]
        public void Initialize_NullConfig_LogsError()
        {
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(@"\[ADS:ERROR\]"));
            AppOpenAdManager.Initialize(null);
            Assert.IsFalse(AppOpenAdManager.IsInitialized);
        }

        [Test]
        public void IsReady_FalseUntilLoaded()
        {
            AppOpenAdManager.Initialize(_cfg, _src);
            Assert.IsFalse(AppOpenAdManager.IsReady());
            AppOpenAdManager.Load();
            Assert.IsTrue(AppOpenAdManager.IsReady());
        }

        [UnityTest]
        public IEnumerator TryShow_HappyPath_ReturnsClosed()
        {
            AppOpenAdManager.Initialize(_cfg, _src);
            AppOpenAdManager.Load();
            AdResultCode? code = null;
            var ok = AppOpenAdManager.TryShow(c => code = c);
            yield return null;
            yield return null;
            Assert.IsTrue(ok);
            Assert.AreEqual(AdResultCode.Closed, code);
        }

        [Test]
        public void TryShow_AdsDisabled_ReturnsDisabled()
        {
            _src.AdsEnabled = false;
            AppOpenAdManager.Initialize(_cfg, _src);
            AdResultCode? code = null;
            var ok = AppOpenAdManager.TryShow(c => code = c);
            Assert.IsFalse(ok);
            Assert.AreEqual(AdResultCode.Disabled, code);
        }

        [Test]
        public void CanShow_WhenAppOpenDisabled_BlocksWithReason()
        {
            _src.AppOpenEnabled = false;
            AppOpenAdManager.Initialize(_cfg, _src);
            var ok = AppOpenAdManager.CanShow(out var reason);
            Assert.IsFalse(ok);
            StringAssert.Contains("disabled", reason);
        }

        [Test]
        public void CanShow_FirstLaunchSkipped_BlocksWithReason()
        {
            _src.AppOpenSkipFirstLaunch = true;
            AppOpenAdManager.Initialize(_cfg, _src);
            var ok = AppOpenAdManager.CanShow(out var reason);
            Assert.IsFalse(ok);
            StringAssert.Contains("first launch", reason);
        }

        [Test]
        public void Shutdown_ResetsState()
        {
            AppOpenAdManager.Initialize(_cfg, _src);
            AppOpenAdManager.Shutdown();
            Assert.IsFalse(AppOpenAdManager.IsInitialized);
        }
    }
}
