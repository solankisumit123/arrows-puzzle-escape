using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowsPuzzle.Engine
{
    public enum BlockOrientation { Horizontal, Vertical }
    public enum BlockType { Normal, Ice, Locked, Key, OneWay, Teleporter, Bomb }

    [Serializable]
    public class Block
    {
        public string Id;
        public string Color;
        public BlockOrientation Orientation;
        public int Length;
        public int Row;       // top-left anchor row
        public int Col;       // top-left anchor col
        public BlockType Type;
        public bool IsLocked;
        public bool IsGoal;   // must exit through its gate to win
        public bool HasExited;

        // For ice blocks: once moved, slides until wall
        // For one-way: only positive axis direction allowed
        public bool OneWayPositive;  // true = right/down only

        // For teleporter blocks: cell pair id
        public string TeleporterPairId;

        // For key/lock: which lock id this key activates
        public string ActivatesLockId;

        public Block Clone()
        {
            return (Block)MemberwiseClone();
        }

        /// <summary>Returns all (row,col) cells occupied by this block.</summary>
        public List<(int row, int col)> OccupiedCells()
        {
            var cells = new List<(int, int)>();
            for (int i = 0; i < Length; i++)
            {
                if (Orientation == BlockOrientation.Horizontal)
                    cells.Add((Row, Col + i));
                else
                    cells.Add((Row + i, Col));
            }
            return cells;
        }

        public override string ToString() => $"Block[{Id} {Color} {Orientation} {Length} @ ({Row},{Col}) {Type}]";
    }
}
