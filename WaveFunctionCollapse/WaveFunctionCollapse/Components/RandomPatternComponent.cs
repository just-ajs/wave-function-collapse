using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WaveFunctionCollapse
{
    public class RandomPattern : GH_Component
    {
        private static Random random = new Random();

        public RandomPattern()
          : base("RandomPattern", "Nickname", "Description", "WFC", "Wave Fucntion Collapse")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Wave", "", "", GH_ParamAccess.list);
            pManager.AddSurfaceParameter("TILE 01", "TILE 01", "First tile type here", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("TILE 02", "TILE 02", "First tile type here", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("EMPTY", "TILE 02", "First tile type here", GH_ParamAccess.item);
            pManager.AddNumberParameter("X WAVE LOCATION", "X", "X", GH_ParamAccess.item);
            pManager.AddNumberParameter("Y WAVE LOCATION", "Y", "Y", GH_ParamAccess.item);
            pManager.AddNumberParameter("Weights", "", "", GH_ParamAccess.list);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Wave: half surfaces", "", "", GH_ParamAccess.list);
            pManager.AddSurfaceParameter("Wave: full surfaces", "", "", GH_ParamAccess.list);
            pManager.AddSurfaceParameter("Wave: empty surfaces", "", "", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //// WAVE TO OBSERVE: AREA FOR PATTERN
            List<Point3d> wavePoints = new List<Point3d>();
            DA.GetDataList<Point3d>(0, wavePoints);

            Brep surfaceType01 = new Brep();
            DA.GetData<Brep>(1, ref surfaceType01);

            Brep surfaceType02 = new Brep();
            DA.GetData<Brep>(2, ref surfaceType02);

            Brep surfaceTypeEmpty = new Brep();
            DA.GetData<Brep>(3, ref surfaceTypeEmpty);

            double wavePositionX = new double();
            DA.GetData<double>(4, ref wavePositionX);

            double wavePositionY = new double();
            DA.GetData<double>(5, ref wavePositionY);

            List<double> pointsCount = new List<double>();
            DA.GetDataList<double>(6, pointsCount);

            List<Vector3d> half = new List<Vector3d>();
            List<Vector3d> full = new List<Vector3d>();
            List<Vector3d> empty = new List<Vector3d>();


            // get size of wave
            int width = GetNumberofPointsInOneDimension(wavePoints[0].X, wavePoints[wavePoints.Count - 1].X);
            int height = GetNumberofPointsInOneDimension(wavePoints[0].Y, wavePoints[wavePoints.Count - 1].Y);

            SetRandomVectorLists(width, height, half, full, empty, pointsCount);


            var halfMoved = MovePointsOnCanvas((int)wavePositionX, (int)wavePositionY, half);
            var fullMoved = MovePointsOnCanvas((int)wavePositionX, (int)wavePositionY, full);
            var emptyMoved = MovePointsOnCanvas((int)wavePositionX, (int)wavePositionY, empty);


            var duplicatedHalfSurfaces = DuplicateTiles(surfaceType01, halfMoved);
            var duplicatedFullSurfaces = DuplicateTiles(surfaceType01, fullMoved);
            var duplicatedEmptySurfaces = DuplicateTiles(surfaceType01, emptyMoved);


            DA.SetDataList(0, duplicatedHalfSurfaces);
            DA.SetDataList(1, duplicatedFullSurfaces);
            DA.SetDataList(2, duplicatedEmptySurfaces);

        }

        double[] GetWeights(List<double> a)
        {
            double[] weights = new double[3];

            double sum = a[0] + a[1] + a[2];
            weights[0] = a[0] / sum;
            weights[1] = a[1] / sum;
            weights[2] = a[2] / sum;

            return weights;
        }

         List<Brep> DuplicateTiles(Brep tileType, List<Vector3d> vectors)
        {
            List<Brep> list = new List<Brep>();
            for (int i = 0; i < vectors.Count; i++)
            {
                var k = DuplicateTileAndMoveByVector(tileType, vectors[i]);
                list.Add(k);
            }

            return list;
        }

        // Move by x and y
        List<Vector3d> MovePointsOnCanvas(int xRelocation, int yRelocation, List<Vector3d> elementToRelocate)
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

        Brep DuplicateTileAndMoveByVector(Brep tileToMove, Vector3d vector)
        {
            var dup = tileToMove.DuplicateBrep();
            dup.Translate(vector);
            return dup;
        }

        void SetRandomVectorLists(int width, int height, List<Vector3d> half, List<Vector3d> full, List<Vector3d> empty, List<double> pointCount)
        {
            double sum = pointCount[0] + pointCount[1] + pointCount[2];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int randomNumber = random.Next(0, (int)sum);

                    if (randomNumber < pointCount[0])
                    {
                        half.Add(new Vector3d(i*2, j*2, 0));
                    }
                    else if (randomNumber >= pointCount[0] && randomNumber < pointCount[0] + pointCount[1])
                    {
                        full.Add(new Vector3d(i * 2, j * 2, 0));

                    }
                    else if (randomNumber >= pointCount[0] + pointCount[1])
                    {
                        empty.Add(new Vector3d(i * 2, j * 2, 0));

                    }
                }
            }
        }

        State[,] GetRandomStates(int width, int height)
        {
            State[,] randomStates = new State[width, height];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int randomNumber = random.Next(0, 1);

                    if (randomNumber < 0.33) { randomStates[i, j] = State.EMPTY; }
                    else if (randomNumber >= 0.33 && randomNumber < 0.66) { randomStates[i, j] = State.HALF_TILE; }
                    else if (randomNumber >= 0.66) { randomStates[i, j] = State.HALF_TILE; }

                }
            }

            return randomStates;
        }


        public int GetNumberofPointsInOneDimension(double firstPointCoordinate, double secondPointCoordinate)
        {
            return Math.Abs((int)(0.5 * (firstPointCoordinate - secondPointCoordinate) - 1));
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("c0b0b52a-a5d0-4e18-b356-db227da33b34"); }
        }
    }
}