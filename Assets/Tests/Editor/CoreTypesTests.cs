using NUnit.Framework;

namespace Rinval.MobileAdsAndIapKit
{
    public class CoreTypesTests
    {
        [Test]
        public void AdResultCode_AllValuesAreDistinct()
        {
            var values = System.Enum.GetValues(typeof(AdResultCode));
            var set = new System.Collections.Generic.HashSet<int>();
            foreach (var v in values)
                Assert.IsTrue(set.Add((int)v), $"Duplicate value for {v}");
            Assert.AreEqual(10, values.Length, "AdResultCode should have exactly 10 members");
        }

        [Test]
        public void AdFormat_HasExpectedMembers()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(AdFormat), AdFormat.Banner));
            Assert.IsTrue(System.Enum.IsDefined(typeof(AdFormat), AdFormat.Interstitial));
            Assert.IsTrue(System.Enum.IsDefined(typeof(AdFormat), AdFormat.Rewarded));
            Assert.IsTrue(System.Enum.IsDefined(typeof(AdFormat), AdFormat.MediumRectangle));
            Assert.IsTrue(System.Enum.IsDefined(typeof(AdFormat), AdFormat.AppOpen));
            Assert.AreEqual(7, System.Enum.GetValues(typeof(AdFormat)).Length);
            Assert.IsTrue(System.Enum.IsDefined(typeof(AdFormat), AdFormat.RewardedInterstitial));
            Assert.IsTrue(System.Enum.IsDefined(typeof(AdFormat), AdFormat.Native));
        }

        [Test]
        public void BannerAnchor_HasSixPositions()
        {
            Assert.AreEqual(6, System.Enum.GetValues(typeof(BannerAnchor)).Length);
        }

        [Test]
        public void MediatorKind_HasExpectedMembers()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(MediatorKind), MediatorKind.None));
            Assert.IsTrue(System.Enum.IsDefined(typeof(MediatorKind), MediatorKind.Editor));
            Assert.IsTrue(System.Enum.IsDefined(typeof(MediatorKind), MediatorKind.AppLovinMax));
            Assert.IsTrue(System.Enum.IsDefined(typeof(MediatorKind), MediatorKind.GoogleAdMob));
            Assert.IsTrue(System.Enum.IsDefined(typeof(MediatorKind), MediatorKind.UnityLevelPlay));
        }

        [Test]
        public void AdRevenueInfo_StoresAllFields()
        {
            var info = new AdRevenueInfo(
                AdFormat.Rewarded, "ad-unit-123", "level_complete", "AppLovin", "USD", 0.0123);

            Assert.AreEqual(AdFormat.Rewarded, info.Format);
            Assert.AreEqual("ad-unit-123", info.AdUnitId);
            Assert.AreEqual("level_complete", info.Placement);
            Assert.AreEqual("AppLovin", info.NetworkName);
            Assert.AreEqual("USD", info.Currency);
            Assert.AreEqual(0.0123, info.Amount, 1e-9);
        }

        [Test]
        public void AdRevenueInfo_NullStringsBecomeEmpty()
        {
            var info = new AdRevenueInfo(AdFormat.Banner, null, null, null, null, 0.0);
            Assert.AreEqual(string.Empty, info.AdUnitId);
            Assert.AreEqual(string.Empty, info.Placement);
            Assert.AreEqual(string.Empty, info.NetworkName);
            Assert.AreEqual("USD", info.Currency, "Null currency should default to USD");
        }

        [Test]
        public void AdRevenueInfo_EmptyCurrencyDefaultsToUsd()
        {
            var info = new AdRevenueInfo(AdFormat.Banner, "u", "p", "n", "", 0.0);
            Assert.AreEqual("USD", info.Currency);
        }

        [Test]
        public void AdRevenueInfo_ToStringContainsKeyFields()
        {
            var info = new AdRevenueInfo(
                AdFormat.Interstitial, "u", "shop", "AdMob", "EUR", 1.23);
            var s = info.ToString();
            StringAssert.Contains("Interstitial", s);
            StringAssert.Contains("AdMob", s);
            StringAssert.Contains("shop", s);
            StringAssert.Contains("EUR", s);
        }

        [Test]
        public void AdLoadFailure_StoresAllFields()
        {
            var f = new AdLoadFailure(AdFormat.Rewarded, "MAX", -1, "no fill");
            Assert.AreEqual(AdFormat.Rewarded, f.Format);
            Assert.AreEqual("MAX", f.NetworkName);
            Assert.AreEqual(-1, f.ErrorCode);
            Assert.AreEqual("no fill", f.Message);
            StringAssert.Contains("Rewarded", f.ToString());
            StringAssert.Contains("MAX", f.ToString());
            StringAssert.Contains("no fill", f.ToString());
        }

        [Test]
        public void AdLoadFailure_NullsBecomeEmpty()
        {
            var f = new AdLoadFailure(AdFormat.Banner, null, 0, null);
            Assert.AreEqual(string.Empty, f.NetworkName);
            Assert.AreEqual(string.Empty, f.Message);
        }
    }
}
