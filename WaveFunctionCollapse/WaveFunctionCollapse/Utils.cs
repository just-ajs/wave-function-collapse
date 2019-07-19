using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveFunctionCollapse
{
    public static class Utils
    {
        // Find width and height of surface
        public static int GetNumberofPointsInOneDimension(double firstPointCoordinate, double secondPointCoordinate)
        {
            return Math.Abs((int)(0.5 * (firstPointCoordinate - secondPointCoordinate) - 1));
        }

        public static void ConvertToRGB(int[,] pixValues, int index, string series, int width, int height, byte[] _imageBuffer)
        {
            // Plot pixels
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int yIndex = height - 1 - y;

                    if (pixValues[x, yIndex] == 0)
                    {
                        Utils.PlotPixel(x, y, 255, 255, 255, width, height, _imageBuffer);
                    }
                    else if (pixValues[x, yIndex] == 1)
                    {
                        Utils.PlotPixel(x, y, 0, 128, 225, width, height, _imageBuffer);
                    }
                    else if (pixValues[x, yIndex] == 2)
                    {
                        Utils.PlotPixel(x, y, 0, 153, 0, width, height, _imageBuffer);
                    }
                }
            }
        }


        public static void SaveToPicture(int[,] pixValues, int index, string series, int width, int height, byte[] _imageBuffer)
        {
            string name = index.ToString();
            string i1Path = @"r:\pix2pix_dataset\" + series + name + ".png";

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


        public static void PlotPixel(int x, int y, byte redValue, byte greenValue, byte blueValue, int width, int height, byte[] _imageBuffer)
        {
            int offset = ((width * 4) * y) + (x * 4);

            _imageBuffer[offset] = blueValue;
            _imageBuffer[offset + 1] = greenValue;
            _imageBuffer[offset + 2] = redValue;

            // Fixed alpha value (No transparency)
            _imageBuffer[offset + 3] = 255;
        }


        public static PatternFromSampleElement GetPatternsFromSample(GH_PatternsFromSample patternFromSample)
        {
            var patterns = patternFromSample.Value;
            return patterns;
        }

        public static List<WaveCollapseHistoryElement> GetObservedWave(GH_WaveCollapseResults results)
        {
            if (results.Value.Elements.Count == 0) return null;
            //int index = results.Value.Elements.Count - 1;
            var waveElements = results.Value.Elements.ToList();
            return waveElements;
        }

        public static void SaveToTextFile(Superposition[,] superpositions, int width, int height)
        {
            string[] indices = new string[superpositions.GetLength(0) * superpositions.GetLength(1)];

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

        public static int[,] GetImageValuesFromWave(Superposition[,] superpositions)
        {
            int width = superpositions.GetLength(0);
            int height = superpositions.GetLength(1);
            int[,] values = new int[width, height];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                        if (superpositions[i,j].state == State.EMPTY)
                        {
                            values[i, j] = 0;
                        }
                        else if (superpositions[i,j].state == State.FULL_TILE)
                        {
                            values[i, j] = 1;
                        }
                        else if (superpositions[i,j].state == State.HALF_TILE)
                        {
                            values[i, j] = 2;
                        }
                }
            }

            return values;
        }
    }
}
