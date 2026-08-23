using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using ArrowsPuzzle.Gameplay;
using ArrowsPuzzle.Systems;

namespace ArrowsPuzzle.UI
{
    public class LevelCompletePanel : MonoBehaviour
    {
        [Header("Star Icons")]
        [SerializeField] private Image[] _starImages; // 3 star slots
        [SerializeField] private Sprite _starFilledSprite;
        [SerializeField] private Sprite _starEmptySprite;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI _movesText;
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private TextMeshProUGUI _coinsRewardText;
        [SerializeField] private GameObject _newBestBadge;

        [Header("Buttons")]
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _replayButton;
        [SerializeField] private Button _shareButton;
        [SerializeField] private CanvasGroup _buttonsGroup;

        [Header("Animation Container")]
        [SerializeField] private RectTransform _panelRoot;

        private void Awake()
        {
            gameObject.SetActive(false);
            if (_nextLevelButton != null) _nextLevelButton.onClick.AddListener(() => GameManager.Instance.GoToNextLevel());
            if (_replayButton != null) _replayButton.onClick.AddListener(() => {
                gameObject.SetActive(false);
                GameManager.Instance.Reset();
            });
            if (_shareButton != null) _shareButton.onClick.AddListener(ShareResult);
        }

        public void Show(int stars, int coins, int moves, float time, bool isNewBest)
        {
            gameObject.SetActive(true);
            _buttonsGroup.alpha = 0f;
            if (_newBestBadge != null) _newBestBadge.SetActive(false);

            // Reset stars
            for (int i = 0; i < _starImages.Length; i++)
            {
                if (_starImages[i] != null)
                {
                    _starImages[i].sprite = _starEmptySprite;
                    _starImages[i].transform.localScale = Vector3.one;
                }
            }

            // Panel slide/scale in
            _panelRoot.localScale = Vector3.zero;
            _panelRoot.DOScale(1f, 0.4f).SetEase(Ease.OutBack);

            StartCoroutine(SequenceRoutine(stars, coins, moves, time, isNewBest));
        }

        private IEnumerator SequenceRoutine(int stars, int coins, int moves, float time, bool isNewBest)
        {
            yield return new WaitForSeconds(0.4f);

            // Staggered star animation
            for (int i = 0; i < stars && i < _starImages.Length; i++)
            {
                yield return new WaitForSeconds(0.15f);
                if (_starImages[i] != null)
                {
                    _starImages[i].sprite = _starFilledSprite;
                    AudioManager.Instance?.PlayStar();
                    if (!SaveSystem.Data.reduceMotion)
                    {
                        _starImages[i].transform.localScale = Vector3.zero;
                        _starImages[i].transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack)
                            .OnComplete(() => _starImages[i].transform.DOScale(1f, 0.1f));
                    }
                }
            }

            yield return new WaitForSeconds(0.2f);

            // Moves and Time count up
            if (_movesText != null) _movesText.text = $"Moves: {moves}";
            if (_timeText != null) _timeText.text = $"Time: {(int)time}s";
            if (_coinsRewardText != null) _coinsRewardText.text = $"+{coins} Coins";

            if (isNewBest && _newBestBadge != null)
            {
                _newBestBadge.SetActive(true);
                _newBestBadge.transform.DOScale(1f, 0.2f).From(0f).SetEase(Ease.OutBounce);
            }

            yield return new WaitForSeconds(0.3f);

            // Fade in buttons
            _buttonsGroup.DOFade(1f, 0.3f);
        }

        private void ShareResult()
        {
            Debug.Log("[Share] Shared puzzle score with friends!");
            // In Android/iOS build, call native share sheet plugin
        }
    }
}
