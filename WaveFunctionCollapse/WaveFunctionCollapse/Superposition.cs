using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace WaveFunctionCollapse
{
    public class Superposition
    {
        public bool[] coefficients;


        public Superposition(List<Pattern> patternFromSample)
        {
            coefficients = new bool[patternFromSample.Count];
            for (int i = 0; i < coefficients.Length; i++) { coefficients[i] = true; }
        }


        public void MakeAllFalseBesideOnePattern (int patternIndex)
        {
            for (int i = 0; i < coefficients.Length; i++) { coefficients[i] = false; }

            coefficients[patternIndex] = true;
        }

        public void OverlayWithAnother (Superposition superpositionToOVerlay)
        {
            for (int i = 0; i < coefficients.Length; i++)
            {
                if (coefficients[i] == true && superpositionToOVerlay.coefficients[i] == false) coefficients[i] = false;
            }
        }

        public int Entropy {
            get
            {
                var ent = 0;
                for (int i = 0; i < coefficients.Length; i++)
                {
                    if (coefficients[i]) ent++;
                }
                return ent - 1;
            }
        }


        float GetSumWeight(List<Pattern> candidates)
        {
            float weightSum = 0;

            for (int i = 0; i < candidates.Count; i++) { weightSum += candidates[i].Weight; }

            return weightSum;
        }

        Pattern rouletteWheelSelections(List<Pattern> patternFromSample)
        {
            var candidates = GetPatternsFromSuperposition(patternFromSample);
            Pattern selected = null;

            float sum = GetSumWeight(candidates);
            var randomPoint = GetRandomNumber(0, sum);

            float currentSum = 0;

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

        public double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        public Pattern GetRandomPatternWithHighWeight (List<Pattern> patternsFromSample)
        {
            Pattern selectedFromHighWeights = rouletteWheelSelections(patternsFromSample);
            //int indexOfSelectedPattern = findPatternIndex(selectedFromHighWeights, patternsFromSample);

            return selectedFromHighWeights;

        }

        int findPatternIndex (Pattern pattern, List<Pattern> a)
        {
            int index = 0;
            for (int i = 0; i < a.Count; i++) {  if (Pattern.ReferenceEquals(pattern, a[i])) index = i; }
            return index;
        }

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
    }
}