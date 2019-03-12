using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;
using WaveFunctionCollapse;

namespace WaveFunctionCollapseTests
{
    [TestClass]
    public class WaveFunctionCollapseTests
    {
        [TestMethod]
        public void BuildPatternsFromSample_ShouldReturnPatterns_Test()
        {
            //var N = 2;
            //var wfc = new WaveFunctionCollapseRunner();
            
            //var tilesA = new List<Point3d>()
            //{
            //    new Point3d(0.0, 1.0, 0.0),
            //    new Point3d(1.0, 0.0, 0.0),
            //    new Point3d(2.0, 2.0, 0.0)
            //};

            //var tilesB = new List<Point3d>()
            //{
            //    new Point3d(0.0, 0.0, 0.0),
            //    new Point3d(2.0, 1.0, 0.0)
            //};

            //var allTiles = new List<Point3d>()
            //{
            //    new Point3d(0.0, 0.0, 0.0),
            //    new Point3d(0.0, 1.0, 0.0),
            //    new Point3d(0.0, 2.0, 0.0),
            //    new Point3d(1.0, 0.0, 0.0),
            //    new Point3d(1.0, 1.0, 0.0),
            //    new Point3d(1.0, 2.0, 0.0),
            //    new Point3d(2.0, 0.0, 0.0),
            //    new Point3d(2.0, 1.0, 0.0),
            //    new Point3d(2.0, 2.0, 0.0),
            //};

            //// weights
            //var halfTileWeight = (float)tilesA.Count / allTiles.Count;
            //var fullTileWeight = (float)tilesB.Count / allTiles.Count;
            //var emptyTileWeight = 1 - (halfTileWeight + fullTileWeight);

            //float[] weights = new float[3] { halfTileWeight, fullTileWeight, emptyTileWeight };

            ////var result = wfc.PatternsFromSample(tilesA, tilesB, allTiles, weights);

            //Assert.AreEqual(16, result.Count);

            //var firstLeftBottomElement = result[0];

            //Assert.AreEqual(State.FULL_TILE, firstLeftBottomElement.MiniTile[0, 0]);
            //Assert.AreEqual(State.HALF_TILE, firstLeftBottomElement.MiniTile[0, 1]);
            //Assert.AreEqual(State.HALF_TILE, firstLeftBottomElement.MiniTile[1, 0]);
            //Assert.AreEqual(State.EMPTY, firstLeftBottomElement.MiniTile[1, 1]);
        }

        [TestMethod]
        public void BuildPropagator_ShouldReturnProper_Test()
        {
        //    var N = 2;
        //    var wfc = new WaveFunctionCollapseRunner();
            
        //    // weights
        //    var halfTileWeight = (float)2/9;
        //    var fullTileWeight = (float)3/9;
        //    var emptyTileWeight = 1 - (halfTileWeight + fullTileWeight);

        //    float[] weights = new float[3] { halfTileWeight, fullTileWeight, emptyTileWeight };

        //    var patternsFromSample = new List<Pattern>
        //    {
        //        new Pattern( new State[2, 2] { { State.FULL_TILE, State.HALF_TILE }, { State.HALF_TILE, State.EMPTY } }, weights, N),
        //        new Pattern( new State[2, 2] { { State.HALF_TILE, State.EMPTY }, { State.EMPTY, State.FULL_TILE } }, weights, N),
        //        new Pattern( new State[2, 2] { { State.HALF_TILE, State.EMPTY }, { State.EMPTY, State.EMPTY } }, weights, N),
        //        new Pattern( new State[2, 2] { { State.EMPTY, State.EMPTY }, { State.FULL_TILE, State.HALF_TILE } }, weights, N),

        //        new Pattern( new State[2, 2] { { State.HALF_TILE, State.FULL_TILE }, { State.EMPTY, State.HALF_TILE } }, weights, N),
        //        new Pattern( new State[2, 2] { { State.EMPTY, State.HALF_TILE }, { State.HALF_TILE, State.FULL_TILE } }, weights, N),
        //        new Pattern( new State[2, 2] { { State.HALF_TILE, State.EMPTY }, { State.FULL_TILE, State.HALF_TILE } }, weights, N),
        //        new Pattern( new State[2, 2] { { State.EMPTY, State.HALF_TILE }, { State.FULL_TILE, State.EMPTY } }, weights, N),

        //        new Pattern( new State[2, 2] { { State.FULL_TILE, State.EMPTY }, { State.EMPTY, State.HALF_TILE } }, weights, N),
        //        new Pattern( new State[2, 2] { { State.EMPTY, State.FULL_TILE }, { State.HALF_TILE, State.EMPTY } }, weights, N),
        //        new Pattern( new State[2, 2] { { State.EMPTY, State.HALF_TILE }, { State.EMPTY, State.EMPTY } }, weights, N),
        //        new Pattern( new State[2, 2] { { State.EMPTY, State.EMPTY }, { State.EMPTY, State.HALF_TILE } }, weights, N),

        //        new Pattern( new State[2, 2] { { State.EMPTY, State.EMPTY }, { State.HALF_TILE, State.EMPTY } }, weights, N),
        //        new Pattern( new State[2, 2] { { State.FULL_TILE, State.EMPTY }, { State.HALF_TILE, State.EMPTY } }, weights, N),
        //        new Pattern( new State[2, 2] { { State.HALF_TILE, State.FULL_TILE }, { State.EMPTY, State.EMPTY } }, weights, N),
        //        new Pattern( new State[2, 2] { { State.EMPTY, State.HALF_TILE }, { State.EMPTY, State.FULL_TILE } }, weights, N)
        //    };

        //    wfc.BuildPropagator(N, patternsFromSample);

        //    var firstPattern = patternsFromSample[0];

        //    var firstPatternLeftBottomCornerSuperposition = firstPattern.overlapsSuperpositions[0, 0];

        //    var numberOfTrueElements = firstPatternLeftBottomCornerSuperposition.coefficients.Count(c => c);

        //    Assert.AreEqual(3, numberOfTrueElements);

        //    Assert.AreEqual(2, firstPatternLeftBottomCornerSuperposition.Entropy);
        }
    }
}
