using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System;
using System.Windows.Media;

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
                Filter = "Raw Image Files (*.tiff) | *.tiff"
            };
            if (ofd.ShowDialog() == true)
            {
                Compressee.Source = new BitmapImage(new Uri(ofd.FileName));
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
