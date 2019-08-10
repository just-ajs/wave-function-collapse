using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WaveFunctionCollapse.Components
{
    public class SaveSuccessRateToCSV : GH_Component
    {
        public SaveSuccessRateToCSV()
          : base("SaveSuccessRateToCSV", "",  "",  "WFC", "Data Analysis")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Dark cell success rate", "", "Percentage of dark cells in the dark image area", GH_ParamAccess.item);
            pManager.AddNumberParameter("Success List", "", "", GH_ParamAccess.list);
            pManager.AddTextParameter("Method Name", "", "", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double averageSuccess = 0.0f;
            DA.GetData<double>(0, ref averageSuccess);

            List<double> list = new List<double>();
            DA.GetDataList<double>(1, list);

            string name = "";
            DA.GetData<string>(2, ref name);

            var standardDev = SampleStandardDeviation(list);

            SaveToCSV(averageSuccess, list, standardDev, name);

        }

        void SaveToCSV(double average, List<double> list, double standardDev, string fileName)
        {
            StringBuilder sb = new StringBuilder();
            // Average
            sb.AppendFormat("{0}", fileName + ": average");
            sb.AppendLine();
            sb.AppendFormat("{0}", average);
            sb.AppendLine();

            // Standard deviation
            sb.AppendFormat("{0}", fileName + ": standard dev");
            sb.AppendLine();
            sb.AppendFormat("{0}", standardDev);
            sb.AppendLine();

            // List of values
            sb.AppendFormat("{0}", fileName + ": values");
            sb.AppendLine();

            for (int i = 0; i < list.Count; i++)
            {
                sb.AppendFormat("{0}", list[i]);
                sb.AppendLine();
            }

            string filePath = @"R:\csv\successrate\" + fileName + ".csv";
            System.IO.File.WriteAllText(filePath, sb.ToString());

        }


        static double SampleStandardDeviation(List<double> numberSet)
        {
            double mean = numberSet.Sum() / numberSet.Count;

            return Math.Sqrt(numberSet.Sum(x => Math.Pow(x - mean, 2)) / (numberSet.Count - 1));
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
            get { return new Guid("f12b03e6-6bd5-48c8-9a93-9851e1b4f6e6"); }
        }
    }
}