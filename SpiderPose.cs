using System;

namespace CornerSpider {
    public struct V3 {
        public double X,Y,Z;
        public V3(double x,double y,double z) { X=x;Y=y;Z=z; }
        public double Length { get { return Math.Sqrt(X*X+Y*Y+Z*Z); } }
        public static V3 operator +(V3 a,V3 b) { return new V3(a.X+b.X,a.Y+b.Y,a.Z+b.Z); }
        public static V3 operator -(V3 a,V3 b) { return new V3(a.X-b.X,a.Y-b.Y,a.Z-b.Z); }
        public static V3 operator *(V3 a,double b) { return new V3(a.X*b,a.Y*b,a.Z*b); }
        public static double Dot(V3 a,V3 b) { return a.X*b.X+a.Y*b.Y+a.Z*b.Z; }
    }
    public struct LegPose {
        public V Root,Hip,Knee,Ankle,Toe,Tip;
        public V3 Root3,Hip3,Knee3,Ankle3,Toe3,Tip3;
        public double Lift;
    }
    public static class SpiderPose {
        public const double Radius=70;
        static readonly double[] rootY={-14,-9,-4,0};
        static readonly double[] ankleX={27,30,30,23},ankleY={-35,-14,13,31};
        public static readonly double[] Femur={20.5,11.5,11,14},Lower={21,11.8,11.5,15};
        static V3 Lerp(V3 a,V3 b,double t) { return a+(b-a)*t; }
        static V3 Unit(V3 p) { return p*(1/Math.Max(.0001,p.Length)); }
        // Slightly oblique dorsal projection: Z is elevation above the desktop.
        // Hinge motion happens in 3D before projection, never as a sideways elbow.
        public static V Project(V3 p) { return new V(p.X,p.Y*.9659258+p.Z*.258819); }
        public static LegPose Leg(World w,int side,int leg) { return Leg(w,side,leg,null); }
        public static LegPose Leg(World w,int side,int leg,SkinProfile skin) {
            var p=new LegPose();
            double limb=skin==null?1:skin.LimbScale(leg),headX=skin==null?1:skin.HeadWidth,headY=skin==null?1:skin.HeadLength;
            double tuck=w.NestTuck;
            p.Root3=new V3(side*(leg==3?9:10.5)*headX,rootY[leg]*headY,7-2*tuck);
            p.Hip3=p.Root3+new V3(side*(leg==0?4.5:2),leg<2?-1:1,1);
            double phase=(w.StepPhase/(Math.PI*2)+(leg%2)*.5+(side>0?.5:0))%1;
            if(phase<0) phase+=1;
            double stride,lift=0;
            if(phase<.65) stride=-9+18*phase/.65;
            else { double t=(phase-.65)/.35;stride=9-18*t*t*(3-2*t);lift=Math.Sin(t*Math.PI)*3; }
            stride*=w.GaitBlend*(1-tuck);lift*=w.GaitBlend*(1-tuck);
            V3 ankle=new V3(side*ankleX[leg],ankleY[leg]+stride,.8+lift);
            double reach=leg==0?w.FrontReach(side):0;
            if(reach>0) {
                // Both long front legs reach forward and UP. The raised feet stay
                // in narrow, nearly parallel sagittal planes on either side of the face.
                V3 display=new V3(side*21,-46,25);
                if(w.State==Mood.Inspect) display=new V3(side*21,-43,29+Math.Sin(w.StateTime*7)*.7);
                if(w.State==Mood.SpeciesAct && w.Personality.Gesture==SpeciesGesture.SilkTap) display=new V3(side*18,-41,2+Math.Max(0,Math.Sin(w.StateTime*11+side))*6);
                if(w.State==Mood.Crouch) display=new V3(side*22,-46,12);
                if(w.State==Mood.Jump) display=new V3(side*(22+2*w.JumpProgress),-48,14);
                ankle=Lerp(ankle,display,reach);
            }
            if(w.State==Mood.Crouch && leg>0) ankle+=new V3(side*1.5,leg==3?-3:1,0);
            if(w.State==Mood.Dismount && leg>0) ankle+=new V3(side*1.5,leg==3?-3:1,0)*w.DismountBrace;
            if(w.State==Mood.Jump && leg>0) {
                ankle=new V3(side*(leg==1?29:leg==2?32:22),ankleY[leg]+(leg==3?7*(1-w.JumpProgress):3),5);
            }
            if(w.State==Mood.Land && leg>0) ankle.X+=side*2*(1-Math.Min(1,w.StateTime/.20));
            if(w.State==Mood.Struggle) {
                double effort=w.CaptureStruggle;
                if(leg==0) ankle=Lerp(ankle,new V3(side*(15+Math.Sin(w.StateTime*19+side)*2*effort),-30+Math.Sin(w.StateTime*15)*effort,5+effort*2),1);
                else ankle.X+=side*1.3*effort;
            }
            // Resting feet fold alongside the body; the spatial hinge solver
            // still preserves every segment length and the forelegs' joint plane.
            if(tuck>0) ankle=Lerp(ankle,new V3(side*(leg==0?15:leg==1?18:leg==2?20:14),leg==0?-29:leg==1?-12:leg==2?10:25,1.5),tuck);
            // Eye/chelicera cleaning belongs to the palps. Walking legs stay planted.
            if(skin!=null) { ankle.X*=skin.Stance;ankle.Y*=limb; }
            if(w.Hanging) {
                if(leg==0) {
                    V grip=new V(0,-52);V relative=(w.FingerTip(w.Cursor)-w.Spider)*(1/w.BodyScale);
                    if(relative.Length<110) {
                        double angle=w.Angle*Math.PI/180;
                        grip=new V(relative.X*Math.Cos(angle)+relative.Y*Math.Sin(angle),-relative.X*Math.Sin(angle)+relative.Y*Math.Cos(angle));
                    }
                    ankle=new V3(grip.X+side*6,(grip.Y+7-4*.258819)/.9659258,4);
                } else ankle=Lerp(p.Hip3,ankle,.63)+new V3(0,3,2);
            } else if(w.OnFinger || w.Climbing || w.State==Mood.Dismount) {
                if(reach<.01) ankle.X*=.73; // Supporting feet curl over the finger's sides.
            }
            if(w.State==Mood.WebPluck && w.WebStage==WebVisit.Plucking && leg==0 && side==w.GuestPluckingSide) {
                V relative=(w.GuestFootContact-w.Spider)*(1/w.BodyScale);double angle=w.Angle*Math.PI/180;
                V tip=new V(relative.X*Math.Cos(angle)+relative.Y*Math.Sin(angle),-relative.X*Math.Sin(angle)+relative.Y*Math.Cos(angle));
                V3 toeDirection=Unit(new V3(side*.06,-1,-.1));V toeOffset=Project(toeDirection*(7.5*limb));
                double z=.8+3*w.GuestPulse;
                V3 planted=new V3(tip.X-toeOffset.X,(tip.Y-toeOffset.Y-z*.258819)/.9659258,z);
                double blend=Math.Min(1,w.StateTime/.75);blend=blend*blend*(3-2*blend);
                ankle=Lerp(ankle,planted,blend);
            }
            double a=Femur[leg]*limb,b=Lower[leg]*limb;
            V3 delta=ankle-p.Hip3;
            // Limit horizontal reach without lifting a planted foot off the ground.
            double horizontal=Math.Sqrt(delta.X*delta.X+delta.Y*delta.Y);
            double available=Math.Sqrt(Math.Max(.0001,(a+b-.01)*(a+b-.01)-delta.Z*delta.Z));
            if(horizontal>available) { delta.X*=available/horizontal;delta.Y*=available/horizontal; }
            V3 direction=Unit(delta);
            double distance=Math.Max(Math.Abs(a-b)+.01,Math.Min(a+b-.01,delta.Length));
            p.Ankle3=p.Hip3+direction*distance;
            double along=(a*a-b*b+distance*distance)/(2*distance);
            double bend=Math.Sqrt(Math.Max(0,a*a-along*along));
            // Up projected perpendicular to the hip/foot axis defines the hinge plane.
            // The knee therefore rises above the ground instead of bowing outward.
            // Supporting legs fan from the coxa; the front pair flexes vertically.
            V3 up=new V3(leg==0?0:side*(leg==3?.7:1.3),0,1),hinge=Unit(up-direction*V3.Dot(up,direction));
            p.Knee3=p.Hip3+direction*along+hinge*bend;
            V3 foot;
            if(leg==0) foot=Unit(new V3(side*.06,-1+.55*reach,-.1+1.05*reach));
            else foot=Unit(new V3(side*(leg==3?.4:.8),leg==1?-.6:leg==2?.4:1,-.1));
            if(tuck>0) foot=Unit(Lerp(foot,Unit(new V3(side*.12,leg<2?-1:1,-.06)),tuck));
            p.Toe3=p.Ankle3+foot*((leg==0?4.5:3.3)*limb);
            p.Tip3=p.Toe3+foot*((leg==0?3:2)*limb);
            p.Lift=Math.Max(0,ankle.Z-.8);
            p.Root=Project(p.Root3);p.Hip=Project(p.Hip3);p.Knee=Project(p.Knee3);
            p.Ankle=Project(p.Ankle3);p.Toe=Project(p.Toe3);p.Tip=Project(p.Tip3);
            return p;
        }
    }
}
