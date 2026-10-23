using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace CornerSpider {
    public static partial class RuntimeTests {
        public static void RenderTravelPreview(string folder) {
            string frames=Path.Combine(folder,"travel-frames");Directory.CreateDirectory(frames);
            var w=new World(1600,700,1);w.AutoFeed=w.CursorPlay=false;w.SetSpecies("Bold");
            w.Spider=new V(200,350);w.Target=new V(1400,350);w.State=Mood.Walk;w.Angle=90;
            var trail=new DrawingGroup();V previous=w.Spider;
            var csv=new StringBuilder("frame,seconds,state,x,y,speed,angle,sideways,gaitX,gaitY\n");
            double rest=0;bool sawSideways=false,sawRun=false;
            for(int frame=0;frame<600;frame++) {
                w.Tick(.04,new V(-10000,-10000));sawSideways|=w.SideStepping;sawRun|=w.Speed>250;
                var segment=new DrawingGroup();using(var d=segment.Open()) {
                    d.DrawLine(new Pen(new SolidColorBrush(w.SideStepping?Color.FromRgb(229,179,106):Color.FromRgb(92,153,147)),1.3),new Point(previous.X,previous.Y),new Point(w.Spider.X,w.Spider.Y));
                }segment.Freeze();trail.Children.Add(segment);previous=w.Spider;
                var image=new DrawingVisual();using(var d=image.RenderOpen()) {
                    d.DrawRectangle(BPreview("#24313a"),null,new Rect(0,0,960,420));
                    Text(d,"TRAVEL / short probes, longer runs and sideways scuttles",18,12,21,Colors.White);
                    Text(d,w.State==Mood.Rest?"Arrived and settled":w.Speed<1?"Pause and look":w.SideStepping?"Sideways adjustment":w.Speed>240?"Longer run":"Shorter movement",18,43,16,Color.FromRgb(214,222,211));
                    d.PushTransform(new ScaleTransform(.6,.6));d.DrawDrawing(trail);PetCanvas.DrawSpider(d,w,"Bold");d.Pop();
                    Text(d,"Speed "+(w.Speed/w.BodyScale).ToString("F0",CultureInfo.InvariantCulture)+" logical px/s",18,384,15,Colors.White);
                    // Detail view uses the same pose, orientation and leg solver.
                    d.PushClip(new RectangleGeometry(new Rect(770,240,180,170)));
                    d.DrawRectangle(BPreview("#1d2932"),null,new Rect(770,240,180,170));
                    d.PushTransform(new TranslateTransform(860,325));d.PushTransform(new ScaleTransform(1.5,1.5));
                    d.PushTransform(new TranslateTransform(-w.Spider.X,-w.Spider.Y));PetCanvas.DrawSpider(d,w,"Bold");d.Pop();d.Pop();d.Pop();d.Pop();
                }
                SavePng(image,960,420,Path.Combine(frames,frame.ToString("D4")+".png"));
                if(w.SideStepping) SavePng(image,960,420,Path.Combine(folder,"travel-sideways.png"));
                csv.AppendLine(string.Format(CultureInfo.InvariantCulture,"{0},{1:F2},{2},{3:F3},{4:F3},{5:F3},{6:F3},{7},{8:F4},{9:F4}",frame,w.Clock,w.State,w.Spider.X,w.Spider.Y,w.Speed,w.Angle,w.SideStepping,w.GaitDirection.X,w.GaitDirection.Y));
                if(w.State==Mood.Rest) {
                    rest+=.04;
                    if(rest>=.8) { SavePng(image,960,420,Path.Combine(folder,"travel-path.png"));break; }
                }
            }
            File.WriteAllText(Path.Combine(folder,"travel-trace.csv"),csv.ToString());
            if(w.State!=Mood.Rest || !sawSideways || !sawRun) throw new InvalidOperationException("Travel preview must include a run, a sideways step and arrival.");
        }
    }
}
