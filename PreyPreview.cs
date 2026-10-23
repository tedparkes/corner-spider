using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CornerSpider {
    public static partial class RuntimeTests {
        static World SpiderHunt() {
            var w=new World(3200,1800,5);w.AutoFeed=w.CursorPlay=false;w.Spider=new V(1600,900);w.SetSpecies("PortiaLabiata");w.SpawnSmallSpider();return w;
        }
        public static void RenderPreyPreview(string folder) {
            var v=new DrawingVisual();using(var d=v.RenderOpen()) {
                d.DrawRectangle(BPreview("#e6ebe1"),null,new Rect(0,0,1200,350));
                Text(d,"PORTIA / SMALL SPIDER PREY",20,12,24,Color.FromRgb(30,50,44));
                var w=SpiderHunt();w.State=Mood.Sleep;for(int i=0;i<60;i++) w.Tick(.025,new V(-10000,-10000));
                w.Scale=1.7;w.Spider=new V(130,190);w.Fly=new V(280,190);w.State=Mood.Rest;w.Angle=180;
                PetCanvas.DrawSpider(d,w,"PortiaLabiata");PetCanvas.DrawPrey(d,w);
                Text(d,"Relative size at 170%",30,310,16,Color.FromRgb(30,50,44));
                d.DrawRectangle(BPreview("#24313a"),null,new Rect(380,70,360,260));
                w.Scale=6;w.Fly=new V(560,195);PetCanvas.DrawPrey(d,w);Text(d,"Eight-legged crawler (enlarged)",400,305,16,Colors.White);
                w.HasFly=false;w.SpawnFly();w.Fly=new V(940,195);PetCanvas.DrawPrey(d,w);Text(d,"Flies remain in the mix",800,305,16,Color.FromRgb(30,50,44));
            }SavePng(v,1200,350,Path.Combine(folder,"portia-prey.png"));
            v=new DrawingVisual();Mood[] stages={Mood.Notice,Mood.Stalk,Mood.Aim,Mood.Crouch,Mood.Jump,Mood.Eat};
            using(var d=v.RenderOpen()) {
                d.DrawRectangle(BPreview("#e6ebe1"),null,new Rect(0,0,1260,790));
                Text(d,"PORTIA / SPIDER HUNT",20,12,24,Color.FromRgb(30,50,44));
                for(int i=0;i<stages.Length;i++) {
                    var w=SpiderHunt();for(int f=0;f<2400 && (w.State!=stages[i] || w.StateTime<(stages[i]==Mood.Jump?.10:.15));f++) w.Tick(.016,new V(-10000,-10000));
                    d.PushTransform(new TranslateTransform(i%3*420,60+i/3*360));
                    d.PushClip(new RectangleGeometry(new Rect(0,0,420,310)));d.DrawRectangle(BPreview("#24313a"),null,new Rect(2,0,416,310));
                    V center=w.HasFly?(w.Spider+w.Fly)*.5:w.Spider;
                    double zoom=w.HasFly?Math.Min(2,Math.Min(380/(Math.Abs(w.Spider.X-w.Fly.X)+150),280/(Math.Abs(w.Spider.Y-w.Fly.Y)+150))):2.7;
                    d.PushTransform(new TranslateTransform(210,155));d.PushTransform(new ScaleTransform(zoom,zoom));d.PushTransform(new TranslateTransform(-center.X,-center.Y));
                    PetCanvas.DrawPrey(d,w);PetCanvas.DrawSpider(d,w,"PortiaLabiata");d.Pop();d.Pop();d.Pop();d.Pop();
                    Text(d,stages[i].ToString(),16,319,18,Color.FromRgb(30,50,44));d.Pop();
                }
            }SavePng(v,1260,790,Path.Combine(folder,"portia-hunt.png"));
            RenderPouncePreview(folder);
        }
        static void RenderPouncePreview(string folder) {
            var visual=new DrawingVisual();Mood[] stages={Mood.Crouch,Mood.Jump,Mood.Land};
            using(var d=visual.RenderOpen()) {
                d.DrawRectangle(BPreview("#e6ebe1"),null,new Rect(0,0,1260,650));
                Text(d,"LONGER HUNTING POUNCES / ALL SPECIES",20,12,24,Color.FromRgb(30,50,44));
                Text(d,"Same camera position throughout each row. Actual pet renderer and behavior.",20,48,16,Color.FromRgb(30,50,44));
                for(int row=0;row<2;row++) for(int col=0;col<3;col++) {
                    string skin=row==0?"Bold":"PortiaLabiata";var w=new World(3200,1800,42);
                    w.AutoFeed=w.CursorPlay=false;w.Spider=new V(1600,900);w.SetSpecies(skin);
                    if(row==0) w.SpawnFly();else w.SpawnSmallSpider();w.Fly=new V(2050,900);w.Angle=90;
                    for(int f=0;f<2400 && (w.State!=stages[col] || w.StateTime<(col==1?.15:.10));f++) w.Tick(.016,new V(-10000,-10000));
                    V start=col==0?w.Spider:w.JumpStart;
                    var canvas=new PetCanvas { World=w,Species=skin,Width=420,Height=220,Offset=new V(start.X-90,900-130) };
                    canvas.Measure(new Size(420,220));canvas.Arrange(new Rect(0,0,420,220));
                    var frame=new RenderTargetBitmap(420,220,96,96,PixelFormats.Pbgra32);frame.Render(canvas);
                    d.PushTransform(new TranslateTransform(col*420,85+row*280));
                    d.DrawRectangle(BPreview("#24313a"),null,new Rect(2,0,416,220));d.DrawImage(frame,new Rect(0,0,420,220));
                    Text(d,(row==0?"Bold / fly":"Portia / small spider")+" - "+stages[col],12,228,16,Color.FromRgb(30,50,44));
                    if(col==1) Text(d,((w.JumpEnd-w.JumpStart).Length/w.BodyScale).ToString("F0")+" px leap in "+w.JumpDuration.ToString("F2")+" seconds",12,250,14,Color.FromRgb(30,50,44));
                    d.Pop();
                }
            }SavePng(visual,1260,650,Path.Combine(folder,"long-pounces.png"));
        }
        static void CheckPortiaPrey(PetHost host,string folder) {
            string old=host.Settings.Species;var w=host.World;w.GoHome();host.SetSpecies("PortiaLabiata");
            Check(w.HuntsSpiders && host.Commands["feed"].Text=="Release a small spider","Portia selection changes actual world diet and tray feeding label");
            host.Commands["feed"].PerformClick();Check(w.HasFly && w.Prey==PreyKind.SmallSpider,"tray feeding releases one small spider for Portia");
            V first=w.Fly;host.Commands["feed"].PerformClick();Check((w.Fly-first).Length==0 && w.Prey==PreyKind.SmallSpider,"tray feeding preserves the single prey slot");
            host.SetSpecies("Bold");Check(w.Prey==PreyKind.SmallSpider && host.Commands["feed"].Text=="Release a fly","switching skins updates future food without morphing existing prey");
            w.GoHome();host.Commands["feed"].PerformClick();Check(w.Prey==PreyKind.Fly,"other skins retain fly feeding");w.GoHome();host.SetSpecies(old);
            foreach(double size in new[]{.2,1,1.8}) foreach(double dpi in new[]{1,1.5,2}) {
                var prey=SpiderHunt();prey.ConfigureDisplays(new[]{new Area(0,0,300,300,dpi)},0);prey.SetScale(size);prey.Fly=new V(150,150);
                var visual=new DrawingVisual();using(var d=visual.RenderOpen()) PetCanvas.DrawPrey(d,prey);
                var bitmap=new RenderTargetBitmap(300,300,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);byte[] pixels=new byte[300*300*4];bitmap.CopyPixels(pixels,1200,0);
                int visible=0;bool edge=false;for(int y=0;y<300;y++) for(int x=0;x<300;x++) if(pixels[(y*300+x)*4+3]>0) {visible++;if(x==0 || x==299 || y==0 || y==299) edge=true;}
                Check(visible>10 && !edge,"spider prey is visible and unclipped at size "+size+" / DPI "+dpi);
                Check(pixels[3]==0,"spider prey keeps a transparent background");
            }
            RenderPreyPreview(folder);
        }
    }
}
