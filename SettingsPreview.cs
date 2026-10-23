using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace CornerSpider {
    public sealed class FingerPreview : FrameworkElement {
        readonly World preview=new World(2000,1200,42);
        public double Tone=World.DefaultFingerTone;
        public bool Dark=true;
        public FingerPreview() { ClipToBounds=true;MinHeight=250; }
        protected override void OnRender(DrawingContext d) {
            base.OnRender(d);
            d.DrawRoundedRectangle(new SolidColorBrush(Dark?Color.FromRgb(30,42,49):Color.FromRgb(232,236,226)),null,new Rect(0,0,ActualWidth,ActualHeight),14,14);
            double scale=Math.Max(.1,Math.Min((ActualWidth-32)/220,(ActualHeight-32)/175));
            preview.Scale=scale;preview.FingerTone=Tone;preview.FingerBlend=1;
            preview.FingerOrigin=new V(ActualWidth/2-30*scale,ActualHeight/2-15*scale);
            PetCanvas.DrawFinger(d,preview);
        }
    }
    public sealed class SettingsPreview : FrameworkElement,IDisposable {
        readonly World preview=new World(2000,1200,42);
        readonly World silkPreview=new World(400,400,42) { Web=1,AutoFeed=false,CursorPlay=false };
        PetCanvas silkArt=new PetCanvas();
        bool disposed;
        readonly DispatcherTimer timer=new DispatcherTimer { Interval=TimeSpan.FromMilliseconds(50) };
        public string Species="Regal";
        public double PetScale=1,Angle=180;
        public bool Enlarged=true,Dark=true;
        public bool Running { get { return timer.IsEnabled; } }
        public SettingsPreview() {
            ClipToBounds=true;MinHeight=250;timer.Tick+=Animate;
            Loaded+=delegate { if(!disposed) timer.Start(); };Unloaded+=delegate { timer.Stop(); };
        }
        void Animate(object sender,EventArgs e) { if(IsVisible) { preview.Clock+=.05;InvalidateVisual(); } }
        protected override void OnRender(DrawingContext d) {
            base.OnRender(d);
            if(disposed) return;
            var color=Dark?Color.FromRgb(30,42,49):Color.FromRgb(232,236,226);
            var bounds=new Rect(0,0,ActualWidth,ActualHeight);
            d.DrawRoundedRectangle(new SolidColorBrush(color),null,bounds,14,14);
            d.PushClip(new RectangleGeometry(bounds,14,14));
            var grid=new Pen(new SolidColorBrush(Dark?Color.FromArgb(18,255,255,255):Color.FromArgb(16,35,50,40)),1);
            for(double x=20;x<ActualWidth;x+=24) d.DrawLine(grid,new Point(x,0),new Point(x,ActualHeight));
            for(double y=20;y<ActualHeight;y+=24) d.DrawLine(grid,new Point(0,y),new Point(ActualWidth,y));
            // Use the desktop silk renderer and retain its current artwork between
            // animation ticks. The separate world never changes the pet's nest.
            // Reserve enough space for the largest size so the slider scales the
            // whole retreat without moving its attachments away from the corner.
            double silkFit=Math.Max(0,Math.Min(170,Math.Min(ActualWidth*.42,ActualHeight*.46)))/230;
            double silkScale=silkFit*(Enlarged?1:Math.Max(.2,Math.Min(1.8,PetScale))/1.8);
            if(silkScale>0) {
                d.PushTransform(new TranslateTransform(ActualWidth-400*silkScale,0));
                d.PushTransform(new ScaleTransform(silkScale,silkScale));
                silkArt.DrawHammock(d,silkPreview,false,Species,true);
                silkArt.DrawHammock(d,silkPreview,true,Species,true);
                d.Pop();d.Pop();
            }
            preview.SetSpecies(Species);double cycle=preview.Clock%16;
            preview.State=cycle<2?Mood.Inspect:cycle>=3 && cycle<6.5?Mood.Groom:Mood.Rest;
            if(cycle>=9 && cycle<9+preview.Personality.Duration) { preview.State=Mood.SpeciesAct;preview.StateTime=cycle-9; }
            else preview.StateTime=preview.State==Mood.Groom?cycle-3:cycle;preview.Angle=Angle;
            preview.Scale=Enlarged?Math.Min(2.5,Math.Min(ActualWidth,ActualHeight)/(SpiderPose.Radius*2+18)):PetScale;
            preview.Spider=new V(ActualWidth/2,ActualHeight/2+8);PetCanvas.DrawSpider(d,preview,Species);
            d.Pop();
        }
        public void Dispose() { disposed=true;timer.Stop();timer.Tick-=Animate;silkArt=null;InvalidateVisual(); }
    }
}
