using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowsPuzzle.Systems
{
    [Serializable]
    public class AchievementDef
    {
        public string id;
        public string title;
        public string description;
        public string iconName;
        public int targetValue;
        public int coinReward;
    }

    public static class AchievementSystem
    {
        public static Action<AchievementDef> OnAchievementUnlocked;

        private static List<AchievementDef> _definitions;

        public static void Initialize()
        {
            var asset = Resources.Load<TextAsset>("Data/achievements");
            if (asset != null)
            {
                var wrapper = JsonUtility.FromJson<AchDefWrapper>(asset.text);
                _definitions = new List<AchievementDef>(wrapper.achievements);
            }
            else
            {
                _definitions = GetDefaultAchievements();
            }
        }

        public static void Increment(string id, int amount = 1)
        {
            if (SaveSystem.Data.unlockedAchievements.Contains(id)) return;

            var def = _definitions?.Find(d => d.id == id);
            if (def == null) return;

            if (!SaveSystem.Data.achievementProgress.ContainsKey(id))
                SaveSystem.Data.achievementProgress[id] = 0;

            SaveSystem.Data.achievementProgress[id] += amount;

            if (SaveSystem.Data.achievementProgress[id] >= def.targetValue)
                Unlock(def);

            SaveSystem.Save();
        }

        public static void Unlock(AchievementDef def)
        {
            if (SaveSystem.Data.unlockedAchievements.Contains(def.id)) return;
            SaveSystem.Data.unlockedAchievements.Add(def.id);
            CoinEconomy.AddCoins(def.coinReward);
            OnAchievementUnlocked?.Invoke(def);
            SaveSystem.Save();
        }

        public static int GetProgress(string id)
        {
            SaveSystem.Data.achievementProgress.TryGetValue(id, out int val);
            return val;
        }

        public static bool IsUnlocked(string id) =>
            SaveSystem.Data.unlockedAchievements.Contains(id);

        public static List<AchievementDef> GetAll() => _definitions ?? new List<AchievementDef>();

        private static List<AchievementDef> GetDefaultAchievements() => new List<AchievementDef>
        {
            new AchievementDef { id="first_level",   title="First Step",          description="Complete your first level",         targetValue=1,   coinReward=50  },
            new AchievementDef { id="ten_3stars",    title="Triple Threat",       description="Solve 10 levels with 3 stars",      targetValue=10,  coinReward=200 },
            new AchievementDef { id="no_hint_5",     title="Pure Genius",         description="Solve 5 levels in a row without hints", targetValue=5, coinReward=150 },
            new AchievementDef { id="chapter1_done", title="Woodworker",          description="Complete Chapter 1",                targetValue=20,  coinReward=300 },
            new AchievementDef { id="chapter5_done", title="Temple Explorer",     description="Complete Chapter 5",                targetValue=100, coinReward=500 },
            new AchievementDef { id="all_chapters",  title="Puzzle Master",       description="Complete all 10 chapters",          targetValue=200, coinReward=2000},
            new AchievementDef { id="streak_7",      title="7-Day Streak",        description="Log in 7 days in a row",            targetValue=7,   coinReward=350 },
            new AchievementDef { id="streak_30",     title="Dedicated",           description="Log in 30 days in a row",           targetValue=30,  coinReward=1000},
            new AchievementDef { id="hint_never",    title="Unaided",             description="Complete 20 levels without hints",  targetValue=20,  coinReward=400 },
            new AchievementDef { id="bomb_used_10",  title="Demo Expert",         description="Use the bomb power-up 10 times",   targetValue=10,  coinReward=100 },
            new AchievementDef { id="daily_7",       title="Daily Devotee",       description="Complete 7 daily challenges",       targetValue=7,   coinReward=250 },
            new AchievementDef { id="daily_30",      title="Challenge Champion",  description="Complete 30 daily challenges",      targetValue=30,  coinReward=750 },
            new AchievementDef { id="custom_puzzle", title="Artist",              description="Create your first custom puzzle",   targetValue=1,   coinReward=100 },
            new AchievementDef { id="speed_run",     title="Speed Demon",         description="Solve a level in under 30 seconds", targetValue=1,   coinReward=200 },
            new AchievementDef { id="total_1000",    title="Puzzle Enthusiast",   description="Complete 1000 total moves",         targetValue=1000,coinReward=300 },
            new AchievementDef { id="undo_never",    title="No Regrets",          description="Complete 10 levels without undo",   targetValue=10,  coinReward=200 },
            new AchievementDef { id="perfect_chapter",title="Perfectionist",      description="3-star every level in a chapter",  targetValue=20,  coinReward=500 },
            new AchievementDef { id="coin_1000",     title="Rich Puzzler",        description="Accumulate 1000 coins",             targetValue=1000,coinReward=50  },
            new AchievementDef { id="skin_unlock",   title="Fashion Forward",     description="Unlock any block skin",             targetValue=1,   coinReward=50  },
            new AchievementDef { id="tournament_top","title"="Tournament Pro",    description="Finish top 3 in a weekly tournament",targetValue=1,  coinReward=500 },
        };

        [Serializable]
        private class AchDefWrapper { public AchievementDef[] achievements; }
    }
}
