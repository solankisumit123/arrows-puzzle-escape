using UnityEngine;
using ArrowsPuzzle.Engine;
using ArrowsPuzzle.Systems;

namespace ArrowsPuzzle.Gameplay
{
    public class HintSystem : MonoBehaviour
    {
        [SerializeField] private GridView _gridView;
        private string _activeHintBlockId;

        public bool TryShowHint()
        {
            if (GameManager.Instance.IsLevelComplete || GameManager.Instance.IsPaused) return false;

            if (SaveSystem.Data.hintCount <= 0 && !CoinEconomy.SpendCoins(CoinEconomy.HintCost))
            {
                Debug.Log("[HintSystem] Not enough coins for hint.");
                return false;
            }

            if (SaveSystem.Data.hintCount > 0)
            {
                SaveSystem.Data.hintCount--;
                SaveSystem.Save();
            }

            var nextMove = BFSSolver.GetNextHintMove(GameManager.Instance.Engine);
            if (nextMove.HasValue)
            {
                ClearHint();
                _activeHintBlockId = nextMove.Value.BlockId;
                var bv = _gridView.GetBlockView(_activeHintBlockId);
                if (bv != null)
                {
                    bv.ShowHintGlow(true, nextMove.Value.Offset > 0);
                    GameManager.Instance.OnHintUsed();
                    AnalyticsManager.TrackPowerUpUsed("hint");
                    return true;
                }
            }
            return false;
        }

        public void ClearHint()
        {
            if (!string.IsNullOrEmpty(_activeHintBlockId))
            {
                var bv = _gridView.GetBlockView(_activeHintBlockId);
                if (bv != null) bv.ShowHintGlow(false);
                _activeHintBlockId = null;
            }
        }
    }
}
