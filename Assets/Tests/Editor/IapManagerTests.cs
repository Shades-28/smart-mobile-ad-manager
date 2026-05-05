using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Rinval.MobileAdsAndIapKit
{
    public class IapManagerTests
    {
        private List<IapProduct> _catalog;

        [SetUp]
        public void Setup()
        {
            // Touch dispatcher on main thread before anything that may call Enqueue.
            _ = MainThreadDispatcher.Instance;
            IapManager.Shutdown();
            _catalog = new List<IapProduct>
            {
                new IapProduct("coins_100", ProductKind.Consumable),
                new IapProduct("remove_ads", ProductKind.NonConsumable),
                new IapProduct("vip_monthly", ProductKind.Subscription),
            };
            EditorIapAdapter.NextResult = PurchaseResultCode.Success;
        }

        [TearDown]
        public void TearDown() => IapManager.Shutdown();

        [UnityTest]
        public IEnumerator Initialize_WithEmptyList_FailsCleanly()
        {
            LogAssert.Expect(UnityEngine.LogType.Error, new System.Text.RegularExpressions.Regex(@"\[ADS:ERROR\].*empty product list"));
            bool? ok = null;
            IapManager.Initialize(new List<IapProduct>(), onReady: r => ok = r);
            yield return null;
            Assert.IsFalse(ok ?? true);
        }

        [UnityTest]
        public IEnumerator Initialize_Succeeds_WithEditorAdapter()
        {
            bool? ok = null;
            IapManager.Initialize(_catalog, onReady: r => ok = r);
            // EditorIapAdapter is synchronous but onReady marshals via MainThreadDispatcher.
            // Drain by running its Update via a frame.
            yield return null;
            yield return null;
            Assert.IsTrue(ok ?? false);
            Assert.IsTrue(IapManager.IsInitialized);
        }

        [UnityTest]
        public IEnumerator Purchase_Consumable_Succeeds()
        {
            IapManager.Initialize(_catalog);
            yield return null; yield return null;
            PurchaseResult? captured = null;
            IapManager.Purchase("coins_100", r => captured = r);
            yield return null; yield return null;
            Assert.IsTrue(captured.HasValue);
            Assert.AreEqual(PurchaseResultCode.Success, captured.Value.Code);
        }

        [UnityTest]
        public IEnumerator Purchase_NonConsumable_TracksOwnership()
        {
            IapManager.Initialize(_catalog);
            yield return null; yield return null;
            IapManager.Purchase("remove_ads", null);
            yield return null; yield return null;
            Assert.IsTrue(IapManager.HasNonConsumable("remove_ads"));
            Assert.IsFalse(IapManager.HasNonConsumable("coins_100"));
        }

        [UnityTest]
        public IEnumerator Purchase_Cancelled_DoesNotGrant()
        {
            IapManager.Initialize(_catalog);
            yield return null; yield return null;
            EditorIapAdapter.NextResult = PurchaseResultCode.UserCancelled;
            PurchaseResult? captured = null;
            IapManager.Purchase("remove_ads", r => captured = r);
            yield return null; yield return null;
            Assert.AreEqual(PurchaseResultCode.UserCancelled, captured.Value.Code);
            Assert.IsFalse(IapManager.HasNonConsumable("remove_ads"));
        }

        [UnityTest]
        public IEnumerator Subscription_FiresActivatedEvent()
        {
            IapManager.Initialize(_catalog);
            yield return null; yield return null;
            string activated = null;
            IapManager.SubscriptionActivated += pid => activated = pid;
            IapManager.Purchase("vip_monthly", null);
            yield return null; yield return null;
            Assert.AreEqual("vip_monthly", activated);
            Assert.IsTrue(IapManager.IsSubscriptionActive("vip_monthly"));
        }
    }
}
