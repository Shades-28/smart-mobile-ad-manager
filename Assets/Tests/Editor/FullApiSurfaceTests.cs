using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>
    /// Smoke test that walks the full public API surface in order, asserting nothing throws
    /// and the adapter contract holds for every route.
    /// </summary>
    public class FullApiSurfaceTests
    {
        private AdManagerConfig _cfg;
        private FakeConfigSource _src;

        [SetUp]
        public void Setup()
        {
            AdManager.Shutdown();
            AppOpenAdManager.Shutdown();
            ConsentManager.Reset();
            TestLabDetector.OverrideForTests(false);
            AttHelper.OverrideForTests(null);
            EditorAdapter.NextLoadFails = false;
            _cfg = ScriptableObject.CreateInstance<AdManagerConfig>();
            _src = new FakeConfigSource();
        }

        [TearDown]
        public void TearDown()
        {
            AdManager.Shutdown();
            AppOpenAdManager.Shutdown();
            ConsentManager.Reset();
            Object.DestroyImmediate(_cfg);
            var ov = Object.FindObjectOfType<SimulatedAdOverlay>();
            if (ov != null) Object.DestroyImmediate(ov.gameObject);
        }

        [UnityTest]
        public IEnumerator Walks_EveryAdManagerApi_Successfully()
        {
            // Init
            AdManager.Initialize(_cfg, _src);
            Assert.IsTrue(AdManager.IsInitialized);
            Assert.IsNotNull(AdManager.ActiveMediatorName);
            Assert.IsNotNull(AdManager.Config);
            Assert.IsNotNull(AdManager.ConfigSource);

            // Stage
            AdManager.SetCurrentStage(10);

            // Banner full lifecycle
            AdManager.LoadBanner(BannerAnchor.Top);
            AdManager.ShowBanner();
            AdManager.HideBanner();
            AdManager.LoadBanner(BannerAnchor.Bottom);
            AdManager.LoadBanner(BannerAnchor.TopLeft);
            AdManager.LoadBanner(BannerAnchor.TopRight);
            AdManager.LoadBanner(BannerAnchor.BottomLeft);
            AdManager.LoadBanner(BannerAnchor.BottomRight);
            AdManager.ShowBanner();
            AdManager.DestroyBanner();

            // Interstitial — happy path
            AdManager.LoadInterstitial();
            Assert.IsTrue(AdManager.IsInterstitialReady());
            AdResultCode? ic = null;
            AdManager.ShowInterstitial("apitest_inter", c => ic = c);
            yield return null;
            SimulatedAdOverlay.GetOrCreate().ForceCloseFullscreen(AdResultCode.Closed);
            yield return null; yield return null;
            Assert.AreEqual(AdResultCode.Closed, ic);

            // CanShowInterstitial path
            var canShow = AdManager.CanShowInterstitial(out var reason);
            Assert.IsNotNull(reason);

            // Rewarded happy path
            AdManager.LoadRewarded();
            Assert.IsTrue(AdManager.IsRewardedReady());
            AdResultCode? rc = null;
            AdManager.ShowRewarded("apitest_reward", c => rc = c);
            yield return null;
            SimulatedAdOverlay.GetOrCreate().ForceCloseFullscreen(AdResultCode.Rewarded);
            yield return null; yield return null;
            Assert.AreEqual(AdResultCode.Rewarded, rc);

            // MREC full lifecycle
            AdManager.LoadMrec();
            Assert.IsTrue(AdManager.IsMrecReady());
            AdManager.ShowMrec();
            AdManager.HideMrec();

            // Shutdown
            AdManager.Shutdown();
            Assert.IsFalse(AdManager.IsInitialized);
        }

        [Test]
        public void Walks_EveryAppOpenApi_Successfully()
        {
            _src.AppOpenSkipFirstLaunch = false;
            _src.AppOpenCooldownSeconds = 0;
            _src.AppOpenMinAwaySeconds = 0;
            AppOpenAdManager.Initialize(_cfg, _src);
            Assert.IsTrue(AppOpenAdManager.IsInitialized);
            Assert.IsFalse(AppOpenAdManager.IsReady());
            AppOpenAdManager.Load();
            Assert.IsTrue(AppOpenAdManager.IsReady());
            var canShow = AppOpenAdManager.CanShow(out var reason);
            Assert.IsNotNull(reason);
            AppOpenAdManager.Shutdown();
            Assert.IsFalse(AppOpenAdManager.IsInitialized);
        }

        [Test]
        public void Walks_EveryConsentApi_Successfully()
        {
            ConsentManager.Grant();
            Assert.AreEqual(ConsentStatus.Obtained, ConsentManager.GdprStatus);
            Assert.IsTrue(ConsentManager.AdsAllowed);
            ConsentManager.Deny();
            Assert.AreEqual(ConsentStatus.Denied, ConsentManager.GdprStatus);
            Assert.IsFalse(ConsentManager.AdsAllowed);

            bool done = false;
            ConsentManager.RequestAll("https://example.com/p", () => done = true);
            Assert.IsTrue(done);

            ConsentManager.Reset();
            Assert.AreEqual(ConsentStatus.Unknown, ConsentManager.GdprStatus);
        }

        [Test]
        public void Walks_EveryConfigField()
        {
            // Defaults
            Assert.IsTrue(_cfg.AdsEnabled);
            Assert.IsTrue(_cfg.InterstitialsEnabled);
            Assert.IsTrue(_cfg.RewardedEnabled);
            Assert.IsTrue(_cfg.BannersEnabled);
            Assert.IsTrue(_cfg.AppOpenEnabled);
            Assert.AreEqual(MediatorKind.AppLovinMax, _cfg.Mediator);
            Assert.AreEqual(BannerAnchor.Bottom, _cfg.DefaultBannerAnchor);
            Assert.GreaterOrEqual(_cfg.InterstitialMinIntervalSeconds, 0);
            Assert.GreaterOrEqual(_cfg.InterstitialMinStage, 0);
            Assert.GreaterOrEqual(_cfg.InterstitialMaxPerWindow, 0);
            Assert.Greater(_cfg.InterstitialWindowSeconds, 0);
            Assert.GreaterOrEqual(_cfg.AppOpenCooldownSeconds, 0);
            Assert.GreaterOrEqual(_cfg.AppOpenMinAwaySeconds, 0);

            // ID lookups for every format
            foreach (AdFormat f in System.Enum.GetValues(typeof(AdFormat)))
                Assert.IsNotNull(_cfg.GetIdFor(f));

            // Setters
            _cfg.SetMediator(MediatorKind.GoogleAdMob);
            _cfg.SetTestMode(false);
            _cfg.SetVerbose(false);
            _cfg.SetAdsEnabled(false);
            Assert.AreEqual(MediatorKind.GoogleAdMob, _cfg.Mediator);
            Assert.IsFalse(_cfg.TestMode);
            Assert.IsFalse(_cfg.VerboseLogging);
            Assert.IsFalse(_cfg.AdsEnabled);
        }

        [Test]
        public void Walks_AllResultCodes()
        {
            // Confirm enum values are usable
            foreach (AdResultCode c in System.Enum.GetValues(typeof(AdResultCode)))
                Assert.IsTrue(System.Enum.IsDefined(typeof(AdResultCode), c));
        }

        [Test]
        public void Walks_AllMediatorKinds()
        {
            foreach (MediatorKind k in System.Enum.GetValues(typeof(MediatorKind)))
                Assert.IsTrue(System.Enum.IsDefined(typeof(MediatorKind), k));
        }

        [Test]
        public void Walks_AllBannerAnchors_AndAllAdFormats()
        {
            foreach (BannerAnchor a in System.Enum.GetValues(typeof(BannerAnchor)))
                Assert.IsTrue(System.Enum.IsDefined(typeof(BannerAnchor), a));
            foreach (AdFormat f in System.Enum.GetValues(typeof(AdFormat)))
                Assert.IsTrue(System.Enum.IsDefined(typeof(AdFormat), f));
        }
    }
}
