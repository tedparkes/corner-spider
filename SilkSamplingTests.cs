using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CornerSpider {
    public static partial class RuntimeTests {
        static RenderTargetBitmap SampleSilk(Drawing drawing,double scale,int samples) {
            var visual=new DrawingVisual();using(var d=visual.RenderOpen()) {
                d.PushTransform(new ScaleTransform(samples,samples));
                d.PushTransform(new TranslateTransform(8.125,8.375));d.PushTransform(new ScaleTransform(scale,scale));
                d.DrawDrawing(drawing);d.Pop();d.Pop();d.Pop();
            }
            var bitmap=new RenderTargetBitmap(360*samples,360*samples,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);return bitmap;
        }
        static byte[] SampleAlpha(BitmapSource bitmap,int samples) {
            int width=bitmap.PixelWidth,height=bitmap.PixelHeight;byte[] pixels=new byte[width*height*4];bitmap.CopyPixels(pixels,width*4,0);
            byte[] alpha=new byte[width*height/(samples*samples)];int targetWidth=width/samples;
            for(int y=0;y<height;y+=samples) for(int x=0;x<width;x+=samples) {
                int sum=0;for(int j=0;j<samples;j++) for(int k=0;k<samples;k++) sum+=pixels[((y+j)*width+x+k)*4+3];
                alpha[(y/samples)*targetWidth+x/samples]=(byte)((sum+samples*samples/2)/(samples*samples));
            }return alpha;
        }
        static double AlphaError(byte[] image,byte[] reference) {
            double sum=0;for(int i=0;i<image.Length;i++) { double delta=image[i]-reference[i];sum+=delta*delta; }return sum/image.Length;
        }
        static void CheckSilkSampling(string folder) {
            var strands=new DrawingGroup();using(var d=strands.Open()) {
                var pen=new Pen(Brushes.White,.32) { StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round };
                for(int i=0;i<24;i++) { double a=i*Math.PI/12;d.DrawLine(pen,new Point(90,90),new Point(90+80*Math.Cos(a),90+80*Math.Sin(a))); }
                for(int i=1;i<=9;i++) d.DrawEllipse(null,pen,new Point(90,90),i*8.3,i*7.5);
            }strands.Freeze();
            double oldError=0,newError=0;
            foreach(double scale in new[]{.2,.55,1,1.8}) {
                var cache=PetCanvas.CacheSilk(strands,scale);var raw=new DrawingGroup();using(var d=raw.Open()) d.DrawImage(cache.Image,cache.Bounds);raw.Freeze();
                byte[] reference=SampleAlpha(SampleSilk(strands,scale,8),8);
                byte[] before=SampleAlpha(SampleSilk(raw,scale,1),1),after=SampleAlpha(SampleSilk(cache.Drawing,scale,1),1);
                double prior=AlphaError(before,reference),current=AlphaError(after,reference);oldError+=prior;newError+=current;
                log.AppendLine("SAMPLING scale="+scale+": default MSE="+prior.ToString("F4")+", filtered MSE="+current.ToString("F4"));
                Check(current<prior,"filtered strands are closer to an independent 8x area-sampled reference at scale "+scale);
                Check(cache.Image.IsFrozen && cache.Drawing.IsFrozen && cache.Image.Format==PixelFormats.Pbgra32,"silk keeps frozen premultiplied-alpha images");
            }
            Check(newError<oldError*.75,"silk filtering reduces aggregate subpixel coverage error by at least 25 percent");
            var huge=new DrawingGroup();using(var d=huge.Open()) d.DrawLine(new Pen(Brushes.White,.4),new Point(-1000,-1000),new Point(9000,7000));huge.Freeze();
            var bounded=PetCanvas.CacheSilk(huge,10.8);
            log.AppendLine("BOUNDED: "+bounded.Image.PixelWidth+" x "+bounded.Image.PixelHeight+" pixels");
            Check(bounded.Image.PixelWidth<=PetCanvas.MaxSilkSide && bounded.Image.PixelHeight<=PetCanvas.MaxSilkSide && bounded.Image.PixelWidth*bounded.Image.PixelHeight<=PetCanvas.MaxSilkPixels,"large silk has a bounded raster allocation");
            var empty=new DrawingGroup();empty.Freeze();var blank=PetCanvas.CacheSilk(empty,1);
            Check(blank.Image==null && blank.Drawing.IsFrozen,"an empty web uses no bitmap");
            foreach(var skin in SkinCatalog.All) {
                // Leave room for the larger orb to resize at both pet sizes.
                var w=new World(1900,1800,42);w.SetSpecies(skin.Id);w.Web=1;var canvas=new PetCanvas();SilkBitmap(canvas,w,skin.Id);
                int built=PetCanvas.SilkRasterBuilds;
                for(int i=0;i<5;i++) SilkBitmap(canvas,w,skin.Id);
                Check(PetCanvas.SilkRasterBuilds==built,"unchanged frames reuse silk samples: "+skin.Id);
                w.SetScale(1.8);SilkBitmap(canvas,w,skin.Id);int resized=PetCanvas.SilkRasterBuilds;SilkBitmap(canvas,w,skin.Id);
                Check(resized>built && PetCanvas.SilkRasterBuilds==resized,"size changes replace current silk samples once: "+skin.Id);
            }
        }
    }
}
