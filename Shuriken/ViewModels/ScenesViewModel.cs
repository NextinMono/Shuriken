using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Shuriken.Models;
using Shuriken.Commands;
using Shuriken.Views;

namespace Shuriken.ViewModels
{
    public class ScenesViewModel : ViewModelBase
    {
        public float SizeX { get; set; } = 1280;
        public float SizeY { get; set; } = 10;
        public float MinZoom => 0.25f;
        public float MaxZoom => 2.50f;
        public float Time { get; set; }
        public bool Playing { get; set; }
        public float PlaybackSpeed { get; set; }

        private float zoom;
        public float Zoom
        {
            get => zoom;
            set { zoom = Math.Clamp(value, MinZoom, MaxZoom); }
        }
        
        public RelayCommand TogglePlayingCmd { get; }
        public RelayCommand StopPlayingCmd { get; }
        public RelayCommand ReplayCmd { get; }
        public RelayCommand<float> SeekCmd { get; }
        public RelayCommand ZoomOutCmd { get; }
        public RelayCommand ZoomInCmd { get; }

        public RelayCommand CreateSceneCmd { get; }
        public RelayCommand RemoveSceneCmd { get; }
        public RelayCommand CloneSceneCmd { get; }
        public RelayCommand CreateGroupCmd { get; }
        public RelayCommand RemoveGroupCmd { get; }
        public RelayCommand ChangeColorsTmp { get; }
        public RelayCommand<int> ChangeCastSpriteCmd { get; }
        public RelayCommand CreateCastCmd { get; }
        public RelayCommand RemoveCastCmd { get; }

        public void SelectCastSprite(object index)
        {
            SpritePickerWindow dialog = new SpritePickerWindow(Project.TextureLists);
            dialog.ShowDialog();

            if (dialog.DialogResult == true)
            {
                if (dialog.SelectedSpriteID != -1)
                {
                    int sprIndex = (int)(SelectedUIObject as UICast).CastNumber;
                    ChangeCastSprite(sprIndex, dialog.SelectedSpriteID);
                }
            }
        }

        public void ChangeCastSprite(int index, int sprID)
        {
            try
            {
                if (SelectedUIObject is UICast)
                {
                    var cast = (UICast)SelectedUIObject;
                    cast.Sprites[index] = sprID;
                }

            }
            catch(ArgumentOutOfRangeException ex)
            {
#if DEBUG
                System.Diagnostics.Debugger.Break();
#else
                System.Windows.MessageBox.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
#endif
            }
        }

        public void TogglePlaying()
        {
            Playing ^= true;
        }

        public void Stop()
        {
            Playing = false;
            Time = 0.0f;
        }

        public void Replay()
        {
            Stop();
            TogglePlaying();
        }

        public void Seek(object frames)
        {
            float f = (float)frames;
            if (Time + f >= 0.0f)
                Time += f;
        }

        public void Tick(float d)
        {
            Time += d * PlaybackSpeed * (Playing ? 1 : 0);
        }

        public void CreateScene()
        {
            //Should probably make it so that it just creates a new list, but I didn't want to mess with the thing too much.
            if (Scenes != null)
            Scenes.Add(new UIScene("scene"));
        }

        public void RemoveSelectedScene()
        {
            Scenes.Remove(SelectedScene);
        }

        public void AddGroupToSelection()
        {
            SelectedScene.Groups.Add(new UICastGroup());
        }

        public void RemoveSelectedGroup()
        {
            if (SelectedUIObject is UICastGroup group)
                SelectedScene.Groups.Remove(group);
        }

        public void AddCastToSelection()
        {
            if (SelectedUIObject is ICastContainer container)
                container.AddCast(new UICast());
        } 
        public void CloneCastToSelection()
        {
            if (SelectedUIObject is ICastContainer container)
            {
                UICast c = (UICast)SelectedUIObject;
                for (int i = 0; i < SelectedScene.Groups.Count; i++)
                {
                    for (int x = 0; x < SelectedScene.Groups[i].Casts.Count; x++)
                    {
                        if (E(SelectedScene.Groups[i].Casts[x], SelectedScene.Groups[i].Casts[0], SelectedScene.Groups[i], SelectedScene))
                            return;
                    }
                }
                
                

                



            }
        }

        bool E(UICast cast, UICast cast2, UICastGroup group, UIScene scene)
        {
            if (cast.Name == (SelectedUIObject as UICast).Name)
            {
                UICast newc = (UICast)(SelectedUIObject as UICast).Clone();

                cast2.AddCast(newc);
                group.CastsOrderedByIndex.Add(newc);

                for (int i = 0; i < scene.Animations.Count; i++)
                {
                    for (int x = 0; x < scene.Animations[i].LayerAnimations.Count; x++)
                    {
                        if (scene.Animations[i].LayerAnimations[x].Layer == cast)
                        {
                            var g = (Models.Animation.AnimationList)scene.Animations[i].LayerAnimations[x].Clone();
                            g.Layer = newc;
                            scene.Animations[i].LayerAnimations.Add(g);
                            break;
                        }
                    }
                    
                }

                return true;
            }
            else
            {
                for (int i = 0; i < cast.Children.Count; i++)
                {
                   if( E(cast.Children[i], cast, group, scene))
                        return true;
                }
                return false;
            }
        }

        public void RemoveSelectedCast()
        {
            if (ParentNode is ICastContainer container)
                container.RemoveCast(SelectedUIObject as UICast);
        }

        public void CreateSceneGroup()
        {
            throw new NotImplementedException();
        }

        public void RemoveSelecteSceneGroup()
        {
            throw new NotImplementedException();
        }

        public UISceneGroup SelectedSceneGroup { get; set; }
        public UIScene SelectedScene { get; set; }
        public object ParentNode { get; set; }
        public object SelectedUIObject { get; set; }
        public ObservableCollection<UISceneGroup> SceneGroups => Project.SceneGroups;
        public ObservableCollection<UIScene> Scenes => SelectedSceneGroup?.Scenes;

        public ScenesViewModel()
        {
            DisplayName = "Scenes";
            IconCode = "\xf008";

            zoom = 0.65f;
            PlaybackSpeed = 1.0f;

            TogglePlayingCmd    = new RelayCommand(() => Playing ^= true, null);
            StopPlayingCmd      = new RelayCommand(Stop, null);
            ReplayCmd           = new RelayCommand(Replay, null);
            SeekCmd             = new RelayCommand<float>(Seek, () => !Playing);
            ZoomOutCmd          = new RelayCommand(() => Zoom -= 0.25f, null);
            ZoomInCmd           = new RelayCommand(() => Zoom += 0.25f, null);

            CreateSceneCmd      = new RelayCommand(CreateScene, null);
            RemoveSceneCmd      = new RelayCommand(RemoveSelectedScene, () => SelectedScene != null);
            CloneSceneCmd      = new RelayCommand(CloneSelectedScene, () => SelectedScene != null);
            CreateGroupCmd      = new RelayCommand(AddGroupToSelection, () => SelectedScene != null);
            RemoveGroupCmd      = new RelayCommand(RemoveSelectedGroup, () => SelectedUIObject is UICastGroup);
            CreateCastCmd       = new RelayCommand(CloneCastToSelection, () => SelectedUIObject is ICastContainer);
            RemoveCastCmd       = new RelayCommand(RemoveSelectedCast, () => SelectedUIObject is UICast);
            ChangeCastSpriteCmd = new RelayCommand<int>(SelectCastSprite, () => SelectedUIObject is UICast);
        }

      
        private void CloneSelectedScene()
        {

            //snew.Groups = SelectedScene.Groups;
            //snew.Field00 = SelectedScene.Field00;
            //snew.Field0C = SelectedScene.Field0C;
            //snew.Field10 = SelectedScene.Field10;
            //snew.AnimationFramerate = SelectedScene.AnimationFramerate;
            //snew.Animations = SelectedScene.Animations;
            //snew.AspectRatio = SelectedScene.AspectRatio;
            //snew.Name = SelectedScene.Name + "_Clone";
            //snew.TextureSizes = SelectedScene.TextureSizes;
            //snew.Visible = SelectedScene.Visible;
            //snew.ZIndex = SelectedScene.ZIndex;
            Scenes.Add((UIScene)SelectedScene.Clone());
            //SelectedScene.
        }
    }
}
