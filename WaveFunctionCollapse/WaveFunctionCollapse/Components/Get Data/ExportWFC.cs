using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WaveFunctionCollapse
{
    public class MyComponent2 : GH_Component
    {
        public MyComponent2()
          : base("Export wfc", "Export wfc",
              "Export wfc", "WFC", "Data Analysis")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new PatternResultsParam(), "Wave Function Dataset", "WFC", "Wave Function Collapse with history", GH_ParamAccess.item);
            pManager.AddParameter(new PatternFromSampleParam(), "PatternFromSample", "", "", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        // Picture data
        static int biteSize;
        private static byte[] _imageBuffer;
        static int width, height;

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_WaveCollapseResults waveCollapseDataset = new GH_WaveCollapseResults();
            DA.GetData<GH_WaveCollapseResults>(0, ref waveCollapseDataset);

            GH_PatternsFromSample gh_patterns = new GH_PatternsFromSample();
            DA.GetData<GH_PatternsFromSample>(1, ref gh_patterns);

            var waveElements = GetObservedWave(waveCollapseDataset);

            var patterns = getPatternsFromSample(gh_patterns);

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
                var pixelsCodes = getImageValuesFromWave(superpositions, patterns);

                // Save ti file
                SaveToPicture(pixelsCodes, i);
            }
            

        }

        int[,] getImageValuesFromWave(Superposition[,] superpositions, PatternFromSampleElement patterns)
        {
            int width = superpositions.GetLength(0);
            int height = superpositions.GetLength(1);
            int[,] values = new int[width, height];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    for (int k = 0; k < superpositions[i,j].coefficients.Length; k++)
                    {
                        if (superpositions[i, j].coefficients[k] == true)
                        {
                            if (patterns.Patterns[k].MiniTile[0, 0] == State.EMPTY)
                            {
                                values[i, j] = 0;
                            }
                            else if (patterns.Patterns[k].MiniTile[0, 0] == State.FULL_TILE)
                            {
                                values[i, j] = 1;
                            }
                            else if (patterns.Patterns[k].MiniTile[0, 0] == State.HALF_TILE)
                            {
                                values[i, j] = 2;
                            }
                        }
                    }
                }
            }

            return values;
        }


        PatternFromSampleElement getPatternsFromSample(GH_PatternsFromSample patternFromSample)
        {
            var patterns = patternFromSample.Value;
            return patterns;
        }

        List<WaveCollapseHistoryElement> GetObservedWave(GH_WaveCollapseResults results)
        {
            if (results.Value.Elements.Count == 0) return null;
            //int index = results.Value.Elements.Count - 1;
            var waveElements = results.Value.Elements.ToList();
            return waveElements;
        }

        void SaveToTextFile(Superposition[,] superpositions, int width, int height)
        {
            string[] indices = new string[superpositions.GetLength(0)* superpositions.GetLength(1)];

            for (int i = 0; i < superpositions.GetLength(0); i++)
            {
                for (int j = 0; j < superpositions.GetLength(1); j++)
                {
                    for (int k = 0; k < superpositions[i, j].coefficients.Length; k++)
                    {
                        var x = superpositions[i, j].coefficients[k];
                        if (x)
                        {
                            int firstindex = i * superpositions.GetLength(1) + j;
                            indices[firstindex] = k.ToString();
                        }

                    }
                }
            }

            System.IO.File.WriteAllLines(@"R:\text_save\WriteLines.txt", indices);
            
        }

        void SaveToPicture(int[,] pixValues, int index)
        {
            string name = index.ToString();
            string i1Path = @"r:\dataset\x"+ name + ".png";

            // Plot pixels
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int yIndex = height - 1 - y;

                    if (pixValues[x, yIndex] == 0)
                    {
                        PlotPixel(x, y, 255, 255, 255);
                    }
                    else if (pixValues[x, yIndex] == 1)
                    {
                        PlotPixel(x, y, 0, 128, 225);
                    }
                    else if (pixValues[x, yIndex] == 2)
                    { 
                        PlotPixel(x, y, 0,153,0);
                    }
                }
            }

            // Saving to file
            unsafe
            {
                fixed (byte* ptr = _imageBuffer)
                {
                    using (Bitmap image = new Bitmap(width, height, width * 4,
                       PixelFormat.Format32bppRgb, new IntPtr(ptr)))
                    {
                        image.Save(i1Path);
                    }
                }
            }
        }


        static void PlotPixel(int x, int y, byte redValue,  byte greenValue, byte blueValue)
        {
            int offset = ((width * 4) * y) + (x * 4);

            _imageBuffer[offset] = blueValue;
            _imageBuffer[offset + 1] = greenValue;
            _imageBuffer[offset + 2] = redValue;

            // Fixed alpha value (No transparency)
            _imageBuffer[offset + 3] = 255;
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