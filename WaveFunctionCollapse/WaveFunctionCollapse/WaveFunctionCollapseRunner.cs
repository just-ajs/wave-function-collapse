using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveFunctionCollapse
{
    public class WaveFunctionCollapseRunner
    {
        //int N = 2;
        Wave wave;

        // Main function in which wave function is being observed
        public WaveCollapseHistory Run(List<Pattern> patterns, IEnumerable<Point3d> unitElementsOfTypeA, IEnumerable<Point3d> unitElementsOfTypeB, IEnumerable<Point3d> areaCentres,
            int N, List<Point3d> wavePoints, float[] weights)
        {
            WaveCollapseHistory timelapse = new WaveCollapseHistory(); ;

            var guardCounter = 0;
            while (guardCounter < 15)
            {
                guardCounter++;
                bool collapsedObservations = false;
                int steps = 0;
                
                // Get size of wave
                int initialWidth = GetNumberofPointsInOneDimension(wavePoints[0].X, wavePoints[wavePoints.Count - 1].X);
                int initialHeight = GetNumberofPointsInOneDimension(wavePoints[0].Y, wavePoints[wavePoints.Count - 1].Y);

                int width = initialWidth / (N - 1);
                int height = initialHeight / (N - 1);

                wave = new Wave(width, height, patterns, N);

                // Start with random seed
                SeedRandom(width, height, patterns);
                AddCurrentFrameToHistory(timelapse);

                // Break if contradiction, otherwise run observations until it is not completaly observed
                while (!wave.IsCollapsed())
                {
                    if (wave.Contradiction()) { break; }

                    //Ponizej to dopisane
                    var waveCopy = new Wave(width, height, patterns, N);
                    waveCopy = wave;
                    var observedCopy = waveCopy.Observe();
                    waveCopy.PropagateByUpdatingSuperposition(observedCopy.Item1, observedCopy.Item2, observedCopy.Item3);

                    if (waveCopy.Contradiction())
                    {
                        wave.BlockPattern(observedCopy.Item1, observedCopy.Item2, observedCopy.Item3);
                        continue;

                    }
                    else
                    {
                        wave.PropagateByUpdatingSuperposition(observedCopy.Item1, observedCopy.Item2, observedCopy.Item3);

                        AddCurrentFrameToHistory(timelapse);
                        steps++;
                    }
                    // To było wczesniej:
                    // var observed = wave.Observe();
                    // wave.PropagateByUpdatingSuperposition(observed.Item1, observed.Item2, observed.Item3);

                    //AddCurrentFrameToHistory(timelapse);
                    // steps++;
                }

                if (wave.Contradiction())
                {
                    timelapse.Clear();
                    continue;
                }
                else
                {
                    break;
                }

            }
            return timelapse;
        }

        public void SeedRandom(int width, int height, List<Pattern> patterns)
        {
            Random xRand = new Random();
            Random yRand = new Random();

            var x = xRand.Next(width);
            var y = yRand.Next(height);

            var seedPattern = GetSeedPattern(x, y, patterns);
            wave.PropagateByUpdatingSuperposition(x, y, seedPattern);
        }

        public int[] GetPatternCounts()
        {
            return wave.GetCollapsedPatternsCounts();
        }

        // For time visualisation - each step is added to the timelapse
        void AddCurrentFrameToHistory(WaveCollapseHistory timelapse)
        {
            var timeFrameToPoints = OutputObservations();
            var timeFrameUncollapsed =  wave.GetPossibleTileTypes();
            var patternOccurence = wave.GetCollapsedPatternsCounts();

            var timeFrameElement = new WaveCollapseHistoryElement(timeFrameToPoints.Item1, 
                timeFrameToPoints.Item2, timeFrameToPoints.Item3, wave.superpositions, timeFrameUncollapsed, patternOccurence);
            timelapse.AddTimeFrame(timeFrameElement);
        }

        // Get 3d points of new tiles 
        public Tuple<List<Point3d>, List<Point3d>, List<Point3d>> OutputObservations()
        {
            var k = wave.Visualise();
            return Tuple.Create(k.Item1, k.Item2, k.Item3);
        }

        // Find width and height of surface
        public int GetNumberofPointsInOneDimension(double firstPointCoordinate, double secondPointCoordinate)
        {
            return Math.Abs((int)(0.5 * (firstPointCoordinate - secondPointCoordinate) - 1));
        }

        // Return first pattern that will be a seed for surface
        public Pattern GetSeedPattern(int x, int y, List<Pattern> patternsFromSample)
        {
            var patternToSeed = highestWeightPattern(patternsFromSample);

            var index = FindPatternIndex(patternToSeed, patternsFromSample);

            var outputSeed = patternToSeed.Instantiate(x, y, 0);

            return patternToSeed;
        }

        // Get pattern with highest weight, if there is few of them - pick randomly
        public Pattern highestWeightPattern(List<Pattern> patterns)
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

        // Check which index has provided pattern in a pattern library
        int FindPatternIndex(Pattern patternToFind, List<Pattern> listOfPatterns)
        {
            int index = 0;
            for (int i = 0; i < listOfPatterns.Count; i++)
            {
                if (Pattern.ReferenceEquals(patternToFind, listOfPatterns[i])) index = i;
            }

            return index;
        }
     
    }
}
