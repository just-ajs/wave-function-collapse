using System.Collections.Generic;

namespace WaveFunctionCollapse
{
    internal class Wave
    {
        int width;
        int height;
        private Superposition[,] superpositions;
        List<Pattern> patterns;


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
        }

        void UpdateSuperPosition (int xPatternCoordOnWave, int yPatternCoordonWave, Pattern placedPatternOnWave, int patternIndex)
        {
            // in the wave superposition for this location, make only pattern that is placed on wave true, else to false
            superpositions[xPatternCoordOnWave, yPatternCoordonWave].MakeAllFalseBesideOnePattern(patternIndex);

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
                    int superpositionIndexY = yPatternCoordonWave - ((patternSuperpositionsYSize - 1) / 2) + i;

                    superpositions[superpositionIndexX, superpositionIndexY].OverlayWithAnother(placedPatternOnWave.overlapsSuperpositions[i, j]);

                }
            }
        }

        Superposition GetCellWithLowestEntropy()
        {
            var candidates = GetCellsWithLowestEntropy();

            System.Random random = new System.Random();
            int randomNumber = random.Next(candidates.Count - 1);

            return candidates[randomNumber];
        }

        List<Superposition> GetCellsWithLowestEntropy()
        {
            List<Superposition> lowestEntropySuperpositions = new List<Superposition>();

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (superpositions[i, j].Entropy == LowestEntropy()) lowestEntropySuperpositions.Add(superpositions[i, j]);
                }
            }
            return lowestEntropySuperpositions;
        }

        int LowestEntropy()
        {
            int lowest = 1000;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (superpositions[i, j].Entropy < lowest) lowest = superpositions[i, j].Entropy;
                }
            }
            return lowest;
        }



    }
}