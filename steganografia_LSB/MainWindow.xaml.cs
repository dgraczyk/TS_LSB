using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace steganografia_LSB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Bitmap sourceBitmap;
        private Bitmap changedBitmap;
        private string tempPath = AppDomain.CurrentDomain.BaseDirectory + "temp.bmp";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItemLoad_OnClick(object sender, RoutedEventArgs e)
        {
            var path = LoadFile();

            if (!string.IsNullOrEmpty(path))
            {
                this.ImageBefore.Source = new BitmapImage(new Uri(path));   
                this.sourceBitmap = new Bitmap(path);
            }
        }

        private void Insert_OnClick(object sender, RoutedEventArgs e)
        {
            if (sourceBitmap == null || string.IsNullOrEmpty(this.Information.Text))
            {
                MessageBox.Show("Empty text or image", "", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var encodeText = String.Format("{0} {1}", this.Information.Text.Length, this.Information.Text);
            
            if (sourceBitmap.Size.Height * sourceBitmap.Size.Width * 3 / 8 > encodeText.Length)
            {
                changedBitmap = LSB.Encode(encodeText, sourceBitmap);
                changedBitmap.Save(tempPath);

                this.ImageAfter.Source = BitmapFrame.Create(new Uri(tempPath), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                File.Delete(tempPath);

                MessageBox.Show("Message inserted", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Image is too small for this text", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            
        }

        private void MenuItemRead_OnClick(object sender, RoutedEventArgs e)
        {
            var path = LoadFile();

            if (string.IsNullOrEmpty(path))
                return;

            var text = LSB.Decode(new Bitmap(path));

            MessageBox.Show(text, "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuItemSave_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.changedBitmap == null)
            {
                MessageBox.Show("Before saving, encrypt some message", "", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var dialog = new SaveFileDialog();
            dialog.Filter = "Image files (*.bmp) | *.bmp";

            var result = dialog.ShowDialog();

            if (result == true)
            {
                changedBitmap.Save(dialog.FileName);
            }
        }

        private string LoadFile()
        {
            var dialog = new OpenFileDialog();

            dialog.Filter = "Image files (*.bmp) | *.bmp";

            var result = dialog.ShowDialog();

            if (result == true)
            {
                return dialog.FileName;
            }

            return null;
        }
    }
}
