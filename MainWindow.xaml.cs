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
            byte[,] nums =
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
            var oned = Helper.Mogarithm(nums);
            Helper.PrintArray(oned);
            var twod = Helper.ArrayToMatrix(oned, 8);
            Helper.PrintMatrix(twod);

            int[] arr = { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
            Helper.PrintArray(arr);
            Helper.ReverseRange(arr, 4, 8);
            Helper.PrintArray(arr);
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
                Compressee.Source = new BitmapImage(new Uri(ofd.FileName));/*
                Compressee.Width = Constants.IMAGE_SIZE;
                Compressee.Height = Constants.IMAGE_SIZE;
                BitmapSource src = Compressee.Source as BitmapSource ?? throw new ArgumentException("No Image Provided");
                float scaleX = (float)Constants.IMAGE_SIZE / src.PixelWidth;
                float scaleY = (float)Constants.IMAGE_SIZE / src.PixelHeight;
                Compressee.Source = new TransformedBitmap(src, new ScaleTransform(scaleX, scaleY));*/
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
