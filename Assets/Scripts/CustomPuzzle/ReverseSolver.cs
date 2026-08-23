using System;
using System.Collections.Generic;
using ArrowsPuzzle.Engine;

namespace ArrowsPuzzle.CustomPuzzle
{
    public static class ReverseSolver
    {
        /// <summary>
        /// Scrambles blocks from their solved positions by executing valid reverse moves.
        /// Guaranteed solvable because each step is a reversible legal move.
        /// </summary>
        public static void ScrambleFromSolved(PuzzleEngine engine, int moveCount, System.Random rng = null)
        {
            if (rng == null) rng = new System.Random();

            for (int step = 0; step < moveCount; step++)
            {
                var blocks = engine.GetActiveBlocks();
                if (blocks.Count == 0) break;

                // Pick random block
                var block = blocks[rng.Next(blocks.Count)];

                var (min, max) = MoveValidator.GetValidMoveRange(block, engine.Grid, new List<Gate>());
                int range = max - min;
                if (range <= 0) continue;

                int offset = min + rng.Next(range + 1);
                if (offset == 0) continue;

                // Direct position update on grid state
                engine.Grid.RemoveBlock(block);
                if (block.Orientation == BlockOrientation.Horizontal)
                    block.Col += offset;
                else
                    block.Row += offset;
                engine.Grid.PlaceBlock(block);
            }
        }
    }
}
