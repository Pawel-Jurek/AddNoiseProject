using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Threading;

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
        public void addNoiseToImage(bool selectedAssembler, int numberOfThreads)
        {

            List<DivideThread> threadSettings = divideImageForThreads(numberOfThreads);
            List<Thread> threads = new List<Thread>();

            foreach (DivideThread settings in threadSettings)
            {
                if (selectedAssembler)
                {
                    //threads.Add(new Thread(() =>
                        
                    //));
                }
                else
                {
                    //threads.Add(new Thread(() =>

                    //));
                }
            }

            foreach (Thread thread in threads)
            {
                thread.Start();
            }
            foreach (Thread thread in threads)
            {
                thread.Join();
            }
            savePixelRGBsToBitmap();
            SaveBitmapToBitArray();


        }
        private List<DivideThread> divideImageForThreads(int numberOfThreads)
        {
            List<DivideThread> threads = new List<DivideThread>();
            int colsToDivide = bitmap.Width;
            int colsPerThread = colsToDivide / numberOfThreads;
            int extraCols = colsToDivide % numberOfThreads;

            int currentCol = 0;
            for (int i = 0; i < numberOfThreads; i++)
            {
                DivideThread tempThread = new DivideThread
                {
                    processId = i,
                    imgWidth = bitmap.Width,
                    imgHeight = bitmap.Height,
                    imgColStart = currentCol
                };

                int colsForThisThread = colsPerThread + (i < extraCols ? 1 : 0);

                currentCol += colsForThisThread;
                tempThread.imgColStop = currentCol;

                threads.Add(tempThread);
            }

            return threads;
        }
        private void savePixelRGBsToBitmap()
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color noisedImg = Color.FromArgb(pixelRGBs[x, y, 0], pixelRGBs[x, y, 1], pixelRGBs[x, y, 2]);
                    bitmap.SetPixel(x, y, noisedImg);
                }
            }
        }

        private void SaveBitmapToBitArray()
        {
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Position = 0;
                for (int i = 0; i < finalImage.Length; i++)
                    finalImage[i] = (byte)ms.ReadByte();

            }
        }
    }

    
}
