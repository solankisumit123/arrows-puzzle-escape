using System;
using System.Collections.Generic;
using System.Linq;

namespace ArrowsPuzzle.Engine
{
    public struct Move
    {
        public string BlockId;
        public int Offset;
        public Move(string blockId, int offset) { BlockId = blockId; Offset = offset; }
        public override string ToString() => $"Move({BlockId},{Offset:+#;-#;0})";
    }

    public struct MoveOutcome
    {
        public MoveResult Result;
        public bool BlockExited;
        public string ExitedBlockId;
        public bool UnlockedBlock;
        public string UnlockedBlockId;
        public bool TeleportOccurred;
        public (int row, int col) TeleportDestination;
    }

    /// <summary>
    /// Core puzzle engine. Pure C#, no Unity dependencies.
    /// Owns grid state, block list, gate list, move history.
    /// </summary>
    public class PuzzleEngine
    {
        public int GridSize { get; private set; }
        public int LevelId { get; private set; }
        public int OptimalMoves { get; private set; }
        public int MoveCount { get; private set; }

        public IReadOnlyList<Block> Blocks => _blocks;
        public IReadOnlyList<Gate> Gates => _gates;
        public GridState Grid => _grid;

        private List<Block> _blocks;
        private List<Gate> _gates;
        private GridState _grid;

        // Move history for undo
        private Stack<(List<Block> blocks, GridState grid, int moveCount)> _history
            = new Stack<(List<Block>, GridState, int)>();

        // Initial state for reset
        private List<Block> _initialBlocks;
        private GridState _initialGrid;

        // Events (used by Unity layer via polling or direct reference)
        public Action<string, int> OnBlockMoved;          // (blockId, offset)
        public Action<string> OnBlockExited;              // (blockId)
        public Action<string> OnBlockUnlocked;            // (blockId)
        public Action OnLevelComplete;
        public Action<string, (int,int)> OnBlockTeleported; // (blockId, dest)
        public Action<string, int, int> OnIceSlide;      // (blockId, fromOffset, toOffset)

        public PuzzleEngine() { }

        public void Initialize(int gridSize, int levelId, int optimalMoves,
            List<Block> blocks, List<Gate> gates, List<(string, string)> teleporterPairs)
        {
            GridSize = gridSize;
            LevelId = levelId;
            OptimalMoves = optimalMoves;
            MoveCount = 0;

            _gates = gates ?? new List<Gate>();
            _grid = new GridState(gridSize);

            foreach (var pair in teleporterPairs ?? new List<(string, string)>())
                _grid.RegisterTeleporterPair(pair.Item1, pair.Item2);

            _blocks = blocks ?? new List<Block>();
            foreach (var block in _blocks)
                _grid.PlaceBlock(block);

            // Save initial state
            _initialBlocks = _blocks.Select(b => b.Clone()).ToList();
            _initialGrid = _grid.Clone();
            _history.Clear();
        }

        // ─── Move ─────────────────────────────────────────────────────────────

        public MoveOutcome TryMove(string blockId, int requestedOffset)
        {
            var block = GetBlock(blockId);
            if (block == null) return new MoveOutcome { Result = MoveResult.Invalid };
            if (block.IsLocked || block.HasExited) return new MoveOutcome { Result = MoveResult.Blocked };
            if (requestedOffset == 0) return new MoveOutcome { Result = MoveResult.Invalid };

            int direction = requestedOffset > 0 ? 1 : -1;

            // Ice blocks: override offset to full slide distance
            int actualOffset = requestedOffset;
            if (block.Type == BlockType.Ice)
                actualOffset = MoveValidator.IceSlideDistance(block, direction, _grid, _gates) ;

            if (actualOffset == 0) return new MoveOutcome { Result = MoveResult.Blocked };

            // One-way restriction
            if (block.Type == BlockType.OneWay && !block.OneWayPositive && direction > 0)
                return new MoveOutcome { Result = MoveResult.Blocked };
            if (block.Type == BlockType.OneWay && block.OneWayPositive && direction < 0)
                return new MoveOutcome { Result = MoveResult.Blocked };

            var (min, max) = MoveValidator.GetValidMoveRange(block, _grid, _gates);
            if (actualOffset < min || actualOffset > max)
                return new MoveOutcome { Result = MoveResult.Blocked };

            // Push undo snapshot
            PushHistory();

            var outcome = new MoveOutcome { Result = MoveResult.Success };

            // Check for gate exit
            bool willExit = MoveValidator.WouldExitGate(block, actualOffset, _grid, _gates);

            if (willExit)
            {
                _grid.RemoveBlock(block);
                block.HasExited = true;
                outcome.Result = MoveResult.ExitedGate;
                outcome.BlockExited = true;
                outcome.ExitedBlockId = blockId;
                OnBlockExited?.Invoke(blockId);
                OnBlockMoved?.Invoke(blockId, actualOffset);
            }
            else
            {
                // Check for teleporter
                _grid.RemoveBlock(block);
                bool teleported = false;

                if (block.Type == BlockType.Teleporter)
                {
                    // Check if leading edge lands on a teleporter cell after move
                    var newRow = block.Row + (block.Orientation == BlockOrientation.Vertical ? actualOffset : 0);
                    var newCol = block.Col + (block.Orientation == BlockOrientation.Horizontal ? actualOffset : 0);
                    // Check all occupied cells of new position
                    for (int i = 0; i < block.Length; i++)
                    {
                        int cr = newRow + (block.Orientation == BlockOrientation.Vertical ? i : 0);
                        int cc = newCol + (block.Orientation == BlockOrientation.Horizontal ? i : 0);
                        if (_grid.IsTeleporter(cr, cc))
                        {
                            var (tr, tc) = _grid.GetTeleporterExit(cr, cc);
                            // Offset from teleporter entrance to block anchor
                            int dr = i > 0 && block.Orientation == BlockOrientation.Vertical ? -i : 0;
                            int dc = i > 0 && block.Orientation == BlockOrientation.Horizontal ? -i : 0;
                            block.Row = tr + dr;
                            block.Col = tc + dc;
                            teleported = true;
                            outcome.TeleportOccurred = true;
                            outcome.TeleportDestination = (block.Row, block.Col);
                            OnBlockTeleported?.Invoke(blockId, (block.Row, block.Col));
                            break;
                        }
                    }
                }

                if (!teleported)
                {
                    if (block.Orientation == BlockOrientation.Horizontal)
                        block.Col += actualOffset;
                    else
                        block.Row += actualOffset;
                }

                _grid.PlaceBlock(block);
                OnBlockMoved?.Invoke(blockId, actualOffset);

                // Key block: check if it activated a switch cell
                if (block.Type == BlockType.Key && !string.IsNullOrEmpty(block.ActivatesLockId))
                {
                    var lockBlock = GetBlock(block.ActivatesLockId);
                    if (lockBlock != null && lockBlock.IsLocked)
                    {
                        lockBlock.IsLocked = false;
                        outcome.UnlockedBlock = true;
                        outcome.UnlockedBlockId = block.ActivatesLockId;
                        OnBlockUnlocked?.Invoke(block.ActivatesLockId);
                    }
                }
            }

            MoveCount++;

            if (IsWon())
                OnLevelComplete?.Invoke();

            return outcome;
        }

        // ─── Undo / Reset ─────────────────────────────────────────────────────

        public bool CanUndo => _history.Count > 0;

        public void Undo()
        {
            if (!CanUndo) return;
            var (blocks, grid, moveCount) = _history.Pop();
            _blocks = blocks;
            _grid = grid;
            MoveCount = moveCount;
        }

        public void Reset()
        {
            _blocks = _initialBlocks.Select(b => b.Clone()).ToList();
            _grid = _initialGrid.Clone();
            // Re-place all blocks
            foreach (var block in _blocks)
                _grid.PlaceBlock(block);
            MoveCount = 0;
            _history.Clear();
        }

        // ─── Win Check ────────────────────────────────────────────────────────

        public bool IsWon()
        {
            foreach (var block in _blocks)
                if (block.IsGoal && !block.HasExited) return false;
            return true;
        }

        // ─── Power-Ups ────────────────────────────────────────────────────────

        /// <summary>Bomb: removes a Bomb-type fixed obstacle block.</summary>
        public bool ApplyBomb(string targetBlockId)
        {
            var block = GetBlock(targetBlockId);
            if (block == null || block.Type != BlockType.Bomb) return false;
            PushHistory();
            _grid.RemoveBlock(block);
            _blocks.Remove(block);
            return true;
        }

        /// <summary>Shuffle: random scramble of current board (not counted in move history undo).</summary>
        public void ApplyShuffle(System.Random rng = null)
        {
            if (rng == null) rng = new System.Random();
            for (int attempt = 0; attempt < 200; attempt++)
            {
                var movableBlocks = _blocks.FindAll(b => !b.IsLocked && !b.HasExited && b.Type != BlockType.Bomb);
                if (movableBlocks.Count == 0) break;
                var pick = movableBlocks[rng.Next(movableBlocks.Count)];
                var (min, max) = MoveValidator.GetValidMoveRange(pick, _grid, _gates);
                int range = max - min;
                if (range <= 0) continue;
                int offset = min + rng.Next(range + 1);
                if (offset == 0) continue;
                TryMove(pick.Id, offset);
            }
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        public Block GetBlock(string id) => _blocks.Find(b => b.Id == id);
        public List<Block> GetActiveBlocks() => _blocks.FindAll(b => !b.HasExited);

        public (int min, int max) GetValidMoveRange(string blockId)
        {
            var block = GetBlock(blockId);
            if (block == null) return (0, 0);
            return MoveValidator.GetValidMoveRange(block, _grid, _gates);
        }

        public int GetStarRating(float adaptiveFactor = 1f)
        {
            int effectiveOptimal = Mathf.RoundToInt(OptimalMoves * adaptiveFactor);
            if (MoveCount <= effectiveOptimal) return 3;
            if (MoveCount <= (int)(effectiveOptimal * 1.5f)) return 2;
            return 1;
        }

        private void PushHistory()
        {
            _history.Push((
                _blocks.Select(b => b.Clone()).ToList(),
                _grid.Clone(),
                MoveCount
            ));
        }

        // ─── Snapshot for BFS ─────────────────────────────────────────────────

        public string GetStateHash() => _grid.GetHash() +
            string.Join("|", _blocks.Select(b => b.HasExited ? "X" : $"{b.Row},{b.Col}"));
    }
}
