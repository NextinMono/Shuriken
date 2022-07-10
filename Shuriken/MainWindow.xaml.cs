﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using Shuriken.ViewModels;
using System.Collections.ObjectModel;
using XNCPLib;

namespace Shuriken
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {   
        private static readonly string filters = "All files (*.xncp;*.yncp)|*.xncp;*.yncp|CSD Project (*.xncp)|*.xncp|CSD Project (*.yncp)|*.yncp";

        private MainViewModel vm;

        public MainWindow()
        {
            FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;
            InitializeComponent();

            vm = new MainViewModel();
            DataContext = vm;

            editorSelect.SelectedIndex = 0;
        }

        private void NewClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.Clear();
            }
        }

        private void OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = filters;

            if (fileDialog.ShowDialog() == true)
            {
                vm.Load(fileDialog.FileName);
            }
        }
        private void SaveMenu_Click(object sender, RoutedEventArgs e)
        {
            try { vm.Save(null); }
            catch(System.IO.IOException error)
            {
#if DEBUG
                System.Diagnostics.Debug.Fail(error.Message, error.StackTrace);                
#else
                MessageBox.Show(error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);                
#endif
            }

        }

        private void SaveAsMenu_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = filters;

            if (fileDialog.ShowDialog() == true)
            {
                vm.Save(fileDialog.FileName);
            }
        }
        private void Merge_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = filters;
            if(openDialog.ShowDialog() == true)
            {
                if(MessageBox.Show("Are you sure you want to merge the two files? If you save, it will be irreversible (for now)!", "Shuriken", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    vm.Merge(@openDialog.FileName, @vm.WorkFilePath);
                }
            }
        }

        private void HelpClick(object sender, RoutedEventArgs e)
        {
        }

        private void ViewClick(object sender, RoutedEventArgs e)
        {
        }
        private void WidescreenSetClick(object sender, RoutedEventArgs e)
        {
            Shuriken.Views.UIEditor.ViewX = 1280;
            Shuriken.Views.UIEditor.ViewY = 720;
        }
        private void FourThreeScreenSetClick(object sender, RoutedEventArgs e)
        {
            Shuriken.Views.UIEditor.ViewX = 640;
            Shuriken.Views.UIEditor.ViewY = 480;
        }


        private void ExitMenu_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Check for differences in the loaded file and prompt the user to save
            Application.Current.Shutdown();
        }

        private void SetDarkBackground(object sender, RoutedEventArgs e)
        {
            Shuriken.Views.UIEditor.ColorView = new Models.Vector3(0.2f, 0.2f, 0.2f);
        }
        private void SetLightBackground(object sender, RoutedEventArgs e)
        {
            Shuriken.Views.UIEditor.ColorView = new Models.Vector3(0.8f, 0.8f, 0.8f);
        }
    }
}
