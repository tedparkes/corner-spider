using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace CornerSpider {
    public static partial class RuntimeTests {
        static void CheckClickDismount(PetHost host) {
            var w=host.World;w.HasFly=false;w.CursorPlay=true;w.SetScale(1);
            V origin=w.Areas[w.HomeArea].Center;
            w.State=Mood.Ride;w.RideAge=6;w.FingerPosition=65;w.FingerDirection=1;
            w.FingerOrigin=w.Cursor=origin;w.Spider=w.FingerPoint(65);w.FingerBlend=1;
            host.Render();Check(host.CursorVisibility.WaitUntilHidden(1500),"click-departure fixture starts with the real cursor hidden");
            host.Clicks.Record(0,0x201);w.Tick(.025,origin,host.Clicks.Consume());host.Render();
            Check(w.State==Mood.Dismount && w.FingerBlend==1,"mouse-button notification starts planned departure with a solid launch pad");
            Check(!w.FingerActive && host.CursorVisibility.WaitForRestore(1500),"click restores native cursor while departure is being prepared");
            for(int i=0;i<80 && w.State==Mood.Dismount;i++) w.Tick(.025,origin);
            Check(w.State==Mood.Jump && w.JumpEnd.Y<origin.Y-80*w.BodyScale && w.SilkOpacity>0,"native world takes off toward an upper landing with silk");
            for(int i=0;i<80;i++) w.Tick(.025,new V(-10000,-10000));
            Check(w.State!=Mood.Ride && w.FingerBlend==0 && w.SilkOpacity==0,"planned departure cleans up its finger and silk");
            w.GoHome();host.Render();
        }
        static World DismountScene(bool edge,bool hanging) {
            var w=new World(430,410,63);w.AutoFeed=w.CursorPlay=false;w.Scale=1;
            w.FingerOrigin=new V(185,edge?78:285);w.FingerPosition=65;w.FingerBlend=1;w.FingerDirection=1;
            w.State=Mood.Ride;w.RideAge=hanging?.5:6;w.Angle=hanging?0:90;
            w.Spider=hanging?w.CursorLanding(w.FingerOrigin):w.FingerPoint(65);return w;
        }
        static void DrawDismountScene(DrawingContext d,World w,string title) {
            d.PushClip(new RectangleGeometry(new Rect(0,0,430,410)));
            d.DrawRectangle(new SolidColorBrush(Color.FromRgb(30,42,49)),null,new Rect(0,0,430,410));
            Text(d,title,16,12,17,Colors.White);
            PetCanvas.DrawFinger(d,w);
            if(w.SilkOpacity>0) {
                d.PushOpacity(w.SilkOpacity);d.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(180,241,250,238)),.9),new Point(w.SilkStart.X,w.SilkStart.Y),new Point(w.SilkEnd.X,w.SilkEnd.Y));d.Pop();
            }
            PetCanvas.DrawSpider(d,w,"Bold");
            if(w.State==Mood.Dismount || w.State==Mood.Jump) {
                // Diagnostic marker only; no target circle is shown on the desktop.
                d.DrawEllipse(null,new Pen(new SolidColorBrush(Color.FromArgb(140,157,209,178)),1),new Point(w.JumpEnd.X,w.JumpEnd.Y),5,5);
            }
            d.Pop();
        }
        public static void RenderDismountPreview(string folder) {
            var visual=new DrawingVisual();using(var d=visual.RenderOpen()) {
                d.DrawRectangle(Brushes.White,null,new Rect(0,0,1720,880));
                Text(d,"CLICK / CHOOSE A SPOT, TURN, BRACE, JUMP UP",18,12,24,Color.FromRgb(30,50,44));
                for(int row=0;row<2;row++) for(int stage=0;stage<4;stage++) {
                    var w=DismountScene(row==1,false);V origin=w.FingerOrigin;w.Tick(.025,origin,true);
                    if(stage>0) for(int i=0;i<100 && w.State==Mood.Dismount;i++) { w.Tick(.016,origin);if(stage==1 && w.DismountBrace>.55) break; }
                    if(stage==2) { while(w.State==Mood.Jump && w.JumpProgress<.5) w.Tick(.016,origin); }
                    if(stage==3) { for(int i=0;i<100 && w.State!=Mood.Land;i++) w.Tick(.016,origin); }
                    string[] titles={"Click: choose the landing","Turn and brace","Upward takeoff + silk","Land at the chosen spot"};
                    if(row==1) titles=new[]{"Near the top edge","Aim along the ceiling","Hop to safe space","Land without dropping"};
                    d.PushTransform(new TranslateTransform(stage*430,52+row*414));DrawDismountScene(d,w,titles[stage]);d.Pop();
                }
            }SavePng(visual,1720,880,Path.Combine(folder,"click-dismount.png"));
            string frames=Path.Combine(folder,"dismount-frames");Directory.CreateDirectory(frames);
            foreach(bool hanging in new[]{false,true}) {
                var w=DismountScene(false,hanging);w.CursorPlay=true;V origin=w.FingerOrigin;
                for(int frame=0;frame<65;frame++) {
                    if(frame>=10) w.Tick(.04,origin,frame==10);
                    var v=new DrawingVisual();using(var d=v.RenderOpen()) DrawDismountScene(d,w,hanging?"Click during a catch":"Click from the finger");
                    SavePng(v,430,410,Path.Combine(frames,(hanging?"catch-":"perch-")+frame.ToString("D3")+".png"));
                }
            }
        }
    }
}
