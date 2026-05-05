using NUnit.Framework;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    public class IapProductCatalogTests
    {
        [Test]
        public void EmptyCatalog_ReturnsEmptyList()
        {
            var catalog = ScriptableObject.CreateInstance<IapProductCatalog>();
            try
            {
                Assert.AreEqual(0, catalog.Products.Count);
                Assert.AreEqual(0, catalog.AsList().Count);
            }
            finally { Object.DestroyImmediate(catalog); }
        }

        [Test]
        public void AsList_ReturnsACopy_NotTheBackingStore()
        {
            var catalog = ScriptableObject.CreateInstance<IapProductCatalog>();
            try
            {
                var list = catalog.AsList();
                list.Add(new IapProduct("test", ProductKind.Consumable));
                // Mutating the returned list should not affect the catalog
                Assert.AreEqual(0, catalog.Products.Count);
            }
            finally { Object.DestroyImmediate(catalog); }
        }
    }
}
