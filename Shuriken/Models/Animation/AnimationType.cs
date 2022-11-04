using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuriken.Models;
using System.ComponentModel;

namespace Shuriken.Models.Animation
{
    public enum AnimationType : uint
    {
        [Description("None")]
        None        = 0,
        [Description("Hide Flag")]
        HideFlag     = 1,
        [Description("X Position")]
        XPosition   = 2,
        [Description("Y Position")]
        YPosition   = 4,
        [Description("Rotation")]
        Rotation    = 8,
        [Description("X Scale")]
        XScale      = 16,
        [Description("Y Scale")]
        YScale      = 32,
        [Description("Subimage")]
        SubImage    = 64,
        [Description("Color")]
        Color       = 128,
        [Description("Gradient Top-Left")]
        GradientTL  = 256,
        [Description("Gradient Bottom-Left")]
        GradientBL  = 512,
        [Description("Gradient Top-Right")]
        GradientTR  = 1024,
        [Description("Gradient Bottom-Right")]
        GradientBR  = 2048
    }

    public static class AnimationTypeMethods
    {
        public static bool IsColor(this AnimationType type)
        {
            return new AnimationType[] { 
                AnimationType.Color,
                AnimationType.GradientTL,
                AnimationType.GradientBL,
                AnimationType.GradientTR,
                AnimationType.GradientBR
            }.Contains(type);
        }
    }
}
