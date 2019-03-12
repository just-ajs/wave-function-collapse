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
        int N = 2;
        Wave wave;

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
                
                // get size of wave
                int width = GetNumberofPointsInOneDimension(wavePoints[0].X, wavePoints[wavePoints.Count - 1].X);
                int height = GetNumberofPointsInOneDimension(wavePoints[0].Y, wavePoints[wavePoints.Count - 1].Y);

                // start with random seed in the middle
                wave = new Wave(width, height, patterns);
                var seedPattern = GetSeedPattern(width / 2, height / 2, patterns);
                wave.PropagateByUpdatingSuperposition(width / 2, height / 2, seedPattern);

                AddCurrentFrameToHistory(timelapse);

                while (!wave.IsCollapsed())
                {
                    if (wave.Contradiction()) { break; }

                    var observed = wave.Observe();
                    wave.PropagateByUpdatingSuperposition(observed.Item1, observed.Item2, observed.Item3);

                    AddCurrentFrameToHistory(timelapse);
                    steps++;
                }

                if (wave.Contradiction()) {
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

        void AddCurrentFrameToHistory(WaveCollapseHistory timelapse)
        {
            var timeFrameToPoints = OutputObservations();
            var timeFrameUncollapsed =  wave.GetPossibleTileTypes();
            var timeFrameElement = new WaveCollapseHistoryElement(timeFrameToPoints.Item1, timeFrameToPoints.Item2, timeFrameToPoints.Item3, wave.superpositions, timeFrameUncollapsed);
            timelapse.AddTimeFrame(timeFrameElement);
        }

        public Tuple<List<Point3d>, List<Point3d>, List<Point3d>> OutputObservations()
        {
            var k = wave.Visualise();
            return Tuple.Create(k.Item1, k.Item2, k.Item3);
        }

        public TileSuperposition[,] OutputUnobserved()
        {
            var k = wave.GetPossibleTileTypes();
            return k;
        }

        public int GetNumberofPointsInOneDimension(double firstPointCoordinate, double secondPointCoordinate)
        {
            return Math.Abs((int)(0.5 * (firstPointCoordinate - secondPointCoordinate) - 1));
        }

        public Pattern GetSeedPattern(int x, int y, List<Pattern> patternsFromSample)
        {
            var patternToSeed = highestWeightPattern(patternsFromSample);

            var index = FindPatternIndex(patternToSeed, patternsFromSample);

            var outputSeed = patternToSeed.Instantiate(x, y, 0);

            return patternToSeed;
        }

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

        int FindPatternIndex(Pattern patternToFind, List<Pattern> listOfPatterns)
        {
            int index = 0;
            for (int i = 0; i < listOfPatterns.Count; i++)
            {
                if (Pattern.ReferenceEquals(patternToFind, listOfPatterns[i])) index = i;
            }

            return index;
        }

        void Propagate(int waveX, int waveY, Pattern chosenForXY, Wave wave)
        {
            
        }

      
    }
}
