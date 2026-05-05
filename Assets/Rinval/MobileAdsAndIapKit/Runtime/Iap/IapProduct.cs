using System;
using UnityEngine;

namespace Rinval.MobileAdsAndIapKit
{
    [Serializable]
    public class IapProduct
    {
        [SerializeField] private string _productId;
        [SerializeField] private ProductKind _kind = ProductKind.Consumable;
        [SerializeField] private string _googlePlaySku;
        [SerializeField] private string _appStoreSku;

        public string ProductId => _productId ?? string.Empty;
        public ProductKind Kind => _kind;
        public string PlatformSku
        {
            get
            {
#if UNITY_ANDROID
                return string.IsNullOrEmpty(_googlePlaySku) ? _productId : _googlePlaySku;
#elif UNITY_IOS
                return string.IsNullOrEmpty(_appStoreSku) ? _productId : _appStoreSku;
#else
                return _productId;
#endif
            }
        }

        public IapProduct() { }

        public IapProduct(string productId, ProductKind kind)
        {
            _productId = productId;
            _kind = kind;
        }
    }
}
