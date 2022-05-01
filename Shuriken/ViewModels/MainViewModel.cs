﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;
using XNCPLib.XNCP;
using Shuriken.Models;
using Shuriken.Commands;
using System.Windows;
using Shuriken.Misc;
using System.Reflection;
using Shuriken.Models.Animation;

namespace Shuriken.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public static string AppVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public List<string> MissingTextures { get; set; }
        public ObservableCollection<ViewModelBase> Editors { get; set; }

        // File Info
        public FAPCFile WorkFile { get; set; }
        public string WorkFilePath { get; set; }
        public bool IsLoaded { get; set; }
        public MainViewModel()
        {
            MissingTextures = new List<string>();

            Editors = new ObservableCollection<ViewModelBase>
            {
                new ScenesViewModel(),
                new SpritesViewModel(),
                new FontsViewModel(),
                new AboutViewModel()
            };

            IsLoaded = false;
#if DEBUG
            //LoadTestXNCP();
#endif
        }

        public void LoadTestXNCP()
        {
            Load("Test/ui_gameplay.xncp");
        }

        /// <summary>
        /// Loads a Ninja Chao Project file for editing
        /// </summary>
        /// <param name="filename">The path of the file to load</param>
        public void Load(string filename)
        {
            WorkFile = new FAPCFile();
            WorkFile.Load(filename);

            string root = Path.GetDirectoryName(Path.GetFullPath(filename));

            List<Scene> xScenes = WorkFile.Resources[0].Content.CsdmProject.Root.Scenes;
            List<SceneID> xIDs = WorkFile.Resources[0].Content.CsdmProject.Root.SceneIDTable;
            List<XTexture> xTextures = WorkFile.Resources[1].Content.TextureList.Textures;
            FontList xFontList = WorkFile.Resources[0].Content.CsdmProject.Fonts;

            Clear();

            TextureList texList = new TextureList("textures");
            foreach (XTexture texture in xTextures)
            {
                string texPath = Path.Combine(root, texture.Name);
                if (File.Exists(texPath))
                    texList.Textures.Add(new Texture(texPath));
                else
                    MissingTextures.Add(texture.Name);
            }

            if (MissingTextures.Count > 0)
                WarnMissingTextures();

            if (xScenes.Count > 0)
            {
                // Hack: we load sprites from the first scene only since whatever tool sonic team uses
                // seems to work the same way as SWIF:
                // Sprites belong to textures and layers and fonts reference a specific sprite using the texutre index and sprite index.
                int subImageIndex = 0;
                foreach (SubImage subimage in xScenes[0].SubImages)
                {
                    int textureIndex = (int)subimage.TextureIndex;
                    if (textureIndex >= 0 && textureIndex < texList.Textures.Count)
                    {
                        int id = Project.CreateSprite(texList.Textures[textureIndex], subimage.TopLeft.Y, subimage.TopLeft.X,
                            subimage.BottomRight.Y, subimage.BottomRight.X);
                        
                        texList.Textures[textureIndex].Sprites.Add(id);
                    }
                    ++subImageIndex;
                }
            }

            foreach (var entry in xFontList.FontIDTable)
            {
                int id = Project.CreateFont(entry.Name);
                UIFont font = Project.TryGetFont(id);
                foreach (var mapping in xFontList.Fonts[(int)entry.Index].CharacterMappings)
                {
                    var sprite = Utilities.FindSpriteIDFromNCPScene((int)mapping.SubImageIndex, xScenes[0].SubImages, texList.Textures);
                    font.Mappings.Add(new Models.CharacterMapping(mapping.SourceCharacter, sprite));
                }
            }

            foreach (SceneID sceneID in xIDs)
                Project.Scenes.Add(new UIScene(xScenes[(int)sceneID.Index], sceneID.Name, texList));

            Project.TextureLists.Add(texList);

            WorkFilePath = filename;
            IsLoaded = !MissingTextures.Any();
        }

        // Very barebones save method which doesn't add anything into the original NCP file, and only changes what's already there
        // It also *may* not save everything, but it's progress...
        public void Save(string path)
        {
            if (path == null) path = WorkFilePath;
            else WorkFilePath = path;

            string root = Path.GetDirectoryName(Path.GetFullPath(WorkFilePath));

            List<Scene> xScenes = WorkFile.Resources[0].Content.CsdmProject.Root.Scenes;
            List<SceneID> xIDs = WorkFile.Resources[0].Content.CsdmProject.Root.SceneIDTable;
            List<XTexture> xTextures = WorkFile.Resources[1].Content.TextureList.Textures;
            FontList xFontList = WorkFile.Resources[0].Content.CsdmProject.Fonts;

            xTextures.Clear();
            TextureList texList = Project.TextureLists[0];
            foreach (Texture texture in texList.Textures)
            {
                XTexture xTexture = new XTexture();
                xTexture.Name = texture.Name + ".dds";
                xTextures.Add(xTexture);
            }

            List<SubImage> newSubImages = new List<SubImage>();
            foreach (var entry in Project.Sprites)
            {
                Sprite sprite = entry.Value;
                int textureIndex = texList.Textures.IndexOf(sprite.Texture);

                SubImage subimage = new SubImage();
                subimage.TextureIndex = (uint)textureIndex;
                subimage.TopLeft = new Vector2((float)sprite.X / sprite.Texture.Width, (float)sprite.Y / sprite.Texture.Height);
                subimage.BottomRight = new Vector2(((float)sprite.Width / sprite.Texture.Width) + subimage.TopLeft.X, ((float)sprite.Height / sprite.Texture.Height) + subimage.TopLeft.Y);
                newSubImages.Add(subimage);
            }

            foreach (Scene scene in xScenes)
            {
                scene.SubImages = newSubImages;
            }

            xFontList.Fonts.Clear();
            xFontList.FontIDTable.Clear();
            foreach (var entry in Project.Fonts)
            {
                UIFont uiFont = entry.Value;

                FontID fontID = new FontID();
                fontID.Index = (uint)xFontList.Fonts.Count;
                fontID.Name = uiFont.Name;
                xFontList.FontIDTable.Add(fontID);

                Font font = new Font();
                foreach (var mapping in uiFont.Mappings)
                {
                    // This seems to work fine, but causes different values to be saved in ui_gameplay.xncp. Duplicate subimage entry?
                    XNCPLib.XNCP.CharacterMapping characterMapping = new XNCPLib.XNCP.CharacterMapping();
                    characterMapping.SubImageIndex = Utilities.FindSubImageIndexFromSprite(Project.TryGetSprite(mapping.Sprite), xScenes[0].SubImages, texList.Textures);
                    characterMapping.SourceCharacter = mapping.Character;
                    font.CharacterMappings.Add(characterMapping);
                }
                xFontList.Fonts.Add(font);
            }

            int sceneIndex = 0;
            foreach (SceneID sceneID in xIDs)
            {
                Scene scene = xScenes[(int)sceneID.Index];
                UIScene uiScene = Project.Scenes[sceneIndex++];

                sceneID.Name = uiScene.Name.Substring(0, sceneID.Name.Length); // TODO: This will break with names larger than the original one

                scene.Field00 = uiScene.Field00;
                scene.ZIndex = uiScene.ZIndex;
                scene.Field0C = uiScene.Field0C;
                scene.Field10 = uiScene.Field10;
                scene.AspectRatio = uiScene.AspectRatio;
                scene.AnimationFramerate = uiScene.AnimationFramerate;

                int textureSizeIndex = 0;
                for (int i = 0; i < scene.Data1.Count; ++i)
                {
                    scene.Data1[i] = uiScene.TextureSizes[textureSizeIndex++];
                }

                SaveCasts(uiScene, scene);
            }

            WorkFile.Save(path);
        }

        private void SaveHierarchyTree(UICastGroup uiCastGroup, CastGroup castGroup)
        {
            castGroup.CastHierarchyTree = new List<CastHierarchyTreeNode>();
            castGroup.CastHierarchyTree.AddRange
            (
                Enumerable.Repeat(new CastHierarchyTreeNode(-1, -1), uiCastGroup.CastsOrderedByIndex.Count)
            );

            GenerateHierarchyForCastList(uiCastGroup, uiCastGroup.Casts, castGroup.CastHierarchyTree);
        }

        private void GenerateHierarchyForCastList(UICastGroup uiCastGroup, ObservableCollection<UICast> uiCasts, List<CastHierarchyTreeNode> o_tree)
        {
            for (int i = 0; i < uiCasts.Count; i++)
            {
                UICast uiCast = uiCasts[i];

                int currentIndex = uiCastGroup.CastsOrderedByIndex.IndexOf(uiCast);
                Debug.Assert(currentIndex != -1);
                CastHierarchyTreeNode castHierarchyTreeNode = new CastHierarchyTreeNode(-1, -1);

                if (uiCast.Children.Count > 0)
                {
                    castHierarchyTreeNode.ChildIndex = uiCastGroup.CastsOrderedByIndex.IndexOf(uiCast.Children[0]);
                    Debug.Assert(castHierarchyTreeNode.ChildIndex != -1);
                }

                if (i + 1 < uiCasts.Count)
                {
                    castHierarchyTreeNode.NextIndex = uiCastGroup.CastsOrderedByIndex.IndexOf(uiCasts[i + 1]);
                    Debug.Assert(castHierarchyTreeNode.NextIndex != -1);
                }

                o_tree[currentIndex] = castHierarchyTreeNode;
                GenerateHierarchyForCastList(uiCastGroup, uiCast.Children, o_tree);
            }
        }

        private void SaveCasts(UIScene uiScene, Scene scene)
        {
            for (int g = 0; g < scene.UICastGroups.Count; ++g)
            {
                scene.UICastGroups[g].Field08 = uiScene.Groups[g].Field08;
            }

            // Pre-process animations
            Dictionary<int, int> entryIndexMap = new Dictionary<int, int>();
            int animIndex = 0;
            foreach (var entry in scene.AnimationDictionaries)
            {
                scene.AnimationFrameDataList[(int)entry.Index].Field00 = uiScene.Animations[animIndex].Field00;
                scene.AnimationFrameDataList[(int)entry.Index].FrameCount = uiScene.Animations[animIndex].Duration;

                entryIndexMap.Add(animIndex++, (int)entry.Index);
            }

            // process group layers
            for (int g = 0; g < uiScene.Groups.Count; ++g)
            {
                for (int c = 0; c < scene.UICastGroups[g].Casts.Count; ++c)
                {
                    Cast cast = scene.UICastGroups[g].Casts[c];
                    UICast uiCast = uiScene.Groups[g].CastsOrderedByIndex[c];

                    cast.Field00 = uiCast.Field00;
                    cast.Field04 = (uint)uiCast.Type;
                    cast.IsEnabled = uiCast.IsEnabled ? (uint)1 : 0;

                    /*
                    float right = Math.Abs(cast.TopRight.X) - Math.Abs(cast.TopLeft.X);
                    float top = Math.Abs(cast.TopRight.Y) - Math.Abs(cast.BottomRight.Y);
                    Anchor = new Vector2(right, top);
                    */

                    cast.TopLeft = new Vector2(uiCast.TopLeft);
                    cast.TopRight = new Vector2(uiCast.TopRight);
                    cast.BottomLeft = new Vector2(uiCast.BottomLeft);
                    cast.BottomRight = new Vector2(uiCast.BottomRight);

                    cast.Field2C = uiCast.Field2C;
                    cast.Field34 = uiCast.Field34;
                    cast.Field38 = uiCast.Flags;
                    cast.Field3C = uiCast.Field3C;

                    cast.FontCharacters = uiCast.FontCharacters;

                    cast.FontSpacingAdjustment = uiCast.FontSpacingAdjustment;
                    cast.Width = uiCast.Width;
                    cast.Height = uiCast.Height;
                    cast.Field58 = uiCast.Field58;
                    cast.Field5C = uiCast.Field5C;

                    cast.Offset = new Vector2(uiCast.Offset);

                    cast.Field68 = uiCast.Field68;
                    cast.Field6C = uiCast.Field6C;
                    cast.FontSpacingAdjustment = uiCast.FontSpacingAdjustment;

                    // Cast Info
                    cast.CastInfoData.Field00 = uiCast.InfoField00;
                    cast.CastInfoData.Translation = new Vector2(uiCast.Translation);
                    cast.CastInfoData.Rotation = uiCast.Rotation;
                    cast.CastInfoData.Scale = new Vector2(uiCast.Scale.X, uiCast.Scale.Y);

                    cast.CastInfoData.Field00 = uiCast.InfoField00;
                    cast.CastInfoData.Color = uiCast.Color.ToUint();
                    cast.CastInfoData.GradientTopLeft = uiCast.GradientTopLeft.ToUint();
                    cast.CastInfoData.GradientBottomLeft = uiCast.GradientBottomLeft.ToUint();
                    cast.CastInfoData.GradientTopRight = uiCast.GradientTopRight.ToUint();
                    cast.CastInfoData.GradientBottomRight = uiCast.GradientBottomRight.ToUint();
                    cast.CastInfoData.Field30 = uiCast.InfoField30;
                    cast.CastInfoData.Field34 = uiCast.InfoField34;
                    cast.CastInfoData.Field38 = uiCast.InfoField38;

                    if (uiCast.Type == DrawType.Sprite)
                    {
                        int[] castSprites = cast.CastMaterialData.SubImageIndices;
                        for (int index = 0; index < uiCast.Sprites.Count; ++index)
                        {
                            if (uiCast.Sprites[index] == -1)
                            {
                                castSprites[index] = -1;
                                continue;
                            }

                            Sprite uiSprite = Project.TryGetSprite(uiCast.Sprites[index]);

                            // TODO: Doesn't support new sprites
                            castSprites[index] = (int)Utilities.FindSubImageIndexFromSprite(uiSprite, scene.SubImages, Project.TextureLists[0].Textures);
                        }
                        
                    }
                    else if (uiCast.Type == DrawType.Font)
                    {
                        foreach (var font in Project.Fonts)
                        {
                            UIFont uiFont = Project.TryGetFont(uiCast.FontID);
                            if (uiFont != null)
                                cast.FontName = uiFont.Name;
                        }
                    }
                    
                }

                foreach (var entry in entryIndexMap)
                {
                    int trackIndex = 0;
                    int trackAnimIndex = 0;
                    XNCPLib.XNCP.Animation.AnimationKeyframeData keyframeData = scene.AnimationKeyframeDataList[entry.Value];
                    for (int c = 0; c < keyframeData.GroupAnimationDataList[g].CastAnimationDataList.Count; ++c)
                    {
                        XNCPLib.XNCP.Animation.CastAnimationData castAnimData = keyframeData.GroupAnimationDataList[g].CastAnimationDataList[c];

                        int castAnimDataIndex = 0;
                        List<AnimationTrack> tracks = null;
                        for (int i = 0; i < 12; ++i)
                        {
                            // check each animation type if it exists in Flags

                            // TODO: Save new anim flags
                            if ((castAnimData.Flags & (1 << i)) != 0)
                            {

                                if (tracks == null)
                                {
                                    tracks = uiScene.Animations[entry.Key].LayerAnimations[trackIndex++].Tracks.ToList();
                                    trackAnimIndex = 0;
                                }
                                AnimationTrack anim = tracks[trackAnimIndex++];

                                castAnimData.SubDataList[castAnimDataIndex].Field00 = anim.Field00;

                                int keyframeIndex = 0;
                                foreach (var key in castAnimData.SubDataList[castAnimDataIndex].Keyframes)
                                {
                                    var uiKey = anim.Keyframes[keyframeIndex++];

                                    key.Frame = uiKey.HasNoFrame ? 0xFFFFFFFF : (uint)uiKey.Frame;
                                    key.Value = uiKey.KValue;
                                    key.Field08 = (uint)uiKey.Field08;
                                    key.Offset1 = uiKey.Offset1;
                                    key.Offset2 = uiKey.Offset2;
                                    key.Field14 = (uint)uiKey.Field14;
                                }

                                ++castAnimDataIndex;
                            }
                        }
                    }
                }

                // TODO: Save Cast Hierarchy tree
            }
        }

        public void Clear()
        {
            Project.Clear();
            MissingTextures.Clear();
        }

        private void WarnMissingTextures()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("The loaded UI file uses textures that were not found. Saving has been disabled. In order to save, please copy the files listed below into the UI file's directory, and re-open it.\n");
            foreach (var texture in MissingTextures)
                builder.AppendLine(texture);

            MessageBox.Show(builder.ToString(), "Missing Textures", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
