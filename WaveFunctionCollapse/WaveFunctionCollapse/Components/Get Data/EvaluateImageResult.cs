using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WaveFunctionCollapse
{
    public class EvaluateImageResult : GH_Component
    {
          public EvaluateImageResult()
          : base("EvaluateImageResult", "",  "", "WFC", "Data Analysis")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new PatternHistoryParam(), "Wave Function Collapsed", "WFC", "Wave Function Collapse with history", GH_ParamAccess.item);

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Dark cell success rate", "", "Percentage of dark cells in the dark image area", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_WaveCollapseHistory waveCollapseHistory = new GH_WaveCollapseHistory();
            DA.GetData<GH_WaveCollapseHistory>(0, ref waveCollapseHistory);

            var lastIteration = waveCollapseHistory.Value.Elements[waveCollapseHistory.Value.Elements.Count - 1];

            var image = Utils.generateRandomImage(lastIteration.Superpositions.GetLength(0), lastIteration.Superpositions.GetLength(1));

            float successRate = Utils.CountSuccessfullCellsInImageArea(image, lastIteration);

            DA.SetData(0, successRate);
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

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6733a31d-22af-4407-9353-77bb134abfe8"); }
        }
    }
}