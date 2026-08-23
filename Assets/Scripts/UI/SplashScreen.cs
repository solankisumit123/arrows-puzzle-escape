using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using ArrowsPuzzle.Systems;

namespace ArrowsPuzzle.UI
{
    public class SplashScreen : MonoBehaviour
    {
        [SerializeField] private Image _logoImage;
        [SerializeField] private Image _arrowIcon;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _duration = 2.0f;

        private bool _skipped = false;

        private void Start()
        {
            SaveSystem.Load();
            AchievementSystem.Initialize();

            // Animate Logo + Arrow Icon
            if (_arrowIcon != null)
            {
                _arrowIcon.transform.localPosition = new Vector3(-200, 0, 0);
                _arrowIcon.transform.DOLocalMoveX(0, 0.8f).SetEase(Ease.OutBack);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.DOFade(1f, 0.5f);
            }

            Invoke(nameof(ProceedToMainMenu), _duration);
        }

        public void OnScreenTapped()
        {
            if (!_skipped)
            {
                _skipped = true;
                CancelInvoke(nameof(ProceedToMainMenu));
                ProceedToMainMenu();
            }
        }

        private void ProceedToMainMenu()
        {
            SceneLoader.LoadScene(SceneLoader.SceneMainMenu);
        }
    }
}
