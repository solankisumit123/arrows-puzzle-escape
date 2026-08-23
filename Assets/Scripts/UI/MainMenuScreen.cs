using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using ArrowsPuzzle.Systems;

namespace ArrowsPuzzle.UI
{
    public class MainMenuScreen : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _dailyChallengeButton;
        [SerializeField] private Button _customPuzzleButton;
        [SerializeField] private Button _shopButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _achievementsButton;

        [Header("Header Info")]
        [SerializeField] private TextMeshProUGUI _coinsText;
        [SerializeField] private TextMeshProUGUI _starsText;
        [SerializeField] private TextMeshProUGUI _streakText;
        [SerializeField] private RectTransform _coinIcon;

        [Header("Daily Reward Popup")]
        [SerializeField] private GameObject _dailyRewardPopup;
        [SerializeField] private TextMeshProUGUI _dailyRewardText;

        private void Start()
        {
            _playButton.onClick.AddListener(() => SceneLoader.LoadScene(SceneLoader.SceneWorldMap));
            _dailyChallengeButton.onClick.AddListener(() => SceneLoader.LoadGameplay(0, 1, false, true));
            _customPuzzleButton.onClick.AddListener(() => SceneLoader.LoadScene(SceneLoader.SceneCustomPuzzle));
            
            _shopButton.onClick.AddListener(() => FindObjectOfType<ShopScreen>(true)?.Show());
            _settingsButton.onClick.AddListener(() => FindObjectOfType<SettingsScreen>(true)?.Show());
            _achievementsButton.onClick.AddListener(() => FindObjectOfType<AchievementsScreen>(true)?.Show());

            UpdateUI();
            CoinEconomy.OnCoinsChanged += (o, n) => UpdateCoinsUI(n);

            CheckDailyLogin();
        }

        private void OnDestroy()
        {
            CoinEconomy.OnCoinsChanged -= (o, n) => UpdateCoinsUI(n);
        }

        private void UpdateUI()
        {
            if (_coinsText != null) _coinsText.text = SaveSystem.Data.totalCoins.ToString();
            if (_starsText != null) _starsText.text = SaveSystem.Data.totalStars.ToString();
            if (_streakText != null) _streakText.text = SaveSystem.Data.loginStreak.ToString();
        }

        private void UpdateCoinsUI(int newCoins)
        {
            if (_coinsText != null) _coinsText.text = newCoins.ToString();
            if (_coinIcon != null && !SaveSystem.Data.reduceMotion)
            {
                _coinIcon.DOKill();
                _coinIcon.DOScale(1.25f, 0.1f).SetEase(Ease.OutQuad).OnComplete(() =>
                    _coinIcon.DOScale(1f, 0.15f).SetEase(Ease.OutBack));
            }
        }

        private void CheckDailyLogin()
        {
            if (SaveSystem.TryClaimDailyLogin())
            {
                if (_dailyRewardPopup != null)
                {
                    _dailyRewardPopup.SetActive(true);
                    if (_dailyRewardText != null)
                        _dailyRewardText.text = $"Day {SaveSystem.Data.loginStreak} Streak Bonus!\n+{50 + SaveSystem.Data.loginStreak * 20} Coins";
                }
            }
        }

        public void CloseDailyReward()
        {
            if (_dailyRewardPopup != null) _dailyRewardPopup.SetActive(false);
        }
    }
}
