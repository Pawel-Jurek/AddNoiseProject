#include <cstddef>
#include <bitset>

#using <System.dll>

using namespace System;
using namespace System::Collections::Generic;
using std::byte;

extern "C" __declspec(dllexport) void addRandomNoiseInCSharp(array<byte> ^ pixelRGBs, int width, int height, List<KeyValuePair<int, int>> ^ listOfCoordinates)
{
    Random^ random = gcnew Random();

    for each (KeyValuePair<int, int> coordinates in listOfCoordinates)
    {
        int index = (coordinates.Value * width + coordinates.Key) * 3;

        byte newRed = (byte)random->Next(256);
        byte newGreen = (byte)random->Next(256);
        byte newBlue = (byte)random->Next(256);

        pixelRGBs[index] = newRed;
        pixelRGBs[index + 1] = newGreen;
        pixelRGBs[index + 2] = newBlue;
    }
}

extern "C" __declspec(dllexport) void addWhiteNoiseInCSharp(array<byte> ^ pixelRGBs, int width, int height, List<KeyValuePair<int, int>> ^ pixelsCoordinates, int noisePower)
{
    Random^ random = gcnew Random();

    for each (KeyValuePair<int, int> coordinates in pixelsCoordinates)
    {
        int index = (coordinates.Value * width + coordinates.Key) * 3;

        double u1 = 1.0 - random->NextDouble();
        double u2 = 1.0 - random->NextDouble();
        double z0 = Math::Sqrt(-2.0 * Math::Log(u1)) * Math::Cos(2.0 * Math::PI * u2);

        for (int i = 0; i < 3; i++)
        {
            double noise = noisePower * z0;
            int newValue = (int)(pixelRGBs[index + i] + noise);

            newValue = Math::Max(0, Math::Min(255, newValue));
            pixelRGBs[index + i] = (byte)newValue;
        }
    }
}

extern "C" __declspec(dllexport) void addColorNoiseInCSharp(array<byte> ^ pixelRGBs, int width, int height, List<KeyValuePair<int, int>> ^ pixelsCoordinates, int noisePower)
{
    Random^ random = gcnew Random();

    for each (KeyValuePair<int, int> coordinates in pixelsCoordinates)
    {
        int index = (coordinates.Value * width + coordinates.Key) * 3;

        array<double>^ z = gcnew array<double>(3);
        for (int i = 0; i < 3; i++)
        {
            double u1 = 1.0 - random->NextDouble();
            double u2 = 1.0 - random->NextDouble();
            z[i] = Math::Sqrt(-2.0 * Math::Log(u1)) * Math::Cos(2.0 * Math::PI * u2);
        }

        for (int i = 0; i < 3; i++)
        {
            double noise = noisePower * z[i];
            int newValue = (int)(pixelRGBs[index + i] + noise);

            newValue = Math::Max(0, Math::Min(255, newValue));
            pixelRGBs[index + i] = (byte)newValue;
        }
    }
}
