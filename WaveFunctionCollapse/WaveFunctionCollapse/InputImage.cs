using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveFunctionCollapse
{
     public class InputImage
    {
        public double[,] Brightness { get; set; }

        public InputImage (double[,] brightness)
        {
            this.Brightness = brightness;
        }

        public InputImage()
        {

        }
    }


}
                   