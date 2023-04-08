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
//using System.Windows.Shapes;
using Shuriken.ViewModels;
using System.Collections.ObjectModel;
using XNCPLib;
using System.IO;
using Shuriken.Models;
using System.Diagnostics;

namespace Shuriken
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string filters = "All files (*.xncp;*.yncp;*.gncp)|*.xncp;*.yncp;*.gncp|CSD Project (*.xncp)|*.xncp|CSD Project (*.yncp)|*.yncp|CSD Project (*.gncp)|*.gncp|CSD Project (*.sncp)|*.sncp";


        private MainViewModel vm;

        int MergeFirstSel, MergeSecondSel;

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
                vm.RecentFiles.Add(fileDialog.FileName);
            }
        }
        private void SaveMenu_Click(object sender, RoutedEventArgs e)
        {
            try { vm.Save(null); System.Media.SystemSounds.Asterisk.Play(); }
            catch (System.IO.IOException error)
            {
#if DEBUG
                System.Diagnostics.Debug.Fail(error.Message, error.StackTrace);
#endif
                MessageBox.Show(error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

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
            if (openDialog.ShowDialog() == true)
            {
                if (MessageBox.Show("Are you sure you want to merge the two files? If you save, it will be irreversible (for now)!", "Shuriken", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    vm.Merge(@openDialog.FileName, @vm.WorkFilePath);
                }
            }
        }

        private void HelpClick(object sender, RoutedEventArgs e)
        {
        }

        private void WidescreenSetClick(object sender, RoutedEventArgs e)
        {
            Shuriken.Views.UIEditor.ViewResolution = new Models.Vector2(1280, 720);
        }
        private void LetterboxSetClick(object sender, RoutedEventArgs e)
        {
            Shuriken.Views.UIEditor.ViewResolution = new Models.Vector2(640, 480);
        }
        private void ExitMenu_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Check for differences in the loaded file and prompt the user to save
            if (MainViewModel.IsDirty)
                MessageBox.Show("Do you want to save your progress?", "Shuriken", MessageBoxButton.YesNo);
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
        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            if(vm.WorkFilePath != null)
            vm.Load(vm.WorkFilePath);
        }
        private void Tools_ChangeColorOrder(object sender, RoutedEventArgs e)
        {
            IEnumerable<UICast> allCasts = Project.SceneGroups
                       .SelectMany(sceneGroup => sceneGroup.Scenes)
                       .SelectMany(scene => scene.Groups)
                       .SelectMany(group => group.Casts);

            foreach (var cast in allCasts)
            {
                Tools_ChangeColors(cast);
            }
        }
        void Tools_ChangeColors(UICast cast)
        {
            cast.Color.InvertOrder();
            cast.GradientTopRight.InvertOrder();
            cast.GradientTopLeft.InvertOrder();
            cast.GradientBottomLeft.InvertOrder();
            cast.GradientBottomRight.InvertOrder();
            foreach (var f in cast.Children)
                Tools_ChangeColors(f);
        }
        private void MergeAllNodes(object sender, RoutedEventArgs e)
        {
            var list = Project.SceneGroups[0].Scenes.ToList();
            for (int i = 0; i < Project.SceneGroups.Count; i++)
            {
                var h = Project.SceneGroups[i].Children;
                foreach (var child in h)
                {
                    list.AddRange(GetScenesFromChildren(child));
                }
            }
            for (int i = 0; i < Project.SceneGroups.Count; i++)
            {
                Project.SceneGroups[i].Children.Clear();
            }
            for (int i = 0; i < list.Count; i++)
            {
                Project.SceneGroups[0].Scenes.Add(list[i]);
            }
        }
        private void Tools_SelectFirstSceneMerge(object sender, RoutedEventArgs e)
        {
            object uiobj = Views.UIEditor.SelectedUIObject;

            if (uiobj is UIScene)
            {
                UIScene scene = (UIScene)uiobj;
                MergeFirstSel = Project.SceneGroups[0].Scenes.IndexOf(scene);
            }
        }
        private void Tools_SelectSecondSceneMerge(object sender, RoutedEventArgs e)
        {
            object uiobj = Views.UIEditor.SelectedUIObject;

            if (uiobj is UIScene)
            {
                UIScene scene = (UIScene)uiobj;
                MergeSecondSel = Project.SceneGroups[0].Scenes.IndexOf(scene);
            }
        }
        private void Tools_MergeScenes(object sender, RoutedEventArgs e)
        {
            Project.SceneGroups[0].Scenes[MergeFirstSel].Merge(Project.SceneGroups[0].Scenes[MergeSecondSel]);
        }
        ObservableCollection<UIScene> GetScenesFromChildren(UISceneGroup g)
        {
            var r = g.Scenes.ToList();
            for (int i = 0; i < g.Children.Count; i++)
            {
                r.AddRange(GetScenesFromChildren(g.Children[i]));
            }
            return new ObservableCollection<UIScene>(r);
        }
        private void Tools_LetterboxToWidescreen(object sender, RoutedEventArgs e)
        {
            object uiobj = Views.UIEditor.SelectedUIObject;

            if (uiobj is UIScene)
            {
                UIScene scene = (UIScene)uiobj;
                Tools_ScalingConvertValues(scene, scene.Groups[0].Casts[0]);
            }
        }
        void Tools_ScalingConvertValues(UIScene scene, UICast cast)
        {
            cast.Field00 = 0;
            cast.Field5C = 0;
        }
        private void Tools_Clone(object sender, RoutedEventArgs e)
        {
            if (Shuriken.Views.UIEditor.SelectedUIObject is UIScene)
                Project.SceneGroups[0].Scenes.Add((UIScene)((UIScene)Shuriken.Views.UIEditor.SelectedUIObject).Clone());
        }
        private void UpdateResolutionText(object sender, RoutedEventArgs e)
        {
            ResolutionHeader.Header = vm.Resolution;
        }
    }
}
