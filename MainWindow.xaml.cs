﻿using System.Windows;
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
            int num = 16;
            float[,] arr = new float[num, num];
            for (int i = 0; i < num; ++i)
            {
                for (int j = 0; j < num; ++j)
                {
                    arr[i, j] = i * num + j;
                }
            }
            Helper.PrintMatrix(arr);
            var sub = Compression.CreateSubsets(arr);
            arr = Compression.ReassembleSubsets(sub);
            Helper.PrintMatrix(arr);
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
                Filter = "Raw Image Files (*.ARW) | *.ARW"
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
