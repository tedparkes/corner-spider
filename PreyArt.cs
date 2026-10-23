using System;
using System.Windows;
using System.Windows.Media;

namespace CornerSpider {
    public partial class PetCanvas {
        static readonly double[] preyRoots={-2,-.5,1,2.5},preyKnees={-7,-2,4,9},preyTips={-12,-4,7,14};
        static DrawingGroup preyBody;
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
            double scale=w.Scale*w.Areas[w.AreaAt(w.Fly)].Dpi;
            d.PushTransform(new TranslateTransform(w.Fly.X,w.Fly.Y));d.PushTransform(new ScaleTransform(scale,scale));
            d.PushTransform(new RotateTransform(w.PreyAngle));DrawSmallSpider(d,w.PreySteps,w.PreyWalking,0);d.Pop();d.Pop();d.Pop();
        }
        static void DrawMeal(DrawingContext d,World w) {
            if(w.State!=Mood.Eat) return;
            if(w.Meal==PreyKind.Fly) { Oval(d,B("#393f32"),0,-29,2.6,2);Line(d,"#6e877a",.5,-2,-30,-5,-31);return; }
            double fade=Math.Max(0,Math.Min(1,(w.MealDuration-w.StateTime)/.8));
            d.PushOpacity(fade);d.PushTransform(new TranslateTransform(0,-30.2));
            d.PushTransform(new RotateTransform(90+Math.Sin(w.StateTime*6)*3));d.PushTransform(new ScaleTransform(.68,.68));
            DrawSmallSpider(d,0,false,.85);d.Pop();d.Pop();d.Pop();d.Pop();
        }
    }
}
