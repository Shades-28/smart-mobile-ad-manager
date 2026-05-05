using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    public class AbTestManagerTests
    {
        private AbExperimentCatalog _catalog;

        [SetUp]
        public void Setup()
        {
            AbTestManager.ResetForTests();
            _catalog = ScriptableObject.CreateInstance<AbExperimentCatalog>();
            // Build a 2-variant experiment via reflection (no public setters on the SO classes).
            var exp = new AbExperiment();
            SetField(exp, "_key", "test_exp");
            SetField(exp, "_placement", "test_placement");
            var variants = new List<AbVariant>();
            variants.Add(MakeVariant("control", 50));
            variants.Add(MakeVariant("treatment", 50));
            SetField(exp, "_variants", variants);
            SetField(_catalog, "_experiments", new List<AbExperiment> { exp });
            AbTestManager.SetCatalog(_catalog);
        }

        [TearDown]
        public void TearDown()
        {
            AbTestManager.ResetForTests();
            Object.DestroyImmediate(_catalog);
        }

        [Test]
        public void GetVariantKey_StableAcrossCalls()
        {
            var first = AbTestManager.GetVariantKey("test_exp");
            var second = AbTestManager.GetVariantKey("test_exp");
            var third = AbTestManager.GetVariantKey("test_exp");
            Assert.AreEqual(first, second);
            Assert.AreEqual(second, third);
        }

        [Test]
        public void GetVariantKey_UnknownExperiment_ReturnsControl()
        {
            Assert.AreEqual("control", AbTestManager.GetVariantKey("does_not_exist"));
        }

        [Test]
        public void GetVariantKey_NoCatalog_ReturnsControl()
        {
            AbTestManager.SetCatalog(null);
            Assert.AreEqual("control", AbTestManager.GetVariantKey("test_exp"));
        }

        [Test]
        public void GetVariantKey_AssignsToOneOfDeclaredVariants()
        {
            var v = AbTestManager.GetVariantKey("test_exp");
            Assert.IsTrue(v == "control" || v == "treatment", $"Got unexpected variant: {v}");
        }

        [Test]
        public void GetVariantKey_FiresVariantAssignedEvent_OnlyOnce()
        {
            int events = 0;
            AbTestManager.VariantAssigned += (_, __) => events++;
            AbTestManager.GetVariantKey("test_exp");
            AbTestManager.GetVariantKey("test_exp");
            AbTestManager.GetVariantKey("test_exp");
            Assert.AreEqual(1, events, "Sticky assignment should only fire the event the first time");
        }

        [Test]
        public void DifferentUserIds_GetIndependentBucketing()
        {
            // Same user (sticky)
            var a1 = AbTestManager.GetVariantKey("test_exp");
            var a2 = AbTestManager.GetVariantKey("test_exp");
            Assert.AreEqual(a1, a2);
        }

        // -- helpers --
        private static void SetField(object o, string field, object value)
        {
            var f = o.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            f.SetValue(o, value);
        }

        private static AbVariant MakeVariant(string key, int weight)
        {
            var v = new AbVariant();
            SetField(v, "_key", key);
            SetField(v, "_weight", weight);
            return v;
        }
    }
}
