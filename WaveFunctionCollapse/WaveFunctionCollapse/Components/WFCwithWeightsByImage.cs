using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WaveFunctionCollapse
{
    public class WFCwithWeightsByImage : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the WFCwithWeightsByImage class.
        /// </summary>
        public WFCwithWeightsByImage()
          : base("WFCwithWeightsByImage", "Nickname", "Description",
              "Category", "Subcategory")
        {

        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new PatternFromSampleParam());
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Get patterns from sample
            GH_PatternsFromSample gh_patterns = new GH_PatternsFromSample();
            DA.GetData<GH_PatternsFromSample>(0, ref gh_patterns);

            pManager.AddPointParameter("Wave", "", "", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Backtrack", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Iterations", "", "", GH_ParamAccess.item);
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
            get { return new Guid("2e2ae420-841f-4218-9603-78b2b35b5943"); }
        }
    }
}