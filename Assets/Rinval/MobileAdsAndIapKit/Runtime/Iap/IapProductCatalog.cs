using System.Collections.Generic;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Drag products into this asset, then drag the asset into MobileKitBootstrap or pass to IapManager.Initialize. Eliminates the "list of products in C# code" smell.</summary>
    [CreateAssetMenu(
        fileName = "IapProductCatalog",
        menuName = "Rinval/Mobile Ads & IAP/IAP Product Catalog",
        order = 110)]
    public class IapProductCatalog : ScriptableObject
    {
        [SerializeField] private List<IapProduct> _products = new List<IapProduct>();

        public IReadOnlyList<IapProduct> Products => _products;

        public IList<IapProduct> AsList()
        {
            return new List<IapProduct>(_products);
        }
    }
}
