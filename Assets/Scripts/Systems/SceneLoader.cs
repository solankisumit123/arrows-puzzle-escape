using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArrowsPuzzle.Systems
{
    public static class SceneLoader
    {
        public const string SceneSplash       = "Splash";
        public const string SceneMainMenu     = "MainMenu";
        public const string SceneWorldMap     = "WorldMap";
        public const string SceneLevelSelect  = "LevelSelect";
        public const string SceneGameplay     = "Gameplay";
        public const string SceneCustomPuzzle = "CustomPuzzle";

        // Data passed between scenes
        public static int SelectedChapterId;
        public static int SelectedLevelId;
        public static bool IsCustomPuzzle;
        public static bool IsDailyChallenge;

        public static void LoadScene(string sceneName, Action onLoaded = null)
        {
            SceneManager.LoadScene(sceneName);
            onLoaded?.Invoke();
        }

        public static void LoadGameplay(int chapterId, int levelId, bool isCustom = false, bool isDaily = false)
        {
            SelectedChapterId = chapterId;
            SelectedLevelId = levelId;
            IsCustomPuzzle = isCustom;
            IsDailyChallenge = isDaily;
            LoadScene(SceneGameplay);
        }

        public static void LoadLevelSelect(int chapterId)
        {
            SelectedChapterId = chapterId;
            LoadScene(SceneLevelSelect);
        }
    }
}
