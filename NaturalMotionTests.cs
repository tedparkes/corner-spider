using System;
using System.Collections.Generic;
using CornerSpider;

static class NaturalMotionTests {
    static int checks;
    static readonly V away=new V(-10000,-10000);
    static void Check(bool ok,string why) { checks++;if(!ok) throw new Exception("Natural motion: "+why); }
    static World New(int seed) { var w=new World(1280,720,seed);w.AutoFeed=false;w.Spider=new V(600,350);return w; }
    public static void Run() {
        var w=New(42);w.CursorPlay=false;w.Angle=90;w.State=Mood.Walk;w.Target=new V(1100,350);
        double peak=0,run=0,longest=0,still=0;int bursts=0;bool moving=false;
        for(int i=0;i<2000 && w.State==Mood.Walk;i++) {
            V before=w.Spider;w.Tick(.025,away);bool now=w.Speed>5;
            if(now && !moving) bursts++;
            run=now?run+.025:0;longest=Math.Max(longest,run);moving=now;peak=Math.Max(peak,w.Speed);
            if((w.Spider-before).Length<.001 && !now) still+=.025;
        }
        Check(peak>100,"patrol has quick steps, not a slow cruise");
        Check(bursts>=4 && still>1,"patrol repeatedly stops on planted feet");
        Check(longest<.8,"movement bursts stay short");
        Check((w.Spider-w.Target).Length<2,"bursty movement still reaches its destination");

        w=New(8);V cursor=new V(600,250);bool inspected=false,raised=false;int entries=0;Mood last=w.State;
        for(int i=0;i<80;i++) {
            w.Tick(.025,cursor);
            if(w.State==Mood.Inspect && last!=Mood.Inspect) entries++;
            if(w.State==Mood.Inspect) {
                inspected=true;raised|=w.FrontReach(-1)>.5 || w.FrontReach(1)>.5;
                Check((w.Spider-new V(600,350)).Length<.01,"inspection holds its perch");
            }
            last=w.State;
        }
        Check(inspected && raised && entries==1,"near cursor gets one readable foreleg inspection");
        Check(w.State!=Mood.Ride,"reaction is separate from voluntary boarding");
        w=New(4);w.Spider=new V(600,65);w.Tick(.025,new V(600,20));
        Check(w.State==Mood.Inspect,"cursor at top edge is noticed even where boarding is unsafe");
        w=New(8);w.CursorPlay=false;
        for(int i=0;i<100;i++) { w.Tick(.025,cursor);Check(w.State!=Mood.Inspect,"disabled interaction suppresses cursor gestures"); }

        w=New(12);
        for(int i=0;i<500 && w.State!=Mood.Reach;i++) w.Tick(.025,cursor);
        Check(w.State==Mood.Reach,"cursor boarding includes an explicit reach");
        w.Tick(.025,cursor+new V(350,0));
        Check(w.State==Mood.Rest,"moving target cancels a prepared cursor leap");

        foreach(double dt in new[]{.016,.033,.05}) for(int seed=0;seed<20;seed++) {
            w=New(seed);w.SpawnFly();var phases=new HashSet<Mood>();double air=0;bool landed=false;
            for(int i=0;i<18/dt && w.Meals==0;i++) {
                w.Tick(dt,away);phases.Add(w.State);
                if(w.State==Mood.Jump) { air+=dt;Check(phases.Contains(Mood.Reach) && phases.Contains(Mood.Crouch),"reach and brace precede takeoff"); }
                if(w.Meals>0) landed=true;
            }
            Check(landed && w.Meals==1,"capture survives variable frame intervals, seed "+seed);
            Check(air>=.1 && air<.40,"quick airborne interval without a floating ease");
        }
        foreach(Mood state in Enum.GetValues(typeof(Mood))) for(int frame=0;frame<40;frame++) {
            w=New(2);w.State=state;w.StateTime=frame*.04;w.StepPhase=frame*.27;w.GaitBlend=state==Mood.Walk?1:0;w.JumpTime=frame*.01;
            for(int side=-1;side<=1;side+=2) for(int leg=0;leg<4;leg++) {
                LegPose p=SpiderPose.Leg(w,side,leg);
                Check(World.Finite(p.Tip.X) && World.Finite(p.Tip.Y),"finite foot across all poses");
                Check(Math.Abs((p.Knee3-p.Hip3).Length-SpiderPose.Femur[leg])<.0001,"3D femur cannot stretch during a reach");
                Check(Math.Abs((p.Ankle3-p.Knee3).Length-SpiderPose.Lower[leg])<.0001,"3D lower leg cannot stretch during a jump");
                V3 axis=p.Ankle3-p.Hip3,knee=p.Knee3-p.Hip3;
                if(leg==0) Check(Math.Abs(axis.X*knee.Y-axis.Y*knee.X)<.0001,"front hinge folds up in its radial plane, never sideways");
                Check(knee.Z-axis.Z*V3.Dot(knee,axis)/V3.Dot(axis,axis)>0,"hinge bends above its hip-to-foot axis");
                Check((p.Knee-SpiderPose.Project(p.Knee3)).Length<.0001,"screen knee is projected from its spatial joint");
                Check(p.Root.Y<=4 && p.Root.Y>=-14,"every leg attaches to the front body");
                // The longer forelegs intentionally exceed the old 44x47 box.
                // Check the entire limb in a rotation-safe circle, with room for fur.
                foreach(V joint in new[]{p.Root,p.Hip,p.Knee,p.Ankle,p.Toe,p.Tip})
                    Check(joint.Length<68 && joint.Length+2<=SpiderPose.Radius,"whole limb fits the new desktop safety margin at every angle");
            }
        }
        w=New(5);w.State=Mood.Inspect;w.StateTime=.45;
        Check(SpiderPose.Leg(w,-1,0).Lift>3,"inspection raises a front leg");
        for(int side=-1;side<=1;side+=2) for(int leg=1;leg<4;leg++) Check(SpiderPose.Leg(w,side,leg).Lift==0,"six supporting legs remain grounded while inspecting");
        w.State=Mood.Rest;LegPose front=SpiderPose.Leg(w,-1,0);
        for(int leg=1;leg<4;leg++) {
            LegPose other=SpiderPose.Leg(w,-1,leg);
            double frontLength=(front.Knee3-front.Hip3).Length+(front.Ankle3-front.Knee3).Length;
            double otherLength=(other.Knee3-other.Hip3).Length+(other.Ankle3-other.Knee3).Length;
            Check(frontLength>otherLength*1.4,"front pair is visibly longer than each supporting pair");
        }
        Check(front.Tip.Y<-30 && front.Tip3.Z<1,"long forelegs rest forward with the feet on the surface");
        for(int side=-1;side<=1;side+=2) {
            double minX=double.MaxValue,maxX=double.MinValue,elevation=0;V previous=new V();
            w.State=Mood.Inspect;
            for(int frame=0;frame<=165;frame++) {
                w.StateTime=frame*.01;LegPose p=SpiderPose.Leg(w,side,0);
                minX=Math.Min(minX,side*p.Tip.X);maxX=Math.Max(maxX,side*p.Tip.X);elevation=Math.Max(elevation,p.Tip3.Z);
                if(frame>0) Check((p.Tip-previous).Length<3,"display sweep has no sudden wrist teleport");
                previous=p.Tip;
                for(int leg=1;leg<4;leg++) {
                    LegPose supporting=SpiderPose.Leg(w,side,leg);
                    Check(supporting.Lift==0 && Math.Abs(supporting.Ankle3.Z-.8)<.0001,"display leaves supporting feet planted throughout");
                }
                Check(Math.Abs(p.Tip3.Z-SpiderPose.Leg(w,-side,0).Tip3.Z)<.0001,"both front legs rise together");
            }
            Check(maxX-minX<8 && elevation>30,"paired reach rises above the body without sideways arm swings");
            w.State=Mood.Rest;
            Check((SpiderPose.Leg(w,side,0).Tip-previous).Length<.001,"foreleg settles back into its resting pose");
        }
        // Actual endpoint projections at the desktop boundary, not just a rig bound.
        foreach(double size in new[]{.7,1.8}) foreach(double dpi in new[]{1.0,2.0}) {
            w=New(2);w.ConfigureDisplays(new[]{new Area(0,0,1920,1080,dpi)},0);w.SetScale(size);
            for(int corner=0;corner<4;corner++) {
                w.Spider=w.Clamp(new V(corner%2==0?0:1920,corner<2?0:1080));
                w.State=Mood.Inspect;
                for(int frame=0;frame<33;frame++) {
                    w.StateTime=frame*.05;
                    for(int angle=0;angle<360;angle+=30) for(int side=-1;side<=1;side+=2) {
                        LegPose p=SpiderPose.Leg(w,side,0);double r=angle*Math.PI/180;
                        foreach(V joint in new[]{p.Knee,p.Ankle,p.Tip}) {
                            V at=w.Spider+new V(joint.X*Math.Cos(r)-joint.Y*Math.Sin(r),joint.X*Math.Sin(r)+joint.Y*Math.Cos(r))*w.BodyScale;
                            Check(at.X>0 && at.X<1920 && at.Y>0 && at.Y<1080,"long forelegs stay visible at every corner, size and DPI");
                        }
                    }
                }
            }
        }
        Console.WriteLine("PASS: "+checks+" natural-motion checks (bursts, paired upward reach, spatial hinges, fixed leg lengths, full-limb bounds and DPI edge clearance).");
    }
}
