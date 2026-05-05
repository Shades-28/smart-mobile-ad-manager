using NUnit.Framework;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    public class AdManagerConfigTests
    {
        private AdManagerConfig _cfg;

        [SetUp]
        public void Setup()
        {
            _cfg = ScriptableObject.CreateInstance<AdManagerConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_cfg);
        }

        [Test]
        public void Defaults_AdsEnabled()
        {
            Assert.IsTrue(_cfg.AdsEnabled);
            Assert.IsTrue(_cfg.InterstitialsEnabled);
            Assert.IsTrue(_cfg.RewardedEnabled);
            Assert.IsTrue(_cfg.BannersEnabled);
            Assert.IsTrue(_cfg.AppOpenEnabled);
        }

        [Test]
        public void Defaults_FrequencyCaps()
        {
            Assert.AreEqual(60, _cfg.InterstitialMinIntervalSeconds);
            Assert.AreEqual(0, _cfg.InterstitialMinStage);
            Assert.AreEqual(5, _cfg.InterstitialMaxPerWindow);
            Assert.AreEqual(300, _cfg.InterstitialWindowSeconds);
            Assert.IsTrue(_cfg.SkipFirstInterstitial);
        }

        [Test]
        public void Defaults_AppOpen()
        {
            Assert.AreEqual(60, _cfg.AppOpenCooldownSeconds);
            Assert.AreEqual(30, _cfg.AppOpenMinAwaySeconds);
            Assert.IsTrue(_cfg.AppOpenSkipFirstLaunch);
        }

        [Test]
        public void Defaults_DiagnosticsAndMediator()
        {
            Assert.IsTrue(_cfg.VerboseLogging);
            Assert.IsTrue(_cfg.TestMode);
            Assert.AreEqual(MediatorKind.AppLovinMax, _cfg.Mediator);
            Assert.AreEqual(BannerAnchor.Bottom, _cfg.DefaultBannerAnchor);
        }

        [Test]
        public void GetIdFor_AllFormats_ReturnsNonNull()
        {
            // Defaults are empty strings, but never null
            Assert.IsNotNull(_cfg.GetIdFor(AdFormat.Banner));
            Assert.IsNotNull(_cfg.GetIdFor(AdFormat.Interstitial));
            Assert.IsNotNull(_cfg.GetIdFor(AdFormat.Rewarded));
            Assert.IsNotNull(_cfg.GetIdFor(AdFormat.MediumRectangle));
            Assert.IsNotNull(_cfg.GetIdFor(AdFormat.AppOpen));
        }

        [Test]
        public void GetIdFor_UnknownFormat_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, _cfg.GetIdFor((AdFormat)999));
        }

        [Test]
        public void Setters_TestMode_TogglesProperty()
        {
            _cfg.SetTestMode(false);
            Assert.IsFalse(_cfg.TestMode);
            _cfg.SetTestMode(true);
            Assert.IsTrue(_cfg.TestMode);
        }

        [Test]
        public void Setters_Verbose_TogglesProperty()
        {
            _cfg.SetVerbose(false);
            Assert.IsFalse(_cfg.VerboseLogging);
            _cfg.SetVerbose(true);
            Assert.IsTrue(_cfg.VerboseLogging);
        }

        [Test]
        public void Setters_AdsEnabled_TogglesProperty()
        {
            _cfg.SetAdsEnabled(false);
            Assert.IsFalse(_cfg.AdsEnabled);
            _cfg.SetAdsEnabled(true);
            Assert.IsTrue(_cfg.AdsEnabled);
        }

        [Test]
        public void Setters_Mediator_ChangesMediator()
        {
            _cfg.SetMediator(MediatorKind.GoogleAdMob);
            Assert.AreEqual(MediatorKind.GoogleAdMob, _cfg.Mediator);
            _cfg.SetMediator(MediatorKind.UnityLevelPlay);
            Assert.AreEqual(MediatorKind.UnityLevelPlay, _cfg.Mediator);
        }

        [Test]
        public void IConfigSource_ContractIsHonored()
        {
            IConfigSource src = _cfg;
            // Touching every member of the interface to ensure binding compiles
            Assert.IsTrue(src.AdsEnabled);
            Assert.IsTrue(src.InterstitialsEnabled);
            Assert.IsTrue(src.RewardedEnabled);
            Assert.IsTrue(src.BannersEnabled);
            Assert.IsTrue(src.AppOpenEnabled);
            Assert.AreEqual(60, src.InterstitialMinIntervalSeconds);
            Assert.AreEqual(0, src.InterstitialMinStage);
            Assert.AreEqual(5, src.InterstitialMaxPerWindow);
            Assert.AreEqual(300, src.InterstitialWindowSeconds);
            Assert.IsTrue(src.SkipFirstInterstitial);
            Assert.AreEqual(60, src.AppOpenCooldownSeconds);
            Assert.AreEqual(30, src.AppOpenMinAwaySeconds);
            Assert.IsTrue(src.AppOpenSkipFirstLaunch);
            Assert.IsTrue(src.VerboseLogging);
            Assert.IsTrue(src.TestMode);
            Assert.IsNotNull(src.PlacementRules);
        }
    }
}
