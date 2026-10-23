using System;
using System.Windows;
using System.Windows.Media;

namespace CornerSpider {
    public partial class PetCanvas {
        internal static void DrawGuestWeb(DrawingContext d,World w) {
            if(w.GuestOpacity<=0) return;
            d.PushOpacity(w.GuestOpacity*.82);
            double built=w.GuestProgress*40;
            for(int i=0;i<40 && i<built;i++) {
                V a=w.GuestThreadStart(i),b=w.GuestThreadEnd(i);
                double fraction=Math.Min(1,Math.Max(0,(built-i-World.GuestTravelFraction)/(1-World.GuestTravelFraction)));
                if(fraction<=0) continue;b=a+(b-a)*fraction;
                V mid=(a+b)*.5;
                double vibrate=w.GuestPulse*Math.Sin(i*2.4+w.GuestStageAge*32)*1.7*w.GuestScale;
                double slack=i<8?1:4+Math.Sin(i*1.7)*2;
                V sag=new V(w.GuestInward.X*(slack+vibrate),w.GuestInward.Y*(slack+1+vibrate));
                d.PushOpacity(i<8?.78:.50);
                SilkCurve(d,(i<8?.58:.30)*w.GuestScale,new Point(a.X,a.Y),new Point(mid.X+sag.X,mid.Y+sag.Y),new Point(mid.X+sag.X,mid.Y+sag.Y),new Point(b.X,b.Y));d.Pop();
            }
            if(w.State==Mood.WebPluck) {
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
