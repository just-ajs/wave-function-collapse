using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WaveFunctionCollapse
{
    public class GetWaveDimension : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetWaveDimension class.
        /// </summary>
        public GetWaveDimension()
          : base("GetWaveDimension", "Nickname", "Description",
              "WFC", "Data Analysis")
        {
        }

 
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Wave", "", "", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Width", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "", "", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> wavePoints = new List<Point3d>();
            DA.GetDataList<Point3d>(0, wavePoints);

            int width = Utils.GetNumberofPointsInOneDimension(wavePoints[0].X, wavePoints[wavePoints.Count - 1].X);
            int height = Utils.GetNumberofPointsInOneDimension(wavePoints[0].Y, wavePoints[wavePoints.Count - 1].Y);

            DA.SetData(0, width);
            DA.SetData(1, height);
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
            get { return new Guid("a96a06f7-9b93-491a-874e-c806c08d0ff1"); }
        }
    }
}