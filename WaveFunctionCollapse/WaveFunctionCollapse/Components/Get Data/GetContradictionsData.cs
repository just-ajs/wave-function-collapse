using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WaveFunctionCollapse
{
    public class GetContradictionsData : GH_Component
    {
        public GetContradictionsData()
          : base("GetContradictionsData", "Contradictions count",
              "This component counts how many times wave is not solve and gets contradiction",
              "WFC", "Data Analysis")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new PatternFromSampleParam());
            pManager.AddNumberParameter("Dataset Size", "", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Backtrack", "", "", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Successful runs", "", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Surface size", "", "", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_PatternsFromSample gh_patterns = new GH_PatternsFromSample();
            DA.GetData<GH_PatternsFromSample>(0, ref gh_patterns);

            double iterations = 0;
            DA.GetData<double>(1, ref iterations);

            bool backtrack = false;
            DA.GetData<bool>(2, ref backtrack);

            var patterns = gh_patterns.Value.Patterns;
            var weights = gh_patterns.Value.TilesWeights;
            var N = gh_patterns.Value.N;

            List<WaveCollapseHistoryElement> outputs = new List<WaveCollapseHistoryElement>();

            var return_value = new GH_WaveCollapseResults();

            List<int> successfulRuns = new List<int>();
            List<int> outputSizes = new List<int>();

            // This function run the WFC for iteration number of times, for different tile sizes. 
            // Component returns how many times for iteration number of sizes it was successful

            for (int size = 4; size < 13; size += 2)
            {
                outputSizes.Add(size);

                int successfulIterations = 0;

                for (int i = 0; i < (int)iterations; i++)
                {
                    var wfc = new WaveFunctionCollapseRunner();
                    var history = wfc.Run(patterns, N, size, size, weights, backtrack);

                    if (history.Elements.Count != 0)
                    {
                        successfulIterations++;
                    }
                }

                successfulRuns.Add(successfulIterations);
            }

            DA.SetDataList(0, successfulRuns);
            DA.SetDataList(1, outputSizes);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("0aca8065-932f-4680-90d9-903185ef452c"); }
        }
    }
}