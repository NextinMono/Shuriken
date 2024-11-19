using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Shuriken.Converters;
using Shuriken.Rendering;
using DirectXTexNet;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Media.Media3D;
using System.Net;
using SharpDX;
using SharpDX.Direct3D11;
using SixLabors.ImageSharp.Formats.Tga;
using System.Net.NetworkInformation;

namespace Shuriken.Models
{
    public class Texture
    {
        public string Name { get; set; }
        public string FullName { get; }
        public string FilePath { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public int RelativeWidth { get;  set; }
        public int RelativeHeight { get;  set; }

        public BitmapSource ImageSource { get; private set; }
        internal GLTexture GlTex { get; private set; }
        public ObservableCollection<int> Sprites { get; set; }

        private void CreateTexture(ScratchImage img)
        {

            if (TexHelper.Instance.IsCompressed(img.GetMetadata().Format))
                img = img.Decompress(DXGI_FORMAT.B8G8R8A8_UNORM);

            else if (img.GetMetadata().Format != DXGI_FORMAT.B8G8R8A8_UNORM)
                img = img.Convert(DXGI_FORMAT.B8G8R8A8_UNORM, TEX_FILTER_FLAGS.DEFAULT, 0.5f);

            Width = img.GetImage(0).Width;
            Height = img.GetImage(0).Height;


            GlTex = new GLTexture(img.FlipRotate(TEX_FR_FLAGS.FLIP_VERTICAL).GetImage(0).Pixels, Width, Height);

            CreateBitmap(img);

            img.Dispose();
        }
        private unsafe void CreateTextureNN()
        {
            ///Someone should definitely remake this, and also someone should probably make a custom GVR decoder -Nextin
            
            //Decode GVR to byte array
            PuyoTools.Core.Textures.Gvr.GvrTextureDecoder decoder = new PuyoTools.Core.Textures.Gvr.GvrTextureDecoder(FilePath);
            Width = decoder.Width;
            Height = decoder.Height;
            byte[] bytes = decoder.GetPixelData();

            //Create bitmap from byte array
            Bitmap bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bytes, 0, bmpData.Scan0, bytes.Length);
            bitmap.UnlockBits(bmpData);

            byte[] pngBytes;
            using (var memoryStream = new MemoryStream())
            {
                // Save the bitmap to the memory stream in PNG format
                bitmap.Save(memoryStream, ImageFormat.Png);
                pngBytes = memoryStream.ToArray();
                memoryStream.Dispose();
            }
            //Create texture just like the DDS version, but with WIC
            fixed (byte* pBytes = pngBytes)
                CreateTexture(TexHelper.Instance.LoadFromWICMemory((IntPtr)pBytes, pngBytes.Length, WIC_FLAGS.NONE));
        }

        private unsafe void CreateTexture(byte[] bytes)
        {
            fixed (byte* pBytes = bytes)
                CreateTexture(TexHelper.Instance.LoadFromDDSMemory((IntPtr)pBytes, bytes.Length, DDS_FLAGS.NONE));
        }

        private void CreateTexture(string filename)
        {
            FilePath = filename;
            if (Path.GetExtension(FilePath) == ".gvr")
                CreateTextureNN();
            else
                CreateTexture(TexHelper.Instance.LoadFromDDSFile(filename, DDS_FLAGS.NONE));
        }

        private void CreateBitmap(ScratchImage img)
        {
            var bmp = BitmapConverter.FromTextureImage(img, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            ImageSource = BitmapConverter.FromBitmap(bmp);

            img.Dispose();
            bmp.Dispose();
        }

        public Texture(string filename, int relativeWidth = 0, int relativeHeight = 0) : this()
        {
            FullName = filename;
            Name = Path.GetFileNameWithoutExtension(filename);
            CreateTexture(filename);
        }

        public Texture(string name, byte[] bytes) : this()
        {
            FullName = name;
            Name = name;
            CreateTexture(bytes);
        }

        public Texture()
        {
            Name = FullName = "";
            Width = Height = 0;
            ImageSource = null;
            GlTex = null;

            Sprites = new ObservableCollection<int>();
        }
    }
}
