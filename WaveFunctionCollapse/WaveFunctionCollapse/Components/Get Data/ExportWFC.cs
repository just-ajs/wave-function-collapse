using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WaveFunctionCollapse
{
    public class ExportWFC : GH_Component
    {
        public ExportWFC()
          : base("Export wfc", "Export wfc",
              "Export wfc", "WFC", "Data Analysis")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new PatternResultsParam(), "Wave Function Dataset", "WFC", "Wave Function Collapse with history", GH_ParamAccess.item);
            pManager.AddParameter(new PatternFromSampleParam(), "PatternFromSample", "", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Series name", "", "", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        // Picture data
        static int biteSize;
        private static byte[] _imageBuffer;
        static int width, height;


        int[] white = new int[3] { 255, 255, 255 };
        int[] green = new int[3] { 49, 58, 179 };
        int[] blue = new int[3] { 86, 206, 27 };

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_WaveCollapseResults waveCollapseDataset = new GH_WaveCollapseResults();
            DA.GetData<GH_WaveCollapseResults>(0, ref waveCollapseDataset);

            GH_PatternsFromSample gh_patterns = new GH_PatternsFromSample();
            DA.GetData<GH_PatternsFromSample>(1, ref gh_patterns);

            string series = "";
            DA.GetData<string>(2, ref series);

            var waveElements = Utils.GetObservedWave(waveCollapseDataset);
            var patterns = Utils.GetPatternsFromSample(gh_patterns);

            // Assign generic values for every image
            var sampleSuperpositions = waveElements[0].Superpositions;

            // Get picture width and height
            width = sampleSuperpositions.GetLength(0);
            height = sampleSuperpositions.GetLength(1);

            // Allocate memory for picture
            biteSize = (int)((width * 4) * height);
            _imageBuffer = new byte[biteSize];

            // Add image from series
            for (int i = 0; i < waveElements.Count; i++)
            {
                var superpositions = waveElements[i].Superpositions;

                // Get values for pixels
                var pixelsCodes = Utils.GetImageValuesFromWave(superpositions);

                // Change to rgb
                var pictureRGB = Utils.CellsValuesToRGBColours(pixelsCodes, white, blue, green);

                // Plot picture from rgb
                Utils.PlotPixelsFromRGB(pictureRGB, _imageBuffer);

                // Save ti file
                Utils.SaveToPicture(pictureRGB, i, series, _imageBuffer);
            }
            

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
            get { return new Guid("5e2170c1-144d-491e-a975-ca0dcba9d045"); }
        }
    }
}