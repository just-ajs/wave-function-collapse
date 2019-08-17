using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WaveFunctionCollapse
{
    public class Distancebetweenimages : GH_Component
    {

        public Distancebetweenimages()
          : base("Distancebetweenimages", "",  "",   "WFC", "Evaluate")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Original", "", "", GH_ParamAccess.item);
            pManager.AddTextParameter("GAN Image", "", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Entropy Image", "", "", GH_ParamAccess.item);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string originalFilepath = "";
            DA.GetData(0, ref originalFilepath);

            string GANmethodFilePath = "";
            DA.GetData(0, ref GANmethodFilePath);

            string EntropyMethodFilePath = "";
            DA.GetData(0, ref EntropyMethodFilePath);

            Bitmap original = (Bitmap)Image.FromFile(originalFilepath);
            Bitmap ganMethod = (Bitmap)Image.FromFile(GANmethodFilePath);
            Bitmap entropyMethod = (Bitmap)Image.FromFile(EntropyMethodFilePath);


        }

        List<float> ImageToHSBList(Bitmap image)
        {
            Color[,] img_colors = new Color[image.Width, image.Height];

            List<float> hsbList = new List<float>();

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    img_colors[i,j] = image.GetPixel(i, j);
                    hsbList.Add(image.GetPixel(i, j).GetHue());
                    hsbList.Add(image.GetPixel(i, j).GetSaturation());
                    hsbList.Add(image.GetPixel(i, j).GetBrightness());
                    
                }
            }

            return hsbList;
        }

        double EuclideanDistanceBetweenImages (List<float> img01, List<float> img02)
        {
            double dist = 0;

            for (int i = 0; i < img01.Count; i++)
            {
                dist += Math.Pow((img01[i] + img02[i]), 2);
            }

            dist = Math.Sqrt(dist);

            return dist;
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
            get { return new Guid("65599969-c7c9-44b6-b517-cf0d68ed5689"); }
        }
    }
}