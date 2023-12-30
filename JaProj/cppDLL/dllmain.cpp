#define _USE_MATH_DEFINES
#include "pch.h"
#include <windows.h>
#include <cstdlib>
#include <cmath>
#include <random>
#define M_PI 3.14159265358979323846
#using <System.dll>

using namespace System;
using namespace System::Collections::Generic;

extern "C" __declspec(dllexport) void test(double* x) {
    (*x) = log(*x);
}

std::random_device rd;
std::mt19937 gen(rd());
std::uniform_int_distribution<int> distribution(0, 255);


extern "C" __declspec(dllexport) void addRandomNoiseInCpp(
    unsigned char* pixelRGBs,
    int width,
    int height,
    int* xCoordinates,
    int* yCoordinates,
    int count)
{
    srand(static_cast<unsigned int>(time(nullptr)));

    for (int i = 0; i < count; i++)
    {
        int index = (yCoordinates[i] * width + xCoordinates[i]) * 3;

        unsigned char newRed = static_cast<unsigned char>(distribution(gen));
        unsigned char newGreen = static_cast<unsigned char>(distribution(gen));
        unsigned char newBlue = static_cast<unsigned char>(distribution(gen));

        pixelRGBs[index] = newRed;
        pixelRGBs[index + 1] = newGreen;
        pixelRGBs[index + 2] = newBlue;
    }
}

extern "C" __declspec(dllexport)
void addWhiteNoiseInCpp(int* xCoordinates, int* yCoordinates, int noisePower, unsigned char* pixelRGBs, int bitmapWidth, int length)
{
    srand(static_cast<unsigned int>(time(nullptr)));

    for (int j = 0; j < length; j++)
    {
        int index = (yCoordinates[j] * bitmapWidth + xCoordinates[j]) * 3;
        
        double u1 = 1.0 - static_cast<double>(rand()) / RAND_MAX;
        double u2 = 1.0 - static_cast<double>(rand()) / RAND_MAX;
        double z0 = sqrt(-2.0 * log(u1)) * cos(2.0 * M_PI * u2);
        
        for (int i = 0; i < 3; i++) 
        {
            double noise = noisePower * z0;
            int newValue = static_cast<int>(pixelRGBs[index + i] + noise);

            newValue = Math::Max(0, Math::Min(255, newValue));
            pixelRGBs[index + i] = static_cast<unsigned char>(newValue);
        }
    }
}


extern "C" __declspec(dllexport)
void addColorNoiseInCpp(int* xCoordinates, int* yCoordinates, int noisePower, unsigned char* pixelRGBs, int bitmapWidth, int length, float* colorMask)
{
    srand(static_cast<unsigned int>(time(nullptr)));

    for (int j = 0; j < length; j++)
    {
        int index = (yCoordinates[j] * bitmapWidth + xCoordinates[j]) * 3;

        double z[3];
        for (int i = 0; i < 3; i++)
        {
            double u1 = 1.0 - static_cast<double>(rand()) / RAND_MAX;
            double u2 = 1.0 - static_cast<double>(rand()) / RAND_MAX;
            z[i] = sqrt(-2.0 * log(u1)) * cos(2.0 * M_PI * u2);
        }

        for (int i = 0; i < 3; i++)
        {
            double noise = noisePower * z[i];
            int baseColor = colorMask[i];
            int newValue = static_cast<int>(baseColor + noise);

            newValue = Math::Max(0, Math::Min(255, newValue));

            pixelRGBs[index + i] = static_cast<unsigned char>(newValue);
        }
    }
}
