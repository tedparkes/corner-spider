using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace CornerSpider {
    public static partial class RuntimeTests {
        static void CheckNestEntry(PetHost host) {
            var w=host.World;w.AutoFeed=w.CursorPlay=false;w.HasFly=false;w.SetScale(1);
            w.Spider=w.Areas[w.HomeArea].Center;w.GoHome();bool entering=false;
            for(int i=0;i<6000 && w.State!=Mood.Sleep;i++) {
                w.Tick(.025,new V(-10000,-10000));
                if(!entering && w.State==Mood.EnterNest) {
                    entering=true;Check((w.Spider-w.NestEntrance).Length<.01,"native world approaches the end opening before entering");host.Render();
                }
            }
            Check(entering && w.State==Mood.Sleep && w.UnderNestSilk,"native world crawls inside before its silk cover and sleep pose");
            for(int i=0;i<100;i++) w.Tick(.025,new V(-10000,-10000));w.Wake();
            for(int i=0;i<400 && w.State==Mood.ExitNest;i++) w.Tick(.025,new V(-10000,-10000));
            Check(w.State==Mood.Rest && !w.UnderNestSilk && (w.Spider-w.NestExit).Length<.01,"native world leaves through the other end on waking");
            w.AutoFeed=w.CursorPlay=true;host.Render();
        }
        static World EntryScene(string corner) {
            var w=new World(1000,800,39);w.AutoFeed=w.CursorPlay=false;w.Corner=corner;w.SetScale(1.5);w.Web=.8;
            V f=w.NestForward;double side=corner.StartsWith("Top")==corner.EndsWith("Right")?1:-1;
            w.Spider=w.Home+new V(f.Y,-f.X)*(150*w.HomeScale*side);w.Angle=31;w.GoHome();return w;
        }
        static double EntryAlong(World w) { V p=(w.Spider-w.Home)*(1/w.HomeScale),f=w.NestForward;return p.X*f.X+p.Y*f.Y; }
        static bool EntryStage(World w,int stage) {
            if(stage==0) return true;
            if(stage==1) return w.State==Mood.EnterNest && w.StateTime>.25;
            if(stage==2) return w.State==Mood.EnterNest && EntryAlong(w)>-55;
            if(stage==3) return w.State==Mood.EnterNest && EntryAlong(w)>-20;
            if(stage==4) return w.State==Mood.Sleep && w.NestTuck==1;
            return w.State==Mood.ExitNest && EntryAlong(w)>45;
        }
        static void DrawEntryScene(DrawingContext d,PetCanvas canvas,World w,bool dark) {
            d.PushClip(new RectangleGeometry(new Rect(0,0,500,500)));
            d.DrawRectangle(BPreview(dark?"#25313b":"#e6eadd"),null,new Rect(0,0,500,500));
            d.PushTransform(new TranslateTransform(w.Corner.EndsWith("Right")?-500:0,w.Corner.StartsWith("Top")?0:-300));
            canvas.DrawNestScene(d,w,"Bold");d.Pop();d.Pop();
        }
        public static void RenderNestEntry(string folder) {
            string[] names={"Walk around the outside","Line up at the end","Head and front legs enter","Abdomen follows through","Settle inside the silk","Wake and crawl out the other end"};
            var canvas=new PetCanvas();
            foreach(bool dark in new[]{true,false}) {
                var visual=new DrawingVisual();using(var d=visual.RenderOpen()) {
                    d.DrawRectangle(Brushes.White,null,new Rect(0,0,1500,1130));
                    Text(d,"CORNER SPIDER / END ENTRANCE",18,12,24,Color.FromRgb(30,50,44));
                    for(int stage=0;stage<6;stage++) {
                        World w=EntryScene("TopRight");
                        for(int i=0;i<5000 && !EntryStage(w,stage);i++) { if(stage==5 && w.State==Mood.Sleep && w.NestTuck==1) w.Wake();w.Tick(.025,new V(-10000,-10000)); }
                        d.PushTransform(new TranslateTransform(stage%3*500,52+stage/3*538));DrawEntryScene(d,canvas,w,dark);
                        Text(d,names[stage],12,506,17,Color.FromRgb(30,50,44));d.Pop();
                    }
                }SavePng(visual,1500,1130,Path.Combine(folder,"nest-entry-"+(dark?"dark":"light")+".png"));
            }
            string frames=Path.Combine(folder,"nest-entry-frames");Directory.CreateDirectory(frames);
            foreach(string corner in Settings.Corners) {
                World w=EntryScene(corner);bool waking=false;int frame=0;
                for(int i=0;i<1800;i++) {
                    if(w.State==Mood.Sleep && w.NestTuck==1 && w.StateTime>=2.5) { w.Wake();waking=true; }
                    w.Tick(.025,new V(-10000,-10000));
                    if(i%4==0) {
                        var visual=new DrawingVisual();using(var d=visual.RenderOpen()) DrawEntryScene(d,canvas,w,true);
                        SavePng(visual,500,500,Path.Combine(frames,corner+"-"+(frame++).ToString("D3")+".png"));
                    }
                    if(waking && w.State==Mood.Rest) break;
                }
            }
        }
    }
}
