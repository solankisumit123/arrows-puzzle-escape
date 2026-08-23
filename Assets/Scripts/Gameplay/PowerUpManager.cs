using UnityEngine;
using ArrowsPuzzle.Systems;

namespace ArrowsPuzzle.Gameplay
{
    public class PowerUpManager : MonoBehaviour
    {
        [SerializeField] private HintSystem _hintSystem;

        public bool UseHint() => _hintSystem != null && _hintSystem.TryShowHint();

        public bool UseBomb(string targetBlockId)
        {
            if (SaveSystem.Data.bombCount <= 0 && !CoinEconomy.SpendCoins(CoinEconomy.BombCost))
                return false;

            if (SaveSystem.Data.bombCount > 0)
            {
                SaveSystem.Data.bombCount--;
                SaveSystem.Save();
            }

            bool success = GameManager.Instance.ApplyBomb(targetBlockId);
            if (success)
            {
                AudioManager.Instance?.PlayBomb();
                AnalyticsManager.TrackPowerUpUsed("bomb");
                AchievementSystem.Increment("bomb_used_10");
            }
            return success;
        }

        public bool UseShuffle()
        {
            if (SaveSystem.Data.shuffleCount <= 0 && !CoinEconomy.SpendCoins(CoinEconomy.ShuffleCost))
                return false;

            if (SaveSystem.Data.shuffleCount > 0)
            {
                SaveSystem.Data.shuffleCount--;
                SaveSystem.Save();
            }

            GameManager.Instance.ApplyShuffle();
            AnalyticsManager.TrackPowerUpUsed("shuffle");
            return true;
        }
    }
}
