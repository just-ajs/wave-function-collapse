using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveFunctionCollapse
{
    public class GH_WaveCollapseHistory : GH_Goo<IWaveCollapseHistory>
    {
        public GH_WaveCollapseHistory() { Value = new WaveCollapseHistory(); }
        public GH_WaveCollapseHistory(IWaveCollapseHistory native) { Value = native; }

        public override bool IsValid => true;

        public override string TypeName => "WaveCollapseHistory";

        public override string TypeDescription => "Wave Collapse Algorithm history object";

        public override bool CastFrom(object source)
        {
            return base.CastFrom(source);
        }

        public override IGH_Goo Duplicate()
        {
            return new GH_WaveCollapseHistory();
        }

        public override string ToString()
        {
            return $"History object containing {Value.Elements.Count} elements";
        }
    }


    public class GH_PatternsFromSample : GH_Goo<PatternFromSampleElement>
    {
        public GH_PatternsFromSample() { Value = new PatternFromSampleElement(); }
        public GH_PatternsFromSample(PatternFromSampleElement native) { Value = native; }

        public override bool IsValid => true;

        public override string TypeName => "IPatternFromSample";

        public override string TypeDescription => "Library of patterns from sample";

        public override bool CastFrom(object source)
        {
            return base.CastFrom(source);
        }

        public override IGH_Goo Duplicate()
        {
            return new GH_PatternsFromSample();
        }

        public override string ToString()
        {
            return $"History object containing {Value.Patterns.Count} elements";
        }
    }

 

    
}
