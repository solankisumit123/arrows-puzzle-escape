using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ArrowsPuzzle.Systems;

namespace ArrowsPuzzle.UI
{
    public class DailyChallengeScreen : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private TextMeshProUGUI _dateText;
        [SerializeField] private TextMeshProUGUI _streakText;
        [SerializeField] private TextMeshProUGUI _rewardText;

        private void Start()
        {
            _backButton.onClick.AddListener(() => SceneLoader.LoadScene(SceneLoader.SceneMainMenu));
            _playButton.onClick.AddListener(() => SceneLoader.LoadGameplay(0, 1, false, true));

            if (_dateText != null) _dateText.text = System.DateTime.UtcNow.ToString("MMMM dd, yyyy");
            if (_streakText != null) _streakText.text = $"Streak: {SaveSystem.Data.loginStreak} Days";
            if (_rewardText != null) _rewardText.text = "Reward: 100 Coins + 3 Stars";
        }
    }
}
