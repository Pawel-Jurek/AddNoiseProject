using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace AddNoise.ViewModels
{
    public partial class MainWindowViewModel: ViewModelBase
    {

        public MainWindowViewModel() 
        { 
        }



        public RelayCommand SelectImage { get; private set; }
        public void InitializeCommands()
        {
            InitializeSelectImage();
        }


        private void InitializeSelectImage()
        {
            SelectImage = new RelayCommand(() =>
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.InitialDirectory = "c:\\";
                dlg.Filter = "Image files (*.bmp)|*.bmp|All Files (*.*)|*.*";
                dlg.RestoreDirectory = true;
                bool? wasOKButtonClicked = dlg.ShowDialog();

                if (wasOKButtonClicked == null ? false : (bool)wasOKButtonClicked)
                {
                    string selectedFileName = dlg.FileName;

                    Debug.WriteLine("Button clicked. Selected file: " + selectedFileName);
                }
            });
        }


    }
}
