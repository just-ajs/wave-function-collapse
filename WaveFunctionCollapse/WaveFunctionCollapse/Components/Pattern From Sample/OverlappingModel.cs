using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WaveFunctionCollapse
{
    public class PatternComponent : GH_Component
    {

        public PatternComponent()
          : base("Overlapping Model", "Patterns Library",  "This will create library of pattern from provided sample", "WFC", "Pattern From Sample")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddPointParameter("Tile Type A", "Type A", "List of all centers of tiles of type A", GH_ParamAccess.list);
            pManager.AddPointParameter("Tile Type B", "Type B", "List of all centers of tiles of type B", GH_ParamAccess.list);
            pManager.AddPointParameter("Whole Area", "Tile Points", "List of all centers in tile design space", GH_ParamAccess.list);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new PatternFromSampleParam());
            pManager.AddPointParameter("Half tiles", "", "", GH_ParamAccess.list);
            pManager.AddPointParameter("Full tiles", "", "", GH_ParamAccess.list);
            pManager.AddPointParameter("Empty tiles", "", "", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //  TILES A: center points
            List<Point3d> tilesA = new List<Point3d>();
            DA.GetDataList<Point3d>(0, tilesA);

            // TILES B: center points
            List<Point3d> tilesB = new List<Point3d>();
            DA.GetDataList<Point3d>(1, tilesB);

            // ALL TILES (A+B+NULL): CENTER POINTS
            List<Point3d> allTiles = new List<Point3d>();
            DA.GetDataList<Point3d>(2, allTiles);

            float[] weights = GetWeights(tilesA, tilesB, allTiles);

            // tiles for patterns from sample
            var halfTilesForPatterns = new List<Point3d>();
            var fullTilesForPatterns = new List<Point3d>();
            var emptyTilesForPatterns = new List<Point3d>();

            PatternFromSampleExtractor patternsToGenerate = new PatternFromSampleExtractor(tilesA, tilesB, allTiles, weights, 2);
            var patternsFromSample = patternsToGenerate.Extract();

            var result = new PatternFromSampleElement();
            result.Patterns = patternsFromSample;
            result.UnitElementsOfType0 = tilesA;
            result.UnitElementsOfType1 = tilesB;
            result.UnitElementsCenters = allTiles;
            result.TilesWeights = weights;

            int x = 20, y = 0, z = 0;


            for (var i = 0; i < result.Patterns.Count; i++)
            {
                    var pattern = result.Patterns[i];
                    var instance = pattern.Instantiate(x + i * 6, y, z);

                    halfTilesForPatterns.AddRange(instance[State.HALF_TILE]);
                    fullTilesForPatterns.AddRange(instance[State.FULL_TILE]);
                    emptyTilesForPatterns.AddRange(instance[State.EMPTY]);
            }

            var r = new GH_PatternsFromSample(result);
            
            DA.SetData(0, r);
            DA.SetDataList(1, halfTilesForPatterns);
            DA.SetDataList(2, fullTilesForPatterns);
            DA.SetDataList(3, emptyTilesForPatterns);
        }

        // Calculate weights based on percentage of each tile in sample
        float[] GetWeights (List<Point3d> a, List<Point3d> b, List<Point3d> all)
        {
            float[] weights = new float[3];

            // weights
            float halfTileWeight = (float)a.Count / all.Count;
            float fullTileWeight = (float)b.Count / all.Count;
            float emptyTileWeight = 1 - (halfTileWeight + fullTileWeight);

            weights[0] = halfTileWeight;
            weights[1] = fullTileWeight;
            weights[2] = emptyTileWeight;

            return weights;
       }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("3fac7ab0-722f-7eb0-b65a-e53640525e4b"); }

        }
    }
}