using NUnit.Framework;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    public class StubAdapterTests
    {
        private AdManagerConfig _cfg;
        private StubAdapter _adapter;

        [SetUp]
        public void Setup()
        {
            _cfg = ScriptableObject.CreateInstance<AdManagerConfig>();
            _adapter = new StubAdapter();
            _adapter.Initialize(_cfg, _ => { });
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_cfg);

        [Test]
        public void Kind_IsNone() => Assert.AreEqual(MediatorKind.None, _adapter.Kind);

        [Test]
        public void IsInitialized_AfterInit() => Assert.IsTrue(_adapter.IsInitialized);

        [Test]
        public void DisplayName_NotEmpty() => Assert.IsNotEmpty(_adapter.DisplayName);

        [Test]
        public void Banner_AllOps_DoNotThrow()
        {
            Assert.DoesNotThrow(() => _adapter.LoadBanner(BannerAnchor.Top));
            Assert.DoesNotThrow(() => _adapter.ShowBanner());
            Assert.DoesNotThrow(() => _adapter.HideBanner());
            Assert.DoesNotThrow(() => _adapter.DestroyBanner());
        }

        [Test]
        public void Interstitial_NotReady_AndShowReturnsDisabled()
        {
            _adapter.LoadInterstitial(null);
            Assert.IsFalse(_adapter.IsInterstitialReady());
            AdResultCode? result = null;
            _adapter.ShowInterstitial("p", c => result = c);
            Assert.AreEqual(AdResultCode.Disabled, result);
        }

        [Test]
        public void Rewarded_NotReady_AndShowReturnsDisabled()
        {
            _adapter.LoadRewarded(null);
            Assert.IsFalse(_adapter.IsRewardedReady());
            AdResultCode? result = null;
            _adapter.ShowRewarded("p", c => result = c);
            Assert.AreEqual(AdResultCode.Disabled, result);
        }

        [Test]
        public void Mrec_NotReady_OpsDoNotThrow()
        {
            _adapter.LoadMrec(null);
            Assert.IsFalse(_adapter.IsMrecReady());
            Assert.DoesNotThrow(() => _adapter.ShowMrec());
            Assert.DoesNotThrow(() => _adapter.HideMrec());
        }

        [Test]
        public void OnApplicationPause_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _adapter.OnApplicationPause(true));
            Assert.DoesNotThrow(() => _adapter.OnApplicationPause(false));
        }

        [Test]
        public void NullCallback_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _adapter.ShowInterstitial("p", null));
            Assert.DoesNotThrow(() => _adapter.ShowRewarded("p", null));
        }
    }
}
