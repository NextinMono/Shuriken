using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using Shuriken.Models;
using Shuriken.ViewModels;
using XNCPLib.XNCP;

namespace Shuriken.Misc
{
    public static class Utilities
    {
        //For GVR, taken from GVRTool, which in turn takes it from Puyotools
        public static ushort SwapU16(ushort value)
        {
            return (ushort)((value << 8) | (value >> 8));
        }

        public static uint SwapU32(uint value)
        {
            return (value << 24) | ((value & 0x0000FF00) << 8) | ((value & 0x00FF0000) >> 8) | (value >> 24);
        }
        public static uint ReadUInt32Endian(this BinaryReader br, bool bigEndian)
        {
            if (bigEndian) return SwapU32(br.ReadUInt32());
            else return br.ReadUInt32();
        }
        public static ushort ReadUInt16Endian(this BinaryReader br, bool bigEndian)
        {
            if (bigEndian) return SwapU16(br.ReadUInt16());
            else return br.ReadUInt16();
        }
        //
        public static Shuriken.Models.Color ReverseColor(SharpNeedle.Color<byte> color)
        {
            return new Shuriken.Models.Color(color.A, color.B, color.G, color.R);
        }
        public static float ToRadians(float degrees)
        {
            return degrees * MathF.PI / 180.0f;
        }

        public static float ToDegrees(float radians)
        {
            return radians * 180.0f / MathF.PI;
        }

        public static float ConvertRange(float value, float oldLow, float oldHigh, float newLow, float newHigh)
        {
            float percent = (value - oldLow) / oldHigh;
            return (percent * (newHigh - newLow)) + newLow;
        }

        public static TreeViewItem GetParentTreeViewItem(TreeViewItem child)
        {
            if (child != null)
            {
                var parent = VisualTreeHelper.GetParent(child);
                while (parent is not TreeViewItem && parent != null)
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                return parent is TreeViewItem ? parent as TreeViewItem : null;
            }

            return null;
        }

        public static int ToPixels(float v, float factor)
        {
            return (int)(v * factor);
        }
        public static int FindSpriteIDFromNCPScene(int spriteIndex, List<SharpNeedle.Ninja.Csd.Sprite> spriteList, ObservableCollection<Texture> textures)
        {
            if (spriteIndex >= 0 && spriteIndex < spriteList.Count)
            {
                int textureIndex = (int)spriteList[spriteIndex].TextureIndex;
                if (textureIndex >= 0 && textureIndex < textures.Count)
                {
                    var sprites = textures[textureIndex].Sprites;
                    var target = spriteList[spriteIndex];
                    Sprite targetToCompare = new Sprite(0, textures[textureIndex], target.TopLeft.Y, target.TopLeft.X,
                        target.BottomRight.Y, target.BottomRight.X);
                    for (int s = 0; s < sprites.Count; ++s)
                    {
                        var spr = Project.TryGetSprite(sprites[s]);

                        if (spr.X == targetToCompare.X && spr.Y == targetToCompare.Y
                            && spr.Width == targetToCompare.Width
                            && spr.Height == targetToCompare.Height)
                        {
                            return textures[textureIndex].Sprites[s];
                        }
                    }
                }
            }

            return -1;
        }
        public static int FindSpriteIDFromNCPScene(int spriteIndex, List<SubImage> spriteList, ObservableCollection<Texture> textures)
        {
            if (spriteIndex >= 0 && spriteIndex < spriteList.Count)
            {
                int textureIndex = (int)spriteList[spriteIndex].TextureIndex;
                if (textureIndex >= 0 && textureIndex < textures.Count)
                {
                    var sprites = textures[textureIndex].Sprites;
                    var target = spriteList[spriteIndex];
                    Sprite targetToCompare = new Sprite(0, textures[textureIndex], target.TopLeft.Y, target.TopLeft.X,
                        target.BottomRight.Y, target.BottomRight.X);
                    for (int s = 0; s < sprites.Count; ++s)
                    {
                        var spr = Project.TryGetSprite(sprites[s]);

                        if (spr.X == targetToCompare.X && spr.Y == targetToCompare.Y
                            && spr.Width == targetToCompare.Width
                            && spr.Height == targetToCompare.Height)
                        {
                            return textures[textureIndex].Sprites[s];
                        }
                    }
                }
            }

            return -1;
        }
    }
}