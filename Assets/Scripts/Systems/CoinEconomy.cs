using System;
using UnityEngine;

namespace ArrowsPuzzle.Systems
{
    public static class CoinEconomy
    {
        public static Action<int, int> OnCoinsChanged; // (oldAmount, newAmount)

        public static int Coins => SaveSystem.Data.totalCoins;

        public static void AddCoins(int amount)
        {
            int old = SaveSystem.Data.totalCoins;
            SaveSystem.Data.totalCoins += amount;
            SaveSystem.Save();
            OnCoinsChanged?.Invoke(old, SaveSystem.Data.totalCoins);
        }

        public static bool SpendCoins(int amount)
        {
            if (SaveSystem.Data.totalCoins < amount) return false;
            int old = SaveSystem.Data.totalCoins;
            SaveSystem.Data.totalCoins -= amount;
            SaveSystem.Save();
            OnCoinsChanged?.Invoke(old, SaveSystem.Data.totalCoins);
            return true;
        }

        // ─── Level Reward ────────────────────────────────────────────────────
        public static int CalcLevelReward(int stars, int chapterId)
        {
            int baseReward = 20 + chapterId * 5;
            return baseReward * stars;
        }

        // ─── Power-up costs ──────────────────────────────────────────────────
        public const int HintCost     = 50;
        public const int BombCost     = 80;
        public const int ShuffleCost  = 40;
        public const int ExtraMoveCost = 30;
        public const int UndoCost     = 0;  // free but limited to 1-step

        // ─── Shop packs ──────────────────────────────────────────────────────
        public const int SmallPackCoins  = 500;
        public const int MediumPackCoins = 1500;
        public const int LargePackCoins  = 5000;
    }
}
