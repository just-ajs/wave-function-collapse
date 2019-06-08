using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WaveFunctionCollapse
{
    public class MyComponent1 : GH_Component
    {
        public MyComponent1()
          : base("Simple Tiled Model", "Patterns Library", "This will create library of pattern from provided sample", "WFC", "Pattern From Sample")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Tile Type A", "Type A", "List of all centers of tiles of type A", GH_ParamAccess.list);
            pManager.AddPointParameter("Tile Type B", "Type B", "List of all centers of tiles of type B", GH_ParamAccess.list);
            pManager.AddPointParameter("Whole Area", "Tile Points", "List of all centers in tile design space", GH_ParamAccess.list);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new PatternFromSampleParam());
            pManager.AddPointParameter("Half tiles", "", "", GH_ParamAccess.list);
            pManager.AddPointParameter("Full tiles", "", "", GH_ParamAccess.list);
            pManager.AddPointParameter("Empty tiles", "", "", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
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
            get { return new Guid("cd65e59c-7175-4f2c-be3e-69998f36af47"); }
        }
    }
}