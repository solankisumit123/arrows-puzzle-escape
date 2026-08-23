using System;
using System.Collections.Generic;
using System.Text;

namespace ArrowsPuzzle.Engine
{
    /// <summary>
    /// Immutable-style 2D occupancy grid. Stores which block id occupies each cell.
    /// Null means empty.
    /// </summary>
    public class GridState
    {
        public int Size { get; private set; }
        private string[,] _cells;

        // Teleporter pairs: cellKey -> paired cellKey
        public Dictionary<string, string> TeleporterPairs { get; private set; }

        public GridState(int size)
        {
            Size = size;
            _cells = new string[size, size];
            TeleporterPairs = new Dictionary<string, string>();
        }

        private GridState(int size, string[,] cells, Dictionary<string, string> teleporters)
        {
            Size = size;
            _cells = cells;
            TeleporterPairs = teleporters;
        }

        public string GetBlockId(int row, int col)
        {
            if (row < 0 || row >= Size || col < 0 || col >= Size) return null;
            return _cells[row, col];
        }

        public bool IsEmpty(int row, int col)
        {
            if (row < 0 || row >= Size || col < 0 || col >= Size) return false;
            return _cells[row, col] == null;
        }

        public bool IsInBounds(int row, int col) => row >= 0 && row < Size && col >= 0 && col < Size;

        public void PlaceBlock(Block block)
        {
            foreach (var (r, c) in block.OccupiedCells())
                _cells[r, c] = block.Id;
        }

        public void RemoveBlock(Block block)
        {
            foreach (var (r, c) in block.OccupiedCells())
                _cells[r, c] = null;
        }

        public void RegisterTeleporterPair(string cellKeyA, string cellKeyB)
        {
            TeleporterPairs[cellKeyA] = cellKeyB;
            TeleporterPairs[cellKeyB] = cellKeyA;
        }

        public string TeleporterKey(int row, int col) => $"{row},{col}";

        public bool IsTeleporter(int row, int col) => TeleporterPairs.ContainsKey(TeleporterKey(row, col));

        public (int row, int col) GetTeleporterExit(int row, int col)
        {
            var key = TeleporterKey(row, col);
            if (TeleporterPairs.TryGetValue(key, out var paired))
            {
                var parts = paired.Split(',');
                return (int.Parse(parts[0]), int.Parse(parts[1]));
            }
            return (row, col);
        }

        public GridState Clone()
        {
            var newCells = (string[,])_cells.Clone();
            var newTele = new Dictionary<string, string>(TeleporterPairs);
            return new GridState(Size, newCells, newTele);
        }

        /// <summary>Canonical hash for BFS visited-set.</summary>
        public string GetHash()
        {
            var sb = new StringBuilder(Size * Size * 4);
            for (int r = 0; r < Size; r++)
                for (int c = 0; c < Size; c++)
                    sb.Append(_cells[r, c] ?? "_").Append('|');
            return sb.ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                    sb.Append((_cells[r, c] ?? ".").PadRight(4));
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
