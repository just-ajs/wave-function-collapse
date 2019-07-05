using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveFunctionCollapse
{
    // store all steps of wave funtion observation
    public interface IWaveCollapseOutputs
    {
        IList<WaveCollapseHistoryElement> Elements { get; }
        void AddToList(WaveCollapseHistoryElement element);
    }


    public class WaveCollapseOutputs : IWaveCollapseOutputs
    {
        List<WaveCollapseHistoryElement> _elements;

        public WaveCollapseOutputs()
        {
            _elements = new List<WaveCollapseHistoryElement>();
        }

        public IList<WaveCollapseHistoryElement> Elements => _elements;

        public void AddToList(WaveCollapseHistoryElement element)
        {
            _elements.Add(element);
        }

        public void Clear()
        {
            _elements.Clear();
        }
    }
}
