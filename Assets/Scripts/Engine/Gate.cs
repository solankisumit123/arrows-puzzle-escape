using System;
using System.Collections.Generic;

namespace ArrowsPuzzle.Engine
{
    public enum GateSide { Left, Right, Top, Bottom }

    [Serializable]
    public class Gate
    {
        public string Color;
        public GateSide Side;
        public int Index;           // row (for Left/Right gates) or col (for Top/Bottom gates)
        public bool IsMultiColor;
        public List<string> AcceptedColors = new List<string>();

        public bool AcceptsBlock(Block block)
        {
            if (IsMultiColor)
                return AcceptedColors.Contains(block.Color);
            return Color == block.Color;
        }

        /// <summary>
        /// Returns whether this gate is aligned with the given block at its current position,
        /// so the block could exit when slid to the edge.
        /// </summary>
        public bool IsAlignedWithBlock(Block block)
        {
            switch (Side)
            {
                case GateSide.Left:
                case GateSide.Right:
                    return block.Orientation == BlockOrientation.Horizontal && block.Row == Index;
                case GateSide.Top:
                case GateSide.Bottom:
                    return block.Orientation == BlockOrientation.Vertical && block.Col == Index;
            }
            return false;
        }
    }
}
