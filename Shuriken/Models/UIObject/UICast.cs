using System;
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
using SharpNeedle.Ninja.Csd;
using Cast = XNCPLib.XNCP.Cast;
using Shuriken.Misc.Extensions;
using Shuriken.Views;


namespace Shuriken.Models
{
    [Flags]
    public enum ElementInheritanceFlags
    {
        None = 0,
        InheritRotation = 0x2,
        InheritColor = 0x8,
        InheritXPosition = 0x100,
        InheritYPosition = 0x200,
        InheritScaleX = 0x400,
        InheritScaleY = 0x800,
    }
    [Flags]
    public enum ElementMaterialFlags
    {
        None = 0,
        AdditiveBlending = 0x1,
        MirrorX = 0x400,
        MirrorY = 0x800,
        LinearFiltering = 0x1000
    }
    public enum ElementMaterialFiltering
    {
        [Description("Nearest")]
        NearestNeighbor = 0,
        [Description("Linear")]
        Linear = 1
    }
    public enum ElementMaterialBlend
    {
        [Description("Normal")]
        Normal = 0,
        [Description("Additive")]
        Additive = 1
    }
    public class ShurikenUIElement : INotifyPropertyChanged, ICastContainer
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
        public ElementInheritanceFlags InheritanceFlags { get; set; }
        public ElementMaterialFlags Flags { get; set; }
        public uint SubImageCount { get; set; }

        public int FontID { get; set; }
        public string FontCharacters { get; set; }

        public float FontSpacingAdjustment { get; set; }
        public Vector2 Size { get; set; }
        public uint Field58 { get; set; }
        public uint Field5C { get; set; }

        public Vector2 Offset { get; set; }
        public float Field68 { get; set; }
        public float Field6C { get; set; }
        public uint Field70 { get; set; }
        public int HideFlag { get; set; }

        public Vector3 Translation { get; set; }
        public float ZTranslation { get; set; }
        public float Rotation { get; set; }
        public Vector3 Scale { get; set; }

        public float DefaultSprite { get; set; }
        public Color Color { get; set; }
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
        public ObservableCollection<ShurikenUIElement> Children { get; set; }
        public SharpNeedle.Ninja.Csd.Cast CastCsd;
        //public SharpNeedle.Ninja.Csd CastSwif;

        //These are for the UI to mess with
        #region UI Funcs
        public Vector2 UVTopLeft 
        { 
            get 
            { return TopLeft * UIEditor.ViewResolution; } 
            set 
            { TopLeft = value / UIEditor.ViewResolution; }}
        public Vector2 UVTopRight { get { return TopRight * UIEditor.ViewResolution; } set { TopRight = value / UIEditor.ViewResolution; }}
        public Vector2 UVBottomLeft { get { return BottomLeft * UIEditor.ViewResolution; } set { BottomLeft = value / UIEditor.ViewResolution; }}
        public Vector2 UVBottomRight { get { return BottomRight * UIEditor.ViewResolution; } set { BottomRight = value / UIEditor.ViewResolution; }}


        public bool InheritsXPosition 
        { 
            get { return InheritanceFlags.HasFlag(ElementInheritanceFlags.InheritXPosition); }
            set { InheritanceFlags = InheritanceFlags.SetFlag<ElementInheritanceFlags>(ElementInheritanceFlags.InheritXPosition, value); }
        }
        public bool InheritsYPosition
        {
            get { return InheritanceFlags.HasFlag(ElementInheritanceFlags.InheritYPosition); }
            set { InheritanceFlags = InheritanceFlags.SetFlag(ElementInheritanceFlags.InheritYPosition, value); }
        }
        public bool InheritsRotation
        {
            get { return InheritanceFlags.HasFlag(ElementInheritanceFlags.InheritRotation); }
            set { InheritanceFlags = InheritanceFlags = InheritanceFlags.SetFlag(ElementInheritanceFlags.InheritRotation, value); }
        }
        public bool InheritsColor
        {
            get { return InheritanceFlags.HasFlag(ElementInheritanceFlags.InheritColor); }
            set { InheritanceFlags = InheritanceFlags.SetFlag(ElementInheritanceFlags.InheritColor, value); }
        }
        public bool InheritsScaleX
        {
            get { return InheritanceFlags.HasFlag(ElementInheritanceFlags.InheritScaleX); }
            set { InheritanceFlags = InheritanceFlags.SetFlag(ElementInheritanceFlags.InheritScaleX, value); }
        }
        public bool InheritsScaleY
        {
            get { return InheritanceFlags.HasFlag(ElementInheritanceFlags.InheritScaleY); }
            set { InheritanceFlags = InheritanceFlags.SetFlag(ElementInheritanceFlags.InheritScaleY, value); }
        }
        public ElementMaterialBlend Additive
        {
            get
            {
                return (ElementMaterialBlend)(Flags.HasFlag(ElementMaterialFlags.AdditiveBlending) ? 1 : 0);
            }
            set
            {
                Flags = Flags.SetFlag(ElementMaterialFlags.AdditiveBlending, value == ElementMaterialBlend.Additive);
            }
        }
        public ElementMaterialFiltering Filtering
        {
            get 
            {
                return (ElementMaterialFiltering)(Flags.HasFlag(ElementMaterialFlags.LinearFiltering) ? 1 : 0); 
            }
            set 
            {
                Flags = Flags.SetFlag(ElementMaterialFlags.LinearFiltering, value == ElementMaterialFiltering.Linear);
            }
        }
        public bool MirrorX
        {
            get { return Flags.HasFlag(ElementMaterialFlags.MirrorX); }
            set { if (value) Flags |= ElementMaterialFlags.MirrorX; else Flags &= ~ElementMaterialFlags.MirrorX; }
        }
        public bool MirrorY
        {
            get { return Flags.HasFlag(ElementMaterialFlags.MirrorY); }
            set { if (value) Flags |= ElementMaterialFlags.MirrorY; else Flags &= ~ElementMaterialFlags.MirrorY; }
        }
        #endregion
        public List<ShurikenUIElement> GetAllChildren()
        {
            List<ShurikenUIElement> shurikenUIElements = new List<ShurikenUIElement>();
            shurikenUIElements.Add(this);
            foreach(var auto in Children)
                shurikenUIElements.AddRange(auto.GetAllChildren());
            return shurikenUIElements;
        }
        public void AddCast(ShurikenUIElement cast)
        {
            Children.Add(cast);
        }

        public void RemoveCast(ShurikenUIElement cast)
        {
            Children.Remove(cast);
        }
        public ShurikenUIElement(Cast cast, string name, int priority)
        {
            Name = name;
            Field00 = cast.Field00;
            Type = (DrawType)cast.Field04;
            IsEnabled = cast.IsEnabled != 0;
            Visible = true;
            ZIndex = priority;
            Children = new ObservableCollection<ShurikenUIElement>();

            TopLeft = new Vector2(cast.TopLeft);
            TopRight = new Vector2(cast.TopRight);
            BottomLeft = new Vector2(cast.BottomLeft);
            BottomRight = new Vector2(cast.BottomRight);

            Field2C = cast.Field2C;
            InheritanceFlags = (ElementInheritanceFlags)cast.Field34;
            Flags = (ElementMaterialFlags)cast.Field38;
            SubImageCount = cast.SubImageCount;

            FontID = -1;
            FontCharacters = cast.FontCharacters;

            FontSpacingAdjustment = cast.FontSpacingAdjustment;
            Size = new Vector2(cast.Width, cast.Height);

            Field58 = cast.Field58;
            Field5C = cast.Field5C;

            Offset = new Vector2(cast.Offset);

            Field68 = cast.Field68;
            Field6C = cast.Field6C;
            Field70 = cast.Field70;

            HideFlag = cast.CastInfoData.HideFlag;
            Translation = new Vector3(cast.CastInfoData.Translation);
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
        public ShurikenUIElement(SharpNeedle.Ninja.Csd.Cast cast, string name, int index)
        {
            CastCsd = cast;
            Name = name;
            Field00 = cast.Field00;
            Type = (DrawType)cast.Field04;
            IsEnabled = cast.Enabled != false;
            Visible = true;
            ZIndex = index;
            Children = new ObservableCollection<ShurikenUIElement>();

            TopLeft = new Vector2(cast.TopLeft);
            TopRight = new Vector2(cast.TopRight);
            BottomLeft = new Vector2(cast.BottomLeft);
            BottomRight = new Vector2(cast.BottomRight);

            Field2C = cast.Field2C;
            InheritanceFlags = (ElementInheritanceFlags)cast.InheritanceFlags.Value;
            Flags = (ElementMaterialFlags)cast.Field38;
            SubImageCount = (uint)cast.SpriteIndices.Length;

            FontID = -1;
            FontCharacters = cast.Text;

            FontSpacingAdjustment = BitConverter.ToSingle(BitConverter.GetBytes(cast.Field4C));
            Size = new Vector2(cast.Width, cast.Height);

            Field58 = cast.Field58;
            Field5C = cast.Field5C;

            Offset = new Vector2(cast.Origin);

            Field68 = cast.Position.X;
            Field6C = cast.Position.Y;
            Field70 = cast.Field70;

            HideFlag = (int)cast.Info.HideFlag;
            Translation = new Vector3(cast.Info.Translation);
            Rotation = cast.Info.Rotation;
            Scale = new Vector3(cast.Info.Scale.X, cast.Info.Scale.Y, 1.0f);
            DefaultSprite = cast.Info.SpriteIndex;
            Color = new Color(cast.Info.Color.A, cast.Info.Color.B, cast.Info.Color.G, cast.Info.Color.R);
            GradientTopLeft = new Color(cast.Info.GradientTopLeft.A, cast.Info.GradientTopLeft.B, cast.Info.GradientTopLeft.G, cast.Info.GradientTopLeft.R);
            GradientBottomLeft = new Color(cast.Info.GradientBottomLeft.A, cast.Info.GradientBottomLeft.B, cast.Info.GradientBottomLeft.G, cast.Info.GradientBottomLeft.R);
            GradientTopRight = new Color(cast.Info.GradientTopRight.A, cast.Info.GradientTopRight.B, cast.Info.GradientTopRight.G, cast.Info.GradientTopRight.R);
            GradientBottomRight = new Color(cast.Info.GradientBottomRight.A, cast.Info.GradientBottomRight.B, cast.Info.GradientBottomRight.G, cast.Info.GradientBottomRight.R);
            InfoField30 = cast.Info.Field30;
            InfoField34 = cast.Info.Field34;
            InfoField38 = cast.Info.Field38;

            Sprites = new ObservableCollection<int>(Enumerable.Repeat(-1, cast.SpriteIndices.Length).ToList());
        }
        public ShurikenUIElement()
        {
            Name = "New_Cast";
            Field00 = 0;
            Type = DrawType.Sprite;
            IsEnabled = true;
            Visible = true;
            ZIndex = 0;
            Children = new ObservableCollection<ShurikenUIElement>();

            Field2C = 0;
            InheritanceFlags = 0;
            Flags = 0;
            SubImageCount = 0;

            FontID = -1;
            FontCharacters = "";

            FontSpacingAdjustment = 0;
            Size = new Vector2(64, 64);
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
            Translation = new Vector3();
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

        public ShurikenUIElement(ShurikenUIElement c)
        {
            Name = name;
            Field00 = c.Field00;
            Type = c.Type;
            IsEnabled = c.IsEnabled;
            Visible = true;
            ZIndex = ZIndex;
            Children = new ObservableCollection<ShurikenUIElement>(c.Children);

            TopLeft = new Vector2(c.TopLeft);
            TopRight = new Vector2(c.TopRight);
            BottomLeft = new Vector2(c.BottomLeft);
            BottomRight = new Vector2(c.BottomRight);

            Field2C = c.Field2C;
            InheritanceFlags = c.InheritanceFlags;
            Flags = c.Flags;
            SubImageCount = c.SubImageCount;

            FontID = -1;
            FontCharacters = c.FontCharacters;

            FontSpacingAdjustment = c.FontSpacingAdjustment;

            Size = c.Size;
            Field58 = c.Field58;
            Field5C = c.Field5C;

            Offset = new Vector2(c.Offset);

            Field68 = c.Field68;
            Field6C = c.Field6C;
            Field70 = c.Field70;

            HideFlag = c.HideFlag;
            Translation = new Vector3(c.Translation);
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
