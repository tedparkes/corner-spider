using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CornerSpider {
    // Retained, frozen artwork and pens keep the 30 FPS animation inexpensive.
    public partial class PetCanvas : FrameworkElement {
        public World World;
        public string Species="Regal";
        public V Offset;
        public double DeviceScale=1;
        static readonly Dictionary<string,Brush> brushes=new Dictionary<string,Brush>();
        static readonly Dictionary<string,Pen> pens=new Dictionary<string,Pen>();
        static readonly Dictionary<string,DrawingGroup> bodies=new Dictionary<string,DrawingGroup>();
        static readonly Dictionary<string,DrawingGroup> heads=new Dictionary<string,DrawingGroup>();
        static readonly Dictionary<string,DrawingGroup> segments=new Dictionary<string,DrawingGroup>();
        struct CachedArt { public BitmapSource Image;public Rect Bounds; }
        static readonly Dictionary<DrawingGroup,CachedArt> rasterCache=new Dictionary<DrawingGroup,CachedArt>();
        static Brush B(string color) { Brush b;if(!brushes.TryGetValue(color,out b)) { b=(Brush)new BrushConverter().ConvertFromString(color);b.Freeze();brushes[color]=b; }return b; }
        static Pen P(string color,double width) { width=Math.Round(width,1);string key=color+":"+width.ToString(System.Globalization.CultureInfo.InvariantCulture);Pen p;if(!pens.TryGetValue(key,out p)) { p=new Pen(B(color),width) { StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round,LineJoin=PenLineJoin.Round };p.Freeze();pens[key]=p; }return p; }
        static Brush Glow(string bright,string dark) { var b=new RadialGradientBrush((Color)ColorConverter.ConvertFromString(bright),(Color)ColorConverter.ConvertFromString(dark));b.GradientOrigin=new Point(.35,.2);b.Center=new Point(.4,.35);b.RadiusX=.8;b.RadiusY=.8;b.Freeze();return b; }
        static Brush SoftGlow(string color) { var b=new RadialGradientBrush((Color)ColorConverter.ConvertFromString(color),Colors.Transparent);b.Center=b.GradientOrigin=new Point(.5,.5);b.RadiusX=b.RadiusY=.5;b.Freeze();return b; }
        static readonly Brush eye=Glow("#202d32","#020405"),shine=B("#e4eee9");
        static DrawingGroup finger;
        static CachedArt fingerImage;
        static int fingerTone=-1;
        static void Oval(DrawingContext d,Brush b,double x,double y,double rx,double ry) { d.DrawEllipse(b,null,new Point(x,y),rx,ry); }
        static void DrawArt(DrawingContext d,DrawingGroup drawing) {
            CachedArt cached;
            if(!rasterCache.TryGetValue(drawing,out cached)) {
                cached=CacheArt(drawing);rasterCache[drawing]=cached;
            }
            d.DrawImage(cached.Image,cached.Bounds);
        }
        static CachedArt CacheArt(DrawingGroup drawing) {
            // Generate a 4x cache from vectors, not an external asset. Fine fur
            // and silk needn't be retessellated each frame on high-DPI desktops.
            Rect bounds=drawing.Bounds;bounds.Inflate(.5,.5);
            var visual=new DrawingVisual();using(var c=visual.RenderOpen()) {
                c.PushTransform(new ScaleTransform(4,4));c.PushTransform(new TranslateTransform(-bounds.X,-bounds.Y));
                c.DrawDrawing(drawing);c.Pop();c.Pop();
            }
            int width=(int)Math.Ceiling(bounds.Width*4),height=(int)Math.Ceiling(bounds.Height*4);
            var bitmap=new RenderTargetBitmap(width,height,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);bitmap.Freeze();
            return new CachedArt { Image=bitmap,Bounds=new Rect(bounds.X,bounds.Y,width/4.0,height/4.0) };
        }
        static void Line(DrawingContext d,string color,double width,double x,double y,double a,double b) { d.DrawLine(P(color,width),new Point(x,y),new Point(a,b)); }
        static void Curve(DrawingContext d,string color,double width,Point a,Point b,Point c,Point end) {
            var g=new StreamGeometry();using(var x=g.Open()) { x.BeginFigure(a,false,false);x.BezierTo(b,c,end,true,false); }g.Freeze();d.DrawGeometry(null,P(color,width),g);
        }
        static void SilkCurve(DrawingContext d,double width,Point a,Point b,Point c,Point end) {
            Curve(d,"#722e454f",width+1,a,b,c,end);
            Curve(d,"#d9f5fff7",width,a,b,c,end);
        }
        static void SilkLine(DrawingContext d,double width,V a,V b) {
            Line(d,"#70253642",width+1,a.X,a.Y,b.X,b.Y);
            Line(d,"#e0f5fff6",width,a.X,a.Y,b.X,b.Y);
        }
        static DrawingGroup FingerArt(double tone) {
            int key=FingerPalette.Key(tone);
            if(finger!=null && fingerTone==key) return finger;
            fingerTone=key;var tint=new FingerPalette(key);
            finger=new DrawingGroup();using(var d=finger.Open()) {
                // Side-on bent index finger. The hand beyond the knuckle dissolves
                // diagonally into transparency, including its outline and shading.
                var shape=new StreamGeometry();using(var p=shape.Open()) {
                    p.BeginFigure(new Point(0,0),true,true);
                    p.BezierTo(new Point(3,-14),new Point(15,-25),new Point(32,-31),true,false);
                    p.BezierTo(new Point(59,-40),new Point(88,-42),new Point(119,-46),true,false);
                    p.BezierTo(new Point(137,-48),new Point(150,-41),new Point(161,-29),true,false);
                    p.BezierTo(new Point(182,-15),new Point(201,12),new Point(212,32),true,false);
                    p.LineTo(new Point(218,120),true,false);p.LineTo(new Point(105,120),true,false);
                    p.BezierTo(new Point(105,81),new Point(102,37),new Point(111,9),true,false);
                    p.BezierTo(new Point(96,10),new Point(80,20),new Point(63,12),true,false);
                    p.BezierTo(new Point(55,9),new Point(52,5),new Point(48,1),true,false);
                    p.BezierTo(new Point(42,21),new Point(27,29),new Point(12,24),true,false);
                    p.BezierTo(new Point(1,20),new Point(-4,12),new Point(0,0),true,false);
                }shape.Freeze();
                var skin=new LinearGradientBrush { StartPoint=new Point(0,-42),EndPoint=new Point(15,45),MappingMode=BrushMappingMode.Absolute };
                skin.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(tint.Light),0));
                skin.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(tint.Base),.5));
                skin.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(tint.Shade),1));skin.Freeze();
                d.DrawGeometry(skin,P(tint.Outline,.9),shape);
                d.PushClip(shape);
                // Soft rounded pads and knuckle volume, clipped to the silhouette.
                Oval(d,SoftGlow(FingerPalette.Alpha(tint.Highlight,0x60)),72,-24,65,26);
                Oval(d,SoftGlow(FingerPalette.Alpha(tint.Blush,0x35)),20,15,30,20);
                Oval(d,SoftGlow(FingerPalette.Alpha(tint.Blush,0x36)),67,15,43,20);
                Oval(d,SoftGlow(FingerPalette.Alpha(tint.Highlight,0x40)),122,-31,27,20);
                var nail=new StreamGeometry();using(var p=nail.Open()) {
                    p.BeginFigure(new Point(4,-4),true,true);
                    p.BezierTo(new Point(8,-16),new Point(18,-23),new Point(28,-25),true,false);
                    p.BezierTo(new Point(24,-19),new Point(14,-10),new Point(8,0),true,false);
                    p.BezierTo(new Point(5,2),new Point(3,0),new Point(4,-4),true,false);
                }nail.Freeze();d.DrawGeometry(Glow(tint.NailLight,tint.NailShade),P(tint.NailEdge,.4),nail);
                Curve(d,FingerPalette.Alpha(tint.Highlight,0xcf),.8,new Point(5,-5),new Point(10,-16),new Point(18,-21),new Point(25,-23));
                Curve(d,FingerPalette.Alpha(tint.Highlight,0x50),1.5,new Point(31,-27),new Point(59,-35),new Point(88,-37),new Point(116,-41));
                Curve(d,FingerPalette.Alpha(tint.Crease,0x77),1,new Point(46,-23),new Point(44,-14),new Point(45,-4),new Point(49,3));
                Curve(d,FingerPalette.Alpha(tint.Highlight,0x55),1,new Point(49,-22),new Point(47,-13),new Point(49,-4),new Point(52,1));
                Curve(d,FingerPalette.Alpha(tint.Crease,0x55),.8,new Point(111,-29),new Point(105,-19),new Point(105,-5),new Point(111,6));
                Curve(d,FingerPalette.Alpha(tint.Crease,0x44),.8,new Point(119,-28),new Point(114,-18),new Point(115,-7),new Point(118,-2));
                Curve(d,FingerPalette.Alpha(tint.Crease,0x32),.6,new Point(53,-17),new Point(52,-12),new Point(52,-7),new Point(54,-4));
                Curve(d,FingerPalette.Alpha(tint.Crease,0x28),.55,new Point(124,-23),new Point(121,-18),new Point(121,-11),new Point(123,-7));
                d.Pop();
            }
            var fade=new LinearGradientBrush { StartPoint=new Point(112,-15),EndPoint=new Point(170,55),MappingMode=BrushMappingMode.Absolute };
            fade.GradientStops.Add(new GradientStop(Colors.White,0));
            fade.GradientStops.Add(new GradientStop(Color.FromArgb(220,255,255,255),.25));
            fade.GradientStops.Add(new GradientStop(Color.FromArgb(100,255,255,255),.55));
            fade.GradientStops.Add(new GradientStop(Color.FromArgb(20,255,255,255),.8));
            fade.GradientStops.Add(new GradientStop(Colors.Transparent,1));fade.Freeze();
            finger.OpacityMask=fade;finger.Freeze();
            // Replace one cache entry rather than retain a large image per slider step.
            fingerImage=CacheArt(finger);return finger;
        }
        internal static void DrawFinger(DrawingContext d,World w) {
            if(w.FingerBlend<=0) return;
            double b=w.FingerBlend*bEase(w.FingerBlend);
            d.PushOpacity(w.FingerBlend*.96);d.PushTransform(new TranslateTransform(w.FingerOrigin.X,w.FingerOrigin.Y));
            d.PushTransform(new ScaleTransform(w.BodyScale*w.FingerDirection*(.35+.65*b),w.BodyScale*(.55+.45*b)));
            d.PushTransform(new TranslateTransform(-75,12));
            FingerArt(w.FingerTone);d.DrawImage(fingerImage.Image,fingerImage.Bounds);d.Pop();d.Pop();d.Pop();d.Pop();
        }
        static double bEase(double t) { return t*(3-2*t); }
        static string FurColor(string species) { var skin=SkinCatalog.Get(species);return skin.LegFur??skin.Hair; }
        static void Coat(DrawingContext d,string species,double rx,double ry,double cy,int seed,int count) {
            var random=new Random(seed);
            var skin=SkinCatalog.Get(species);string baseColor=cy>0?skin.Body:skin.Head,shade=cy>0?skin.BodyShade:skin.HeadShade;
            string hair=cy>0?skin.Hair:skin.HeadHair??skin.Hair;
            string[] colors={"#90"+shade.Substring(1),"#90"+baseColor.Substring(1),"#60"+hair.Substring(1),"#a0"+baseColor.Substring(1),"#60"+shade.Substring(1)};
            // Dense, low-contrast overlapping setae, with only occasional guard hairs.
            // The previous evenly spaced bright strokes made the coat look like wire.
            for(int i=0;i<count*4*skin.CoatDensity;i++) {
                double a=random.NextDouble()*Math.PI*2,r=Math.Sqrt(random.NextDouble())*.98;
                double x=Math.Cos(a)*rx*r,y=cy+Math.Sin(a)*ry*r,len=(.35+random.NextDouble()*1.25)*skin.HairLength*(cy<0 && skin.Shape=="Heavy"?.55:1);
                double dx=x/rx*.75+(random.NextDouble()-.5)*.65,dy=.4+(y-cy)/ry*.5;
                Line(d,colors[i%colors.Length],.15,x,y,x+dx*len,y+dy*len);
            }
            int fringe=(int)(240*skin.CoatDensity);
            for(int i=0;i<fringe;i++) {
                double a=i*Math.PI*2/fringe+(random.NextDouble()-.5)*.05;
                double x=Math.Cos(a)*rx*.96,y=cy+Math.Sin(a)*ry*.96,len=(.5+random.NextDouble()*2.3)*skin.HairLength*(cy<0 && skin.Shape=="Heavy"?.45:1);
                Line(d,i%13==0?hair:colors[i%colors.Length],.18,x,y,x+Math.Cos(a)*len,y+Math.Sin(a)*len);
            }
        }
        static void TuftSpot(DrawingContext d,double x,double y,double rx,double ry,string color,int seed) {
            Oval(d,B(color),x,y,rx,ry);
            var random=new Random(seed);
            for(int i=0;i<36;i++) {
                double a=i*Math.PI*2/36;
                double sx=x+Math.Cos(a)*rx*.9,sy=y+Math.Sin(a)*ry*.9;
                Line(d,color,.3,sx,sy,sx+Math.Cos(a)*(.5+random.NextDouble()*.6),sy+Math.Sin(a)*.7);
            }
        }
        static DrawingGroup Body(string species) {
            DrawingGroup group;if(bodies.TryGetValue(species,out group)) return group;
            var skin=SkinCatalog.Get(species);double rx=skin.BodyWidth,ry=skin.BodyLength;
            group=new DrawingGroup();using(var d=group.Open()) {
                Oval(d,B("#101012"),0,7,4,4);
                // Paint in a common marking space, then deform the whole abdomen.
                d.PushTransform(new TranslateTransform(0,22));
                d.PushTransform(new ScaleTransform(rx/14,ry/17));
                d.PushTransform(new TranslateTransform(0,-22));
                Geometry shape=BodyShape(skin);d.DrawGeometry(SurfaceShade(skin,skin.Body,skin.BodyShade),null,shape);
                BodyFringe(d,skin);d.PushClip(shape);Coat(d,species,14,17,22,21,450);
                if(skin.Pattern=="Zebra") {
                    for(int i=0;i<3;i++) for(int side=-1;side<=1;side+=2) Curve(d,skin.Mark,2.7,new Point(side*(11-i*.3),10+i*9),new Point(side*8,13+i*9),new Point(side*5,16+i*9),new Point(side*(.7+i*.35),14+i*9));
                } else if(skin.Pattern=="Tan") {
                    for(int i=0;i<4;i++) {
                        Curve(d,"#584f40",2.5,new Point(-11,9+i*7),new Point(-6,10+i*7),new Point(-4,16+i*7),new Point(0,16+i*7));
                        Curve(d,"#584f40",2.5,new Point(0,16+i*7),new Point(4,16+i*7),new Point(6,10+i*7),new Point(11,9+i*7));
                        Curve(d,skin.Mark,1,new Point(-10,7+i*7),new Point(-3,13+i*7),new Point(3,13+i*7),new Point(10,7+i*7));
                    }
                } else if(skin.Pattern=="Wall") {
                    for(int side=-1;side<=1;side+=2) Curve(d,"#383e36",4,new Point(side*9,5),new Point(side*12,15),new Point(side*10,33),new Point(0,40));
                    for(int i=0;i<4;i++) Curve(d,"#777d70",1.3,new Point(-6,11+i*6),new Point(-2,16+i*6),new Point(2,16+i*6),new Point(6,11+i*6));
                } else if(skin.Pattern=="Stripe") {
                    Curve(d,skin.Mark,5,new Point(0,4),new Point(-1,12),new Point(1,24),new Point(0,31));
                    TuftSpot(d,-5,34,2.2,1.7,skin.Mark,4);TuftSpot(d,5,34,2.2,1.7,skin.Mark,5);
                } else if(skin.Pattern=="Emerald") {
                    for(int side=-1;side<=1;side+=2) {
                        Curve(d,skin.Mark,1.8,new Point(side*10,7),new Point(side*16,18),new Point(side*13,28),new Point(side*7,36));
                        for(int i=0;i<4;i++) TuftSpot(d,side*(i==0?7:i==1?8:i==2?7:4.8),10+i*7.6,i==2?2.7:1.4,1.1,skin.Mark,7+i);
                        TuftSpot(d,side*12,22,1.1,1.5,"#b87d3c",17);
                    }
                } else if(skin.Pattern=="Spots" || skin.Pattern=="Regal") {
                    string color=skin.Mark;
                    if(skin.Pattern=="Regal") Curve(d,color,2.8,new Point(-11,9),new Point(-5,5),new Point(5,5),new Point(11,9));
                    TuftSpot(d,0,18,3.8,2.7,color,2);TuftSpot(d,0,20,2.1,2.2,color,22);
                    double rearHeight=skin.Pattern=="Spots"?.85:1.7;
                    TuftSpot(d,-6,31,3.0,rearHeight,color,3);TuftSpot(d,6,31,3.0,rearHeight,color,4);
                }
                ExtraMarkings(d,skin);d.Pop();
                Oval(d,B("#18181a"),-1.4,39,1,1.3);Oval(d,B("#18181a"),1.4,39,1,1.3);
                d.Pop();d.Pop();d.Pop();
            }group.Freeze();bodies[species]=group;return group;
        }
        protected override void OnRender(DrawingContext d) {
            base.OnRender(d);var w=World;if(w==null) return;
            d.PushTransform(new ScaleTransform(1/DeviceScale,1/DeviceScale));
            d.PushTransform(new TranslateTransform(-Offset.X,-Offset.Y));
            DrawHammock(d,w,false);
            if(!w.UnderNestSilk) DrawHammock(d,w,true);
            DrawGuestWeb(d,w);
            DrawFinger(d,w);
            if(w.HasFly) DrawPrey(d,w);
            if(w.SilkOpacity>0) {
                d.PushOpacity(w.SilkOpacity*w.WebVisibility);SilkLine(d,.75*w.BodyScale,w.SilkStart,w.SilkEnd);d.Pop();
            }
            DrawSpider(d,w,Species);
            if(w.UnderNestSilk) DrawHammock(d,w,true);d.Pop();d.Pop();
        }
        static void DrawFly(DrawingContext d,World w) {
            double s=w.Scale*w.Areas[w.AreaAt(w.Fly)].Dpi;
            d.PushTransform(new TranslateTransform(w.Fly.X,w.Fly.Y));d.PushTransform(new ScaleTransform(s,s));
            double flap=2+Math.Abs(Math.Sin(w.Clock*65))*3;
            Oval(d,B("#a4cce8ed"),-4,-3,4,flap);Oval(d,B("#a4cce8ed"),4,-3,4,flap);
            Oval(d,B("#30383b"),0,0,2.8,4);Oval(d,B("#9d6850"),0,-4,2.5,2.5);
            Line(d,"#434a45",.8,-1,2,-4,5);Line(d,"#434a45",.8,1,2,4,5);d.Pop();d.Pop();
        }
        public static void DrawSpider(DrawingContext d,World w,string species) {
            d.PushTransform(new TranslateTransform(w.Spider.X,w.Spider.Y));
            Oval(d,B("#16000000"),0,12*w.BodyScale,18*w.BodyScale,9*w.BodyScale);
            d.PushTransform(new TranslateTransform(0,-w.Lift));d.PushTransform(new ScaleTransform(w.BodyScale,w.BodyScale));
            d.PushTransform(new RotateTransform(w.Angle));
            // Feet stay in their own articulated frame while the body lowers to brace.
            for(int leg=3;leg>=0;leg--) for(int side=-1;side<=1;side+=2)
                if(leg!=0 || w.FrontReach(side)<.25) DrawLeg(d,w,species,side,leg);
            d.PushTransform(new TranslateTransform(0,(1-w.Squash)*19));
            double breathing=1+Math.Sin(w.Clock*1.5)*.003;
            d.PushTransform(new ScaleTransform(breathing,w.Squash));
            DrawArt(d,Body(species));
            DrawArt(d,Head(species));
            // Raised forelegs pass in front of the face, instead of vanishing under it.
            d.Pop();d.Pop();
            for(int side=-1;side<=1;side+=2) if(w.FrontReach(side)>=.25) DrawLeg(d,w,species,side,0);
            d.PushTransform(new TranslateTransform(0,(1-w.Squash)*19));
            d.PushTransform(new ScaleTransform(breathing,w.Squash));
            // The prosoma is one rigid part. No swivelling portrait face or eyelid blink.
            DrawMeal(d,w);DrawPalps(d,w,SkinCatalog.Get(species));
            d.Pop();d.Pop();d.Pop();d.Pop();d.Pop();d.Pop();
        }
        static void Segment(DrawingContext d,string species,V from,V to,double width,bool cuff=false) {
            var skin=SkinCatalog.Get(species);width*=skin.LegWidth;
            V delta=to-from;double length=delta.Length;if(length<.01) return;
            string key=species+width.ToString(System.Globalization.CultureInfo.InvariantCulture)+cuff;
            DrawingGroup g;if(!segments.TryGetValue(key,out g)) {
                g=new DrawingGroup();using(var c=g.Open()) {
                    var shape=new StreamGeometry();using(var s=shape.Open()) {
                        s.BeginFigure(new Point(-width*.24,0),true,true);
                        s.BezierTo(new Point(-width*.62,4),new Point(-width*.48,10),new Point(-width*.20,16),true,false);
                        s.LineTo(new Point(width*.18,16),true,false);
                        s.BezierTo(new Point(width*.44,10),new Point(width*.58,4),new Point(width*.23,0),true,false);
                    }shape.Freeze();
                    c.DrawGeometry(Glow(skin.Legs,skin.LegShade),null,shape);
                    var random=new Random(731+(int)(width*10));
                    for(int i=0;i<180*skin.CoatDensity;i++) {
                        double y=random.NextDouble()*16,radius=width*(.22+.24*Math.Sin(y/16*Math.PI));
                        double x=(random.NextDouble()*2-1)*radius;
                        bool pale=(cuff && y>9.5 && y<13.5) || ((skin.Pattern=="Zebra" || skin.Pattern=="Portia") && y>6 && y<10);
                        string color=pale?skin.Palps:i%7==0?FurColor(species):"#90"+skin.LegShade.Substring(1);
                        double hairLength=(.5+random.NextDouble()*(pale?2.1:1.2))*skin.LegHair;
                        Line(c,color,.16,x,y,x+Math.Sign(x)*hairLength*.65,y+hairLength);
                    }
                    for(int i=0;i<70*skin.CoatDensity;i++) {
                        double y=random.NextDouble()*16,sign=i%2==0?-1:1,radius=width*(.22+.24*Math.Sin(y/16*Math.PI));
                        bool pale=cuff && y>9 && y<13;
                        double len=(.4+random.NextDouble()*(pale?1.6:1.0))*skin.LegHair;
                        Line(c,pale?FurColor(species):i%8==0?"#7b6e63":"#70242424",.16,sign*radius,y,sign*(radius+len),y+len*.4);
                    }
                // Portia has isolated dark brushes, with fine exposed shafts between them.
                    if(skin.Pattern=="Portia" && cuff) for(int i=0;i<25;i++) {
                        double y=8.1+random.NextDouble()*2.4,side=i%2==0?-1:1;
                        double radius=width*.45,len=.8+random.NextDouble()*2.6;
                        Line(c,i%5==0?skin.Hair:skin.LegShade,.20,side*radius,y,side*(radius+len),y+len*.35);
                    }
                    if(skin.Pattern=="Emerald" && cuff) Curve(c,skin.LegShade,1,new Point(-width*.4,8),new Point(-width*.1,9),new Point(width*.2,9),new Point(width*.4,8));
                }g.Freeze();segments[key]=g;
            }
            d.PushTransform(new TranslateTransform(from.X,from.Y));
            d.PushTransform(new RotateTransform(Math.Atan2(delta.Y,delta.X)*180/Math.PI-90));
            d.PushTransform(new ScaleTransform(1,length/16));DrawArt(d,g);d.Pop();d.Pop();d.Pop();
        }
        static void DrawLeg(DrawingContext d,World w,string species,int side,int leg) {
            LegPose p=SpiderPose.Leg(w,side,leg,SkinCatalog.Get(species));
            V patella=p.Knee+(p.Ankle-p.Knee)*.18;
            bool front=leg==0;double bulk=front?SkinCatalog.Get(species).FrontBulk:1;
            Segment(d,species,p.Root,p.Hip,3);
            Segment(d,species,p.Hip,p.Knee,front?6.8*bulk:4.2,front);
            Segment(d,species,p.Knee,patella,front?5.8*bulk:3.6);
            Segment(d,species,patella,p.Ankle,front?5.1*bulk:3.0,front);
            Segment(d,species,p.Ankle,p.Toe,front?1.9:1.6);
            Segment(d,species,p.Toe,p.Tip,front?1.1:.9);
            // Small paired claws, rather than round cartoon shoes.
            Line(d,"#333e39",.4,p.Tip.X,p.Tip.Y,p.Tip.X+side*.7,p.Tip.Y+(leg<2?-.7:.7));
        }
        static void Eye(DrawingContext d,double x,double y,double radius) {
            Oval(d,B("#47382e"),x,y,radius+.22,radius*.72+.22);
            Oval(d,eye,x,y,radius,radius*.72);
            Oval(d,B("#aec0d5"),x-radius*.25,y-radius*.23,radius*.35,radius*.13);
            Oval(d,shine,x-radius*.31,y-radius*.28,radius*.14,radius*.09);
        }
        static DrawingGroup Head(string species) {
            DrawingGroup group;if(heads.TryGetValue(species,out group)) return group;
            var skin=SkinCatalog.Get(species);
            group=new DrawingGroup();using(var d=group.Open()) {
                d.PushTransform(new ScaleTransform(skin.HeadWidth,skin.HeadLength));
                var shape=new StreamGeometry();using(var p=shape.Open()) {
                    p.BeginFigure(new Point(-12,-23),true,true);
                    if(skin.Shape=="Tufted") { p.LineTo(new Point(-14,-15),true,false);p.LineTo(new Point(-12,-2),true,false);p.LineTo(new Point(-7,7),true,false); }
                    else p.BezierTo(new Point(-18,-20),new Point(-17,-3),new Point(-12,5),true,false);
                    p.BezierTo(new Point(-7*skin.HeadRear,11),new Point(7*skin.HeadRear,11),skin.Shape=="Tufted"?new Point(7,7):new Point(12,5),true,false);
                    if(skin.Shape=="Tufted") { p.LineTo(new Point(12,-2),true,false);p.LineTo(new Point(14,-15),true,false);p.LineTo(new Point(12,-23),true,false); }
                    else p.BezierTo(new Point(17,-3),new Point(18,-20),new Point(12,-23),true,false);
                    p.BezierTo(new Point(7,-25),new Point(-7,-25),new Point(-12,-23),true,false);
                }shape.Freeze();d.DrawGeometry(SurfaceShade(skin,skin.Head,skin.HeadShade),null,shape);
                if(skin.Shape=="Tufted") d.PushClip(shape);
                Coat(d,species,15,15,-7,91,460);
                if(skin.Shape=="Tufted") d.Pop();
                if(skin.Pattern=="Apache" || skin.Pattern=="ApacheStripe") Oval(d,B("#29221e"),0,-18,13,7);
                if(skin.Pattern=="Stripe") Line(d,skin.Mark,3.6,0,6,0,-22);
                if(skin.Pattern=="Wall" || skin.Pattern=="WallMale" || skin.Pattern=="Stripe" || skin.Pattern=="Zebra" || skin.Pattern=="Emerald")
                    for(int side=-1;side<=1;side+=2) Curve(d,skin.Mark,skin.Pattern=="Zebra"?2:1.4,new Point(side*10,4),new Point(side*16,-3),new Point(side*16,-14),new Point(side*10,-23));
                HeadDetails(d,skin);
                for(int side=-1;side<=1;side+=2) {
                    if(skin.Pattern=="Zebra" && skin.Male) {
                        var jaw=new StreamGeometry();using(var j=jaw.Open()) { j.BeginFigure(new Point(side*1.2,-23),true,true);j.QuadraticBezierTo(new Point(side*1.5,-35),new Point(side*3.5,-36.5),true,false);j.QuadraticBezierTo(new Point(side*6,-30),new Point(side*5,-23),true,false); }jaw.Freeze();d.DrawGeometry(Glow("#584337","#1e1816"),null,jaw);
                    } else Oval(d,Glow(skin.Mouth,skin.MouthShade),side*3.1,-23.2-2.5*skin.Chelicera,2.8,4.4*skin.Chelicera);
                    for(int i=0;i<17;i++) Line(d,i%3==0?"#80"+skin.Mouth.Substring(1):"#50"+skin.MouthShade.Substring(1),.16,side*3.1-1.6,-29+i*.39,side*3.1+1.5,-29.4+i*.39);
                    if(skin.Pattern=="WallMale") Line(d,skin.Mark,.8,side*.5,-25.5,side*4,-29.5);
                    if(skin.Pattern=="Tan" && skin.Male) for(int hair=0;hair<21;hair++) { double x=side*(1.5+hair*.18);Line(d,skin.Palps,.2,x,-25,x+side*.25,-28.5-(hair%4)*.3); }
                    // Two large anterior median eyes; three smaller pairs surround them.
                    Eye(d,side*4.4,-20.9,3.7);
                    Eye(d,side*11.3,-19.8,2.1);
                    Eye(d,side*13.3,-10.2,skin.MiddleEye);
                    Eye(d,side*12,-1.6,1.35);
                    for(int i=0;i<32;i++) {
                        double x=side*(1+i*.38),y=-17.1+Math.Abs(16-i)*.035;
                        Line(d,i%4==0?"#968c80":"#49413b",.15,x,y,x+side*.25,y+.8+(i%3)*.3);
                    }
                }
                d.Pop();
            }group.Freeze();heads[species]=group;return group;
        }
    }
}
