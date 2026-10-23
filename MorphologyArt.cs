using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CornerSpider {
    public partial class PetCanvas {
        static readonly Dictionary<string,DrawingGroup> palpTips=new Dictionary<string,DrawingGroup>();
        static Point BodyEdge(SkinProfile skin,double a,double outset) {
            double irregular=skin.Shape=="Tufted"?1+.045*Math.Cos(a*6):1;
            return new Point(Math.Cos(a)*(14*(1+skin.BodyTaper*Math.Sin(a))*irregular+outset),22+Math.Sin(a)*(17+outset));
        }
        static Geometry BodyShape(SkinProfile skin) {
            var shape=new StreamGeometry();using(var g=shape.Open()) {
                g.BeginFigure(BodyEdge(skin,0,0),true,true);
                for(int i=1;i<128;i++) g.LineTo(BodyEdge(skin,i*Math.PI*2/128,0),true,false);
            }shape.Freeze();return shape;
        }
        static void BodyFringe(DrawingContext d,SkinProfile skin) {
            var random=new Random(174);for(int i=0;i<220;i++) {
                double a=i*Math.PI*2/220,len=.4+random.NextDouble()*1.2;
                if(skin.Shape=="Tufted") len*=1+2.2*Math.Pow(Math.Max(0,Math.Cos(a*7)),8);
                Point start=BodyEdge(skin,a,-.3),end=BodyEdge(skin,a,len);
                d.DrawLine(P(i%9==0?skin.Hair:"#90"+skin.Body.Substring(1),.17),start,end);
            }
        }
        static void ExtraMarkings(DrawingContext d,SkinProfile skin) {
            if(skin.Pattern=="RedStripe" || skin.Pattern=="ApacheStripe") {
                Curve(d,"#24221f",6,new Point(0,6),new Point(-1,18),new Point(1,27),new Point(0,38));
            } else if(skin.Pattern=="WallMale") {
                for(int side=-1;side<=1;side+=2) Curve(d,skin.Mark,3.6,new Point(side*10,6),new Point(side*15,16),new Point(side*11,31),new Point(side*3,38));
            } else if(skin.Pattern=="StripeFemale") {
                Curve(d,"#c3b18a",6,new Point(0,5),new Point(-1,17),new Point(1,26),new Point(0,36));
                for(int i=0;i<3;i++) Curve(d,"#77684f",1.5,new Point(-6,12+i*6),new Point(-3,16+i*6),new Point(3,16+i*6),new Point(6,12+i*6));
                for(int side=-1;side<=1;side+=2) { Line(d,"#3c3328",3,side*7,25,side*6,37);TuftSpot(d,side*7,29,1.4,1,skin.Mark,71);TuftSpot(d,side*6,35,1.5,1.1,skin.Mark,72); }
            } else if(skin.Pattern=="Hyllus") {
                for(int i=0;i<4;i++) Curve(d,"#465b30",1.8,new Point(-9,12+i*6),new Point(-4,16+i*6),new Point(4,16+i*6),new Point(9,12+i*6));
                Curve(d,skin.Mark,1.4,new Point(-10,9),new Point(-2,5),new Point(2,5),new Point(10,9));
            } else if(skin.Pattern=="Portia") {
                var random=new Random(95);for(int i=0;i<85;i++) {
                    double x=random.NextDouble()*24-12,y=6+random.NextDouble()*32;
                    Curve(d,i%3==0?"#aa"+skin.Mark.Substring(1):"#c0"+skin.BodyShade.Substring(1),.5+random.NextDouble()*1.3,
                        new Point(x,y),new Point(x+2,y+.3),new Point(x-1,y+2),new Point(x+random.NextDouble()*2,y+3));
                }
                Line(d,skin.Mark,2,0,7,0,14);
                for(int side=-1;side<=1;side+=2) for(int i=0;i<3;i++) TuftSpot(d,side*(8-i),18+i*7,1.1,1.6,skin.Hair,i+9);
            }
        }
        static void HeadDetails(DrawingContext d,SkinProfile skin) {
            if(skin.Pattern=="Tan" && skin.Male) Line(d,"#c38a50",2.1,-12,-23.7,12,-23.7);
            if(skin.Pattern=="WallMale") Line(d,skin.Mark,3,0,6,0,-19);
            if(skin.Pattern=="StripeFemale") { Line(d,"#b99868",2,0,5,0,-13);Oval(d,B("#493e30"),0,-17,12,6); }
            if(skin.Pattern=="Portia") {
                for(int side=-1;side<=1;side+=2) {
                    Curve(d,skin.BodyShade,1,new Point(0,-5),new Point(side*8,-9),new Point(side*10,-10),new Point(side*13,-11));
                    for(int i=0;i<38;i++) Line(d,skin.Palps,.22,side*(i*.30),-24.6,side*(i*.32),-26.2-(i%5)*.2);
                }
            }
            if(skin.Pattern=="Hyllus") for(int side=-1;side<=1;side+=2) for(int i=0;i<25;i++)
                Line(d,i%4==0?skin.Hair:skin.HeadShade,.19,side*(12+i*.04),-8+(i%5)*.22,side*(15+i*.08),-12-(i%7)*.4);
        }
        static DrawingGroup PalpTip(SkinProfile skin) {
            DrawingGroup g;if(palpTips.TryGetValue(skin.Id,out g)) return g;
            double rx=skin.PalpWidth,ry=(skin.Male?2.6:2.1)*skin.HeadLength;
            g=new DrawingGroup();using(var d=g.Open()) {
                // Fine tapered female tarsi; adult male tips have a rounded bulb.
                Oval(d,Glow(skin.Palps,skin.LegShade),0,-.5,rx,ry);
                if(skin.Male) Oval(d,B("#80"+skin.LegShade.Substring(1)),rx*.35,.25,rx*.48,ry*.65);
                var random=new Random(612);for(int i=0;i<90;i++) {
                    double a=random.NextDouble()*Math.PI*2,r=Math.Sqrt(random.NextDouble()),x=Math.Cos(a)*rx*r,y=-.5+Math.Sin(a)*ry*r;
                    double len=.25+random.NextDouble()*.65;
                    Line(d,i%5==0?skin.Hair:skin.Palps,.13,x,y,x+Math.Cos(a)*len,y+Math.Sin(a)*len);
                }
            }g.Freeze();palpTips[skin.Id]=g;return g;
        }
        static void DrawPalps(DrawingContext d,World w,SkinProfile skin) {
            for(int side=-1;side<=1;side+=2) {
                PalpJoints p=PalpPose.At(w,skin,side);
                Segment(d,skin.Id,p.Root,p.Elbow,1.9);Segment(d,skin.Id,p.Elbow,p.Tip,1.5);
                Line(d,"#95"+skin.Palps.Substring(1),.7,p.Elbow.X,p.Elbow.Y,p.Tip.X,p.Tip.Y);
                V delta=p.Tip-p.Elbow;d.PushTransform(new TranslateTransform(p.Tip.X,p.Tip.Y));
                d.PushTransform(new RotateTransform(Math.Atan2(delta.Y,delta.X)*180/Math.PI-90));
                DrawArt(d,PalpTip(skin));d.Pop();d.Pop();
            }
        }
    }
}
