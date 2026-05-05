using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Rinval.MobileAdsAndIapKit
{
    /// <summary>Drop-on-button "Rate Us" trigger. Calls RateGameManager.TryShow which respects platform limits (90-day cooldown, 3/year cap on iOS). UnityEvent fires when the prompt was shown.</summary>
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("Mobile Ads & IAP Kit/Rate Game Button")]
    public class RateGameButton : MonoBehaviour
    {
        [Tooltip("Hide the button when CanShow returns false (already shown recently / cap reached).")]
        [SerializeField] private bool _hideWhenCannotShow = false;

        [Header("Events")]
        public UnityEvent OnPromptShown;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(Trigger);
            RateGameManager.Shown += () => OnPromptShown?.Invoke();
        }

        private void OnEnable() => RefreshUi();

        public void Trigger()
        {
            RateGameManager.TryShow();
            RefreshUi();
        }

        public void RefreshUi()
        {
            if (!_hideWhenCannotShow) return;
            gameObject.SetActive(RateGameManager.CanShow());
        }
    }
}
