using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System;
using System.Collections.Generic;
using Line = System.Windows.Shapes.Line;
using System.Linq;

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
                Compressee.Width = ImageLoader.Width;
                Compressee.Height = ImageLoader.Height;
            }
            ImageLoader.MouseUp -= LoadImage;
        }

        private void LoadSource(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                Title = "Select an Image NOW",
                Filter = "Raw Image Files (*.ARW) | *.ARW; *.png; *.jpg; *.jpeg; *.bmp"
            };
            if (ofd.ShowDialog() == true)
            {
                Source.Source = new BitmapImage(new Uri(ofd.FileName));
                Source.Width = SourceLoader.Width;
                Source.Height = SourceLoader.Height;
            }
            Source.MouseUp -= LoadSource;
        }

        private void Compress(object sender, RoutedEventArgs e)
        {
            new Compression(Compressee);
        }

        private void OpenCompressed(object sender, RoutedEventArgs e)
        {
            Compressee.Source = Compression.OpenCompressed();
            Compressee.Width = Width / 2;
            Compressee.Height = Height;
        }

        private void MotionVectors(object sender, RoutedEventArgs e)
        {
            Compression.Mpegger(Compressee, ImageLoader, Source, SourceLoader);
        }
    }
}
