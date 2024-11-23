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
using GvrTool.Gvr;
using System.Collections;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Drawing.Imaging;
using System.Windows.Media;

namespace Shuriken.Models
{
    public class Texture
    {
        public string Name { get; }
        public string FullName { get; }
        public int Width { get; private set; }
        public int Height { get; private set; }

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
        /// <summary>
        /// Used for GVR textures for GNCPs, converts GVR's to BitmapSource and output a pixel array for the GL WPF Control
        /// </summary>
        /// <param name="in_Gvr">GVR Texture</param>
        /// <param name="out_Pixels">Pixel array output for GL</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the GVR pixel array is null</exception>
        public static BitmapSource LoadTga(GVR in_Gvr, ref byte[] out_Pixels)
        {
            if (in_Gvr.Pixels == null) throw new ArgumentNullException("GVR Image might be invalid, pixel array is null.");
            var pixelFormat = PixelFormats.Bgr32; //temporary!!!!!!

            int bytesPerPixel = pixelFormat.BitsPerPixel / 8;
            int stride = in_Gvr.Width * bytesPerPixel;

            var bitmap = new WriteableBitmap(
                in_Gvr.Width, in_Gvr.Height,
                96, 96,
                pixelFormat,
                null
            );

            bitmap.WritePixels(
                new Int32Rect(0, 0, in_Gvr.Width, in_Gvr.Height),
                in_Gvr.Pixels,
                stride,
                0
            );
            //Flip vertically
            var tb = new TransformedBitmap();
            var bi = bitmap.Clone();
            tb.BeginInit();
            tb.Source = bi;
            var transform = new ScaleTransform(1, -1, 0, 0);
            tb.Transform = transform;
            tb.EndInit();

            out_Pixels = new byte[stride * tb.PixelHeight];

            tb.CopyPixels(out_Pixels, stride, 0);

            return bitmap;
        }
        private unsafe void CreateTextureGvr(GVR gvr)
        {
            Width = gvr.Width;
            Height = gvr.Height;

            byte[] forGlTex = null;
            var bmp = (BitmapSource)LoadTga(gvr, ref forGlTex);
           

            fixed (byte* pBytes = forGlTex)
                GlTex = new GLTexture((IntPtr)pBytes, Width, Height);
            ImageSource = bmp;
        }

        private unsafe void CreateTexture(byte[] bytes)
        {
            fixed (byte* pBytes = bytes)
                CreateTexture(TexHelper.Instance.LoadFromDDSMemory((IntPtr)pBytes, bytes.Length, DDS_FLAGS.NONE));
        }

        private void CreateTexture(string filename)
        {
           CreateTexture(TexHelper.Instance.LoadFromDDSFile(filename, DDS_FLAGS.NONE));
        }

        private void CreateBitmap(ScratchImage img)
        {
            var bmp = BitmapConverter.FromTextureImage(img, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            ImageSource = BitmapConverter.FromBitmap(bmp);

            img.Dispose();
            bmp.Dispose();
        }

        public Texture(string filename, bool gvrTex = false) : this()
        {
            FullName = filename;
            Name = Path.GetFileNameWithoutExtension(filename);
            if (gvrTex)
            {
                GVR gVR = new GVR();
                gVR.LoadFromGvrFile(filename);
                CreateTextureGvr(gVR);
            }
            else
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
