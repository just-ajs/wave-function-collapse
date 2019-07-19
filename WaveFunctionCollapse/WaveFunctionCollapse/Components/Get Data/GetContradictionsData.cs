using System;
using System.Collections.Generic;
using System.Text;
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

        // This function run the WFC for iteration number of times, for different tile sizes. 
        // Component returns how many times for iteration number of sizes it was successful
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


            var sb = getContradictionData(outputSizes, (int)iterations, patterns, N, weights, ref successfulRuns);

            string filePath = @"R:\csv\asd2_14_26.csv";

            System.IO.File.WriteAllText(filePath, sb.ToString());

            DA.SetDataList(0, successfulRuns);
            DA.SetDataList(1, outputSizes);
        }

        StringBuilder getContradictionData(List<int> outputSizes, int iterations, List<Pattern> patterns, 
            int N, float[] weights, ref List<int> successfulRuns)
        {
            // Export to .csv file
            StringBuilder sb = new StringBuilder();

            int successfulIterations_nobacktrack = 0;
            int successfulIterations_backtrack = 0;

            // Titles
            sb.AppendFormat("{0}, {1}, {2}", "Dimension", "No Backtracking", "Backtracking");
            sb.AppendLine();

            for (int size = 14; size < 26; size += 2)
            {
                outputSizes.Add(size);

                successfulIterations_nobacktrack = 0;
                successfulIterations_backtrack = 0;

                for (int i = 0; i < (int)iterations; i++)
                {
                    // Count all successful iterations without backtracking.
                    var wfc_nobacktrack = new WaveFunctionCollapseRunner();
                    var history_nobacktrack = wfc_nobacktrack.Run(patterns, N, size, size, weights, false, 1);

                    if (history_nobacktrack.Elements.Count != 0)
                    {
                        successfulIterations_nobacktrack++;
                    }

                    // Count all successfull iterations with backtracking. 
                    var wfc_backtrack = new WaveFunctionCollapseRunner();
                    var history_backtrack = wfc_backtrack.Run(patterns, N, size, size, weights, true, 1);

                    if (history_backtrack.Elements.Count != 0)
                    {
                        successfulIterations_backtrack++;
                    }
                }
                sb.AppendFormat("{0}, {1}, {2}", size, successfulIterations_nobacktrack, successfulIterations_backtrack);
                sb.AppendLine();
                successfulRuns.Add(successfulIterations_nobacktrack);
                successfulRuns.Add(successfulIterations_backtrack);

            }

            return sb;
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("0aca8065-932f-4680-90d9-903185ef452c"); }
        }
    }
}