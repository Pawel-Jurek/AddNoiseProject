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
using System.Windows.Media.Imaging;
using AddNoise.Models;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace AddNoise.ViewModels
{
    public partial class MainWindowViewModel: ViewModelBase
    {
        public MainWindowViewModel() 
        { 
        }

        

        public RelayCommand selectImage { get; private set; }
        public RelayCommand addNoiseCommand { get;private set; }
        public RelayCommand saveImageCommand { get; private set; }
        public RelayCommand TestCommand { get; private set; }
        public void InitializeCommands()
        {
            InitializeSelectImage();
            InitializeNoiseAdding();
            InitializeSaveImageCommand();
            InitializeTestCommand();
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

        public bool isButtonEnabled
        {
            get { return _isButtonEnabled; }
            set
            {
                if (value != _isButtonEnabled)
                {
                    _isButtonEnabled = value;
                    OnPropertyChanged("isButtonEnabled");
                }
            }
        }
        private bool _isButtonEnabled;

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
                    noiseAdding = new NoiseAdding(fileName);
                    image = noiseAdding.originalImage;

                }
            });
        }
        public void InitializeProperties()
        {
            threadsNumber = System.Environment.ProcessorCount;
            selectedRandomNoise = true;
            selectedAssembler = true;
            noisePower = 30;
            isButtonEnabled = true;
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
                isButtonEnabled = false;
                string option = selectedColorNoise ? "color" : selectedRandomNoise ? "random" : "white";
                noiseAdding = new NoiseAdding(fileName);
                int miliseconds = noiseAdding.addNoiseToImage(selectedAssembler, option, threadsNumber, noisePower);
                computationTime = TimeSpan.FromMilliseconds(miliseconds);
                image = noiseAdding.finalImage;
                isButtonEnabled = true;
            });
        }
        private void InitializeSaveImageCommand()
        {
            saveImageCommand = new RelayCommand(() =>
            {
                string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // Przejście do katalogu nadrzędnego projektu
                DirectoryInfo parentDirectory = Directory.GetParent(projectDirectory);
                string projectParentDirectory = parentDirectory?.Parent?.Parent?.Parent?.Parent?.Parent?.FullName;
                if (projectParentDirectory != null)
                {
                    string outputDirectory = Path.Combine(projectParentDirectory, "outputs");

                    Directory.CreateDirectory(outputDirectory);

                    string fileExtension = ".bmp";
                    string option = (selectedColorNoise ? "color" : selectedRandomNoise ? "random" : "white") + "Noise";

                    string[] existingFiles = Directory.GetFiles(outputDirectory, option + "Image*" + fileExtension);

                    string newFileName = Path.Combine(outputDirectory, $"{option}Image{existingFiles.Length + 1}.bmp");

                    saveImage(newFileName);
                }
                else
                {
                    Debug.WriteLine("Unable to access the parent directory of the project.");
                }
            });

        }
        private void saveImage(string path)
        {
            if (image == null)
            {
                MessageBox.Show("Select an image first!");
                return;
            }
            try
            {
                File.WriteAllBytes(path, image);
                MessageBox.Show("Saved file in the current location.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void InitializeTestCommand()
        {
            TestCommand = new RelayCommand( async () =>
            {
                if (image == null)
                {
                    MessageBox.Show("Select an image first!");
                    return;
                }
                List<int> threadCounts = new List<int>() { 1, 2, 4, 8, 16, 32, 64 };
                List<bool> usingAsmLib = new List<bool>() { true, false };

                isButtonEnabled = false;
                string option = selectedColorNoise ? "color" : selectedRandomNoise ? "random" : "white";
                noiseAdding = new NoiseAdding(fileName);
                int timeToWait = await Task.Run(() => noiseAdding.addNoiseToImage(true, option, 1, noisePower)) * 2;
                await Task.Delay(timeToWait);
                await Task.Run(() => noiseAdding.addNoiseToImage(false, option, 1, noisePower));

                List<TestResult> testResults = new List<TestResult>();
                try
                {
                    foreach (int threadCountUsed in threadCounts)
                    {
                        foreach (bool isAsm in usingAsmLib)
                        {
                            int milisecondsEstimated = 0;
                            for (int ctr = 0; ctr < 5; ctr++)
                            {
                                milisecondsEstimated += await Task.Run(() => noiseAdding.addNoiseToImage(isAsm, option, threadsNumber, noisePower));
                                await Task.Delay(timeToWait);
                            }
                            

                            testResults.Add(new TestResult()
                            {
                                isAssemblerLibraryActive = isAsm,
                                miliseconds = milisecondsEstimated/5,
                                threadCount = threadCountUsed
                            });
                        }
                    }
                } catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
                
                string result = "";
                foreach (TestResult testResult in testResults)
                {
                    string library = testResult.isAssemblerLibraryActive
                        ? "assembly"
                        : "C++";
                    result += $"{testResult.threadCount} threads, generated in {testResult.miliseconds} ms using the {library} library. \r\n";
                }
                isButtonEnabled = true;
                MessageBox.Show(result);
            });
        }
    }
}
