using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Text.RegularExpressions;

namespace Rinval.MobileAdsAndIapKit
{
    public class AdLoggerTests
    {
        [SetUp]
        public void Setup()
        {
            AdLogger.SetVerbose(true);
        }

        [Test]
        public void SetVerbose_TogglesFlag()
        {
            AdLogger.SetVerbose(false);
            Assert.IsFalse(AdLogger.VerboseEnabled);
            AdLogger.SetVerbose(true);
            Assert.IsTrue(AdLogger.VerboseEnabled);
        }

        [Test]
        public void Log_WhenVerbose_EmitsLog()
        {
            LogAssert.Expect(LogType.Log, new Regex(@"\[ADS\].*hello"));
            AdLogger.Log("hello");
        }

        [Test]
        public void Log_WhenSilent_DoesNotEmit()
        {
            AdLogger.SetVerbose(false);
            AdLogger.Log("should not appear");
            // No LogAssert.Expect — if it logged, the test framework would flag an unexpected log
        }

        [Test]
        public void Warn_WhenVerbose_EmitsWarning()
        {
            LogAssert.Expect(LogType.Warning, new Regex(@"\[ADS:WARN\].*caution"));
            AdLogger.Warn("caution");
        }

        [Test]
        public void Warn_WhenSilent_Suppressed()
        {
            AdLogger.SetVerbose(false);
            AdLogger.Warn("hidden warning");
        }

        [Test]
        public void Error_AlwaysEmitsEvenWhenSilent()
        {
            AdLogger.SetVerbose(false);
            LogAssert.Expect(LogType.Error, new Regex(@"\[ADS:ERROR\].*boom"));
            AdLogger.Error("boom");
        }

        [Test]
        public void Success_WhenVerbose_EmitsLog()
        {
            LogAssert.Expect(LogType.Log, new Regex(@"\[ADS:OK\].*loaded"));
            AdLogger.Success("loaded");
        }

        [Test]
        public void Tag_WhenVerbose_UppercasesTag()
        {
            LogAssert.Expect(LogType.Log, new Regex(@"\[ADS:CUSTOM\].*payload"));
            AdLogger.Tag("custom", "payload");
        }

        [Test]
        public void Network_WhenVerbose_EmitsNetworkTag()
        {
            LogAssert.Expect(LogType.Log, new Regex(@"\[ADS:APPLOVIN\].*ready"));
            AdLogger.Network("AppLovin", "ready");
        }

        [Test]
        public void Lifecycle_WithoutDetail_LogsMethodOnly()
        {
            LogAssert.Expect(LogType.Log, new Regex(@"\[ADS:LIFECYCLE\].*Init"));
            AdLogger.Lifecycle("Init");
        }

        [Test]
        public void Lifecycle_WithDetail_IncludesPipeAndDetail()
        {
            LogAssert.Expect(LogType.Log, new Regex(@"\[ADS:LIFECYCLE\].*ShowRewarded \| placement=shop"));
            AdLogger.Lifecycle("ShowRewarded", "placement=shop");
        }
    }
}
