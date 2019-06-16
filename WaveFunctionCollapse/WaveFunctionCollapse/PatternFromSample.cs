using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveFunctionCollapse
{
    public class PatternFromSampleExtractor
    {
        IEnumerable<Point3d> unitElementsOfType0;
        IEnumerable<Point3d> unitElementsOfType1;
        IEnumerable<Point3d> unitElementsCenters;

        float[] tilesWeights = new float[3];
        int N = 0;

        bool rotation = false;

        public PatternFromSampleExtractor (IEnumerable<Point3d> unitElementsOfTypeA, IEnumerable<Point3d> unitElementsOfTypeB, IEnumerable<Point3d> areaCentres, float[] weight, int N, bool rotation)
        {
            unitElementsOfType0 = unitElementsOfTypeA;
            unitElementsOfType1 = unitElementsOfTypeB;
            unitElementsCenters = areaCentres;
            tilesWeights = weight;
            this.N = N;
            this.rotation = rotation;
        }

        // Get patterns from sample and calculate their superpositions of overlapping neighbours
        public List<Pattern> Extract ()
        {
            var patterns = PatternsFromSampleOverlapping(unitElementsOfType0, unitElementsOfType1, unitElementsCenters, tilesWeights, rotation);
            BuildPropagator(N, patterns);

            return patterns;
        }

        // From provided points create patterns that are result of pairing 4 points that are next to each other
        public List<Pattern> PatternsFromSampleOverlapping(IEnumerable<Point3d> unitElementsOfTypeA, 
            IEnumerable<Point3d> unitElementsOfTypeB, IEnumerable<Point3d> areaCentres, float[] weight, bool rotation)
        {
            // Get states based on unit tile type
            State[,] tileStates = GetTileStates(ConvertToInt(unitElementsOfTypeA), ConvertToInt(unitElementsOfTypeB), ConvertToInt(areaCentres));

            // Size of side of input sample (if 5x5, then there is 25 area centers)
            int tileSize = (int)Math.Sqrt(areaCentres.Count());

            // Number of small patterns from sample pattern
            int numberOfSubTiles = (int)Math.Pow((tileSize + 1) - N, 2);
            // const int patternSize = 2;

            var subTileStates = new List<Pattern>();
            var counter = 0;

            // Assign states values to pattern
            for (int i = 0; i < numberOfSubTiles; i++)
            {
                var miniTile = new State[N, N];
                for (int j = 0; j < N; j++)
                {
                    for (int k = 0; k < N; k++)
                    {
                        miniTile[j, k] = tileStates[counter % (tileSize + 1 - N) + j, counter / (tileSize + 1 - N) + k];
                    }
                }
                counter++;

                // Create a new pattern
                var pattern = new Pattern(miniTile, weight, (int)N);

                subTileStates.Add(pattern);
            }

            var patterns = subTileStates;

            // From tiles above, rotate each tile to get more variety - if asked
            // Remove duplicates no matter if the was rotation or not
            if (rotation)
            {
                List<Pattern> rotatedPatterns = new List<Pattern>();
                rotatedPatterns = GenerateRotatedTiles(patterns, weight);

                return RemoveDuplicates(rotatedPatterns);
            }
            else
            {
                return RemoveDuplicates(patterns);

            }

        }

        // This function creates for each pattern a list of possible overlapping neighbours
        public void BuildPropagator(int N, List<Pattern> patternsFromSample)
        {
            foreach (Pattern pattern in patternsFromSample)
            {
                pattern.InitializeListOfOverlappingNeighbours(patternsFromSample);
            }
        }

        // if tiles are the same - remove them
        List<Pattern> RemoveDuplicates(List<Pattern> rawPatternsWithRotations)
        {
            var duplicatesRemoved = new List<Pattern>(rawPatternsWithRotations);

            for (int i = 0; i < duplicatesRemoved.Count - 1; i++)
            {
                for (int j = i + 1; j < duplicatesRemoved.Count; j++)
                {
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

        // Set states based on unit types
        private State[,] GetTileStates(IEnumerable<IntPoint3d> unitElementsOfTypeA, IEnumerable<IntPoint3d> unitElementsOfTypeB, IEnumerable<IntPoint3d> areaCentres)
        {
            // A + B
            var unitElements = unitElementsOfTypeA.Concat(unitElementsOfTypeB);
            var elements = new HashSet<IntPoint3d>(unitElements);

            //null space
            var nulls = new HashSet<IntPoint3d>(areaCentres);
            nulls.ExceptWith(elements);

            // all points
            var allPoints = elements.Concat(nulls);
            var sortedPoints = allPoints.OrderBy(p => p.X).ThenBy(p => p.Y);

            int tileSize = (int)Math.Sqrt(allPoints.Count());

            // MAP ARRAY INTO TILESIZE x TILESIZE 2D ARRAY
            var tileUnits = Reshape(sortedPoints, tileSize, tileSize);

            // BASED ON 4X4 ARRAY CREATE SAME ARRAY BUT WITH STATES (EMPTY/HELF/FULL) INSTEAD OF POINTS
            State[,] tileStates = SetStatesBaseOnTileShape(unitElementsOfTypeA, unitElementsOfTypeB, tileUnits, tileSize);

            return tileStates;
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

        private IEnumerable<IntPoint3d> ConvertToInt(IEnumerable<Point3d> allPoints)
        {
            var result = new List<IntPoint3d>(allPoints.Count());

            foreach (var el in allPoints)
            {
                var elementToAdd = new IntPoint3d
                {
                    X = (int)Math.Round(el.X, 0),
                    Y = (int)Math.Round(el.Y,0),
                    Z = (int)Math.Round(el.Z)
                };

                result.Add(elementToAdd);
            }

            return result;
        }

        // Convert list into 2d array 
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
    }

    public class PatternFromSampleElement
    {
        public List<Pattern> Patterns { get; set; }
        public IEnumerable<Point3d> UnitElementsOfType0 { get; set; }
        public IEnumerable<Point3d> UnitElementsOfType1 { get; set; }
        public IEnumerable<Point3d> UnitElementsCenters { get; set; }
        public float[] TilesWeights { get; set; }
        public int N { get; set; }
    }

}
