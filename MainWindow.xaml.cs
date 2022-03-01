using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System;
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
            List<int> l = new();
            for (int i = 0; i < 256; ++i)
            {
                l.Add(i);
            }
            for (int i = 0; i < l.Count; i += 64)
            {
                Helper.PrintArray(l.GetRange(i, 64).ToArray());
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
                Filter = "Raw Image Files (*.ARW) | *.ARW; *.png; *.jpg; *.jpeg; *.bmp"
            };
            if (ofd.ShowDialog() == true)
            {
                Compressee.Source = new BitmapImage(new Uri(ofd.FileName));
                Compressee.Width = Width;
                Compressee.Height = Height;
                /*Compressee.Width = Constants.IMAGE_SIZE;
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
            new Compression(Compressee);
            Debug.WriteLine("Did something");
        }

    }
}
