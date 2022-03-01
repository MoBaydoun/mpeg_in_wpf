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
            Stopwatch bench = new();
            bench.Start();
            //Construct stuff from source
            Debug.WriteLine($"Constructing Data...");
            var src = img.Source as BitmapSource ?? throw new ArgumentNullException("Image does not exist");
            var image = ConvertData(src);
            //Bytes to RGB
            Debug.WriteLine($"Assembling RGB...");
            var pixels = RGB.MatrixBufferToRGB(image);
            //RGB to YCbCr
            Debug.WriteLine($"Converting to YCBCR...");
            YCBCR[,]? YCbCrPixels = YCBCR.RGBMatrixToYCBCRMatrix(pixels);
            //Seperate channels
            Debug.WriteLine($"Seperating YCBCR Channels...");
            YCBCR.DeconstructYCBCR(YCbCrPixels, out float[,]? y, out float[,]? cb, out float[,]? cr);
            YCbCrPixels = null;
            //Subsample cb and cr
            Debug.WriteLine($"Subsampling...");
            cb = SubSample(cb);
            cr = SubSample(cr);
            //Pad
            Debug.WriteLine($"Padding...");
            y = Prepad(y, out int yRow, out int yCol);
            cb = Prepad(cb, out int cbRow, out int cbCol);
            cr = Prepad(cr, out int crRow, out int crCol);
            //Create 8x8s
            Debug.WriteLine($"Creating Subsets...");
            var ySubsets = Helper.CreateSubsets(y);
            var cbSubsets = Helper.CreateSubsets(cb);
            var crSubsets = Helper.CreateSubsets(cr);
            y = null;
            cb = null;
            cr = null;
            //DCT 8x8s
            Debug.WriteLine($"DCT Start...");
            var dctY = DCTNonThreadedRunnerSquared(ySubsets);
            var dctCb = DCTNonThreadedRunnerSquared(cbSubsets);
            var dctCr = DCTNonThreadedRunnerSquared(crSubsets);
            ySubsets = null;
            cbSubsets = null;
            crSubsets = null;
            Debug.WriteLine($"DCT Finished: {bench.ElapsedMilliseconds} ms");
            //Quantize
            Debug.WriteLine($"Quantizing...");
            var bSubY = QuantizeY(dctY);
            var bSubCb = QuantizeC(dctCb);
            var bSubCr = QuantizeC(dctCr);
            dctY = null;
            dctCb = null;
            dctCr = null;
            Helper.SaveWidthHeight(bSubY, out int yWidth, out int yHeight);
            Helper.SaveWidthHeight(bSubCb, out int cbWidth, out int cbHeight);
            Helper.SaveWidthHeight(bSubCr, out int crWidth, out int crHeight);
            //Mogarithm
            Debug.WriteLine($"Mogarithmizing...");
            var yBytes = MogarithmRunner(bSubY);
            var cbBytes = MogarithmRunner(bSubCb);
            var crBytes = MogarithmRunner(bSubCr);
            bSubY = null;
            bSubCb = null;
            bSubCr = null;
            //store length for unpacking
            var yLength = yBytes.Length;
            var cbLength = cbBytes.Length;
            var crLength = crBytes.Length;
            //Combine bytes
            Debug.WriteLine($"Combining Channels...");
            List<byte>? bytes = new();
            bytes.AddRange(yBytes);
            bytes.AddRange(cbBytes);
            bytes.AddRange(crBytes);
            //compress
            Debug.WriteLine($"Compressing...");
            var compressed = Helper.MRLE(bytes.ToArray());
            bytes = null;
            Debug.WriteLine($"Compressed Size: {compressed.Length}");
            //decompress
            Debug.WriteLine($"Decompressing...");
            compressed = Helper.Decompress(compressed);
            var compressedaslist = compressed.ToList();
            compressed = null;
            //unpack
            Debug.WriteLine($"Unpacking Channels");
            yBytes = compressedaslist.GetRange(0, yLength).ToArray();
            cbBytes = compressedaslist.GetRange(yLength, cbLength).ToArray();
            crBytes = compressedaslist.GetRange(yLength + cbLength, crLength).ToArray();
            compressedaslist = null;
            //Demogarithmize
            Debug.WriteLine($"Demogarithmizing...");
            bSubY = Demogarithmizer(yBytes, yWidth, yHeight);
            bSubCr = Demogarithmizer(cbBytes, cbWidth, cbHeight);
            bSubCb = Demogarithmizer(crBytes, crWidth, crHeight);
            yBytes = null;
            cbBytes = null;
            crBytes = null;
            //Unquantize
            Debug.WriteLine($"Unquantizing...");
            var fSubY = DeQuantizeY(bSubY);
            var fSubCb = DeQuantizeC(bSubCb);
            var fSubCr = DeQuantizeC(bSubCr);
            bSubY = null;
            bSubCb = null;
            bSubCr = null;
            //IDCT
            Debug.WriteLine($"IDCT Start...");
            dctY = IDCTNonThreadedRunnerSquared(fSubY);
            dctCb = IDCTNonThreadedRunnerSquared(fSubCb);
            dctCr = IDCTNonThreadedRunnerSquared(fSubCr);
            fSubY = null;
            fSubCb = null;
            fSubCr = null;
            Debug.WriteLine($"IDCT Finished: {bench.ElapsedMilliseconds} ms");
            //Reassemble
            Debug.WriteLine($"Reassembling Subsets...");
            y = Helper.ReassembleSubsets(dctY);
            cb = Helper.ReassembleSubsets(dctCb);
            cr = Helper.ReassembleSubsets(dctCr);
            dctY = null;
            dctCb = null;
            dctCr = null;
            //Unpad
            Debug.WriteLine($"Unpadding...");
            y = Unpad(y, yRow, yCol);
            cb = Unpad(cb, cbRow, cbCol);
            cr = Unpad(cr, crRow, crCol);
            //Unsample
            Debug.WriteLine($"Unsampling...");
            cb = Unsample(cb);
            cr = Unsample(cr);
            //Combine channels
            Debug.WriteLine($"Reconstructing YCBCR...");
            YCbCrPixels = YCBCR.ReconstructYCBCR(y, cb, cr);
            //Writing to buffer
            Debug.WriteLine($"Writing to Buffer...");
            var buffer = Helper.MatrixToArray(YCBCR.YCBCRtoBuffer(YCbCrPixels));
            YCbCrPixels = null;
            WriteableBitmap bmp = new(img.Source as BitmapSource);
            bmp.WritePixels(
                new System.Windows.Int32Rect(0, 0, src.PixelWidth, src.PixelHeight),
                buffer,
                src.PixelWidth * src.Format.BitsPerPixel / 8,
                0);
            img.Source = bmp;
            bench.Stop();
            Debug.WriteLine($"Finished: {bench.ElapsedMilliseconds} ms");
            GC.Collect();
        }

        public static byte[] MogarithmRunner(List<List<float[,]>> subsets)
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

        public static List<List<float[,]>> Demogarithmizer(byte[] channel, int width, int height)
        {
            List<float[,]> temp = new();
            var channellist = channel.ToList();
            for (int i = 0; i < channel.Length; i += 64)
            {
                temp.Add(Helper.InversentMogarithm(channellist.GetRange(i, 64).ToArray()));
            }
            foreach (var subset in temp)
            {
                for (int i = 0; i < subset.GetLength(0); ++i)
                {
                    for (int j = 0; j < subset.GetLength(1); ++j)
                    {
                        subset[i, j] -= 128;
                    }
                }
            }
            List<List<float[,]>> ret = new();
            for (int i = 0; i < height; ++i)
            {
                ret.Add(new List<float[,]>());
                for (int j = 0; j < width; ++j)
                {
                    ret[i].Add(new float[0, 0]);
                    ret[i][j] = temp[j * width + i];
                }
            }
            return ret;
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
            if (rowDivis == 0 && colDivis == 0) return arr;
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

        private List<List<float[,]>> QuantizeY(List<List<float[,]>> y)
        {
            List<List<float[,]>> ret = new();
            for (int i = 0; i < y.Count; ++i)
            {
                ret.Add(new List<float[,]>());
                for (int j = 0; j < y[i].Count; ++j)
                {
                    ret[i].Add(new float[0, 0]);
                    ret[i][j] = Helper.QuantizeLuminosity(y[i][j]);
                }
            }
            return ret;
        }

        private List<List<float[,]>> QuantizeC(List<List<float[,]>> c)
        {
            List<List<float[,]>> ret = new();
            for (int i = 0; i < c.Count; ++i)
            {
                ret.Add(new List<float[,]>());
                for (int j = 0; j < c[i].Count; ++j)
                {
                    ret[i].Add(new float[0, 0]);
                    ret[i][j] = Helper.QuantizeChrominance(c[i][j]);
                }
            }
            return ret;
        }

        private List<List<float[,]>> DeQuantizeY(List<List<float[,]>> y)
        {
            List<List<float[,]>> ret = new();
            for (int i = 0; i < y.Count; ++i)
            {
                ret.Add(new List<float[,]>());
                for (int j = 0; j < y[i].Count; ++j)
                {
                    ret[i].Add(new float[0, 0]);
                    ret[i][j] = Helper.DeQuantizeLuminosity(y[i][j]);
                }
            }
            return ret;
        }

        private List<List<float[,]>> DeQuantizeC(List<List<float[,]>> c)
        {
            List<List<float[,]>> ret = new();
            for (int i = 0; i < c.Count; ++i)
            {
                ret.Add(new List<float[,]>());
                for (int j = 0; j < c[i].Count; ++j)
                {
                    ret[i].Add(new float[0, 0]);
                    ret[i][j] = Helper.DeQuantizeChrominance(c[i][j]);
                }
            }
            return ret;
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

        private List<List<float[,]>> IDCTNonThreadedRunnerSquared(List<List<float[,]>> img)
        {
            List<List<float[,]>> ret = new();
            foreach (var list in img)
            {
                ret.Add(IDCTNonThreadedRunner(list));
            }
            return ret;
        }

        private List<float[,]> IDCTNonThreadedRunner(List<float[,]> img)
        {
            List<float[,]> ret = new();
            foreach (var subset in img)
            {
                ret.Add(InverseDCT(subset));
            }
            return ret;
        }

        private List<List<float[,]>> IDCTRunnerSquared(List<List<float[,]>> img)
        {
            List<List<float[,]>> returnable = new();
            foreach (var subset in img)
            {
                returnable.Add(IDCTRunner(subset));
            }
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
            Debug.WriteLine($"Original Size: {buffer.Length}");
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
