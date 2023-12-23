using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JaProj
{
    class Program
    {
        [DllImport(@"C:\Users\pawel\source\repos\AddNoiseProject\JaProj\x64\Debug\JAAsm.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void whiteNoiseAsm();
        [DllImport(@"C:\Users\pawel\source\repos\AddNoiseProject\JaProj\x64\Debug\JAAsm.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void randomNoiseAsm(byte[] pixelRGBs, byte[] data, int[] lenWidthHeight, int[] coordinatesArray);

        static void Main(string[] args)
        {
            Random random = new Random();
            int width = 5;
            int height = 5;
            byte[] pixelRGBs = new byte[width* height* 3];
            byte[] testPixelRGBs = new byte[width * height * 3];
            int[] xCoordinates = new int[] { 1, 0, 1, 0 };
            int[] yCoordinates = new int[] { 1, 1, 0, 0 };


            int values = yCoordinates.Length * 3;
            byte[] data = new byte[values];

            for (int i = 0; i < values; i++)
            {
                data[i] = (byte)random.Next(256);
                Console.Write(data[i] + ", ");
            }

            Console.WriteLine();
            Console.WriteLine();

            int[] coordinatesArray = new int[yCoordinates.Length * 2];
            int ctr = 0;
            int c = 0;

            for (int i = 0; i < testPixelRGBs.Length; i++)
            {
                pixelRGBs[i] = 1;
                testPixelRGBs[i] = 1;
            }

            for (int i = 0; i< xCoordinates.Length; i++)
            {
                coordinatesArray[c++] = xCoordinates[i];
                coordinatesArray[c++] = yCoordinates[i];
                int index = (yCoordinates[i] * width + xCoordinates[i]) * 3;
                testPixelRGBs[index] = data[ctr++];
                testPixelRGBs[index+1] = data[ctr++];
                testPixelRGBs[index+2] = data[ctr++];
            }


            int[] dimensions = {data.Length, width, height };
            randomNoiseAsm(pixelRGBs, data, dimensions, coordinatesArray);

            for (int i = 0; i < height; i++)
            {
                for (int k = 0; k < width * 3; k++)
                {
                    string formattedText = $"{pixelRGBs[i * width + k],-4}";
                    Console.Write(formattedText);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            for (int i = 0; i < height; i++)
            {
                for (int k = 0; k < width * 3; k++)
                {
                    string formattedText = $"{testPixelRGBs[i * width + k],-4}";
                    Console.Write(formattedText);
                }
                Console.WriteLine();
            }


            int counter = 0;
            for(int i = 0; i < testPixelRGBs.Length; i++)
            {
                if (testPixelRGBs[i] != pixelRGBs[i]) counter++;
            }
            Console.WriteLine("Counter = " + counter);
            
            Console.ReadLine();
            
        }


    }
}



