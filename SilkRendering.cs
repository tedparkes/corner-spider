using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CornerSpider {
    public partial class PetCanvas {
        internal struct SilkImage {
            public BitmapSource Image;
            public Rect Bounds;
            public DrawingGroup Drawing;
        }
        internal const int MaxSilkPixels=2*1024*1024,MaxSilkSide=2048;
        internal static int SilkRasterBuilds;
        internal static SilkImage CacheSilk(DrawingGroup drawing,double physicalScale) {
            var filtered=new DrawingGroup();
            // Apply resampling to silk alone. Moving bodies and the finger keep
            // their existing rendering cost. The frozen group retains this mode
            // in desktop overlays, settings thumbnails and diagnostic renders.
            RenderOptions.SetBitmapScalingMode(filtered,BitmapScalingMode.HighQuality);
            var result=new SilkImage { Drawing=filtered,Bounds=Rect.Empty };
            Rect bounds=drawing.Bounds;
            if(!bounds.IsEmpty && bounds.Width>0 && bounds.Height>0) {
                double resolution=Math.Max(4,physicalScale*4);
                // Four samples per physical pixel in each axis. Cap both area
                // and dimensions so large webs/colonies cannot allocate huge textures.
                resolution=Math.Min(resolution,Math.Min((MaxSilkSide-4)/Math.Max(bounds.Width,bounds.Height),
                    Math.Sqrt((MaxSilkPixels-8*MaxSilkSide-16)/(bounds.Width*bounds.Height))));
                double left=Math.Floor(bounds.Left*resolution)-1,top=Math.Floor(bounds.Top*resolution)-1;
                int width=(int)Math.Ceiling(bounds.Right*resolution)- (int)left+1;
                int height=(int)Math.Ceiling(bounds.Bottom*resolution)-(int)top+1;
                // Snap the source origin to its sample grid: adding aged silk
                // must not shift the sampling phase of threads already present.
                var visual=new DrawingVisual();RenderOptions.SetEdgeMode(visual,EdgeMode.Unspecified);
                using(var c=visual.RenderOpen()) {
                    c.PushTransform(new MatrixTransform(resolution,0,0,resolution,-left,-top));
                    c.DrawDrawing(drawing);c.Pop();
                }
                var bitmap=new RenderTargetBitmap(width,height,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);bitmap.Freeze();
                result.Image=bitmap;result.Bounds=new Rect(left/resolution,top/resolution,width/resolution,height/resolution);
                using(var c=filtered.Open()) c.DrawImage(bitmap,result.Bounds);
                SilkRasterBuilds++;
            }
            filtered.Freeze();return result;
        }
    }
}
