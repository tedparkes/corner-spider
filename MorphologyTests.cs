using System;
using System.Collections.Generic;
using CornerSpider;

static class MorphologyTests {
    static int checks;
    static void Check(bool ok,string why) { checks++;if(!ok) throw new Exception("Species anatomy: "+why); }
    public static void Run() {
        foreach(string id in new[]{"Regal","Bold","Zebra","RegalMale","Tan","RedBack","Emerald","GrayWall","Pantropical","Apache"})
            Check(SkinCatalog.Contains(id) && SkinCatalog.Get(id).Id==id,"original saved ID retained: "+id);
        var sexes=new Dictionary<string,HashSet<string>>();
        foreach(var skin in SkinCatalog.All) {
            if(!sexes.ContainsKey(skin.Scientific)) sexes[skin.Scientific]=new HashSet<string>();sexes[skin.Scientific].Add(skin.Sex);
            var w=new World(1600,1000,42);w.AutoFeed=w.CursorPlay=false;w.Spider=new V(800,500);
            foreach(Mood state in Enum.GetValues(typeof(Mood))) for(int frame=0;frame<40;frame++) {
                w.State=state;w.StateTime=frame*.09;w.StepPhase=frame*.27;w.GaitBlend=state==Mood.Walk?1:0;w.JumpTime=frame*.01;
                for(int side=-1;side<=1;side+=2) for(int leg=0;leg<4;leg++) {
                    LegPose p=SpiderPose.Leg(w,side,leg,skin);double limb=skin.LimbScale(leg);
                    Check(Math.Abs((p.Knee3-p.Hip3).Length-SpiderPose.Femur[leg]*limb)<.0001,skin.Id+" fixed femur length");
                    Check(Math.Abs((p.Ankle3-p.Knee3).Length-SpiderPose.Lower[leg]*limb)<.0001,skin.Id+" fixed lower-leg length");
                    V3 axis=p.Ankle3-p.Hip3,knee=p.Knee3-p.Hip3;
                    if(leg==0) Check(Math.Abs(axis.X*knee.Y-axis.Y*knee.X)<.0001,skin.Id+" front hinge stays in vertical plane");
                    foreach(V joint in new[]{p.Root,p.Hip,p.Knee,p.Ankle,p.Toe,p.Tip})
                        Check(World.Finite(joint.X) && World.Finite(joint.Y) && joint.Length<68,skin.Id+" whole limb within the desktop margin: "+state);
                    Check(Math.Abs(p.Root3.X)<16*skin.HeadWidth && p.Root3.Y>=-23*skin.HeadLength,skin.Id+" legs attach to the front body");
                }
            }
            w.State=Mood.Groom;
            for(int side=-1;side<=1;side+=2) {
                V last=new V();double eyeDistance=100,chelsDistance=100;V rest=new V(side*7*skin.HeadWidth,-30.1*skin.HeadLength);
                for(int f=0;f<=350;f++) {
                    w.StateTime=f*.01;PalpJoints p=PalpPose.At(w,skin,side);
                    Check(Math.Abs((p.Elbow-p.Root).Length-p.Upper)<.00001 && Math.Abs((p.Tip-p.Elbow).Length-p.Lower)<.00001,skin.Id+" palp segment lengths stay fixed");
                    if(f>0) Check((p.Tip-last).Length<.85,skin.Id+" palp motion is continuous");last=p.Tip;
                    eyeDistance=Math.Min(eyeDistance,(p.Tip-new V(side*4.4*skin.HeadWidth,-20.9*skin.HeadLength)).Length);
                    chelsDistance=Math.Min(chelsDistance,(p.Tip-new V(side*3.6*skin.HeadWidth,-26.8*skin.HeadLength)).Length);
                    if(f==0 || f==350) Check((p.Tip-rest).Length<.01,skin.Id+" grooming begins and ends at rest");
                    Check(Math.Abs(p.Root.X-side*9.6*skin.HeadWidth)<.00001,skin.Id+" palp root remains attached");
                }
                Check(eyeDistance<.1 && chelsDistance<.1,skin.Id+" each palp actually reaches its eye and chelicera");
            }
            // Both cleaning appendages must not cover the eyes at once.
            for(int f=0;f<350;f++) {
                w.StateTime=f*.01;int active=0;for(int side=-1;side<=1;side+=2) if((PalpPose.At(w,skin,side).Tip-new V(side*7*skin.HeadWidth,-30.1*skin.HeadLength)).Length>.1) active++;
                Check(active<=1,skin.Id+" alternating grooming leaves the other palp resting");
            }
            w.Spider=w.Home;w.GoHome();for(int f=0;f<160;f++) w.Tick(.025,new V(-10000,-10000));
            Check(w.NestTuck>.99,skin.Id+" settles before testing folded limbs");
            for(int side=-1;side<=1;side+=2) for(int leg=0;leg<4;leg++) {
                var pose=SpiderPose.Leg(w,side,leg,skin);
                foreach(V joint in new[]{pose.Knee,pose.Ankle,pose.Tip}) Check(Math.Abs(joint.X)<33 && Math.Abs(joint.Y)<53,skin.Id+" folded limb fits the silk retreat: "+joint.X+","+joint.Y);
            }
            w=new World(1600,1000,12);w.AutoFeed=false;V cursor=new V(770,450);w.Spider=cursor+new V(-140,40);
            for(int f=0;f<600 && w.State!=Mood.Ride;f++) w.Tick(.025,cursor);
            Check(w.Hanging,skin.Id+" reaches an actual finger catch");
            for(int f=0;f<45;f++) {
                cursor+=new V(.8,.15);w.Tick(.025,cursor);Check(w.Hanging,skin.Id+" remains suspended during gentle carrying");
                for(int side=-1;side<=1;side+=2) {
                    var pose=SpiderPose.Leg(w,side,0,skin);double a=w.Angle*Math.PI/180;
                    V tip=w.Spider+new V(pose.Tip.X*Math.Cos(a)-pose.Tip.Y*Math.Sin(a),pose.Tip.X*Math.Sin(a)+pose.Tip.Y*Math.Cos(a))*w.BodyScale;
                    Check((tip-w.FingerTip(cursor)).Length<12*w.BodyScale,skin.Id+" scaled front feet remain on the finger: frame "+f+" gap="+(tip-w.FingerTip(cursor)).Length+" angle="+w.Angle+" reach="+(w.Spider-w.FingerTip(cursor)).Length+" tip="+pose.Tip.X+","+pose.Tip.Y);
                }
            }
            for(int direction=0;direction<8;direction++) {
                w=new World(1600,1000,42);w.AutoFeed=false;cursor=new V(800,500);double a=direction*Math.PI/4;
                w.Spider=cursor+new V(Math.Cos(a),Math.Sin(a))*110;
                for(int f=0;f<600 && w.State!=Mood.Ride;f++) w.Tick(.025,cursor);
                Check(w.State==Mood.Ride,"finger boarding completes from approach "+direction);
                if(w.Hanging) {
                    Check(w.JumpStart.X<w.FingerTip(cursor).X-18*w.BodyScale && w.JumpStart.Y>w.FingerTip(cursor).Y-12*w.BodyScale,"catch starts on the exposed tip side");
                    Check(Math.Abs(World.AngleDelta(w.Angle,0))<.01,"finger catch is oriented toward its grip");
                } else {
                    Check(w.OnFinger && (w.Spider-w.FingerPoint(w.FingerPosition)).Length<.01,"other approaches touch down on the upper surface");
                    Check(Math.Abs(World.AngleDelta(w.Angle,Math.Atan2(w.JumpEnd.Y-w.JumpStart.Y,w.JumpEnd.X-w.JumpStart.X)*180/Math.PI+90))<12,"surface landing retains its travel-facing orientation");
                }
            }
        }
        int paired=0;foreach(var item in sexes) if(item.Value.Count==2) paired++;
        Check(paired==9 && sexes.Count==11,"all nine original species have male and female options, plus two new species");
        foreach(var female in SkinCatalog.All) foreach(var male in SkinCatalog.All) if(!female.Male && male.Male && female.Scientific==male.Scientific) {
            Check(female.BodyWidth*female.BodyLength>male.BodyWidth*male.BodyLength,"female has a fuller abdomen: "+female.Scientific);
            Check(male.PalpWidth>female.PalpWidth*1.4,"adult male terminal bulbs differ from female palps");
        }
        Check(SkinCatalog.Get("PortiaLabiata").RearLeg>1.4 && SkinCatalog.Get("PortiaLabiata").LegWidth<.6,"Portia has longer, slimmer supporting limbs");
        Check(SkinCatalog.Get("HyllusDiardiGreen").BodyLength/SkinCatalog.Get("HyllusDiardiGreen").BodyWidth>1.4,"Hyllus female retains its long full abdomen");
        Console.WriteLine("PASS: "+checks+" species anatomy checks (20 profiles, original IDs, nine sex pairs, fixed joints, all-state bounds and alternating eye/chelicera grooming).");
    }
}
