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
using System.Reflection;
using System.Xml;

namespace AddNoise.Models
{
    public class NoiseAdding
    {
        [DllImport(@"C:\Users\pawel\source\repos\AddNoiseProject\JaProj\x64\Debug\JAAsm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void randomNoiseAsm(byte[] pixelRGBs, byte[] data, int[] lenWidthHeight, int[] coordinatesArray);
        [DllImport(@"C:\Users\pawel\source\repos\AddNoiseProject\JaProj\x64\Debug\JAAsm.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void whiteNoiseAsm(float[] u, float[] rgb, int noisePower);
        [DllImport(@"C:\Users\pawel\source\repos\AddNoiseProject\JaProj\x64\Debug\JAAsm.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void colorNoiseAsm(float[] u, float[] rgb, float[] colorMask, int noisePower);

        [DllImport(@"C:\Users\pawel\source\repos\AddNoiseProject\JaProj\x64\Debug\cppDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void addRandomNoiseInCpp(byte[] pixelRGBs, int width, int height, int[] x, int[] y, int count);

        [DllImport(@"C:\Users\pawel\source\repos\AddNoiseProject\JaProj\x64\Debug\cppDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void addWhiteNoiseInCpp(int[] xCoordinates, int[] yCoordinates, int noisePower, byte[] pixelRGBs, int bitmapWidth, int length);
        
        [DllImport(@"C:\Users\pawel\source\repos\AddNoiseProject\JaProj\x64\Debug\cppDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void addColorNoiseInCpp(int[] xCoordinates, int[] yCoordinates, int noisePower, byte[] pixelRGBs, int bitmapWidth, int length, float[]colorMask);
        
        public byte[] originalImage { get; private set; }
        public byte[] finalImage { get; private set; }
        public byte[] pixelRGBs { get; private set; }
        private int bitmapWidth;
        private int bitmapHeight;
        private Bitmap bitmap;
        public NoiseAdding(string filename) 
        {
            originalImage = File.ReadAllBytes(filename);
            finalImage = new byte[originalImage.Length];
            bitmap = new Bitmap(filename, true);
            bitmapWidth = bitmap.Width;
            bitmapHeight = bitmap.Height;

            pixelRGBs = new byte[bitmapWidth * bitmapHeight * 3];
            for (int x = 0; x < bitmapWidth; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);
                    int index = (y * bitmapWidth + x) * 3;

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
            Random random = new Random();
            float[] colorMask = { random.Next(256), random.Next(256), random.Next(256) };
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
                            threads.Add(new Thread(() => addWhiteNoiseInAssembler(thread.xCoordinates, thread.yCoordinates, noisePower)));
                            break;
                        case "color":
                            
                            threads.Add(new Thread(() => addColorNoiseInAssembler(thread.xCoordinates, thread.yCoordinates, noisePower, colorMask)));
                            break;
                        default: break;
                    }                  
                }
                else
                {
                    int coordinatesCount = thread.yCoordinates.Length;
                    switch (selectedNoise)
                    {                     
                        case "random":
                            threads.Add(new Thread(() =>addRandomNoiseInCpp(pixelRGBs, bitmapWidth, bitmapHeight, thread.xCoordinates, thread.yCoordinates, coordinatesCount)));
                            break;
                        case "white":
                            threads.Add(new Thread(() => addWhiteNoiseInCpp(thread.xCoordinates, thread.yCoordinates, noisePower, pixelRGBs, bitmapWidth, coordinatesCount)));
                            break;
                        case "color":
                            threads.Add(new Thread(() => addColorNoiseInCpp(thread.xCoordinates, thread.yCoordinates, noisePower, pixelRGBs, bitmapWidth, coordinatesCount, colorMask)));
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
            int totalPixels = bitmapWidth * bitmapHeight;
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
                    tempThread.xCoordinates[j] = random.Next(bitmapWidth);
                    tempThread.yCoordinates[j] = random.Next(bitmapHeight);                
                }
    
                threads.Add(tempThread);

            }
            return threads;
        }
        private void savePixelRGBsToBitmap()
        {
            for (int x = 0; x < bitmapWidth; x++)
            {
                for (int y = 0; y < bitmapHeight; y++)
                {
                    int index = (y * bitmapWidth + x) * 3;

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

            int[] dimensions = { data.Length, bitmapWidth, bitmapHeight };
            randomNoiseAsm(pixelRGBs, data, dimensions, coordinatesArray);
        }

        public void addWhiteNoiseInAssembler(int[] xCoordinates, int[] yCoordinates, int noisePower)
        {
            Random random = new Random();
            int baseLength = xCoordinates.Length;
            int index;
            int j = 0;
            while (baseLength > 0)
            {
                int quantity = baseLength > 4? 4: baseLength ;
                baseLength -= quantity;
                float[] u1u2 = new float[quantity*2];
                for (int i = 0; i < u1u2.Length; i++)
                {
                    if (i < quantity)
                    {
                        u1u2[i] = (float)Math.Log(1.0 - random.NextDouble());
                    }
                    else
                    {
                        u1u2[i] = (float)Math.Cos(2.0 * Math.PI * (1.0 - random.NextDouble()));
                    }
                }
                float[] rgb = new float[quantity*3];
                for (int k = 0; k < quantity; k++)
                {
                    index = (yCoordinates[j + k] * bitmapWidth + xCoordinates[j + k]) * 3;
                    rgb[k] = pixelRGBs[index++];
                    rgb[k + quantity] = pixelRGBs[index++];
                    rgb[k + quantity*2] = pixelRGBs[index];
                }

                whiteNoiseAsm(u1u2, rgb, noisePower);
                for (int k = 0; k < quantity; k++)
                {
                    index = (yCoordinates[j + k] * bitmapWidth + xCoordinates[j + k]) * 3;
                    pixelRGBs[index++] = (byte)Math.Max(0, Math.Min(255, rgb[k]));
                    pixelRGBs[index++] = (byte)Math.Max(0, Math.Min(255, rgb[k + quantity]));
                    pixelRGBs[index] = (byte)Math.Max(0, Math.Min(255, rgb[k + quantity*2]));
                }
                j++;
            }
        }
        public void addColorNoiseInAssembler(int[] xCoordinates, int[] yCoordinates, int noisePower, float[] colorMask)
        {
            Random random = new Random();
            int length = xCoordinates.Length;
            int index;

            for (int j = 0; j < length; j++)
            {
                float[] u1u2 = new float[6];
                for (int i = 0; i < u1u2.Length; i++)
                {
                    if (i < 3)
                    {
                        u1u2[i] = (float)Math.Log(1.0 - random.NextDouble());
                    }
                    else
                    {
                        u1u2[i] = (float)Math.Cos(2.0 * Math.PI * (1.0 - random.NextDouble()));
                    }
                }
                float[] rgb = new float[3];
                index = (yCoordinates[j] * bitmapWidth + xCoordinates[j]) * 3;

                colorNoiseAsm(u1u2, rgb, colorMask, noisePower);
                
                pixelRGBs[index] = (byte)Math.Max(0, Math.Min(255, rgb[0]));
                pixelRGBs[index+1] = (byte)Math.Max(0, Math.Min(255, rgb[1]));
                pixelRGBs[index+2] = (byte)Math.Max(0, Math.Min(255, rgb[2]));

            }
            
        }

    }

}
