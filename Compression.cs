using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace ImageCompression
{

    /// <summary>
    /// DCT:
    /// H(u, v) = C(u) || C(v) * 2 / N loop rows loop columns cos(pi(2*x+1)/2N) * cos(pi(2*y+1)/2N) * h(x, y)
    /// what is C???
    /// C = if arg is anything other than 0, C = 1, otherwise C = 1/root2
    /// </summary>

    class Compression
    {

        public RGB[,]? pixels { get; private set; }
        public int stride { get; private set; }
        public Compression(Image img)
        {
            var src = img.Source as BitmapSource ?? throw new ArgumentNullException("Image does not exist");
            var image = ConvertData(src);
            //Bytes to pixels
            pixels = RGB.MatrixBufferToRGB(image);
            //RGB to YCbCr
            YCBCR[,] YCbCrPixels = YCBCR.RGBMatrixToYCBCRMatrix(pixels);
            //Seperate channels
            float[,] y;
            float[,] cb;
            float[,] cr;
            YCBCR.DeconstructYCBCR(YCbCrPixels, out y, out cb, out cr);
            //Subsample cb and cr
            cb = SubSample(cb);
            cr = SubSample(cr);
            //Create 8x8s
            var ySubsets = CreateSubsets(y);
            var cbSubsets = CreateSubsets(cb);
            var crSubsets = CreateSubsets(cr);
            //DCT 8x8s
            var dctY = DCTRunnerSquared(ySubsets);
            var dctCb = DCTRunnerSquared(cbSubsets);
            var dctCr = DCTRunnerSquared(crSubsets);
            //Quantize
            QuantizeY(ref dctY);
            QuantizeC(ref dctCb);
            QuantizeC(ref dctCr);



            WriteableBitmap bmp = new(img.Source as BitmapSource);
            /*bmp.WritePixels(
                new System.Windows.Int32Rect(0, 0, src.PixelWidth, src.PixelHeight),
                changed,
                src.PixelWidth * src.Format.BitsPerPixel / 8,
                0);*/
            img.Source = bmp;
        }

        private void QuantizeY(ref List<List<float[,]>> y)
        {
            for (int i = 0; i < y.Count; ++i)
            {
                for (int j = 0; j < y[i].Count; ++j)
                {
                    y[i][j] = Helper.QuantizeLuminosity(y[i][j]);
                }
            }
        }

        private void QuantizeC(ref List<List<float[,]>> c)
        {
            for (int i = 0; i < c.Count; ++i)
            {
                for (int j = 0; j < c[i].Count; ++j)
                {
                    c[i][j] = Helper.QuantizeChrominance(c[i][j]);
                }
            }
        }

        public static List<List<float[,]>> CreateSubsets(float[,] img)
        {
            int width = img.GetLength(0);
            int height = img.GetLength(1);
            List<List<float[,]>> subsets = new(width / Constants.MATRIX_SIZE);
            for (int i = 0; i < width; i += Constants.MATRIX_SIZE)
            {
                subsets.Add(new List<float[,]>());
                for (int j = 0; j < height; j += Constants.MATRIX_SIZE)
                {
                    subsets[i / Constants.MATRIX_SIZE].Add(PadSubset(GetSubset(img, i, j)));
                }
            }
            return subsets;
        }

        public static float[,] ReassembleSubsets(List<List<float[,]>> subsets)
        {
            float[,] img = new float[subsets.Count * Constants.MATRIX_SIZE, subsets[0].Count * Constants.MATRIX_SIZE];
            for (int i = 0; i < subsets.Count; ++i)
            {
                for (int j = 0; j < subsets[i].Count; ++j)
                {
                    for (int k = 0; k < subsets[i][j].GetLength(0); ++k)
                    {
                        for (int h = 0; h < subsets[i][j].GetLength(1); ++h)
                        {
                            img[k * Constants.MATRIX_SIZE + i, h * Constants.MATRIX_SIZE + j] = subsets[i][j][k, h];
                        }
                    }
                }
            }
            return img;
        }

        private static float[,] PadSubset(float[,] faulty)
        {
            if (faulty.GetLength(0) == Constants.MATRIX_SIZE && faulty.GetLength(1) == Constants.MATRIX_SIZE) return faulty;
            float[,] subset = new float[Constants.MATRIX_SIZE, Constants.MATRIX_SIZE];
            for (int i = 0; i < faulty.GetLength(0); ++i)
            {
                for (int j = 0; j < faulty.GetLength(1); ++j)
                {
                    subset[i, j] = faulty[i, j];
                }
            }
            return subset;
        }

        private static float[,] GetSubset(float[,] img, int offsetX, int offsetY)
        {
            float[,] subset = new float[Constants.MATRIX_SIZE, Constants.MATRIX_SIZE];
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

        private List<List<float[,]>> DCTRunnerSquared(List<List<float[,]>> img)
        {
            List<List<float[,]>> returnable = new();
            foreach (var subset in img)
            {
                returnable.Add(DCTRunner(subset));
            }
            return returnable;
        }

        private List<float[,]> DCTRunner(List<float[,]> img)
        {
            int tasks = img.Count() / Constants.MAX_THREADS;
            List<float[,]> result = new();
            List<Task<List<float[,]>>> threads = new();
            Stopwatch w = new();
            w.Start();
            for (int t = 0; t < Constants.MAX_THREADS; ++t)
            {
                threads.Add(Task.Factory.StartNew(() =>
                {
                    List<float[,]> returnable = new();
                    for (int i = 0; i < tasks; ++i)
                    {
                        returnable.Add(DCT(img[t * tasks + i]));
                        if (i == tasks - 1) break;
                    }
                    return returnable;
                }));
                if (t == Constants.MAX_THREADS - 1) break;
            }
            Task.WaitAll(threads.ToArray());
            for (int t = 0; t < Constants.MAX_THREADS; ++t)
            {
                for (int i = 0; i < threads[t].Result.Count(); ++i)
                {
                    result.Add(threads[t].Result[i]);
                    if (i == threads[t].Result.Count() - 1) break;
                }
                if (t == Constants.MAX_THREADS - 1) break;
            }
            w.Stop();
            Trace.WriteLine($"Threaded finished: {w.ElapsedMilliseconds}ms");
            return result;
        }

        private static float[,] DCT(float[,] h)
        {
            int width = h.GetLength(0);
            int height = h.GetLength(1);
            float[,] H = new float[width, height];
            float accumulator;
            for (int u = 0; u < width; ++u)
            {
                for (int v = 0; v < height; ++v)
                {
                    accumulator = 0;
                    for (int x = 0; x < width; ++x)
                    {
                        for (int y = 0; y < height; ++y)
                        {
                            accumulator += MathF.Cos((u * MathF.PI * (2 * x + 1)) / (2 * width))
                                * MathF.Cos((v * MathF.PI * (2 * y + 1)) / (2 * height))
                                * h[x, y];
                        }
                    }
                    H[u, v] = FloatRound(accumulator * (2 / MathF.Sqrt(height * width)) * C(u) * C(v));
                }
            }
            return H;
        }

        private static float[,] InverseDCT(float[,] H)
        {
            int width = H.GetLength(0);
            int height = H.GetLength(1);
            float[,] h = new float[width, height];
            float accumulator;
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    accumulator = 0;
                    for (int u = 0; u < width; ++u)
                    {
                        for (int v = 0; v < height; ++v)
                        {
                            accumulator += C(u) * C(v)
                                * MathF.Cos(u * MathF.PI * (2 * x + 1) / (2 * width))
                                * MathF.Cos(v * MathF.PI * (2 * y + 1) / (2 * height))
                                * H[u, v];
                        }
                    }
                    h[x, y] = FloatRound(accumulator * (2 / MathF.Sqrt(height * width)));
                }
            }
            return h;
        }

        private static float C(int num)
        {
            return num == 0 ? (float)(1 / Math.Sqrt(2)) : 1;
        }

        /*public static YCBCR[,] SubSample(YCBCR[,] image)
        {
            YCBCR[,] result = new YCBCR[image.GetLength(0), image.GetLength(1)];
            for (int r = 0; r < image.GetLength(0); ++r)
            {
                for (int c = 0; c < image.GetLength(1); ++c)
                {
                    result[r, c] = new(image[r, c]);
                    if (c % 2 != 0 && r % 2 == 0)
                    {
                        result[r, c].Cb = image[r, c - 1].Cb;
                        result[r, c].Cr = image[r, c - 1].Cr;
                    }
                    else if (r % 2 != 0)
                    {
                        result[r, c].Cb = image[r - 1, c].Cb;
                        result[r, c].Cr = image[r - 1, c].Cr;
                    }
                }
            }
            return result;
        }*/

        public static float[,] SubSample(float[,] pComponent)
        {
            float[,] component = new float[pComponent.GetLength(0) / 2, pComponent.GetLength(1) / 2];
            for (int i = 0; i < pComponent.GetLength(0) - 1; i += 2)
            {
                for (int j = 0; j < pComponent.GetLength(1) - 1; j += 2)
                {
                    component[i / 2, j / 2] = pComponent[i, j];
                }
            }
            return component;
        }

        public static float[,] ConvertData(BitmapSource src)
        {
            int stride = src.PixelWidth * src.Format.BitsPerPixel / 8;
            int total = src.PixelWidth * src.PixelHeight * src.Format.BitsPerPixel / 8;
            int stridenums = total / stride;
            byte[] buffer = new byte[total];
            src.CopyPixels(buffer, stride, 0);
            float[,] data = new float[stridenums, stride];

            for (int y = 0; y < stride; ++y)
            {
                for (int x = 0; x < stridenums; ++x)
                {
                    data[x, y] = buffer[x * stride + y];
                }
            }
            return data;
        }

        private byte[] ConvertBack(byte[,] data)
        {
            List<byte> bytes = new();
            for (int i = 0; i < data.GetLength(0); ++i)
            {
                for (int j = 0; j < data.GetLength(1); ++j)
                {
                    bytes.Add(data[i, j]);
                }
            }
            return bytes.ToArray();
        }

        private static float FloatRound(float f)
        {
            float r = f < 0 ? (int)f - 0.5f : (int)f + 0.5f;
            return r == -0 ? 0 : r;
        }
    }
}
