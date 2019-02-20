using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace WaveFunctionCollapse
{
    internal class Superposition
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
                return ent;
            }
        }

        List<Pattern> GetPatternsFromSuperposition(List<Pattern> patternFromSample)
        {
            var patternsFromSuperposition = new List<Pattern>(patternFromSample);

            for (int i = 0; i < patternFromSample.Count; i++)
            {
                if (!coefficients[i]) patternsFromSuperposition.RemoveAt(i);
            }

            return patternsFromSuperposition;
        }
    }
}