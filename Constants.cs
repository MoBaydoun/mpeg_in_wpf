using Vector3 = System.Numerics.Vector3;

namespace ImageCompression
{
    public static class Constants
    {
        public const int MAX_THREADS = 10;
        public const int MATRIX_SIZE = 8;
        public const int IMAGE_SIZE = 400;
        public static readonly Vector3 LUMINOSITY = new(0.299f, 0.587f, 0.114f);
        public static readonly Vector3 BLUE_DIFF = new(0.168736f, 0.331264f, 0.5f);
        public static readonly Vector3 RED_DIFF = new(0.5f, 0.418688f, 0.081312f);
    }
}
