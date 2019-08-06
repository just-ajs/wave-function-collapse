using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WaveFunctionCollapse
{
    public class WFCbyImage : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the WFCbyImage class.
        /// </summary>
        public WFCbyImage()
          : base("WFCbyImage", "Nickname", "Description",
              "WFC", "Data Analysis")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // Wave function collapse inputs. 
            pManager.AddParameter(new PatternFromSampleParam());
            pManager.AddPointParameter("Wave", "", "", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Backtrack", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Iterations", "", "", GH_ParamAccess.item);

            // Image. 
            pManager.AddNumberParameter("Image", "", "", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new PatternHistoryParam());
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Get wave function collapse data
            GH_PatternsFromSample gh_patterns = new GH_PatternsFromSample();
            DA.GetData<GH_PatternsFromSample>(0, ref gh_patterns);

            List<Point3d> wavePoints = new List<Point3d>();
            DA.GetDataList<Point3d>(1, wavePoints);

            bool backtrack = false;
            DA.GetData<bool>(2, ref backtrack);

            double iterations = 0;
            DA.GetData<double>(3, ref iterations);

            // Get image data.
            List<double> rawImage = new List<double>();
            DA.GetDataList(4, rawImage);
            
            // Extract parameters to run Wave Function Collapse.
            var patterns = gh_patterns.Value.Patterns;
            var weights = gh_patterns.Value.TilesWeights;
            var N = gh_patterns.Value.N;

            int width = Utils.GetNumberofPointsInOneDimension(wavePoints[0].X, wavePoints[wavePoints.Count - 1].X);
            int height = Utils.GetNumberofPointsInOneDimension(wavePoints[0].Y, wavePoints[wavePoints.Count - 1].Y);

            // Prepare image data. 
            //var image = convertImageListToArray(rawImage, width, height);
            var image = generateImage(width, height);


            // Run Wave Function Collapse.
            var wfc = new WaveFunctionCollapseRunner();
            var history = wfc.Run(patterns, N, width, height, weights, (int)iterations, backtrack, image);
            var return_value = new GH_WaveCollapseHistory(history);

            

            DA.SetData(0, return_value);
        }

        double[,] generateImage(int width, int height)
        {
            double[,] convertedImage = new double[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if ((i < 5 && j < 5) || ( i > width - 5 && j > height - 5 ))
                    {
                        convertedImage[i, j] = 0;

                    }
                    else
                    {
                        convertedImage[i, j] = 1;
                    }
                }
            }
            return convertedImage;
        }

        double[,] convertImageListToArray(List<double> image, int width, int height)
        {
            double[,] convertedImage = new double[width, height];

            for (int i = 0; i < width; i ++)
            {
                for (int j = 0; j < height; j ++)
                {
                    convertedImage[i, j] = image[i*j + j];
                }
            }

            return convertedImage;
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
            get { return new Guid("d0778493-3bba-401d-baae-edb26b684c1b"); }
        }
    }
}