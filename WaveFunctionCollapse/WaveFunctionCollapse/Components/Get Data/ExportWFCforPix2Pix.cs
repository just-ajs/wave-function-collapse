using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WaveFunctionCollapse.Components
{
    public class ExportWFCforPix2Pix : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ExportWFCforPix2Pix class.
        /// </summary>
        public ExportWFCforPix2Pix()
          : base("ExportWFCforPix2Pix", "Nickname", "Description",
               "WFC", "Data Analysis")
        {
        }

        // Picture data
        static int biteSize;
        private static byte[] _imageBuffer;
        static int width, height;

        int[] white = new int[3] { 255, 255, 255 };
        int[] green = new int[3] { 20, 220, 50 };
        int[] blue = new int[3] { 10, 50, 220 };

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new PatternResultsParam(), "Wave Function Dataset", "WFC", "Wave Function Collapse with history", GH_ParamAccess.item);
            pManager.AddParameter(new PatternFromSampleParam(), "PatternFromSample", "", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Series name", "", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Save pictures", "", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Save average", "", "", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("AverageWeights", "", "", GH_ParamAccess.list);
        }

       protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_WaveCollapseResults waveCollapseDataset = new GH_WaveCollapseResults();
            DA.GetData<GH_WaveCollapseResults>(0, ref waveCollapseDataset);

            GH_PatternsFromSample gh_patterns = new GH_PatternsFromSample();
            DA.GetData<GH_PatternsFromSample>(1, ref gh_patterns);

            string series = "";
            DA.GetData<string>(2, ref series);

            bool savePictures = false;
            DA.GetData<bool>(3, ref savePictures);

            bool savePatternOccurence = false;
            DA.GetData<bool>(4, ref savePatternOccurence);

            var waveElements = Utils.GetObservedWave(waveCollapseDataset);
            var patterns = Utils.GetPatternsFromSample(gh_patterns);

            var onewfc = waveElements[0];

            List<List<int>> patternOccurence = new List<List<int>>();

            // Get picture width and height.
            width = waveElements[0].Superpositions.GetLength(0);
            height = waveElements[0].Superpositions.GetLength(1);

            // Allocate memory.
            biteSize = (int)((width * 2 * 4) * height);
            _imageBuffer = new byte[biteSize];

            // For each wave function collapse solution: duplicate image, remove noise, save as image
            for (int i = 0; i < waveElements.Count; i++)
            {
                // Generate new superposition with noise reduced.
                var reducedNoise = RemoveNoiseFromImage(waveElements[i], patterns);

                // Get values of pixels from original picture and represent them as int{0,1,2}. 
                var pixelsCodesOriginal = Utils.GetImageValuesFromWave(waveElements[i].Superpositions);
                var pixelsCodesNoiseReduced = Utils.GetImageValuesFromWave(reducedNoise);

                // Change to rgb
                var orignalPictureRGB = Utils.CellsValuesToRGBColours(pixelsCodesOriginal, white, blue, green);
                var processedPictureRGB = Utils.CellsValuesToRGBColours(pixelsCodesNoiseReduced, white, blue, green);

                // Blurr the image
                var blurredNoiseReduced = Utils.BlurImage(processedPictureRGB);
                var blurredNoiseReduced02 = Utils.BlurImage(blurredNoiseReduced);
                var blurredNoiseReduced03 = Utils.BlurImage(blurredNoiseReduced02);

                // Add masks
                var background = Utils.ColorFromRGB(255, 159, 0);
                var mask = Utils.ColorFromRGB(0, 0, 222);
                var blurredNoiseReducedMasked = Utils.AddMask(blurredNoiseReduced02, background, mask, 0.7f);

                if (savePictures)
                {
                    // Merge original and processed image.
                    //var mergedImages = mergeTwoImages(orignalPictureRGB, blurredNoiseReducedMasked);

                    // Plot from RGB
                    //Utils.PlotPixelsFromRGB(mergedImages, _imageBuffer);

                    // Save to file
                    //Utils.SaveToPicture(mergedImages, i, series, _imageBuffer);

                    // Save 01
                    saveSeries(orignalPictureRGB, processedPictureRGB, "first");

                    // Save 02
                    saveSeries(blurredNoiseReduced, blurredNoiseReduced02, "second");

                    // Save 03
                    saveSeries(blurredNoiseReduced03, blurredNoiseReducedMasked, "third");

                    
                }


                var list = GetPatternOccurenceFromMaskedArea(waveElements[i], patterns, blurredNoiseReducedMasked, mask);
                patternOccurence.Add(list);
            }

            if (savePatternOccurence)
            {
                Utils.SavePatternOccurencesToFiles(patternOccurence);

            }
            var average = averagePatternOccurence(patternOccurence);

            DA.SetDataList(0, average);

        }

        void processTheImage

        void saveSeries(Color[,] a, Color[,] b, string name)
        {
            // Merge original and processed image.
            var mergedImages = mergeTwoImages(a, b);

            // Plot from RGB
            Utils.PlotPixelsFromRGB(mergedImages, _imageBuffer);

            // Save to file
            Utils.SaveToPicture(mergedImages, 0, name, _imageBuffer);
        }

        List<float> averagePatternOccurence (List<List<int>> list)
        {
            List<float> average = new List<float>();

            for(int i = 0; i < list[0].Count; i++)
            {
                average.Add(0);
            }

            // Go through every list of pattern occurence and sum
            for (int i = 0; i < list.Count; i++)
            {
                // Go through every element and add to list
                for (int j = 0; j < list[0].Count; j++)
                {
                    average[j] += list[i].ElementAt(j);
                }
            }

            // Now divide and return floats
            for (int i = 0; i < average.Count; i++)
            {
                average[i] = average[i] / list.Count;
            }

            return average;
        }

        List<int> GetPatternOccurenceFromMaskedArea(WaveCollapseHistoryElement wfc, PatternFromSampleElement patterns, Color[,] masked, Color black)
        {
            //var x = wfc.
            var x = patterns.Patterns;
            var superposition = wfc.Superpositions;

            // List of pattern 
            List<int> goodPatternsOccurence = new List<int>();

            // Fill list with zeros
            for (int i = 0; i < x.Count; i++)
            {
                goodPatternsOccurence.Add(0);
            }

            for (int i = 0; i < superposition.GetLength(0); i++)
            {
                for (int j = 0; j < superposition.GetLength(1); j++)
                {
                    if (masked[i,j] == black)
                    {
                        int index = superposition[i, j].collapsedIndex;
                        goodPatternsOccurence[index]++;
                    }
                }
            }

            return goodPatternsOccurence;
        }

        

        Superposition[,] RemoveNoiseFromImage(WaveCollapseHistoryElement wfc, PatternFromSampleElement p)
        {
            var patterns = p.Patterns;
            // Original image superpositions
            var superpositions = wfc.Superpositions;

            // Create new copied superpositions[,]. 
            Superposition[,] superpositionsWithoutNoise = NewCopiedSuperposition(superpositions, patterns);

            // Find superposition to replace once colorful cells should be changed to empty. 
            Superposition emptySuperpositiontoReplace = FindEmptySuperpositionToReplace(superpositions, patterns);

            RemoveSingleCells(ref superpositionsWithoutNoise, superpositions, emptySuperpositiontoReplace, 2, 1);
            RemoveSingleCells(ref superpositionsWithoutNoise, superpositions, emptySuperpositiontoReplace, 1, 1);
            RemoveSingleCells(ref superpositionsWithoutNoise, superpositions, emptySuperpositiontoReplace, 2, 1);
            RemoveSingleCells(ref superpositionsWithoutNoise, superpositions, emptySuperpositiontoReplace, 1, 1);
            //RemoveSingleCells(ref superpositionsWithoutNoise, superpositions, emptySuperpositiontoReplace, 2, 1);
            //RemoveSingleCells(ref superpositionsWithoutNoise, superpositions, emptySuperpositiontoReplace, 1, 1);
            //RemoveSingleCells(ref superpositionsWithoutNoise, superpositions, emptySuperpositiontoReplace, 2, 1);
            //RemoveSingleCells(ref superpositionsWithoutNoise, superpositions, emptySuperpositiontoReplace, 1, 1);


            return superpositionsWithoutNoise;
        }

        void RemoveSingleCells(ref Superposition[,] newSuperpositon, Superposition[,] existingSuperpositions,
            Superposition empty, int neighbourLimit, int iterations)
        {
            
            for (int k = 0; k < iterations; k++)
            {
                int removed = 0;
                for (int i = 0; i < existingSuperpositions.GetLength(0); i++)
                {
                    for (int j = 0; j < existingSuperpositions.GetLength(1); j++)
                    {
                        // If tile is not white - check the neighbours. 
                        if (existingSuperpositions[i, j].state != State.EMPTY)
                        {
                            // Count all the neighbours that are not white. 
                            var neighbours = CountNotEmptyNeighbours(i, j, existingSuperpositions);

                            if (neighbours < neighbourLimit)
                            {
                                newSuperpositon[i, j] = empty;
                                newSuperpositon[i, j].state = State.EMPTY;

                                removed++;
                            }
                        }
                    }
                }
                Console.WriteLine("removed: " + removed);
            }

            
        }


        int CountNotEmptyNeighbours(int i, int j, Superposition[,] superpositions)
        {
            int neighbours = 0;
            for (int k = i - 1; k < i + 2; k++)
            {
                for (int m = j - 1; m < j + 2; m++)
                {
                    // Check the boundaries. 
                    if (k < 0 || k > superpositions.GetLength(0) - 1 || m < 0 || m > superpositions.GetLength(1) - 1)
                    {
                        continue;
                    }

                    // Check if the cell is not checking itself. 
                    if (k == i && m == j)
                    {
                        continue;
                    }

                    // Do not check the corners. 
                    if (k == i - 1 && m == j - 1) continue;
                    if (k == i - 1 && m == j + 1) continue;
                    if (k == i + 1 && m == j - 1) continue;
                    if (k == i + 1 && m == j - 1) continue;

                    
                    var neighbourState = superpositions[k, m].state;
                    if (neighbourState == State.HALF_TILE || neighbourState == State.FULL_TILE)
                    {
                            neighbours++;
                    }
                    
                }
            }

            return neighbours;
        }

        Superposition FindEmptySuperpositionToReplace (Superposition[,] superpositions, List<Pattern> patterns)
        {
            Superposition x = new Superposition(superpositions[0,0], patterns);

            for (int i = 0; i < superpositions.GetLength(0); i++)
            {
                for (int j = 0; j < superpositions.GetLength(1); j++)
                {
                    if (superpositions[i,j].state == State.EMPTY)
                    {
                        return superpositions[i, j];
                    }
                }
            }

            return null; 
        }

        // Create new superpositions that are empty. 
        Superposition[,] NewCopiedSuperposition (Superposition[,] superpositions, List<Pattern> patterns)
        {
            var empty = new Superposition[superpositions.GetLength(0), superpositions.GetLength(1)];
            {
                for (int i = 0; i < superpositions.GetLength(0); i++)
                {
                    for (int j = 0; j < superpositions.GetLength(1); j++)
                    {
                        empty[i, j] = new Superposition(superpositions[i, j], patterns);
                    }
                }
            }

            return empty;
        }



        int[,] doubleTheImage (int[,] image)
        {
            int[,] newImage = new int[image.GetLength(0) * 2, image.GetLength(1)];

            for (int i = 0; i < newImage.GetLength(0); i++)
            {
                for (int j = 0; j < newImage.GetLength(1); j++)
                {
                    if (i > image.GetLength(0)-1 && i < newImage.GetLength(0))
                    {
                        newImage[i, j] = image[i - image.GetLength(0), j];
                    }
                    else
                    {
                        newImage[i, j] = image[i, j];
                    }
                }
            }

            return newImage;
            
        }

        Color[,] mergeTwoImages (Color[,] imageOriginal, Color[,] imageProcessed)
        {
            Color[,] merged = new Color[imageOriginal.GetLength(0) + imageProcessed.GetLength(0), imageOriginal.GetLength(1)];

            for (int i = 0; i < merged.GetLength(0); i++)
            {
                for (int j = 0; j < merged.GetLength(1); j++)
                {
                    if (i < imageOriginal.GetLength(0))
                    {
                        merged[i, j] = imageOriginal[i, j];
                    }
                    else if (i > imageOriginal.GetLength(0) - 1 && i < merged.GetLength(0))
                    {
                        merged[i, j] = imageProcessed[i - imageOriginal.GetLength(0), j];
                    }
                }
            }

            return merged;
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
            get { return new Guid("c74056e4-3da9-4b4e-b9e6-fdb98bf51ff7"); }
        }
    }
}