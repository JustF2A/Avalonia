using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls.Platform.Surfaces;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering;
using SkiaSharp;

namespace Avalonia.Skia
{
    public partial class PlatformRenderInterface : IPlatformRenderInterface, IRendererFactory
    {
        public IBitmapImpl CreateBitmap(int width, int height)
        {
            return CreateRenderTargetBitmap(width, height);
        }

        public IFormattedTextImpl CreateFormattedText(string text, string fontFamilyName, double fontSize, FontStyle fontStyle,
            TextAlignment textAlignment, FontWeight fontWeight, TextWrapping wrapping)
        {
            return new FormattedTextImpl(text, fontFamilyName, fontSize, fontStyle, textAlignment, fontWeight, wrapping);
        }

        public IStreamGeometryImpl CreateStreamGeometry()
        {
            return new StreamGeometryImpl();
        }

        public IBitmapImpl LoadBitmap(System.IO.Stream stream)
        {
            using (var s = new SKManagedStream(stream))
            {
                var bitmap = SKBitmap.Decode(s);
                if (bitmap != null)
                {
                    return new BitmapImpl(bitmap);
                }
                else
                {
                    throw new ArgumentException("Unable to load bitmap from provided data");
                }
            }
        }

        public IBitmapImpl LoadBitmap(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                return LoadBitmap(stream);
            }
        }

        public IBitmapImpl LoadBitmap(PixelFormat format, IntPtr data, int width, int height, int stride)
        {
            using (var tmp = new SKBitmap())
            {
                tmp.InstallPixels(new SKImageInfo(width, height, format.ToSkColorType(), SKAlphaType.Premul)
                    , data, stride);
                return new BitmapImpl(tmp.Copy());
            }
        }

        public IRenderer CreateRenderer(IRenderRoot root, IRenderLoop renderLoop)
        {
            return new Renderer(root, renderLoop);
        }

        public IRenderTargetBitmapImpl CreateRenderTargetBitmap(int width, int height)
        {
            if (width < 1)
                throw new ArgumentException("Width can't be less than 1", nameof(width));
            if (height < 1)
                throw new ArgumentException("Height can't be less than 1", nameof(height));

            return new BitmapImpl(width, height);
        }

        public virtual IRenderTarget CreateRenderTarget(IEnumerable<object> surfaces)
        {
            var fb = surfaces?.OfType<IFramebufferPlatformSurface>().FirstOrDefault();
            if (fb == null)
                throw new Exception("Skia backend currently only supports framebuffer render target");
            return new FramebufferRenderTarget(fb);
        }
    }
}
