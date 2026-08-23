using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using ArrowsPuzzle.Systems;

namespace ArrowsPuzzle.Effects
{
    public class ScreenTransitionManager : MonoBehaviour
    {
        public static ScreenTransitionManager Instance { get; private set; }

        [SerializeField] private Image _fadeOverlay;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void FadeTransition(Action onFadeOutComplete)
        {
            if (_fadeOverlay == null || SaveSystem.Data.reduceMotion)
            {
                onFadeOutComplete?.Invoke();
                return;
            }

            _fadeOverlay.gameObject.SetActive(true);
            _fadeOverlay.color = new Color(0, 0, 0, 0);
            _fadeOverlay.DOFade(1f, 0.25f).OnComplete(() =>
            {
                onFadeOutComplete?.Invoke();
                _fadeOverlay.DOFade(0f, 0.25f).OnComplete(() => _fadeOverlay.gameObject.SetActive(false));
            });
        }
    }
}
