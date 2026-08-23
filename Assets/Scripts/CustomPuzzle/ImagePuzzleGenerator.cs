using System;
using System.Collections.Generic;
using UnityEngine;
using ArrowsPuzzle.Engine;

namespace ArrowsPuzzle.CustomPuzzle
{
    public static class ImagePuzzleGenerator
    {
        private static readonly string[] GateColorList = { "red", "blue", "green", "yellow", "purple", "orange" };

        public static LevelData Generate(Texture2D photo, int gridSize, int scrambleMoves)
        {
            var blocks = new List<BlockData>();
            var gates = new List<GateData>();

            // 1. Divide grid into solved blocks that span across gates
            int blockIdx = 1;
            var occupied = new bool[gridSize, gridSize];

            for (int r = 0; r < gridSize; r++)
            {
                for (int c = 0; c < gridSize; c++)
                {
                    if (occupied[r, c]) continue;

                    // Decide length 2 or 3 if space permits
                    int len = (c + 2 < gridSize && !occupied[r, c + 1] && !occupied[r, c + 2]) ? 3 :
                              (c + 1 < gridSize && !occupied[r, c + 1]) ? 2 : 1;

                    string color = GateColorList[(blockIdx - 1) % GateColorList.Length];

                    blocks.Add(new BlockData
                    {
                        id = $"cp_b{blockIdx}",
                        color = color,
                        orientation = "H",
                        length = len,
                        row = r,
                        col = c,
                        type = "normal",
                        isLocked = false,
                        isGoal = true
                    });

                    // Corresponding exit gate on right side
                    gates.Add(new GateData
                    {
                        color = color,
                        side = "right",
                        index = r,
                        isMultiColor = false,
                        acceptedColors = new string[] { color }
                    });

                    for (int i = 0; i < len; i++) occupied[r, c + i] = true;
                    blockIdx++;
                }
            }

            var levelData = new LevelData
            {
                levelId = 9000,
                chapterId = 0,
                gridSize = gridSize,
                optimalMoves = scrambleMoves,
                isTutorial = false,
                chapterTheme = "custom",
                blocks = blocks.ToArray(),
                gates = gates.ToArray()
            };

            // 2. Build engine & Scramble via ReverseSolver
            var engine = LevelLoader.BuildEngine(levelData);
            ReverseSolver.ScrambleFromSolved(engine, scrambleMoves);

            // 3. Update block starting positions from scrambled engine state
            foreach (var b in blocks)
            {
                var live = engine.GetBlock(b.id);
                if (live != null)
                {
                    b.row = live.Row;
                    b.col = live.Col;
                }
            }

            // 4. Compute true BFS optimal moves
            var solution = BFSSolver.Solve(engine, 50_000);
            levelData.optimalMoves = solution.IsSolvable ? solution.OptimalMoveCount : scrambleMoves;

            return levelData;
        }
    }
}
