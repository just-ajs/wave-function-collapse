using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace WaveFunctionCollapse
{
    public class Pattern
    {
        private int patternSize;
        public Superposition[,] overlapsSuperpositions;
        float[] overalWeights;
        
        // Create a new pattern from provided states, weights and size of pattern
        public Pattern(State[,] miniTile, float[] overalWeights, int N)
        {
            MiniTile = miniTile;
            patternSize = N;
            this.overalWeights = overalWeights;
        }

        public State[,] MiniTile { get; set; }

        // Create a list of a superpositions for each possible overlapping neighbour for each pattern.
        // Each superposition has true value for possible pattern and false for others
        public void InitializeListOfOverlappingNeighbours(List<Pattern> patternsFromSample)
        {
            int offsetsToConsider = (int)Math.Pow((2 * (patternSize - 1) + 1), 2);
            var overlapDimension = (int)Math.Sqrt(offsetsToConsider);
            overlapsSuperpositions = new Superposition[overlapDimension, overlapDimension];

            var oneSideNeighboursInOneDimension = ((overlapDimension - 1) / 2);

            for (int x = -oneSideNeighboursInOneDimension; x <= oneSideNeighboursInOneDimension; x++)
            {
                for (int y = -oneSideNeighboursInOneDimension; y <= oneSideNeighboursInOneDimension; y++)
                {
                    overlapsSuperpositions[x + oneSideNeighboursInOneDimension, y + oneSideNeighboursInOneDimension] = GetLocalSuperposition(x, y, patternsFromSample);
                }
            }
        }

        // Create a list of a superpositions for each possible simple tiled neighbour.
        // Each superposition has true value for possible pattern and false for others
        public void InitializeListOfSimpleNeighbours(List<Pattern> patternsFromSample)
        {
            int offsetsToConsider = (int)Math.Pow((2 * (patternSize - 1) + 1), 2);
            var overlapDimension = (int)Math.Sqrt(offsetsToConsider);
            overlapsSuperpositions = new Superposition[overlapDimension, overlapDimension];

            var oneSideNeighboursInOneDimension = ((overlapDimension - 1) / 2);

            for (int x = -oneSideNeighboursInOneDimension; x <= oneSideNeighboursInOneDimension; x++)
            {
                for (int y = -oneSideNeighboursInOneDimension; y <= oneSideNeighboursInOneDimension; y++)
                {
                    overlapsSuperpositions[x + oneSideNeighboursInOneDimension, y + oneSideNeighboursInOneDimension] = GetLocalSuperposition(x, y, patternsFromSample);
                }
            }
        }

        // Create Superposition in specified  overlapping location
        Superposition GetLocalSuperposition(int x, int y, List<Pattern> patternsFromSample)
        {
            Superposition superpositionForXY = new Superposition(patternsFromSample);

            for (var i = 0; i < patternsFromSample.Count; i++)
            {
                var candidate = patternsFromSample[i];
                if (!this.Overlaps(x, y, candidate))
                {
                    superpositionForXY.coefficients[i] = false;
                }
            }
            return superpositionForXY;
        }


        // Compare the values of two patterns in overlapping space. If possible new pattern will overlap in right bottom corner of
        // existing pattern, it will compare right bottom of existing with left top of new pattern. 
        private bool Overlaps(int x, int y, Pattern other)
        {
            for (int i = x; i < x + 2; i++)
            {
                for (int j = y; j < y + 2; j++)
                {
                    if (i < 0 || j < 0) continue;
                    if (i > this.MiniTile.GetLength(0) - 1 || j > this.MiniTile.GetLength(1) - 1) continue;
                    var patternLocalValue = this.MiniTile[i, j];

                    int xFromCheckTile = 0, yFromCheckTile = 0;

                    // set x and y of pattern that can be possibly a neighbout
                    if (x < 0) xFromCheckTile = i + 1;
                    else if (x == 0) xFromCheckTile = i;
                    else if (x > 0) xFromCheckTile = i - 1;

                    if (y < 0) yFromCheckTile = j + 1;
                    else if (y == 0) yFromCheckTile = j;
                    else if (y > 0) yFromCheckTile = j - 1;

                    // continue if values are outside of array
                    if (xFromCheckTile < 0 || yFromCheckTile < 0) continue;
                    if (xFromCheckTile > this.MiniTile.GetLength(0) - 1 || yFromCheckTile > this.MiniTile.GetLength(1) - 1) continue;

                    // if the values are not the same, return false
                    var patternOtherValue = other.MiniTile[xFromCheckTile, yFromCheckTile];
                    if (patternOtherValue != patternLocalValue)
                    {
                        return false;
                    }
                }
            }

            return true;
        }


        // Calculate weights of the pattern based on calculated weights of each tile type
        public float Weight
        {
            get
            {
                float weight = 0;
                for (int i = 0; i < MiniTile.GetLength(0); i++)
                {
                    for (int j = 0; j < MiniTile.GetLength(1); j++)
                    {
                        if (MiniTile[i, j] == State.HALF_TILE) weight += overalWeights[0];
                        else if (MiniTile[i, j] == State.FULL_TILE) weight += overalWeights[1];
                        else if (MiniTile[i, j] == State.EMPTY) weight += overalWeights[2];
                    }
                }
                return weight;
            }
        }

        // Rotate 2x2 array by 90 degrees
        // Code from: [stackoverflow.com/questions/42519/how-do-you-rotate-a-two-dimensional-array];
        public State[,] RotateMatrix()
        {
            var ret = new State[MiniTile.GetLength(0), MiniTile.GetLength(1)];

            for (int i = 0; i < MiniTile.GetLength(0); ++i)
            {
                for (int j = 0; j < MiniTile.GetLength(1); ++j)
                {
                    ret[i, j] = MiniTile[MiniTile.GetLength(0) - j - 1, i];
                }
            }
            return ret;
        }

        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Pattern other = (Pattern)obj;
                for (var i = 0; i < MiniTile.GetLength(0); i++)
                {
                    for (var j = 0; j < MiniTile.GetLength(1); j++)
                    {
                        if (this.MiniTile[i, j] != other.MiniTile[i, j])
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        // Add a new point for given location
        internal Dictionary<State, List<Point3d>> Instantiate(int x, int y, int z)
        {
            var result = new Dictionary<State, List<Point3d>>()
            {
                { State.HALF_TILE, new List<Point3d>() },
                { State.FULL_TILE, new List<Point3d>() },
                { State.EMPTY, new List<Point3d>() }
            };

            for (var i = 0; i < MiniTile.GetLength(0); i++)
            {
                for (var j = 0; j < MiniTile.GetLength(1); j++)
                {
                    var tileUnit = MiniTile[i, j];
                    result[tileUnit].Add(new Point3d(x + i * 2, y + j * 2, z));
                }
            }

            return result;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < MiniTile.GetLength(0); ++i)
            {
                for (int j = 0; j < MiniTile.GetLength(1); ++j)
                {
                    var el = MiniTile[i, j];
                    if (el == State.EMPTY)
                    {
                        sb.Append("E ");
                    }
                    else if (el == State.HALF_TILE)
                    {
                        sb.Append("H ");
                    }
                    else if (el == State.FULL_TILE)
                    {
                        sb.Append("F ");
                    }

                }
            }

            return sb.ToString();
        }
    }
}