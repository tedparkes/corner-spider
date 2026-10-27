using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CornerSpider {
    public static partial class RuntimeTests {
        static World SilkWorld(string species,string corner,double web) {
            var w=new World(900,650,42);w.AutoFeed=w.CursorPlay=false;w.SetSpecies(species);w.Corner=corner;w.SetScale(1.2);w.Spider=w.Home;w.Angle=w.NestAngle;w.GoHome();
            for(int i=0;i<140;i++) w.Tick(.025,new V(-10000,-10000));w.Web=web;return w;
        }
        public static void RenderSilkGallery(string folder) {
            string[] ids={"Regal","Bold","Zebra","Tan","RedBackFemale","Emerald","GrayWall","Pantropical","Apache","PortiaLabiata","HyllusDiardiGreen"};
            foreach(bool dark in new[]{false,true}) {
                var v=new DrawingVisual();using(var d=v.RenderOpen()) {
                    d.DrawRectangle(BPreview("#e6ebe1"),null,new Rect(0,0,1400,1060));Text(d,"CORNER SPIDER / ANCHORED SILK RETREATS",20,14,24,Color.FromRgb(30,50,44));
                    Text(d,"The upper and left edges of each panel are the desktop corner. Species-inspired variations, shared end entrances.",20,48,15,Color.FromRgb(40,60,54));
                    for(int i=0;i<12;i++) {
                        d.PushTransform(new TranslateTransform(i%4*350,85+i/4*325));
                        d.PushClip(new RectangleGeometry(new Rect(0,0,346,277)));d.DrawRectangle(BPreview(dark?"#24313a":"#e7eadd"),null,new Rect(0,0,346,277));
                        if(i<11) {
                            var w=SilkWorld(ids[i],"TopLeft",1);new PetCanvas().DrawNestScene(d,w,ids[i]);
                        } else {
                            var w=new World(1200,850,42);w.AutoFeed=w.CursorPlay=false;w.SetSpecies("PortiaLabiata");w.StartWebVisit();
                            for(int f=0;f<2600 && w.WebStage==WebVisit.Building;f++) w.Tick(.1,new V(-10000,-10000));
                            double fit=245/(w.GuestWebExtent+35*w.GuestScale);d.PushTransform(new ScaleTransform(fit,fit));
                            d.PushTransform(new TranslateTransform(-w.GuestCorner.X,-w.GuestCorner.Y));
                            d.PushTransform(new ScaleTransform(w.GuestInward.X>0?1:-1,w.GuestInward.Y>0?1:-1,w.GuestCorner.X,w.GuestCorner.Y));
                            PetCanvas.DrawGuestWeb(d,w);PetCanvas.DrawPrey(d,w);d.Pop();d.Pop();d.Pop();
                        }
                        d.Pop();Text(d,i<11?SkinCatalog.Get(ids[i]).Name:"Web resident (generic prey)",12,283,15,Color.FromRgb(30,50,44));
                        Text(d,i<11?SilkProfile.Get(ids[i]).Name:"Silk fixed to both desktop edges",12,304,13,Color.FromRgb(50,75,65));d.Pop();
                    }
                }SavePng(v,1400,1060,Path.Combine(folder,"species-retreats-"+(dark?"dark":"light")+".png"));
            }
        }
        static RenderTargetBitmap SilkBitmap(PetCanvas canvas,World w,string species) {
            var v=new DrawingVisual();using(var d=v.RenderOpen()) { canvas.DrawHammock(d,w,false,species);canvas.DrawHammock(d,w,true,species); }
            var bitmap=new RenderTargetBitmap(360,360,96,96,PixelFormats.Pbgra32);bitmap.Render(v);return bitmap;
        }
        static void CheckSilkArt(string folder) {
            var canvas=new PetCanvas();var w=new World(360,360,42);w.AutoFeed=w.CursorPlay=false;w.Web=1;
            var original=SilkBitmap(canvas,w,"Regal");int initialBuilds=canvas.NestBuilds;
            byte[] full=new byte[360*360*4];original.CopyPixels(full,1440,0);long fullAlpha=0;for(int i=3;i<full.Length;i+=4) fullAlpha+=full[i];
            foreach(double visibility in new[]{.5,0,1}) {
                w.WebVisibility=visibility;byte[] pixels=new byte[full.Length];SilkBitmap(canvas,w,"Regal").CopyPixels(pixels,1440,0);
                long alpha=0;bool identical=true;for(int i=3;i<pixels.Length;i+=4) { alpha+=pixels[i];if(pixels[i]!=full[i]) identical=false; }
                Check(visibility==0?alpha==0:visibility==1?identical:alpha>0 && alpha<fullAlpha,"hammock opacity fades and restores its original alpha: "+visibility);
                Check(canvas.NestBuilds==initialBuilds && w.Web==1,"opacity changes retain cached art and nest progress");
            }
            foreach(var skin in SkinCatalog.All) foreach(string corner in Settings.Corners) {
                w.Corner=corner;w.UpdateHome();RenderTargetBitmap bitmap=SilkBitmap(canvas,w,skin.Id);
                byte[] pixels=new byte[360*360*4];bitmap.CopyPixels(pixels,1440,0);
                bool right=corner.EndsWith("Right"),top=corner.StartsWith("Top");int vertical=0,horizontal=0;
                for(int j=2;j<358;j++) {
                    for(int k=0;k<2;k++) {
                        if(pixels[((right?359-k:k)+360*j)*4+3]>12) vertical++;
                        if(pixels[(j+360*(top?k:359-k))*4+3]>12) horizontal++;
                    }
                }
                Check(vertical>25 && horizontal>25,"retreat has multiple visible attachments to both desktop edges: "+skin.Id+" / "+corner);
                int far=(top?350:10)*360+(right?10:350);
                Check(pixels[far*4+3]==0,"retreat preserves clear desktop outside its corner: "+skin.Id+" / "+corner);
                int builds=canvas.NestBuilds;SilkBitmap(canvas,w,skin.Id);
                Check(canvas.NestBuilds==builds,"static silk reuses its retained artwork: "+skin.Id+" / "+corner);
            }
            int before=canvas.NestBuilds;SilkBitmap(canvas,w,"Regal");SilkBitmap(canvas,w,"PortiaLabiata");
            Check(canvas.NestBuilds==before+2,"switching species replaces the retained silk style immediately");
            w.Web=.2;SilkBitmap(canvas,w,"PortiaLabiata");Check(canvas.NestBuilds==before+3,"saved silk progress still controls growth layers");
            foreach(var skin in SkinCatalog.All) {
                w.Web=.2;var young=SilkBitmap(canvas,w,skin.Id);w.Web=.9;var grown=SilkBitmap(canvas,w,skin.Id);
                byte[] a=new byte[360*360*4],b=new byte[360*360*4];young.CopyPixels(a,1440,0);grown.CopyPixels(b,1440,0);bool stable=true;
                for(int i=3;i<a.Length;i+=4) if(b[i]+1<a[i]) { stable=false;break; }
                Check(stable,"growing silk preserves existing threads rather than rearranging them: "+skin.Id);
            }
            RenderSilkGallery(folder);
        }
        static void CheckRetreatVariety(string folder) {
            string[] ids={"Regal","Bold","Zebra","Tan","RedBackFemale","Emerald","GrayWall","Pantropical","Apache","PortiaLabiata","HyllusDiardiGreen"};
            var masks=new System.Collections.Generic.List<byte[]>();
            for(int s=0;s<ids.Length;s++) {
                var profile=SilkProfile.Get(ids[s]);var visual=new DrawingVisual();
                using(var d=visual.RenderOpen()) {
                    d.PushTransform(new TranslateTransform(80,80));d.PushTransform(new ScaleTransform(profile.Width,profile.Length));
                    d.DrawGeometry(Brushes.White,null,PetCanvas.Pocket(profile));d.Pop();d.Pop();
                }
                var bitmap=new RenderTargetBitmap(160,160,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);
                byte[] mask=new byte[160*160*4];bitmap.CopyPixels(mask,640,0);
                for(int previous=0;previous<masks.Count;previous++) {
                    int union=0,difference=0;for(int i=3;i<mask.Length;i+=4) {
                        bool a=mask[i]>127,b=masks[previous][i]>127;if(a||b) union++;if(a!=b) difference++;
                    }
                    Check(difference>union*.075,"retreat silhouettes visibly differ before texture: "+ids[s]+" / "+ids[previous]);
                }
                masks.Add(mask);
            }
            // The same renderer is used by the settings thumbnail, without image
            // caches. Exercise every skin in the preview and at the supported size extremes.
            var gallery=new DrawingVisual();using(var d=gallery.RenderOpen()) {
                d.DrawRectangle(BPreview("#e6ebe1"),null,new Rect(0,0,1200,1160));
                Text(d,"RETREAT VARIETY / EMPTY SHELTER AND SETTINGS PREVIEW",18,12,22,Color.FromRgb(30,50,44));
                for(int i=0;i<SkinCatalog.All.Length;i++) {
                    var skin=SkinCatalog.All[i];double x=i%5*240,y=55+i/5*275;
                    d.PushTransform(new TranslateTransform(x,y));
                    var preview=new SettingsPreview { Species=skin.Id,Dark=i%2==0,Enlarged=true };
                    preview.Measure(new Size(236,250));preview.Arrange(new Rect(0,0,236,250));preview.UpdateLayout();
                    var bitmap=new RenderTargetBitmap(236,250,96,96,PixelFormats.Pbgra32);bitmap.Render(preview);
                    d.DrawImage(bitmap,new Rect(0,0,236,250));preview.Dispose();
                    Text(d,skin.Name,8,254,12,Color.FromRgb(30,50,44));d.Pop();
                }
            }SavePng(gallery,1200,1160,Path.Combine(folder,"retreat-settings.png"));
            var canvas=new PetCanvas();
            foreach(var skin in SkinCatalog.All) foreach(double size in new[]{.2,1.0,1.8}) {
                // A large offscreen work area prevents the small-screen scale fit
                // from disguising the true 20% and 180% rendering bounds.
                var w=new World(1200,900,42);w.AutoFeed=w.CursorPlay=false;w.SetScale(size);w.Web=1;
                var v=new DrawingVisual();using(var d=v.RenderOpen()) {
                    canvas.DrawHammock(d,w,false,skin.Id,true);canvas.DrawHammock(d,w,true,skin.Id,true);
                }
                var bitmap=new RenderTargetBitmap(1200,900,96,96,PixelFormats.Pbgra32);bitmap.Render(v);
                byte[] pixels=new byte[1200*900*4];bitmap.CopyPixels(pixels,4800,0);int painted=0;
                for(int i=3;i<pixels.Length;i+=4) if(pixels[i]>12) painted++;
                Check(painted>25,"visible vector silk at "+size+": "+skin.Id);
            }
            foreach(string id in new[]{"Regal","PortiaLabiata"}) {
                var w=new World(360,360,42);w.Web=1;var v=new DrawingVisual();
                using(var d=v.RenderOpen()) canvas.DrawHammock(d,w,true,id,true);
                var bitmap=new RenderTargetBitmap(360,360,96,96,PixelFormats.Pbgra32);bitmap.Render(v);
                byte[] pixels=new byte[360*360*4];bitmap.CopyPixels(pixels,1440,0);long center=0;
                for(int y=(int)w.Home.Y-4;y<=(int)w.Home.Y+4;y++) for(int x=(int)w.Home.X-4;x<=(int)w.Home.X+4;x++) center+=pixels[(y*360+x)*4+3];
                Check(id=="PortiaLabiata"?center==0:center>200,"open Portia platform versus covered oval: "+id);
            }
        }
    }
}
