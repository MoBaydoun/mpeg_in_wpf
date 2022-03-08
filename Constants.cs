using Vector3 = System.Numerics.Vector3;

namespace ImageCompression
{
    public static class Constants
    {
        public const int MAX_THREADS = 5;
        public const int MATRIX_SIZE = 8;
        public const int IMAGE_SIZE = 400;
        public const int KEY = 0;
        public const int BLOCKS = 8;
        public const int P = 50;
        public static readonly Vector3 LUMINOSITY = new(0.299f, 0.587f, 0.114f);
        public static readonly Vector3 BLUE_DIFF = new(0.168736f, 0.331264f, 0.5f);
        public static readonly Vector3 RED_DIFF = new(0.5f, 0.418688f, 0.081312f);
        public static readonly int[,] Q_LUMINOSITY =
        {
            {16, 11, 10, 16, 24, 40, 51, 61},
            {12, 12, 14, 19, 26, 58, 60, 55},
            {14, 13, 16, 24, 40, 57, 69, 56},
            {14, 17, 22, 29, 51, 87, 80, 62},
            {18, 22, 37, 56, 68, 109, 103, 77},
            {24, 35, 55, 64, 81, 104, 113, 92},
            {49, 64, 78, 87, 103, 121, 120, 101},
            {72, 92, 95, 98, 112, 100, 103, 99}
        };
        public static readonly int[,] Q_CHROMINANCE =
        {
            {17, 18, 24, 47, 99, 99, 99, 99},
            {18, 21, 26, 66, 99, 99, 99, 99},
            {24, 26, 56, 99, 99, 99, 99, 99},
            {47, 66, 99, 99, 99, 99, 99, 99},
            {99, 99, 99, 99, 99, 99, 99, 99},
            {99, 99, 99, 99, 99, 99, 99, 99},
            {99, 99, 99, 99, 99, 99, 99, 99},
            {99, 99, 99, 99, 99, 99, 99, 99}
        };
    }
}
