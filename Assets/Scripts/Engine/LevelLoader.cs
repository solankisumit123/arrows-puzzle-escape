using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowsPuzzle.Engine
{
    [Serializable]
    public class LevelData
    {
        public int levelId;
        public int chapterId;
        public int gridSize;
        public int optimalMoves;
        public bool isTutorial;
        public int tutorialStep;          // 1-3 for forced hand-holding
        public string chapterTheme;
        public GateData[] gates;
        public BlockData[] blocks;
        public TeleporterPairData[] teleporterPairs;
        public ChapterData chapter;
    }

    [Serializable]
    public class GateData
    {
        public string color;
        public string side;   // "left","right","top","bottom"
        public int index;
        public bool isMultiColor;
        public string[] acceptedColors;
    }

    [Serializable]
    public class BlockData
    {
        public string id;
        public string color;
        public string orientation; // "H" or "V"
        public int length;
        public int row;
        public int col;
        public string type;        // "normal","ice","locked","key","oneway","teleporter","bomb"
        public bool isLocked;
        public bool isGoal;
        public bool oneWayPositive;
        public string teleporterPairId;
        public string activatesLockId;
    }

    [Serializable]
    public class TeleporterPairData
    {
        public int rowA, colA, rowB, colB;
    }

    [Serializable]
    public class ChapterData
    {
        public int chapterId;
        public string name;
        public string theme;
        public int startLevel;
        public int endLevel;
        public string colorPrimary;
        public string colorSecondary;
        public string backgroundGradientTop;
        public string backgroundGradientBottom;
    }

    /// <summary>
    /// Parses level JSON files → constructs PuzzleEngine instances.
    /// </summary>
    public static class LevelLoader
    {
        public static LevelData LoadFromJson(string json)
        {
            return JsonUtility.FromJson<LevelData>(json);
        }

        public static LevelData LoadFromResources(int levelId, int chapterId)
        {
            string path = $"Levels/Chapter{chapterId:D2}/level_{levelId:D3}";
            var textAsset = Resources.Load<TextAsset>(path);
            if (textAsset == null)
            {
                Debug.LogError($"[LevelLoader] Level not found: {path}");
                return null;
            }
            return LoadFromJson(textAsset.text);
        }

        public static PuzzleEngine BuildEngine(LevelData data)
        {
            var blocks = new List<Block>();
            foreach (var bd in data.blocks)
            {
                var block = new Block
                {
                    Id = bd.id,
                    Color = bd.color,
                    Orientation = bd.orientation == "H" ? BlockOrientation.Horizontal : BlockOrientation.Vertical,
                    Length = bd.length,
                    Row = bd.row,
                    Col = bd.col,
                    Type = ParseBlockType(bd.type),
                    IsLocked = bd.isLocked,
                    IsGoal = bd.isGoal,
                    HasExited = false,
                    OneWayPositive = bd.oneWayPositive,
                    TeleporterPairId = bd.teleporterPairId,
                    ActivatesLockId = bd.activatesLockId
                };
                blocks.Add(block);
            }

            var gates = new List<Gate>();
            foreach (var gd in data.gates)
            {
                var gate = new Gate
                {
                    Color = gd.color,
                    Side = ParseGateSide(gd.side),
                    Index = gd.index,
                    IsMultiColor = gd.isMultiColor,
                    AcceptedColors = gd.acceptedColors != null
                        ? new List<string>(gd.acceptedColors)
                        : new List<string> { gd.color }
                };
                gates.Add(gate);
            }

            var teleporterPairs = new List<(string, string)>();
            if (data.teleporterPairs != null)
            {
                foreach (var tp in data.teleporterPairs)
                {
                    var grid = new GridState(data.gridSize);
                    teleporterPairs.Add((grid.TeleporterKey(tp.rowA, tp.colA), grid.TeleporterKey(tp.rowB, tp.colB)));
                }
            }

            var engine = new PuzzleEngine();
            engine.Initialize(data.gridSize, data.levelId, data.optimalMoves, blocks, gates, teleporterPairs);
            return engine;
        }

        private static BlockType ParseBlockType(string type)
        {
            return type?.ToLower() switch
            {
                "ice" => BlockType.Ice,
                "locked" => BlockType.Locked,
                "key" => BlockType.Key,
                "oneway" => BlockType.OneWay,
                "teleporter" => BlockType.Teleporter,
                "bomb" => BlockType.Bomb,
                _ => BlockType.Normal
            };
        }

        private static GateSide ParseGateSide(string side)
        {
            return side?.ToLower() switch
            {
                "left" => GateSide.Left,
                "right" => GateSide.Right,
                "top" => GateSide.Top,
                "bottom" => GateSide.Bottom,
                _ => GateSide.Right
            };
        }

        /// <summary>Loads all chapter metadata from Resources/Data/chapters.json</summary>
        public static List<ChapterData> LoadAllChapters()
        {
            var asset = Resources.Load<TextAsset>("Data/chapters");
            if (asset == null) return new List<ChapterData>();
            var wrapper = JsonUtility.FromJson<ChapterListWrapper>(asset.text);
            return new List<ChapterData>(wrapper.chapters);
        }

        [Serializable]
        private class ChapterListWrapper { public ChapterData[] chapters; }
    }
}
