using System;

namespace CornerSpider {
    public struct PalpJoints { public V Root,Elbow,Tip;public double Upper,Lower; }
    public static class PalpPose {
        static double Smooth(double t) { t=Math.Max(0,Math.Min(1,t));return t*t*(3-2*t); }
        static V Blend(V a,V b,double t) { return a+(b-a)*Smooth(t); }
        public static PalpJoints At(World w,SkinProfile skin,int side) {
            // Unlike the walking forelegs these appendages may sweep sideways.
            // Alternate eye wipes and chelicera strokes, returning to rest at 3.5s.
            V rest=new V(side*7,-30.1),target=rest;
            if(w.State==Mood.Groom) {
                double t=w.StateTime-(side<0?.08:1.72);
                V eye=new V(side*4.4,-20.9),chels=new V(side*3.6,-26.8);
                if(t>=0 && t<1.6) {
                    if(t<.3) target=Blend(rest,new V(side*11.6,-25),t/.3);
                    else if(t<.57) target=Blend(new V(side*11.6,-25),eye,(t-.3)/.27);
                    else if(t<.83) target=eye+new V(side*1.1*Math.Sin((t-.57)/.26*Math.PI*2),.6*Math.Sin((t-.57)/.26*Math.PI*2));
                    else if(t<1.06) target=Blend(eye,chels,(t-.83)/.23);
                    else if(t<1.3) target=chels+new V(side*.7*Math.Sin((t-1.06)/.24*Math.PI*2),-1.4*Math.Sin((t-1.06)/.24*Math.PI*2));
                    else target=Blend(chels,rest,(t-1.3)/.3);
                }
            } else if(w.State==Mood.Eat) target+=new V(side*.45*Math.Sin(w.StateTime*11),.55*Math.Sin(w.StateTime*11+side));
            else if(w.State==Mood.WebPluck) target+=new V(side*.8*w.GuestPulse,-1.2*w.GuestPulse);
            else if(w.State==Mood.SpeciesAct && (w.Personality.Gesture==SpeciesGesture.Drum || w.Personality.Gesture==SpeciesGesture.SilkTap)) target+=new V(side*.8*Math.Sin(w.StateTime*9+side),.8*Math.Sin(w.StateTime*9));
            else if(w.State==Mood.Inspect) target+=new V(side*.3*Math.Sin(w.StateTime*7),.2*Math.Sin(w.StateTime*7));
            var p=new PalpJoints();p.Root=new V(side*9.6*skin.HeadWidth,-19.7*skin.HeadLength);
            target=new V(target.X*skin.HeadWidth,target.Y*skin.HeadLength);
            double scale=(skin.HeadWidth+skin.HeadLength)/2;p.Upper=6.8*scale;p.Lower=5.6*scale;
            V delta=target-p.Root;double distance=Math.Max(Math.Abs(p.Upper-p.Lower)+.001,Math.Min(p.Upper+p.Lower-.001,delta.Length));
            V axis=delta*(1/Math.Max(.0001,delta.Length));p.Tip=p.Root+axis*distance;
            double along=(p.Upper*p.Upper-p.Lower*p.Lower+distance*distance)/(2*distance);
            double bend=Math.Sqrt(Math.Max(0,p.Upper*p.Upper-along*along));
            p.Elbow=p.Root+axis*along+new V(-axis.Y,axis.X)*(side*bend);
            return p;
        }
    }
}
