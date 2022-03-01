using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace ImageCompression
{
    class Compression
    {
        public Compression(Image img)
        {
            //Construct stuff from source
            var src = img.Source as BitmapSource ?? throw new ArgumentNullException("Image does not exist");
            var image = ConvertData(src);
            //Bytes to RGB
            var pixels = RGB.MatrixBufferToRGB(image);
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
            //Pad
            y = Prepad(y, out int yRow, out int yCol);
            cb = Prepad(cb, out int cbRow, out int cbCol);
            cr = Prepad(cr, out int crRow, out int crCol);
            //Create 8x8s
            var ySubsets = Helper.CreateSubsets(y);
            var cbSubsets = Helper.CreateSubsets(cb);
            var crSubsets = Helper.CreateSubsets(cr);
            //DCT 8x8s
            var dctY = DCTNonThreadedRunnerSquared(ySubsets);
            var dctCb = DCTNonThreadedRunnerSquared(cbSubsets);
            var dctCr = DCTNonThreadedRunnerSquared(crSubsets);
            //Quantize
            var bSubY = QuantizeY(dctY);
            var bSubCb = QuantizeC(dctCb);
            var bSubCr = QuantizeC(dctCr);
            //Mogarithm
            var yBytes = MogarithmRunner(bSubY);
            var cbBytes = MogarithmRunner(bSubCb);
            var crBytes = MogarithmRunner(bSubCr);
            //Combine bytes
            List<byte> bytes = new();
            bytes.AddRange(yBytes);
            bytes.AddRange(cbBytes);
            bytes.AddRange(crBytes);
            //compress
            var compressed = Helper.MRLE(Helper.DifferentialEncoding(bytes.ToArray()));
            Trace.WriteLine(compressed.Length);
            /*WriteableBitmap bmp = new(img.Source as BitmapSource);
            bmp.WritePixels(
                new System.Windows.Int32Rect(0, 0, src.PixelWidth, src.PixelHeight),
                buffer,
                src.PixelWidth * src.Format.BitsPerPixel / 8,
                0);
            img.Source = bmp;*/
            GC.Collect();
        }

        public static byte[] MogarithmRunner(List<List<byte[,]>> subsets)
        {
            List<byte> ret = new();
            for (int i = 0; i < subsets.Count; ++i)
            {
                for (int j = 0; j < subsets[i].Count; ++j)
                {
                    ret.AddRange(Helper.Mogarithm(subsets[i][j]));
                }
            }
            return ret.ToArray();
        }

        public static float[,] Unsample(float[,] arr)
        {
            float[,] ret = new float[arr.GetLength(0) * 2, arr.GetLength(1) * 2];
            for (int i = 0; i < ret.GetLength(0) - 1; i += 2)
            {
                for (int j = 0; j < ret.GetLength(1) - 1; j += 2)
                {
                    ret[i, j] = arr[i / 2, j / 2];
                    ret[i + 1, j + 1] = arr[i / 2, j / 2];
                    ret[i, j + 1] = arr[i / 2, j / 2];
                    ret[i + 1, j] = arr[i / 2, j / 2];
                }
            }
            return ret;
        }

        public static float[,] Unpad(float[,] arr, int rowDivis, int colDivis)
        {
            if (rowDivis == 0 && colDivis == 0)
            {
                Trace.WriteLine("im a funcking moron");
                return arr;
            }
            float[,] ret = new float[arr.GetLength(0) - rowDivis, arr.GetLength(1) - colDivis];
            for (int i = 0; i < ret.GetLength(0); ++i)
            {
                for (int j = 0; j < ret.GetLength(1); ++j)
                {
                    ret[i, j] = arr[i, j];
                }
            }
            return ret;
        }

        public static float[,] Prepad(float[,] arr, out int rowDivis, out int colDivis)
        {
            rowDivis = 0;
            colDivis = 0;
            while ((arr.GetLength(0) + rowDivis) % Constants.MATRIX_SIZE != 0)
            {
                ++rowDivis;
            }
            while ((arr.GetLength(1) + colDivis) % Constants.MATRIX_SIZE != 0)
            {
                ++colDivis;
            }
            float[,] ret = new float[arr.GetLength(0) + rowDivis, arr.GetLength(1) + colDivis];
            for (int i = 0; i < arr.GetLength(0); ++i)
            {
                for (int j = 0; j < arr.GetLength(1); ++j)
                {
                    ret[i, j] = arr[i, j];
                }
            }
            return ret;
        }

        private List<List<byte[,]>> QuantizeY(List<List<float[,]>> y)
        {
            List<List<byte[,]>> ret = new();
            for (int i = 0; i < y.Count; ++i)
            {
                ret.Add(new List<byte[,]>());
                for (int j = 0; j < y[i].Count; ++j)
                {
                    ret[i].Add(new byte[0, 0]);
                    ret[i][j] = Helper.QuantizeLuminosity(y[i][j]);
                }
            }
            return ret;
        }

        private List<List<byte[,]>> QuantizeC(List<List<float[,]>> c)
        {
            List<List<byte[,]>> ret = new();
            for (int i = 0; i < c.Count; ++i)
            {
                ret.Add(new List<byte[,]>());
                for (int j = 0; j < c[i].Count; ++j)
                {
                    ret[i].Add(new byte[0, 0]);
                    ret[i][j] = Helper.QuantizeChrominance(c[i][j]);
                }
            }
            return ret;
        }

        private void DeQuantizeY(ref List<List<float[,]>> y)
        {
            for (int i = 0; i < y.Count; ++i)
            {
                for (int j = 0; j < y[i].Count; ++j)
                {
                    y[i][j] = Helper.DeQuantizeLuminosity(y[i][j]);
                }
            }
        }

        private void DeQuantizeC(ref List<List<float[,]>> c)
        {
            for (int i = 0; i < c.Count; ++i)
            {
                for (int j = 0; j < c[i].Count; ++j)
                {
                    c[i][j] = Helper.DeQuantizeChrominance(c[i][j]);
                }
            }
        }

        private List<List<float[,]>> DCTRunnerSquared(List<List<float[,]>> img)
        {
            Trace.WriteLine("DCT Start");
            List<List<float[,]>> returnable = new();
            foreach (var subset in img)
            {
                returnable.Add(DCTRunner(subset));
            }
            Trace.WriteLine("DCT End");
            return returnable;
        }

        private List<float[,]> DCTRunner(List<float[,]> img)
        {
            int tasks = img.Count() / Constants.MAX_THREADS;
            List<float[,]> result = new();
            List<Task<List<float[,]>>> threads = new();
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
            return result;
        }

        private List<List<float[,]>> DCTNonThreadedRunnerSquared(List<List<float[,]>> img)
        {
            List<List<float[,]>> ret = new();
            foreach (var list in img)
            {
                ret.Add(DCTNonThreadedRunner(list));
            }
            return ret;
        }

        private List<float[,]> DCTNonThreadedRunner(List<float[,]> img)
        {
            List<float[,]> ret = new();
            foreach (var subset in img)
            {
                ret.Add(DCT(subset));
            }
            return ret;
        }

        private List<List<float[,]>> IDCTRunnerSquared(List<List<float[,]>> img)
        {
            Trace.WriteLine("IDCT Start");
            List<List<float[,]>> returnable = new();
            foreach (var subset in img)
            {
                returnable.Add(IDCTRunner(subset));
            }
            Trace.WriteLine("IDCT End");
            return returnable;
        }

        private List<float[,]> IDCTRunner(List<float[,]> img)
        {
            int tasks = img.Count() / Constants.MAX_THREADS;
            List<float[,]> result = new();
            List<Task<List<float[,]>>> threads = new();
            for (int t = 0; t < Constants.MAX_THREADS; ++t)
            {
                threads.Add(Task.Factory.StartNew(() =>
                {
                    List<float[,]> returnable = new();
                    for (int i = 0; i < tasks; ++i)
                    {
                        returnable.Add(InverseDCT(img[t * tasks + i]));
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
            Trace.WriteLine(buffer.Length);
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

        private static float FloatRound(float f)
        {
            float r = f < 0 ? (int)f - 0.5f : (int)f + 0.5f;
            return r == -0 ? 0 : r;
        }
    }
}
