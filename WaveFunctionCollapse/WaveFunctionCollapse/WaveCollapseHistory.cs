using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveFunctionCollapse
{
    // store all steps of wave funtion observation
    public interface IWaveCollapseHistory
    {
        IList<WaveCollapseHistoryElement> Elements { get; }
        void AddTimeFrame(WaveCollapseHistoryElement element);
    }

    public class WaveCollapseHistoryElement
    {
        public List<Point3d> HalfTile { get; }
        public List<Point3d> FullTile { get; }
        public List<Point3d> Empty { get; }
        public Superposition[,] Superpositions;
        public TileSuperposition[,] Uncollapsed;
        public int[] PatternOccurence;
        public double[,] Entropies;


        public WaveCollapseHistoryElement(List<Point3d> halfTile, List<Point3d> fullTile, 
            List<Point3d> empty, Superposition[,] superpositions, TileSuperposition[,] uncollapsed, int[] patternOccurence,
            double[,] entropies)
        {
            HalfTile = halfTile;
            FullTile = fullTile;
            Empty = empty;
            Superpositions = superpositions;
            Uncollapsed = uncollapsed;
            PatternOccurence = patternOccurence;
            Entropies = entropies.Clone() as double[,];
        }
    }

    public class WaveCollapseHistory : IWaveCollapseHistory
    {
        List<WaveCollapseHistoryElement> _elements;

        public WaveCollapseHistory()
        {
            _elements = new List<WaveCollapseHistoryElement>();
        }

        public IList<WaveCollapseHistoryElement> Elements => _elements;

        public void AddTimeFrame(WaveCollapseHistoryElement element)
        {
            _elements.Add(element);
        }

        public void Clear()
        {
            _elements.Clear();
        }
    }
}
