using NUnit.Framework;
using System.Collections.Generic;
using ArrowsPuzzle.Engine;

namespace ArrowsPuzzle.Tests
{
    public class BFSSolverTests
    {
        [Test]
        public void TestBFSFindsOptimalSolution()
        {
            var engine = new PuzzleEngine();
            var blocks = new List<Block>
            {
                new Block { Id = "goal", Color = "red", Orientation = BlockOrientation.Horizontal, Length = 2, Row = 2, Col = 0, Type = BlockType.Normal, IsGoal = true },
                new Block { Id = "obs", Color = "blue", Orientation = BlockOrientation.Vertical, Length = 2, Row = 1, Col = 3, Type = BlockType.Normal, IsGoal = false }
            };
            var gates = new List<Gate>
            {
                new Gate { Color = "red", Side = GateSide.Right, Index = 2, IsMultiColor = false, AcceptedColors = new List<string> { "red" } }
            };

            engine.Initialize(6, 1, 3, blocks, gates, null);

            var sol = BFSSolver.Solve(engine);
            Assert.IsTrue(sol.IsSolvable);
            Assert.IsTrue(sol.OptimalMoveCount > 0);
        }
    }
}
