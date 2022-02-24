using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System;
using System.Windows.Media;
using System.Collections.Generic;

namespace ImageCompression
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            int[,] nums =
            {
                {1, 3, 4, 10, 11, 21, 22, 36},
                {2, 5, 9, 12, 20, 23, 35, 37},
                {6, 8, 13, 19, 24, 34, 38, 49},
                {7, 14, 18, 25, 33, 39, 48, 50},
                {15, 17, 26, 32, 40, 47, 51, 58},
                {16, 27, 31, 41, 46, 52, 57, 59},
                {28, 30, 42, 45, 53, 56, 60, 63},
                {29, 43, 44, 54, 55, 61, 62, 64}
            };
            int[] disgusting1drepresentation =
            {
                1, 3, 4, 10, 11, 21, 22, 36,
                2, 5, 9, 12, 20, 23, 35, 37,
                6, 8, 13, 19, 24, 34, 38, 49,
                7, 14, 18, 25, 33, 39, 48, 50,
                15, 17, 26, 32, 40, 47, 51, 58,
                16, 27, 31, 41, 46, 52, 57, 59,
                28, 30, 42, 45, 53, 56, 60, 63,
                29, 43, 44, 54, 55, 61, 62, 64
            };

            List<int> result = new();
            for (int i = 0; i < nums.GetLength(0) * 2; ++i)
            {
                List<int> temp = new();
                int index = i > nums.GetLength(0) - 1 ? nums.GetLength(0) - 1 : i;
                for (int j = index; j >= 0; --j)
                {
                    if (i - j >= nums.GetLength(0)) continue;
                    temp.Add(nums[j, i - j]);
                }
                if (i % 2 == 0) temp.Reverse();
                result.AddRange(temp);
            }

            foreach (int num in result)
            {
                Trace.WriteLine(num);
            }
        }


        /*
         * 
         * 
         * Handlers
         * 
         * 
         */
        private void LoadImage(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                Title = "Select an Image NOW",
                Filter = "Raw Image Files (*.ARW) | *.ARW; *.png; *.jpg; *.jpeg"
            };
            if (ofd.ShowDialog() == true)
            {
                Compressee.Source = new BitmapImage(new Uri(ofd.FileName));
                Compressee.Width = Constants.IMAGE_SIZE;
                Compressee.Height = Constants.IMAGE_SIZE;
                BitmapSource src = Compressee.Source as BitmapSource ?? throw new ArgumentException("No Image Provided");
                float scaleX = (float)Constants.IMAGE_SIZE / src.PixelWidth;
                float scaleY = (float)Constants.IMAGE_SIZE / src.PixelHeight;
                Compressee.Source = new TransformedBitmap(src, new ScaleTransform(scaleX, scaleY));
            }
            ImageLoader.MouseUp -= LoadImage;
        }

        private void Compress(object sender, RoutedEventArgs e)
        {
            Compression c = new(Compressee);
            Trace.WriteLine("Did something");
        }

    }
}
