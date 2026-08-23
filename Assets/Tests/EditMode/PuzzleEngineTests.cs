using NUnit.Framework;
using System.Collections.Generic;
using ArrowsPuzzle.Engine;

namespace ArrowsPuzzle.Tests
{
    public class PuzzleEngineTests
    {
        [Test]
        public void TestBlockPlacementAndMove()
        {
            var engine = new PuzzleEngine();
            var blocks = new List<Block>
            {
                new Block { Id = "b1", Color = "red", Orientation = BlockOrientation.Horizontal, Length = 2, Row = 2, Col = 1, Type = BlockType.Normal, IsGoal = true }
            };
            var gates = new List<Gate>
            {
                new Gate { Color = "red", Side = GateSide.Right, Index = 2, IsMultiColor = false, AcceptedColors = new List<string> { "red" } }
            };

            engine.Initialize(6, 1, 1, blocks, gates, null);

            Assert.IsFalse(engine.Grid.IsEmpty(2, 1));
            Assert.IsFalse(engine.Grid.IsEmpty(2, 2));

            // Move right 1
            var outcome = engine.TryMove("b1", 1);
            Assert.AreEqual(MoveResult.Success, outcome.Result);
            Assert.AreEqual(2, engine.GetBlock("b1").Col);
        }

        [Test]
        public void TestGateExitAndWin()
        {
            var engine = new PuzzleEngine();
            var blocks = new List<Block>
            {
                new Block { Id = "b1", Color = "red", Orientation = BlockOrientation.Horizontal, Length = 2, Row = 2, Col = 4, Type = BlockType.Normal, IsGoal = true }
            };
            var gates = new List<Gate>
            {
                new Gate { Color = "red", Side = GateSide.Right, Index = 2, IsMultiColor = false, AcceptedColors = new List<string> { "red" } }
            };

            engine.Initialize(6, 1, 1, blocks, gates, null);

            // Move right through gate
            var outcome = engine.TryMove("b1", 1);
            Assert.AreEqual(MoveResult.ExitedGate, outcome.Result);
            Assert.IsTrue(engine.IsWon());
        }

        [Test]
        public void TestUndoAndReset()
        {
            var engine = new PuzzleEngine();
            var blocks = new List<Block>
            {
                new Block { Id = "b1", Color = "red", Orientation = BlockOrientation.Horizontal, Length = 2, Row = 2, Col = 1, Type = BlockType.Normal, IsGoal = true }
            };
            engine.Initialize(6, 1, 1, blocks, new List<Gate>(), null);

            engine.TryMove("b1", 1);
            Assert.AreEqual(2, engine.GetBlock("b1").Col);

            engine.Undo();
            Assert.AreEqual(1, engine.GetBlock("b1").Col);

            engine.TryMove("b1", 2);
            engine.Reset();
            Assert.AreEqual(1, engine.GetBlock("b1").Col);
            Assert.AreEqual(0, engine.MoveCount);
        }
    }
}
