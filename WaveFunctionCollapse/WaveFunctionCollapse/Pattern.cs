using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace WaveFunctionCollapse
{
    internal class Pattern
    {
        private int patternSize;
        float weight;


        public Pattern(State[,] miniTile, float[] overalWeights)
        {
            MiniTile = miniTile;
        }

        public State[,] MiniTile { get; set; }


        float WeightPattern ()
        {
            float weight = 0;

            float denominator = MiniTile.GetLength(0) + MiniTile.GetLength(1);

            for (int i = 0; i < MiniTile.GetLength(0); i++)
            {
                for (int j = 0; j < MiniTile.GetLength(1); j++)
                {
                    //if (MiniTile[i,j] == State.EMPTY) weight += 1/denominator*
                }
            }
            return weight;
        }

        public State[] Flatten ()
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
                    result[tileUnit].Add(new Point3d(x + i*2, y + j*2, z));
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

        public override int GetHashCode()
        {
            var hashCode = -628551497;
            hashCode = hashCode * -1521134295 + patternSize.GetHashCode();
            hashCode = hashCode * -1521134295 + weight.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<State[,]>.Default.GetHashCode(MiniTile);
            return hashCode;
        }
    }
}