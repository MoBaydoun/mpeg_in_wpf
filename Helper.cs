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
            Debug.WriteLine(s);
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
            Debug.WriteLine(s);
        }

        public static List<List<T[,]>> ConvertJaggedMatrixToList<T>(T[,][,] arr)
        {
            List<List<T[,]>> ret = new();
            for (int i = 0; i < arr.GetLength(0); ++i)
            {
                ret.Add(new List<T[,]>());
                for (int j = 0; j < arr.GetLength(1); ++j)
                {
                    ret[i].Add(arr[i, j]);
                }
            }
            return ret;
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
                    ret[i, j] = arr[i, j] * Constants.Q_CHROMINANCE[i, j];
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
                    ret[i, j] = arr[i, j] * Constants.Q_LUMINOSITY[i, j];
                }
            }
            return ret;
        }

        public static byte[] Mogarithm(float[,] subset)
        {
            List<byte> result = new();
            for (int i = 0; i < subset.GetLength(0) * 2; ++i)
            {
                List<byte> temp = new();
                int index = i > subset.GetLength(0) - 1 ? subset.GetLength(0) - 1 : i;
                for (int j = index; j >= 0; --j)
                {
                    if (i - j >= subset.GetLength(0)) continue;
                    temp.Add((byte)(subset[j, i - j] + sbyte.MaxValue + 1));
                }
                if (i % 2 == 0) temp.Reverse();
                result.AddRange(temp);
            }
            return result.ToArray();
        }

        public static float[,] InversentMogarithm(byte[] s)
        {
            return new float[8, 8]
            {
                {s[0], s[2], s[3], s[9], s[10], s[20], s[21], s[35]},
                {s[1], s[4], s[8], s[11], s[19], s[22], s[34], s[36]},
                {s[5], s[7], s[12], s[18], s[23], s[33], s[37], s[48]},
                {s[6], s[13], s[17], s[24], s[32], s[38], s[47], s[49]},
                {s[14], s[16], s[25], s[31], s[39], s[46], s[50], s[57]},
                {s[15], s[26], s[30], s[40], s[45], s[51], s[56], s[58]},
                {s[27], s[29], s[41], s[44], s[52], s[55], s[59], s[62]},
                {s[28], s[42], s[43], s[53], s[54], s[60], s[61], s[63]}
            };
        }

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

        public static T[] MatrixToArray<T>(T[,] data)
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

        public static T[,] GetSubset<T>(T[,] img, int offsetX, int offsetY)
        {
            T[,] subset = new T[Constants.MATRIX_SIZE, Constants.MATRIX_SIZE];
            for (int i = 0; i < subset.GetLength(0); ++i)
            {
                for (int j = 0; j < subset.GetLength(1); ++j)
                {
                    subset[i, j] = img[offsetX + i, offsetY + j];
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

        public static float[,] SubtractMatrix(float[,] m1, float[,] m2)
        {
            Debug.Assert(m1.GetLength(0) == m2.GetLength(0) && m1.GetLength(1) == m2.GetLength(1));
            float[,] ret = new float[m1.GetLength(0), m2.GetLength(1)];
            for (int i = 0; i < m1.GetLength(0); ++i)
            {
                for (int j = 0; j < m2.GetLength(1); ++j)
                {
                    ret[i, j] = m1[i, j] - m2[i, j];
                }
            }
            return ret;
        }

        public static float[,] AddMatrix(float[,] m1, float[,] m2)
        {
            Debug.Assert(m1.GetLength(0) == m2.GetLength(0) && m1.GetLength(1) == m2.GetLength(1));
            float[,] ret = new float[m1.GetLength(0), m2.GetLength(1)];
            for (int i = 0; i < m1.GetLength(0); ++i)
            {
                for (int j = 0; j < m2.GetLength(1); ++j)
                {
                    ret[i, j] = m1[i, j] + m2[i, j];
                }
            }
            return ret;
        }

        public static void SaveWidthHeight<T>(List<List<T[,]>> subsets, out int width, out int height)
        {
            height = subsets.Count;
            width = subsets[0].Count;
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

        public static byte[] Decompress(byte[] b)
        {
            List<byte> decompress = new List<byte>();
            int n = b.Length;
            for (int i = 0; i < n; ++i)
            {
                if (b[i] == Constants.KEY)
                {
                    int size = b[i + 1];
                    i += 2;
                    for (int j = 0; j < size; ++j)
                    {
                        decompress.Add(b[i]);
                    }
                }
                else
                {
                    decompress.Add(b[i]);
                }
            }
            return decompress.ToArray();
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

        public static byte[] ReverseDifferentialEncoding(byte[] b)
        {
            List<byte> rediff = new List<byte>();
            int n = b.Length;
            rediff.Add(b[0]);
            for (int i = 1; i < n - 1; ++i)
            {
                rediff.Add((byte)(rediff[i - 1] - b[i]));
            }
            return rediff.ToArray();
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

