using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ArrowsPuzzle.Systems
{
    [Serializable]
    public class LevelProgress
    {
        public int levelId;
        public int stars;          // 0-3
        public int bestMoves;
        public float bestTime;
        public bool completed;
    }

    [Serializable]
    public class SaveData
    {
        public int totalCoins;
        public int totalStars;
        public int currentChapter;
        public int currentLevel;
        public int[] unlockedChapters = new int[0];
        public List<LevelProgress> levelProgress = new List<LevelProgress>();

        // Settings
        public bool soundEnabled = true;
        public bool musicEnabled = true;
        public bool vibrationEnabled = true;
        public bool reduceMotion = false;
        public bool colorblindMode = false;
        public int textSize = 1; // 0=small,1=normal,2=large
        public string language = "en";
        public bool darkMode = false;
        public bool adsRemoved = false;

        // Economy & streaks
        public int hintCount;
        public int bombCount;
        public int shuffleCount;
        public int extraMoveCount;
        public int consecutive3StarStreak;
        public int noHintStreak;
        public int loginStreak;
        public string lastLoginDate;
        public string lastDailyChallengeDate;
        public int dailyChallengeStars;

        // Achievements (tracked by id)
        public List<string> unlockedAchievements = new List<string>();
        public Dictionary<string, int> achievementProgress = new Dictionary<string, int>();

        // Active skin
        public string activeSkin = "default";
        public List<string> ownedSkins = new List<string> { "default" };

        // Adaptive difficulty
        public float avgMovesRatio = 1f;  // player's avg moves/optimal ratio
        public float adaptiveFactor = 1f;  // multiplier applied to star thresholds

        // Ghost replay: last solution moves per level
        public Dictionary<int, string> lastSolutionJson = new Dictionary<int, string>();
    }

    public static class SaveSystem
    {
        private static readonly string SavePath =
            Path.Combine(Application.persistentDataPath, "save.json");

        private static SaveData _cache;

        public static SaveData Data
        {
            get
            {
                if (_cache == null) Load();
                return _cache;
            }
        }

        public static void Load()
        {
            if (File.Exists(SavePath))
            {
                try
                {
                    string json = File.ReadAllText(SavePath);
                    _cache = JsonUtility.FromJson<SaveData>(json);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[SaveSystem] Failed to load: {e.Message}. Starting fresh.");
                    _cache = new SaveData();
                }
            }
            else
            {
                _cache = new SaveData();
            }
        }

        public static void Save()
        {
            try
            {
                string json = JsonUtility.ToJson(_cache, true);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to save: {e.Message}");
            }
        }

        public static void ResetProgress()
        {
            _cache = new SaveData();
            if (File.Exists(SavePath)) File.Delete(SavePath);
        }

        // ─── Level Progress ──────────────────────────────────────────────────

        public static LevelProgress GetLevelProgress(int levelId)
        {
            var prog = Data.levelProgress.Find(p => p.levelId == levelId);
            if (prog == null)
            {
                prog = new LevelProgress { levelId = levelId };
                Data.levelProgress.Add(prog);
            }
            return prog;
        }

        public static void RecordLevelComplete(int levelId, int stars, int moves, float time)
        {
            var prog = GetLevelProgress(levelId);
            bool improved = stars > prog.stars || (stars == prog.stars && moves < prog.bestMoves);
            if (improved || !prog.completed)
            {
                if (stars > prog.stars) Data.totalStars += (stars - prog.stars);
                prog.stars = Mathf.Max(prog.stars, stars);
                prog.bestMoves = prog.completed ? Mathf.Min(prog.bestMoves, moves) : moves;
                prog.bestTime = prog.completed ? Mathf.Min(prog.bestTime, time) : time;
                prog.completed = true;
            }
            UpdateAdaptiveDifficulty(moves, GetLevelProgress(levelId).bestMoves);
            Save();
        }

        private static void UpdateAdaptiveDifficulty(int playerMoves, int optimalMoves)
        {
            if (optimalMoves <= 0) return;
            float ratio = (float)playerMoves / optimalMoves;
            Data.avgMovesRatio = Data.avgMovesRatio * 0.85f + ratio * 0.15f; // EWA
            // Adapt star threshold: if player consistently overshoots, soften it
            if (Data.avgMovesRatio > 1.8f)
                Data.adaptiveFactor = Mathf.Min(Data.adaptiveFactor + 0.05f, 1.5f);
            else if (Data.avgMovesRatio < 1.1f)
                Data.adaptiveFactor = Mathf.Max(Data.adaptiveFactor - 0.02f, 1.0f);
        }

        // ─── Chapter unlock ──────────────────────────────────────────────────

        public static bool IsChapterUnlocked(int chapterId)
        {
            if (chapterId == 1) return true;
            int starsNeeded = (chapterId - 1) * 20; // 20 stars per chapter to unlock next
            return Data.totalStars >= starsNeeded;
        }

        // ─── Daily Login ─────────────────────────────────────────────────────

        public static bool TryClaimDailyLogin()
        {
            string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            if (Data.lastLoginDate == today) return false;

            bool consecutive = Data.lastLoginDate ==
                DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
            Data.loginStreak = consecutive ? Data.loginStreak + 1 : 1;
            Data.lastLoginDate = today;
            int bonus = 50 + (Data.loginStreak >= 7 ? 200 : Data.loginStreak * 20);
            CoinEconomy.AddCoins(bonus);
            Save();
            return true;
        }
    }
}
