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
        static readonly double[] groundPhases={0,.53,.07,.57};
        static readonly double[] silkPhases={0,.50,.25,.75};
        static readonly double[] familyAnkleX={29,44,45,36},familyAnkleY={-44,-24,15,39};
        static readonly double[] familyKneeX={19,27,27,23},familyKneeY={-40,-24,-1,20};
        public static readonly double[] Femur={20.5,11.5,11,14},Lower={21,11.8,11.5,15};
        const double DorsalDepth=.9659258,DorsalHeight=.258819;
        const double GroundCadence=1.12,GroundDuty=.65;
        const double PhaseDistance=4.5;
        static V3 Lerp(V3 a,V3 b,double t) { return a+(b-a)*t; }
        static V3 Unit(V3 p) { return p*(1/Math.Max(.0001,p.Length)); }
        // Slightly oblique dorsal projection: Z is elevation above the desktop.
        // Hinge motion happens in 3D before projection, never as a sideways elbow.
        public static V Project(V3 p) { return new V(p.X,p.Y*DorsalDepth+p.Z*DorsalHeight); }
        static double Ease(double progress) {
            double t=Math.Max(0,Math.Min(1,progress));
            return t*t*(3-2*t);
        }
        static void FootCycle(World w,double offset,double cadence,double planted,double swingHeight,out double stride,out double lift) {
            double phase=w.StepPhase*cadence/(Math.PI*2)+offset;
            phase-=Math.Floor(phase);
            // StepPhase advances by one radian per 4.5 units travelled. Match
            // that motion during stance, then lift and replace the foot.
            double pace=PhaseDistance*Math.PI*2/cadence;
            double half=pace*planted*.5;
            lift=0;
            if(phase<planted) stride=pace*phase-half;
            else {
                double t=(phase-planted)/(1-planted);
                stride=half-2*half*t*t*(3-2*t)+pace*(1-planted)*t*(1-t)*(1-2*t);
                double arc=Math.Sin(t*Math.PI);
                lift=swingHeight*arc*arc;
            }
            stride*=w.GaitBlend;
            lift*=w.GaitBlend;
        }
        static V3 FitStride(V3 neutral,V3 hip,V3 axis,double legLength) {
            // Center the support stroke in the leg's reachable disk so the IK
            // limit does not drag a planted foot along with the spider.
            double height=hip.Z-neutral.Z;
            double radius=Math.Sqrt(Math.Max(0,legLength*legLength-height*height));
            double axisLength=axis.Length;
            if(axisLength<.001 || radius<.001) return neutral;
            double half=Math.Min(radius,PhaseDistance*Math.PI*GroundDuty*axisLength/GroundCadence);
            V3 along=axis*(1/axisLength),offset=new V3(neutral.X-hip.X,neutral.Y-hip.Y,0);
            double forward=V3.Dot(offset,along);
            V3 side=offset-along*forward;
            double limit=Math.Sqrt(Math.Max(0,radius*radius-half*half));
            if(side.Length>limit) side=side*(limit/side.Length);
            double reach=Math.Sqrt(Math.Max(0,radius*radius-side.Length*side.Length))-half;
            V3 fitted=hip+side+along*Math.Max(-reach,Math.Min(reach,forward));
            return new V3(fitted.X,fitted.Y,neutral.Z);
        }
        static V3 LeapFoot(World w,V3 standing,V3 braced,V3 pushed,V3 tucked,V3 landed) {
            if(w.State==Mood.Crouch) return Lerp(standing,braced,Ease(w.StateTime/.26));
            if(w.State==Mood.Land) return Lerp(landed,standing,Ease(w.StateTime/.20));
            double t=w.JumpProgress;
            if(t<.22) return Lerp(braced,pushed,Ease(t/.22));
            if(t<.58) return Lerp(pushed,tucked,Ease((t-.22)/.36));
            return Lerp(tucked,landed,Ease((t-.58)/.42));
        }
        public static LegPose Leg(World w,int side,int leg) { return Leg(w,side,leg,null); }
        public static LegPose Leg(World w,int side,int leg,SkinProfile skin) {
            var p=new LegPose();
            double limb=skin==null?1:skin.LimbScale(leg),headX=skin==null?1:skin.HeadWidth,headY=skin==null?1:skin.HeadLength;
            double tuck=w.NestTuck;
            p.Root3=new V3(side*(leg==3?9:10.5)*headX,rootY[leg]*headY,7-2*tuck);
            p.Hip3=p.Root3+new V3(side*(leg==0?4.5:2),leg<2?-1:1,1);
            p.Root3=w.SurveyAttachment(p.Root3);
            p.Hip3=w.SurveyAttachment(p.Hip3);
            double stride,lift;
            FootCycle(w,groundPhases[leg]+(side>0?.5:0),GroundCadence,GroundDuty,leg==0?5:3.5,out stride,out lift);
            stride*=1-tuck;
            lift*=1-tuck;
            // Compensate for the later species proportions so every grounded
            // foot follows the same travel vector. Raised poses override stride.
            double stance=skin==null?1:skin.Stance;
            double upperLength=Femur[leg]*limb,lowerLength=Lower[leg]*limb;
            V3 neutral=new V3(side*ankleX[leg]*stance,ankleY[leg]*limb,.8);
            V3 axis=new V3(w.GaitDirection.X,w.GaitDirection.Y/DorsalDepth,0);
            if(w.GaitBlend>0) neutral=Lerp(neutral,FitStride(neutral,p.Hip3,axis,upperLength+lowerLength-.02),w.GaitBlend*(1-tuck));
            V3 step=neutral-axis*stride;
            V3 ankle=new V3(step.X/stance,step.Y/limb,.8+lift);
            double reach=leg==0?w.FrontReach(side):0;
            bool leap=w.State==Mood.Crouch || w.State==Mood.Jump || w.State==Mood.Land;
            if(leap && leg==0) {
                V3 standing=w.State==Mood.Crouch?new V3(side*21,-46,25):ankle;
                ankle=LeapFoot(w,standing,new V3(side*22,-41,1.8),new V3(side*23,-49,15),
                    new V3(side*21,-43,16),new V3(side*24,-47,1.5));
                if(w.State==Mood.Crouch) reach=1-.8*Ease(w.StateTime/.26);
                else if(w.State==Mood.Jump) reach=.2+.8*Math.Sin(Math.PI*w.JumpProgress);
                else reach=.2*(1-Ease(w.StateTime/.20));
            } else if(reach>0) {
                // Both long front legs reach forward and UP. The raised feet stay
                // in narrow, nearly parallel sagittal planes on either side of the face.
                V3 display=new V3(side*21,-46,25);
                if(w.State==Mood.Inspect) display=new V3(side*21,-43,29+Math.Sin(w.StateTime*7)*.7);
                if(w.State==Mood.SpeciesAct && w.Personality.Gesture==SpeciesGesture.SilkTap) display=new V3(side*18,-41,2+Math.Max(0,Math.Sin(w.StateTime*11+side))*6);
                ankle=Lerp(ankle,display,reach);
            }
            if(leap && leg>0) {
                double x=ankleX[leg],y=ankleY[leg];
                ankle=LeapFoot(w,ankle,new V3(side*(x+1.5),y+(leg==3?-3:1),.8),
                    new V3(side*(x+2),y+(leg==3?7:3),4),new V3(side*(x-4),y+(leg==3?-5:0),7),
                    new V3(side*(x+1),y+(leg==3?1:-1),.8));
            }
            if(w.State==Mood.Dismount && leg>0) ankle+=new V3(side*1.5,leg==3?-3:1,0)*w.DismountBrace;
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
            if(w.State==Mood.WebPluck && w.WebStage==WebVisit.Plucking) {
                bool plucking=leg==0 && side==w.GuestPluckingSide;
                // The eyes follow the approaching resident. Preserve the seven
                // world-space footholds through that small body turn.
                V relative=((plucking?w.GuestFootContact:w.GuestSupportFoot(side,leg))-w.Spider)*(1/w.BodyScale);double angle=w.Angle*Math.PI/180;
                V tip=new V(relative.X*Math.Cos(angle)+relative.Y*Math.Sin(angle),-relative.X*Math.Sin(angle)+relative.Y*Math.Cos(angle));
                V3 toeDirection=leg==0?Unit(new V3(side*.06,-1,-.1)):Unit(new V3(side*(leg==3?.4:.8),leg==1?-.6:leg==2?.4:1,-.1));
                V toeOffset=Project(toeDirection*((leg==0?7.5:5.3)*limb));
                double z=.8+(plucking?3*w.GuestPulse:0);
                V3 planted=new V3(tip.X-toeOffset.X,(tip.Y-toeOffset.Y-z*.258819)/.9659258,z);
                double blend=Math.Min(1,w.StateTime/.75);blend=blend*blend*(3-2*blend);
                ankle=Lerp(ankle,planted,blend);
            }
            // Supporting legs fan from the coxa. The front pair flexes vertically.
            V3 up=new V3(leg==0?0:side*(leg==3?.7:1.3),0,1);
            V3 foot;
            if(leg==0) foot=Unit(new V3(side*.06,-1+.55*reach,-.1+1.05*reach));
            else foot=Unit(new V3(side*(leg==3?.4:.8),leg==1?-.6:leg==2?.4:1,-.1));
            if(tuck>0) foot=Unit(Lerp(foot,Unit(new V3(side*.12,leg<2?-1:1,-.06)),tuck));
            return Solve(p,ankle,upperLength,lowerLength,up,foot,(leg==0?4.5:3.3)*limb,(leg==0?3:2)*limb);
        }
        public static LegPose FamilyLeg(World w,int side,int leg,SkinProfile skin) {
            var p=new LegPose();
            double spread=skin.WebKind==WebKind.Sheet?.92:skin.WebKind==WebKind.Tangle?.96:1;
            double length=skin.Pattern=="Golden"?1.06:1;
            double tuck=w.NestTuck,reach=leg==0?w.FrontReach(side):0;
            p.Root3=new V3(side*5,-15+leg*5,7);
            p.Hip3=p.Root3+new V3(side*3,0,1);
            V3 neutral=new V3(side*familyAnkleX[leg]*spread,familyAnkleY[leg]*length,.8);
            V3 knee=new V3(side*familyKneeX[leg]*spread,familyKneeY[leg]*length,21);
            bool silk=skin.WebKind!=WebKind.Sheet;
            double phase=silk?silkPhases[leg]+(side>0?.375:0):groundPhases[leg]+(side>0?.5:0);
            double planted=silk?.76:.65;
            double stride,lift;
            FootCycle(w,phase,1,planted,silk?(leg==0?9:6.5):5,out stride,out lift);
            V3 ankle=neutral+new V3(-w.GaitDirection.X*stride,-w.GaitDirection.Y*stride/DorsalDepth,lift);
            if(tuck>0) ankle=Lerp(ankle,new V3(neutral.X*.78,neutral.Y*.88,1),tuck);
            if(w.State==Mood.Crouch || w.State==Mood.Jump || w.State==Mood.Land) {
                if(leg==0) {
                    V3 standing=w.State==Mood.Crouch?new V3(side*21,-54,13):ankle;
                    ankle=LeapFoot(w,standing,new V3(neutral.X*.91,neutral.Y*.92,.8),new V3(side*25,-57,10),
                        new V3(side*23,-46,13),new V3(side*27,-49,.8));
                } else {
                    ankle=LeapFoot(w,ankle,new V3(neutral.X*1.04,neutral.Y*.95,.8),
                        new V3(neutral.X*1.03,neutral.Y+(leg==3?6:2),4),new V3(neutral.X*.76,neutral.Y*.91,7),
                        new V3(neutral.X*1.03,neutral.Y,.8));
                }
            } else if(reach>0) ankle=Lerp(ankle,new V3(side*21,-54,13),reach);
            if(w.OnFinger || w.Climbing || w.State==Mood.Dismount) ankle.X*=.78;
            if(w.Hanging) {
                if(leg==0) {
                    V grip=(w.FingerTip(w.FingerOrigin)-w.Spider)*(1/w.BodyScale);
                    double angle=-w.Angle*Math.PI/180;
                    ankle=new V3(grip.X*Math.Cos(angle)-grip.Y*Math.Sin(angle)+side*3,
                        (grip.X*Math.Sin(angle)+grip.Y*Math.Cos(angle)+5)/DorsalDepth,2);
                } else ankle=Lerp(p.Hip3,ankle,.7);
            }
            // Keep the tip inside the clearance used by desktop edges and previews.
            V projected=Project(ankle);
            if(projected.Length>62) {
                projected*=62/projected.Length;
                ankle.X=projected.X;
                ankle.Y=(projected.Y-ankle.Z*DorsalHeight)/DorsalDepth;
            }
            V3 foot=Unit(new V3(side*(leg==0?.25:leg==3?.52:.84),leg<2?-1:leg==2?.45:.9,-.10));
            V3 kneeDirection=knee-p.Hip3;
            p.Root3.Z-=3*w.WebSense;
            p.Hip3.Z-=3*w.WebSense;
            return Solve(p,ankle,kneeDirection.Length,(neutral-knee).Length,kneeDirection,foot,3,2);
        }
        static LegPose Solve(LegPose p,V3 ankle,double a,double b,V3 up,V3 foot,double toeLength,double tipLength) {
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
            // Project the resting knee direction into the active hinge plane.
            V3 hinge=Unit(up-direction*V3.Dot(up,direction));
            p.Knee3=p.Hip3+direction*along+hinge*bend;
            p.Toe3=p.Ankle3+foot*toeLength;
            p.Tip3=p.Toe3+foot*tipLength;
            p.Lift=Math.Max(0,ankle.Z-.8);
            p.Root=Project(p.Root3);p.Hip=Project(p.Hip3);p.Knee=Project(p.Knee3);
            p.Ankle=Project(p.Ankle3);p.Toe=Project(p.Toe3);p.Tip=Project(p.Tip3);
            return p;
        }
    }
}
