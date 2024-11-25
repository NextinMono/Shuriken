using Microsoft.Win32;
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
    public partial class GSncpImportWindow : Window
    {   
        private static readonly string filters = "All files (*.xncp;*.yncp;*.gncp;*.sncp;*.swif)|*.xncp;*.yncp;*.gncp;*.sncp;*.swif|CSD Project (*.xncp)|*.xncp|CSD Project (*.yncp)|*.yncp";

        private MainViewModel vm;

        public GSncpImportWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
