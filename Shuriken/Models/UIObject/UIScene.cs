﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNCPLib.XNCP;
using System.ComponentModel;
using Shuriken.Models.Animation;
using Shuriken.Misc;
using Shuriken.ViewModels;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Shuriken.Models
{
    public class UIScene : INotifyPropertyChanged, IComparable<UIScene>
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
        public UIScene(Scene scene, string sceneName, TextureList texList)
        {
            Name = sceneName;
            Field00 = scene.Version;
            ZIndex = scene.ZIndex;
            Field0C = scene.Field0C;
            Field10 = scene.Field10;
            AspectRatio = scene.AspectRatio;
            AnimationFramerate = scene.AnimationFramerate;
            TextureSizes = new ObservableCollection<Vector2>();
            Animations = new ObservableCollection<AnimationGroup>();
            Groups = new ObservableCollection<UICastGroup>();

            foreach (var texSize in scene.Data1)
            {
                TextureSizes.Add(new Vector2(texSize.X, texSize.Y));
            }

            ProcessCasts(scene, texList);
            Visible = false;
        }

        public UIScene(string sceneName)
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

        public UIScene(UIScene s)
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

        private void ProcessCasts(Scene scene, TextureList texList)
        {
            // Create groups
            for (int g = 0; g < scene.UICastGroups.Count; ++g)
            {
                Groups.Add(new UICastGroup
                {
                    Name = "Group_" + g,
                    RootCastIndex = scene.UICastGroups[g].RootCastIndex
                });
            }

            // Pre-process animations
            List<XNCPLib.XNCP.Animation.AnimationDictionary> AnimIDSorted = scene.AnimationDictionaries.OrderBy(o => o.Index).ToList();
            for (int a = 0; a < scene.AnimationFrameDataList.Count; a++)
            {
                Animations.Add(new AnimationGroup(AnimIDSorted[a].Name)
                {
                    Field00 = scene.AnimationFrameDataList[a].Field00,
                    Duration = scene.AnimationFrameDataList[a].FrameCount
                });
            }

            // process group layers
            List<UICast> tempCasts = new List<UICast>();
            for (int g = 0; g < Groups.Count; ++g)
            {
                for (int c = 0; c < scene.UICastGroups[g].Casts.Count; ++c)
                {
                    UICast cast = new UICast(scene.UICastGroups[g].Casts[c], GetCastName(g, c, scene.CastDictionaries), c);

                    // sprite
                    if (cast.Type == DrawType.Sprite)
                    {
                        int[] castSprites = scene.UICastGroups[g].Casts[c].CastMaterialData.SubImageIndices;
                        for (int index = 0; index < cast.Sprites.Count; ++index)
                        {
                            cast.Sprites[index] = Utilities.FindSpriteIDFromNCPScene(castSprites[index], scene.SubImages, texList.Textures);
                        }
                    }
                    else if (cast.Type == DrawType.Font)
                    {
                        foreach (var font in Project.Fonts)
                        {
                            if (font.Name == scene.UICastGroups[g].Casts[c].FontName)
                                cast.FontID = font.ID;
                        }
                    }

                    tempCasts.Add(cast);
                }

                for (int a = 0; a < scene.AnimationFrameDataList.Count; a++)
                {
                    XNCPLib.XNCP.Animation.AnimationKeyframeData keyframeData = scene.AnimationKeyframeDataList[a];
                    for (int c = 0; c < keyframeData.GroupAnimationDataList[g].CastAnimationDataList.Count; ++c)
                    {
                        XNCPLib.XNCP.Animation.CastAnimationData castAnimData = keyframeData.GroupAnimationDataList[g].CastAnimationDataList[c];
                        List<AnimationTrack> tracks = new List<AnimationTrack>((int)XNCPLib.Misc.Utilities.CountSetBits(castAnimData.Flags));

                        int castAnimDataIndex = 0;
                        for (int i = 0; i < 12; ++i)
                        {
                            // check each animation type if it exists in Flags
                            if ((castAnimData.Flags & (1 << i)) != 0)
                            {
                                AnimationType type = (AnimationType)(1 << i);
                                AnimationTrack anim = new AnimationTrack(type)
                                {
                                    Field00 = castAnimData.SubDataList[castAnimDataIndex].Field00,
                                };

                                int keyIndex = 0;
                                foreach (var key in castAnimData.SubDataList[castAnimDataIndex].Keyframes)
                                {
                                    System.Numerics.Vector3 data8Value = 
                                        MainViewModel.Type == XNCPLib.NinjaType.SonicNext ?
                                        new() :
                                        scene.AnimationData2List[a].GroupList.GroupList[g].AnimationData2List.ListData[c].Data.SubData[castAnimDataIndex].Data.Data[keyIndex].Value;
                                    anim.Keyframes.Add(new Keyframe(key, data8Value));
                                    keyIndex++;
                                }

                                tracks.Add(anim);
                                ++castAnimDataIndex;
                            }
                        }

                        if (tracks.Count > 0)
                        {
                            AnimationList layerAnimationList = new AnimationList(tempCasts[c], tracks);
                            Animations[a].LayerAnimations.Add(layerAnimationList);
                        }
                    }
                }

                // build hierarchy tree
                CreateHierarchyTree(g, scene.UICastGroups[g].CastHierarchyTree, tempCasts);

                tempCasts.Clear();
            }
        }

        private void CreateHierarchyTree(int group, List<CastHierarchyTreeNode> tree, List<UICast> lyrs)
        {
            int next = 0;
            while (next != -1)
            {
                Groups[group].Casts.Add(lyrs[next]);
                BuildTree(next, tree, lyrs, null);

                next = tree[next].NextIndex;
            }

            Groups[group].CastsOrderedByIndex = new List<UICast>(lyrs);
        }

        private void BuildTree(int c, List<CastHierarchyTreeNode> tree, List<UICast> lyrs, UICast parent)
        {
            int childIndex = tree[c].ChildIndex;
            if (childIndex != -1)
            {
                UICast child = lyrs[childIndex];
                lyrs[c].Children.Add(child);

                BuildTree(childIndex, tree, lyrs, lyrs[c]);
            }

            if (parent != null)
            {
                int siblingIndex = tree[c].NextIndex;
                if (siblingIndex != -1)
                {
                    UICast sibling = lyrs[siblingIndex];
                    parent.Children.Add(sibling);

                    BuildTree(siblingIndex, tree, lyrs, parent);
                }
            }
        }

        /// <summary>
        /// Gets the cast name from a cast dictionary provided its index and group index.
        /// If the cast is not found, an empty string is returned.
        /// </summary>
        /// <param name="groupIndex">The index of the group in which the cast belongs</param>
        /// <param name="castIndex">The index of the cast</param>
        /// <param name="castDictionary">A dictionary containing cast names, group indices and cast indices.</param>
        /// <returns></returns>
        public string GetCastName(int groupIndex, int castIndex, List<CastDictionary> castDictionary)
        {
            foreach (var entry in castDictionary)
            {
                if (entry.GroupIndex == groupIndex && entry.CastIndex == castIndex)
                    return entry.Name;
            }

            return String.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName, object before, object after)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int CompareTo(UIScene other)
        {
            return (int)(ZIndex - other.ZIndex);
        }
    }
}
