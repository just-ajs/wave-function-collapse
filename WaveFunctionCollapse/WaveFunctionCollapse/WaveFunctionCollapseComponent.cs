using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Grasshopper.Kernel;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace WaveFunctionCollapse
{
    public partial class WaveFunctionCollapseComponent : GH_Component
    {
        public WaveFunctionCollapseComponent() : base("WaveFunctionCollapse", "WFC",
              "Me trying to code something", "TERM2", "WFC_WIP")
        { }

        // INPUT
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Tile Type A", "Type A", "List of all centers of tiles of type A", GH_ParamAccess.list);
            pManager.AddPointParameter("Tile Type B", "Type B", "List of all centers of tiles of type B", GH_ParamAccess.list);
            pManager.AddPointParameter("Whole Tile Points", "Tile Points", "List of all centers in tile design space", GH_ParamAccess.list);
            pManager.AddPointParameter("Wave", "", "", GH_ParamAccess.list);

        }

        // OUTPUT
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Nulls in tileset", "All Points", "Nulls", GH_ParamAccess.list);
            //pManager.AddTextParameter("States", "", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Number of rotated tiles", "Offsetes count", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Panel texts", "", "", GH_ParamAccess.list);
            pManager.AddPointParameter("Half tiles", "", "", GH_ParamAccess.list);
            pManager.AddPointParameter("Full tiles", "", "", GH_ParamAccess.list);
            pManager.AddPointParameter("Empty tiles", "", "", GH_ParamAccess.list);
            pManager.AddPointParameter("Seed Pattern", "", "", GH_ParamAccess.tree);
        }

        int N = 2;
        // INSIDE
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

            // WAVE TO OBSERVE: AREA FOR PATTERN
            List<Point3d> wavePoints = new List<Point3d>();
            DA.GetDataList<Point3d>(3, wavePoints);


            var halfTiles = new List<Point3d>();
            var fullTiles = new List<Point3d>();
            var emptyTiles = new List<Point3d>();

            // weights
            float halfTileWeight = (float)tilesA.Count / allTiles.Count;
            float fullTileWeight = (float)tilesB.Count / allTiles.Count;
            float emptyTileWeight = 1 - (halfTileWeight + fullTileWeight);

            float[] weights = new float[3];
            weights[0] = halfTileWeight;
            weights[1] = fullTileWeight;
            weights[2] = emptyTileWeight;

            // RUN WAVEFUNCION COLLAPSE
            var patterns = Run(tilesA, tilesB, allTiles, N, wavePoints, weights, DA);

            int x = -10, y = -10, z = -10;
            for (var i = 0; i < patterns.Count; i++)
            {
                var pattern = patterns[i];
                var instance = pattern.Instantiate(x + i * 8, y, z);

                halfTiles.AddRange(instance[State.HALF_TILE]);
                fullTiles.AddRange(instance[State.FULL_TILE]);
                emptyTiles.AddRange(instance[State.EMPTY]);
            }


            if (true)
            {
                DA.SetDataList(0, patterns);
                //DA.SetData(1, statesString);
                DA.SetData(1, patterns.Count);
                DA.SetDataList(2, patterns.Select(p => p.ToString()));
                DA.SetDataList(3, halfTiles);
                DA.SetDataList(4, fullTiles);
                DA.SetDataList(5, emptyTiles);
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Check the inputs you idiot!");
            }
        }

        int GetNumberofPointsInOneDimension(double firstPointCoordinate, double secondPointCoordinate)
        {
            return Math.Abs((int)(0.5 * (firstPointCoordinate - secondPointCoordinate) - 1));
        }

        string BuiltOutputString(int subtilesCount, int subtilesSize, State[,] statestoConvert)
        {
            var states = new StringBuilder();

            for (int i = 0; i < subtilesCount; i++)
            {
                for (int j = 0; j < subtilesSize; j++)
                {
                    for (int k = 0; k < subtilesSize; k++)
                    {
                        states.Append(statestoConvert[j, k]);
                        states.Append("   ");
                    }
                    states.Append("\n");
                }
                states.Append("\n");
            }

            return states.ToString();
        }

        State[,] SetStatesBaseOnTileShape(IEnumerable<IntPoint3d> A, IEnumerable<IntPoint3d> B, IntPoint3d[,] tiles, int arraySize)
        {
            State[,] tileStates = new State[arraySize, arraySize];

            for (int i = 0; i < arraySize; i++)
            {
                for (int j = 0; j < arraySize; j++)
                {
                    State current;
                    if (A.Contains(tiles[i, j])) current = State.HALF_TILE;
                    else if (B.Contains(tiles[i, j])) current = State.FULL_TILE;
                    else current = State.EMPTY;

                    tileStates[i, j] = current;
                }
            }
            return tileStates;
        }

        List<Pattern> Run(IEnumerable<Point3d> unitElementsOfTypeA, IEnumerable<Point3d> unitElementsOfTypeB, IEnumerable<Point3d> areaCentres, 
            int N, List<Point3d> wavePoints, float [] weights, IGH_DataAccess DA)
        {
            //bool finished = false;

            var patterns = PatternsFromSample(unitElementsOfTypeA, unitElementsOfTypeB, areaCentres, weights);
            BuildPropagator(N, patterns);


            int width = GetNumberofPointsInOneDimension(wavePoints[0].X, wavePoints[wavePoints.Count - 1].X);
            int height = GetNumberofPointsInOneDimension(wavePoints[0].Y, wavePoints[wavePoints.Count - 1].Y);
            // initial settings
            Wave newWave = new Wave(width, height, patterns);
            SeedPattern(width / 2, height / 2, patterns, DA);
            return patterns;
             
            
            //while (!finished)
            //{
            //    Observe();
            //    Propagate();
            //}

            //OutputObservations();
        }

        void SeedPattern(int x, int y, List<Pattern> patternsFromSample, IGH_DataAccess DA )
        {
            Pattern patternToSeed = highestWeightPattern(patternsFromSample);
            var outputSeed = patternToSeed.Instantiate(x, y, 0);


            var test = new Grasshopper.DataTree<List<Point3d>>();
    
            var halfTilePoints = outputSeed[State.HALF_TILE];
            var fullTilePoints = outputSeed[State.FULL_TILE];
            var emptyTilePoints = outputSeed[State.EMPTY];

            test.Add(halfTilePoints);
            test.Add(fullTilePoints);
            test.Add(emptyTilePoints);

            DA.SetDataTree(6, test);
        }

        Pattern highestWeightPattern(List<Pattern> patterns)
        {
            List<Pattern> patternsWithHeighestWeight = new List<Pattern>();
            float highestWeight = 0;

            for (int i = 0; i < patterns.Count; i++)
            {
                if (patterns[i].Weight > highestWeight) highestWeight = patterns[i].Weight;
            }

            for (int i = 0; i < patterns.Count; i++)
            {
                if (patterns[i].Weight == highestWeight) patternsWithHeighestWeight.Add(patterns[i]);
            }

            if (patternsWithHeighestWeight.Count > 1)
            {
                Random random = new Random();
                int randomNumber = random.Next(patternsWithHeighestWeight.Count - 1);

                return patternsWithHeighestWeight[randomNumber];
            }
            else
            {
                return patternsWithHeighestWeight[0];
            }
        }



        List<Pattern> PatternsFromSample(IEnumerable<Point3d> unitElementsOfTypeA, IEnumerable<Point3d> unitElementsOfTypeB, IEnumerable<Point3d> areaCentres, float[] weight)
        {
            State[,] tileStates = GetTileStates(ConvertToInt(unitElementsOfTypeA), ConvertToInt(unitElementsOfTypeB), ConvertToInt(areaCentres));

            int tileSize = (int)Math.Sqrt(areaCentres.Count());
            int numberOfSubTiles = (int)Math.Pow(tileSize - 1, 2);
            const int patternSize = 2;

            var subTileStates = new List<Pattern>();
            var counter = 0;

            // GENERATE PATTERNS FROM SAMPLE
            for (int i = 0; i < numberOfSubTiles; i++)
            {
                var miniTile = new State[patternSize, patternSize];
                for (int j = 0; j < patternSize; j++)
                {
                    for (int k = 0; k < patternSize; k++)
                    {
                        miniTile[j, k] = tileStates[counter % (tileSize - 1) + j, counter / (tileSize - 1) + k];
                    }
                }
                counter++;

                var pattern = new Pattern(miniTile, weight, (int)N);

                subTileStates.Add(pattern);
            }

            var patterns = subTileStates;

            // FROM TILES CREATED ABOVE, ROTATE EACH TILE, CREATE NEW TILE FROM ROTATED, THEN CHECK IF THERE ARE ANY DUPLICATES: REMOVE THEM
            List<Pattern> rotatedPatterns = new List<Pattern>();
            rotatedPatterns = GenerateRotatedTiles(patterns, weight);
            var rotatedPatternsWithoutDuplicates = RemoveDuplicates(rotatedPatterns);

            return rotatedPatternsWithoutDuplicates;
        }

        void BuildPropagator(int N, List<Pattern> patternsFromSample)
        {
            foreach (Pattern pattern in patternsFromSample)
            {
                pattern.InitializeListOfOverlappingNeighbours(patternsFromSample);
            }
        }

        void Observe()
        {
            //FindLowestEntropy();
        }
        void Propagate() { }
        void OutputObservations() { }

        void FindLowestEntropy(Wave wave)
        {
            
        }

        List<Pattern> GenerateRotatedTiles(List<Pattern> rawPatternsFromSample, float[] tilesWeights)
        {
            List<Pattern> withRotation = new List<Pattern>(rawPatternsFromSample);

            for (int i = 0; i < rawPatternsFromSample.Count; i++)
            {
                var firstRotation = new Pattern(rawPatternsFromSample[i].RotateMatrix(), tilesWeights, N);
                withRotation.Add(firstRotation);
                var secondRotation = new Pattern(firstRotation.RotateMatrix(), tilesWeights, N);
                withRotation.Add(secondRotation);
                var thirdRotation = new Pattern(secondRotation.RotateMatrix(), tilesWeights, N);
                withRotation.Add(thirdRotation);
            }

            return withRotation;
        }


        List<Pattern> RemoveDuplicates(List<Pattern> rawPatternsWithRotations)
        {
            var duplicatesRemoved = new List<Pattern>(rawPatternsWithRotations);

            for (int i = 0; i < duplicatesRemoved.Count - 1; i++)
            {
                for (int j = i + 1; j < duplicatesRemoved.Count; j++)
                {
                    // Use list[i] and list[j]
                    var areEqual = duplicatesRemoved[i].Equals(duplicatesRemoved[j]);

                    if (areEqual)
                    {
                        duplicatesRemoved.RemoveAt(j);
                        j--;
                    }
                }
            }
            return duplicatesRemoved;
        }

        private State[,] GetTileStates(IEnumerable<IntPoint3d> unitElementsOfTypeA, IEnumerable<IntPoint3d> unitElementsOfTypeB, IEnumerable<IntPoint3d> areaCentres)
        {
            // A + B
            var unitElements = unitElementsOfTypeA.Concat(unitElementsOfTypeB);

            //null space
            var nulls = new HashSet<IntPoint3d>(areaCentres);
            nulls.ExceptWith(unitElements);

            // all points
            var allPoints = unitElements.Concat(nulls);
            var sortedPoints = allPoints.OrderBy(p => p.X).ThenBy(p => p.Y);

            int tileSize = (int)Math.Sqrt(allPoints.Count());

            // MAP ARRAY INTO TILESIZE x TILESIZE 2D ARRAY
            var tileUnits = Reshape(sortedPoints, tileSize, tileSize);

            // BASED ON 4X4 ARRAY CREATE SAME ARRAY BUT WITH STATES (EMPTY/HELF/FULL) INSTEAD OF POINTS
            State[,] tileStates = SetStatesBaseOnTileShape(unitElementsOfTypeA, unitElementsOfTypeB, tileUnits, tileSize);

            return tileStates;
        }

        private IEnumerable<IntPoint3d> ConvertToInt(IEnumerable<Point3d> allPoints)
        {
            var result = new List<IntPoint3d>(allPoints.Count());

            foreach (var el in allPoints)
            {
                var elementToAdd = new IntPoint3d
                {
                    X = (int)el.X,
                    Y = (int)el.Y,
                    Z = (int)el.Z
                };

                result.Add(elementToAdd);
            }

            return result;
        }

        private T[,] Reshape<T>(IEnumerable<T> container, int rows, int columns)
        {
            T[,] result = new T[rows, columns];

            int counter = 0;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    result[i, j] = container.ElementAt(counter);
                    counter++;
                }
            }

            return result;
        }

        // below: create list of neighbours + find tiles that match: moved to Pattern Class
        /* 
        List<Pattern>[,] CreateListofNeighbours(Pattern patternFromSample, List<Pattern> possibleNeighbours, int N)
        {
            int offsetsToConsider = (int)Math.Pow((2 * (N - 1) + 1), 2);
            var overlapDimension = (int)Math.Sqrt(offsetsToConsider);
            var neighbours = new List<Pattern>[overlapDimension, overlapDimension];

            for (int x = 0; x < overlapDimension; x++)
            {
                for (int y = 0; y < overlapDimension; y++)
                {
                    var neighboursOfGivenOffset = FindTilesThatMatch(x - 1, y - 1, patternFromSample, possibleNeighbours);
                    neighbours[x, y] = neighboursOfGivenOffset;
                }
            }
            return neighbours;
        }


        // Figure 6, for loop fro x = -1 to x = 1, same for y
        List<Pattern> FindTilesThatMatch(int x, int y, Pattern patternToCheck, List<Pattern> possibleCandidates)
        {
            List<Pattern> rightMatchForThisLocation = new List<Pattern>(possibleCandidates);

            for (int i = x; i < x + 2; i++)
            {
                for (int j = y; j < y + 2; j++)
                {
                    if (i < 0 || j < 0) continue;
                    if (i > patternToCheck.MiniTile.GetLength(0) - 1 || j > patternToCheck.MiniTile.GetLength(1) - 1) continue;
                    var patternLocalValue = patternToCheck.MiniTile[i, j];

                    int xFromCheckTile = 0, yFromCheckTile = 0;

                    if (x < 0) xFromCheckTile = i + 1;
                    else if (x == 0) xFromCheckTile = i;
                    else if (x > 0) xFromCheckTile = i - 1;

                    if (y < 0) yFromCheckTile = j + 1;
                    else if (y == 0) yFromCheckTile = j;
                    else if (y > 0) yFromCheckTile = j - 1;

                    if (xFromCheckTile < 0 || yFromCheckTile < 0) continue;
                    if (xFromCheckTile > patternToCheck.MiniTile.GetLength(0) - 1 || yFromCheckTile > patternToCheck.MiniTile.GetLength(1) - 1) continue;

                    foreach (var candidate in possibleCandidates)
                    {
                        var patternOtherValue = candidate.MiniTile[xFromCheckTile, yFromCheckTile];
                        if (patternOtherValue != patternLocalValue) rightMatchForThisLocation.Remove(candidate);
                    }
                }
            }
        
            return rightMatchForThisLocation;
        }
        */

        float GetShannonEntropyForSquare (float weight)
        {
            float shannonEntropy = (float)Math.Log(weight) - (weight * (float)Math.Log(weight) / weight);

            return shannonEntropy;
        }

        /// Provides an Icon for every component that will be visible in the User Interface. Icons need to be 24x24 pixels.
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                // You can add image files to your project resources and access them like this:
                return WaveFunctionCollapse.Properties.Resources.Icon;
                //return null;
            }
        }

        /// Each component must have a unique Guid to identify it. It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        public override Guid ComponentGuid
        {
            get { return new Guid("3aac7ab0-722c-4eb0-b65a-e53640525e4b"); }
        }

        public object MyAssemblyName { get; private set; }
    }

    struct IntPoint3d
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public override string ToString()
        {
            return $"{X}, {Y}, {Z}";
        }
    }

}
