using NUnit.Framework;

namespace Rinval.MobileAdsAndIapKit
{
    public class ConsentTests
    {
        [SetUp]
        public void Setup()
        {
            ConsentManager.Reset();
            AttHelper.OverrideForTests(null);
        }

        [TearDown]
        public void TearDown()
        {
            ConsentManager.Reset();
            AttHelper.OverrideForTests(null);
        }

        [Test]
        public void Ump_DefaultStatusIsUnknown()
        {
            Assert.AreEqual(ConsentStatus.Unknown, UmpHelper.Status);
        }

        [Test]
        public void Ump_RequestConsent_SetsNotRequired()
        {
            ConsentStatus? observed = null;
            UmpHelper.RequestConsent("https://example.com/privacy", s => observed = s);
            Assert.AreEqual(ConsentStatus.NotRequired, observed);
            Assert.AreEqual(ConsentStatus.NotRequired, UmpHelper.Status);
        }

        [Test]
        public void Ump_SetStatus_ChangesStatus()
        {
            UmpHelper.SetStatus(ConsentStatus.Obtained);
            Assert.AreEqual(ConsentStatus.Obtained, UmpHelper.Status);
            UmpHelper.SetStatus(ConsentStatus.Denied);
            Assert.AreEqual(ConsentStatus.Denied, UmpHelper.Status);
        }

        [Test]
        public void Att_OverrideToAuthorized_Reports()
        {
            AttHelper.OverrideForTests(AttStatus.Authorized);
            Assert.AreEqual(AttStatus.Authorized, AttHelper.CurrentStatus);
            Assert.IsTrue(AttHelper.IsSupported);
        }

        [Test]
        public void Att_OverrideToUnsupported_Reports()
        {
            AttHelper.OverrideForTests(AttStatus.Unsupported);
            Assert.AreEqual(AttStatus.Unsupported, AttHelper.CurrentStatus);
            Assert.IsFalse(AttHelper.IsSupported);
        }

        [Test]
        public void Att_RequestAuthorization_FiresCallback()
        {
            AttHelper.OverrideForTests(AttStatus.Authorized);
            AttStatus? observed = null;
            AttHelper.RequestAuthorization(s => observed = s);
            Assert.IsTrue(observed.HasValue);
        }

        [Test]
        public void Att_RequestAuthorization_WhenUnsupported_ReturnsUnsupported()
        {
            AttHelper.OverrideForTests(AttStatus.Unsupported);
            AttStatus? observed = null;
            AttHelper.RequestAuthorization(s => observed = s);
            Assert.AreEqual(AttStatus.Unsupported, observed);
        }

        [Test]
        public void Consent_Grant_SetsObtainedAndFiresEvent()
        {
            ConsentStatus? observed = null;
            ConsentManager.ConsentChanged += s => observed = s;
            ConsentManager.Grant();
            Assert.AreEqual(ConsentStatus.Obtained, observed);
            Assert.AreEqual(ConsentStatus.Obtained, ConsentManager.GdprStatus);
        }

        [Test]
        public void Consent_Deny_SetsDeniedAndFiresEvent()
        {
            ConsentStatus? observed = null;
            ConsentManager.ConsentChanged += s => observed = s;
            ConsentManager.Deny();
            Assert.AreEqual(ConsentStatus.Denied, observed);
        }

        [Test]
        public void Consent_AdsAllowed_FalseWhenDenied()
        {
            ConsentManager.Deny();
            Assert.IsFalse(ConsentManager.AdsAllowed);
        }

        [Test]
        public void Consent_AdsAllowed_TrueWhenObtained()
        {
            ConsentManager.Grant();
            Assert.IsTrue(ConsentManager.AdsAllowed);
        }

        [Test]
        public void Consent_RequestAll_FiresOnComplete()
        {
            bool done = false;
            ConsentManager.RequestAll("https://example.com/privacy", () => done = true);
            Assert.IsTrue(done);
        }

        [Test]
        public void Consent_Reset_ClearsStatusAndListeners()
        {
            ConsentManager.Grant();
            ConsentManager.Reset();
            Assert.AreEqual(ConsentStatus.Unknown, ConsentManager.GdprStatus);
        }
    }
}
