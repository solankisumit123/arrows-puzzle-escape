using System.Collections.Generic;
using System.Linq;

namespace ArrowsPuzzle.Engine
{
    public class SolutionPath
    {
        public bool IsSolvable;
        public List<Move> Moves = new List<Move>();
        public int OptimalMoveCount => Moves.Count;
    }

    /// <summary>
    /// BFS solver over puzzle states.
    /// Pure C#, no Unity dependencies.
    /// Used for: hint system, optimal-move count, level validation, custom puzzle generation.
    /// </summary>
    public static class BFSSolver
    {
        private const int MaxStates = 500_000;

        /// <summary>
        /// Solves from the current engine state.
        /// Returns a SolutionPath with the optimal sequence of moves.
        /// </summary>
        public static SolutionPath Solve(PuzzleEngine engine, int maxStates = MaxStates)
        {
            if (engine.IsWon())
                return new SolutionPath { IsSolvable = true };

            // BFS node: (state hash, move list to reach this state, serialized block positions)
            var initial = CloneEngineState(engine);
            var queue = new Queue<BFSNode>();
            var visited = new HashSet<string>();

            var startHash = engine.GetStateHash();
            visited.Add(startHash);
            queue.Enqueue(new BFSNode { Engine = initial, Moves = new List<Move>() });

            while (queue.Count > 0 && visited.Count < maxStates)
            {
                var node = queue.Dequeue();
                var eng = node.Engine;

                foreach (var block in eng.GetActiveBlocks())
                {
                    if (block.IsLocked || block.HasExited) continue;
                    if (block.Type == BlockType.Bomb) continue;

                    var (min, max) = MoveValidator.GetValidMoveRange(block, eng.Grid, eng.Gates.ToList());

                    for (int offset = min; offset <= max; offset++)
                    {
                        if (offset == 0) continue;

                        var childEng = CloneEngineState(eng);
                        var outcome = childEng.TryMove(block.Id, offset);

                        if (outcome.Result == MoveResult.Invalid || outcome.Result == MoveResult.Blocked)
                            continue;

                        var newMoves = new List<Move>(node.Moves) { new Move(block.Id, offset) };

                        if (childEng.IsWon())
                            return new SolutionPath { IsSolvable = true, Moves = newMoves };

                        var hash = childEng.GetStateHash();
                        if (!visited.Contains(hash))
                        {
                            visited.Add(hash);
                            queue.Enqueue(new BFSNode { Engine = childEng, Moves = newMoves });
                        }
                    }
                }
            }

            return new SolutionPath { IsSolvable = false };
        }

        /// <summary>Gets just the next best move (for the hint system).</summary>
        public static Move? GetNextHintMove(PuzzleEngine engine)
        {
            var solution = Solve(engine, 100_000);
            if (solution.IsSolvable && solution.Moves.Count > 0)
                return solution.Moves[0];
            return null;
        }

        // Deep clone engine state for BFS branching
        private static PuzzleEngine CloneEngineState(PuzzleEngine source)
        {
            var blocks = source.Blocks.Select(b => b.Clone()).ToList();
            var gates = source.Gates.ToList();
            var eng = new PuzzleEngine();
            var teleporterPairs = new List<(string, string)>();
            foreach (var kv in source.Grid.TeleporterPairs)
            {
                var a = kv.Key; var b = kv.Value;
                if (string.Compare(a, b) < 0)
                    teleporterPairs.Add((a, b));
            }
            eng.Initialize(source.GridSize, source.LevelId, source.OptimalMoves, blocks, gates, teleporterPairs);
            // Restore HasExited flags and positions
            foreach (var srcBlock in source.Blocks)
            {
                var dstBlock = eng.GetBlock(srcBlock.Id);
                if (dstBlock != null)
                {
                    dstBlock.Row = srcBlock.Row;
                    dstBlock.Col = srcBlock.Col;
                    dstBlock.HasExited = srcBlock.HasExited;
                    dstBlock.IsLocked = srcBlock.IsLocked;
                }
            }
            return eng;
        }

        private class BFSNode
        {
            public PuzzleEngine Engine;
            public List<Move> Moves;
        }
    }
}
