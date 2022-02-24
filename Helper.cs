using System;
using System.Diagnostics;

namespace ImageCompression
{
    static class Helper
    {
        public static void PrintMatrix<T>(T[,] matrix)
        {
            string s = "{";
            for (int i = 0; i < matrix.GetLength(0); ++i)
            {
                for (int j = 0; j < matrix.GetLength(1); ++j)
                {
                    s += matrix[i, j] + ", ";
                }
                s = s.Remove(s.Length - 2);
                s += "}\n{";
            }
            s = s.Remove(s.Length - 1);
            Trace.WriteLine(s);
        }

        public static void PrintArray<T>(T[] array)
        {
            string s = "{";
            for (int i = 0; i < array.Length; ++i)
            {
                s += $"{array[i]},";
            }
            s = s.Remove(s.Length - 2);
            s += "}";
            Trace.WriteLine(s);
        }

        public static float[,] ConvertByteMatrixToFloat(byte[,] arr)
        {
            float[,] ret = new float[arr.GetLength(0), arr.GetLength(1)];
            for (int i = 0; i < arr.GetLength(0); ++i)
            {
                for (int j = 0; j < arr.GetLength(1); ++j)
                {
                    ret[i, j] = arr[i, j];
                }
            }
            return ret;
        }

        public static byte[,] ConvertFloatMatrixToByte(float[,] arr)
        {
            byte[,] ret = new byte[arr.GetLength(0), arr.GetLength(1)];
            for (int i = 0; i < arr.GetLength(0); ++i)
            {
                for (int j = 0; j < arr.GetLength(1); ++j)
                {
                    ret[i, j] = Math.Clamp((byte)arr[i, j], byte.MinValue, byte.MaxValue);
                }
            }
            return ret;
        }

        public static float[,] QuantizeChrominance(float[,] arr)
        {
            float[,] ret = new float[arr.GetLength(0), arr.GetLength(1)];
            for (int i = 0; i < arr.GetLength(0); ++i)
            {
                for (int j = 0; j < arr.GetLength(1); ++j)
                {
                    ret[i, j] = MathF.Round(arr[i, j] / Constants.Q_CHROMINANCE[i, j]);
                }
            }
            return ret;
        }

        public static float[,] QuantizeLuminosity(float[,] arr)
        {
            float[,] ret = new float[arr.GetLength(0), arr.GetLength(1)];
            for (int i = 0; i < arr.GetLength(0); ++i)
            {
                for (int j = 0; j < arr.GetLength(1); ++j)
                {
                    ret[i, j] = MathF.Round(arr[i, j] / Constants.Q_LUMINOSITY[i, j]);
                }
            }
            return ret;
        }

        public static float[,] DeQuantizeChrominance(float[,] arr)
        {
            float[,] ret = new float[arr.GetLength(0), arr.GetLength(1)];
            for (int i = 0; i < arr.GetLength(0); ++i)
            {
                for (int j = 0; j < arr.GetLength(1); ++j)
                {
                    ret[i, j] = MathF.Round(arr[i, j] * Constants.Q_CHROMINANCE[i, j]);
                }
            }
            return ret;
        }

        public static float[,] DeQuantizeLuminosity(float[,] arr)
        {
            float[,] ret = new float[arr.GetLength(0), arr.GetLength(1)];
            for (int i = 0; i < arr.GetLength(0); ++i)
            {
                for (int j = 0; j < arr.GetLength(1); ++j)
                {
                    ret[i, j] = MathF.Round(arr[i, j] * Constants.Q_LUMINOSITY[i, j]);
                }
            }
            return ret;
        }

        /*public static int[,] KernelProcessing(int[,] image, int[,] kernel)
        {
            int[,] result = new T[image.GetLength(0), image.GetLength(1)];
            for (int i = 0; i < image.GetLength(0); ++i)
            {
                result[i, 0] = image[i, 0];
                result[i, image.GetLength(1) - 1] = image[i, image.GetLength(1) - 1];
            }
            for (int i = 0; i < image.GetLength(1); ++i)
            {
                result[0, i] = image[0, i];
                result[image.GetLength(0) - 1, i] = image[image.GetLength(0) - 1, i];
            }
            int accumulator;
            int kernelX = kernel.GetLength(0) / 2;
            int kernelY = kernel.GetLength(1) / 2;
            for (int i = kernelX; i < image.GetLength(0) - kernelX; ++i)
            {
                for (int j = kernelY; j < image.GetLength(1) - kernelY; ++j)
                {
                    accumulator = 0;
                    for (int x = 0; x < kernel.GetLength(0); ++x)
                    {
                        for (int y = 0; y < kernel.GetLength(1); ++y)
                        {
                            int offsetX = x - kernelX;
                            int offsetY = y - kernelY;
                            accumulator += kernel[x, y] * image[i - offsetX, j - offsetY];
                        }
                    }
                    result[i, j] = accumulator;
                }
            }
            return result;
        }*/
    }
}

