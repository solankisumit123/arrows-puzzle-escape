using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using ArrowsPuzzle.Engine;
using ArrowsPuzzle.Systems;

namespace ArrowsPuzzle.UI
{
    public class WorldMapScreen : MonoBehaviour
    {
        [SerializeField] private Transform _chapterListContainer;
        [SerializeField] private GameObject _chapterTilePrefab;
        [SerializeField] private Button _backButton;
        [SerializeField] private TextMeshProUGUI _totalStarsText;
        [SerializeField] private ScrollRect _scrollRect;

        private void Start()
        {
            _backButton.onClick.AddListener(() => SceneLoader.LoadScene(SceneLoader.SceneMainMenu));
            if (_totalStarsText != null) _totalStarsText.text = SaveSystem.Data.totalStars.ToString();

            BuildChapterList();
        }

        private void BuildChapterList()
        {
            var chapters = LevelLoader.LoadAllChapters();
            if (chapters == null || chapters.Count == 0)
            {
                // Fallback default 10 chapters
                chapters = new List<ChapterData>();
                string[] names = { "Wood Workshop", "Ice Cave", "Space Station", "Neon City", "Jungle Temple", "Crystal Caves", "Volcano", "Sky Fortress", "Deep Ocean", "The Void" };
                for (int i = 1; i <= 10; i++)
                {
                    chapters.Add(new ChapterData { chapterId = i, name = names[i - 1], startLevel = (i - 1) * 20 + 1, endLevel = i * 20 });
                }
            }

            foreach (var ch in chapters)
            {
                var tileGo = _chapterTilePrefab != null 
                    ? Instantiate(_chapterTilePrefab, _chapterListContainer)
                    : CreateDefaultChapterTile(_chapterListContainer, ch);

                bool isUnlocked = SaveSystem.IsChapterUnlocked(ch.chapterId);
                var btn = tileGo.GetComponent<Button>();
                btn.onClick.AddListener(() =>
                {
                    if (isUnlocked)
                    {
                        SceneLoader.LoadLevelSelect(ch.chapterId);
                    }
                    else
                    {
                        // Shake locked chapter tile
                        AudioManager.Instance?.PlayLock();
                        if (!SaveSystem.Data.reduceMotion)
                            tileGo.transform.DOShakePosition(0.3f, 10f, 20);
                    }
                });
            }
        }

        private GameObject CreateDefaultChapterTile(Transform parent, ChapterData ch)
        {
            var go = new GameObject($"Chapter_{ch.chapterId}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500, 100);

            var img = go.GetComponent<Image>();
            bool isUnlocked = SaveSystem.IsChapterUnlocked(ch.chapterId);
            img.color = isUnlocked ? new Color(0.2f, 0.3f, 0.4f, 0.8f) : new Color(0.1f, 0.1f, 0.15f, 0.8f);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(go.transform, false);
            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = $"Chapter {ch.chapterId}: {ch.name}" + (isUnlocked ? "" : $" (Locked - Need {(ch.chapterId - 1) * 20} Stars)");
            tmp.fontSize = 22;
            tmp.alignment = TextAlignmentOptions.Center;

            return go;
        }
    }
}
