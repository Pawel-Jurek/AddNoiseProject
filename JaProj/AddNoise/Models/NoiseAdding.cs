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
using System.Runtime.InteropServices;

namespace AddNoise.Models
{
    public class NoiseAdding
    {
        [DllImport(@"C:\Users\pawel\source\repos\AddNoiseProject\JaProj\x64\Debug\JAAsm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void randomNoiseAsm(byte[] pixelRGBs, byte[] data, int[] lenWidthHeight, int[] coordinatesArray);
        public byte[] originalImage { get; private set; }
        public byte[] finalImage { get; private set; }
        public byte[] pixelRGBs { get; private set; }
        private Bitmap bitmap;
        public NoiseAdding(string filename) 
        {
            originalImage = File.ReadAllBytes(filename);
            finalImage = new byte[originalImage.Length];
            bitmap = new Bitmap(filename, true);

            pixelRGBs = new byte[bitmap.Width * bitmap.Height * 3];
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);
                    int index = (y * bitmap.Width + x) * 3;

                    pixelRGBs[index] = pixelColor.R;
                    pixelRGBs[index + 1] = pixelColor.G;
                    pixelRGBs[index + 2] = pixelColor.B;
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
                            threads.Add(new Thread(() => addRandomNoiseInAssembler(thread.pixelsCoordinates)));
                            break;
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
                Debug.WriteLine($"Wątek {thread.ManagedThreadId}");
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
            Debug.WriteLine("Obrazek przetworzony");
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
                pixelsToNoise = 1000;
                //pixelsToNoise = (int)(0.01 * (random.Next(5,70)) * totalPixels);
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
                    int index = (y * bitmap.Width + x) * 3;

                    Color noisedImg = Color.FromArgb(pixelRGBs[index], pixelRGBs[index + 1], pixelRGBs[index + 2]);
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

            foreach (KeyValuePair<int, int> coordinates in listOfCoordinates)
            {
                int index = (coordinates.Value * bitmap.Width + coordinates.Key) * 3;

                byte newRed = (byte)random.Next(256);
                byte newGreen = (byte)random.Next(256);
                byte newBlue = (byte)random.Next(256);

                pixelRGBs[index] = newRed;
                pixelRGBs[index + 1] = newGreen;
                pixelRGBs[index + 2] = newBlue;
            }
        }


        public void addRandomNoiseInAssembler(List<KeyValuePair<int, int>> listOfCoordinates)
        {

            Random random = new Random();
            int width = bitmap.Width;
            int height = bitmap.Height;

            int values = listOfCoordinates.Count * 3;
            byte[] data = new byte[values];

            for (int i = 0; i < values; i++)
            {
                data[i] = (byte)random.Next(256);
            }

            int[] coordinatesArray = new int[listOfCoordinates.Count * 2];
            int c = 0;
            foreach (KeyValuePair<int, int> coordinates in listOfCoordinates)
            {
                coordinatesArray[c++] = coordinates.Key;
                coordinatesArray[c++] = coordinates.Value;
            }

            int[] dimensions = { data.Length, width, height };
            randomNoiseAsm(pixelRGBs, data, dimensions, coordinatesArray);

            int counter = 0;
            int j = 0;
            foreach (var coords in listOfCoordinates)
            {
                if (pixelRGBs[(coords.Value * width + coords.Key) * 3] != data[j * 3]) counter++;
                if (pixelRGBs[(coords.Value * width + coords.Key) * 3 + 1] != data[j * 3 + 1]) counter++;
                if (pixelRGBs[(coords.Value * width + coords.Key) * 3 + 2] != data[j * 3 + 2]) counter++;
                j++;
            }
            Debug.WriteLine("Not changed: " + counter + "/" + listOfCoordinates.Count*3);
        }


        private void PrintPixelValues(List<KeyValuePair<int, int>> listOfCoordinates)
        {
            int j = 0;
            foreach (var coords in listOfCoordinates)
            {
                Debug.WriteLine($"pixelRGBs[{coords.Key}, {coords.Value}, 0] = {pixelRGBs[j * 3]}");
                Debug.WriteLine($"pixelRGBs[{coords.Key}, {coords.Value}, 1] = {pixelRGBs[j * 3 + 1]}");
                Debug.WriteLine($"pixelRGBs[{coords.Key}, {coords.Value}, 2] = {pixelRGBs[j * 3 + 2]}");
                j++;
            }
        }

        public void addWhiteNoiseInCSharp(List<KeyValuePair<int, int>> pixelsCoordinates, int noisePower)
        {
            Random random = new Random();

            foreach (KeyValuePair<int, int> coordinates in pixelsCoordinates)
            {
                int index = (coordinates.Value * bitmap.Width + coordinates.Key) * 3;

                double u1 = 1.0 - random.NextDouble();
                double u2 = 1.0 - random.NextDouble();
                double z0 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);

                for (int i = 0; i < 3; i++)
                {
                    double noise = noisePower * z0;
                    int newValue = (int)(pixelRGBs[index + i] + noise);

                    newValue = Math.Max(0, Math.Min(255, newValue));
                    pixelRGBs[index + i] = (byte)newValue;
                }
            }
        }

        public void addColorNoiseInCSharp(List<KeyValuePair<int, int>> pixelsCoordinates, int noisePower)
        {
            Random random = new Random();

            foreach (KeyValuePair<int, int> coordinates in pixelsCoordinates)
            {
                int index = (coordinates.Value * bitmap.Width + coordinates.Key) * 3;

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
                    int newValue = (int)(pixelRGBs[index + i] + noise);

                    newValue = Math.Max(0, Math.Min(255, newValue));
                    pixelRGBs[index + i] = (byte)newValue;
                }
            }
        }



    }

}
