using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rinval.MobileAdsAndIapKit
{
    public class ConfigValidatorTests
    {
        [Test]
        public void NullConfig_LogsError()
        {
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(@"No config"));
            ConfigValidator.Validate(null);
        }

        [Test]
        public void DefaultConfig_LogsWarningsForEmptyIds()
        {
            var cfg = ScriptableObject.CreateInstance<AdManagerConfig>();
            ConfigValidator.Validate(cfg); // expected to warn but not error
            Object.DestroyImmediate(cfg);
        }

        [Test]
        public void DisabledMediator_WarnsButDoesNotError()
        {
            var cfg = ScriptableObject.CreateInstance<AdManagerConfig>();
            cfg.SetMediator(MediatorKind.None);
            ConfigValidator.Validate(cfg);
            Object.DestroyImmediate(cfg);
        }
    }
}
