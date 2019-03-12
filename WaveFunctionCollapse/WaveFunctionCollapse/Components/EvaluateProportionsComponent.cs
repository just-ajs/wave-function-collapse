using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace WaveFunctionCollapse
{
    public class EvaluateProportionsComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EvaluateProportions class.
        /// </summary>
        public EvaluateProportionsComponent()
          : base("EvaluateProportions", "Nickname","This component will display proportions of occurence of tiles types", "TERM2", "WFC_WIP")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("wave: half surfaces", "", "", GH_ParamAccess.list);
            pManager.AddSurfaceParameter("wave: full surfaces", "", "", GH_ParamAccess.list);
            pManager.AddSurfaceParameter("wave: empty surfaces", "", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("X", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Y", "", "", GH_ParamAccess.item);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddSurfaceParameter("", "", "", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("", "", "", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("", "", "", GH_ParamAccess.item);
            pManager.AddTextParameter("", "", "", GH_ParamAccess.item);
            pManager.AddTextParameter("", "", "", GH_ParamAccess.item);
            pManager.AddTextParameter("", "", "", GH_ParamAccess.item);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Brep> surfaceType01 = new List<Brep>();
            DA.GetDataList<Brep>(0, surfaceType01);

            List<Brep> surfaceType02 = new List<Brep>();
            DA.GetDataList<Brep>(1, surfaceType02);

            List<Brep> surfaceType03 = new List<Brep>();
            DA.GetDataList<Brep>(2, surfaceType03);

            double x = 0;
            DA.GetData<double>(3, ref x);

            double y = 0;
            DA.GetData<double>(4, ref y);

            int sum = GetSum(surfaceType01, surfaceType02, surfaceType03);

            var height01 = GetObjectVisualHeight(surfaceType01, sum) * 40;
            var height02 = GetObjectVisualHeight(surfaceType02, sum) * 40;
            var height03 = GetObjectVisualHeight(surfaceType03, sum) * 40;

            var points01 = GetSurfacePoints(height01, x , y);
            var points02 = GetSurfacePoints(height02, x + 2, y);
            var points03 = GetSurfacePoints(height03, x + 4, y);

            string text01 = surfaceType01.Count.ToString();
            string text02 = surfaceType02.Count.ToString();
            string text03 = surfaceType03.Count.ToString();

            var half = NurbsSurface.CreateFromCorners(points01[0], points01[1], points01[3], points01[2]);
            var full = NurbsSurface.CreateFromCorners(points02[0], points02[1], points02[3], points02[2]);
            var empty = NurbsSurface.CreateFromCorners(points03[0], points03[1], points03[3], points03[2]);

           // var t = new TextObject();


            DA.SetData(0, half);
            DA.SetData(1, full);
            DA.SetData(2, empty);
            DA.SetData(3, text01);
            DA.SetData(4, text02);
            DA.SetData(5, text03);
        }


        List<Point3d> GetSurfacePoints (float height, double xLoc, double yLoc)
        {
            var surfacePoints = new List<Point3d>();

            surfacePoints.Add(new Point3d(xLoc - 0.3, yLoc, 0));
            surfacePoints.Add(new Point3d(xLoc + 0.3, yLoc, 0));
            surfacePoints.Add(new Point3d(xLoc - 0.3, yLoc + height, 0));
            surfacePoints.Add(new Point3d(xLoc + 0.3, yLoc + height, 0));

            return surfacePoints;
        }

        int GetSum(List<Brep> a, List<Brep> b, List<Brep> c)
        {
            return a.Count + b.Count + c.Count;
        }

        float GetObjectVisualHeight(List<Brep> a, int sum)
        {
            return (a.Count / (float)sum);
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
            get { return new Guid("af29a922-960e-4cd3-99cc-a2de7bb6ee46"); }
        }
    }
}