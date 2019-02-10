using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper.Kernel;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace WaveFunctionCollapse
{
    public class WaveFunctionCollapseComponent : GH_Component
    {
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, new tabs/panels will automatically be created.

        public WaveFunctionCollapseComponent()
          : base("WaveFunctionCollapse", "WFC bla",
              "Me trying to code something",
              "TERM2", "WFC_WIP")
        {
        }

        public enum State { EMPTY, FULL_TILE, HALF_TILE };


        // INPUT
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Tile Type A", "Type A", "List of all centers of tiles of type A", GH_ParamAccess.list);
            pManager.AddPointParameter("Tile Type B", "Type B", "List of all centers of tiles of type B", GH_ParamAccess.list);
            pManager.AddPointParameter("Whole Tile Points", "Tile Points", "List of all centers in tile design space", GH_ParamAccess.list);
        }

        // OUTPUT
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Nulls in tileset", "All Points", "Nulls", GH_ParamAccess.list);
            pManager.AddTextParameter("States", "", "", GH_ParamAccess.list);
                
        }

        // INSIDE
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // get tile A
            List<Point3d> tilesA = new List<Point3d>();
            DA.GetDataList<Point3d>(0, tilesA);

            // get tile B
            List<Point3d> tilesB = new List<Point3d>();
            DA.GetDataList<Point3d>(1, tilesB);

            // A + B
            var mergeAB = tilesA.Concat(tilesB);

            // design space centers
            List<Point3d> centers = new List<Point3d>();
            DA.GetDataList<Point3d>(2, centers);

            Run(tilesA, tilesB, centers);

            //null space
            var nulls = new HashSet<Point3d>(centers);
            nulls.ExceptWith(mergeAB);

            // all points
            var allPoints = mergeAB.Concat(nulls);
            var sortedPoints = allPoints.OrderBy(p => p.X).ThenBy(p => p.Y);

            int tileSize = (int)Math.Sqrt(allPoints.Count());

            // REMAP ONE DIMENSIONAL ARRAY INTO TWO DIMENSIONAL (4X4) ARRAY
            int counter = 0;
            Point3d[,] tileUnits = new Point3d[tileSize,tileSize];

            for (int i = 0; i < tileSize; i++)
            {
                for (int j = 0; j < tileSize; j++)
                {
                    tileUnits[i, j] = sortedPoints.ElementAt(counter);
                    counter++;
                }
            }

            // BASED ON 4X4 ARRAY CREATE SAME ARRAY BUT WITH STATES (EMPTY/HELF/FULL) INSTEAD OF POINTS
            State[,] tileStates = setStates(tilesA, tilesB, tileUnits, tileSize);


            // PATTERNS FROM SAMPLE
            int numberOfSubTiles = (int)Math.Pow(tileSize - 1, 2);
            int miniTileSize = tileSize/2;

            var subTileStates = new List<State[,]>(numberOfSubTiles);

            //State[][,] subTileStates = new State[numberOfSubTiles][miniTileSize, miniTileSize];

            counter = 0;
            //int counterK = 0;
            for (int i = 0; i < numberOfSubTiles; i++)
            {
                var miniTile = new State[miniTileSize, miniTileSize];
                counter = 0;
                for (int j = 0; j < miniTileSize; j++)
                {
                    for (int k = 0; k < miniTileSize; k++)
                    {
                        miniTile[j, k] = tileStates[j + counter, k + counter];

                    }

                }
                counter++;
                subTileStates.Add(miniTile);
            }

            //var strings = new List<String>();

            var states = new StringBuilder();

            for (int i = 0; i < numberOfSubTiles; i++)
            {
                for (int j = 0; j < miniTileSize; j++)
                {
                    for (int k = 0; k < miniTileSize; k++)
                    {
                        states.Append(tileStates[j, k]);
                        states.Append("   ");
                    }
                    states.Append("\n");
                }

                states.Append("\n");
            }

            var statesString = states.ToString();


            if (true)
            {
                DA.SetDataList(0, sortedPoints);
                DA.SetData(1, statesString);
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Check the inputs you idiot!");
            }
        }

        State[,] setStates (IEnumerable<Point3d> A, IEnumerable<Point3d> B, Point3d[,] tiles, int arraySize)
        {
            State[,] tileStates = new State[arraySize, arraySize];

            for (int i = 0; i < arraySize; i++)
            {
                for (int j = 0; j < arraySize; j++)
                {
                    State current;
                    if (A.Contains(tiles[i, j]))
                    {
                        current = State.HALF_TILE;
                    }
                    else if (B.Contains(tiles[i, j]))
                    {
                        current = State.FULL_TILE;
                    }
                    else
                    {
                        current = State.EMPTY;
                    }

                    tileStates[i, j] = current;
                }
            }
            return tileStates;
        }

        void Run(IEnumerable<Point3d> unitElementsOfTypeA, IEnumerable<Point3d> unitElementsOfTypeB, IEnumerable<Point3d> areaCentres)
        {
            bool finished = false;

            PatternsFromSample(unitElementsOfTypeA, unitElementsOfTypeB, areaCentres);
            BuildPropagator();

            while(!finished)
            {
                Observe();
                Propagate();
            }

            OutputObservations();
        }


        void PatternsFromSample(IEnumerable<Point3d> unitElementsOfTypeA, IEnumerable<Point3d> unitElementsOfTypeB, IEnumerable<Point3d> areaCentres)
        {
            // A + B
            var unitElements = unitElementsOfTypeA.Concat(unitElementsOfTypeB);

            //null space
            var nulls = new HashSet<Point3d>(areaCentres);
            nulls.ExceptWith(unitElements);

            // all points
            var allPoints = unitElements.Concat(nulls);
            var sortedPoints = allPoints.OrderBy(p => p.X).ThenBy(p => p.Y);

            int tileSize = (int)Math.Sqrt(allPoints.Count());

            // REMAP ONE DIMENSIONAL ARRAY INTO TWO DIMENSIONAL (4X4) ARRAY
            int counter = 0;
            Point3d[,] tileUnits = new Point3d[tileSize, tileSize];

            for (int i = 0; i < tileSize; i++)
            {
                for (int j = 0; j < tileSize; j++)
                {
                    tileUnits[i, j] = sortedPoints.ElementAt(counter);
                    counter++;
                }
            }

            // BASED ON 4X4 ARRAY CREATE SAME ARRAY BUT WITH STATES (EMPTY/HELF/FULL) INSTEAD OF POINTS
            State[,] tileStates = setStates(unitElementsOfTypeA, unitElementsOfTypeB, tileUnits, tileSize);


            // PATTERNS FROM SAMPLE
            int numberOfSubTiles = (int)Math.Pow(tileSize - 1, 2);
            int miniTileSize = tileSize / 2;

            var subTileStates = new List<State[,]>(numberOfSubTiles);

            //State[][,] subTileStates = new State[numberOfSubTiles][miniTileSize, miniTileSize];

            counter = 0;
            //int counterK = 0;
            for (int i = 0; i < numberOfSubTiles; i++)
            {
                var miniTile = new State[miniTileSize, miniTileSize];
                counter = 0;
                for (int j = 0; j < miniTileSize; j++)
                {
                    for (int k = 0; k < miniTileSize; k++)
                    {
                        miniTile[j, k] = tileStates[j + counter, k + counter];

                    }

                }
                counter++;
                subTileStates.Add(miniTile);
            }

            //var strings = new List<String>();

            var states = new StringBuilder();

            for (int i = 0; i < numberOfSubTiles; i++)
            {
                for (int j = 0; j < miniTileSize; j++)
                {
                    for (int k = 0; k < miniTileSize; k++)
                    {
                        states.Append(tileStates[j, k]);
                        states.Append("   ");
                    }
                    states.Append("\n");
                }

                states.Append("\n");
            }

            var statesString = states.ToString();
        }

        void BuildPropagator(){}
        void Observe(){}
        void Propagate(){}
        void OutputObservations(){}

        /// Provides an Icon for every component that will be visible in the User Interface. Icons need to be 24x24 pixels.
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// Each component must have a unique Guid to identify it. It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        public override Guid ComponentGuid
        {
            get { return new Guid("3aac7ab0-722c-4eb0-b65a-e53640525e4b"); }
        }

        //public string ToString(this State[,] xD) {

        //    return xD[0, 0].ToString();
        //}
    }
}
