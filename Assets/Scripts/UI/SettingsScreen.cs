using UnityEngine;
using UnityEngine.UI;
using ArrowsPuzzle.Systems;

namespace ArrowsPuzzle.UI
{
    public class SettingsScreen : MonoBehaviour
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private Toggle _soundToggle;
        [SerializeField] private Toggle _musicToggle;
        [SerializeField] private Toggle _vibrationToggle;
        [SerializeField] private Toggle _reduceMotionToggle;
        [SerializeField] private Toggle _colorblindToggle;
        [SerializeField] private Toggle _darkModeToggle;
        [SerializeField] private Button _resetProgressButton;
        [SerializeField] private GameObject _confirmResetPopup;
        [SerializeField] private Button _confirmResetYes;
        [SerializeField] private Button _confirmResetNo;

        private void Awake()
        {
            gameObject.SetActive(false);
            _closeButton.onClick.AddListener(Hide);

            _soundToggle?.onValueChanged.AddListener(val => {
                SaveSystem.Data.soundEnabled = val;
                SaveSystem.Save();
            });

            _musicToggle?.onValueChanged.AddListener(val => {
                SaveSystem.Data.musicEnabled = val;
                AudioManager.Instance?.SetMusicEnabled(val);
                SaveSystem.Save();
            });

            _vibrationToggle?.onValueChanged.AddListener(val => {
                SaveSystem.Data.vibrationEnabled = val;
                SaveSystem.Save();
            });

            _reduceMotionToggle?.onValueChanged.AddListener(val => {
                SaveSystem.Data.reduceMotion = val;
                SaveSystem.Save();
            });

            _colorblindToggle?.onValueChanged.AddListener(val => {
                SaveSystem.Data.colorblindMode = val;
                SaveSystem.Save();
            });

            _darkModeToggle?.onValueChanged.AddListener(val => {
                SaveSystem.Data.darkMode = val;
                SaveSystem.Save();
            });

            _resetProgressButton?.onClick.AddListener(() => _confirmResetPopup?.SetActive(true));
            _confirmResetNo?.onClick.AddListener(() => _confirmResetPopup?.SetActive(false));
            _confirmResetYes?.onClick.AddListener(() => {
                SaveSystem.ResetProgress();
                _confirmResetPopup?.SetActive(false);
                Hide();
                SceneLoader.LoadScene(SceneLoader.SceneMainMenu);
            });
        }

        public void Show()
        {
            gameObject.SetActive(true);
            if (_soundToggle != null) _soundToggle.isOn = SaveSystem.Data.soundEnabled;
            if (_musicToggle != null) _musicToggle.isOn = SaveSystem.Data.musicEnabled;
            if (_vibrationToggle != null) _vibrationToggle.isOn = SaveSystem.Data.vibrationEnabled;
            if (_reduceMotionToggle != null) _reduceMotionToggle.isOn = SaveSystem.Data.reduceMotion;
            if (_colorblindToggle != null) _colorblindToggle.isOn = SaveSystem.Data.colorblindMode;
            if (_darkModeToggle != null) _darkModeToggle.isOn = SaveSystem.Data.darkMode;
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
