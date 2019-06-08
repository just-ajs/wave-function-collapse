using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace WaveFunctionCollapse
{
    public class VisualiseByHistoryComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public VisualiseByHistoryComponent()
          : base("Visualise by History", "Nickname",  "Description",
              "WFC", "Wave Fucntion Collapse")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new PatternHistoryParam(), "Wave Function Collapsed", "WFC", "Wave Function Collapse with history", GH_ParamAccess.item);
            pManager.AddNumberParameter("Timelapse", "Timelapse", "Timelapse to visualise", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("TILE 01", "TILE 01", "First tile type here", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("TILE 02", "TILE 02", "First tile type here", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("TILE 03", "TILE 03", "First tile type here", GH_ParamAccess.item);
            pManager.AddNumberParameter("X WAVE LOCATION", "X", "X", GH_ParamAccess.item);
            pManager.AddNumberParameter("Y WAVE LOCATION", "Y", "Y", GH_ParamAccess.item);
            pManager.AddColourParameter("tile 01 colour", "", "", GH_ParamAccess.item);
            pManager.AddColourParameter("tile 02 colour", "", "", GH_ParamAccess.item);
            pManager.AddColourParameter("tile empty", "", "", GH_ParamAccess.item);


        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddSurfaceParameter("wave: half surfaces", "", "", GH_ParamAccess.list);
            pManager.AddSurfaceParameter("wave: full surfaces", "", "", GH_ParamAccess.list);
            pManager.AddSurfaceParameter("wave: empty surfaces", "", "", GH_ParamAccess.list);
            pManager.AddColourParameter("colours", "", "", GH_ParamAccess.list);
            pManager.AddSurfaceParameter("uncollapsed", "", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("pattern count", "", "", GH_ParamAccess.list);
           

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_WaveCollapseHistory waveCollapseHistory = new GH_WaveCollapseHistory();
            DA.GetData<GH_WaveCollapseHistory>(0, ref waveCollapseHistory);

            double frameFromTimeLapse = new double();
            DA.GetData<double>(1, ref frameFromTimeLapse);

            Brep surfaceType01 = new Brep();
            DA.GetData<Brep>(2, ref surfaceType01);

            Brep surfaceType02 = new Brep();
            DA.GetData<Brep>(3, ref surfaceType02);


            Brep surfaceType03 = new Brep();
            DA.GetData<Brep>(4, ref surfaceType03);


            double wavePositionX = new double();
            DA.GetData<double>(5, ref wavePositionX);

            double wavePositionY = new double();
            DA.GetData<double>(6, ref wavePositionY);

            Color tile01Colour = new Color();
            DA.GetData<Color>(7, ref tile01Colour);

            Color tile02Colour = new Color();
            DA.GetData<Color>(8, ref tile02Colour);

            Color tile03Colour = new Color();
            DA.GetData<Color>(9, ref tile03Colour);

            var waveElements = GetWaveAtSpecificTime(waveCollapseHistory, frameFromTimeLapse);

            var uncollapsed = waveElements.Uncollapsed;

            var tempUncollapsed = GetUncollapsedPoints(uncollapsed);

            // move tiles on canvas
            var halfTilesRelocated = MovePointsOnCanvas((int)wavePositionX, (int)wavePositionY, waveElements.HalfTile);
            var fullTilesRelocated = MovePointsOnCanvas((int)wavePositionX, (int)wavePositionY, waveElements.FullTile);
            var emptyRelocated = MovePointsOnCanvas((int)wavePositionX, (int)wavePositionY, waveElements.Empty);

            // change all lists to vectors
            var halfTilesRelocatedVectors = pointsToVectors(halfTilesRelocated);
            var fullTilesRelocatedVectors = pointsToVectors(fullTilesRelocated);
            var emptyTilesRelocatedVectors = pointsToVectors(emptyRelocated);

            // move uncollapsed on canvas
            var uncollapsedMoved = MovePointsOnCanvas((int)wavePositionX, (int)wavePositionY, tempUncollapsed);
            var uncollapsedRelocatedVectors = pointsToVectors(uncollapsedMoved);


            // duplicate surfaces to calculated locations
            var duplicatedHalfSurfaces = DuplicateTiles(surfaceType01, halfTilesRelocatedVectors);
            var duplicatedFullSurfaces = DuplicateTiles(surfaceType02, fullTilesRelocatedVectors);
            var duplicatedEmptySurfaces = DuplicateTiles(surfaceType03, emptyTilesRelocatedVectors);
            var duplicatedUncollapsed = DuplicateTiles(surfaceType01, uncollapsedRelocatedVectors);

            var colours = GetUncollapsedColors(uncollapsed, tile01Colour, tile02Colour, tile03Colour);

            DA.SetDataList(0, duplicatedHalfSurfaces);
            DA.SetDataList(1, duplicatedFullSurfaces);
            DA.SetDataList(2, duplicatedEmptySurfaces);
            DA.SetDataList(3, colours);
            DA.SetDataList(4, duplicatedUncollapsed);
            DA.SetDataList(5, waveElements.PatternOccurence);
        }


        IEnumerable<float> CalculateTileTypeDistances (List<Vector3d> oneTileType)
        {
            float averageDistance = 0;

            for (int i = 0; i < oneTileType.Count; i++)
            {
                for (int j = 0; j < oneTileType.Count; j++)
                {
                    var currentDistance = (float)EuclideanDistance(oneTileType[i], oneTileType[j]);
                    averageDistance += currentDistance;
                }

            }
            averageDistance = averageDistance / (oneTileType.Count * oneTileType.Count);
            yield return averageDistance;
        }


        // implementation for floating-point EuclideanDistance
        // code from: 
        //https://codereview.stackexchange.com/questions/120933/calculating-distance-with-euclidean-manhattan-and-chebyshev-in-c

        public static double EuclideanDistance(Vector3d a, Vector3d b)
        {
            double square = (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
            return square;
        }

        List<Color> GetUncollapsedColors(TileSuperposition[,] uncollapsed, Color a, Color b, Color c)
        {
            List<Color> colors = new List<Color>();

            for (int i = 0; i < uncollapsed.GetLength(0); i++)
            {
                for (int j = 0; j < uncollapsed.GetLength(1); j++)
                {
                    if (uncollapsed[i, j] == null) continue;
                    else
                    {
                        Color color = GetColour(a, b, c, uncollapsed[i, j]);
                        colors.Add(color);
                    }
                }
            }

            return colors;
        }

        List<Vector3d> GetUncollapsedVectors (TileSuperposition[,] uncollapsed)
        {
            List<Vector3d> uncollapsedVectors = new List<Vector3d>();

            for (int i = 0; i < uncollapsed.GetLength(0); i++)
            {
                for (int j = 0; j < uncollapsed.GetLength(1); j++)
                {
                    if (uncollapsed[i, j] == null) continue;
                    else
                    {
                        Vector3d vector = new Vector3d(i, j, 0);
                        uncollapsedVectors.Add(vector);
                    }
                }
            }

            return uncollapsedVectors;
        }

        List<Point3d> GetUncollapsedPoints(TileSuperposition[,] uncollapsed)
        {
            List<Point3d> uncollapsedVectors = new List<Point3d>();

            for (int i = 0; i < uncollapsed.GetLength(0); i++)
            {
                for (int j = 0; j < uncollapsed.GetLength(1); j++)
                {
                    Point3d vector = new Point3d((i*2)-2, (j*2)-2, 0);
                    uncollapsedVectors.Add(vector);
                }
            }

            return uncollapsedVectors;
        }

        Color GetColour(Color a, Color b, Color c, TileSuperposition tile)
        {
            Color res = new Color();
            float firstTilePercentage = tile.Superpositions[0] / (float)tile.Sum;
            float secondtTilePercentage = tile.Superpositions[1] / (float)tile.Sum;
            float thirdTilePercentage = tile.Superpositions[2] / (float)tile.Sum;

            int redValueSum = (int)((firstTilePercentage * a.R) + (secondtTilePercentage * b.R) + (thirdTilePercentage * c.R));
            int greenValueSum = (int)((firstTilePercentage * a.G) + (secondtTilePercentage * b.G) + (thirdTilePercentage * c.G));
            int blueValueSum = (int)((firstTilePercentage * a.B) + (secondtTilePercentage * b.B) + (thirdTilePercentage * c.B));

            if (redValueSum > 255) redValueSum = 255;
            if (greenValueSum > 255) greenValueSum = 255;
            if (blueValueSum > 255) blueValueSum = 255;

            if (redValueSum < 0) redValueSum = 0;
            if (greenValueSum < 0) greenValueSum = 0;
            if (blueValueSum < 0) blueValueSum = 0;

            res = Color.FromArgb(redValueSum, greenValueSum, blueValueSum);

            return res;
        }

        public List<Brep> DuplicateTiles(Brep tileType, List<Vector3d> vectors)
        {
            List<Brep> list = new List<Brep>();
            for (int i = 0; i < vectors.Count; i++)
            {
                var k = DuplicateTileAndMoveByVector(tileType, vectors[i]);
                list.Add(k);
            }

            return list;
        }

        List<Vector3d> pointsToVectors (List<Point3d> pointsToConvert)
        {
            List<Vector3d> vectors = new List<Vector3d>();

            for (int i = 0; i < pointsToConvert.Count; i++)
            {
               var vector = ToVector(pointsToConvert[i]);
                vectors.Add(vector);
            }

            return vectors;
        }

        Vector3d ToVector(Point3d point)
        {
            Vector3d a = new Vector3d();
            a.X = point.X;
            a.Y = point.Y;
            a.Z = point.Z;

            return a;
        }

        public Brep DuplicateTileAndMoveByVector(Brep tileToMove, Vector3d vector)
        {
            var dup = tileToMove.DuplicateBrep();
            dup.Translate(vector);
            return dup;
        }

        List<Point3d> MovePointsOnCanvas(int xRelocation, int yRelocation, List<Point3d> elementToRelocate)
        {
            List<Point3d> relocatedPoints = new List<Point3d>();

            for (int i = 0; i < elementToRelocate.Count; i++)
            {
                Point3d relocatedPoint = new Point3d();
                relocatedPoint.X = elementToRelocate[i].X + xRelocation;
                relocatedPoint.Y = elementToRelocate[i].Y + yRelocation;

                relocatedPoints.Add(relocatedPoint);
            }

            return relocatedPoints;
        }

        List<Vector3d> MoveVectorOnCanvas(int xRelocation, int yRelocation, List<Vector3d> elementToRelocate)
        {
            List<Vector3d> relocatedPoints = new List<Vector3d>();

            for (int i = 0; i < elementToRelocate.Count; i++)
            {
                Vector3d relocatedPoint = new Vector3d();
                relocatedPoint.X = elementToRelocate[i].X + xRelocation;
                relocatedPoint.Y = elementToRelocate[i].Y + yRelocation;

                relocatedPoints.Add(relocatedPoint);
            }

            return relocatedPoints;
        }

        WaveCollapseHistoryElement GetWaveAtSpecificTime (GH_WaveCollapseHistory waveCollapseHistory, double frameFromTimeLapse)
        {
            if (waveCollapseHistory.Value.Elements.Count == 0) return null;

            int frameToVisualise = GetIndexToVisualise(waveCollapseHistory, frameFromTimeLapse);

            if (frameToVisualise == waveCollapseHistory.Value.Elements.Count && frameToVisualise != 0) frameToVisualise = frameToVisualise - 1;

            var waveElements = waveCollapseHistory.Value.Elements[frameToVisualise];
            return waveElements;
        }

        int GetIndexToVisualise(GH_WaveCollapseHistory timelapse, double frameLocation)
        {
            int index = 0;
            var numberOfFrames = timelapse.Value.Elements.Count;

            index = (int)Math.Floor(frameLocation * numberOfFrames);
            return index;
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
            get { return new Guid("6E3F0727-07AF-490F-8AD0-B4F3D7EEE7D4"); }
        }
    }
}