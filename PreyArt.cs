using System;
using System.Windows;
using System.Windows.Media;

namespace CornerSpider {
    public partial class PetCanvas {
        static readonly double[] preyRoots={-2,-.5,1,2.5},preyKnees={-7,-2,4,9},preyTips={-12,-4,7,14};
        static DrawingGroup preyBody;
        static readonly Geometry mothLeft=FrozenWing("M 0,-5 C -8,-16 -20,-8 -17,4 L -7,12 0,6 Z"),mothRight=FrozenWing("M 0,-5 C 8,-16 20,-8 17,4 L 7,12 0,6 Z");
        static Geometry FrozenWing(string path) { var wing=Geometry.Parse(path);wing.Freeze();return wing; }
        static void DrawSmallSpider(DrawingContext d,double steps,bool walking,double folded) {
            for(int side=-1;side<=1;side+=2) for(int leg=0;leg<4;leg++) {
                double stride=walking?Math.Sin(steps+(leg%2)*Math.PI+(side<0?0:Math.PI))*1.8:0;
                V root=new V(side*1.8,preyRoots[leg]);
                V knee=new V(side*(leg==0 || leg==3?7:9)*(1-folded*.38),preyKnees[leg]*(1-folded*.42));
                V foot=new V(side*(leg==0?11:leg==1?13:leg==2?12:9)*(1-folded*.7),(preyTips[leg]+stride)*(1-folded*.55));
                for(int layer=0;layer<2;layer++) {
                    string color=layer==0?"#493b30":"#ceb38c";double width=layer==0?1.3:.55;
                    Line(d,color,width,root.X,root.Y,knee.X,knee.Y);Line(d,color,width,knee.X,knee.Y,foot.X,foot.Y);
                }
            }
            DrawArt(d,SmallSpiderBody());
        }
        static DrawingGroup SmallSpiderBody() {
            if(preyBody!=null) return preyBody;
            preyBody=new DrawingGroup();using(var d=preyBody.Open()) {
            Oval(d,Glow("#c0a27c","#64503d"),0,4.3,3.4,4.9);
            for(int i=0;i<3;i++) Curve(d,"#726048",.65,new Point(-2,2+i*2),new Point(-1,3+i*2),new Point(1,3+i*2),new Point(2,2+i*2));
            Oval(d,Glow("#b49a74","#514332"),0,-2.4,2.7,2.9);
            for(int side=-1;side<=1;side+=2) {
                Oval(d,B("#202120"),side*.8,-4.5,.4,.4);Oval(d,B("#32312a"),side*1.8,-3.8,.3,.3);
                Line(d,"#bca884",.7,side*1.3,-4.8,side*1.7,-6.1);
            }
            }preyBody.Freeze();return preyBody;
        }
        internal static void DrawPrey(DrawingContext d,World w) {
            if(!w.HasFly) return;
            if(w.Prey==PreyKind.Fly) { DrawFly(d,w);return; }
            if(w.Prey!=PreyKind.SmallSpider) {
                double foodScale=w.Scale*w.Areas[w.AreaAt(w.Fly)].Dpi;
                d.PushTransform(new TranslateTransform(w.Fly.X,w.Fly.Y));d.PushTransform(new ScaleTransform(foodScale,foodScale));d.PushTransform(new RotateTransform(w.PreyAngle));DrawFood(d,w.Prey,w.Clock,w.PreySteps);d.Pop();d.Pop();d.Pop();return;
            }
            double scale=World.SmallSpiderSize*w.Scale*w.Areas[w.AreaAt(w.Fly)].Dpi;
            d.PushTransform(new TranslateTransform(w.Fly.X,w.Fly.Y));d.PushTransform(new ScaleTransform(scale,scale));
            d.PushTransform(new RotateTransform(w.PreyAngle));DrawSmallSpider(d,w.PreySteps,w.PreyWalking,0);d.Pop();d.Pop();d.Pop();
        }
        static void DrawMeal(DrawingContext d,World w) {
            if(!w.HoldingMeal) return;
            if(w.Meal!=PreyKind.Fly && w.Meal!=PreyKind.SmallSpider) { d.PushTransform(new TranslateTransform(0,-30));d.PushTransform(new ScaleTransform(.65,.65));DrawFood(d,w.Meal,w.StateTime,0);d.Pop();d.Pop();return; }
            if(w.Meal==PreyKind.Fly) { Oval(d,B("#393f32"),0,-29,2.6,2);Line(d,"#6e877a",.5,-2,-30,-5,-31);return; }
            double effort=w.CaptureStruggle;
            double fade=w.State==Mood.Eat?Math.Max(0,Math.Min(1,(w.MealDuration-w.StateTime)/.8)):1;
            d.PushOpacity(fade);d.PushTransform(new TranslateTransform(Math.Sin(w.StateTime*19)*1.5*effort,-30.2));
            d.PushTransform(new RotateTransform(90+Math.Sin(w.StateTime*19)*12*effort));d.PushTransform(new ScaleTransform(.68*World.SmallSpiderSize,.68*World.SmallSpiderSize));
            DrawSmallSpider(d,w.StateTime*24,effort>.01,.85-.5*effort);d.Pop();d.Pop();d.Pop();d.Pop();
        }
        static void DrawFood(DrawingContext d,PreyKind kind,double time,double steps) {
            if(kind==PreyKind.Mealworm) {
                for(int i=0;i<9;i++) { double x=Math.Sin(steps-i*.4)*1.6;Oval(d,Glow("#dcc08d","#957049"),x,(i-4)*2.6,3.2,2.2);Line(d,"#6d5036",.35,x-2.5,(i-4)*2.6,x+2.5,(i-4)*2.6); }
                Oval(d,B("#655035"),0,-12,2.3,2);return;
            }
            if(kind==PreyKind.Gnat) { double wing=2+Math.Abs(Math.Sin(time*60));Oval(d,B("#90d7e2dd"),-3,0,3,wing);Oval(d,B("#90d7e2dd"),3,0,3,wing);Oval(d,B("#414038"),0,1,1.3,3.2);return; }
            if(kind==PreyKind.Moth) {
                double flap=.75+Math.Abs(Math.Sin(time*15))*.25;
                for(int side=-1;side<=1;side+=2) { d.PushTransform(new ScaleTransform(flap,1));d.DrawGeometry(Glow("#cbb789","#776343"),null,side<0?mothLeft:mothRight);Oval(d,B("#64563b"),side*10,1,2,3);d.Pop(); }
                Oval(d,B("#7c7052"),0,0,3,10);return;
            }
            string color=kind==PreyKind.Beetle?"#686f42":"#9c8054";
            for(int side=-1;side<=1;side+=2) for(int leg=0;leg<3;leg++) {
                double y=-4+leg*5,x=side*(kind==PreyKind.Cricket && leg==2?13:8),step=Math.Sin(steps+leg)*2;
                Line(d,"#544b32",1,side*3,y,x,y-5);Line(d,"#544b32",.7,x,y-5,side*11,y+6+step);
            }
            Oval(d,Glow(color,"#424632"),0,2,kind==PreyKind.Beetle?6:4,9);Oval(d,B("#665638"),0,-8,3.5,3);
            if(kind==PreyKind.Beetle) Line(d,"#b2b582",.7,0,-5,0,10);
            for(int side=-1;side<=1;side+=2) Curve(d,"#a5966c",.5,new Point(side*2,-9),new Point(side*4,-15),new Point(side*9,-15),new Point(side*13,-17));
        }
    }
}
