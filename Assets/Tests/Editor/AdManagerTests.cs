using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rinval.MobileAdsAndIapKit
{
    public class FakeConfigSource : IConfigSource
    {
        public bool AdsEnabled { get; set; } = true;
        public bool InterstitialsEnabled { get; set; } = true;
        public bool RewardedEnabled { get; set; } = true;
        public bool BannersEnabled { get; set; } = true;
        public bool AppOpenEnabled { get; set; } = true;
        public int InterstitialMinIntervalSeconds { get; set; } = 0;
        public int InterstitialMinStage { get; set; } = 0;
        public int InterstitialMaxPerWindow { get; set; } = 100;
        public int InterstitialWindowSeconds { get; set; } = 300;
        public bool SkipFirstInterstitial { get; set; } = false;
        public int AppOpenCooldownSeconds { get; set; } = 60;
        public int AppOpenMinAwaySeconds { get; set; } = 30;
        public bool AppOpenSkipFirstLaunch { get; set; } = true;
        public bool VerboseLogging { get; set; } = false;
        public bool TestMode { get; set; } = true;
        public IList<PlacementRule> PlacementRules { get; set; } = new List<PlacementRule>();
    }

    public class AdManagerTests
    {
        private AdManagerConfig _cfg;
        private FakeConfigSource _src;

        [SetUp]
        public void Setup()
        {
            AdManager.Shutdown();
            _cfg = ScriptableObject.CreateInstance<AdManagerConfig>();
            _src = new FakeConfigSource();
            TestLabDetector.OverrideForTests(false);
            EditorAdapter.NextLoadFails = false;
            EditorAdapter.NextRewardedResult = AdResultCode.Rewarded;
            EditorAdapter.NextInterstitialResult = AdResultCode.Closed;
        }

        [TearDown]
        public void TearDown()
        {
            AdManager.Shutdown();
            Object.DestroyImmediate(_cfg);
            TestLabDetector.OverrideForTests(null);
            var overlay = Object.FindObjectOfType<SimulatedAdOverlay>();
            if (overlay != null) Object.DestroyImmediate(overlay.gameObject);
        }

        [Test]
        public void Initialize_SetsState()
        {
            AdManager.Initialize(_cfg, _src);
            Assert.IsTrue(AdManager.IsInitialized);
            Assert.AreEqual(MediatorKind.Editor, AdManager.ActiveMediator);
            Assert.IsNotNull(AdManager.ActiveMediatorName);
        }

        [Test]
        public void Initialize_NullConfig_LogsErrorAndDoesNotInitialize()
        {
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(@"\[ADS:ERROR\]"));
            AdManager.Initialize(null);
            Assert.IsFalse(AdManager.IsInitialized);
        }

        [Test]
        public void Initialize_TwiceLogsWarning()
        {
            AdManager.Initialize(_cfg, _src);
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(@"\[ADS:WARN\]"));
            AdManager.Initialize(_cfg, _src);
            Assert.IsTrue(AdManager.IsInitialized);
        }

        [Test]
        public void Banner_RoutesToAdapter()
        {
            AdManager.Initialize(_cfg, _src);
            Assert.DoesNotThrow(() => AdManager.LoadBanner(BannerAnchor.Top));
            Assert.DoesNotThrow(() => AdManager.ShowBanner());
            Assert.DoesNotThrow(() => AdManager.HideBanner());
            Assert.DoesNotThrow(() => AdManager.DestroyBanner());
        }

        [Test]
        public void Banner_BannersDisabled_NoOps()
        {
            _src.BannersEnabled = false;
            AdManager.Initialize(_cfg, _src);
            Assert.DoesNotThrow(() => AdManager.LoadBanner(BannerAnchor.Bottom));
            Assert.DoesNotThrow(() => AdManager.ShowBanner());
        }

        [Test]
        public void IsInterstitialReady_FalseUntilLoaded()
        {
            AdManager.Initialize(_cfg, _src);
            Assert.IsFalse(AdManager.IsInterstitialReady());
            AdManager.LoadInterstitial();
            Assert.IsTrue(AdManager.IsInterstitialReady());
        }

        [Test]
        public void IsInterstitialReady_FalseWhenAdsDisabled()
        {
            _src.AdsEnabled = false;
            AdManager.Initialize(_cfg, _src);
            AdManager.LoadInterstitial();
            Assert.IsFalse(AdManager.IsInterstitialReady());
        }

        [UnityTest]
        public IEnumerator ShowInterstitial_HappyPath_FiresClosedAndCounts()
        {
            AdManager.Initialize(_cfg, _src);
            AdManager.LoadInterstitial();
            AdResultCode? code = null;
            AdManager.ShowInterstitial("level_end", c => code = c);

            yield return null;
            SimulatedAdOverlay.GetOrCreate().ForceCloseFullscreen(AdResultCode.Closed);
            yield return null;
            yield return null; // dispatcher

            Assert.AreEqual(AdResultCode.Closed, code);
            Assert.AreEqual(1, AdManager.InterstitialsShownTotal);
        }

        [Test]
        public void ShowInterstitial_AdsDisabled_ReturnsDisabled()
        {
            _src.AdsEnabled = false;
            AdManager.Initialize(_cfg, _src);
            AdResultCode? code = null;
            var ok = AdManager.ShowInterstitial("p", c => code = c);
            Assert.IsFalse(ok);
            Assert.AreEqual(AdResultCode.Disabled, code);
        }

        [UnityTest]
        public IEnumerator ShowInterstitial_IntervalCap_Blocks()
        {
            _src.InterstitialMinIntervalSeconds = 99999;
            AdManager.Initialize(_cfg, _src);
            AdManager.LoadInterstitial();
            AdManager.ShowInterstitial("p", null);
            yield return null;
            SimulatedAdOverlay.GetOrCreate().ForceCloseFullscreen(AdResultCode.Closed);
            yield return null; yield return null;

            AdManager.LoadInterstitial();
            AdResultCode? code = null;
            var ok = AdManager.ShowInterstitial("p2", c => code = c);
            Assert.IsFalse(ok);
            Assert.AreEqual(AdResultCode.NotReady, code);
        }

        [Test]
        public void ShowInterstitial_StageCap_Blocks()
        {
            _src.InterstitialMinStage = 5;
            AdManager.Initialize(_cfg, _src);
            AdManager.SetCurrentStage(2);
            AdManager.LoadInterstitial();
            AdResultCode? code = null;
            var ok = AdManager.ShowInterstitial("p", c => code = c);
            Assert.IsFalse(ok);
            Assert.AreEqual(AdResultCode.NotReady, code);
        }

        [Test]
        public void ShowInterstitial_SkipFirstInterstitial_BlocksFirstOnly()
        {
            _src.SkipFirstInterstitial = true;
            AdManager.Initialize(_cfg, _src);
            AdManager.LoadInterstitial();
            AdResultCode? code = null;
            var ok = AdManager.ShowInterstitial("p", c => code = c);
            Assert.IsFalse(ok);
            Assert.AreEqual(AdResultCode.NotReady, code);
        }

        [Test]
        public void CanShowInterstitial_ReportsReason_WhenBlocked()
        {
            _src.InterstitialsEnabled = false;
            AdManager.Initialize(_cfg, _src);
            var ok = AdManager.CanShowInterstitial(out var reason);
            Assert.IsFalse(ok);
            StringAssert.Contains("disabled", reason);
        }

        [UnityTest]
        public IEnumerator ShowRewarded_HappyPath_FiresRewarded()
        {
            AdManager.Initialize(_cfg, _src);
            AdManager.LoadRewarded();
            AdResultCode? code = null;
            AdManager.ShowRewarded("level", c => code = c);
            yield return null;
            SimulatedAdOverlay.GetOrCreate().ForceCloseFullscreen(AdResultCode.Rewarded);
            yield return null; yield return null;
            Assert.AreEqual(AdResultCode.Rewarded, code);
        }

        [Test]
        public void ShowRewarded_AdsDisabled_ReturnsDisabled()
        {
            _src.AdsEnabled = false;
            AdManager.Initialize(_cfg, _src);
            AdResultCode? code = null;
            AdManager.ShowRewarded("p", c => code = c);
            Assert.AreEqual(AdResultCode.Disabled, code);
        }

        [Test]
        public void ShowRewarded_NotReady_ReturnsNotReady()
        {
            AdManager.Initialize(_cfg, _src);
            AdResultCode? code = null;
            AdManager.ShowRewarded("p", c => code = c);
            Assert.AreEqual(AdResultCode.NotReady, code);
        }

        [Test]
        public void Mrec_LoadShowHide()
        {
            AdManager.Initialize(_cfg, _src);
            Assert.IsFalse(AdManager.IsMrecReady());
            AdManager.LoadMrec();
            Assert.IsTrue(AdManager.IsMrecReady());
            Assert.DoesNotThrow(() => AdManager.ShowMrec());
            Assert.DoesNotThrow(() => AdManager.HideMrec());
        }

        [UnityTest]
        public IEnumerator RevenuePaid_FiresOnInterstitialClosed()
        {
            AdManager.Initialize(_cfg, _src);
            var revenues = new List<AdRevenueInfo>();
            AdManager.RevenuePaid += info => revenues.Add(info);
            AdManager.LoadInterstitial();
            AdManager.ShowInterstitial("rev_test", null);
            yield return null;
            SimulatedAdOverlay.GetOrCreate().ForceCloseFullscreen(AdResultCode.Closed);
            yield return null; yield return null;
            Assert.GreaterOrEqual(revenues.Count, 1);
            var info = revenues.Find(r => r.Format == AdFormat.Interstitial);
            Assert.AreEqual("rev_test", info.Placement);
        }

        [UnityTest]
        public IEnumerator InterstitialClosed_EventFires()
        {
            AdManager.Initialize(_cfg, _src);
            AdResultCode? closed = null;
            AdManager.InterstitialClosed += c => closed = c;
            AdManager.LoadInterstitial();
            AdManager.ShowInterstitial("p", null);
            yield return null;
            SimulatedAdOverlay.GetOrCreate().ForceCloseFullscreen(AdResultCode.Closed);
            yield return null; yield return null;
            Assert.AreEqual(AdResultCode.Closed, closed);
        }

        [UnityTest]
        public IEnumerator RewardedClosed_EventFires()
        {
            AdManager.Initialize(_cfg, _src);
            AdResultCode? r = null;
            AdManager.RewardedClosed += c => r = c;
            AdManager.LoadRewarded();
            AdManager.ShowRewarded("p", null);
            yield return null;
            SimulatedAdOverlay.GetOrCreate().ForceCloseFullscreen(AdResultCode.Rewarded);
            yield return null; yield return null;
            Assert.AreEqual(AdResultCode.Rewarded, r);
        }

        [Test]
        public void TestLab_ForcesStubAdapter()
        {
            TestLabDetector.OverrideForTests(true);
            AdManager.Initialize(_cfg, _src);
            Assert.AreEqual(MediatorKind.None, AdManager.ActiveMediator);
        }

        [Test]
        public void Shutdown_ResetsState()
        {
            AdManager.Initialize(_cfg, _src);
            Assert.IsTrue(AdManager.IsInitialized);
            AdManager.Shutdown();
            Assert.IsFalse(AdManager.IsInitialized);
            Assert.AreEqual(MediatorKind.None, AdManager.ActiveMediator);
        }

        [Test]
        public void UninitializedCalls_LogErrorButDoNotThrow()
        {
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(@"\[ADS:ERROR\]"));
            Assert.DoesNotThrow(() => AdManager.LoadBanner(BannerAnchor.Top));
        }
    }
}
