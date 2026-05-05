using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rinval.MobileAdsAndIapKit
{
    public class EditorAdapterTests
    {
        private AdManagerConfig _cfg;
        private EditorAdapter _adapter;
        private System.Collections.Generic.List<AdRevenueInfo> _revenueLog;

        [SetUp]
        public void Setup()
        {
            _cfg = ScriptableObject.CreateInstance<AdManagerConfig>();
            _adapter = new EditorAdapter();
            _revenueLog = new System.Collections.Generic.List<AdRevenueInfo>();
            _adapter.Initialize(_cfg, info => _revenueLog.Add(info));
            EditorAdapter.NextLoadFails = false;
            EditorAdapter.NextRewardedResult = AdResultCode.Rewarded;
            EditorAdapter.NextInterstitialResult = AdResultCode.Closed;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_cfg);
            var overlay = SimulatedAdOverlay.GetOrCreate();
            if (overlay != null) Object.DestroyImmediate(overlay.gameObject);
        }

        [Test]
        public void Kind_IsEditor()
        {
            Assert.AreEqual(MediatorKind.Editor, _adapter.Kind);
        }

        [Test]
        public void Initialize_SetsIsInitialized()
        {
            Assert.IsTrue(_adapter.IsInitialized);
        }

        [Test]
        public void DisplayName_NotEmpty()
        {
            Assert.IsNotEmpty(_adapter.DisplayName);
        }

        [Test]
        public void Banner_LoadShowHide_Works()
        {
            _adapter.LoadBanner(BannerAnchor.Top);
            _adapter.ShowBanner();
            Assert.AreEqual(1, _revenueLog.Count, "Show should fire revenue once");
            _adapter.HideBanner();
            _adapter.DestroyBanner();
        }

        [Test]
        public void Interstitial_LoadAndReady()
        {
            Assert.IsFalse(_adapter.IsInterstitialReady());
            _adapter.LoadInterstitial(null);
            Assert.IsTrue(_adapter.IsInterstitialReady());
        }

        [Test]
        public void Interstitial_LoadFailsWhenForcedToFail()
        {
            EditorAdapter.NextLoadFails = true;
            _adapter.LoadInterstitial(null);
            Assert.IsFalse(_adapter.IsInterstitialReady());
        }

        [UnityTest]
        public IEnumerator Interstitial_Show_FiresClosedAndRevenue()
        {
            _adapter.LoadInterstitial(null);
            AdResultCode? result = null;
            EditorAdapter.NextInterstitialResult = AdResultCode.Closed;
            _adapter.ShowInterstitial("test_placement", code => result = code);

            yield return null;
            SimulatedAdOverlay.GetOrCreate().ForceCloseFullscreen(AdResultCode.Closed);
            yield return null;

            Assert.AreEqual(AdResultCode.Closed, result);
            Assert.AreEqual(1, _revenueLog.Count);
            Assert.AreEqual(AdFormat.Interstitial, _revenueLog[0].Format);
            Assert.AreEqual("test_placement", _revenueLog[0].Placement);
        }

        [UnityTest]
        public IEnumerator Interstitial_NotReady_ReturnsNotReady()
        {
            AdResultCode? result = null;
            _adapter.ShowInterstitial("p", c => result = c);
            yield return null;
            Assert.AreEqual(AdResultCode.NotReady, result);
        }

        [UnityTest]
        public IEnumerator Rewarded_Show_FiresRewardedAndRevenue()
        {
            _adapter.LoadRewarded(null);
            AdResultCode? result = null;
            EditorAdapter.NextRewardedResult = AdResultCode.Rewarded;
            _adapter.ShowRewarded("level_complete", code => result = code);

            yield return null;
            SimulatedAdOverlay.GetOrCreate().ForceCloseFullscreen(AdResultCode.Rewarded);
            yield return null;

            Assert.AreEqual(AdResultCode.Rewarded, result);
            Assert.AreEqual(1, _revenueLog.Count);
            Assert.AreEqual(AdFormat.Rewarded, _revenueLog[0].Format);
        }

        [UnityTest]
        public IEnumerator Rewarded_Cancelled_DoesNotFireRevenue()
        {
            _adapter.LoadRewarded(null);
            AdResultCode? result = null;
            EditorAdapter.NextRewardedResult = AdResultCode.Cancelled;
            _adapter.ShowRewarded("p", c => result = c);
            yield return null;
            SimulatedAdOverlay.GetOrCreate().ForceCloseFullscreen(AdResultCode.Cancelled);
            yield return null;

            Assert.AreEqual(AdResultCode.Cancelled, result);
            Assert.AreEqual(0, _revenueLog.Count, "Cancelled rewarded should not fire revenue");
        }

        [UnityTest]
        public IEnumerator Rewarded_NotReady_Returns()
        {
            AdResultCode? result = null;
            _adapter.ShowRewarded("p", c => result = c);
            yield return null;
            Assert.AreEqual(AdResultCode.NotReady, result);
        }

        [Test]
        public void Mrec_LoadShowHide()
        {
            Assert.IsFalse(_adapter.IsMrecReady());
            _adapter.LoadMrec(null);
            Assert.IsTrue(_adapter.IsMrecReady());
            _adapter.ShowMrec();
            Assert.AreEqual(1, _revenueLog.Count);
            _adapter.HideMrec();
        }

        [Test]
        public void OnApplicationPause_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _adapter.OnApplicationPause(true));
            Assert.DoesNotThrow(() => _adapter.OnApplicationPause(false));
        }
    }
}
