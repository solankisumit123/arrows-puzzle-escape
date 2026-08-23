using UnityEngine;
using UnityEngine.UI;
using ArrowsPuzzle.Gameplay;
using ArrowsPuzzle.Systems;

namespace ArrowsPuzzle.UI
{
    public class PauseMenuPanel : MonoBehaviour
    {
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;

        private void Start()
        {
            _resumeButton.onClick.AddListener(() => GameManager.Instance.SetPaused(false));
            _restartButton.onClick.AddListener(() => {
                GameManager.Instance.SetPaused(false);
                GameManager.Instance.Reset();
            });
            _settingsButton.onClick.AddListener(() => FindObjectOfType<SettingsScreen>(true)?.Show());
            _quitButton.onClick.AddListener(() => {
                GameManager.Instance.SetPaused(false);
                SceneLoader.LoadScene(SceneLoader.SceneWorldMap);
            });
        }
    }
}
