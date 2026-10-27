using System;
using System.Windows;
using System.Windows.Media;

namespace CornerSpider {
    public partial class PetCanvas {
        // Two retained layers: the pet rests between the silk floor and its veil.
        // Only this canvas's current pair is retained; growth cannot grow a cache.
        DrawingGroup nestBack,nestFront;
        CachedArt nestBackImage,nestFrontImage;
        int nestLevel=-1;
        double nestEdgeX,nestEdgeY;
        string nestSpecies;
        internal int NestBuilds;
        static double R(Random random,double a,double b) { return a+random.NextDouble()*(b-a); }
        static Point NestPoint(double x,double y) { return new Point(-(x+y)*.70710678,(x-y)*.70710678); }
        static void Fiber(DrawingContext d,double opacity,double width,Point a,Point b,Point c,Point end,bool bundle) {
            d.PushOpacity(opacity);
            // A quiet shadow under occasional bundles keeps pale silk legible
            // on light wallpaper without outlining every individual strand.
            if(bundle) Curve(d,"#5234464d",width+.7,a,b,c,end);
            Curve(d,"#edf8fbf4",width,a,b,c,end);d.Pop();
        }
        static Point PocketPoint(SilkProfile p,int side,double t,double inset=0) {
            return new Point(p.Edge(t,side)-side*inset,p.Along(t));
        }
        internal static StreamGeometry Pocket(SilkProfile profile) {
            var g=new StreamGeometry();using(var p=g.Open()) {
                p.BeginFigure(PocketPoint(profile,-1,0),true,true);
                for(int i=1;i<=64;i++) p.LineTo(PocketPoint(profile,-1,i/64.0),true,false);
                Point a=PocketPoint(profile,1,1),b=PocketPoint(profile,-1,1);
                p.BezierTo(new Point(b.X*.5,b.Y-4),new Point(a.X*.5,a.Y-4),a,true,false);
                for(int i=63;i>=0;i--) p.LineTo(PocketPoint(profile,1,i/64.0),true,false);
                a=PocketPoint(profile,-1,0);b=PocketPoint(profile,1,0);
                p.BezierTo(new Point(b.X*.5,b.Y+4),new Point(a.X*.5,a.Y+4),a,true,false);
            }g.Freeze();return g;
        }
        static void Seam(DrawingContext d,SilkProfile profile,int side,double inset,double opacity,double width) {
            var g=new StreamGeometry();using(var p=g.Open()) {
                p.BeginFigure(PocketPoint(profile,side,0,inset),false,false);
                for(int i=1;i<=64;i++) p.LineTo(PocketPoint(profile,side,i/64.0,inset),true,false);
            }g.Freeze();d.PushOpacity(opacity);d.DrawGeometry(null,P("#edf8fbf4",width),g);d.Pop();
        }
        static void RetreatDetails(DrawingContext d,SilkProfile p,double maturity) {
            // Construction changes the silhouette and folded layers, rather than
            // merely recoloring a common oval. No decoration blocks the end axis.
            double strength=.65+maturity*.35;
            switch(p.Style) {
                case RetreatStyle.Tube:case RetreatStyle.LongTube:case RetreatStyle.Pocket:
                    for(int side=-1;side<=1;side+=2) for(int i=0;i<5;i++)
                        Seam(d,p,side,1.2+i*.9,strength*.24*p.Density,i==0?.65:.35);
                    break;
                case RetreatStyle.Bark:case RetreatStyle.Crevice:
                    for(int side=-1;side<=1;side+=2) for(int i=0;i<4;i++)
                        Seam(d,p,side,1+i*.7,strength*.30,.4);
                    break;
                case RetreatStyle.Leaf:
                    for(int i=0;i<14;i++) {
                        double x=i*.55;
                        Fiber(d,.16*strength,.45,new Point(-24+x,-25),new Point(9+x,-15),new Point(-9+x,18),new Point(23+x,33),i==0);
                    }
                    break;
                case RetreatStyle.Cell:
                    for(int side=-1;side<=1;side+=2) Seam(d,p,side,2,.55*strength,.7);
                    for(int i=0;i<12;i++) {
                        double y=-20+i*4;
                        Fiber(d,.20*strength,.4,new Point(-27,y),new Point(-9,y-3),new Point(12,y-3),new Point(29,y),false);
                    }
                    break;
                case RetreatStyle.Taper:
                    for(int i=0;i<9;i++) {
                        double x=-19+i*5;
                        Fiber(d,.27*strength,.45,new Point(x,-29),new Point(x*1.1,-4),new Point(x*.5,28),new Point(x*.35,48),i%4==0);
                    }
                    break;
            }
        }
        static void TangledWrap(DrawingContext d,SilkProfile p,int level) {
            var r=new Random(p.Seed+339);
            for(int i=0;i<48+level;i++) {
                double t=R(r,.08,.92),next=R(r,.08,.92);int side=i%2==0?-1:1;
                Point a=PocketPoint(p,side,t),b=PocketPoint(p,side,next);
                Fiber(d,.22,.3,a,new Point(a.X+side*R(r,6,16),a.Y+R(r,-12,12)),new Point(b.X+side*R(r,6,16),b.Y+R(r,-12,12)),b,i%13==0);
            }
        }
        static Point Lerp(Point a,Point b,double t) { return new Point(a.X+(b.X-a.X)*t,a.Y+(b.Y-a.Y)*t); }
        static Point RibPoint(Point a,Point b,double t) {
            // All cross threads terminate on an actual tension rib.
            Point p=Lerp(a,b,t);double sag=5.1*t*(1-t);
            return new Point(p.X-sag,p.Y+sag);
        }
        static void Rib(DrawingContext d,double opacity,double width,Point a,Point b,bool bundle) {
            Fiber(d,opacity,width,a,new Point(a.X+(b.X-a.X)/3-1.7,a.Y+(b.Y-a.Y)/3+1.7),new Point(a.X+(b.X-a.X)*2/3-1.7,a.Y+(b.Y-a.Y)*2/3+1.7),b,bundle);
        }
        static void Foundation(DrawingContext d,SilkProfile profile,int level,double edgeX,double edgeY) {
            double maturity=level/30.0;
            var random=new Random(profile.Seed+77);
            const int count=18;var roots=new Point[count];var ends=new Point[count];
            for(int i=0;i<count;i++) {
                bool top=i<count/2;int at=i%(count/2);double t=Math.Max(0,Math.Min(1,(at+R(random,-.26,.26))/8.0));
                // Broadly spaced attachment patches meet both actual desktop edges.
                // Their roots extend back toward the corner, not just two thin fans.
                roots[i]=top?new Point(-56+(edgeX+48)*t,-edgeY):new Point(edgeX,-edgeY+8+(edgeY+48)*t);
                double along=Math.Max(.04,Math.Min(.96,(top? .93-t*.79:.74-t*.70)+R(random,-.025,.025)));
                // Meet this retreat's actual side seam, even on narrow sleeves.
                ends[i]=NestPoint((profile.Edge(along,-1)+1)*profile.Width,profile.Along(along)*profile.Length);
                for(int strand=0;strand<3;strand++) {
                    Point a=top?new Point(roots[i].X+strand*.8,roots[i].Y):new Point(roots[i].X,roots[i].Y+strand*.8);
                    Point b=new Point(ends[i].X+strand*.35,ends[i].Y-strand*.35);
                    Rib(d,(strand==0?.40:.16)+maturity*.12,strand==0?.5:.22,a,b,strand==0 && i%3==0);
                }
                // Small silk attachment smears run along the substrate edge.
                d.PushOpacity(.30);
                if(top) d.DrawLine(P("#edf8fbf4",.7),new Point(roots[i].X-3,roots[i].Y+.35),new Point(roots[i].X+4,roots[i].Y+.35));
                else d.DrawLine(P("#edf8fbf4",.7),new Point(roots[i].X-.35,roots[i].Y-3),new Point(roots[i].X-.35,roots[i].Y+4));d.Pop();
            }
            // Irregular cross ties stitch neighboring load-bearing bundles into
            // a continuous support mesh. No regular radial/orb-web pattern.
            for(int i=0;i<24+(int)(level*.7*profile.Weave);i++) {
                int first=random.Next(count),next=first<9?Math.Min(8,first+1+random.Next(3)):Math.Min(17,first+1+random.Next(3));
                if(first==next) next=first-1;
                double t=R(random,.24,.90);
                Point a=RibPoint(roots[first],ends[first],t),b=RibPoint(roots[next],ends[next],Math.Max(.12,Math.Min(.96,t+R(random,-.1,.1))));
                Rib(d,.19+maturity*.10,.24,a,b,false);
            }
            // A loose funnel-like volume gives Portia a different silhouette.
            // The sleeping platform retains both usable end entrances.
            if(profile.Scaffold) {
                Point high=new Point(edgeX*.64,-edgeY*.83),low=NestPoint(-22,22);
                for(int i=0;i<24;i++) {
                    Point root=roots[i%count];Point at=Lerp(high,low,(i%8)/8.0);
                    Rib(d,.29,.38,root,at,i%4==0);Rib(d,.23,.3,at,ends[(i+5)%count],false);
                }
            }
        }
        void BuildHammock(int level,double edgeX,double edgeY,string species) {
            nestLevel=level;nestEdgeX=edgeX;nestEdgeY=edgeY;nestSpecies=species;NestBuilds++;
            var profile=SilkProfile.Get(species);
            double maturity=level/30.0;
            nestBack=new DrawingGroup();nestFront=new DrawingGroup();
            using(var d=nestBack.Open()) {
                var random=new Random(profile.Seed);
                Foundation(d,profile,level,edgeX,edgeY);
                d.PushTransform(new RotateTransform(135));d.PushTransform(new ScaleTransform(profile.Width,profile.Length));
                var shape=Pocket(profile);
                if(profile.Style==RetreatStyle.Tangle) TangledWrap(d,profile,level);
                d.PushOpacity((.5+maturity*.5)*profile.Density);d.DrawGeometry(B(profile.Scaffold?"#08e6f0eb":"#1fe6f0eb"),null,shape);d.Pop();
                d.PushClip(shape);
                if(!profile.Scaffold) {
                    Oval(d,SoftGlow("#42edf7ef"),profile.Edge(.5,-1),7,12,53);
                    Oval(d,SoftGlow("#38edf7ef"),profile.Edge(.5,1),8,12,49);
                }
                // Uneven, crossing lengthwise sheets, not a regular capture-web grid.
                for(int i=0;i<(65+level*2)*profile.Density;i++) {
                    double x=R(random,-53,53),end=R(random,-42,42);
                    Fiber(d,R(random,.19,.46),i%11==0?.55:.25,new Point(x,-50),new Point(x*.8+R(random,-5,5),-12),new Point(end*.8+R(random,-6,6),27),new Point(end,56),i%17==0);
                }
                random=new Random(profile.Seed+113);
                for(int i=0;i<(16+level)*profile.Weave;i++) {
                    double y=R(random,-46,49);
                    Fiber(d,R(random,.12,.32),.3,new Point(-55,y),new Point(-12,y+R(random,-9,10)),new Point(14,y+R(random,-7,9)),new Point(55,y+R(random,-11,11)),false);
                }
                d.Pop();d.Pop();d.Pop();
            }nestBack.Freeze();
            using(var d=nestFront.Open()) {
                var random=new Random(profile.Seed+5516);
                d.PushTransform(new RotateTransform(135));d.PushTransform(new ScaleTransform(profile.Width,profile.Length));var shape=Pocket(profile);
                d.PushClip(shape);
                // Portia's sleeping sheet is open. Only the wall-side lip has a
                // canopy; the broad floor and tall scaffold remain visible below.
                if(profile.Scaffold) d.PushClip(new RectangleGeometry(new Rect(-60,-60,39,125)));
                d.PushOpacity((.6+maturity*.4)*Math.Min(1,profile.Density));d.DrawGeometry(B("#16f3f9ef"),null,shape);d.Pop();
                // A thin translucent cover leaves eyes and coat visible underneath.
                for(int i=0;i<(30+level)*profile.Density;i++) {
                    double x=R(random,-53,53),end=x+R(random,-18,18);
                    Fiber(d,R(random,.15,.38),i%13==0?.5:.25,new Point(x,-50),new Point(x+R(random,-7,7),-11),new Point(end+R(random,-6,6),28),new Point(end,56),i%23==0);
                }
                random=new Random(profile.Seed+224);
                for(int i=0;i<(10+level/2)*profile.Weave;i++) {
                    double y=R(random,-43,48);
                    Fiber(d,.18,.25,new Point(-55,y),new Point(-14,y+9),new Point(15,y-4),new Point(55,y+8),false);
                }
                RetreatDetails(d,profile,maturity);
                if(profile.Scaffold) d.Pop();
                d.Pop();
                random=new Random(profile.Seed+225);
                // Loose, frayed rims suggest the two small retreat entrances.
                int rims=profile.Scaffold?2:profile.Style==RetreatStyle.Tube?11:profile.Style==RetreatStyle.Pocket?9:5;
                for(int end=-1;end<=1;end+=2) for(int i=0;i<rims;i++) {
                    double y=end<0?-43:50,offset=R(random,-1.8,1.8);
                    double t=end<0?0:1,left=profile.Edge(t,-1),right=profile.Edge(t,1);
                    Fiber(d,(.35+maturity*.12)*Math.Min(1,profile.Density),.3,new Point(left-i*.18,y+offset),new Point(left*.5,y-end*(4+i*.28)+offset),new Point(right*.5,y-end*(4+i*.28)+offset),new Point(right+i*.18,y+offset),i==0);
                }
                d.Pop();d.Pop();
            }nestFront.Freeze();
            nestBackImage=new CachedArt();nestFrontImage=new CachedArt();
        }
        public void DrawNestScene(DrawingContext d,World w,string species) {
            DrawHammock(d,w,false,species);
            if(!w.UnderNestSilk) DrawHammock(d,w,true,species);
            DrawSpider(d,w,species);
            if(w.UnderNestSilk) DrawHammock(d,w,true,species);
        }
        public void DrawHammock(DrawingContext d,World w,bool front,string species=null,bool vectorPreview=false) {
            if(w.WebVisibility<=0) return;
            d.PushOpacity(w.WebVisibility);
            species=species??Species;
            var a=w.Areas[w.HomeArea];double scale=w.HomeScale;
            double edgeX=Math.Abs((w.Corner.EndsWith("Right")?a.X+a.W:a.X)-w.Home.X)/scale;
            double edgeY=Math.Abs((w.Corner.StartsWith("Top")?a.Y:a.Y+a.H)-w.Home.Y)/scale;
            int level=(int)(Math.Max(0,Math.Min(1,w.Web))*30);
            if(nestBack==null || nestSpecies!=species || nestLevel!=level || Math.Abs(nestEdgeX-edgeX)>.01 || Math.Abs(nestEdgeY-edgeY)>.01) BuildHammock(level,edgeX,edgeY,species);
            d.PushTransform(new TranslateTransform(w.Home.X,w.Home.Y));
            d.PushTransform(new ScaleTransform(scale*(w.Corner.EndsWith("Right")?1:-1),scale*(w.Corner.StartsWith("Top")?1:-1)));
            if(vectorPreview) d.DrawDrawing(front?nestFront:nestBack);
            else {
                // Only desktop rendering needs 4x image caches. The small settings
                // thumbnail uses the same frozen vectors without render targets.
                // Each canvas retains only its current pair, never old growth stages.
                if(front && nestFrontImage.Image==null) nestFrontImage=CacheArt(nestFront);
                if(!front && nestBackImage.Image==null) nestBackImage=CacheArt(nestBack);
                CachedArt art=front?nestFrontImage:nestBackImage;d.DrawImage(art.Image,art.Bounds);
            }
            d.Pop();d.Pop();d.Pop();
        }
    }
}
