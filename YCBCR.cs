using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageCompression
{
    class YCBCR
    {
        public float Y { get; set; }
        public float Cb { get; set; }
        public float Cr { get; set; }
        public float A { get; set; }

        public YCBCR()
        {
        }

        public YCBCR(float Y, float Cb, float Cr)
        {
            this.Y = Y;
            this.Cb = Cb;
            this.Cr = Cr;
            A = 255;
        }

        public YCBCR(byte[] data)
        {
            Y = data[0];
            Cb = data[1];
            Cr = data[2];
            A = 255;
        }

        public YCBCR(YCBCR pix)
        {
            Y = pix.Y;
            Cb = pix.Cb;
            Cr = pix.Cr;
            A = pix.A;
        }

        public YCBCR(RGB p)
        {
            Y = 0 + (Constants.LUMINOSITY.X * p.R) + (Constants.LUMINOSITY.Y * p.G) + (Constants.LUMINOSITY.Z * p.B);
            Cb = 128 - (Constants.BLUE_DIFF.X * p.R) - (Constants.BLUE_DIFF.Y * p.G) + (Constants.BLUE_DIFF.Z * p.B);
            Cr = 128 + (Constants.RED_DIFF.X * p.R) - (Constants.RED_DIFF.Y * p.G) - (Constants.RED_DIFF.Z * p.B);
            A = 255;
        }

        public override string ToString()
        {
            string s = $"Y: {Y}\nCb: {Cb}\nCr: {Cr}";
            return s;
        }

        public static YCBCR[,] RGBMatrixToYCBCRMatrix(RGB[,] pixels)
        {
            int width = pixels.GetLength(0);
            int height = pixels.GetLength(1);
            YCBCR[,] converted = new YCBCR[width, height];
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    converted[i, j] = new YCBCR(pixels[i, j]);
                }
            }
            return converted;
        }

        public static byte[,] YCBCRtoBuffer(YCBCR[,] pixels)
        {
            byte[,] converted = new byte[pixels.GetLength(0), pixels.GetLength(1) * 4];
            for (int i = 0; i < converted.GetLength(0); ++i)
            {
                for (int j = 0; j < converted.GetLength(1); j += 4)
                {
                    converted[i, j] = (byte)Math.Clamp(pixels[i, j / 4].Y, 0, 255);
                    converted[i, j + 1] = (byte)Math.Clamp(pixels[i, j / 4].Cb, 0, 255);
                    converted[i, j + 2] = (byte)Math.Clamp(pixels[i, j / 4].Cr, 0, 255);
                }
            }
            return converted;
        }

        public static YCBCR[,] BufferToYCBCR(byte[,] data)
        {
            int rows = data.GetLength(0);
            int columns = data.GetLength(1);
            YCBCR[,] pixels = new YCBCR[rows, columns / 4];
            for (int y = 0; y < rows; ++y)
            {
                for (int x = 0; x < columns; x += 4)
                {
                    pixels[y, x / 4] = new(new byte[] {
                            data[y, x],
                            data[y, x + 1],
                            data[y, x + 2]
                    });
                }
            }
            return pixels;
        }

        public static void DeconstructYCBCR(YCBCR[,] pixels, out float[,] y, out float[,] cb, out float[,] cr)
        {
            y = new float[pixels.GetLength(0), pixels.GetLength(1)];
            cb = new float[pixels.GetLength(0), pixels.GetLength(1)];
            cr = new float[pixels.GetLength(0), pixels.GetLength(1)];
            for (int i = 0; i < pixels.GetLength(0); ++i)
            {
                for (int j = 0; j < pixels.GetLength(1); ++j)
                {
                    y[i, j] = pixels[i, j].Y;
                    cb[i, j] = pixels[i, j].Cb;
                    cr[i, j] = pixels[i, j].Cr;
                }
            }
        }
    }
}
