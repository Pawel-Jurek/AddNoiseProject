using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Timers;
using System.Diagnostics;
using System.Windows;

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

        public int addNoiseToImage(bool selectedAssembler, string selectedNoise, int numberOfThreads, int noisePower)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<DivideThread> threadParams = divideImageForThreads(numberOfThreads, selectedNoise, noisePower);
            List<Thread> threads = new List<Thread>();

            foreach (DivideThread thread in threadParams)
            {
                if (selectedAssembler)
                {
                    switch (selectedNoise)
                    {
                        case "random":
                        case "white":
                        case "color":
                        default: break;
                    }                  
                }
                else
                {
                    switch (selectedNoise)
                    {
                        case "random":
                            threads.Add(new Thread(() =>addRandomNoiseInCSharp(thread.pixelsCoordinates)));
                            break;
                        case "white":
                            threads.Add(new Thread(() => addWhiteNoiseInCSharp(thread.pixelsCoordinates, noisePower)));
                            break;
                        case "color":
                            threads.Add(new Thread(() => addColorNoiseInCSharp(thread.pixelsCoordinates, noisePower)));
                            break;
                        default: break;
                    }
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


            stopwatch.Stop();
            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            savePixelRGBsToBitmap();
            saveBitmapToBitArray();
            return (int)elapsedMilliseconds;

        }
        private List<DivideThread> divideImageForThreads(int numberOfThreads, string noiseType, int noisePower)
        {
            List<DivideThread> threads = new List<DivideThread>();
            Random random = new Random();
            int totalPixels = bitmap.Width * bitmap.Height;
            int pixelsToNoise;  
            if (noiseType == "random")
            {
                pixelsToNoise = (int)(0.01 * (random.Next(5,70)) * totalPixels);
            } 
            else
            {
                pixelsToNoise = (int)(noisePower * totalPixels / 100);
            }
            int pixelsPerThread = pixelsToNoise / numberOfThreads;
            int extraPixels = pixelsToNoise % numberOfThreads;

            for (int i = 0; i < numberOfThreads; i++)
            {
                DivideThread tempThread = new DivideThread
                {
                    processId = i,
                    pixelsCoordinates = new List<KeyValuePair<int, int>>()
                };
                int pixelsForThisThread = pixelsPerThread + (i < extraPixels ? 1 : 0);
                for (int j = 0; j < pixelsForThisThread; j++)
                {
                    tempThread.pixelsCoordinates.Add(new KeyValuePair<int, int>(random.Next(bitmap.Width), random.Next(bitmap.Height)));
                }
    
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

        private void saveBitmapToBitArray()
        {
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Position = 0;
                for (int i = 0; i < finalImage.Length; i++)
                    finalImage[i] = (byte)ms.ReadByte();

            }
        }
        
        public void addRandomNoiseInCSharp(List<KeyValuePair<int, int>> listOfCoordinates)
        {          
            Random random = new Random();

            foreach (KeyValuePair<int,int> coordinates in listOfCoordinates)
            {
                byte newRed = (byte)random.Next(256);
                byte newGreen = (byte)random.Next(256);
                byte newBlue = (byte)random.Next(256);

                pixelRGBs[coordinates.Key, coordinates.Value, 0] = newRed;
                pixelRGBs[coordinates.Key, coordinates.Value, 1] = newGreen;
                pixelRGBs[coordinates.Key, coordinates.Value, 2] = newBlue;
            }
        }

        public void addWhiteNoiseInCSharp(List<KeyValuePair<int, int>> pixelsCoordinates, int noisePower)
        {
            Random random = new Random();

            foreach (KeyValuePair<int, int> coordinates in pixelsCoordinates)
            {
                double u1 = 1.0 - random.NextDouble();
                double u2 = 1.0 - random.NextDouble();
                double z0 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);

                for (int i = 0; i < 3; i++)
                {
                    double noise = noisePower * z0; 
                    int newValue = (int)(pixelRGBs[coordinates.Key, coordinates.Value, i] + noise);

                    newValue = Math.Max(0, Math.Min(255, newValue));
                    pixelRGBs[coordinates.Key, coordinates.Value, i] = (byte)newValue;
                }
            }
        }

        public void addColorNoiseInCSharp(List<KeyValuePair<int, int>> pixelsCoordinates, int noisePower)
        {
            Random random = new Random();

            foreach (KeyValuePair<int, int> coordinates in pixelsCoordinates)
            {
                double[] z = new double[3];
                for (int i = 0; i < 3; i++)
                {
                    double u1 = 1.0 - random.NextDouble();
                    double u2 = 1.0 - random.NextDouble();
                    z[i] = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
                }

                for (int i = 0; i < 3; i++)
                {
                    double noise = noisePower * z[i];
                    int newValue = (int)(pixelRGBs[coordinates.Key, coordinates.Value, i] + noise);

                    newValue = Math.Max(0, Math.Min(255, newValue));
                    pixelRGBs[coordinates.Key, coordinates.Value, i] = (byte)newValue;
                }
            }
        }


    }

}
