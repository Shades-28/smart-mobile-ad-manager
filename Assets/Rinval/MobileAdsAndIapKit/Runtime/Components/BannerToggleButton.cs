using UnityEngine;
using UnityEngine.UI;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Drop-on-button banner show/hide toggle. Useful for "Pause Menu → Hide Ads" or "Settings → Show Banner" controls.</summary>
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("Mobile Ads & IAP Kit/Banner Toggle Button")]
    public class BannerToggleButton : MonoBehaviour
    {
        public enum Mode { Show, Hide, Toggle }

        [SerializeField] private Mode _mode = Mode.Toggle;
        [SerializeField] private BannerAnchor _anchor = BannerAnchor.Bottom;
        [SerializeField] private BannerSize _size = BannerSize.Standard;

        private static bool _bannerVisible;
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(Trigger);
        }

        public void Trigger()
        {
            if (!AdManager.IsInitialized) return;
            switch (_mode)
            {
                case Mode.Show:
                    AdManager.LoadBanner(_anchor, _size);
                    AdManager.ShowBanner();
                    _bannerVisible = true;
                    break;
                case Mode.Hide:
                    AdManager.HideBanner();
                    _bannerVisible = false;
                    break;
                case Mode.Toggle:
                    if (_bannerVisible) { AdManager.HideBanner(); _bannerVisible = false; }
                    else { AdManager.LoadBanner(_anchor, _size); AdManager.ShowBanner(); _bannerVisible = true; }
                    break;
            }
        }
    }
}
