using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WaveFunctionCollapse
{
    internal class Wave
    {
        int width;
        int height;
        public Superposition[,] superpositions;
        public Pattern[,] waveCollapse;
        readonly List<Pattern> patterns;

        public List<Point3d> half;
        public List<Point3d> full;
        public List<Point3d> empty;

        public Wave(int width, int height, List<Pattern> patterns)
        {
            this.width = width;
            this.height = height;
            this.patterns = patterns;

            superpositions = new Superposition[width, height];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    superpositions[i, j] = new Superposition(patterns);
                }
            }

            waveCollapse = new Pattern[width, height];
        }

        public TileSuperposition[,] GetPossibleTileTypes()
        {
            TileSuperposition[,] uncollapsed = new TileSuperposition[width, height];

            for (int i = 0; i < waveCollapse.GetLength(0); i ++)
            {
                for (int j = 0; j < waveCollapse.GetLength(1); j ++)
                {
                    //if (waveCollapse[i, j] != null) continue;
                    //else
                    {
                        int halfStates = 0;
                        int fullStates = 0;
                        int emptyStates = 0;

                        Point3d center = new Point3d((double)i, (double)j, 0.0);

                        var list = new Point3d[2,2];

                        Point3d leftBottom = new Point3d((center.X - 1) * 2, (center.Y - 1) * 2, 0.0);
                        //Point3d leftTop = new Point3d((center.X - 1) * 2, (center.Y + 1) * 2, 0.0);
                        //Point3d rightBottom = new Point3d((center.X + 1) * 2, (center.Y - 1) * 2, 0.0);
                        //Point3d rightTop = new Point3d((center.X + 1) * 2, (center.Y + 1) * 2, 0.0);

                        //list[0,0] = leftBottom;
                        //list[0,1] = leftTop;
                        //list[1,0] = rightBottom;
                        //list[1,1] = rightTop;

                        var size = superpositions[i, j].coefficients.Length;
                        for (int k = 0; k < size; k++)
                        {

                            if (superpositions[i, j].coefficients[k] == true)
                            {

                                for (int m = 0; m < list.GetLength(0); m++)
                                {
                                    for (int n = 0; n < list.GetLength(1); n++)
                                    {
                                        if (patterns[k].MiniTile[m,n] == State.HALF_TILE)
                                        {
                                            halfStates++;
                                        }
                                        else if (patterns[k].MiniTile[0, 0] == State.FULL_TILE)
                                        {
                                            fullStates++;
                                        }
                                        else if (patterns[k].MiniTile[0, 0] == State.EMPTY)
                                        {
                                            emptyStates++;
                                        }
                                    }

                                }
                            }

                            TileSuperposition a = new TileSuperposition(halfStates, fullStates, emptyStates);
                            uncollapsed[i, j] = a;

                        }


                    }
                }
            }

            return uncollapsed;
        }

        public Tuple<List<Point3d>, List<Point3d>, List<Point3d>> Visualise()
        {
            half = new List<Point3d>();
            full = new List<Point3d>();
            empty = new List<Point3d>();

            for (int i = 0; i < waveCollapse.GetLength(0); i ++)
            {
                for (int j = 0; j < waveCollapse.GetLength(1); j ++)
                {
                    if (waveCollapse[i, j] == null) continue;

                    Point3d center = new Point3d((double)i, (double)j, 0.0);

                    Point3d[,] tilePoints = new Point3d[2, 2];
                    tilePoints[0, 0] = new Point3d((center.X - 1) * 2, (center.Y - 1) * 2, 0.0);
                    //tilePoints[0, 1] = new Point3d((center.X - 1) * 2, (center.Y + 1) * 2, 0.0);
                    //tilePoints[1, 0] = new Point3d((center.X + 1) * 2, (center.Y - 1) * 2, 0.0);
                    //tilePoints[1, 1] = new Point3d((center.X + 1) * 2, (center.Y + 1) * 2, 0.0);

                    //for (int m = 0; m < waveCollapse[i, j].MiniTile.GetLength(0); m++)
                    //{
                    //    for (int n = 0; n < waveCollapse[i, j].MiniTile.GetLength(1); n++)
                        {
                            if (waveCollapse[i, j].MiniTile[0,0] == State.HALF_TILE)
                            {
                                half.Add(tilePoints[0, 0]);
                            }
                            else if (waveCollapse[i, j].MiniTile[0, 0] == State.FULL_TILE)
                            {
                                full.Add(tilePoints[0, 0]);
                            }
                            else if (waveCollapse[i, j].MiniTile[0, 0] == State.EMPTY)
                            {
                                empty.Add(tilePoints[0, 0]);
                            }
                        }
                //    }
                }
            }

            var sortedHalfPoints = half.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            var sortedFullPoints = full.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            var sortedEmptyPoints = empty.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();

            //var halfResult = RemoveDuplicatesPoints(sortedHalfPoints);
            //var fullResult = RemoveDuplicatesPoints(sortedFullPoints);
            //var emptyResult = RemoveDuplicatesPoints(sortedEmptyPoints);

            return Tuple.Create(sortedHalfPoints, sortedFullPoints, sortedEmptyPoints);
        }

        public bool Contradiction()
        {
            return LowestEntropy() == -1;
        }

        public List<Point3d> RemoveDuplicatesPoints(List<Point3d> rawPatternsWithRotations)
        {
            var duplicatesRemoved = new List<Point3d>(rawPatternsWithRotations);

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


        public Tuple<int, int, Pattern> Observe()
        {
            // Find lowest entropy 
            var lowestEntropyPosition = FindLowestEntropy();
            var nextPatternToSeedX = lowestEntropyPosition.Item1;
            var nextPatternToSeedY = lowestEntropyPosition.Item2;

            // find pattern element to place in the index indicated above
            //var nextPattern = wave.superpositions[nextPatternToSeedX, nextPatternToSeedY].GetRandomPatternWithHighWeight(patterns);
            var nextPattern = this.PickRandomPatternForGivenSuperposition(nextPatternToSeedX, nextPatternToSeedY);
            //var nextPatternToPlaceToWave = nextPattern.Item1; //pattern
            //var nextPatternToPlaceToWaveIndex = nextPattern.Item2; //index of this pattern

            return Tuple.Create(nextPatternToSeedX, nextPatternToSeedY, nextPattern);
        }

        Tuple<int, int> FindLowestEntropy()
        {
            int lowEntropy = this.LowestEntropy();

            // If there is contradiction, throw an error and quit

            //if (lowEntropy == -1)
            //{
            //    throw new InvalidOperationException("Cell has no patterns left to fill - contradiction");
            //}

            if (lowEntropy == 0)
            {
                var indicesOfPatternsWithTheLowestEntropy = this.GetIndicesOfCellsWithLowestEntropy();
                var lowestEntropyPattern = indicesOfPatternsWithTheLowestEntropy[0];
                return lowestEntropyPattern;
            }
            // If all cells are at entropy 0, processing is complete: Return CollapsedObservations()


            int xLoc = 0;
            int yLoc = 0;
            var pickedCellFromWave = this.GetRandomCellWithLowestEntropy();

            // find index of this selection
            for (int x = 0; x < this.superpositions.GetLength(0); ++x)
            {
                for (int y = 0; y < this.superpositions.GetLength(1); ++y)
                {
                    //if (wave.superpositions[x, y].Equals(pickedCellFromWave))
                    if (this.superpositions[x, y] == pickedCellFromWave)
                    {
                        xLoc = x;
                        yLoc = y;
                    }
                }
            }
            return Tuple.Create(xLoc, yLoc);
        }

        public int GetCollapsedCount()
        {
            var counter = 0;
            foreach(var el in waveCollapse)
            {
                if (el != null)
                {
                    counter++;
                }
            }
            return counter;
        }

        public int GetNonCollapsedCount()
        {
            var counter = 0;
            foreach (var el in waveCollapse)
            {
                if (el == null)
                {
                    counter++;
                }
            }
            return counter;
        }

        public bool IsCollapsed()
        {
            if (this.GetNonCollapsedCount() == 0)
            {
                return true;
            }

            return false;
        }

        public void PropagateByUpdatingSuperposition(int xPatternCoordOnWave, int yPatternCoordonWave, Pattern placedPatternOnWave)
        {
            var patternIndex = patterns.FindIndex(c => c == placedPatternOnWave);
            // assign pattern
            waveCollapse[xPatternCoordOnWave, yPatternCoordonWave] = placedPatternOnWave;

            // assign superpositions:

            // in the wave superposition for this location, make only pattern that is placed on wave true, else to false
            var currentSuperposition = superpositions[xPatternCoordOnWave, yPatternCoordonWave];
            currentSuperposition.MakeAllFalseBesideOnePattern(patternIndex);

            // initialize list of overlapping neigbours that could be next tile
            placedPatternOnWave.InitializeListOfOverlappingNeighbours(patterns);

            // setup indices of superpositions on the wave that is influenced by placed pattern
            int patternSuperpositionsXSize = placedPatternOnWave.overlapsSuperpositions.GetLength(0); // 3
            int patternSuperpositionsYSize = placedPatternOnWave.overlapsSuperpositions.GetLength(1);

            // go through every overlapping neighbour and overlay pattern superpositions over wave superpositions
            for (int i = 0; i < patternSuperpositionsXSize; i++)
            {
                for (int j = 0; j < patternSuperpositionsYSize; j++)
                {
                    if (i == (patternSuperpositionsXSize - 1) / 2 && j == (patternSuperpositionsYSize - 1) / 2) continue;

                    int superpositionIndexX = xPatternCoordOnWave - ((patternSuperpositionsXSize - 1) / 2) + i;
                    int superpositionIndexY = yPatternCoordonWave - ((patternSuperpositionsYSize - 1) / 2) + j;

                    if (superpositionIndexX < 0 || superpositionIndexY < 0 || superpositionIndexX > superpositions.GetLength(0) - 1 || superpositionIndexY > superpositions.GetLength(1) - 1) continue;

                    if (waveCollapse[superpositionIndexX, superpositionIndexY] == null)
                    {
                        superpositions[superpositionIndexX, superpositionIndexY].OverlayWithAnother(placedPatternOnWave.overlapsSuperpositions[i, j]);
                    }

                }
            }
        }


        // gets the superposition with lowest entropy however i need index :/
        public Superposition GetRandomCellWithLowestEntropy()
        {
            var candidates = GetCellsWithLowestEntropy();

            System.Random random = new System.Random();
            int randomNumber = random.Next(candidates.Count - 1);

            return candidates[randomNumber];
        }

        public List<Superposition> GetCellsWithLowestEntropy()
        {
            List<Superposition> lowestEntropySuperpositions = new List<Superposition>();

            var lowestEntropy = LowestEntropy();
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    {
                        if (waveCollapse[i, j] == null && superpositions[i, j].Entropy == lowestEntropy) lowestEntropySuperpositions.Add(superpositions[i, j]);
                    }
                }
            }
            return lowestEntropySuperpositions;
        }

        public List<Tuple<int, int>> GetIndicesOfCellsWithLowestEntropy()
        {
            var lowestEntropySuperpositionsIndices = new List<Tuple<int, int>>();

            var lowestEntropy = LowestEntropy();
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (waveCollapse[i, j] == null && superpositions[i, j].Entropy == lowestEntropy)
                    {
                        lowestEntropySuperpositionsIndices.Add(new Tuple<int, int>(i, j));
                    }
                }
            }
            return lowestEntropySuperpositionsIndices;
        }

        public int LowestEntropy()
        {
            int lowest = 1000;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (waveCollapse[i, j] == null && superpositions[i, j].Entropy < lowest)
                    {
                        lowest = superpositions[i, j].Entropy;
                    }
                }
            }

            if (lowest == 1000) return 0;
            else return lowest;
        }

        public Pattern PickRandomPatternForGivenSuperposition(int nextPatternToSeedX, int nextPatternToSeedY)
        {
            return superpositions[nextPatternToSeedX, nextPatternToSeedY].GetRandomPatternWithHighWeight(patterns);
        }
    }
}