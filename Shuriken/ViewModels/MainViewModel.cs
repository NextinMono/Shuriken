using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;
using XNCPLib;
using XNCPLib.XNCP;
using XNCPLib.XNCP.Animation;
using Shuriken.Models;
using Shuriken.Commands;
using System.Windows;
using Shuriken.Misc;
using System.Reflection;
using Shuriken.Models.Animation;
using SharpNeedle.Ninja.Csd;
using SharpNeedle.Utilities;
using Amicitia.IO.Binary;

namespace Shuriken.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public static string AppVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public List<string> MissingTextures { get; set; }
        public ObservableCollection<ViewModelBase> Editors { get; set; }

        private List<SubImage> ncpSubimages;

        // File Info
        public FAPCFile WorkFile { get; set; }
        public CsdProject WorkProjectCsd;
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
            ncpSubimages = new List<SubImage>();
        }

        void GetSubImages(SharpNeedle.Ninja.Csd.SceneNode node)
        {
            foreach (var scene in node.Scenes)
            {
                if (ncpSubimages.Count > 0)
                    return;


                foreach (var item in scene.Value.Sprites)
                {
                    var i = new SubImage();
                    i.TextureIndex = (uint)item.TextureIndex;
                    i.TopLeft = item.TopLeft;
                    i.BottomRight = item.BottomRight;
                    ncpSubimages.Add(i);
                }
            }

            foreach (KeyValuePair<string, SceneNode> child in node.Children)
            {
                if (ncpSubimages.Count > 0)
                    return;

                GetSubImages(child.Value);
            }
        }


        private void ProcessSceneGroups(SharpNeedle.Ninja.Csd.SceneNode xNode, ShurikenUISceneGroup parent, TextureList texlist, string name)
        {
            ShurikenUISceneGroup ShurikenUISceneGroup = new(name);

            // process node scenes
            foreach (var item in xNode.Scenes)
            {
                ShurikenUISceneGroup.Scenes.Add(new ShurikenUIScene(item.Value, item.Key, texlist));

            }

            if (parent != null)
                parent.Children.Add(ShurikenUISceneGroup);
            else
                Project.SceneGroups.Add(ShurikenUISceneGroup);

            foreach (var item in xNode.Children)
                ProcessSceneGroups(item.Value, ShurikenUISceneGroup, texlist, item.Key);
        }
        private void LoadSubimages(TextureList texList, List<SubImage> subimages)
        {
            foreach (var image in subimages)
            {
                int textureIndex = (int)image.TextureIndex;
                if (textureIndex >= 0 && textureIndex < texList.Textures.Count)
                {
                    int id = Project.CreateSprite(texList.Textures[textureIndex], image.TopLeft.Y, image.TopLeft.X,
                        image.BottomRight.Y, image.BottomRight.X);

                    texList.Textures[textureIndex].Sprites.Add(id);
                }
            }
        }

        /// <summary>
        /// Loads a Ninja Chao Project file for editing
        /// </summary>
        /// <param name="filename">The path of the file to load</param>
        public void Load(string filename)
        {
            string root = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(filename));
            
            Clear();
            ncpSubimages.Clear();
            //Catch errors if the program is built in release mode (since losing progress is awful)
#if !DEBUG
            //try
            //{
#endif
            string extension = System.IO.Path.GetExtension(filename).ToLower();
            if (extension ==".swif")
            {
                List<TextureList> texList = new List<TextureList>();
                SharpNeedle.SurfRide.Draw.SrdProject srdProject = new SharpNeedle.SurfRide.Draw.SrdProject();
                srdProject = ResourceUtility.Open<SharpNeedle.SurfRide.Draw.SrdProject>(@filename);
                var swScenes = srdProject.Project.Scenes;
                var swTextureLists = srdProject.Project.TextureLists;
                var swFontLists = srdProject.Project.Fonts;
                Clear();

                foreach (var textureList in swTextureLists)
                {
                    var texList2 = new TextureList(textureList.Name);
                    foreach (SharpNeedle.SurfRide.Draw.Texture texture in textureList)
                    {
                        string texPath = Path.Combine(root, texture.Name + ".dds");
                        if (File.Exists(texPath))
                        {
                            Texture tex = new Texture(texPath);
                            foreach (SharpNeedle.SurfRide.Draw.Crop subimage in texture)
                            {
                                int id = Project.CreateSprite(tex, subimage.Top, subimage.Left,
                                    subimage.Bottom, subimage.Right);
                                tex.Sprites.Add(id);
                            }

                            texList2.Textures.Add(tex);
                        }
                        else
                        {
                            MissingTextures.Add(texture.Name);
                        }
                    }

                    Project.TextureLists.Add(texList2);
                }

                foreach (var fontList in swFontLists)
                {
                    // Implement Texture List Index and Texture Index too here.
                    UIFont font = new UIFont(fontList.Name, -1);
                    for (int index = 0; index < fontList.Count; ++index)
                    {
                        var texList3 = Project.TextureLists.ElementAt(fontList[index].TextureListIndex);
                        Texture texture = texList3.Textures.ElementAt(fontList[index].TextureListIndex);
                        var sprite = texture.Sprites.ElementAt(fontList[index].TextureIndex);
                        font.Mappings.Add(new Models.CharacterMapping(Convert.ToChar(fontList[index].Code), sprite));
                    }

                    Project.Fonts.Add(font);
                }

                ShurikenUISceneGroup sceneGroup = new ShurikenUISceneGroup("Test");
                foreach (var scene in swScenes)
                {
                    sceneGroup.Scenes.Add(new ShurikenUIScene(srdProject.Project, scene, scene.Name, Project.TextureLists, Project.Fonts));
                }
                Project.SceneGroups.Add(sceneGroup);

                if (MissingTextures.Count > 0)
                    WarnMissingTextures();
            }
            else
            {
                WorkProjectCsd = new CsdProject();

                using var infoReader = new BinaryObjectReader(@filename, Endianness.Big, Encoding.ASCII);
                uint fpacSignature = infoReader.ReadNative<uint>();
                //If the file is actually just the InfoChunk (ends in IF), warn the user and avoid loading the file
                if (((fpacSignature >> 16) & 0xFFFF) == 0x4649)
                {                    
                    MessageBox.Show("This UI file does not contain a texture list.\nMerge the file with its texture list before opening it in this program.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;                    
                }
                else
                    WorkProjectCsd = ResourceUtility.Open<CsdProject>(@filename);


                ITextureList xTextures = WorkProjectCsd.Textures;
                CsdDictionary<SharpNeedle.Ninja.Csd.Font> xFontList = WorkProjectCsd.Project.Fonts;

                TextureList texList = new TextureList("textures");
                if (xTextures != null)
                {
                    bool tempChangeExtension = false;
                    string t = Path.GetExtension(xTextures[0].Name).ToLower();
                    if (t != ".dds")
                    {
                        //MessageBox.Show("This tool is not capable of loading non-dds images yet, convert them to dds manually to make them show up in the tool.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                        tempChangeExtension = true;
                    }
                    foreach (ITexture texture in xTextures)
                    {
                        string texPath = System.IO.Path.Combine(@root, texture.Name);
                        //if (tempChangeExtension)
                        //{
                        //    texPath = Path.ChangeExtension(texPath, "dds");
                        //}

                        if (File.Exists(texPath))
                            texList.Textures.Add(new Texture(texPath, tempChangeExtension));
                        else
                            MissingTextures.Add(texture.Name);
                    }
                }
                else
                {
                    //GNCP/SNCP requires TXDs
                    if(extension == ".gncp" || extension == ".sncp")
                    {
                        GSncpImportWindow windowImport = new GSncpImportWindow();
                        windowImport.ShowDialog();
                    }
                    else
                        MessageBox.Show("The loaded UI file has an invalid texture list, textures will not load.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                if (MissingTextures.Count > 0)
                    WarnMissingTextures();

                GetSubImages(WorkProjectCsd.Project.Root);
                LoadSubimages(texList, ncpSubimages);

                List<FontID> fontID = new List<FontID>();

                if (xFontList != null)
                {
                    //Parse fonts from CsdProject
                    foreach (KeyValuePair<string, SharpNeedle.Ninja.Csd.Font> mFont in xFontList)
                    {
                        int id = Project.CreateFont(mFont.Key);
                        UIFont font = Project.TryGetFont(id);
                        foreach (var mCharacterMap in mFont.Value)
                        {
                            var sprite = Utilities.FindSpriteIDFromNCPScene((int)mCharacterMap.DestinationIndex, ncpSubimages, texList.Textures);
                            font.Mappings.Add(new Models.CharacterMapping((char)mCharacterMap.SourceIndex, sprite));
                        }
                    }
                }


                // ProcessSceneGroups(WorkFile.Resources[0].Content.CsdmProject.Root, null, texList, WorkFile.Resources[0].Content.CsdmProject.ProjectName);
                ProcessSceneGroups(WorkProjectCsd.Project.Root, null, texList, WorkProjectCsd.Project.Name);

                Project.TextureLists.Add(texList);

                WorkFilePath = filename;
                IsLoaded = !MissingTextures.Any();
            }
            #if !DEBUG
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"The UI file is either unsupported or corrupt.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
            #endif

        }

        public void Save(string path)
        {
            if (path == null) path = WorkFilePath;
            else WorkFilePath = path;

            // TODO: We should create a FACPFile from scratch instead of overwritting the working one
            List<XTexture> xTextures = WorkFile.Resources[1].Content.TextureList.Textures;
            FontList xFontList = WorkFile.Resources[0].Content.CsdmProject.Fonts;

            List<SubImage> subImageList = new();
            List<Shuriken.Models.Sprite> spriteList = new();
            BuildSubImageList(ref subImageList, ref spriteList);

            SaveTextures(xTextures);
            SaveFonts(xFontList, spriteList);

            List<System.Numerics.Vector2> Data1 = new();
            TextureList texList = Project.TextureLists[0];
            foreach (Texture tex in texList.Textures)
            {
                Data1.Add(new System.Numerics.Vector2(tex.Width / 1280F, tex.Height / 720F));
            }

            CSDNode rootNode = new();
            SaveNode(rootNode, Project.SceneGroups[0], subImageList, Data1, spriteList);
            WorkFile.Resources[0].Content.CsdmProject.Root = rootNode;

            WorkFile.Save(path);
        }

        private void SaveNode(CSDNode xNode, ShurikenUISceneGroup ShurikenUISceneGroup, List<SubImage> subImageList, List<System.Numerics.Vector2> Data1, List<Shuriken.Models.Sprite> spriteList)
        {
            for (int s = 0; s < ShurikenUISceneGroup.Scenes.Count; ++s)
            {
                SaveScenes(xNode, ShurikenUISceneGroup, subImageList, Data1, spriteList);
            }

            for (int i = 0; i < ShurikenUISceneGroup.Children.Count; ++i)
            {
                NodeDictionary dictionary = new();
                dictionary.Name = ShurikenUISceneGroup.Children[i].Name;
                dictionary.Index = (uint)i;
                xNode.NodeDictionaries.Add(dictionary);

                CSDNode newNode = new();
                SaveNode(newNode, ShurikenUISceneGroup.Children[i], subImageList, Data1, spriteList);
                xNode.Children.Add(newNode);
            }

            // Sort node names
            xNode.NodeDictionaries = xNode.NodeDictionaries.OrderBy(o => o.Name, StringComparer.Ordinal).ToList();
        }

        private void BuildSubImageList(ref List<SubImage> subImages, ref List<Shuriken.Models.Sprite> spriteList)
        {
            subImages = new();
            spriteList = new();

            TextureList texList = Project.TextureLists[0];
            foreach (var entry in Project.Sprites)
            {
                Shuriken.Models.Sprite sprite = entry.Value;
                int textureIndex = texList.Textures.IndexOf(sprite.Texture);
                spriteList.Add(sprite);

                SubImage subImage = new();
                subImage.TextureIndex = (uint)textureIndex;
                subImage.TopLeft = new Vector2((float)sprite.X / sprite.Texture.Width, (float)sprite.Y / sprite.Texture.Height);
                subImage.BottomRight = new Vector2((float)(sprite.X + sprite.Width) / sprite.Texture.Width, (float)(sprite.Y + sprite.Height) / sprite.Texture.Height);
                subImages.Add(subImage);
            }
        }

        private void SaveTextures(List<XTexture> xTextures)
        {
            xTextures.Clear();
            TextureList texList = Project.TextureLists[0];
            foreach (Texture texture in texList.Textures)
            {
                XTexture xTexture = new();
                xTexture.Name = texture.Name + ".dds";
                xTextures.Add(xTexture);
            }
        }

        private void SaveFonts(FontList xFontList, List<Shuriken.Models.Sprite> spriteList)
        {
            xFontList.Fonts.Clear();
            xFontList.FontIDTable.Clear();

            TextureList texList = Project.TextureLists[0];
            foreach (var uiFont in Project.Fonts)
            {
                // NOTE: need to sort by name after
                FontID fontID = new();
                fontID.Index = (uint)xFontList.FontIDTable.Count;
                fontID.Name = uiFont.Name;
                xFontList.FontIDTable.Add(fontID);

                XNCPLib.XNCP.Font font = new();
                foreach (var mapping in uiFont.Mappings)
                {
                    // This seems to work fine, but causes different values to be saved in ui_gameplay.xncp. Duplicate subimage entry?
                    XNCPLib.XNCP.CharacterMapping characterMapping = new();
                    characterMapping.SubImageIndex = (uint)spriteList.IndexOf(Project.TryGetSprite(mapping.Sprite));
                    characterMapping.SourceCharacter = mapping.Character;
                    Debug.Assert(characterMapping.SubImageIndex != 0xFFFFFFFF);
                    font.CharacterMappings.Add(characterMapping);
                }
                xFontList.Fonts.Add(font);
            }

            // Sort font names
            xFontList.FontIDTable = xFontList.FontIDTable.OrderBy(o => o.Name, StringComparer.Ordinal).ToList();
        }

        private void SaveScenes(CSDNode xNode, ShurikenUISceneGroup uiSGroup, List<SubImage> subImageList, List<System.Numerics.Vector2> Data1, List<Shuriken.Models.Sprite> spriteList)
        {
            //xNode.Scenes.Clear();
            //xNode.SceneIDTable.Clear();
            //
            //// Save individual scenes
            //for (int s = 0; s < uiSGroup.Scenes.Count; s++)
            //{
            //    ShurikenUIScene ShurikenUIScene = uiSGroup.Scenes[s];
            //    XNCPLib.XNCP.Scene xScene = new();
            //
            //    // Save scene parameters
            //    xScene.Version = ShurikenUIScene.Field00;
            //    xScene.ZIndex = ShurikenUIScene.ZIndex;
            //    xScene.AnimationFramerate = ShurikenUIScene.AnimationFramerate;
            //    xScene.Field0C = ShurikenUIScene.Field0C;
            //    xScene.Field10 = ShurikenUIScene.Field10;
            //    xScene.AspectRatio = ShurikenUIScene.AspectRatio;
            //    xScene.Data1 = Data1;
            //    xScene.SubImages = subImageList;
            //
            //    // Initial AnimationKeyframeData so we can add groups and cast data in it
            //    foreach (AnimationGroup animGroup in ShurikenUIScene.Animations)
            //    {
            //        AnimationKeyframeData keyframeData = new();
            //        xScene.AnimationKeyframeDataList.Add(keyframeData);
            //
            //        AnimationData2 animationData2 = new();
            //        animationData2.GroupList = new();
            //        animationData2.GroupList.GroupList = new();
            //        animationData2.GroupList.Field00 = 0; // TODO:
            //        xScene.AnimationData2List.Add(animationData2);
            //
            //        // Add animation names, NOTE: need to be sorted after
            //        AnimationDictionary animationDictionary = new();
            //        animationDictionary.Index = (uint)xScene.AnimationDictionaries.Count;
            //        animationDictionary.Name = animGroup.Name;
            //        xScene.AnimationDictionaries.Add(animationDictionary);
            //
            //        // AnimationFrameDataList
            //        AnimationFrameData animationFrameData = new();
            //        animationFrameData.Field00 = animGroup.Field00;
            //        animationFrameData.FrameCount = animGroup.Duration;
            //        xScene.AnimationFrameDataList.Add(animationFrameData);
            //    }
            //
            //    // Sort animation names
            //    xScene.AnimationDictionaries = xScene.AnimationDictionaries.OrderBy(o => o.Name, StringComparer.Ordinal).ToList();
            //
            //    for (int g = 0; g < ShurikenUIScene.Groups.Count; g++)
            //    {
            //        CastGroup xCastGroup = new();
            //        UICastGroup uiCastGroup = ShurikenUIScene.Groups[g];
            //
            //        xCastGroup.RootCastIndex = uiCastGroup.RootCastIndex;
            //        SaveCasts(uiCastGroup.CastsOrderedByIndex, xCastGroup, spriteList);
            //
            //        // Save the hierarchy tree for the current group
            //        xCastGroup.CastHierarchyTree = new();
            //        xCastGroup.CastHierarchyTree.AddRange
            //        (
            //            Enumerable.Repeat(new CastHierarchyTreeNode(-1, -1), uiCastGroup.CastsOrderedByIndex.Count)
            //        );
            //        SaveHierarchyTree(uiCastGroup.Casts, uiCastGroup.CastsOrderedByIndex, xCastGroup.CastHierarchyTree);
            //
            //        // Add cast name to dictionary, NOTE: this need to be sorted after
            //        for (int c = 0; c < uiCastGroup.CastsOrderedByIndex.Count; c++)
            //        {
            //            CastDictionary castDictionary = new();
            //            castDictionary.Name = uiCastGroup.CastsOrderedByIndex[c].Name;
            //            castDictionary.GroupIndex = (uint)g;
            //            castDictionary.CastIndex = (uint)c;
            //            xScene.CastDictionaries.Add(castDictionary);
            //        }
            //        xScene.UICastGroups.Add(xCastGroup);
            //
            //        // Take this oppotunatity to fill group cast keyframe data
            //        for (int a = 0; a < xScene.AnimationKeyframeDataList.Count; a++)
            //        {
            //            AnimationKeyframeData animationKeyframeData = xScene.AnimationKeyframeDataList[a];
            //            AnimationGroup animation = ShurikenUIScene.Animations[a];
            //
            //            GroupAnimationData2 groupAnimationData2 = new();
            //            groupAnimationData2.AnimationData2List = new();
            //            groupAnimationData2.AnimationData2List.ListData = new();
            //
            //            GroupAnimationData groupAnimationData = new();
            //            for (int c = 0; c < uiCastGroup.CastsOrderedByIndex.Count; c++)
            //            {
            //                CastAnimationData2 castAnimationData2 = new();
            //                castAnimationData2.Data = new();
            //                CastAnimationData castAnimationData = new();
            //
            //                ShurikenUIElement uiCast = uiCastGroup.CastsOrderedByIndex[c];
            //                for (int t = 0; t < 12; t++)
            //                {
            //                    AnimationType type = (AnimationType)(1u << t);
            //                    AnimationTrack animationTrack = animation.GetTrack(uiCast, type);
            //                    if (animationTrack == null) continue;
            //                    castAnimationData.Flags |= (uint)type;
            //
            //                    // Initialize if we haven't
            //                    if (castAnimationData2.Data.SubData == null)
            //                    {
            //                        castAnimationData2.Data.SubData = new();
            //                    }
            //
            //                    Data6 data6 = new();
            //                    data6.Data = new();
            //                    data6.Data.Data = new();
            //
            //                    CastAnimationSubData castAnimationSubData = new();
            //                    castAnimationSubData.Field00 = animationTrack.Field00;
            //                    foreach (Models.Animation.Keyframe keyframe in animationTrack.Keyframes)
            //                    {
            //                        XNCPLib.XNCP.Animation.Keyframe xKeyframe = new();
            //                        xKeyframe.Frame = keyframe.HasNoFrame ? 0xFFFFFFFF : (uint)keyframe.Frame;
            //                        xKeyframe.Value = keyframe.KValue;
            //                        xKeyframe.Type = keyframe.Type;
            //                        xKeyframe.InTangent = keyframe.InTangent;
            //                        xKeyframe.OutTangent = keyframe.OutTangent;
            //                        xKeyframe.Field14 = (uint)keyframe.Field14;
            //                        castAnimationSubData.Keyframes.Add(xKeyframe);
            //
            //                        Data8 data8 = new();
            //                        data8.Value = new System.Numerics.Vector3(keyframe.Data8Value.X, keyframe.Data8Value.Y, keyframe.Data8Value.Z);
            //                        data6.Data.Data.Add(data8);
            //                    }
            //
            //                    castAnimationData2.Data.SubData.Add(data6);
            //                    castAnimationData.SubDataList.Add(castAnimationSubData);
            //                }
            //
            //                groupAnimationData2.AnimationData2List.ListData.Add(castAnimationData2);
            //                groupAnimationData.CastAnimationDataList.Add(castAnimationData);
            //            }
            //
            //            AnimationData2 animationData2 = xScene.AnimationData2List[a];
            //            animationData2.GroupList.GroupList.Add(groupAnimationData2);
            //            animationKeyframeData.GroupAnimationDataList.Add(groupAnimationData);
            //        }
            //    }
            //
            //    // Sort cast names
            //    xScene.CastDictionaries = xScene.CastDictionaries.OrderBy(o => o.Name, StringComparer.Ordinal).ToList();
            //
            //    // Add scene name to dictionary, NOTE: this need to sorted after
            //    SceneID xSceneID = new();
            //    xSceneID.Name = ShurikenUIScene.Name;
            //    xSceneID.Index = (uint)s;
            //    xNode.SceneIDTable.Add(xSceneID);
            //    xNode.Scenes.Add(xScene);
            //}
            //
            //// Sort scene names
            //xNode.SceneIDTable = xNode.SceneIDTable.OrderBy(o => o.Name, StringComparer.Ordinal).ToList();
        }

        private void SaveHierarchyTree(ObservableCollection<ShurikenUIElement> children, List<ShurikenUIElement> uiCastList, List<CastHierarchyTreeNode> tree)
        {
            for (int i = 0; i < children.Count; i++)
            {
                ShurikenUIElement uiCast = children[i];

                int currentIndex = uiCastList.IndexOf(uiCast);
                Debug.Assert(currentIndex != -1);
                CastHierarchyTreeNode castHierarchyTreeNode = new(-1, -1);

                if (uiCast.Children.Count > 0)
                {
                    castHierarchyTreeNode.ChildIndex = uiCastList.IndexOf(uiCast.Children[0]);
                    Debug.Assert(castHierarchyTreeNode.ChildIndex != -1);
                }

                if (i + 1 < children.Count)
                {
                    castHierarchyTreeNode.NextIndex = uiCastList.IndexOf(children[i + 1]);
                    Debug.Assert(castHierarchyTreeNode.NextIndex != -1);
                }

                tree[currentIndex] = castHierarchyTreeNode;
                SaveHierarchyTree(uiCast.Children, uiCastList, tree);
            }
        }

        private void SaveCasts(List<ShurikenUIElement> uiCastList, CastGroup xCastGroup, List<Shuriken.Models.Sprite> spriteList)
        {
            foreach (ShurikenUIElement uiCast in uiCastList)
            {
                XNCPLib.XNCP.Cast xCast = new();

                xCast.Field00 = uiCast.Field00;
                xCast.Field04 = (uint)uiCast.Type;
                xCast.IsEnabled = uiCast.IsEnabled ? 1u : 0u;

                xCast.TopLeft = new Vector2(uiCast.TopLeft);
                xCast.TopRight = new Vector2(uiCast.TopRight);
                xCast.BottomLeft = new Vector2(uiCast.BottomLeft);
                xCast.BottomRight = new Vector2(uiCast.BottomRight);

                xCast.Field2C = uiCast.Field2C;
                xCast.Field34 = (uint)uiCast.InheritanceFlags;
                xCast.Field38 = (uint)uiCast.Flags;
                xCast.SubImageCount = uiCast.SubImageCount;

                xCast.FontCharacters = uiCast.FontCharacters;
                if (uiCast.Type == DrawType.Font)
                {
                    UIFont uiFont = Project.TryGetFont(uiCast.FontID);
                    if (uiFont != null)
                    {
                        xCast.FontName = uiFont.Name;
                    }
                }
                xCast.FontSpacingAdjustment = uiCast.FontSpacingAdjustment;

                xCast.Width = (uint)uiCast.Size.X;
                xCast.Height = (uint)uiCast.Size.Y;
                xCast.Field58 = uiCast.Field58;
                xCast.Field5C = uiCast.Field5C;

                xCast.Offset = new Vector2(uiCast.Offset);

                xCast.Field68 = uiCast.Field68;
                xCast.Field6C = uiCast.Field6C;
                xCast.Field70 = uiCast.Field70;

                // Cast Info
                xCast.CastInfoData = new();
                xCast.CastInfoData.HideFlag = uiCast.HideFlag;
                xCast.CastInfoData.Translation = new Vector2(uiCast.Translation);
                xCast.CastInfoData.Rotation = uiCast.Rotation;
                xCast.CastInfoData.Scale = new(uiCast.Scale.X, uiCast.Scale.Y);
                xCast.CastInfoData.SubImage = uiCast.DefaultSprite;
                xCast.CastInfoData.Color = uiCast.Color.ToUint();
                xCast.CastInfoData.GradientTopLeft = uiCast.GradientTopLeft.ToUint();
                xCast.CastInfoData.GradientBottomLeft = uiCast.GradientBottomLeft.ToUint();
                xCast.CastInfoData.GradientTopRight = uiCast.GradientTopRight.ToUint();
                xCast.CastInfoData.GradientBottomRight = uiCast.GradientBottomRight.ToUint();
                xCast.CastInfoData.Field30 = uiCast.InfoField30;
                xCast.CastInfoData.Field34 = uiCast.InfoField34;
                xCast.CastInfoData.Field38 = uiCast.InfoField38;

                // Cast Material Info
                xCast.CastMaterialData = new();
                Debug.Assert(uiCast.Sprites.Count == 32);
                for (int index = 0; index < 32; index++)
                {
                    if (uiCast.Sprites[index] == -1)
                    {
                        xCast.CastMaterialData.SubImageIndices[index] = -1;
                        continue;
                    }

                    Shuriken.Models.Sprite uiSprite = Project.TryGetSprite(uiCast.Sprites[index]);
                    xCast.CastMaterialData.SubImageIndices[index] = spriteList.IndexOf(uiSprite);
                }

                xCastGroup.Casts.Add(xCast);
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
