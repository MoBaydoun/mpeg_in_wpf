using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageCompression
{
    class Pixel
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
}
