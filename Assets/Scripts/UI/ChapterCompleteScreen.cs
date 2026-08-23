using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using ArrowsPuzzle.Systems;

namespace ArrowsPuzzle.UI
{
    public class ChapterCompleteScreen : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _chapterNameText;
        [SerializeField] private Button _continueButton;
        [SerializeField] private RectTransform _lockBreakAnim;

        private void Awake()
        {
            gameObject.SetActive(false);
            _continueButton.onClick.AddListener(() => SceneLoader.LoadScene(SceneLoader.SceneWorldMap));
        }

        public void Show(int chapterId)
        {
            gameObject.SetActive(true);
            if (_chapterNameText != null)
                _chapterNameText.text = $"Chapter {chapterId} Cleared!";

            if (_lockBreakAnim != null && !SaveSystem.Data.reduceMotion)
            {
                _lockBreakAnim.localScale = Vector3.one;
                _lockBreakAnim.DOPunchScale(Vector3.one * 0.4f, 0.6f, 10);
            }
        }
    }
}
