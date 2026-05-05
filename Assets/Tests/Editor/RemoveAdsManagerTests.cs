using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rinval.MobileAdsAndIapKit
{
    public class RemoveAdsManagerTests
    {
        private List<IapProduct> _catalog;

        [SetUp]
        public void Setup()
        {
            _ = MainThreadDispatcher.Instance;
            IapManager.Shutdown();
            RemoveAdsManager.ResetForTests();
            _catalog = new List<IapProduct>
            {
                new IapProduct(RemoveAdsManager.DefaultProductId, ProductKind.NonConsumable),
                new IapProduct("coins_100", ProductKind.Consumable),
            };
            EditorIapAdapter.NextResult = PurchaseResultCode.Success;
        }

        [TearDown]
        public void TearDown()
        {
            IapManager.Shutdown();
            RemoveAdsManager.ResetForTests();
        }

        [UnityTest]
        public IEnumerator BeforeInitialize_IsActiveReadsPlayerPrefs()
        {
            // Pretend a previous session already removed ads.
            PlayerPrefs.SetInt("Rinval.MobileAdsIap.RemoveAdsActive", 1);
            Assert.IsTrue(RemoveAdsManager.IsActive);
            yield return null;
            PlayerPrefs.DeleteKey("Rinval.MobileAdsIap.RemoveAdsActive");
        }

        [UnityTest]
        public IEnumerator PurchasingRemoveAds_ActivatesAndFiresEvent()
        {
            RemoveAdsManager.Initialize();
            IapManager.Initialize(_catalog);
            yield return null; yield return null;

            bool fired = false;
            RemoveAdsManager.Activated += () => fired = true;

            IapManager.Purchase(RemoveAdsManager.DefaultProductId, null);
            yield return null; yield return null;

            Assert.IsTrue(fired, "Activated event should fire after successful remove-ads purchase");
            Assert.IsTrue(RemoveAdsManager.IsActive);
        }

        [UnityTest]
        public IEnumerator PurchasingOtherProduct_DoesNotActivateRemoveAds()
        {
            RemoveAdsManager.Initialize();
            IapManager.Initialize(_catalog);
            yield return null; yield return null;

            bool fired = false;
            RemoveAdsManager.Activated += () => fired = true;
            IapManager.Purchase("coins_100", null);
            yield return null; yield return null;

            Assert.IsFalse(fired, "Buying coins should not activate remove-ads");
            Assert.IsFalse(RemoveAdsManager.IsActive);
        }
    }
}
