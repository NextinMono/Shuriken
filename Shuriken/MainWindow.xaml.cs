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
            vm.Load(vm.WorkFilePath);
        }

        private void ChangeColorOrder_Click(object sender, RoutedEventArgs e)
        {
            //Could use linq? Yeah prob. I don't know how to use it tho so, figures.
            for (int a = 0; a < Project.SceneGroups.Count; a++)
            {
                for (int b = 0; b < Project.SceneGroups[a].Scenes.Count; b++)
                {
                    for (int c = 0; c < Project.SceneGroups[a].Scenes[b].Groups.Count; c++)
                    {
                        for (int d = 0; d < Project.SceneGroups[a].Scenes[b].Groups[c].Casts.Count; d++)
                        {
                            ChangeColors(Project.SceneGroups[a].Scenes[b].Groups[c].Casts[d]);
                        }
                    }
                }
            }
        }
        private void LetterboxToWidescreenTest(object sender, RoutedEventArgs e)
        {
            ////Could use linq? Yeah prob. I don't know how to use it tho so, figures.
            //for (int a = 0; a < Project.SceneGroups.Count; a++)
            //{
            //    for (int b = 0; b < Project.SceneGroups[a].Scenes.Count; b++)
            //    {
            //        for (int c = 0; c < Project.SceneGroups[a].Scenes[b].Groups.Count; c++)
            //        {
            //            for (int d = 0; d < Project.SceneGroups[a].Scenes[b].Groups[c].Casts.Count; d++)
            //            {
            //                ConvertValues(Project.SceneGroups[a].Scenes[b], Project.SceneGroups[a].Scenes[b].Groups[c].Casts[d]);
            //            }
            //        }
            //    }
            //}

            object uiobj = Views.UIEditor.SelectedUIObject;

            if (uiobj is UIScene)
            {
                UIScene scene = (UIScene)uiobj;               
                ConvertValues(scene, scene.Groups[0].Casts[0]);
            }
        }
        void ConvertValues(UIScene scene, UICast cast)
        {

            //cast.Field00 = 1;
            //cast.Field5C = 0;

            for (int i = 0; i < scene.Groups.Count; i++)
            {
                var e = scene.Groups[i].Casts[0].Scale;
                scene.Groups[i].Casts[0].Scale = new Vector3(e.X / (4.0f / 3.0f), e.Y);
            }

            //var translation = cast.Translation;
            //Vector2 nTranslation = new Vector2(((640 * translation.X) / 1280) - (640 / 1280), translation.Y);
            //cast.Translation = nTranslation;

            //var offset = cast.Offset;
            //Vector2 nOffset = new Vector2(offset.X * (640 / 1280), offset.Y);
            //cast.Offset = nOffset;

            //var e = new Vector2(cast.Translation.X + ((16 / 9) * (480 - 640)) / 2, cast.Translation.Y);
            //if (cast.Translation.X >= 0.5f)
            //cast.Translation = new Vector2((cast.Translation.X + ((16 / 9) * (480 - 640)) / 2) / 100, cast.Translation.Y);
            //else
            //    cast.Translation = new Vector2((cast.Translation.X - ((16 / 9) * (480 - 640)) / 2 )/ 100, cast.Translation.Y);
            //if(cast.Type == DrawType.Font)
            //{
            //    var scale = cast.Scale;
            //    Vector3 nScale = new Vector3(scale.X * (16/9) * 1.7f, scale.Y * (16 / 9) * 1.4f, 0);
            //    cast.Scale = nScale;

            //    cast.FontSpacingAdjustment = cast.FontSpacingAdjustment * ((16 / 9) * 3f);

            //    var pos = cast.Translation;
            //    cast.Translation = new Vector2(pos.X, pos.Y - 0.0018f);

            //}
            //else
            //{
            //    if (cast.Children.Count > 0)
            //    {
            //        var scale = cast.Scale;
            //        Vector3 nScale = new Vector3(scale.X - (((640 * scale.X) / 1280) / 3), scale.Y, 0);
            //        cast.Scale = nScale;

            //    }
            //}


            //var topleft = cast.TopLeft;
            //Vector2 nTL = new Vector2((640 * topleft.X) / 1280, (480 * topleft.Y) / 720);
            //cast.TopLeft = nTL;




            //var bottomLeft = cast.BottomLeft;
            //Vector2 nBL = new Vector2((640 * bottomLeft.X) / 1280, (480 * bottomLeft.Y) / 720);
            //cast.BottomLeft = nBL;

            //var topRight = cast.TopRight;
            //Vector2 nTR = new Vector2(((640 * topRight.X) / 1280), (480 * topRight.Y) / 720);
            //cast.TopRight = nTR;

            //var bottomRight = cast.BottomRight;
            //Vector2 nBR = new Vector2(((640 * bottomRight.X) / 1280), (480 * bottomRight.Y) / 720);
            //cast.BottomRight = nBR;



            //foreach (var f in cast.Children)
            //    ConvertValues(scene, f);
        }
        void ChangeColors(UICast cast)
        {
            //Main Color
            byte A = cast.Color.A;
            byte R = cast.Color.R;
            byte G = cast.Color.G;
            byte B = cast.Color.B;
            cast.Color.A = R;
            cast.Color.R = A;
            cast.Color.G = B;
            cast.Color.B = G;

            byte ATR = cast.GradientTopRight.A;
            byte RTR = cast.GradientTopRight.R;
            byte GTR = cast.GradientTopRight.G;
            byte BTR = cast.GradientTopRight.B;
            cast.GradientTopRight.A = RTR;
            cast.GradientTopRight.R = ATR;
            cast.GradientTopRight.G = BTR;
            cast.GradientTopRight.B = GTR;

            byte ATL = cast.GradientTopLeft.A;
            byte RTL = cast.GradientTopLeft.R;
            byte GTL = cast.GradientTopLeft.G;
            byte BTL = cast.GradientTopLeft.B;
            cast.GradientTopLeft.A = RTL;
            cast.GradientTopLeft.R = ATL;
            cast.GradientTopLeft.G = BTL;
            cast.GradientTopLeft.B = GTL;

            byte ABL = cast.GradientBottomLeft.A;
            byte RBL = cast.GradientBottomLeft.R;
            byte GBL = cast.GradientBottomLeft.G;
            byte BBL = cast.GradientBottomLeft.B;
            cast.GradientBottomLeft.A = RBL;
            cast.GradientBottomLeft.R = ABL;
            cast.GradientBottomLeft.G = BBL;
            cast.GradientBottomLeft.B = GBL;

            byte ABR = cast.GradientBottomRight.A;
            byte RBR = cast.GradientBottomRight.R;
            byte GBR = cast.GradientBottomRight.G;
            byte BBR = cast.GradientBottomRight.B;
            cast.GradientBottomRight.A = RBR;
            cast.GradientBottomRight.R = ABR;
            cast.GradientBottomRight.G = BBR;
            cast.GradientBottomRight.B = GBR;

            foreach (var f in cast.Children)
                ChangeColors(f);
        }

        private void Clone_Click(object sender, RoutedEventArgs e)
        {
            if(Shuriken.Views.UIEditor.SelectedUIObject is UIScene)
            Project.SceneGroups[0].Scenes.Add((UIScene)((UIScene)Shuriken.Views.UIEditor.SelectedUIObject).Clone());
            //if(Shuriken.Views.UIEditor.SelectedUIObject is UICast)
            //((UICast)Shuriken.Views.UIEditor.SelectedUIObject).


        }

        private void TextureBoundaries(object sender, RoutedEventArgs e)
        {
            string output = "";
            for (int i = 0; i < Project.TextureLists.Count; i++)
            {

                output += $"**{Project.TextureLists[i].Name}**";
                for (int x = 0; x < Project.TextureLists[i].Textures.Count; x++)
                {
                    output += $"\n###{Project.TextureLists[i].Textures[x].Name}###";

                    for (int g = 0; g < Project.TextureLists[i].Textures[x].Sprites.Count; g++)
                    {
                       var h =  Project.TryGetSprite(Project.TextureLists[i].Textures[x].Sprites[g]);
                        output += $"\n{g}. TopLeft: {h.Start.X}x {h.Start.Y}y - BottomRight: {h.Width + h.Start.X}x {h.Height + h.Start.Y}y";

                    }
                }
            }
            var pathFile = vm.WorkFilePath;
            var dir = Path.GetDirectoryName(pathFile);
            dir = Path.Combine(dir, (System.IO.Path.GetFileName(vm.WorkFilePath) + ".txt"));
            File.WriteAllText(dir, output);
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = dir,
                FileName = "explorer.exe"
            };
            Process.Start(startInfo);
        }
    }
}
