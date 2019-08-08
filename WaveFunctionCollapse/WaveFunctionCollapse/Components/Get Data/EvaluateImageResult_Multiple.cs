using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WaveFunctionCollapse
{
    public class EvaluateImageResult_Multiple : GH_Component
    {

        public EvaluateImageResult_Multiple()
          : base("EvaluateImageResult_Multiple", "", "", "WFC", "Data Analysis")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new PatternResultsParam());
        }

  
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Dark cell success rate", "", "Percentage of dark cells in the dark image area", GH_ParamAccess.item);
            pManager.AddNumberParameter("Success List", "", "", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_WaveCollapseResults waveCollapseDataset = new GH_WaveCollapseResults();
            DA.GetData<GH_WaveCollapseResults>(0, ref waveCollapseDataset);

            var wfcList = waveCollapseDataset.Value.Elements;
            var image = Utils.generateRandomImage(wfcList[0].Superpositions.GetLength(0), wfcList[0].Superpositions.GetLength(1));

            float sumSuccessRate = 0.0f;
            float successProportion = 0.0f;

            List<float> succesRates = new List<float>();

            // Go through every wfc solution
            for (int i = 0; i < wfcList.Count; i++)
            {
                // Get last element of this solution
               var currentWFC = wfcList[i];
                var currentSuccessRate = Utils.CountSuccessfullCellsInImageArea(image, currentWFC);
                sumSuccessRate += currentSuccessRate;

                succesRates.Add(currentSuccessRate);
            }

            successProportion = sumSuccessRate / wfcList.Count;

            DA.SetData(0, successProportion);
            DA.SetDataList(1, succesRates);
            
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
            get { return new Guid("18cf485f-47d0-4a43-a01b-5ad01b2b0ff4"); }
        }
    }
}