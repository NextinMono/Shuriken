using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using XNCPLib.XNCP;
using System.ComponentModel;
using Shuriken.Models.Animation;
using Shuriken.Misc;
using Shuriken.ViewModels;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Diagnostics;
using SharpNeedle.Ninja.Csd;
using SharpNeedle.Ninja.Csd.Motions;
using SharpNeedle.SurfRide.Draw;
using XNCPLib.XNCP;

namespace Shuriken.Models
{
    public class ShurikenUIScene : INotifyPropertyChanged, IComparable<ShurikenUIScene>
    {
        private string name;
        public string Name
        {
            get => name;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    name = value;
            }
        }

        public uint Field00 { get; set; }
        public float ZIndex { get; set; }
        public uint Field0C { get; set; }
        public float Field10 { get; set; }
        public float AspectRatio { get; set; }
        public float AnimationFramerate { get; set; }
        public bool Visible { get; set; }

        public ObservableCollection<Vector2> TextureSizes { get; set; }
        public ObservableCollection<UICastGroup> Groups { get; set; }
        public ObservableCollection<AnimationGroup> Animations { get; set; }
        //public ShurikenUIScene(Scene scene, string sceneName, TextureList texList)
        //{
        //    Name = sceneName;
        //    Field00 = scene.Version;
        //    ZIndex = scene.ZIndex;
        //    Field0C = scene.Field0C;
        //    Field10 = scene.Field10;
        //    AspectRatio = scene.AspectRatio;
        //    AnimationFramerate = scene.AnimationFramerate;
        //    TextureSizes = new ObservableCollection<Vector2>();
        //    Animations = new ObservableCollection<AnimationGroup>();
        //    Groups = new ObservableCollection<UICastGroup>();
        //
        //    foreach (var texSize in scene.Data1)
        //    {
        //        TextureSizes.Add(new Vector2(texSize.X, texSize.Y));
        //    }
        //
        //    //ProcessCasts(scene, texList);
        //    Visible = false;
        //}
        public ShurikenUIScene(SharpNeedle.Ninja.Csd.Scene scene, string sceneName, TextureList texList)
        {
            Name = sceneName;
            Field00 = (uint)scene.Version;
            ZIndex = scene.Priority;
            //Field0C = scene.Field0C;
            //Field10 = scene.Field10;
            AspectRatio = scene.AspectRatio;
            AnimationFramerate = scene.FrameRate;
            TextureSizes = new ObservableCollection<Vector2>();
            Animations = new ObservableCollection<AnimationGroup>();
            Groups = new ObservableCollection<UICastGroup>();

            foreach (var texSize in scene.Textures)
            {
                TextureSizes.Add(new Vector2(texSize.X, texSize.Y));
            }

            ProcessCastsSharpNeedle(scene, texList);
            Visible = false;
        }
        public ShurikenUIScene(SharpNeedle.SurfRide.Draw.ProjectChunk project, SharpNeedle.SurfRide.Draw.Scene scene, string sceneName, IEnumerable<TextureList> texLists, IEnumerable<UIFont> fonts)
        {
            Name = sceneName;
            AspectRatio = scene.Resolution.X / scene.Resolution.Y;
            AnimationFramerate = project.FrameRate;
            TextureSizes = new ObservableCollection<Vector2>();
            Groups = new ObservableCollection<UICastGroup>();

            ProcessSWCasts(scene, texLists, fonts);
            Visible = false;
        }
        private void ProcessSWCasts(SharpNeedle.SurfRide.Draw.Scene scene, IEnumerable<TextureList> texLists, IEnumerable<UIFont> fonts)
        {
            // Create groups
            for (int g = 0; g < scene.Layers.Count; ++g)
            {
                Groups.Add(new UICastGroup
                {
                    Name = scene.Layers[g].Name,
                });

                // process group layers
                var tempCasts = new List<ShurikenUIElement>();

                var castIndexMap = new Dictionary<int, int>();
                for (int c = 0; c < scene.Layers[g].Cells.Count; ++c)
                {
                    var cast = new ShurikenUIElement(scene.Layers[g].Nodes[c], (Cell3D)scene.Layers[g].Cells[c],
                        scene.Resolution, scene.Layers[g].Nodes[c].Name, c);

                    castIndexMap.Add(scene.Layers[g].Nodes[c].ID, c);

                    if (cast.Type == DrawType.Sprite)
                    {
                        var cropRefs = ((ImageCastData)scene.Layers[g].Nodes[c].Data).Surface.CropRefs;
                        for (int index = 0; index < cropRefs.Count; ++index)
                        {
                            var texList = texLists.ElementAt(cropRefs[index].TextureListIndex);
                            var texture = texList.Textures.ElementAt(cropRefs[index].TextureListIndex);
                            cast.Sprites[index] = texture.Sprites.ElementAt(cropRefs[index].TextureIndex);
                        }
                    }
                    else if (cast.Type == DrawType.Font)
                    {
                        var fontdata = (ImageCastData)scene.Layers[g].Nodes[c].Data;
                        cast.FontID = (int)fontdata.FontData.FontListIndex;
                    }

                    Groups[g].Casts.Add(cast);
                }
            }
            int animations = 0;
            Animations = new ObservableCollection<AnimationGroup>();
            //foreach (var animation in scene.Layers[g].Animations)
            //{
            //    Groups[g].Add(new(animation.Name)
            //    {
            //        Duration = animation.FrameCount
            //    });
            //
            //    foreach (var link in animation.AnimationLinks)
            //    {
            //        var tracks = new List<AnimationTrack>();
            //        foreach (var track in link.Tracks)
            //        {
            //            var type = AnimationType.None;
            //            switch (track.AnimationType)
            //            {
            //                case 0:
            //                    type = AnimationType.XPosition;
            //                    break;
            //                case 1:
            //                    type = AnimationType.YPosition;
            //                    break;
            //                case 2:
            //                    type = AnimationType.ZPosition;
            //                    break;
            //                case 6:
            //                    type = AnimationType.XScale;
            //                    break;
            //                case 7:
            //                    type = AnimationType.YScale;
            //                    break;
            //                case 8:
            //                    type = AnimationType.ZScale;
            //                    break;
            //                case 17:
            //                    type = AnimationType.SubImage;
            //                    break;
            //                case 21:
            //                    type = AnimationType.ColorRed;
            //                    break;
            //                case 22:
            //                    type = AnimationType.ColorGreen;
            //                    break;
            //                case 23:
            //                    type = AnimationType.ColorBlue;
            //                    break;
            //                case 24:
            //                    type = AnimationType.ColorAlpha;
            //                    break;
            //                default:
            //                    break;
            //            }
            //
            //            var anim = new AnimationTrack(type, track.Flags);
            //            foreach (var key in track.Keys)
            //            {
            //                var keyframe = new Keyframe(key);
            //                switch (type)
            //                {
            //                    case AnimationType.XPosition:
            //                        keyframe.KValue /= scene.FrameSize.X;
            //                        break;
            //                    case AnimationType.YPosition:
            //                        keyframe.KValue /= -scene.FrameSize.Y;
            //                        break;
            //                    default:
            //                        break;
            //                }
            //
            //                anim.Keyframes.Add(keyframe);
            //
            //            }
            //
            //            tracks.Add(anim);
            //        }
            //
            //        var layerAnimationList = new AnimationList(tempCasts[castIndexMap[link.CastID]], tracks);
            //        Groups[g].Animations[animations].LayerAnimations.Add(layerAnimationList);
            //    }
            //
            //    animations++;
            //}

            //// build hierarchy tree
            //CreateHierarchyTree(g, scene.Layers[g].CastNodes, tempCasts);


        }
        public ShurikenUIScene(string sceneName)
        {
            Name = sceneName;
            ZIndex = 0;
            AspectRatio = 16.0f / 9.0f;
            AnimationFramerate = 60.0f;
            Groups = new ObservableCollection<UICastGroup>();
            TextureSizes = new ObservableCollection<Vector2>();
            Animations = new ObservableCollection<AnimationGroup>();

            Visible = false;
        }

        public ShurikenUIScene(ShurikenUIScene s)
        {
            Name = s.Name;
            ZIndex = s.ZIndex;
            AspectRatio = s.AspectRatio;
            AnimationFramerate = s.AnimationFramerate;

            Groups = new ObservableCollection<UICastGroup>(s.Groups);
            TextureSizes = new ObservableCollection<Vector2>(s.TextureSizes);
            Animations = new ObservableCollection<AnimationGroup>(s.Animations);


            Visible = false;
        }
        #region Conversion Utilities
        ShurikenUIElement ConvertSharpCast(SharpNeedle.Ninja.Csd.Cast in_Cast, SharpNeedle.Ninja.Csd.Scene in_Scene, TextureList in_TexList)
        {
            ShurikenUIElement cast = new ShurikenUIElement(in_Cast, in_Cast.Name, in_Cast.Priority);

            if (cast.Type == DrawType.Sprite)
            {
                int[] castSprites = in_Cast.SpriteIndices;
                for (int index = 0; index < cast.Sprites.Count; ++index)
                {
                    cast.Sprites[index] = Misc.Utilities.FindSpriteIDFromNCPScene(castSprites[index], in_Scene.Sprites, in_TexList.Textures);
                }
            }
            else if (cast.Type == DrawType.Font)
            {
                foreach (var font in Project.Fonts)
                {
                    if (font.Name == in_Cast.FontName)
                        cast.FontID = font.ID;
                }
            }
            return cast;
        }
        ShurikenUIElement RecursiveConvertCast(SharpNeedle.Ninja.Csd.Cast in_ParentCast, SharpNeedle.Ninja.Csd.Scene in_Scene, TextureList in_TexList)
        {
            ShurikenUIElement cast = ConvertSharpCast(in_ParentCast,in_Scene,in_TexList);
            foreach (var child in in_ParentCast.Children)
            {
                cast.Children.Add(RecursiveConvertCast(child, in_Scene, in_TexList));
            }
            return cast;
        }
        ShurikenUIElement FindElementFromCsdCast(ShurikenUIElement node, SharpNeedle.Ninja.Csd.Cast searchValue)
        {
            if (node.CastCsd.Name == searchValue.Name && node.CastCsd.Position == searchValue.Position && node.CastCsd.Priority == searchValue.Priority)
            {
                return node;
            }

            foreach (var child in node.Children)
            {
                var element = FindElementFromCsdCast(child, searchValue);
                if (element != null)
                {
                    return element;
                }
            }

            return null;
        }
        #endregion
        private void ProcessCastsSharpNeedle(SharpNeedle.Ninja.Csd.Scene scene, TextureList texList)
        {
            // Process groups (convert them from SharpNeedle elements to Shuriken elements)
            for (int g = 0; g < scene.Families.Count; ++g)
            {
                //Create new group
                Groups.Add(new UICastGroup
                {
                    Name = "Family_" + Groups.Count
                });

                //Get root cast
                SharpNeedle.Ninja.Csd.Cast csdCast = scene.Families[g].Casts[0];

                //Process casts and its children by converting all of them to Shuriken nodes
                ShurikenUIElement castNew = ConvertSharpCast(csdCast, scene, texList);
                foreach (SharpNeedle.Ninja.Csd.Cast csdCastChild in csdCast.Children)
                {
                    castNew.Children.Add(RecursiveConvertCast(csdCastChild, scene, texList));
                }

                //This basically adds the root node
                Groups[g].AddCast(castNew);
            }
            ///This... awful nested for loop is here to replicate the same functionality from XNCPLib
            ///Fortunately and unfortunately I dont think SharpNeedle stores cast indices, and since
            ///we're not using the sharpneedle types directly for drawing them onto the screen,
            ///we just kinda have to do this to have animations
            ///
            ///Kill this with fire.
            foreach (var sceneMotion in scene.Motions)
            {
                var keyframeData = sceneMotion.Value;
                Animations.Add(new AnimationGroup(sceneMotion.Key));

                foreach (var familyMotion in sceneMotion.Value.FamilyMotions)
                {
                    foreach (var casMot in familyMotion.CastMotions)
                    {
                        CastMotion castAnimData = casMot;
                        List<AnimationTrack> tracks = new List<AnimationTrack>((int)XNCPLib.Misc.Utilities.CountSetBits(castAnimData.Flags));

                        int castAnimDataIndex = 0;
                        //Loop through all possible animation types
                        for (int i = 0; i < 12; ++i)
                        {
                            // check each animation type if it exists in Flags
                            if ((castAnimData.Flags & (1 << i)) != 0)
                            {
                                AnimationType type = (AnimationType)(1 << i);
                                AnimationTrack anim = new AnimationTrack(type)
                                {
                                    Field00 = castAnimData[castAnimDataIndex].Field00,
                                };

                                int keyIndex = 0;
                                foreach (SharpNeedle.Ninja.Csd.Motions.KeyFrame key in castAnimData[castAnimDataIndex].Frames)
                                {
                                    System.Numerics.Vector3 data8Value;
                                    if (scene.Version < 3)
                                    {
                                        data8Value = new();
                                    }
                                    else
                                    {
                                        if (key.Correction != null)
                                        {
                                            data8Value = new System.Numerics.Vector3(key.Correction.Value.Center.X, key.Correction.Value.Center.Y, key.Correction.Value.Offset);
                                        }
                                        else
                                            data8Value = new();
                                    }

                                    anim.Keyframes.Add(new Keyframe(key, data8Value));
                                    keyIndex++;
                                }

                                tracks.Add(anim);
                                ++castAnimDataIndex;
                            }
                        }
                        //If there are tracks, add them to the scene
                        if (tracks.Count > 0)
                        {
                            ShurikenUIElement element = null;
                            foreach (var g in Groups)
                            {
                                foreach (var t in g.Casts)
                                {
                                    element = FindElementFromCsdCast(t, casMot.Cast);
                                    if (element != null)
                                    {
                                        break;
                                    }
                                }
                                AnimationList layerAnimationList = new AnimationList(element, tracks);
                                Animations[^1].LayerAnimations.Add(layerAnimationList);
                            }
                        }
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName, object before, object after)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int CompareTo(ShurikenUIScene other)
        {
            return (int)(ZIndex - other.ZIndex);
        }
    }
}
