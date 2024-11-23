using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using Shuriken.Models;
using Shuriken.Rendering;
using Shuriken.ViewModels;
using Shuriken.Models.Animation;
using Shuriken.Misc;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Wpf;
using Vector2 = System.Numerics.Vector2;

namespace Shuriken.Views
{
    using Vec2 = Models.Vector2;
    using Vec3 = Models.Vector3;
    

    /// <summary>
    /// Interaction logic for UIEditor.xaml
    /// </summary>
    public partial class UIEditor : UserControl
    {
        public static Vec2 ViewResolution = new Vec2(1280, 720);
        Converters.ColorToBrushConverter colorConverter;
        Renderer renderer;

        public UIEditor()
        {
            InitializeComponent();

            GLWpfControlSettings glSettings = new GLWpfControlSettings
            {
                GraphicsProfile = ContextProfile.Core,
                MajorVersion = 3,
                MinorVersion = 3,
            };

            glControl.Start(glSettings);

            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.CullFace);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.FramebufferSrgb);

            colorConverter = new Converters.ColorToBrushConverter();
            renderer = new Renderer(1280, 720);
        }

        private void glControlRender(TimeSpan obj)
        {
            var sv = DataContext as ScenesViewModel;
            if (sv == null) 
                return;
            
            GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            float deltaTime = obj.Milliseconds / 1000.0f * 60.0f;
            sv.SizeX = ViewResolution.X;
            sv.SizeY = ViewResolution.Y;
            sv.Tick(deltaTime);
            renderer.SetShader(renderer.shaderDictionary["basic"]);

            UpdateSceneGroups(Project.SceneGroups, Project.Fonts, sv.Time);

            GL.Finish();
        }

        private void UpdateSceneGroups(IEnumerable<ShurikenUISceneGroup> groups, IEnumerable<UIFont> fonts, float time)
        {
            foreach (var group in groups)
            {
                if (!group.Visible) 
                    continue;

                UpdateSceneGroups(group.Children, fonts, time);

                List<ShurikenUIScene> sortedScenes = group.Scenes.ToList();
                sortedScenes.Sort();

                UpdateScenes(group.Scenes, fonts, time);
            }
        }

        private void UpdateScenes(IEnumerable<ShurikenUIScene> scenes, IEnumerable<UIFont> fonts, float time)
        {
            foreach (var scene in scenes)
            {
                if (!scene.Visible)
                    continue;

                foreach (var group in scene.Groups)
                {
                    if (!group.Visible)
                        continue;

                    renderer.Width = (int)ViewResolution.X;
                    renderer.Height = (int)ViewResolution.Y;
                    renderer.Start();

                    foreach (var lyr in group.Casts) 
                        UpdateCast(scene, lyr, new CastTransform(), time);

                    renderer.End();
                }
            }
        }

        private void UpdateCast(ShurikenUIScene scene, ShurikenUIElement in_UiElement, CastTransform transform, float time)
        {
            bool hideFlag = in_UiElement.HideFlag != 0;
            var position = new Vec2(in_UiElement.Translation.X, in_UiElement.Translation.Y);
            float rotation = in_UiElement.Rotation;
            var scale = new Vec2(in_UiElement.Scale.X, in_UiElement.Scale.Y);
            float sprID = in_UiElement.DefaultSprite;
            var color = in_UiElement.Color;
            var gradientTopLeft = in_UiElement.GradientTopLeft;
            var gradientBottomLeft = in_UiElement.GradientBottomLeft;
            var gradientTopRight = in_UiElement.GradientTopRight;
            var gradientBottomRight = in_UiElement.GradientBottomRight;

            foreach (var animation in scene.Animations)
            {
                if (!animation.Enabled)
                    continue;

                for (int i = 0; i < 12; i++)
                {
                    var type = (AnimationType)(1 << i);
                    var track = animation.GetTrack(in_UiElement, type);

                    if (track == null)
                        continue;

                    switch (type)
                    {
                        case AnimationType.HideFlag:
                            hideFlag = track.GetSingle(time) != 0;
                            break;
                        
                        case AnimationType.XPosition:
                            position.X = track.GetSingle(time);
                            break;
                        
                        case AnimationType.YPosition:
                            position.Y = track.GetSingle(time);
                            break;
                        
                        case AnimationType.Rotation:
                            rotation = track.GetSingle(time);
                            break;

                        case AnimationType.XScale:
                            scale.X = track.GetSingle(time);
                            break;
                        
                        case AnimationType.YScale:
                            scale.Y = track.GetSingle(time);
                            break;

                        case AnimationType.SubImage:
                            sprID = track.GetSingle(time);
                            break;

                        case AnimationType.Color:
                            color = track.GetColor(time);
                            break;
                        
                        case AnimationType.GradientTL:
                            gradientTopLeft = track.GetColor(time);
                            break;
                        
                        case AnimationType.GradientBL:
                            gradientBottomLeft = track.GetColor(time);
                            break;

                        case AnimationType.GradientTR:
                            gradientTopRight = track.GetColor(time);
                            break;

                        case AnimationType.GradientBR:
                            gradientBottomRight = track.GetColor(time);
                            break;
                    }
                }
            }

            if (hideFlag)
                return;

            // Inherit position scale
            // TODO: Is this handled through flags?
            position.X *= transform.Scale.X;
            position.Y *= transform.Scale.Y;

            // Rotate through parent transform
            float angle = Utilities.ToRadians(transform.Rotation);
            float rotatedX = position.X * MathF.Cos(angle) * scene.AspectRatio + position.Y * MathF.Sin(angle);
            float rotatedY = position.Y * MathF.Cos(angle) - position.X * MathF.Sin(angle) * scene.AspectRatio;

            position.X = rotatedX / scene.AspectRatio;
            position.Y = rotatedY;

            position += in_UiElement.Offset;

            // Inherit position
            if (in_UiElement.InheritanceFlags.HasFlag(ElementInheritanceFlags.InheritXPosition))
                position.X += transform.Position.X;

            if (in_UiElement.InheritanceFlags.HasFlag(ElementInheritanceFlags.InheritYPosition))
                position.Y += transform.Position.Y;

            // Inherit rotation
            if (in_UiElement.InheritanceFlags.HasFlag(ElementInheritanceFlags.InheritRotation))
                rotation += transform.Rotation;

            // Inherit scale
            if (in_UiElement.InheritanceFlags.HasFlag(ElementInheritanceFlags.InheritScaleX))
                scale.X *= transform.Scale.X;

            if (in_UiElement.InheritanceFlags.HasFlag(ElementInheritanceFlags.InheritScaleY))
                scale.Y *= transform.Scale.Y;

            // Inherit color
            if (in_UiElement.InheritanceFlags.HasFlag(ElementInheritanceFlags.InheritColor))
            {
                Vector4 cF = Vector4.Multiply(color.ToFloats(), transform.Color.ToFloats());
                color = new Color(cF.X, cF.Y, cF.Z, cF.W);
            }

            if (in_UiElement.Visible && in_UiElement.IsEnabled)
            {
                if (in_UiElement.Type == DrawType.Sprite)
                {
                    var spr = sprID >= 0 ? Project.TryGetSprite(in_UiElement.Sprites[Math.Min(in_UiElement.Sprites.Count - 1, (int)sprID)]) : null;
                    var nextSpr = sprID >= 0 ? Project.TryGetSprite(in_UiElement.Sprites[Math.Min(in_UiElement.Sprites.Count - 1, (int)sprID + 1)]) : null;

                    spr ??= nextSpr;
                    nextSpr ??= spr;
                    if(in_UiElement.RendererType == ElementType.Csd)
                    {
                        renderer.DrawSprite(
                            in_UiElement.TopLeft, in_UiElement.BottomLeft, in_UiElement.TopRight, in_UiElement.BottomRight,
                            position, Utilities.ToRadians(rotation), scale, scene.AspectRatio, spr, nextSpr, sprID % 1, color,
                            gradientTopLeft, gradientBottomLeft, gradientTopRight, gradientBottomRight,
                            in_UiElement.ZIndex, in_UiElement.Flags);
                    }
                    else
                    {
                       // renderer.DrawSprite(position, rotation, new Vec3(lyr.Width, lyr.Height, 1.0f) * scale, spr,
                       //lyr.Flags, color.ToFloats(), gradients[0].ToFloats(), gradients[2].ToFloats(), gradients[3].ToFloats(), gradients[1].ToFloats(), lyr.ZIndex);
                        //renderer.DrawSprite(
                        //    in_UiElement.TopLeft, in_UiElement.BottomLeft, in_UiElement.TopRight, in_UiElement.BottomRight,
                        //    position, Utilities.ToRadians(rotation), scale, scene.AspectRatio, spr, nextSpr, sprID % 1, color,
                        //    gradientTopLeft, gradientBottomLeft, gradientTopRight, gradientBottomRight,
                        //    in_UiElement.ZIndex, in_UiElement.Flags);
                    }   
                }
                else if (in_UiElement.Type == DrawType.Font)
                {
                    float xOffset = 0.0f;
                    if (in_UiElement.FontCharacters == null)
                        in_UiElement.FontCharacters = "";
                    for (var i = 0; i < in_UiElement.FontCharacters.Length; i++)
                    {
                        var font = Project.TryGetFont(in_UiElement.FontID);
                        if (font == null)
                            continue;

                        Sprite spr = null;

                        foreach (var mapping in font.Mappings)
                        {
                            if (mapping.Character != in_UiElement.FontCharacters[i]) 
                                continue;
                            
                            spr = Project.TryGetSprite(mapping.Sprite);
                            break;
                        }

                        if (spr == null)
                            continue;

                        float width = spr.Dimensions.X / renderer.Width;
                        float height = spr.Dimensions.Y / renderer.Height;

                        var begin = (Vector2)in_UiElement.TopLeft;
                        var end = begin + new Vector2(width, height);

                        renderer.DrawSprite(
                            new Vector2(begin.X + xOffset, begin.Y),
                            new Vector2(begin.X + xOffset, end.Y),
                            new Vector2(end.X + xOffset, begin.Y),
                            new Vector2(end.X + xOffset, end.Y),
                            position, Utilities.ToRadians(rotation), scale, scene.AspectRatio, spr, spr, 0, color,
                            gradientTopLeft, gradientBottomLeft, gradientTopRight, gradientBottomRight,
                            in_UiElement.ZIndex, in_UiElement.Flags
                        );

                        xOffset += width + in_UiElement.FontSpacingAdjustment;
                    }
                }

                var childTransform = new CastTransform(position, rotation, scale, color);

                foreach (var child in in_UiElement.Children) 
                    UpdateCast(scene, child, childTransform, time);
            }
        }

        private void ScenesTreeViewSelected(object sender, RoutedEventArgs e)
        {
            TreeViewItem source = e.OriginalSource as TreeViewItem;
            TreeViewItem item = source;
            
            // Move up the tree view until we reach the TreeViewItem holding the ShurikenUIScene
            while (item != null && item.DataContext != null && item.DataContext is not ShurikenUIScene)
                item = Utilities.GetParentTreeViewItem(item);

            if (DataContext is ScenesViewModel vm)
            {
                vm.SelectedScene = item == null ? null : item.DataContext as ShurikenUIScene;
                vm.SelectedUIObject = source.DataContext;

                TreeViewItem parent = Utilities.GetParentTreeViewItem(source);
                vm.ParentNode = parent == null ? null : parent.DataContext;
            }
        }

        private void SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            var tree = e.OriginalSource as TreeView;
            if (tree.Items.Count < 1)
            {
                if (DataContext is ScenesViewModel vm)
                    vm.SelectedUIObject = vm.ParentNode = vm.SelectedScene = null;
            }
        }

        private void NodesTreeViewSelectedItemChanged(object sender, RoutedEventArgs e)
        {
            var item = e.OriginalSource as TreeViewItem;
            if (DataContext is ScenesViewModel vm)
                vm.SelectedSceneGroup = item.DataContext as ShurikenUISceneGroup;
        }

        private void NewCastClickGroup(object sender, RoutedEventArgs e)
        {
            if (DataContext is ScenesViewModel vm)
            {
               ((UICastGroup)vm.SelectedUIObject).Casts.Add(new ShurikenUIElement());
            }
        }
    }
}
