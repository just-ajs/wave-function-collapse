using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WaveFunctionCollapse.Components
{
    public class LoadImage : GH_Component
    {

        public LoadImage()
          : base("LoadImage", "", "",  "WFC", "Data Analysis")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Image path", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "", "", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new InputImageParam());
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string filepath = "";
            DA.GetData(0, ref filepath);

            double width = 0;
            DA.GetData(1, ref width);

            double height = 0;
            DA.GetData(2, ref height);

            Bitmap original = (Bitmap)Image.FromFile(filepath);
            Bitmap resized = new Bitmap(original, new Size((int)width, (int)height));

            var imageB = GetImageBrigthness(resized);

            var ouputimg = new InputImage(imageB);
            var img = new GH_Image(ouputimg);

            DA.SetData(0, img);
        }

        Color[,] BitmapToColorArray (Bitmap img)
        {
            Color[,] imgColors = new Color[img.Width, img.Height];

            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    imgColors[i, j] = img.GetPixel(i, j);
                }
            }

            return imgColors;
        }

        double[,] GetImageBrigthness (Bitmap img)
        {
            double[,] imgBrightness = new double[img.Width, img.Height];

            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    var pix = img.GetPixel(i, img.Height - 1 - j);
                    imgBrightness[i,j] = pix.GetBrightness(); ;
                }
            }

            return imgBrightness;
        }
    

        public Image resizeImage(int newWidth, int newHeight, string stPhotoPath)
        {
            Image imgPhoto = Image.FromFile(stPhotoPath);

            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;

            //Consider vertical pics
            if (sourceWidth < sourceHeight)
            {
                int buff = newWidth;

                newWidth = newHeight;
                newHeight = buff;
            }

            int sourceX = 0, sourceY = 0, destX = 0, destY = 0;
            float nPercent = 0, nPercentW = 0, nPercentH = 0;

            nPercentW = ((float)newWidth / (float)sourceWidth);
            nPercentH = ((float)newHeight / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((newWidth -
                          (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((newHeight -
                          (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);


            Bitmap bmPhoto = new Bitmap(newWidth, newHeight,
                          PixelFormat.Format24bppRgb);

            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                         imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.Black);
            grPhoto.InterpolationMode =
                System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            imgPhoto.Dispose();
            return bmPhoto;
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
            get { return new Guid("89725ef9-f98c-4f46-9ba0-3d161962df96"); }
        }
    }
}