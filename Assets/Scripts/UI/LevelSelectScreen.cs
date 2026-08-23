using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ArrowsPuzzle.Systems;

namespace ArrowsPuzzle.UI
{
    public class LevelSelectScreen : MonoBehaviour
    {
        [SerializeField] private Transform _levelsGridContainer;
        [SerializeField] private GameObject _levelTilePrefab;
        [SerializeField] private Button _backButton;
        [SerializeField] private TextMeshProUGUI _chapterTitleText;

        private void Start()
        {
            _backButton.onClick.AddListener(() => SceneLoader.LoadScene(SceneLoader.SceneWorldMap));
            int chapterId = SceneLoader.SelectedChapterId;
            if (_chapterTitleText != null) _chapterTitleText.text = $"Chapter {chapterId}";

            int startLvl = (chapterId - 1) * 20 + 1;
            int endLvl = chapterId * 20;

            for (int lvl = startLvl; lvl <= endLvl; lvl++)
            {
                int currentLvl = lvl;
                var tileGo = _levelTilePrefab != null 
                    ? Instantiate(_levelTilePrefab, _levelsGridContainer)
                    : CreateDefaultLevelTile(_levelsGridContainer, currentLvl);

                var btn = tileGo.GetComponent<Button>();
                btn.onClick.AddListener(() => SceneLoader.LoadGameplay(chapterId, currentLvl));
            }
        }

        private GameObject CreateDefaultLevelTile(Transform parent, int levelId)
        {
            var go = new GameObject($"Level_{levelId}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(90, 90);

            var img = go.GetComponent<Image>();
            var prog = SaveSystem.GetLevelProgress(levelId);
            img.color = prog.completed ? new Color(0.2f, 0.7f, 0.3f, 0.9f) : new Color(0.25f, 0.35f, 0.45f, 0.9f);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(go.transform, false);
            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = $"{levelId}\n" + (prog.stars > 0 ? new string('★', prog.stars) : "");
            tmp.fontSize = 20;
            tmp.alignment = TextAlignmentOptions.Center;

            return go;
        }
    }
}
