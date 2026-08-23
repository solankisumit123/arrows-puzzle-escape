using NUnit.Framework;
using System.Collections.Generic;
using ArrowsPuzzle.Engine;

namespace ArrowsPuzzle.Tests
{
    public class MoveValidatorTests
    {
        [Test]
        public void TestCollisionStopsBlock()
        {
            var grid = new GridState(6);
            var b1 = new Block { Id = "b1", Color = "red", Orientation = BlockOrientation.Horizontal, Length = 2, Row = 2, Col = 0 };
            var b2 = new Block { Id = "b2", Color = "blue", Orientation = BlockOrientation.Horizontal, Length = 2, Row = 2, Col = 3 };

            grid.PlaceBlock(b1);
            grid.PlaceBlock(b2);

            var (min, max) = MoveValidator.GetValidMoveRange(b1, grid, new List<Gate>());
            Assert.AreEqual(0, min);
            Assert.AreEqual(1, max); // can only move 1 cell before hitting b2 at col 3
        }
    }
}
