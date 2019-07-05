using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace WaveFunctionCollapse
{
    public class PatternHistoryParam : GH_PersistentParam<GH_WaveCollapseHistory>
    {
        public PatternHistoryParam() : base("Pattern history parameter", "PatternHistoryParam", "This is a pattern history parameter", "WFC", "Parameters") { }

        public override Guid ComponentGuid
        {
            get { return new Guid("2D01254D-A488-434C-AC23-0ECCB6EACC7C"); }
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GH_WaveCollapseHistory> values)
        {
            values = new List<GH_WaveCollapseHistory>();
            return GH_GetterResult.success;
        }

        protected override GH_GetterResult Prompt_Singular(ref GH_WaveCollapseHistory value)
        {
            value = new GH_WaveCollapseHistory();
            return GH_GetterResult.success;
        }
    }

    public class PatternResultsParam : GH_PersistentParam<GH_WaveCollapseResults>
    {
        public PatternResultsParam() : base("Pattern history parameter", "PatternHistoryParam", "This is a pattern history parameter", "WFC", "Parameters") { }

        public override Guid ComponentGuid
        {
            get { return new Guid("DF2E27C1-F816-49FE-8B24-F99F71A5EF5F"); }
       
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GH_WaveCollapseResults> values)
        {
            values = new List<GH_WaveCollapseResults>();
            return GH_GetterResult.success;
        }

        protected override GH_GetterResult Prompt_Singular(ref GH_WaveCollapseResults value)
        {
            value = new GH_WaveCollapseResults();
            return GH_GetterResult.success;
        }
    }

    public class PatternFromSampleParam : GH_PersistentParam<GH_PatternsFromSample>
    {
        public PatternFromSampleParam() : base("Pattern from sample parameter", "PatternFromSampleParam", "This is a pattern from sample parameter", "WFC", "Parameters") { }

        public override Guid ComponentGuid
        {
            get { return new Guid("92E5F29F-A28F-4D66-8F01-CA7E9701E7CA"); }
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GH_PatternsFromSample> values)
        {
            values = new List<GH_PatternsFromSample>();
            return GH_GetterResult.success;
        }

        protected override GH_GetterResult Prompt_Singular(ref GH_PatternsFromSample value)
        {
            value = new GH_PatternsFromSample();
            return GH_GetterResult.success;
        }
    }


}