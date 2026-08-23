using System.Collections.Generic;

namespace ArrowsPuzzle.Engine
{
    public enum MoveResult { Success, Blocked, ExitedGate, Invalid }

    /// <summary>
    /// Pure static move validation. No Unity dependencies.
    /// </summary>
    public static class MoveValidator
    {
        /// <summary>
        /// Computes the valid offset range [minOffset, maxOffset] for a block along its axis.
        /// Positive = right/down. Negative = left/up.
        /// </summary>
        public static (int min, int max) GetValidMoveRange(Block block, GridState grid, List<Gate> gates)
        {
            if (block.IsLocked) return (0, 0);
            if (block.Type == BlockType.Bomb) return (0, 0); // fixed obstacle

            grid.RemoveBlock(block);

            int min = 0, max = 0;

            if (block.Orientation == BlockOrientation.Horizontal)
            {
                // Slide left
                if (!block.OneWayPositive)
                {
                    for (int offset = -1; offset >= -grid.Size; offset--)
                    {
                        int newCol = block.Col + offset;
                        // Check if left gate exit
                        if (newCol < 0)
                        {
                            if (HasMatchingGate(block, gates, GateSide.Left))
                                min = offset; // can exit
                            break;
                        }
                        if (!grid.IsEmpty(block.Row, newCol)) break;
                        min = offset;
                    }
                }
                // Slide right
                for (int offset = 1; offset <= grid.Size; offset++)
                {
                    int newCol = block.Col + block.Length - 1 + offset;
                    if (newCol >= grid.Size)
                    {
                        if (HasMatchingGate(block, gates, GateSide.Right))
                            max = offset; // can exit
                        break;
                    }
                    if (!grid.IsEmpty(block.Row, newCol)) break;
                    max = offset;
                }
            }
            else // Vertical
            {
                // Slide up
                if (!block.OneWayPositive)
                {
                    for (int offset = -1; offset >= -grid.Size; offset--)
                    {
                        int newRow = block.Row + offset;
                        if (newRow < 0)
                        {
                            if (HasMatchingGate(block, gates, GateSide.Top))
                                min = offset;
                            break;
                        }
                        if (!grid.IsEmpty(newRow, block.Col)) break;
                        min = offset;
                    }
                }
                // Slide down
                for (int offset = 1; offset <= grid.Size; offset++)
                {
                    int newRow = block.Row + block.Length - 1 + offset;
                    if (newRow >= grid.Size)
                    {
                        if (HasMatchingGate(block, gates, GateSide.Bottom))
                            max = offset;
                        break;
                    }
                    if (!grid.IsEmpty(newRow, block.Col)) break;
                    max = offset;
                }
            }

            grid.PlaceBlock(block);
            return (min, max);
        }

        public static bool WouldExitGate(Block block, int offset, GridState grid, List<Gate> gates)
        {
            grid.RemoveBlock(block);
            bool exits = false;

            if (block.Orientation == BlockOrientation.Horizontal)
            {
                int newCol = block.Col + offset;
                int rightEnd = newCol + block.Length - 1;
                if (offset < 0 && newCol < 0 && HasMatchingGate(block, gates, GateSide.Left))
                    exits = true;
                if (offset > 0 && rightEnd >= grid.Size && HasMatchingGate(block, gates, GateSide.Right))
                    exits = true;
            }
            else
            {
                int newRow = block.Row + offset;
                int bottomEnd = newRow + block.Length - 1;
                if (offset < 0 && newRow < 0 && HasMatchingGate(block, gates, GateSide.Top))
                    exits = true;
                if (offset > 0 && bottomEnd >= grid.Size && HasMatchingGate(block, gates, GateSide.Bottom))
                    exits = true;
            }

            grid.PlaceBlock(block);
            return exits;
        }

        /// <summary>
        /// For ice blocks: compute how far they slide until hitting wall or another block.
        /// Returns the max legal offset in the given direction.
        /// </summary>
        public static int IceSlideDistance(Block block, int direction, GridState grid, List<Gate> gates)
        {
            grid.RemoveBlock(block);
            int slide = 0;
            int step = direction > 0 ? 1 : -1;

            if (block.Orientation == BlockOrientation.Horizontal)
            {
                for (int offset = step; ; offset += step)
                {
                    int checkCol = offset > 0
                        ? block.Col + block.Length - 1 + offset
                        : block.Col + offset;
                    if (checkCol < 0)
                    {
                        if (HasMatchingGate(block, gates, GateSide.Left)) slide = offset;
                        break;
                    }
                    if (checkCol >= grid.Size)
                    {
                        if (HasMatchingGate(block, gates, GateSide.Right)) slide = offset;
                        break;
                    }
                    if (!grid.IsEmpty(block.Row, checkCol)) break;
                    slide = offset;
                }
            }
            else
            {
                for (int offset = step; ; offset += step)
                {
                    int checkRow = offset > 0
                        ? block.Row + block.Length - 1 + offset
                        : block.Row + offset;
                    if (checkRow < 0)
                    {
                        if (HasMatchingGate(block, gates, GateSide.Top)) slide = offset;
                        break;
                    }
                    if (checkRow >= grid.Size)
                    {
                        if (HasMatchingGate(block, gates, GateSide.Bottom)) slide = offset;
                        break;
                    }
                    if (!grid.IsEmpty(checkRow, block.Col)) break;
                    slide = offset;
                }
            }

            grid.PlaceBlock(block);
            return slide;
        }

        private static bool HasMatchingGate(Block block, List<Gate> gates, GateSide side)
        {
            foreach (var gate in gates)
            {
                if (gate.Side == side && gate.IsAlignedWithBlock(block) && gate.AcceptsBlock(block))
                    return true;
            }
            return false;
        }
    }
}
