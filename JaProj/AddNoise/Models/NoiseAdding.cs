using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace AddNoise.Models
{
    public class NoiseAdding
    {
        public byte[] originalImage { get; private set; }
        public byte[] finalImage { get; private set; }
        public byte[,,] pixelRGBs { get; private set; }
        private Bitmap bitmap;
        public NoiseAdding(string filename) 
        {
            originalImage = File.ReadAllBytes(filename);
            finalImage = new byte[originalImage.Length];
            bitmap = new Bitmap(filename, true);
            pixelRGBs = new byte[bitmap.Width, bitmap.Height, 3];
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);
                    pixelRGBs[x, y, 0] = pixelColor.R;
                    pixelRGBs[x, y, 1] = pixelColor.G;
                    pixelRGBs[x, y, 2] = pixelColor.B;
                }
            }
        }
    }
}
