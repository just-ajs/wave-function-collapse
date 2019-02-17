using System.Collections.Generic;

namespace WaveFunctionCollapse
{
    internal class Wave
    {
        private List<Superposition> superpositions;

        public Wave(List<Superposition> superpositions)
        {
            this.superpositions = superpositions;
        }
    }
}