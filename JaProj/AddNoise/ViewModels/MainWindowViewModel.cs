using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using AddNoise.Converters;
using System.Windows.Media.Imaging;
using AddNoise.Models;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace AddNoise.ViewModels
{
    public partial class MainWindowViewModel: ViewModelBase
    {
        private NoiseAdding noiseAdding;
        private byte[]? image;
        public MainWindowViewModel() 
        { 
        }



        public RelayCommand selectImage { get; private set; }
        public void InitializeCommands()
        {
            InitializeSelectImage();
        }

        private BitmapImage _imageSource;
        public BitmapImage imageSource
        {
            get { return _imageSource; }
            set
            {
                _imageSource = value;
                OnPropertyChanged("imageSource"); 
            }
        }
        private void InitializeSelectImage()
        {
            selectImage = new RelayCommand(() =>
            {
                image = null;
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.InitialDirectory = "c:\\";
                dlg.Filter = "Image files (*.bmp)|*.bmp|All Files (*.*)|*.*";
                dlg.RestoreDirectory = true;
                bool? wasOKButtonClicked = dlg.ShowDialog();

                if (wasOKButtonClicked == true)
                {
                    string fileName = dlg.FileName;
                    Debug.WriteLine("Button clicked. Selected file: " + fileName);
                    noiseAdding = new NoiseAdding(fileName);

                    var converter = new ByteArrayToImageConverter();

                    imageSource = (BitmapImage)converter.Convert(noiseAdding.originalImage, typeof(BitmapImage), null, CultureInfo.CurrentCulture);
                }
            });
        }


    }
}
