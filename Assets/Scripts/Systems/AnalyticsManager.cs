using UnityEngine;

namespace ArrowsPuzzle.Systems
{
    public static class AnalyticsManager
    {
        public static void TrackLevelStart(int levelId, int chapterId)
            => Debug.Log($"[Analytics] STUB: level_start level={levelId} chapter={chapterId}");

        public static void TrackLevelComplete(int levelId, int stars, int moves, float time)
            => Debug.Log($"[Analytics] STUB: level_complete level={levelId} stars={stars} moves={moves} time={time:F1}s");

        public static void TrackPowerUpUsed(string type)
            => Debug.Log($"[Analytics] STUB: powerup_used type={type}");

        public static void TrackAdShown(string type)
            => Debug.Log($"[Analytics] STUB: ad_shown type={type}");

        public static void TrackPurchase(string productId)
            => Debug.Log($"[Analytics] STUB: purchase productId={productId}");

        public static void TrackCustomPuzzleCreated(int gridSize, string difficulty)
            => Debug.Log($"[Analytics] STUB: custom_puzzle_created grid={gridSize} difficulty={difficulty}");
    }
}
