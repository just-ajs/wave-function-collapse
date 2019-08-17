using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WaveFunctionCollapse
{
    public class CheckGANConstains : GH_Component
    {

        public CheckGANConstains()
          : base("CheckGANConstains", "", "", "WFC", "Evaluate") { 
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("GANresult", "", "", GH_ParamAccess.item);
            pManager.AddParameter(new PatternFromSampleParam());
            pManager.AddColourParameter("Empty", "", "", GH_ParamAccess.item);
            pManager.AddColourParameter("Half", "", "", GH_ParamAccess.item);
            pManager.AddColourParameter("Full", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Save to file", "", "", GH_ParamAccess.item);


            
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Av. constr. success rate", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Constr. success", "", "", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string GANresult = "";
            DA.GetData(0, ref GANresult);

            GH_PatternsFromSample gh_patterns = new GH_PatternsFromSample();
            DA.GetData<GH_PatternsFromSample>(1, ref gh_patterns);

            Color empty = new Color();
            DA.GetData(2, ref empty);

            Color half = new Color();
            DA.GetData(3, ref half);

            Color full = new Color();
            DA.GetData(4, ref full);

            double width = 0;
            DA.GetData(5, ref width);

            double height = 0;
            DA.GetData(6, ref height);

            bool save = false;
            DA.GetData(7, ref save);

            Bitmap ganImage = (Bitmap)Image.FromFile(GANresult);
            var patterns = gh_patterns.Value.Patterns;
            var x = patterns[0].MiniTile;

            // Folder 
            DirectoryInfo di = new DirectoryInfo(@"R:\pix2pix\small_images\grasshopper_test");
            FileInfo[] Images = di.GetFiles("*.png");

            var files = Directory.GetFiles(@"R:\pix2pix\small_images\grasshopper_test", "*.png");

            List<float> successRates = new List<float>();

            // Rate for one image
            for (int i = 0; i < Images.Length; i++)
            {
                Bitmap image = (Bitmap)Image.FromFile(files[i]);
                float resultFromOneImage = getConstrainsRate(image, (int)width, (int)height, empty, half, full, patterns);

                successRates.Add(resultFromOneImage);
            }


            var d = successRates.Sum();
            var xd = successRates.Average();

            if (save)
            {
                Utils.SaveConstrainsCheckToFile(xd, successRates);

            }

            //patternOccurenceInGANResult()

            DA.SetData(0, xd);
            DA.SetDataList(1, successRates);
            

        }

        // FINISH THAT FUNCTION
        List<int> patternOccurenceInGANResult (State[,] img, bool[,] legalPattern, List<Pattern> patterns)
        {
            List<int> patternOccurence = new List<int>();

            for (int x = 0; x < patterns.Count; x++)
            {
                patternOccurence.Add(0);
            }

            for (int i = 0; i < img.GetLength(0); i++)
            {
                for ( int j = 0; j < img.GetLength(1); j++)
                {
                    if (legalPattern[i,j])
                    {
                        // Minitile
                        var minitile = new State[2, 2];

                        // Check minitile
                        for (int k = 0; k < patterns[0].patternSize; k++)
                        {
                            for (int m = 0; m < patterns[0].patternSize; m++)
                            {
                                int indexK = i + k;
                                int indexJ = j + m;

                                if (indexK >= img.GetLength(0) || indexJ >= img.GetLength(1))
                                {
                                    continue;
                                }

                                minitile[k, m] = img[indexK, indexJ];
                            }
                        }

                        var x = CheckStateIndex(minitile, patterns);
                        patternOccurence[x]++;
                    }

                }
            }

            return patternOccurence;
        }

        float getConstrainsRate(Bitmap ganImage, int width, int height, Color empty, Color half, Color full, List<Pattern> patterns)
        {
            //var scaledInput = RescaleImageToSmall(ganImage, (int)width, (int)height);
            var inputInColors = bitmapToColours(ganImage, (int)width, (int)height);

            var imgStates = ColorsIntoStates(inputInColors, empty, half, full);

            var booleanMaskOfConstrains = checkPatternConstrains(imgStates, patterns);

            float booleanRate = GetBooleanRate(booleanMaskOfConstrains);

            return booleanRate;
        }

        Color[,] bitmapToColours (Bitmap image, int width, int height)
        {
            Color[,] img = new Color[width, height];

            for (int i = 0; i < image.Width; i ++)
            {
                for (int j = 0; j < image.Height; j ++)
                {
                    img[i, j] = image.GetPixel(i, image.Height - 1 - j);

                }
            }

            return img;
        }

        Color[,] RescaleImageToSmall (Bitmap ganImg, int width, int height)
        {
            Color[,] img = new Color[width, height];

            List<Color> imageInList = new List<Color>();

            for (int i = 1; i < ganImg.Width; i += 9 )
            {
                for (int j = 1; j < ganImg.Height; j+= 9)
                {
                    var pix = ganImg.GetPixel(i, ganImg.Height - 1 - j);

                    imageInList.Add(pix);

                    // Figure out how to find the index!!!
                    int indexX = (i % 9) - 1;
                    int indexY = (j % 9) - 1;
                    img[indexX, indexY] = pix;
                }
            }

            return img;
        }

        float GetBooleanRate (bool[,] img)
        {
            float sum = 0;

            for (int i = 0; i < img.GetLength(0); i++)
            {
                for (int j = 0; j < img.GetLength(1); j++)
                {
                    if (img[i,j]) { sum++; }
                }
            }

            float result = sum / (img.GetLength(0) * img.GetLength(1));

            return result;
        }

        // This function will return a representation of the image in the boolean values.
        // True means that a pattern exists in a library of patterns. 
        bool[,] checkPatternConstrains(State[,] img, List<Pattern> patterns)
        {
            bool[,] constrainKept = new bool[img.GetLength(0) - 1, img.GetLength(1) - 1];

            for (int i = 0; i < img.GetLength(0) - 1; i++)
            {
                for (int j = 0; j < img.GetLength(1) - 1; j++)
                {
                    var minitile = new State[2, 2];
                    // Check minitile
                    for (int k = 0; k < patterns[0].patternSize; k++)
                    {
                        for (int m = 0; m < patterns[0].patternSize; m++)
                        {
                            int indexK = i + k;
                            int indexJ = j + m;

                            if ((i + k) >= img.GetLength(0) || (j + m) >= img.GetLength(1))
                            {
                                continue;
                            }

                            minitile[k, m] = img[i + k, j + m];
                        }
                    }

                    constrainKept[i, j] = CheckIfTheStateExists(minitile, patterns);

                }
            }

            return constrainKept;
        }


        bool CheckIfTheStateExists (State[,] state, List<Pattern> pat)
        {
            for (int i = 0; i < pat.Count; i++)
            {
                var patSt = pat[i].MiniTile;
                if (state[0,0] == patSt[0,0] && state[0,1] == patSt[0,1] 
                    && state[1,0] == patSt[1,0] && state[1,1]==patSt[1,1])
                {
                    return true;
                }
            }
            return false;
        }

        int CheckStateIndex(State[,] state, List<Pattern> pat)
        {
            for (int i = 0; i < pat.Count; i++)
            {
                var patSt = pat[i].MiniTile;
                if (state[0, 0] == patSt[0, 0] && state[0, 1] == patSt[0, 1]
                    && state[1, 0] == patSt[1, 0] && state[1, 1] == patSt[1, 1])
                {
                    return i;
                }
            }

            return 0;
        }

        State[,] ColorsIntoStates(Color[,] img, Color empty, Color half, Color full)
        {
            State[,] imgStates = new State[img.GetLength(0), img.GetLength(1)];

            int notAssigned = 0;

            for (int i = 0; i < img.GetLength(0); i ++)
            {
                for (int j = 0; j < img.GetLength(1); j++)
                {
                    var pix = img[i, j];

                    if (pix == empty)
                    {
                        imgStates[i, j] = State.EMPTY;
                    }
                    else if (pix == half)
                    {
                        imgStates[i, j] = State.HALF_TILE;
                    }
                    else if (pix == full)
                    {
                        imgStates[i, j] = State.FULL_TILE;
                    }
                    else
                    {
                        notAssigned++;
                    }
                }
            }

            return imgStates;
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
            get { return new Guid("ef9c31b5-c885-47e0-b2b9-d92947850e3d"); }
        }
    }
}