using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WaveFunctionCollapse
{
    internal class Wave : ICloneable
    {
        int width;
        int height;
        public Superposition[,] superpositions;
        public Pattern[,] waveCollapse;
        readonly List<Pattern> patterns;
        readonly int patternSize;
        float lowestWeight;
        float highestWeight;

        readonly double[,] image;
        
        //public List<double> recalculatedWeights;
        //public List<Pattern> recalculatedPatterns;

        public SortedList<double, Vector2d> cellsByEntropy = new SortedList<double, Vector2d>();
        public SortedList<double, Vector2d> cellsByEntropyFromImage = new SortedList<double, Vector2d>();

        public double[,] entropies;

        public List<Point3d> half;
        public List<Point3d> full;
        public List<Point3d> empty;

        private static Random random = new Random();
        //private Wave wave;

        public Wave(int width, int height, List<Pattern> patterns, int patternSize)
        {
            this.width = width;
            this.height = height;
            this.patterns = patterns;
            this.patternSize = patternSize;
            

            lowestWeight = getLowestWeight(patterns);
            highestWeight = getHighestWeight(patterns);

            superpositions = new Superposition[width, height];


            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    superpositions[i, j] = new Superposition(patterns);
                }
            }

            entropies = new double[width, height];
            waveCollapse = new Pattern[width, height];
            OrderCellsListByEntropy();
            UpdateEntropies();
        }

        public Wave(int width, int height, List<Pattern> patterns, int patternSize,
            double[,] image)
        {
            this.width = width;
            this.height = height;
            this.patterns = patterns;
            this.patternSize = patternSize;

            this.image = image;

            lowestWeight = getLowestWeight(patterns);
            highestWeight = getHighestWeight(patterns);

            superpositions = new Superposition[width, height];



            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    superpositions[i, j] = new Superposition(patterns);
                }
            }

            entropies = new double[width, height];
            waveCollapse = new Pattern[width, height];

            // Sort all elements by the entropy
            OrderCellsListByEntropy();

            // Sort all elements that on the picture has lowe
            OrderCellsListByEntropyAndImage();

        }

        public Wave(Wave wave)
        {
            var xD = new Wave(wave.width, wave.height, wave.patterns, wave.patternSize);

            for (int i = 0; i < wave.width; i++)
            {
                for (int j = 0; j < wave.height; j++)
                {
                    xD.superpositions[i, j] = (Superposition)wave.superpositions[i, j].Clone();
                }
            }

            for (int i = 0; i < wave.width; i++)
            {
                for (int j = 0; j < wave.height; j++)
                {
                    xD.waveCollapse[i, j] = (Pattern)wave.waveCollapse[i, j].Clone();
                }
            }

            for (int i = 0; i < wave.cellsByEntropy.Count; i++)
            {
                //xD.cellsByEntropy[i] = wave.cellsByEntropy[i].Clone();
            }
        }

        public void UpdateEntropies()
        {
            for (int i = 0; i < superpositions.GetLength(0); i++)
            {
                for (int j = 0; j < superpositions.GetLength(1); j++)
                {
                    entropies[i, j] = superpositions[i, j].Entropy;
                }
            }
        }

        // Check the superposition for each place in wave in order to check how many patterns are possible to be placed in each location
        public TileSuperposition[,] GetPossibleTileTypes()
        {
            TileSuperposition[,] uncollapsed = new TileSuperposition[width, height];

            for (int i = 0; i < waveCollapse.GetLength(0); i ++)
            {
                for (int j = 0; j < waveCollapse.GetLength(1); j ++)
                {
                    for (int m = 0; m < 2; m++)
                    {
                        for (int n = 0; n < 2; n++)
                        {
                            int halfStates = 0;
                            int fullStates = 0;
                            int emptyStates = 0;

                            for (int k = 0; k < superpositions[i, j].coefficients.Length; k++)
                            {
                                if (superpositions[i, j].coefficients[k] == true)
                                {
                                    if (patterns[k].MiniTile[m,n] == State.HALF_TILE)
                                    {
                                        halfStates++;
                                    }
                                    else if (patterns[k].MiniTile[m,n] == State.FULL_TILE)
                                    {
                                        fullStates++;
                                    }
                                    else if (patterns[k].MiniTile[m,n] == State.EMPTY)
                                    {
                                        emptyStates++;
                                    }
                                }
                            }
                            TileSuperposition a = new TileSuperposition(halfStates, fullStates, emptyStates);
                            if (i + m > waveCollapse.GetLength(0)-1) continue;
                            if (j + n > waveCollapse.GetLength(1)-1) continue;

                            if (uncollapsed[i + m, j + n] != null)
                            {
                                if (uncollapsed[i + m, j + n].Sum > a.Sum)
                                {
                                    uncollapsed[i + m, j + n] = a;
                                }
                            }
                            else
                            {
                                uncollapsed[i + m, j + n] = a;
                            }
                        }
                    }
                }
            }

            return uncollapsed;
        }

        // Convert states to points
        public Tuple<List<Point3d>, List<Point3d>, List<Point3d>> Visualise()
        {
            half = new List<Point3d>();
            full = new List<Point3d>();
            empty = new List<Point3d>();

            for (int i = 0; i < waveCollapse.GetLength(0); i++)
            {
                for (int j = 0; j < waveCollapse.GetLength(1); j++)
                {
                    if (waveCollapse[i, j] == null) continue;

                    Point3d center = new Point3d((double)i, (double)j, 0.0);

                    Point3d[,] tilePoints = new Point3d[2, 2];
                    tilePoints[0, 0] = new Point3d((center.X - 1) * 2, (center.Y - 1) * 2, 0.0);

                    {
                        if (waveCollapse[i, j].MiniTile[0, 0] == State.HALF_TILE)
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
                }
            }

            var sortedHalfPoints = half.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            var sortedFullPoints = full.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            var sortedEmptyPoints = empty.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();

            return Tuple.Create(sortedHalfPoints, sortedFullPoints, sortedEmptyPoints);
        }

        public bool Contradiction()
        {
            return LowestEntropy() == -1;
        }

        // Find lowest entropy position in entire wave, pick new pattern for this position
        public Tuple<int, int, Pattern> Observe()
        {
            // Sorted list: Find lowest entropy cooridnates based on sorted list
            var coordx = (int)cellsByEntropy.Values[0].X;
            var coordy = (int)cellsByEntropy.Values[0].Y;

            // Find pattern for lowest entropy position
            var nextPattern = this.PickRandomPatternForGivenSuperposition(coordx, coordy);
            return Tuple.Create(coordx, coordy, nextPattern);
        }



        // Find lowest entropy position in entire wave, pick new pattern for this position
        public Tuple<int, int, Pattern, int> ObserveWithImage(double[,] image)
        {
            // Select location
            int coordx, coordy;
            double _random = random.Next(100)/100;

            if ((cellsByEntropyFromImage.Count > 0))// && (_random < 0.9))
            {
               coordx = (int)cellsByEntropyFromImage.Values[0].X;
               coordy = (int)cellsByEntropyFromImage.Values[0].Y;
            }
            else
            {
                coordx = (int)cellsByEntropy.Values[0].X;
                coordy = (int)cellsByEntropy.Values[0].Y;
            }

            // Select pattern
            var nextPattern = new Pattern();
            int patternIndex = 0;

            if (image[coordx, coordy] < 0.1)
            {
                float weight01 = 0.10f;
                float weight02 = 0.45f;
                float weight03 = 0.45f;

                // Recalculate patterns and save new weights with patterns to csv file
                var recalculatedPatternsForThisPixel = RecalculateWeights(image, coordx, coordy, weight01, weight02, weight03);
                var recalculatedWeights = GetWeightsFromPatterns(recalculatedPatternsForThisPixel);
                var originalWeights = GetWeightsFromPatterns(patterns);

                Utils.SaveWeightToFile(originalWeights, recalculatedWeights, weight01 + "_" + weight02 + "_" + weight03);
                
                // Find pattern for lowest entropy position
                nextPattern = this.PickRandomPatternForGivenSuperpositionWithTweakedWeights(coordx, coordy, recalculatedPatternsForThisPixel);
                patternIndex = getPatternIndex(recalculatedPatternsForThisPixel, nextPattern);
            }
            else
            {
                // Find pattern for lowest entropy position
                nextPattern = this.PickRandomPatternForGivenSuperposition(coordx, coordy);
                patternIndex = getPatternIndex(patterns, nextPattern);

            }

            return Tuple.Create(coordx, coordy, nextPattern, patternIndex);
        }


        // Find lowest entropy position in entire wave, pick new pattern for this position
        public Tuple<int, int, Pattern, int> ObserveWithImageAndWeights(double[,] image, List<double> newWeights)
        {
            int coordx, coordy;
            double _random = random.Next(100)/100;

            if ((cellsByEntropyFromImage.Count > 0) && (_random < 0.9))
            {
                coordx = (int)cellsByEntropyFromImage.Values[0].X;
                coordy = (int)cellsByEntropyFromImage.Values[0].Y;
            }
            else
            {
                coordx = (int)cellsByEntropy.Values[0].X;
                coordy = (int)cellsByEntropy.Values[0].Y;
            }

            var nextPattern = new Pattern();
            int patternIndex = 0;

            if (image[coordx, coordy] < 0.3)
            {

                float weight01 = 0.10f;
                float weight02 = 0.45f;
                float weight03 = 0.45f;

                // Recalculate patterns and save new weights with patterns to csv file
                var recalculatedPatternsForThisPixel = AssignNewWeights(image, coordx, coordy, newWeights);
                var recalculatedWeights = GetWeightsFromPatterns(recalculatedPatternsForThisPixel);
                var originalWeights = GetWeightsFromPatterns(patterns);

                //Utils.SaveWeightToFile(originalWeights, recalculatedWeights, weight01 + "_" + weight02 + "_" + weight03);

                // Find pattern for lowest entropy position
                nextPattern = this.PickRandomPatternForGivenSuperpositionWithTweakedWeights(coordx, coordy, recalculatedPatternsForThisPixel);
                patternIndex = getPatternIndex(recalculatedPatternsForThisPixel, nextPattern);
            }
            else
            {
                // Find pattern for lowest entropy position
                nextPattern = this.PickRandomPatternForGivenSuperposition(coordx, coordy);
                patternIndex = getPatternIndex(patterns, nextPattern);

            }

            return Tuple.Create(coordx, coordy, nextPattern, patternIndex);
        }

        List<double> GetWeightsFromPatterns (List<Pattern> list)
        {
            List<double> newWeights = new List<double>();

            for (int i = 0; i < list.Count; i++)
            {
                newWeights.Add(list[i].Weight);
            }

            return newWeights;
        }

        List<Pattern> AssignNewWeights(double[,] image, int coordX, int coordY, List<double> newWeights)
        {
            // Take image value - 0 is black, 1 is white
            float cellValueFromImage = (float)image[coordX, coordY];

            // Copy patterns
            List<Pattern> clone = new List<Pattern>();
            for (int i = 0; i < patterns.Count; i++)
            {
                var patternClone = new Pattern(patterns[i].MiniTile, patterns[i].overalWeights, patterns[i].patternSize);
                clone.Add(patternClone);
            }

            // Go through every pattern and reassing values
            for (int i = 0; i < clone.Count; i++)
            {
                clone[i].Weight = (float)newWeights[i];
            }

            return clone;
        }

        List<Pattern> RecalculateWeights(double[,] image, int coordX, int coordY, float weight01, float weight02, float weight03)
        {
            // Take image value - 0 is black, 1 is white
            float cellValueFromImage = (float)image[coordX, coordY];

            // Copy patterns
            List<Pattern> clone = new List<Pattern>();
            for (int i = 0; i < patterns.Count; i++)
            {
                var patternClone = new Pattern(patterns[i].MiniTile, patterns[i].overalWeights, patterns[i].patternSize);
                clone.Add(patternClone);
            }

            // Get weights
            var weights = clone[0].overalWeights;

            var stateEmptyWeight = weights[0];
            var stateHalfWeight = weights[1];
            var stateFullWeight = weights[2];

            // Go through every pattern and reassing values
            for (int i = 0; i < clone.Count; i++)
            {
                // For each pattern:
                // Calculate new weights
                float sumWeights = 0;

                for (int k = 0; k < patternSize; k++)
                {
                    for (int m = 0; m < patternSize; m++)
                    {
                        if (clone[i].MiniTile[k, m] == State.EMPTY)
                        {
                            sumWeights += weight01;
                        }
                        else if (clone[i].MiniTile[k, m] == State.HALF_TILE)
                        {
                            sumWeights += weight02;
                        }
                        else if (clone[i].MiniTile[k, m] == State.FULL_TILE)
                        {
                            sumWeights += weight03;
                        }
                    }
                }
                clone[i].Weight = powerWeight(clone[i], sumWeights);
            }

            // This should remap values so they are in the same range as in the original image.
            // remapTheWeightsToLowestAndHighestWeight(clone);

            return clone;
        }


        float powerWeight (Pattern pattern, float weight)
        {
            double weightToThePower = 0;
            int notEmpty = 0;
            for (int i = 0; i < pattern.patternSize; i++)
            {
                for (int j = 0; j < pattern.patternSize; j++)
                {
                    if (pattern.MiniTile[i,j] == State.FULL_TILE || pattern.MiniTile[i,j] == State.HALF_TILE)
                    {
                        notEmpty++;
                    }
                }
            }
            double power = notEmpty + 1.0f;

            if (notEmpty != 0)
            {
                weightToThePower = Math.Pow((double)weight, power);
            }

            
            if (weightToThePower > 0)
            {
                return (float)weightToThePower;
            }
            else
            {
                return weight;
            }
        }

        void remapTheWeightsToLowestAndHighestWeight(List<Pattern> recalculatedPatterns)
        {
            // The range of weights of recalculated patterns
            var start1 = getLowestWeight(recalculatedPatterns);
            var start2 = getHighestWeight(recalculatedPatterns);

            for (int i = 0; i < recalculatedPatterns.Count; i++)
            {
                float weight = recalculatedPatterns[i].Weight;
                var newWeight = remap(weight, start1, start2, lowestWeight, highestWeight);

                recalculatedPatterns[i].Weight = newWeight;
            }
           

        }

        float remap(float value, float start1, float stop1,
            float start2, float stop2)
        {
            float result = start2 + (stop2 - start2) * ((value - start1) / (stop1 - start1));

            return result;
        }

        float getLowestWeight(List<Pattern> patterns)
        {
            float lowest = 100000.0f;

            for (int i = 0; i < patterns.Count; i++)
            {
                if (patterns[i].Weight < lowest)
                {
                    lowest = patterns[i].Weight;
                }
            }

            return lowest; 
        }

        float getHighestWeight(List<Pattern> patterns)
        {
            float highest = 0.0f;

            for (int i = 0; i < patterns.Count; i++)
            {
                if (patterns[i].Weight > highest)
                {
                    highest = patterns[i].Weight;
                }
            }

            return highest;
        }

        // Count collapsed places in wave
        public int[] GetCollapsedPatternsCounts()
        {
            var patternCounts = new int[patterns.Count];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var currPattern = waveCollapse[i, j];

                    var patternIndex = patterns.FindIndex(c => c == waveCollapse[i, j]);
                    if (patternIndex < 0) continue;

                    if (patternCounts[patternIndex] == 0)
                    {
                        patternCounts[patternIndex] = 1;
                    }
                    else
                    {
                        patternCounts[patternIndex]++;
                    }
                }
            }
            return patternCounts;
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

        // Check if wave is collapse
        public bool IsCollapsed()
        {
            if (this.GetNonCollapsedCount() == 0)
            {
                return true;
            }

            return false;
        }


        // Block given pattern 
        public void BlockPatternOnWave(int xPatternCoordOnWave, int yPatternCoordonWave, Pattern placedPatternOnWave)
        {
            // Make false value for this pattern.
            var patternIndex = patterns.FindIndex(c => c == placedPatternOnWave);

            var x = superpositions[xPatternCoordOnWave, yPatternCoordonWave];
            x.coefficients[patternIndex] = false;

            // Calculate new entropy
            x.CalculateEntropy();

            // Update list sorted by entropies.
            var patternIndexInSortedList = cellsByEntropy.IndexOfValue(new Vector2d(xPatternCoordOnWave, yPatternCoordonWave));
            cellsByEntropy.RemoveAt(patternIndexInSortedList);

            // Add new key with new entropy
            cellsByEntropy.Add(x.Entropy, new Vector2d(xPatternCoordOnWave, yPatternCoordonWave));
        }

        // With every new placed pattern, its overlapping superposition needs to be applied for a wave
        public void PropagateByUpdatingSuperposition(int xPatternCoordOnWave, int yPatternCoordonWave, Pattern placedPatternOnWave)
        {
            var patternIndex = patterns.FindIndex(c => c == placedPatternOnWave);

            // assign pattern
            waveCollapse[xPatternCoordOnWave, yPatternCoordonWave] = placedPatternOnWave;

            // in the wave superposition for this location, make only pattern that is placed on wave true, else to false
            var currentSuperposition = superpositions[xPatternCoordOnWave, yPatternCoordonWave];
            currentSuperposition.MakeAllFalseBesideOnePattern(patternIndex);

            // In the sorted list, remove chosen pattern
            var patternIndexInSortedList = cellsByEntropy.IndexOfValue(new Vector2d(xPatternCoordOnWave, yPatternCoordonWave));

            // Find in another list that element, if it is there, remove it
            if (cellsByEntropyFromImage.ContainsValue(new Vector2d(xPatternCoordOnWave, yPatternCoordonWave)))
            {
                var patternIndexInSortedList_Image = cellsByEntropyFromImage.IndexOfValue(new Vector2d(xPatternCoordOnWave, yPatternCoordonWave));
                cellsByEntropyFromImage.RemoveAt(patternIndexInSortedList_Image);

            }

            cellsByEntropy.RemoveAt(patternIndexInSortedList);

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
                        // Save superposition location as vector
                        Vector2d vector = new Vector2d(superpositionIndexX, superpositionIndexY);

                        // Find index of this element in the sorted by entropy list
                        var indexOfCellsByEntropy = cellsByEntropy.IndexOfValue(vector);
                        // Remove that element from the sorted list
                        cellsByEntropy.RemoveAt(indexOfCellsByEntropy);

                        superpositions[superpositionIndexX, superpositionIndexY].OverlayWithAnother(placedPatternOnWave.overlapsSuperpositions[i, j]);

                        // Add to sorted list new position with same x, y and new entropy
                        double newEntropyKey = superpositions[superpositionIndexX, superpositionIndexY].Entropy;

                        if (newEntropyKey == 0)
                        {
                            var noise = superpositions[superpositionIndexX, superpositionIndexY].AddNoise();
                            newEntropyKey += noise;

                        } else if (newEntropyKey == -1)
                        {
                            return;
                            throw new DataMisalignedException("contradiction!");
                        }
                            
                        cellsByEntropy.Add(newEntropyKey, vector);

                        // Repeat the same for fake list
                        if (cellsByEntropyFromImage.ContainsValue(vector))
                        {
                            // Check if the list contains this value, if yes, remove, if not continue
                            var indexOfCellsByEntropy_Image = cellsByEntropyFromImage.IndexOfValue(vector);
                            cellsByEntropyFromImage.RemoveAt(indexOfCellsByEntropy_Image);
                            cellsByEntropyFromImage.Add(newEntropyKey, vector);

                        }
                    }
                }
            }
            UpdateEntropies();

        }

        int getPatternIndex(List<Pattern> list, Pattern pattern)
        {
            var patternIndex = list.FindIndex(c => c == pattern);
            return patternIndex;
        }

        // With every new placed pattern, its overlapping superposition needs to be applied for a wave.
        public void PropagateByUpdatingSuperpositionWithRecalculatedPatterns(int xPatternCoordOnWave, int yPatternCoordonWave, 
            Pattern placedPatternOnWave, List<Pattern> recalculatedPatterns)
        {
            // Get index of chosen pattern.
            var patternIndex = getPatternIndex(recalculatedPatterns, placedPatternOnWave);

            // Real pattern with raw weights.
            var realPattern = patterns[patternIndex];
            
            // Assign pattern.
            waveCollapse[xPatternCoordOnWave, yPatternCoordonWave] = realPattern;

            // in the wave superposition for this location, make only pattern that is placed on wave true, else to false.
            var currentSuperposition = superpositions[xPatternCoordOnWave, yPatternCoordonWave];
            currentSuperposition.MakeAllFalseBesideOnePattern(patternIndex);

            // In the sorted list, remove chosen pattern.
            var patternIndexInSortedList = cellsByEntropy.IndexOfValue(new Vector2d(xPatternCoordOnWave, yPatternCoordonWave));
            cellsByEntropy.RemoveAt(patternIndexInSortedList);

            // initialize list of overlapping neigbours that could be next tile.
            realPattern.InitializeListOfOverlappingNeighbours(patterns);

            // setup indices of superpositions on the wave that is influenced by placed pattern.
            int patternSuperpositionsXSize = realPattern.overlapsSuperpositions.GetLength(0); // 3
            int patternSuperpositionsYSize = realPattern.overlapsSuperpositions.GetLength(1);

            // go through every overlapping neighbour and overlay pattern superpositions over wave superpositions.
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
                        // Save superposition location as vector
                        Vector2d vector = new Vector2d(superpositionIndexX, superpositionIndexY);

                        // Find index of this element in the sorted by entropy list
                        var getIndexOfCellsByEntropy = cellsByEntropy.IndexOfValue(vector);

                        // Remove that element from the sorted list
                        cellsByEntropy.RemoveAt(getIndexOfCellsByEntropy);

                        superpositions[superpositionIndexX, superpositionIndexY].OverlayWithAnother(placedPatternOnWave.overlapsSuperpositions[i, j]);

                        // Add to sorted list new position with same x, y and new entropy
                        double newEntropyKey = superpositions[superpositionIndexX, superpositionIndexY].Entropy;

                        if (newEntropyKey == 0)
                        {
                            var noise = superpositions[superpositionIndexX, superpositionIndexY].AddNoise();
                            newEntropyKey += noise;

                        }
                        else if (newEntropyKey == -1)
                        {
                            return;
                            throw new DataMisalignedException("contradiction!");
                        }

                        cellsByEntropy.Add(newEntropyKey, vector);
                    }

                }
            }

            // Recalculate entropy by decreasing entropy in the black ares of image.
            DecreaseEntropyInDarkImageAreas(image);
            UpdateEntropies();

        }


        public bool CheckIfPropagateWithoutContradiction(int xPatternCoordOnWave, int yPatternCoordonWave, Pattern candidate)
        {
            // Find index of the chosen pattern in the patterns from sample list
            var patternIndex = patterns.FindIndex(c => c == candidate);

            // We want to do that even only for testing
            // Initialize list of overlapping neigbours that could be next tile
            candidate.InitializeListOfOverlappingNeighbours(patterns);

            // Setup indices of superpositions on the wave that is influenced by placed pattern
            int patternSuperpositionsXSize = candidate.overlapsSuperpositions.GetLength(0); 
            int patternSuperpositionsYSize = candidate.overlapsSuperpositions.GetLength(1);

            int possiblePatterns = 100;
            // Go through every overlapping neighbour and overlay pattern superpositions over wave superpositions
            for (int i = 0; i < patternSuperpositionsXSize; i++)
            {
                for (int j = 0; j < patternSuperpositionsYSize; j++)
                {
                    if (i == (patternSuperpositionsXSize - 1) / 2 && j == (patternSuperpositionsYSize - 1) / 2) continue;

                    int superpositionIndexX = xPatternCoordOnWave - ((patternSuperpositionsXSize - 1) / 2) + i;
                    int superpositionIndexY = yPatternCoordonWave - ((patternSuperpositionsYSize - 1) / 2) + j;

                    if (superpositionIndexX < 0 || superpositionIndexY < 0 || superpositionIndexX > superpositions.GetLength(0) - 1 || 
                        superpositionIndexY > superpositions.GetLength(1) - 1) continue;

                    if (waveCollapse[superpositionIndexX, superpositionIndexY] == null)
                    {
                        possiblePatterns = superpositions[superpositionIndexX, superpositionIndexY].CheckPossiblePatternsCount(candidate.overlapsSuperpositions[i, j]);
                    }

                    if (possiblePatterns == 0) { return false; }
                }
            }
            return true;
        }

        public bool CheckIfPropagateWithoutContradictionWithRecalculatedWeights(int xPatternCoordOnWave, int yPatternCoordonWave, 
            Pattern candidate, int patternIndex)
        {
            // The pattern from original data
            var proposedPattern = patterns[patternIndex];

            // We want to do that even only for testing
            // Initialize list of overlapping neigbours that could be next tile
            proposedPattern.InitializeListOfOverlappingNeighbours(patterns);

            // Setup indices of superpositions on the wave that is influenced by placed pattern
            int patternSuperpositionsXSize = proposedPattern.overlapsSuperpositions.GetLength(0);
            int patternSuperpositionsYSize = proposedPattern.overlapsSuperpositions.GetLength(1);

            int possiblePatterns = 100;
            // Go through every overlapping neighbour and overlay pattern superpositions over wave superpositions
            for (int i = 0; i < patternSuperpositionsXSize; i++)
            {
                for (int j = 0; j < patternSuperpositionsYSize; j++)
                {
                    if (i == (patternSuperpositionsXSize - 1) / 2 && j == (patternSuperpositionsYSize - 1) / 2) continue;

                    int superpositionIndexX = xPatternCoordOnWave - ((patternSuperpositionsXSize - 1) / 2) + i;
                    int superpositionIndexY = yPatternCoordonWave - ((patternSuperpositionsYSize - 1) / 2) + j;

                    if (superpositionIndexX < 0 || superpositionIndexY < 0 || superpositionIndexX > superpositions.GetLength(0) - 1 ||
                        superpositionIndexY > superpositions.GetLength(1) - 1) continue;

                    if (waveCollapse[superpositionIndexX, superpositionIndexY] == null)
                    {
                        possiblePatterns = superpositions[superpositionIndexX, superpositionIndexY].CheckPossiblePatternsCount(proposedPattern.overlapsSuperpositions[i, j]);
                    }

                    if (possiblePatterns == 0) { return false; }
                }
            }
            return true;
        }





        // In the wave find lowest entropy in order to place there a pattern
        Tuple<int, int> FindLowestEntropy()
        {
            double lowEntropy = this.LowestEntropy();

            if (lowEntropy == 0)
            {
                var indicesOfPatternsWithTheLowestEntropy = this.GetIndicesOfCellsWithLowestEntropy();
                var lowestEntropyPattern = indicesOfPatternsWithTheLowestEntropy[0];
                return lowestEntropyPattern;
            }

            int xLoc = 0;
            int yLoc = 0;
            var pickedCellFromWave = this.GetRandomCellWithLowestEntropy();

            // find index of this selection
            for (int x = 0; x < this.superpositions.GetLength(0); ++x)
            {
                for (int y = 0; y < this.superpositions.GetLength(1); ++y)
                {
                    if (this.superpositions[x, y] == pickedCellFromWave)
                    {
                        xLoc = x;
                        yLoc = y;
                    }
                }
            }
            return Tuple.Create(xLoc, yLoc);
        }

        // Get cell with lowest entropy. If few cells have same entropy, pick randomly from one of them
        public Superposition GetRandomCellWithLowestEntropy()
        {
            var candidates = GetCellsWithLowestEntropy();
            int randomNumber = random.Next(candidates.Count - 1);
            return candidates[randomNumber];
        }

        // Check cells that has least amount of possible patterns that can be placed in this cell
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

        // Lowest entropy is the value of possible patterns that can be placed in a cell with lowest possible patterns count
        public double LowestEntropy()
        {
            double lowest = 1000;

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

        // Pick pattern to seed based on roulettewheel selection (based on weights)
        public Pattern PickRandomPatternForGivenSuperposition(int nextPatternToSeedX, int nextPatternToSeedY)
        {
            return superpositions[nextPatternToSeedX, nextPatternToSeedY].RouletteWheelSelections(patterns);
        }

        // Pick pattern to seed based on roulettewheel selection (based on weights)
        public Pattern PickRandomPatternForGivenSuperpositionWithTweakedWeights(int nextPatternToSeedX, int nextPatternToSeedY, 
            List<Pattern> recalculatedPatterns)
        {
            return superpositions[nextPatternToSeedX, nextPatternToSeedY].RouletteWheelSelections(recalculatedPatterns);
        }

        void OrderCellsListByEntropy()
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    double entropy = superpositions[i, j].Entropy;
                    Vector2d coordinates = new Vector2d(i, j);

                    // Check if key is unique, if not, add more noise
                    while (cellsByEntropy.ContainsKey(entropy))
                    {
                        entropy += superpositions[i, j].AddNoise();
                    }
                    cellsByEntropy.Add(entropy, coordinates);
                }
            }
        }

        void OrderCellsListByEntropyAndImage()
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (image[i,j] < 0.9)
                    {
                        double entropy = superpositions[i, j].Entropy;
                        Vector2d coordinates = new Vector2d(i, j);

                        cellsByEntropyFromImage.Add(entropy, coordinates);
                    }
                }
            }
        }

        void DecreaseEntropyInDarkImageAreas(double[,] image)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (image[i, j] < 0.9)
                    {
                        // Chce sprawdzic czy wartosc jest mniejsza od 0.9. Jezeli tak
                        // to idziemy do listy, i po wartosicach i oraz j, zmieniamy wartosc klucza na troche mniejsza. 
                        var index = cellsByEntropy.IndexOfValue(new Vector2d(i, j));

                        if (index < 0) continue;
                        var keys = cellsByEntropy.Keys;

                        var key = keys[index];
                        var decreasedKey = key * 0.5;

                        cellsByEntropy.RemoveAt(index);
                        cellsByEntropy.Add(decreasedKey, new Vector2d(i, j));
                    }
                }
            }
        }

        public object Clone()
        {
            return new Wave(this);
        }
    }

}