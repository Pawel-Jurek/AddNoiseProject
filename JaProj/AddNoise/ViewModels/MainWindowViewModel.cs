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
        public MainWindowViewModel() 
        { 
        }

        

        public RelayCommand selectImage { get; private set; }
        public RelayCommand addNoiseCommand { get;private set; }
        public void InitializeCommands()
        {
            InitializeSelectImage();
            InitializeNoiseAdding();
        }
        private String fileName;

        public byte[]? image
        {
            get { return _image; }
            private set
            {
                if (Equals(value, _image)) return;
                _image = value;
                OnPropertyChanged("image");
            }
        }

        private byte[] _image;

        public int threadsNumber
        {
            get { return _threadsNumber; }
            set
            {
                if (value.Equals(_threadsNumber)) return;
                _threadsNumber = value;
            }
        }
        private int _threadsNumber;

        public int noisePower
        {
            get { return _noisePower; }
            set
            {
                if (value.Equals(_noisePower)) return;
                _noisePower = value;
            }
        }
        private int _noisePower;

        public NoiseAdding noiseAdding
        {
            get { return _noiseAdding; }
            set
            {
                if (value == _noiseAdding) return;
                _noiseAdding = value;
            }
        }
        private NoiseAdding _noiseAdding;

        public bool selectedAssembler
        {
            get { return _selectedAssembler; }
            set
            {
                if (value == _selectedAssembler) return;
                _selectedAssembler = value;
            }
        }
        private bool _selectedAssembler;

        public bool selectedRandomNoise
        {
            get { return _selectedRandomNoise; }
            set
            {
                if (value == _selectedRandomNoise) return;
                _selectedRandomNoise = value;
            }
        }
        private bool _selectedRandomNoise;

        public bool selectedWhiteNoise
        {
            get { return _selectedWhiteNoise; }
            set
            {
                if (value == _selectedWhiteNoise) return;
                _selectedWhiteNoise = value;
            }
        }
        private bool _selectedWhiteNoise;

        public bool selectedColorNoise
        {
            get { return _selectedColorNoise; }
            set
            {
                if (value == _selectedColorNoise) return;
                _selectedColorNoise = value;
            }
        }
        private bool _selectedColorNoise;

        public bool selectedCSharp
        {
            get { return _selectedCSharp; }
            set
            {
                if (value == _selectedCSharp) return;
                _selectedCSharp = value;
            }
        }

        private bool _selectedCSharp;

        public TimeSpan computationTime
        {
            get { return _computationTime; }
            set
            {
                if (value.Equals(_computationTime)) return;
                _computationTime = value;
                OnPropertyChanged("computationTime");
            }
        }
        private TimeSpan _computationTime;

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
                    fileName = dlg.FileName;
                    Debug.WriteLine("Button clicked. Selected file: " + fileName);
                    noiseAdding = new NoiseAdding(fileName);
                    image = noiseAdding.originalImage;
                    //var converter = new ByteArrayToImageConverter();

                    //imageSource = (BitmapImage)converter.Convert(noiseAdding.originalImage, typeof(BitmapImage), null, CultureInfo.CurrentCulture);
                }
            });
        }
        public void InitializeProperties()
        {
            threadsNumber = 2;
            selectedRandomNoise = true;
            selectedAssembler = true;
            noisePower = 30;
        }
        private void InitializeNoiseAdding()
        {
            addNoiseCommand = new RelayCommand(() =>
            {
                if (image == null)
                {
                    MessageBox.Show("Select an image first!");
                    return;
                }
                string option = selectedColorNoise ? "color" : selectedRandomNoise ? "random" : "white";
                noiseAdding = new NoiseAdding(fileName);
                int miliseconds = noiseAdding.addNoiseToImage(selectedAssembler, option, threadsNumber, noisePower);
                computationTime = TimeSpan.FromMilliseconds(miliseconds);
                image = noiseAdding.finalImage;
            });
        }

    }
}
