using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace WaveFunctionCollapse
{
    internal class Superposition
    {
        int entropy = 0;
        bool[] coefficients;

        public Superposition(List<Pattern> patternFromSample)
        {
            coefficients = new bool[patternFromSample.Count];
        }

        int GetEntropy()
        {
            entropy = 0;
            for (int i = 0; i < coefficients.Length; i++)
            {
                if (coefficients[i]) entropy++; 
            }
            return entropy;
        }
    }
}