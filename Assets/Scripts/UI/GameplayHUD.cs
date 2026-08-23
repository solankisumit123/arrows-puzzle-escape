using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using ArrowsPuzzle.Gameplay;
using ArrowsPuzzle.Systems;

namespace ArrowsPuzzle.UI
{
    public class GameplayHUD : MonoBehaviour
    {
        [Header("Top Bar")]
        [SerializeField] private TextMeshProUGUI _levelTitleText;
        [SerializeField] private TextMeshProUGUI _moveCountText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _coinsText;
        [SerializeField] private RectTransform _coinIcon;
        [SerializeField] private Button _pauseButton;

        [Header("Bottom Bar Controls")]
        [SerializeField] private Button _hintButton;
        [SerializeField] private Button _undoButton;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _bombButton;
        [SerializeField] private Button _shuffleButton;

        [Header("Power-up Managers")]
        [SerializeField] private PowerUpManager _powerUpManager;

        private void Start()
        {
            _pauseButton.onClick.AddListener(() => GameManager.Instance.SetPaused(true));
            _undoButton.onClick.AddListener(() => GameManager.Instance.Undo());
            _resetButton.onClick.AddListener(() => GameManager.Instance.Reset());
            
            _hintButton.onClick.AddListener(() => _powerUpManager?.UseHint());
            _shuffleButton?.onClick.AddListener(() => _powerUpManager?.UseShuffle());

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMoveCountChanged += HandleMoveCountChanged;
                GameManager.Instance.OnTimeUpdate += HandleTimeUpdate;
                GameManager.Instance.OnResetPerformed += HandleReset;
                if (_levelTitleText != null)
                    _levelTitleText.text = $"Level {GameManager.Instance.CurrentLevel?.levelId ?? 1}";
            }

            UpdateCoinsUI(CoinEconomy.Coins);
            CoinEconomy.OnCoinsChanged += (o, n) => UpdateCoinsUI(n);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMoveCountChanged -= HandleMoveCountChanged;
                GameManager.Instance.OnTimeUpdate -= HandleTimeUpdate;
                GameManager.Instance.OnResetPerformed -= HandleReset;
            }
            CoinEconomy.OnCoinsChanged -= (o, n) => UpdateCoinsUI(n);
        }

        private void HandleMoveCountChanged(int oldVal, int newVal)
        {
            if (_moveCountText != null)
            {
                _moveCountText.text = $"Moves: {newVal}";
                if (!SaveSystem.Data.reduceMotion)
                    _moveCountText.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f);
            }
        }

        private void HandleTimeUpdate(float time)
        {
            if (_timerText != null)
            {
                int mins = (int)(time / 60);
                int secs = (int)(time % 60);
                _timerText.text = $"{mins:00}:{secs:00}";
            }
        }

        private void HandleReset()
        {
            if (_moveCountText != null) _moveCountText.text = "Moves: 0";
            if (_timerText != null) _timerText.text = "00:00";
        }

        private void UpdateCoinsUI(int coins)
        {
            if (_coinsText != null) _coinsText.text = coins.ToString();
        }
    }
}
