using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WaveFunctionCollapse
{
    public class GetColor : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetColor class.
        /// </summary>
        public GetColor()
          : base("GetColor", "Nickname",
              "Description",
              "WFC", "other")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Image path", "", "", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddColourParameter("Image color", "", "", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
           string filepath = "";
           DA.GetData(0, ref filepath);

           Bitmap original = (Bitmap)Image.FromFile(filepath);

            var pix = original.GetPixel(original.Width / 2, original.Height / 2);


            DA.SetData(0, pix);
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
            get { return new Guid("c7aeb80a-0565-4945-baaa-8b59dbe63961"); }
        }
    }
}