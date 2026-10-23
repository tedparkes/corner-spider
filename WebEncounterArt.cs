using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CornerSpider {
    public partial class PetCanvas {
        sealed class GuestSilkCache {
            public GuestSilkCache() {}
            public int Complete=-1;
            public int Builds;
            public double Factor,Scale;
            public BitmapSource Image;
            public Rect Bounds;
            public DrawingGroup FreshSilk;
            public int FreshComplete=-1;
        }
        // Weak keys let ended encounters/previews be collected. Retain one
        // image generated from vectors per live world, not historical build steps.
        static readonly ConditionalWeakTable<World,GuestSilkCache> guestSilk=new ConditionalWeakTable<World,GuestSilkCache>();
        internal static int GuestSilkBuilds(World w) { GuestSilkCache cache;return guestSilk.TryGetValue(w,out cache)?cache.Builds:0; }
        static Point GuestPoint(V p) { return new Point(p.X,p.Y); }
        static void GuestCurve(DrawingContext d,GuestStrand strand,double factor) {
            var g=new StreamGeometry();using(var path=g.Open()) {
                path.BeginFigure(GuestPoint(strand.A),false,false);
                path.BezierTo(GuestPoint(strand.B),GuestPoint(strand.C),GuestPoint(strand.D),true,false);
            }g.Freeze();
            double width=strand.Kind==0?.65:strand.Kind==1?.46:.34;
            d.PushOpacity(strand.Kind==0?.9:strand.Kind==1?.78:.73);
            d.DrawGeometry(null,P("#722e454f",(width+.7)/factor),g);
            d.DrawGeometry(null,P("#edf5fff7",width/factor),g);d.Pop();
        }
        internal static void DrawGuestWeb(DrawingContext d,World w) {
            if(w.GuestOpacity<=0) return;
            int active=GuestWebPattern.Active(w.GuestProgress);
            double fraction=GuestWebPattern.Fraction(active,w.GuestProgress);
            int complete=active+(fraction>=1?1:0);
            var cache=guestSilk.GetOrCreateValue(w);
            // Batch fresh silk into the image about one winding at a time. A
            // new native bitmap for every short segment would create needless
            // allocation pressure throughout the multi-minute construction.
            bool refresh=cache.Complete<0 || complete<cache.Complete || complete-cache.Complete>=GuestWebPattern.Spokes ||
                (complete==GuestWebPattern.Count && cache.Complete!=complete) || cache.Factor!=w.GuestWebFactor || cache.Scale!=w.GuestWebScale;
            if(refresh) {
                var drawing=new DrawingGroup();using(var layer=drawing.Open()) {
                    for(int i=0;i<complete;i++) GuestCurve(layer,GuestWebPattern.Strand(i),w.GuestWebFactor);
                }drawing.Freeze();cache.Image=null;
                if(complete>0) {
                    // Rasterize at slightly above the actual physical-pixel size.
                    // Do not send hundreds of translucent curves through WPF's
                    // layered-window software renderer on every animation frame.
                    double pixels=Math.Max(2,w.GuestWebScale*1.25);
                    Rect bounds=drawing.Bounds;bounds.Inflate(1/pixels,1/pixels);
                    var visual=new DrawingVisual();using(var layer=visual.RenderOpen()) {
                        layer.PushTransform(new ScaleTransform(pixels,pixels));layer.PushTransform(new TranslateTransform(-bounds.X,-bounds.Y));
                        layer.DrawDrawing(drawing);layer.Pop();layer.Pop();
                    }
                    int width=(int)Math.Ceiling(bounds.Width*pixels),height=(int)Math.Ceiling(bounds.Height*pixels);
                    var bitmap=new RenderTargetBitmap(width,height,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);bitmap.Freeze();
                    cache.Image=bitmap;cache.Bounds=new Rect(bounds.X,bounds.Y,width/pixels,height/pixels);
                }
                cache.Complete=complete;cache.Factor=w.GuestWebFactor;cache.Scale=w.GuestWebScale;cache.Builds++;
            }
            if(refresh || cache.FreshComplete!=complete) {
                var fresh=new DrawingGroup();using(var layer=fresh.Open()) {
                    for(int i=cache.Complete;i<complete;i++) GuestCurve(layer,GuestWebPattern.Strand(i),w.GuestWebFactor);
                }fresh.Freeze();cache.FreshSilk=fresh;cache.FreshComplete=complete;
            }
            d.PushOpacity(w.GuestOpacity*.9);
            d.PushTransform(new TranslateTransform(w.GuestCorner.X,w.GuestCorner.Y));
            d.PushTransform(new ScaleTransform((w.GuestInward.X>0?1:-1)*w.GuestWebScale,(w.GuestInward.Y>0?1:-1)*w.GuestWebScale));
            if(cache.Image!=null) d.DrawImage(cache.Image,cache.Bounds);
            d.DrawDrawing(cache.FreshSilk);
            if(fraction>0 && fraction<1) GuestCurve(d,GuestWebPattern.Strand(active).Prefix(fraction),w.GuestWebFactor);
            d.Pop();d.Pop();
            if(w.State==Mood.WebPluck) {
                // A local ripple along the contacted spoke, with its ends fixed.
                V center=w.GuestCenter,anchorPoint=w.GuestPluckPoint,axis=anchorPoint-center;
                V sway=new V(-axis.Y,axis.X)*(w.GuestPulse*Math.Sin(w.GuestStageAge*32)*1.5*w.GuestScale/axis.Length);
                d.PushOpacity(w.GuestPulse*.6);
                SilkCurve(d,.35*w.GuestScale,GuestPoint(center),GuestPoint(center+axis*.33+sway),GuestPoint(center+axis*.67-sway),GuestPoint(anchorPoint));d.Pop();
                // Short tension strands connect the actual contacting feet to the
                // outer web anchor. Their motion follows the articulated forelegs.
                double angle=w.Angle*Math.PI/180;var skin=SkinCatalog.Get(w.Species);
                for(int side=-1;side<=1;side+=2) {
                    V foot=SpiderPose.Leg(w,side,0,skin).Tip;
                    V tip=w.Spider+new V(foot.X*Math.Cos(angle)-foot.Y*Math.Sin(angle),foot.X*Math.Sin(angle)+foot.Y*Math.Cos(angle))*w.BodyScale;
                    V anchor=w.GuestPluckPoint,mid=(anchor+tip)*.5;
                    SilkCurve(d,.45*w.GuestScale,new Point(anchor.X,anchor.Y),new Point(mid.X,mid.Y),new Point(mid.X,mid.Y),new Point(tip.X,tip.Y));
                }
            }
            d.Pop();
        }
    }
}
