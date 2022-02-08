using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageCompression
{
    class ConvertedPixel
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
}
