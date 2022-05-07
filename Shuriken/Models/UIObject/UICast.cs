﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using XNCPLib;
using XNCPLib.XNCP;
using Shuriken.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

namespace Shuriken.Models
{
    public class UICast : INotifyPropertyChanged, ICastContainer
    {
        private string name;
        public string Name
        { 
            get { return name; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                    name = value;
            }
        }

        public uint Field00 { get; set; }
        public DrawType Type { get; set; }
        public bool IsEnabled { get; set; }
        public Vector2 TopLeft { get; set; }
        public Vector2 BottomLeft { get; set; }
        public Vector2 TopRight { get; set; }
        public Vector2 BottomRight { get; set; }

        public Vector2 Anchor 
        { 
            get {
                float right = Math.Abs(TopRight.X) - Math.Abs(TopLeft.X);
                float top = Math.Abs(TopRight.Y) - Math.Abs(BottomRight.Y);
                return new Vector2(right, top);
            }
        }

        public uint Field2C { get; set; }
        public uint Field34 { get; set; }
        public uint Flags { get; set; }
        public uint SubImageCount { get; set; }

        public int FontID { get; set; }
        public string FontCharacters { get; set; }

        public float FontSpacingAdjustment { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }
        public uint Field58 { get; set; }
        public uint Field5C { get; set; }

        public Vector2 Offset { get; set; }
        public float Field68 { get; set; }
        public float Field6C { get; set; }
        public uint Field70 { get; set; }
        public int HideFlag { get; set; }

        public Vector2 Translation { get; set; }
        public float ZTranslation { get; set; }
        public float Rotation { get; set; }
        public Vector3 Scale { get; set; }

        public float DefaultSprite { get; set; }
        public Color Color { get ; set; }
        public Color GradientTopLeft { get; set; }
        public Color GradientBottomLeft { get; set; }
        public Color GradientTopRight { get; set; }
        public Color GradientBottomRight { get; set; }

        public uint InfoField30 { get; set; }
        public uint InfoField34 { get; set; }
        public uint InfoField38 { get; set; }

        public bool Visible { get; set; }
        public int ZIndex { get; set; }

        public ObservableCollection<int> Sprites { get; set; }
        public ObservableCollection<UICast> Children { get; set; }

        public void AddCast(UICast cast)
        {
            Children.Add(cast);
        }

        public void RemoveCast(UICast cast)
        {
            Children.Remove(cast);
        }

        public UICast(Cast cast, string name, int index)
        {
            Name = name;
            Field00 = cast.Field00;
            Type = (DrawType)cast.Field04;
            IsEnabled = cast.IsEnabled != 0;
            Visible = true;
            ZIndex = index;
            Children = new ObservableCollection<UICast>();

            TopLeft = new Vector2(cast.TopLeft);
            TopRight = new Vector2(cast.TopRight);
            BottomLeft = new Vector2(cast.BottomLeft);
            BottomRight = new Vector2(cast.BottomRight);

            Field2C = cast.Field2C;
            Field34 = cast.Field34;
            Flags = cast.Field38;
            SubImageCount = cast.SubImageCount;

            FontID = -1;
            FontCharacters = cast.FontCharacters;

            FontSpacingAdjustment = cast.FontSpacingAdjustment;
            Width = cast.Width;
            Height = cast.Height;

            if (MainViewModel.Type == NinjaType.SonicNext)
            {
                Width = (uint)((BottomRight.X - BottomLeft.X) * 1280);
                Height = (uint)((BottomLeft.Y - TopLeft.Y) * 720);
            }

            Field58 = cast.Field58;
            Field5C = cast.Field5C;

            Offset = new Vector2(cast.Offset);

            Field68 = cast.Field68;
            Field6C = cast.Field6C;
            Field70 = cast.Field70;

            HideFlag = cast.CastInfoData.HideFlag;
            Translation = new Vector2(cast.CastInfoData.Translation);
            Rotation = cast.CastInfoData.Rotation;
            Scale = new Vector3(cast.CastInfoData.Scale.X, cast.CastInfoData.Scale.Y, 1.0f);
            DefaultSprite = cast.CastInfoData.SubImage;
            Color = new Color(cast.CastInfoData.Color);
            GradientTopLeft = new Color(cast.CastInfoData.GradientTopLeft);
            GradientBottomLeft = new Color(cast.CastInfoData.GradientBottomLeft);
            GradientTopRight = new Color(cast.CastInfoData.GradientTopRight);
            GradientBottomRight = new Color(cast.CastInfoData.GradientBottomRight);
            InfoField30 = cast.CastInfoData.Field30;
            InfoField34 = cast.CastInfoData.Field34;
            InfoField38 = cast.CastInfoData.Field38;

            Sprites = new ObservableCollection<int>(Enumerable.Repeat(-1, 32).ToList());
        }

        public UICast()
        {
            Name = "Cast";
            Field00 = 0;
            Type = DrawType.Sprite;
            IsEnabled = true;
            Visible = true;
            ZIndex = 0;
            Children = new ObservableCollection<UICast>();

            Field2C = 0;
            Field34 = 0;
            Flags = 0;
            SubImageCount = 0;

            FontID = -1;
            FontCharacters = "";

            FontSpacingAdjustment = 0;
            Width = 64;
            Height = 64;
            Field58 = 0;
            Field5C = 0;

            TopLeft = new Vector2();
            TopRight = new Vector2();
            BottomLeft = new Vector2();
            BottomRight = new Vector2();

            Offset = new Vector2(0.5f, 0.5f);

            Field68 = 0;
            Field6C = 0;
            Field70 = 0;

            HideFlag = 0;
            Translation = new Vector2();
            Rotation = 0;
            Scale = new Vector3(1.0f, 1.0f, 1.0f);
            DefaultSprite = 0;
            Color = new Color(255, 255, 255, 255);
            GradientTopLeft = new Color(255, 255, 255, 255);
            GradientBottomLeft = new Color(255, 255, 255, 255);
            GradientTopRight = new Color(255, 255, 255, 255);
            GradientBottomRight = new Color(255, 255, 255, 255);
            InfoField30 = 0;
            InfoField34 = 0;
            InfoField38 = 0;

            Sprites = new ObservableCollection<int>(Enumerable.Repeat(-1, 32).ToList());

            DefaultSprite = 0;
        }

        public UICast(UICast c)
        {
            Name = name;
            Field00 = c.Field00;
            Type = c.Type;
            IsEnabled = c.IsEnabled;
            Visible = true;
            ZIndex = ZIndex;
            Children = new ObservableCollection<UICast>(c.Children);

            TopLeft = new Vector2(c.TopLeft);
            TopRight = new Vector2(c.TopRight);
            BottomLeft = new Vector2(c.BottomLeft);
            BottomRight = new Vector2(c.BottomRight);

            Field2C = c.Field2C;
            Field34 = c.Field34;
            Flags = c.Flags;
            SubImageCount = c.SubImageCount;

            FontID = -1;
            FontCharacters = c.FontCharacters;

            FontSpacingAdjustment = c.FontSpacingAdjustment;
            Width = c.Width;
            Height = c.Height;
            Field58 = c.Field58;
            Field5C = c.Field5C;

            Offset = new Vector2(c.Offset);

            Field68 = c.Field68;
            Field6C = c.Field6C;
            Field70 = c.Field70;

            HideFlag = c.HideFlag;
            Translation = new Vector2(c.Translation);
            Rotation = c.Rotation;
            Scale = new Vector3(c.Scale);
            DefaultSprite = c.DefaultSprite;
            Color = new Color(c.Color);
            GradientTopLeft = new Color(c.GradientTopLeft);
            GradientBottomLeft = new Color(c.GradientBottomLeft);
            GradientTopRight = new Color(c.GradientTopRight);
            GradientBottomRight = new Color(c.GradientBottomRight);
            InfoField30 = c.InfoField30;
            InfoField34 = c.InfoField34;
            InfoField38 = c.InfoField38;

            Sprites = new ObservableCollection<int>(c.Sprites);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
