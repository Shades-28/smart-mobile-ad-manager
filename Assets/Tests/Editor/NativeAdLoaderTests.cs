using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rinval.MobileAdsAndIapKit
{
    public class NativeAdLoaderTests
    {
        private AdManagerConfig _cfg;

        [SetUp]
        public void Setup()
        {
            _ = MainThreadDispatcher.Instance;
            NativeAdLoader.RegisterAdapter(null);
            _cfg = ScriptableObject.CreateInstance<AdManagerConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            NativeAdLoader.RegisterAdapter(null);
            Object.DestroyImmediate(_cfg);
        }

        [Test]
        public void NoAdapter_LoadReturnsNull()
        {
            NativeAdData captured = new NativeAdData(); // sentinel non-null
            NativeAdLoader.Load("home", data => captured = data);
            Assert.IsNull(captured);
        }

        [UnityTest]
        public IEnumerator EditorAdapter_ReturnsPlaceholderData()
        {
            var adapter = new EditorNativeAdAdapter();
            adapter.Initialize(_cfg, _ => { });
            NativeAdLoader.RegisterAdapter(adapter);

            NativeAdData captured = null;
            NativeAdLoader.Load("home", data => captured = data);
            yield return null;
            yield return null;

            Assert.IsNotNull(captured);
            Assert.IsFalse(string.IsNullOrEmpty(captured.Headline));
            Assert.IsFalse(string.IsNullOrEmpty(captured.CallToAction));
            Assert.AreEqual("Editor (Simulated)", captured.Network);
        }
    }
}
