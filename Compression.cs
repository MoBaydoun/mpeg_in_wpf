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

        public struct Pixel
        {
            public float R { get; set; }
            public float G { get; set; }
            public float B { get; set; }

            public Pixel(float[] data)
            {
                R = data[0];
                G = data[1];
                B = data[2];
            }

            public Pixel(ConvertedPixel p)
            {
                R = p.Y + 1.402f * (p.Cr - 128);
                G = p.Y - 0.344136f * (p.Cb - 128) - 0.714136f * (p.Cr - 128);
                B = p.Y + 1.772f * (p.Cb - 128);
            }

            public override string ToString()
            {
                string s = $"R: {R}\nG: {G}\nB: {B}";
                return s;
            }


            public static Pixel[,] DataToPixels(float[,] data)
            {
                int width = data.GetLength(0);
                int height = data.GetLength(1);
                Pixel[,] pixels = new Pixel[width, height / 3];
                for (int y = 0; y < width; ++y)
                {
                    for (int x = 0; x < height; x += 3)
                    {
                        pixels[x, y / 3] = new Pixel(new float[] {
                            data[x, y],
                            data[x, y + 1],
                            data[x, y + 2]});
                    }
                }
                return pixels;
            }
        }

        public struct ConvertedPixel
        {
            public float Y { get; set; }
            public float Cb { get; set; }
            public float Cr { get; set; }

            public ConvertedPixel(Pixel p)
            {
                Y = 0 + (Constants.LUMINOSITY.X * p.R) + (Constants.LUMINOSITY.Y * p.G) + (Constants.LUMINOSITY.Z * p.B);
                Cb = 128 - (Constants.BLUE_DIFF.X * p.R) - (Constants.BLUE_DIFF.Y * p.G) + (Constants.BLUE_DIFF.Z * p.B);
                Cr = 128 + (Constants.RED_DIFF.X * p.R) - (Constants.RED_DIFF.Y * p.G) - (Constants.RED_DIFF.Z * p.B);
            }

            public override string ToString()
            {
                string s = $"Y: {Y}\nCb: {Cb}\nCr: {Cr}";
                return s;
            }

            public static ConvertedPixel[,] ConvertPixels(Pixel[,] pixels)
            {
                int width = pixels.GetLength(0);
                int height = pixels.GetLength(1);
                ConvertedPixel[,] converted = new ConvertedPixel[width, height];
                for (int i = 0; i < width; ++i)
                {
                    for (int j = 0; j < height; ++j)
                    {
                        converted[i, j] = new ConvertedPixel(pixels[i, j]);
                    }
                }
                return converted;
            }

            public static byte[,] PixelsToData(ConvertedPixel[,] pixels)
            {
                byte[,] converted = new byte[pixels.GetLength(0), pixels.GetLength(1) * 3];
                for (int i = 0; i < converted.GetLength(0); ++i)
                {
                    for (int j = 0; j < converted.GetLength(1); j += 3)
                    {
                        converted[i, j] = (byte)MathF.Round(pixels[i, j].Y);
                        converted[i, j + 1] = (byte)MathF.Round(pixels[i, j].Cb);
                        converted[i, j + 2] = (byte)MathF.Round(pixels[i, j].Cr);
                    }
                }
                return converted;
            }
        }

        public Pixel[,] pixels { get; private set; }
        public Compression(Image img)
        {
            var src = img.Source as BitmapSource ?? throw new ArgumentNullException("Image does not exist");
            var image = ConvertData(src);
            pixels = Pixel.DataToPixels(image);
            var convert = ConvertedPixel.ConvertPixels(pixels);
            var changed = ConvertBack(ConvertedPixel.PixelsToData(convert));
            WriteableBitmap bmp = new(img.Source as BitmapSource);
            bmp.WritePixels(
                new System.Windows.Int32Rect(0, 0, src.PixelWidth, src.PixelHeight),
                changed,
                src.PixelWidth * src.Format.BitsPerPixel / 8,
                0);
            img.Source = bmp;
        }


        private List<float[,]> CreateSubsets(float[,] img)
        {
            int width = img.GetLength(0);
            int height = img.GetLength(1);
            List<float[,]> subsets = new();
            for (int i = 0; i < width; i += Constants.MATRIX_SIZE)
            {
                for (int j = 0; j < height; j += Constants.MATRIX_SIZE)
                {
                    subsets.Add(PadSubset(GetSubset(img, i, j)));
                }
            }
            return subsets;
        }

        private static float[,] PadSubset(float[,] faulty)
        {
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

        public List<float[,]> DCTRunner(List<float[,]> img)
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

        private float[,] SubSample(float[,] image)
        {
            float[,] result = new float[image.GetLength(0) / 2, image.GetLength(1) / 2];
            for (int r = 0; r < image.GetLength(0); r += 2)
            {
                for (int c = 0; c < image.GetLength(1); c += 2)
                {
                    result[r / 2, c / 2] = image[r, c];
                }
            }
            return result;
        }

        private float[,] SubSampleV2(float[,] image)
        {
            float[,] result = new float[image.GetLength(0), image.GetLength(1)];
            for (int r = 0; r < image.GetLength(0); r += 2)
            {
                for (int c = 0; c < image.GetLength(1); c += 2)
                {
                    result[r, c] = image[r, c];
                    result[r, c + 1] = image[r, c];
                    result[r + 1, c] = image[r, c];
                    result[r + 1, c + 1] = image[r, c];
                }
            }
            return result;
        }

        private float[,] ConvertData(BitmapSource src)
        {
            int stride = src.PixelWidth * src.Format.BitsPerPixel / 8;
            int total = src.PixelWidth * src.PixelHeight * src.Format.BitsPerPixel / 8;
            int stridenums = total / stride;

            byte[] buffer = new byte[total];
            src.CopyPixels(buffer, stride, 0);

            float[,] data = new float[stride, stridenums];

            for (int y = 0; y < stridenums; ++y)
            {
                for (int x = 0; x < stride; ++x)
                {
                    data[x, y] = buffer[y * stride + x];
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
            float r = f < 0 ? MathF.Truncate(f - 0.5f) : MathF.Truncate(f + 0.5f);
            return r == -0 ? 0 : r;
        }
    }
}
