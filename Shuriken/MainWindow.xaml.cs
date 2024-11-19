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
using XNCPLib.XNCP;
using Shuriken.Misc;

namespace Shuriken
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string filters = "All files (*.xncp;*.yncp;*.gncp)|*.xncp;*.yncp;*.gncp|CSD Project (*.xncp)|*.xncp|CSD Project (*.yncp)|*.yncp|CSD Project (*.gncp)|*.gncp|CSD Project (*.sncp)|*.sncp";


        private MainViewModel vm;

        int MergeFirstSel, MergeSecondSel, MergeFolderParent1, MergeFolderParent2;

        public MainWindow()
        {
            var app = (App)Application.Current;
            if (Properties.Settings.Default.DarkThemeEnabled)
            app.SwitchTheme(true);
            FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;
            InitializeComponent();

            vm = new MainViewModel();
            DataContext = vm;

            editorSelect.SelectedIndex = 0;
            if(Environment.GetCommandLineArgs().Length > 1)
            {
                if (File.Exists(Environment.GetCommandLineArgs()[1]))
                    vm.Load(Environment.GetCommandLineArgs()[1]);
            }
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
            var efg = (ScenesViewModel)MainViewModel.Editors[0];
            int one = Project.SceneGroups[0].Children.IndexOf(efg.SelectedSceneGroup);

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
            //cast.Field00 = 3;
            //cast.Field5C = 0;
            cast.Field68 = 0;
            cast.Field6C = 0;
        }
        private void Tools_Clone(object sender, RoutedEventArgs e)
        {
            if (Shuriken.Views.UIEditor.SelectedUIObject is UIScene)
                Project.SceneGroups[0].Scenes.Add((UIScene)((UIScene)Shuriken.Views.UIEditor.SelectedUIObject).Clone());
        }

        private void Tools_MergeFolders(object sender, RoutedEventArgs e)
        {
            UISceneGroup first, second;
            if (MergeFolderParent1 != -1)
                first = Project.SceneGroups[MergeFolderParent1].Children[MergeFirstSel];
            else
                first = Project.SceneGroups[MergeFirstSel];
            if (MergeFolderParent2 != -1)
                second = Project.SceneGroups[MergeFolderParent2].Children[MergeSecondSel];
            else
                second = Project.SceneGroups[MergeSecondSel];

            var scenes = second.Scenes;
            foreach (UIScene item in scenes)
            {
                first.Scenes.Add(item);
            }
            if(MergeFolderParent2 != -1)
                Project.SceneGroups[MergeFolderParent2].Children.Remove(second);
            else

                Project.SceneGroups.Remove(second);
        }
        private void Tools_TextureRemove(object sender, RoutedEventArgs e)
        {
            
            List<bool> used = new List<bool>();
            foreach(var tex in Project.TextureLists[0].Textures)
            {
                used.Add(false);
            }
            foreach(var group in Project.SceneGroups)
            {
                foreach(var scene in group.Scenes)
                {
                    foreach(var groupS in scene.Groups)
                    {
                        foreach(var node in groupS.Casts)
                        {
                            foreach(var tex in node.Sprites)
                            {
                                int i = Project.TextureLists[0].Textures.IndexOf(Utilities.GetTextureFromSpriteID(tex, scene.OriginalScene.SubImages, Project.TextureLists[0].Textures));
                                if (i != -1)
                                    used[i] = true;
                            }
                        }
                    }
                }
            }
        }

        private void Tools_TextureFind(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = @vm.WorkFilePath;
            if (dialog.ShowDialog() == true)
            {
                List<string> textureSearch = new List<string>();
                var directory = Directory.GetParent(dialog.FileName);
                
                    foreach(var tex in vm.MissingTextures)
                    {
                        textureSearch.Add(tex);
                    }
                
                    
                DirectoryInfo directoryInWhichToSearch = directory;
                foreach(var name in textureSearch)
                {
                    string[] filesInDir = Directory.GetFiles(directory.FullName, name, SearchOption.AllDirectories);
                    
                    foreach (string file in filesInDir)
                    {
                        string dd = Directory.GetParent(vm.WorkFilePath).FullName;
                        string path = Path.Combine(dd, Path.GetFileName(file));
                        if (!File.Exists(path))
                            File.Copy(file, path, false);
                    }

                }
                //Reload file
                if (vm.WorkFilePath != null)
                    vm.Load(vm.WorkFilePath);



            }            
        }

        private void Tools_SelectSecondFolderMerge(object sender, RoutedEventArgs e)
        {
            var efg = (ScenesViewModel)MainViewModel.Editors[0];
            UISceneGroup uiobj = efg.SelectedSceneGroup;

            MergeFolderParent2 = -1;
            UISceneGroup folder = (UISceneGroup)uiobj;
            if (!Project.SceneGroups.Contains(folder))
            {
                for (int i = 0; i < Project.SceneGroups.Count; i++)
                {
                    for (int y = 0; y < Project.SceneGroups[i].Children.Count; y++)
                    {
                        if (Project.SceneGroups[i].Children[y] == folder)
                        {
                            MergeFolderParent2 = i;
                            MergeSecondSel = Project.SceneGroups[MergeFolderParent2].Children.IndexOf(folder);

                            return;
                        }
                    }
                }
            }
            MergeSecondSel = Project.SceneGroups.IndexOf(folder);
        }

        private void Tools_SelectFirstFolderMerge(object sender, RoutedEventArgs e)
        {
            var efg = (ScenesViewModel)MainViewModel.Editors[0];
            UISceneGroup uiobj = efg.SelectedSceneGroup;

            MergeFolderParent1 = -1;
            UISceneGroup folder = (UISceneGroup)uiobj;
            if (!Project.SceneGroups.Contains(folder))
            {
                for (int i = 0; i < Project.SceneGroups.Count; i++)
                {
                    for (int y = 0; y < Project.SceneGroups[i].Children.Count; y++)
                    {
                        if (Project.SceneGroups[i].Children[y] == folder)
                        {
                            MergeFolderParent1 = i;
                            MergeFirstSel = Project.SceneGroups[MergeFolderParent1].Children.IndexOf(folder);

                            return;
                        }
                    }
                }
            }
            MergeFirstSel = Project.SceneGroups.IndexOf(folder);
        }

        private void UpdateResolutionText(object sender, RoutedEventArgs e)
        {
            ResolutionHeader.Header = vm.Resolution;
        }
    }
}
