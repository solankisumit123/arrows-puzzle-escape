using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ArrowsPuzzle.Systems;

namespace ArrowsPuzzle.UI
{
    public class AchievementsScreen : MonoBehaviour
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private Transform _achievementsContainer;
        [SerializeField] private GameObject _achievementItemPrefab;

        private void Awake()
        {
            gameObject.SetActive(false);
            _closeButton.onClick.AddListener(Hide);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            PopulateList();
        }

        public void Hide() => gameObject.SetActive(false);

        private void PopulateList()
        {
            foreach (Transform t in _achievementsContainer) Destroy(t.gameObject);

            var list = AchievementSystem.GetAll();
            foreach (var ach in list)
            {
                var itemGo = _achievementItemPrefab != null 
                    ? Instantiate(_achievementItemPrefab, _achievementsContainer)
                    : CreateDefaultAchItem(_achievementsContainer, ach);
            }
        }

        private GameObject CreateDefaultAchItem(Transform parent, AchievementDef ach)
        {
            var go = new GameObject($"Ach_{ach.id}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500, 70);

            bool isUnlocked = AchievementSystem.IsUnlocked(ach.id);
            int currentProg = AchievementSystem.GetProgress(ach.id);
            var img = go.GetComponent<Image>();
            img.color = isUnlocked ? new Color(0.2f, 0.5f, 0.3f, 0.8f) : new Color(0.15f, 0.18f, 0.22f, 0.8f);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(go.transform, false);
            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = $"{ach.title} ({(isUnlocked ? "COMPLETED" : $"{currentProg}/{ach.targetValue}")})\n{ach.description}  [+{ach.coinReward} coins]";
            tmp.fontSize = 16;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;

            return go;
        }
    }
}
