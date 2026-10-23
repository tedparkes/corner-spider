using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CornerSpider {
    public static partial class RuntimeTests {
        static BitmapSource PlayLayer(World world,int width,int height) {
            var canvas=new PetCanvas { World=world,Species="Bold",Width=width,Height=height };
            canvas.Measure(new Size(width,height));canvas.Arrange(new Rect(0,0,width,height));
            var bitmap=new RenderTargetBitmap(width,height,96,96,PixelFormats.Pbgra32);bitmap.Render(canvas);return bitmap;
        }
        static World PlayWorld() {
            var w=new World(800,500,12);w.Scale=1.6;w.UpdateHome();w.Spider=new V(240,250);w.AutoFeed=false;w.Web=.75;return w;
        }
        public static void RenderFingerPreview(string folder) {
            var visual=new DrawingVisual();using(var d=visual.RenderOpen()) {
                for(int i=0;i<2;i++) {
                    d.PushTransform(new TranslateTransform(i*500,0));
                    bool dark=i==1;Color ink=dark?Colors.White:Color.FromRgb(35,45,43);
                    d.DrawRectangle(new SolidColorBrush(dark?Color.FromRgb(25,34,43):Color.FromRgb(235,239,233)),null,new Rect(0,0,500,560));
                    Text(d,"Bent finger / soft transparent base",22,20,20,ink);
                    var w=new World(500,560,1);w.Scale=1.9;w.FingerOrigin=new V(210,140);w.FingerBlend=1;
                    PetCanvas.DrawFinger(d,w);
                    w.Scale=1.8;w.FingerOrigin=new V(220,400);w.State=Mood.Ride;w.RideAge=5;w.FingerWaveAge=1.2;w.Angle=180;w.Spider=w.FingerPoint(65);
                    PetCanvas.DrawFinger(d,w);PetCanvas.DrawSpider(d,w,"Bold");
                    Text(d,"Centered on the pointer / arrow hidden during play",22,522,14,ink);d.Pop();
                }
            }
            SavePng(visual,1000,560,Path.Combine(folder,"finger-shape-preview.png"));
            var transparent=new DrawingVisual();using(var d=transparent.RenderOpen()) {
                var w=new World(700,450,1);w.Scale=3;w.FingerBlend=1;w.FingerOrigin=new V(280,170);PetCanvas.DrawFinger(d,w);
            }
            SavePng(transparent,700,450,Path.Combine(folder,"finger-transparent.png"));
            RenderFingerTones(folder);
            RenderBoardingPreview(folder);
        }
        static void RenderBoardingPreview(string folder) {
            var visual=new DrawingVisual();using(var d=visual.RenderOpen()) {
                for(int row=0;row<4;row++) {
                    int side=row==3?-1:1;V cursor=new V(side<0?175:260,190);
                    V offset=row==0 || row==3?new V(-140,40):row==1?new V(65,40):new V(0,-95);
                    var w=new World(500,360,row==0 || row==3?12:2);w.AutoFeed=false;w.Scale=1.25;
                    w.Spider=cursor+new V(offset.X*side,offset.Y)*w.Scale;
                    string title=row==0?"Exposed tip: catch":row==1?"Base side: surface landing":row==2?"Above: surface landing":"Mirrored finger: side catch";
                    for(int column=0;column<3;column++) {
                        for(int i=0;i<700;i++) {
                            w.Tick(.025,cursor);
                            if(column==0 && w.State==Mood.Crouch || column==1 && w.State==Mood.Jump && w.JumpProgress>=.55 || column==2 && w.State==Mood.Ride && w.RideAge>=.10) break;
                        }
                        d.PushTransform(new TranslateTransform(column*500,row*360));d.PushClip(new RectangleGeometry(new Rect(0,0,500,360)));
                        d.DrawRectangle(new SolidColorBrush(row%2==0?Color.FromRgb(30,42,49):Color.FromRgb(232,236,226)),null,new Rect(0,0,500,360));
                        Color ink=row%2==0?Colors.White:Color.FromRgb(30,50,44);
                        Text(d,title,18,12,19,ink);Text(d,column==0?"Choose and brace":column==1?"Jump to the chosen spot":w.Hanging?"Grip with front feet":"Land and settle",18,40,15,ink);
                        PetCanvas.DrawFinger(d,w);PetCanvas.DrawSpider(d,w,"Bold");
                        if(column<2) { V target=w.JumpEnd;d.DrawEllipse(null,new Pen(new SolidColorBrush(ink),1),new Point(target.X,target.Y),4,4); }
                        Text(d,"Actual behavior + desktop renderer",18,330,13,ink);d.Pop();d.Pop();
                    }
                }
            }
            SavePng(visual,1500,1440,Path.Combine(folder,"finger-boarding.png"));
        }
        static void RenderFingerTones(string folder) {
            var visual=new DrawingVisual();using(var d=visual.RenderOpen()) {
                d.DrawRectangle(Brushes.White,null,new Rect(0,0,1440,580));Text(d,"FINGER SKIN TONES / DEEP BROWN TO VERY PALE",20,16,24,Color.FromRgb(30,50,44));
                for(int row=0;row<2;row++) for(int i=0;i<6;i++) {
                    d.PushTransform(new TranslateTransform(i*240,65+row*255));d.PushClip(new RectangleGeometry(new Rect(0,0,240,255)));
                    bool dark=row==1;Color ink=dark?Colors.White:Color.FromRgb(30,50,44);
                    d.DrawRectangle(new SolidColorBrush(dark?Color.FromRgb(30,42,49):Color.FromRgb(232,236,226)),null,new Rect(0,0,240,255));
                    var w=new World(240,255,1);w.Scale=1.1;w.FingerOrigin=new V(100,110);w.FingerBlend=1;w.FingerTone=i*.2;
                    PetCanvas.DrawFinger(d,w);Text(d,(i*20)+"%"+(i==0?" - deep brown":i==5?" - very pale":""),15,215,16,ink);
                    d.Pop();d.Pop();
                }
            }SavePng(visual,1440,580,Path.Combine(folder,"finger-tones.png"));
        }
        static void CheckFingerTones() {
            byte[] alpha=null;long previous=-1;
            foreach(double tone in new[]{0,.2,.4,.6,.8,1}) {
                var visual=new DrawingVisual();using(var d=visual.RenderOpen()) { var w=new World(320,240,1);w.FingerOrigin=new V(100,90);w.FingerBlend=1;w.FingerTone=tone;PetCanvas.DrawFinger(d,w); }
                var bitmap=new RenderTargetBitmap(320,240,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);var pixels=new byte[320*240*4];bitmap.CopyPixels(pixels,320*4,0);
                int opaque=0,faded=0;long brightness=0;bool sameMask=true;
                for(int p=0;p<pixels.Length;p+=4) {
                    if(pixels[p+3]>220) { opaque++;brightness+=pixels[p]+pixels[p+1]+pixels[p+2]; }
                    if(pixels[p+3]>0 && pixels[p+3]<200) faded++;
                    if(alpha!=null && pixels[p+3]!=alpha[p+3]) sameMask=false;
                }
                Check(opaque>2000 && faded>1000,"finger tone "+tone+" retains solid fingertip and transparent hand fade");
                Check(sameMask,"changing finger tone preserves the exact silhouette and alpha fade: "+tone);
                Check(brightness>previous,"finger shades progress from deep brown to very pale: "+tone);previous=brightness;alpha=pixels;
            }
            double last=-1;
            for(int i=0;i<=100;i++) { var palette=new FingerPalette(i);Color c=(Color)ColorConverter.ConvertFromString(palette.Base);double brightness=c.R+c.G+c.B;Check(brightness>last,"finger palette step "+i+" gets lighter");last=brightness; }
            Check(FingerPalette.Key(double.NaN)==80 && FingerPalette.Key(-4)==0 && FingerPalette.Key(4)==100,"renderer rejects nonfinite tones and clamps endpoints");
        }
        public static void RenderPlayPreview(string folder) {
            string frames=Path.Combine(folder,"play-frames");Directory.CreateDirectory(frames);
            var w=PlayWorld();V cursor=new V(320,220);
            var csv=new StringBuilder("frame,state,rideAge,fingerBlend,frontReach,silk,departure\n");
            for(int frame=0;frame<650;frame++) {
                bool fast=frame>=550;
                if(frame==550) {
                    w=PlayWorld();cursor=new V(320,220);
                    for(int i=0;i<350 && !w.OnFinger;i++) w.Tick(.04,cursor);
                }
                if(frame>=225 && frame<475) cursor=new V(320+Math.Sin((frame-225)*.04*.6)*36,220+Math.Sin((frame-225)*.04*.4)*14);
                if(frame==575) cursor+=new V(180,-50);
                w.Tick(.04,cursor,frame==475);
                string label=w.Hanging?"Catch and dangle":w.Climbing?"Climb onto the finger":w.OnFinger?(w.FrontReach(-1)>.05?"Turn to you and wave":"Explore the finger"):w.LastDismount=="click"?"Click: a planned upward hop":w.LastDismount=="fast movement"?"Fast motion: a startled escape":w.FingerBlend>0?"An invitation to climb aboard":"Hold the pointer nearby for five seconds";
                var visual=new DrawingVisual();using(var d=visual.RenderOpen()) {
                    d.DrawRectangle(new SolidColorBrush(Color.FromRgb(229,236,231)),null,new Rect(0,0,800,500));
                    Text(d,"CORNER SPIDER 0.6.4  /  "+(fast?"FAST DEPARTURE":"POINTER PLAY"),22,18,19,Color.FromRgb(25,43,40));
                    Text(d,label,22,48,16,Color.FromRgb(72,97,89));
                    d.DrawImage(PlayLayer(w,800,500),new Rect(0,0,800,500));
                    // Match the real visibility transition; this marker is diagnostic only.
                    var p=new StreamGeometry();using(var g=p.Open()) { g.BeginFigure(new Point(cursor.X,cursor.Y),true,true);g.LineTo(new Point(cursor.X+3,cursor.Y+17),true,false);g.LineTo(new Point(cursor.X+7,cursor.Y+12),true,false);g.LineTo(new Point(cursor.X+13,cursor.Y+12),true,false); }p.Freeze();
                    if(!w.FingerActive || w.FingerBlend<.65) d.DrawGeometry(Brushes.White,new Pen(Brushes.DimGray,1),p);
                    Text(d,"Actual behavior + WPF renderer / 25 fps / centered finger, temporary cursor hiding",22,468,13,Color.FromRgb(72,97,89));
                }
                SavePng(visual,800,500,Path.Combine(frames,frame.ToString("D4")+".png"));
                csv.AppendLine(string.Format(System.Globalization.CultureInfo.InvariantCulture,"{0},{1},{2:F2},{3:F2},{4:F2},{5:F2},{6}",frame,w.State,w.RideAge,w.FingerBlend,w.FrontReach(-1),w.SilkOpacity,w.LastDismount));
            }
            File.WriteAllText(Path.Combine(folder,"play-trace.csv"),csv.ToString());
            var contrast=new DrawingVisual();using(var d=contrast.RenderOpen()) {
                for(int i=0;i<2;i++) {
                    d.PushTransform(new TranslateTransform(i*420,0));
                    bool dark=i==1;d.DrawRectangle(new SolidColorBrush(dark?Color.FromRgb(25,34,43):Color.FromRgb(246,246,243)),null,new Rect(0,0,420,340));
                    Text(d,"Same silk footprint / "+(dark?"dark":"light"),22,22,19,dark?Colors.White:Colors.Black);
                    var nest=new World(420,340,12);nest.Scale=1.8;nest.Web=.8;nest.Home=new V(210,190);nest.Spider=nest.Home;nest.State=Mood.Sleep;nest.Clock=3;
                    d.DrawImage(PlayLayer(nest,420,340),new Rect(0,0,420,340));d.Pop();
                }
            }
            SavePng(contrast,840,340,Path.Combine(folder,"silk-contrast.png"));
        }
    }
}
