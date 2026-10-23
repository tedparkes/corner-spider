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
        static StreamGeometry Pocket() {
            var g=new StreamGeometry();using(var p=g.Open()) {
                p.BeginFigure(new Point(-16,-43),true,true);
                p.BezierTo(new Point(-29,-32),new Point(-34,-5),new Point(-29,20),true,false);
                p.BezierTo(new Point(-27,39),new Point(-19,49),new Point(-13,51),true,false);
                p.BezierTo(new Point(-5,48),new Point(6,48),new Point(14,50),true,false);
                p.BezierTo(new Point(30,34),new Point(33,6),new Point(29,-16),true,false);
                p.BezierTo(new Point(27,-32),new Point(22,-42),new Point(16,-44),true,false);
                p.BezierTo(new Point(7,-40),new Point(-7,-40),new Point(-16,-43),true,false);
            }g.Freeze();return g;
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
                double along=(top?46-t*74:28-t*74)+R(random,-4,4);
                ends[i]=NestPoint((-25+R(random,-2,2))*profile.Width,along*profile.Length);
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
                for(int i=0;i<12;i++) {
                    Point root=roots[i];Point at=Lerp(high,low,(i%6)/6.0);
                    Rib(d,.24,.34,root,at,i%4==0);Rib(d,.20,.25,at,ends[(i+5)%count],false);
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
                var shape=Pocket();
                d.PushOpacity((.5+maturity*.5)*profile.Density);d.DrawGeometry(B("#1fe6f0eb"),null,shape);d.Pop();
                d.PushClip(shape);
                Oval(d,SoftGlow("#42edf7ef"),-20,7,19,53);Oval(d,SoftGlow("#38edf7ef"),22,8,18,49);
                // Uneven, crossing lengthwise sheets, not a regular capture-web grid.
                for(int i=0;i<(65+level*2)*profile.Density;i++) {
                    double x=R(random,-33,33),end=R(random,-25,25);
                    Fiber(d,R(random,.19,.46),i%11==0?.55:.25,new Point(x,-50),new Point(x*.8+R(random,-5,5),-12),new Point(end*.8+R(random,-6,6),27),new Point(end,56),i%17==0);
                }
                random=new Random(profile.Seed+113);
                for(int i=0;i<(16+level)*profile.Weave;i++) {
                    double y=R(random,-46,49);
                    Fiber(d,R(random,.12,.32),.3,new Point(-35,y),new Point(-12,y+R(random,-9,10)),new Point(14,y+R(random,-7,9)),new Point(35,y+R(random,-11,11)),false);
                }
                d.Pop();d.Pop();d.Pop();
            }nestBack.Freeze();
            using(var d=nestFront.Open()) {
                var random=new Random(profile.Seed+5516);
                d.PushTransform(new RotateTransform(135));d.PushTransform(new ScaleTransform(profile.Width,profile.Length));var shape=Pocket();
                d.PushOpacity((.6+maturity*.4)*Math.Min(1,profile.Density));d.DrawGeometry(B("#16f3f9ef"),null,shape);d.Pop();
                d.PushClip(shape);
                // A thin translucent cover leaves eyes and coat visible underneath.
                for(int i=0;i<(30+level)*profile.Density;i++) {
                    double x=R(random,-34,34),end=x+R(random,-18,18);
                    Fiber(d,R(random,.15,.38),i%13==0?.5:.25,new Point(x,-50),new Point(x+R(random,-7,7),-11),new Point(end+R(random,-6,6),28),new Point(end,56),i%23==0);
                }
                random=new Random(profile.Seed+224);
                for(int i=0;i<(10+level/2)*profile.Weave;i++) {
                    double y=R(random,-43,48);
                    Fiber(d,.18,.25,new Point(-34,y),new Point(-14,y+9),new Point(15,y-4),new Point(34,y+8),false);
                }
                d.Pop();
                random=new Random(profile.Seed+225);
                // Loose, frayed rims suggest the two small retreat entrances.
                for(int end=-1;end<=1;end+=2) for(int i=0;i<6;i++) {
                    double y=end<0?-43:50,offset=R(random,-1.8,1.8);
                    Fiber(d,.35+maturity*.12,.3,new Point(-16-i*.4,y+offset),new Point(-8,y-end*5+offset),new Point(7,y-end*5+offset),new Point(16+i*.3,y+offset),i==0);
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
            d.Pop();d.Pop();
        }
    }
}
