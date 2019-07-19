using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace WaveFunctionCollapse
{
    public class Superposition : ICloneable
    {
        private static Random random = new Random();

        public bool[] coefficients;
        public State state;

        // The more possible patterns the highest entropy
        public double Entropy { get; private set; }
        
        readonly List<Pattern> patternFromSample;

        public Superposition(List<Pattern> patternFromSample)
        {
            this.patternFromSample = patternFromSample;

            coefficients = new bool[patternFromSample.Count];
            for (int i = 0; i < coefficients.Length; i++)
            {
                coefficients[i] = true;
            }

            CalculateEntropy();
        }

        public Superposition(Superposition superposition)
        {
            this.patternFromSample = superposition.patternFromSample;

            // Copy coefficients.
            for (var i = 0; i < superposition.coefficients.Length; i++)
            {
                this.coefficients[i] = superposition.coefficients[i];
            }

            this.CalculateEntropy();
        }

        public Superposition(Superposition superposition, List<Pattern> patternFromSample)
        {
            this.patternFromSample = patternFromSample;

            coefficients = new bool[patternFromSample.Count];

            // Copy coefficients.
            for (var i = 0; i < superposition.coefficients.Length; i++)
            {
                this.coefficients[i] = superposition.coefficients[i];
                this.state = superposition.state;
            }
            CalculateEntropy();
        }

        public void MakeAllFalseBesideOnePattern(int patternIndex)
        {
            coefficients = new bool[patternFromSample.Count];
            coefficients[patternIndex] = true;
            
            // Set entropy to zero because pattern is collapsed.
            Entropy = 0;

            // Assign enum value of zero zero. 
            state = patternFromSample[patternIndex].MiniTile[0, 0];
        }

        // If another superposition has false value - set also false to this one
        public void OverlayWithAnother(Superposition superpositionToOVerlay)
        {
            for (int i = 0; i < coefficients.Length; i++)
            {
                if (coefficients[i] == true && superpositionToOVerlay.coefficients[i] == false)
                {
                    coefficients[i] = false;
                }
            }

            CalculateEntropy();
        }

        // Check the entropy after overlaying without changing the values
        public int CheckPossiblePatternsCount (Superposition superpositionToOverlay)
        {
            var fakeCoefficients = new bool[coefficients.Length];
            int patternCount = 0;

            for (int i = 0; i < coefficients.Length; i++)
            {
                if (coefficients[i] == true && superpositionToOverlay.coefficients[i] == true)
                {
                    fakeCoefficients[i] = true;
                    patternCount++;
                }
            }

            return patternCount;
        }


        public void CalculateEntropy()
        {
            double ent = 0;
            int patternCount = 0;

            for (int i = 0; i < coefficients.Length; i++)
            {
                if (coefficients[i])
                {
                    ent += ((patternFromSample[i].Weight) * Math.Log(patternFromSample[i].Weight));
                    patternCount++;
                }
            }
            double noise = AddNoise();
            if (patternCount == 0) { Entropy = -1; }
            else if (patternCount == 1) { Entropy = 0; }
            else Entropy = ent + noise;
        }


        float GetSumWeight(List<Pattern> candidates)
        {
            float weightSum = 0;

            for (int i = 0; i < candidates.Count; i++) { weightSum += candidates[i].Weight; }

            return weightSum;
        }

        // Based on weights, if weights is 0.52 there is 52% chance to be picked
        public Pattern RouletteWheelSelections(List<Pattern> patternFromSample)
        {
            var candidates = GetPatternsFromSuperposition(patternFromSample);
            Pattern selected = null;

            float sum = GetSumWeight(candidates);
            var randomPoint = GetRandomNumber(0, sum);

            float currentSum = 0;

            if (candidates.Count == 0) throw new ArgumentException("No patterns left.");

            for (int i = 0; i < candidates.Count; i++)
            {
                currentSum += candidates[i].Weight;

                if (currentSum >= randomPoint)
                {
                    selected = candidates[i];
                    break;
                }
            }

            return selected;
        }

        public double AddNoise()
        {
            double noise = 1E-6 * random.NextDouble();
            return noise;
        }

        public double GetRandomNumber(double minimum, double maximum)
        {
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        // Get list of all patterns that are still possible candidates 
        List<Pattern> GetPatternsFromSuperposition(List<Pattern> patternFromSample)
        {
            var patternsFromSuperposition = new List<Pattern>(patternFromSample);

            int j = 0;

            for (int i = 0; i < patternFromSample.Count; i++)
            {
                if (coefficients[i])
                {
                    j++;
                }
                else if (!coefficients[i])
                {
                    patternsFromSuperposition.RemoveAt(j);
                    //j--;
                }
            }

            return patternsFromSuperposition;
        }

        public override string ToString()
        {
            return Entropy.ToString();
        }

        public object Clone()
        {
            return new Superposition(this);
        }
    }
}