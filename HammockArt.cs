using System;
using System.Windows;
using System.Windows.Media;

namespace CornerSpider {
    public partial class PetCanvas {
        // The sleeping spider is painted between a silk floor and the loose roof.
        DrawingGroup nestBack,nestFront;
        SilkImage nestBackImage,nestFrontImage;
        double nestImageScale=-1;
        int nestLevel=-1,nestRepairs=-1;
        double nestEdgeX,nestEdgeY;
        string nestSpecies;
        internal int NestBuilds;

        static double RetreatRange(Random random,double a,double b) {
            return a+random.NextDouble()*(b-a);
        }

        static Point RetreatMix(Point a,Point b,double t) {
            return a+(b-a)*t;
        }

        static Point NestPoint(double x,double y) {
            const double sin=.7071067811865476;
            return new Point(-(x+y)*sin,(x-y)*sin);
        }

        static Point PocketPoint(SilkProfile profile,int side,double t) {
            double ruffle=Math.Sin(t*26+profile.Seed)*.84+Math.Sin(t*59+side)*.37;
            double fold=Math.Pow(Math.Max(0,(t-.68)/.32),1.7);
            double taper=fold*(side<0?3.1:6.5);
            double twist=fold*(1.6+1.3*Math.Sin(profile.Seed*.13));
            return new Point(profile.Edge(t,side)+(ruffle-taper)*side+twist,profile.Along(t));
        }

        internal static StreamGeometry Pocket(SilkProfile profile) {
            return Pocket(profile,0,1,0);
        }

        static StreamGeometry Pocket(SilkProfile profile,double start,double end,double lip) {
            var shape=new StreamGeometry();
            double nearSlant=lip>0?.032*Math.Sin(profile.Seed*.071):0;
            double farSlant=lip>0?.038*Math.Cos(profile.Seed*.041):0;
            double otherStart=Math.Max(0,start+farSlant),otherEnd=Math.Min(1,end+nearSlant);
            using(var p=shape.Open()) {
                Point first=PocketPoint(profile,-1,start);
                p.BeginFigure(first,true,true);
                for(int i=1;i<=48;i++) p.LineTo(PocketPoint(profile,-1,start+(end-start)*i/48),true,false);
                Point left=PocketPoint(profile,-1,end),right=PocketPoint(profile,1,otherEnd);
                p.BezierTo(new Point(left.X*.53,left.Y-lip),new Point(right.X*.40,right.Y-lip*.68),right,true,false);
                for(int i=47;i>=0;i--) p.LineTo(PocketPoint(profile,1,otherStart+(otherEnd-otherStart)*i/48),true,false);
                right=PocketPoint(profile,1,otherStart);
                p.BezierTo(new Point(right.X*.52,right.Y+lip*.72),new Point(first.X*.45,first.Y+lip),first,true,false);
            }
            shape.Freeze();
            return shape;
        }

        static void RetreatFiber(DrawingContext d,Point a,Point b,double slack,double width,double opacity,bool shadow=false) {
            Point c=RetreatMix(a,b,.32)+new Vector(slack*.38,slack);
            Point e=RetreatMix(a,b,.69)+new Vector(slack*.25,slack*.72);
            d.PushOpacity(opacity);
            if(shadow) Curve(d,"#65414b50",width+.52,a,c,e,b);
            Curve(d,"#ecf8f7ec",width,a,c,e,b);
            d.Pop();
        }

        static Point RetreatBranchPoint(Point a,Point b,double t,double slack) {
            double u=1-t;
            Point c=RetreatMix(a,b,.32)+new Vector(slack*.38,slack);
            Point e=RetreatMix(a,b,.69)+new Vector(slack*.25,slack*.72);
            return new Point(u*u*u*a.X+3*u*u*t*c.X+3*u*t*t*e.X+t*t*t*b.X,
                             u*u*u*a.Y+3*u*u*t*c.Y+3*u*t*t*e.Y+t*t*t*b.Y);
        }

        static void RetreatAttachment(DrawingContext d,Point point,bool top,double width) {
            // A matted attachment pad occupies the actual surface contact.
            // Independent tapered strands peel away from this spot.
            d.PushTransform(new TranslateTransform(point.X,point.Y));
            if(!top) d.PushTransform(new RotateTransform(90));
            RetreatFiber(d,new Point(-width*.82,0),new Point(width*.90,0),.30,.75,.58,true);
            RetreatFiber(d,new Point(-width*.58,.55),new Point(width*.61,1.1),.3,.42,.42);
            RetreatFiber(d,new Point(-width*.42,0),new Point(width*.29,2.4),-.8,.22,.38);
            if(!top) d.Pop();
            d.Pop();
        }

        static void RetreatFoundation(DrawingContext d,SilkProfile profile,int level,int repairs,
                                       double ex,double ey,bool resident,bool portiaVolume) {
            var random=new Random(profile.Seed+77);
            bool platformOnly=profile.Scaffold && !portiaVolume;
            int count=resident?5:platformOnly?5:9;
            int topCount=platformOnly?2:5;
            var roots=new Point[count];
            var branch=new Point[count];
            var seam=new Point[count];
            var slack=new double[count];
            for(int i=0;i<count;i++) {
                bool top=resident?ey<=ex:i<topCount;
                double along=resident?i/4.0:top?i/(double)(topCount-1):(i-topCount+1)/(double)(count-topCount);
                double wallOffset=RetreatRange(random,-11,10);
                if(resident) {
                    double pos=-95+along*188+wallOffset;
                    roots[i]=top?new Point(pos,-ey):new Point(ex,pos);
                } else if(top) {
                    double x=Math.Min(ex-10,-43+(ex+36)*along+wallOffset);
                    roots[i]=new Point(x,-ey);
                } else {
                    double y=-ey+(ey+42)*along+wallOffset;
                    roots[i]=new Point(ex,y);
                }
                double t=.91-.82*i/(count-1)+RetreatRange(random,-.025,.025);
                Point margin=PocketPoint(profile,-1,t);
                // Structural threads are attached along the same wall-side seam
                // as the veil, spreading the load over the length of the chamber.
                if(profile.Scaffold) {
                    margin.X=-20+3*Math.Sin(t*5.7);
                    margin.Y=profile.Along(t)*.72;
                }
                seam[i]=NestPoint(margin.X*profile.Width,margin.Y*profile.Length);
                branch[i]=RetreatMix(roots[i],seam[i],RetreatRange(random,.25,.69));
                branch[i].Offset(RetreatRange(random,-6,8),RetreatRange(random,-9,5));
                slack[i]=RetreatRange(random,-3.5,3.8);
                RetreatAttachment(d,roots[i],top,RetreatRange(random,10,17));
            }

            for(int i=0;i<count;i++) {
                // Short split bridles peel from each other before reaching a
                // larger trunk. The asymmetry comes from the actual substrate.
                if(!platformOnly && i==1) branch[i]=branch[0];
                if(!platformOnly && i==count-2) branch[i]=branch[count-1];
                bool light=(i%3==1);
                RetreatFiber(d,roots[i],branch[i],slack[i]*.5,light?.32:.50,light?.34:.54,!light);
                RetreatFiber(d,branch[i],seam[i],slack[i],light?.31:.52,light?.35:.51,!light);
            }

            for(int i=0;i<count-1;i++) {
                random=new Random(profile.Seed+713+i*157);
                if(!platformOnly && i%3!=1) {
                    Point outer=RetreatMix(roots[i],branch[i],.58);
                    Point inner=RetreatBranchPoint(branch[i+1],seam[i+1],.21,slack[i+1]);
                    RetreatFiber(d,outer,inner,RetreatRange(random,1.4,7.2),.29,.28);
                }
                Point join=RetreatBranchPoint(branch[i],seam[i],.48,slack[i]);
                if(!platformOnly && i%3==0) RetreatFiber(d,branch[i+1],join,RetreatRange(random,-2,4),.24,.25);
                // Split the short inner spans into shallow stitching where the
                // platform or sleeve takes the tension from the outer frame.
                int threads=(platformOnly?1:2+i%3)+level/(platformOnly?18:7)+repairs/3;
                for(int j=0;j<threads;j++) {
                    double u=RetreatRange(random,.25,.95);
                    double v=Math.Max(0,Math.Min(1,u+RetreatRange(random,-.16,.17)));
                    Point a=RetreatBranchPoint(branch[i],seam[i],u,slack[i]);
                    Point b=RetreatBranchPoint(branch[i+1],seam[i+1],v,slack[i+1]);
                    RetreatFiber(d,a,b,RetreatRange(random,-2,3),j%5==0?.33:.20,.20+j%3*.061);
                }
                // Keep each existing repair in place when the next one appears.
                for(int j=0;j<repairs/2;j++) {
                    double t=.16+.70*((i*7+j*13)%23)/22;
                    Point tie=RetreatBranchPoint(branch[i],seam[i],t,slack[i]);
                    Point other=RetreatBranchPoint(branch[i+1],seam[i+1],Math.Min(1,t+.08),slack[i+1]);
                    RetreatFiber(d,tie,other,1.4,.32,.29);
                }
            }
            if(portiaVolume) RetreatSpaceWeb(d,profile,roots,branch,seam,level,repairs);
        }

        static void RetreatSpaceWeb(DrawingContext d,SilkProfile profile,Point[] roots,
                                    Point[] branch,Point[] seam,int level,int repairs) {
            // The female Portia platform sits near the top of a loose,
            // three-sheet space web. The projected sheets converge into
            // a flexible lower mesh instead of a closed sleeve.
            var random=new Random(profile.Seed+341);
            int last=roots.Length-1;
            Point low=RetreatMix(branch[2],seam[last],.82)+new Vector(-12,67);
            Point high=branch[last-1];
            Point[] upper={roots[0],branch[2],high,roots[last]};
            Point[] lower={seam[1],low,low+new Vector(24,-6),seam[last-1]};
            RetreatFiber(d,low,roots[0],13,.36,.37,true);
            RetreatFiber(d,lower[2],roots[last],9,.33,.32);
            for(int face=0;face<3;face++) {
                random=new Random(profile.Seed+341+face*127);
                int ribs=7+level/6+repairs/5;
                Point previousStart=new Point(),previousEnd=new Point();
                for(int i=0;i<ribs;i++) {
                    double a=.06+.88*(i%7)/6+RetreatRange(random,-.026,.026);
                    double b=Math.Max(0,Math.Min(1,a+RetreatRange(random,-.19,.16)));
                    Point start=RetreatBranchPoint(upper[face],upper[face+1],a,5);
                    Point end=RetreatBranchPoint(lower[face],lower[face+1],b,4);
                    RetreatFiber(d,start,end,7.4,i%4==0?.40:.22,.26+i%3*.067,i%5==0);
                    if(i>0 && i%7!=0) {
                        for(int j=0;j<2+i%3;j++) {
                            double t=RetreatRange(random,.15,.86);
                            Point left=RetreatBranchPoint(previousStart,previousEnd,t,7.4);
                            Point right=RetreatBranchPoint(start,end,Math.Min(.96,t+.13),7.4);
                            RetreatFiber(d,left,right,RetreatRange(random,.3,4.3),.21,.25);
                        }
                    }
                    previousStart=start;previousEnd=end;
                }
                RetreatFiber(d,upper[face],upper[face+1],5,.43,.43,true);
                RetreatFiber(d,lower[face],lower[face+1],4,.31,.37);
            }
        }

        static void RetreatWeave(DrawingContext d,SilkProfile profile,int level,int repairs,bool roof) {
            var random=new Random(profile.Seed+(roof?1979:916));
            double density=profile.Density;
            int count=(int)((roof?31:48)*density)+level+repairs*2;
            for(int i=0;i<count;i++) {
                double t=RetreatRange(random,0,1);
                double x=RetreatRange(random,-36,36);
                double endX=x*.48+RetreatRange(random,-22,22);
                Point start=new Point(x,profile.Along(t*.50));
                Point end=new Point(endX,profile.Along(.51+RetreatRange(random,0,.49)));
                if(i%6==0) {
                    start=PocketPoint(profile,-1,t);
                    end=PocketPoint(profile,1,Math.Max(0,Math.Min(1,t+RetreatRange(random,-.29,.31))));
                }
                RetreatFiber(d,start,end,RetreatRange(random,-5,6),i%9==0?.46:.23,
                    RetreatRange(random,.17,.49),i%13==0);
            }
        }

        static void RetreatMouth(DrawingContext d,SilkProfile profile,bool far) {
            // The two sleeve lips have different heights and fray lengths.
            double variation=.021*Math.Sin(profile.Seed*(far?.113:.181));
            double t=(far?.109:.916)+variation;
            Point a=PocketPoint(profile,-1,t),b=PocketPoint(profile,1,t);
            double width=(b.X-a.X)*.5;
            Oval(d,SoftGlow("#46404747"),(a.X+b.X)*.5+width*.13,a.Y+1,width*(far?.62:.70),far?4.0:5.2);
            var random=new Random(profile.Seed+(far?121:147));
            for(int i=0;i<5;i++) {
                bool foldedSide=i%2==0;
                Point start=RetreatMix(a,b,foldedSide?RetreatRange(random,0,.24):RetreatRange(random,.66,.87));
                Point end=RetreatMix(a,b,foldedSide?RetreatRange(random,.35,.72):RetreatRange(random,.89,1));
                start.Y+=RetreatRange(random,-1,2);
                end.Y+=RetreatRange(random,-1.5,1.8);
                RetreatFiber(d,start,end,(far?1:-1)*RetreatRange(random,1,5),i==0?.44:.23,
                             i==0?.34:.19,i==0);
            }
        }

        static void FeatherRetreat(DrawingContext d,SilkProfile profile) {
            var random=new Random(profile.Seed+2371);
            for(int i=0;i<24;i++) {
                int side=i<16?-1:1;
                double t=RetreatRange(random,.10,.84),end=Math.Min(.97,t+RetreatRange(random,.08,.24));
                Point a=PocketPoint(profile,side,t),b=PocketPoint(profile,side,end);
                a.X-=side*RetreatRange(random,.4,2.9);
                b.X-=side*RetreatRange(random,.1,2.8);
                double drift=RetreatRange(random,1.7,5.4)*side;
                Point c=RetreatMix(a,b,.24)+new Vector(drift,-1.4);
                Point e=RetreatMix(a,b,.69)+new Vector(drift*.83,1.8);
                d.PushOpacity(RetreatRange(random,.12,.39));
                if(i%8==0) Curve(d,"#5639464b",.62,a,c,e,b);
                Curve(d,"#bdf3f5e9",i%6==0?.49:.24,a,c,e,b);
                d.Pop();
            }
        }

        static void DrawPortiaPlatform(DrawingContext d,SilkProfile profile,int level,int repairs) {
            var random=new Random(profile.Seed+127);
            Point[] rim={new Point(-24,-28),new Point(18,-23),new Point(25,27),new Point(-22,35)};
            var sheet=new StreamGeometry();
            using(var path=sheet.Open()) {
                path.BeginFigure(rim[0],true,true);
                for(int i=0;i<rim.Length;i++) {
                    Point a=rim[i],b=rim[(i+1)%rim.Length];
                    path.QuadraticBezierTo(RetreatMix(RetreatMix(a,b,.5),new Point(0,3),.18),b,true,false);
                }
            }
            sheet.Freeze();
            d.DrawGeometry(B("#1638494a"),null,sheet);
            d.DrawGeometry(B("#1deff4e8"),null,sheet);
            d.PushClip(sheet);
            for(int i=0;i<42+level+repairs*2;i++) {
                double t=RetreatRange(random,0,1),u=RetreatRange(random,0,1);
                Point a=RetreatMix(rim[i%4],rim[(i+1)%4],t);
                Point b=RetreatMix(rim[(i+2)%4],rim[(i+3)%4],u);
                RetreatFiber(d,a,b,RetreatRange(random,-4,5),i%7==0?.43:.23,RetreatRange(random,.24,.49),i%6==0);
            }
            d.Pop();
        }

        void BuildHammock(int level,int repairs,double edgeX,double edgeY,string species,bool resident) {
            nestLevel=level;nestRepairs=repairs;nestEdgeX=edgeX;nestEdgeY=edgeY;nestSpecies=species;NestBuilds++;
            SilkProfile profile=SilkProfile.Get(species);
            bool platform=profile.Scaffold;
            nestBack=new DrawingGroup();nestFront=new DrawingGroup();
            using(var d=nestBack.Open()) {
                RetreatFoundation(d,profile,level,repairs,edgeX,edgeY,resident,platform && !SkinCatalog.Get(species).Male);
                d.PushTransform(new RotateTransform(135));d.PushTransform(new ScaleTransform(profile.Width,profile.Length));
                if(platform) DrawPortiaPlatform(d,profile,level,repairs);
                else {
                    var shape=Pocket(profile,.018,.985,3.7);
                    d.DrawGeometry(B("#0becf1e4"),null,shape);
                    d.PushClip(shape);
                    Oval(d,SoftGlow("#53e6ece1"),profile.Edge(.49,-1)+5,3,14,39);
                    Oval(d,SoftGlow("#2ceeedeb"),profile.Edge(.37,1)-4,-4,12,32);
                    RetreatWeave(d,profile,level,repairs,false);
                    d.Pop();
                    RetreatMouth(d,profile,true);
                    RetreatMouth(d,profile,false);
                }
                d.Pop();d.Pop();
            }
            nestBack.Freeze();
            using(var d=nestFront.Open()) {
                if(!platform) {
                    d.PushTransform(new RotateTransform(135));d.PushTransform(new ScaleTransform(profile.Width,profile.Length));
                    var roof=Pocket(profile,.058,.958,13.2);
                    d.PushOpacity(Math.Min(1,profile.Density));
                    d.DrawGeometry(B("#08f2f2e4"),null,roof);
                    d.PushClip(roof);
                    // A roof of overlapping silk rather than an outlined pouch.
                    double offset=7*Math.Sin(profile.Seed*.17);
                    Oval(d,SoftGlow("#68eef2e8"),profile.Edge(.40,-1)+6,-11+offset,15,29);
                    Oval(d,SoftGlow("#4aeeefe6"),profile.Edge(.68,-1)+3,22+offset,11,19);
                    Oval(d,SoftGlow("#34e9eee4"),profile.Edge(.66,1)-6,9-offset,11,25);
                    Oval(d,SoftGlow("#24f5f4e7"),-4,-18-offset,17,22);
                    RetreatWeave(d,profile,level,repairs,true);
                    d.Pop();
                    FeatherRetreat(d,profile);
                    d.Pop();d.Pop();d.Pop();
                }
            }
            nestFront.Freeze();
            nestBackImage=new SilkImage();nestFrontImage=new SilkImage();nestImageScale=-1;
        }

        public void DrawNestScene(DrawingContext d,World w,string species) {
            DrawHammock(d,w,false,species);
            if(!w.UnderNestSilk) DrawHammock(d,w,true,species);
            if(w.IsColonyResident) DrawResidentBody(d,w,species);else DrawSpider(d,w,species);
            if(w.UnderNestSilk) DrawHammock(d,w,true,species);
            DrawLifeScene(d,w);
        }

        public void DrawHammock(DrawingContext d,World w,bool front,string species=null,bool vectorPreview=false) {
            if(w.WebVisibility<=0) return;
            species=species??Species;
            if(!front) DrawLivingSilk(d,w,species);
            if(SkinCatalog.Get(species).WebKind!=WebKind.Retreat) return;
            var area=w.Areas[w.HomeArea];double scale=w.HomeScale;
            double edgeX=Math.Abs((w.Corner.EndsWith("Right")?area.X+area.W:area.X)-w.Home.X)/scale;
            double edgeY=Math.Abs((w.Corner.StartsWith("Top")?area.Y:area.Y+area.H)-w.Home.Y)/scale;
            int level=(int)(Math.Max(0,Math.Min(1,w.Web))*30);
            if(nestBack==null || nestSpecies!=species || nestLevel!=level || nestRepairs!=w.WebLayers ||
               Math.Abs(nestEdgeX-edgeX)>.01 || Math.Abs(nestEdgeY-edgeY)>.01)
                BuildHammock(level,w.WebLayers,edgeX,edgeY,species,w.IsColonyResident);
            d.PushOpacity(w.WebVisibility);
            d.PushTransform(new TranslateTransform(w.Home.X,w.Home.Y));
            d.PushTransform(new ScaleTransform(scale*(w.Corner.EndsWith("Right")?1:-1),scale*(w.Corner.StartsWith("Top")?1:-1)));
            if(vectorPreview) d.DrawDrawing(front?nestFront:nestBack);
            else {
                if(nestImageScale!=scale) { nestImageScale=scale;nestBackImage=new SilkImage();nestFrontImage=new SilkImage(); }
                if(front && nestFrontImage.Drawing==null) nestFrontImage=CacheSilk(nestFront,scale);
                if(!front && nestBackImage.Drawing==null) nestBackImage=CacheSilk(nestBack,scale);
                d.DrawDrawing((front?nestFrontImage:nestBackImage).Drawing);
            }
            d.Pop();d.Pop();d.Pop();
        }
    }
}
