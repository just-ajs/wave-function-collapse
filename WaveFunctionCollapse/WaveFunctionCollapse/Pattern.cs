using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace WaveFunctionCollapse
{
    internal class Pattern
    {
        private int patternSize;
        public Superposition[,] overlapsSuperpositions;
        float[] overalWeights;

        public Pattern(State[,] miniTile, float[] overalWeights, int N)
        {
            MiniTile = miniTile;
            patternSize = N;
            this.overalWeights = overalWeights;
        }

        public State[,] MiniTile { get; set; }

        public void InitializeListOfOverlappingNeighbours(List<Pattern> patternsFromSample)
        {
            int offsetsToConsider = (int)Math.Pow((2 * (patternSize - 1) + 1), 2);
            var overlapDimension = (int)Math.Sqrt(offsetsToConsider);
            overlapsSuperpositions = new Superposition[overlapDimension, overlapDimension];

            for (int x = 0; x < overlapDimension; x++)
            {
                for (int y = 0; y < overlapDimension; y++)
                {
                    overlapsSuperpositions[x, y] = GetLocalSuperposition(x, y, patternsFromSample);
                }
            }
        }

        Superposition GetLocalSuperposition(int x, int y, List<Pattern> patternsFromSample)
        {
            //List<Pattern> rightMatchForThisLocation = new List<Pattern>(patternsFromSample);
            Superposition superpositionForXY = new Superposition(patternsFromSample);

            for (int i = x; i < x + 2; i++)
            {
                for (int j = y; j < y + 2; j++)
                {
                    if (i < 0 || j < 0) continue;
                    if (i > MiniTile.GetLength(0) - 1 || j > MiniTile.GetLength(1) - 1) continue;
                    var patternLocalValue = MiniTile[i, j];

                    int xFromCheckTile = 0, yFromCheckTile = 0;

                    if (x < 0) xFromCheckTile = i + 1;
                    else if (x == 0) xFromCheckTile = i;
                    else if (x > 0) xFromCheckTile = i - 1;

                    if (y < 0) yFromCheckTile = j + 1;
                    else if (y == 0) yFromCheckTile = j;
                    else if (y > 0) yFromCheckTile = j - 1;

                    if (xFromCheckTile < 0 || yFromCheckTile < 0) continue;
                    if (xFromCheckTile > MiniTile.GetLength(0) - 1 || yFromCheckTile > MiniTile.GetLength(1) - 1) continue;

                    foreach (var candidate in patternsFromSample)
                    {
                        var patternOtherValue = candidate.MiniTile[xFromCheckTile, yFromCheckTile];
                        if (patternOtherValue != patternLocalValue)
                        {
                            //rightMatchForThisLocation.Remove(candidate);
                            superpositionForXY.coefficients[i] = false;
                        }
                    }
                }
            }

            return superpositionForXY;
        }

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


        public State[] Flatten()
        {
            var flatStates = new List<State>();
            for (int i = 0; i < MiniTile.GetLength(0); i++)
            {
                for (int j = 0; j < MiniTile.GetLength(1); j++)
                {
                    flatStates.Add(MiniTile[i, j]);
                }
            }
            return flatStates.ToArray();
        }

        // Reference: [stackoverflow.com/questions/42519/how-do-you-rotate-a-two-dimensional-array];
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
        /*
                public override int GetHashCode()
                {
                    var hashCode = -628551497;
                    hashCode = hashCode * -1521134295 + patternSize.GetHashCode();
                    hashCode = hashCode * -1521134295 + weight.GetHashCode();
                    hashCode = hashCode * -1521134295 + EqualityComparer<State[,]>.Default.GetHashCode(MiniTile);
                    return hashCode;
                }

            */
    }
}