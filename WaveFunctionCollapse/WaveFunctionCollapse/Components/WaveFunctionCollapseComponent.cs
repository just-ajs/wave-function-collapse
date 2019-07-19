using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Grasshopper.Kernel;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace WaveFunctionCollapse
{
    public partial class WaveFunctionCollapseComponent : GH_Component
    {
        public WaveFunctionCollapseComponent() : base("Observe and Collapse", "Observe and Collapse",
              "Observe pattern within given wave and populate it", "WFC", "Wave Fucntion Collapse")
        {
        }

        // INPUT
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new PatternFromSampleParam());
            pManager.AddPointParameter("Wave", "", "", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Backtrack", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Iterations", "", "", GH_ParamAccess.item);
        }

        // OUTPUT
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Number of rotated tiles", "Offsetes count", "", GH_ParamAccess.item);
            pManager.AddParameter(new PatternHistoryParam());
            pManager.AddNumberParameter("Pattern Count", "", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Average contradiction value", "", "", GH_ParamAccess.item);
        }

        // INSIDE
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_PatternsFromSample gh_patterns = new GH_PatternsFromSample();
            DA.GetData<GH_PatternsFromSample>(0, ref gh_patterns);

            List<Point3d> wavePoints = new List<Point3d>();
            DA.GetDataList<Point3d>(1, wavePoints);
            
            bool backtrack = false;
            DA.GetData<bool>(2, ref backtrack);

            double iterations = 0;
            DA.GetData<double>(3, ref iterations);

            var patterns = gh_patterns.Value.Patterns;
            var weights = gh_patterns.Value.TilesWeights;
            var N = gh_patterns.Value.N;

            int width = Utils.GetNumberofPointsInOneDimension(wavePoints[0].X, wavePoints[wavePoints.Count - 1].X);
            int height = Utils.GetNumberofPointsInOneDimension(wavePoints[0].Y, wavePoints[wavePoints.Count - 1].Y);

            // RUN WAVEFUNCION COLLAPSE
            var wfc = new WaveFunctionCollapseRunner();
            var history = wfc.Run(patterns, N, width, height, weights, backtrack, (int)iterations);
            var return_value = new GH_WaveCollapseHistory(history);

            var patternsOccurence = wfc.GetPatternCounts();
            var collapseAverage = wfc.GetAverageCollapseStep();

            if (true)
            {
                DA.SetData(0, patterns.Count);
                DA.SetData(1, return_value);
                DA.SetDataList(2, patternsOccurence);
                DA.SetData(3, collapseAverage);
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Check the inputs you idiot!");
            }
        }


        /// Provides an Icon for every component that will be visible in the User Interface. Icons need to be 24x24 pixels.
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return WaveFunctionCollapse.Properties.Resources.Icon;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("3aac7ab0-722c-4eb0-b65a-e53640525e4b"); }

        }

        public object MyAssemblyName { get; private set; }
    }

    struct IntPoint3d
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public override string ToString()
        {
            return $"{X}, {Y}, {Z}";
        }
    }

}
