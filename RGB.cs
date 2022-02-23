using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageCompression
{
    class RGB
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }

        public RGB(float[] data)
        {
            R = data[0];
            G = data[1];
            B = data[2];
            A = 255;
        }

        public RGB(YCBCR p)
        {
            R = p.Y + 1.402f * (p.Cr - 128);
            G = p.Y - 0.344136f * (p.Cb - 128) - 0.714136f * (p.Cr - 128);
            B = p.Y + 1.772f * (p.Cb - 128);
            A = 255;
        }

        public override string ToString()
        {
            string s = $"R: {R} G: {G} B: {B}\n";
            return s;
        }

        public static RGB[,] MatrixBufferToRGB(float[,] data)
        {
            int rows = data.GetLength(0);
            int columns = data.GetLength(1);
            RGB[,] pixels = new RGB[rows, columns / 4];
            for (int y = 0; y < rows; ++y)
            {
                for (int x = 0; x < columns; x += 4)
                {
                    pixels[y, x / 4] = new(new float[] {
                            data[y, x],
                            data[y, x + 1],
                            data[y, x + 2]
                    });
                }
            }
            return pixels;
        }

        public static byte[,] RGBtoBuffer(RGB[,] pixels)
        {
            byte[,] converted = new byte[pixels.GetLength(0), pixels.GetLength(1) * 4];
            for (int i = 0; i < converted.GetLength(0); ++i)
            {
                for (int j = 0; j < converted.GetLength(1); j += 4)
                {
                    converted[i, j] = (byte)Math.Clamp(pixels[i, j / 4].R, 0, 255);
                    converted[i, j + 1] = (byte)Math.Clamp(pixels[i, j / 4].G, 0, 255);
                    converted[i, j + 2] = (byte)Math.Clamp(pixels[i, j / 4].B, 0, 255);
                }
            }
            return converted;
        }

        public static RGB[,] YCBCRtoRGB(YCBCR[,] conv)
        {
            RGB[,] regular = new RGB[conv.GetLength(0), conv.GetLength(1)];
            for (int i = 0; i < conv.GetLength(0); ++i)
            {
                for (int j = 0; j < conv.GetLength(1); ++j)
                {
                    regular[i, j] = new(conv[i, j]);
                }
            }
            return regular;
        }
    }
}
