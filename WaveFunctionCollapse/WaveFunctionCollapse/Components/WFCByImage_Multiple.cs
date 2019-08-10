using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WaveFunctionCollapse
{
    public class WFCByImage_Multiple : GH_Component
    {
        public WFCByImage_Multiple()
          : base("WFCByImage_Multiple", "",  "", "WFC", "Data Analysis")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // Wave function collapse inputs. 
            pManager.AddParameter(new PatternFromSampleParam());
            pManager.AddPointParameter("Wave", "", "", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Backtrack", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Iterations", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Dataset Size", "", "", GH_ParamAccess.item);


            // Image. 
            pManager.AddNumberParameter("Image", "", "", GH_ParamAccess.list);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new PatternResultsParam());

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

            double dataSize = 0;
            DA.GetData<double>(4, ref dataSize);

            // Get image data.
            List<double> rawImage = new List<double>();
            DA.GetDataList(5, rawImage);



            var patterns = gh_patterns.Value.Patterns;
            var weights = gh_patterns.Value.TilesWeights;
            var N = gh_patterns.Value.N;

            // Get width and height based on 2d array of points
            int width = Utils.GetNumberofPointsInOneDimension(wavePoints[0].X, wavePoints[wavePoints.Count - 1].X);
            int height = Utils.GetNumberofPointsInOneDimension(wavePoints[0].Y, wavePoints[wavePoints.Count - 1].Y);

            // Prepare image data. 
            //var image = convertImageListToArray(rawImage, width, height);
            var image = Utils.generateRandomImage(width, height);

            List<WaveCollapseHistoryElement> outputs = new List<WaveCollapseHistoryElement>();

            var return_value = new GH_WaveCollapseResults();

            for (int i = 0; i < (int)dataSize; i++)
            {
                // RUN WAVEFUNCION COLLAPSE
                var wfc = new WaveFunctionCollapseRunner();
                var history = wfc.Run(patterns, N, width, height, weights, (int)iterations, backtrack, image);

                if (history.Elements.Count == 0) continue;

                // Get last element of the wfc run
                var historyEndElement = history.Elements[history.Elements.Count - 1];

                outputs.Add(historyEndElement);
                return_value.Value.AddToList(historyEndElement);
            }

            if (true)
            {
                DA.SetData(0, return_value);
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Check the inputs you idiot!");
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

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("bb38f901-71fd-4f13-a8f0-8f3d9a0f1721"); }
        }
    }
}