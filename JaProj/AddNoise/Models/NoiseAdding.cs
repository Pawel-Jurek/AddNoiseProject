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

        [DllImport(@"C:\Users\pawel\source\repos\AddNoiseProject\JaProj\x64\Debug\cppDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void addRandomNoiseInCpp(byte[] pixelRGBs, int width, int height, int[] x, int[] y, int count);

        [DllImport(@"C:\Users\pawel\source\repos\AddNoiseProject\JaProj\x64\Debug\cppDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void addWhiteNoiseInCpp(int[] xCoordinates, int[] yCoordinates, int noisePower, byte[] pixelRGBs, int bitmapWidth, int length);
        
        [DllImport(@"C:\Users\pawel\source\repos\AddNoiseProject\JaProj\x64\Debug\cppDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void addColorNoiseInCpp(int[] xCoordinates, int[] yCoordinates, int noisePower, byte[] pixelRGBs, int bitmapWidth, int length);
        
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
                            threads.Add(new Thread(() => addRandomNoiseInAssembler(thread.xCoordinates, thread.yCoordinates)));
                            break;
                        case "white":
                        case "color":
                        default: break;
                    }                  
                }
                else
                {

                    int coordinatesCount = thread.yCoordinates.Length;


                    switch (selectedNoise)
                    {
                        
                        case "random":
                            threads.Add(new Thread(() =>addRandomNoiseInCpp(pixelRGBs, bitmap.Width, bitmap.Height, thread.xCoordinates, thread.yCoordinates, coordinatesCount)));
                            break;
                        case "white":
                            threads.Add(new Thread(() => addWhiteNoiseInCpp(thread.xCoordinates, thread.yCoordinates, noisePower, pixelRGBs, bitmap.Width, coordinatesCount)));
                            break;
                        case "color":
                            threads.Add(new Thread(() => addColorNoiseInCpp(thread.xCoordinates, thread.yCoordinates, noisePower, pixelRGBs, bitmap.Width, coordinatesCount)));
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
                //pixelsToNoise = 1000;
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
                int pixelsForThisThread = pixelsPerThread + (i < extraPixels ? 1 : 0);
                DivideThread tempThread = new DivideThread
                {
                    processId = i,
                    xCoordinates = new int[pixelsForThisThread],
                    yCoordinates = new int[pixelsForThisThread]
                };
                
                for (int j = 0; j < pixelsForThisThread; j++)
                {
                    tempThread.xCoordinates[j] = random.Next(bitmap.Width);
                    tempThread.yCoordinates[j] = random.Next(bitmap.Height);                
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

        public void addRandomNoiseInAssembler(int[] xCoordinates, int[] yCoordinates)
        {

            Random random = new Random();
            int width = bitmap.Width;
            int height = bitmap.Height;

            int values = xCoordinates.Length * 3;
            byte[] data = new byte[values];

            for (int i = 0; i < values; i++)
            {
                data[i] = (byte)random.Next(256);
            }

            int[] coordinatesArray = new int[xCoordinates.Length * 2];
            int c = 0;
            for (int i = 0; i< xCoordinates.Length; i++)
            {
                coordinatesArray[c++] = xCoordinates[i];
                coordinatesArray[c++] = yCoordinates[i];
            }
            

            int[] dimensions = { data.Length, width, height };
            randomNoiseAsm(pixelRGBs, data, dimensions, coordinatesArray);

            //int counter = 0;
            //int j = 0;
            //foreach (var coords in listOfCoordinates)
            //{
            //    if (pixelRGBs[(coords.Value * width + coords.Key) * 3] != data[j * 3]) counter++;
            //    if (pixelRGBs[(coords.Value * width + coords.Key) * 3 + 1] != data[j * 3 + 1]) counter++;
            //    if (pixelRGBs[(coords.Value * width + coords.Key) * 3 + 2] != data[j * 3 + 2]) counter++;
            //    j++;
            //}
            //Debug.WriteLine("Not changed: " + counter + "/" + listOfCoordinates.Count*3);
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
        
    }

}
