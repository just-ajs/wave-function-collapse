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

        public static void PlotPixelsFromRGB(Color[,] picture, byte[] _imageBuffer)
        {
            // Plot pixels
            for (int x = 0; x < picture.GetLength(0); x++)
            {
                for (int y = 0; y < picture.GetLength(1); y++)
                {
                    PlotPixel(x, y, picture[x, y], picture.GetLength(0), picture.GetLength(1), _imageBuffer);
                }
            }
        }

        public static double[,] generateRandomImage(int width, int height)
        {
            double[,] convertedImage = new double[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if ((i < 5 && j < 5) || (i > width - 5 && j > height - 5))
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

        public static float CountSuccessfullCellsInImageArea(double[,] image, WaveCollapseHistoryElement wfc)
        {
            int numberOfImageCellsInImage = 0;
            int numberOfNotWhiteCells = 0;

            for (int i = 0; i < wfc.Superpositions.GetLength(0); i++)
            {
                for (int j = 0; j < wfc.Superpositions.GetLength(1); j++)
                {
                    if (image[i, j] < 0.5)
                    {
                        numberOfImageCellsInImage++;

                        var cellState = wfc.Superpositions[i, j].state;
                        if (cellState != State.EMPTY)
                        {
                            numberOfNotWhiteCells++;
                        }
                    }
                }
            }

            return numberOfNotWhiteCells / (numberOfImageCellsInImage * 1.0f);
        }


        public static Color[,] BlurImage(Color[,] image)
        {
            Color[,] blurredImage = new Color[image.GetLength(0), image.GetLength(1)];

            for (int i = 0; i < image.GetLength(0); i++)
            {
                for (int j = 0; j < image.GetLength(1); j++)
                {
                    int redSum = 0;
                    int greenSum = 0;
                    int blueSum = 0;

                    int counter = 0;

                    // Check every pixels neighbours
                    for (int k = i - 1; k < i + 2; k++)
                    {
                        for (int m = j - 1; m < j + 2; m++)
                        {
                            // Check the boundaries
                            if (k < 0 || k > image.GetLength(0) - 1 || m < 0 || m > image.GetLength(1) - 1)
                            {
                                continue;
                            }

                            redSum += image[k, m].R;
                            greenSum += image[k, m].G;
                            blueSum += image[k, m].B;

                            counter++;

                        }
                    }

                    int averageRedSum = redSum / counter;
                    int averageGreenSum = greenSum / counter;
                    int averageBlueSum = blueSum / counter;

                    blurredImage[i, j] = ColorFromRGB(averageRedSum, averageGreenSum, averageBlueSum);
                }
            }
            return blurredImage;
        }

        public static Color[,] AddMask(Color[,] picture, Color background, Color mask, float treshold)
        {
            Color[,] pictureMask = new Color[picture.GetLength(0), picture.GetLength(1)];

            for (int i = 0; i < picture.GetLength(0); i++)
            {
                for (int j = 0; j < picture.GetLength(1); j++)
                {
                    var brightness = picture[i, j].GetBrightness();

                    pictureMask[i, j] = brightness < treshold ? mask : background;
                }
            }

            return pictureMask;
        }

        public static Color ColorFromRGBList(int[] color)
        {
            // Create a green color using the FromRgb static method.
            Color myRgbColor = new Color();
            myRgbColor = Color.FromArgb(color[0], color[1], color[2]);
            return myRgbColor;
        }

        public static Color ColorFromRGB(int red, int green, int blue)
        {
            // Create a green color using the FromRgb static method.
            Color myRgbColor = new Color();
            myRgbColor = Color.FromArgb(red, green, blue);
            return myRgbColor;
        }

        public static Color[,] CellsValuesToRGBColours(int[,] picture, int[] white, int[] blue, int[] green)
        {
            // Represent pictures as array of rgb values in Color class
            Color[,] pictureRGB = new Color[picture.GetLength(0), picture.GetLength(1)];

            for (int i = 0; i < picture.GetLength(0); i++)
            {
                for (int j = 0; j < picture.GetLength(1); j++)
                {
                    int x = picture[i, j];
                    if (x == 0)
                    {
                        pictureRGB[i, j] = Utils.ColorFromRGBList(white);
                    }
                    else if (x == 1)
                    {
                        pictureRGB[i, j] = Utils.ColorFromRGBList(blue);
                    }
                    else if (x == 2)
                    {
                        pictureRGB[i, j] = Utils.ColorFromRGBList(green);

                    }
                }
            }
            return pictureRGB;
        }




        public static void SaveToPicture(Color[,] picture, int index, string series, byte[] _imageBuffer)
        {
            //string i1Path = @"R:\csv\image_processing\" + series +  index.ToString() + ".png";
            string i1Path = @"R:\csv\image_processing\" + series  + ".png";

            // Saving to file
            unsafe
            {
                fixed (byte* ptr = _imageBuffer)
                {
                    using (Bitmap image = new Bitmap(picture.GetLength(0), picture.GetLength(1), picture.GetLength(0) * 4,
                       PixelFormat.Format32bppRgb, new IntPtr(ptr)))
                    {
                        image.Save(i1Path);
                    }
                }
            }
        }


        public static void PlotPixel(int x, int y, Color color, int width, int height, byte[] _imageBuffer)
        {
            int offset = ((width * 4) * y) + (x * 4);

            _imageBuffer[offset] = color.R;
            _imageBuffer[offset + 1] = color.G;
            _imageBuffer[offset + 2] = color.B;

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

        public static void SaveWeightToFile(List<double> realWeights, List<double> newWeight, string fileName)
        {
            var sb = buildWeightDataString(realWeights, newWeight, fileName);
            string filePath = @"R:\csv\weights_exponential\" + fileName+ ".csv";
            System.IO.File.WriteAllText(filePath, sb.ToString());
        }

        static StringBuilder buildWeightDataString(List<double> realWeights, List<double> newWeights, string fileName)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}, {1}", "Original weights", fileName);
            sb.AppendLine();

            for (int i = 0; i < realWeights.Count; i++)
            {
                sb.AppendFormat("{0}, {1}", realWeights[i], newWeights[i]);
                sb.AppendLine();
            }

            return sb;
        }

        public static void SaveConstrainsCheckToFile (float average, List<float> values)
        {
            var sb = buildConstrainCheckString(average, values);
            string filePath = @"R:\csv\constrains_check\" + "01" + ".csv";
            System.IO.File.WriteAllText(filePath, sb.ToString());
        }

        static StringBuilder buildConstrainCheckString(float average, List<float> values)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}", "Average");
            sb.AppendLine();
            sb.AppendFormat("{0}", average);
            sb.AppendLine();
            sb.AppendFormat("{0}", "Values");
            sb.AppendLine();


            for (int i = 0; i < values.Count; i++)
            {
                sb.AppendFormat("{0}", values[i]);
                sb.AppendLine();
            }

            return sb;
        }

        public static void SavePatternOccurencesToFiles(List<List<int>> x)
        {
            for (int i = 0; i < x.Count; i++)
            {
                SavePatternOccurenceToFile(x[i], i);
            }

        }


        public static void SavePatternOccurenceToFile(List<int> y, int index)
        {

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < y.Count; i++)
            {
                sb.AppendFormat("{0}", y[i]);
                sb.AppendLine();
            }

            string filePath = @"R:\csv\success_occurence\" + index + ".csv";
            System.IO.File.WriteAllText(filePath, sb.ToString());
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
