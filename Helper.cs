using System;
using System.Collections.Generic;
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
                s += $"{array[i]}, ";
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

        public static byte[,] QuantizeChrominance(float[,] arr)
        {
            byte[,] ret = new byte[arr.GetLength(0), arr.GetLength(1)];
            for (int i = 0; i < arr.GetLength(0); ++i)
            {
                for (int j = 0; j < arr.GetLength(1); ++j)
                {
                    ret[i, j] = Math.Clamp((byte)MathF.Round(arr[i, j] / Constants.Q_CHROMINANCE[i, j]), byte.MinValue, byte.MaxValue);
                }
            }
            return ret;
        }

        public static byte[,] QuantizeLuminosity(float[,] arr)
        {
            byte[,] ret = new byte[arr.GetLength(0), arr.GetLength(1)];
            for (int i = 0; i < arr.GetLength(0); ++i)
            {
                for (int j = 0; j < arr.GetLength(1); ++j)
                {
                    ret[i, j] = Math.Clamp((byte)MathF.Round(arr[i, j] / Constants.Q_LUMINOSITY[i, j]), byte.MinValue, byte.MaxValue);
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

        public static T[] Mogarithm<T>(T[,] subset)
        {
            List<T> result = new();
            for (int i = 0; i < subset.GetLength(0) * 2; ++i)
            {
                List<T> temp = new();
                int index = i > subset.GetLength(0) - 1 ? subset.GetLength(0) - 1 : i;
                for (int j = index; j >= 0; --j)
                {
                    if (i - j >= subset.GetLength(0)) continue;
                    temp.Add(subset[j, i - j]);
                }
                if (i % 2 == 0) temp.Reverse();
                result.AddRange(temp);
            }
            return result.ToArray();
        }

        /*public static T[,] InverseMogarithm<T>(T[] subset)
        {
            T[,] ret = new T[Constants.MATRIX_SIZE, Constants.MATRIX_SIZE];
            for (int i = 0; i < subset.Length * 2; ++i)
            {
                int index = i > subset.Length - 1 ? subset.Length - 1 : i;
                T[] temp = new T[index];
                for (int j = index; j >= 0; --j)
                {
                    if (i - j >= ret.GetLength(0)) continue;
                    ret[j, i - j] = subset[index];
                }
                for (int j = index; j >= 0; --j)
                {
                    if (i - j >= ret.GetLength(0)) continue;
                    ret[j, i - j] = subset[index];
                }
                if (i % 2 == 0) ReverseRange()
            }
        }*/

        public static void ReverseRange<T>(T[] arr, int offset, int range)
        {
            for (int i = offset; i < (offset + range) / 2 && i < arr.Length; ++i)
            {
                T temp1 = arr[i];
                int temp2 = offset + range - i;
                arr[i] = arr[temp2];
                arr[temp2] = temp1;
            }
        }

        public static T[,] JaggedToDimension<T>(T[][] jagged)
        {
            T[,] ret = new T[jagged.Length, jagged[0].Length];
            for (int i = 0; i < jagged.Length; ++i)
            {
                for (int j = 0; j < jagged[0].Length; ++j)
                {
                    ret[i, j] = jagged[i][j];
                }
            }
            return ret;
        }

        public static T[] ConvertBack<T>(T[,] data)
        {
            List<T> bytes = new();
            for (int i = 0; i < data.GetLength(0); ++i)
            {
                for (int j = 0; j < data.GetLength(1); ++j)
                {
                    bytes.Add(data[i, j]);
                }
            }
            return bytes.ToArray();
        }

        public static List<List<T[,]>> CreateSubsets<T>(T[,] img)
        {
            int width = img.GetLength(0);
            int height = img.GetLength(1);
            List<List<T[,]>> subsets = new(width / Constants.MATRIX_SIZE);
            for (int i = 0; i < width; i += Constants.MATRIX_SIZE)
            {
                subsets.Add(new List<T[,]>());
                for (int j = 0; j < height; j += Constants.MATRIX_SIZE)
                {
                    subsets[i / Constants.MATRIX_SIZE].Add(GetSubset(img, i, j));
                }
            }
            return subsets;
        }

        private static T[,] GetSubset<T>(T[,] img, int offsetX, int offsetY)
        {
            T[,] subset = new T[Constants.MATRIX_SIZE, Constants.MATRIX_SIZE];
            for (int i = 0; i < subset.GetLength(0); ++i)
            {
                for (int j = 0; j < subset.GetLength(1); ++j)
                {
                    int adjusterX = img.GetLength(0) - offsetX + i;
                    int adjusterY = img.GetLength(1) - offsetY + j;
                    subset[i, j] = img[
                        offsetX + i < img.GetLength(0) ? offsetX + i : img.GetLength(0) - adjusterX,
                        offsetY + j < img.GetLength(1) ? offsetY + j : img.GetLength(1) - adjusterY
                        ];
                }
            }
            return subset;
        }

        public static T[,] ReassembleSubsets<T>(List<List<T[,]>> subsets)
        {
            T[,] img = new T[subsets.Count * Constants.MATRIX_SIZE, subsets[0].Count * Constants.MATRIX_SIZE];
            for (int i = 0; i < subsets.Count; ++i)
            {
                for (int j = 0; j < subsets[i].Count; ++j)
                {
                    for (int k = 0; k < subsets[i][j].GetLength(0); ++k)
                    {
                        for (int h = 0; h < subsets[i][j].GetLength(1); ++h)
                        {
                            img[i * Constants.MATRIX_SIZE + k, j * Constants.MATRIX_SIZE + h] = subsets[i][j][k, h];
                        }
                    }
                }
            }
            return img;
        }

        public static byte[] MRLE(byte[] b)
        {
            List<byte> compressed = new List<byte>();
            int n = b.Length;
            for (int i = 0; i < n; ++i)
            {
                int count = 1;
                while (i < n - 1 && b[i] == b[i + 1] && count < byte.MaxValue)
                {
                    ++count;
                    ++i;
                }
                if (count > 2 || b[i] == Constants.KEY)
                {
                    compressed.Add(Constants.KEY);
                    compressed.Add((byte)count);
                    compressed.Add(b[i]);
                }
                else
                {
                    for (int j = 0; j < count; ++j)
                    {
                        compressed.Add(b[i]);
                    }
                }
            }
            return compressed.ToArray();
        }

        public static byte[] DifferentialEncoding(byte[] b)
        {
            List<byte> diff = new List<byte>();
            int n = b.Length;
            diff.Add(b[0]);
            for (int i = 1; i < n; ++i)
            {
                diff.Add((byte)(b[i - 1] - b[i]));
            }
            return diff.ToArray();
        }

        public static T[,] ArrayToMatrix<T>(T[] buffer, int stride)
        {
            int total = buffer.Length;
            int stridenums = total / stride;
            T[,] data = new T[stridenums, stride];
            for (int y = 0; y < stride; ++y)
            {
                for (int x = 0; x < stridenums; ++x)
                {
                    data[x, y] = buffer[x * stride + y];
                }
            }
            return data;
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

