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
        public static extern void randomNoiseAsm(byte[] pixelRGBs, byte[] data, int[] lenWidthHeight, int[] coordinatesArray);
        static void Main(string[] args)
        {
            Random random = new Random();
            int width = 5;
            int height = 5;
            byte[] pixelRGBs = new byte[width* height* 3];
            byte[] testPixelRGBs = new byte[width * height * 3];
            List<KeyValuePair<int, int>> listOfCoordinates = new List<KeyValuePair<int, int>>();

            listOfCoordinates.Add(new KeyValuePair<int, int>(0, 1));
            listOfCoordinates.Add(new KeyValuePair<int, int>(2, 1));
            listOfCoordinates.Add(new KeyValuePair<int, int>(3, 3));
            listOfCoordinates.Add(new KeyValuePair<int, int>(4, 2));

            
            int values = listOfCoordinates.Count * 3;
            byte[] data = new byte[values];

            for (int i = 0; i < values; i++)
            {
                data[i] = (byte)random.Next(256);
                Console.Write(data[i] + ", ");
            }

            Console.WriteLine();

            int[] coordinatesArray = new int[listOfCoordinates.Count * 2];
            int ctr = 0;
            int c = 0;

            for (int i = 0; i < testPixelRGBs.Length; i++)
            {
                pixelRGBs[i] = 1;
                testPixelRGBs[i] = 1;
            }

            foreach (KeyValuePair<int, int> coordinates in listOfCoordinates)
            {
                coordinatesArray[c++] = coordinates.Key;
                coordinatesArray[c++] = coordinates.Value;
                testPixelRGBs[(coordinates.Value * width + coordinates.Key)*3] = data[ctr++];
                testPixelRGBs[(coordinates.Value * width + coordinates.Key)*3+1] = data[ctr++];
                testPixelRGBs[(coordinates.Value * width + coordinates.Key)*3+2] = data[ctr++];
            }


            int[] dimensions = {data.Length, width, height };
            randomNoiseAsm(pixelRGBs, data, dimensions, coordinatesArray);
      
            foreach(var coords in listOfCoordinates)
            {
                Console.WriteLine($"pixelRGBs[{coords.Key}, {coords.Value}, 0] = {pixelRGBs[(coords.Value * width + coords.Key) * 3]}");
                Console.WriteLine($"pixelRGBs[{coords.Key}, {coords.Value}, 1] = {pixelRGBs[(coords.Value * width + coords.Key) * 3 + 1]}");
                Console.WriteLine($"pixelRGBs[{coords.Key}, {coords.Value}, 2] = {pixelRGBs[(coords.Value * width + coords.Key) * 3 + 2]}");
            }

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



