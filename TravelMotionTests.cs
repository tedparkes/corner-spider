using System;
using System.Collections.Generic;
using CornerSpider;

static class TravelMotionTests {
    static int checks,routes,longRuns,shortRuns,lateralRoutes;
    static double longest,shortest=double.MaxValue,peak;
    static readonly V away=new V(-100000,-100000);
    static void Check(bool ok,string why) { checks++;if(!ok) throw new Exception("Travel motion: "+why); }
    static World New(int seed,double size,double dpi,string skin) {
        double s=size*dpi;var w=new World(2400*s,1800*s,seed);
        w.ConfigureDisplays(new[]{new Area(0,0,2400*s,1800*s,dpi)},0);w.SetScale(size);w.SetSpecies(skin);
        w.AutoFeed=w.CursorPlay=false;return w;
    }
    static void Bout(double length,List<double> bouts) {
        if(length<1) return;
        bouts.Add(length);longest=Math.Max(longest,length);shortest=Math.Min(shortest,length);
        if(length>140) longRuns++;if(length<80) shortRuns++;
    }
    static void Route(World w,V start,V end,double dt) {
        double s=w.BodyScale;w.Spider=start;w.Target=end;w.State=Mood.Walk;
        V axis=end-start;w.Angle=Math.Atan2(axis.Y,axis.X)*180/Math.PI+90;
        double length=0,rest=0,sideTime=0,maxDeviation=0,maxBout=0;var bouts=new List<double>();
        for(int frame=0;frame<50/dt && w.State==Mood.Walk;frame++) {
            V before=w.Spider;w.Tick(dt,away);V delta=w.Spider-before;double step=delta.Length;
            Check(w.InsideAny(w.Spider) && World.Finite(w.Spider.X) && World.Finite(w.Speed),"finite, in-bounds travel");
            Check(step<=(370*dt+2.01)*s,"travel cannot teleport between spurts");
            Check((w.Clamp(w.Spider)-w.Spider).Length<.001*s,"full body clearance is retained");
            peak=Math.Max(peak,w.Speed/s);
            if(step>.05*s) {
                length+=step/s;
                double a=w.Angle*Math.PI/180;
                V direction=new V(delta.X*Math.Cos(a)+delta.Y*Math.Sin(a),-delta.X*Math.Sin(a)+delta.Y*Math.Cos(a))*(1/step);
                if(Math.Abs(direction.X)>.65 && w.State==Mood.Walk && w.Speed>30*s) {
                    sideTime+=dt;
                    Check(direction.X*w.GaitDirection.X+direction.Y*w.GaitDirection.Y>.99,"feet follow actual lateral motion instead of playing a forward walk");
                }
            } else { maxBout=Math.Max(maxBout,length);Bout(length,bouts);length=0;rest+=dt; }
            V offset=w.Spider-start;maxDeviation=Math.Max(maxDeviation,Math.Abs(axis.X*offset.Y-axis.Y*offset.X)/axis.Length/s);
        }
        maxBout=Math.Max(maxBout,length);Bout(length,bouts);
        Check(w.State==Mood.Rest && (w.Spider-end).Length<2*s,"skittish route reaches the original destination: skin="+w.Species+", scale="+s+", dt="+dt+", remaining="+(w.Spider-end).Length/s+", state="+w.State);
        Check(maxBout>100 && bouts.Count>=3 && rest>.2,"long journeys combine substantial runs with real stationary pauses");
        if(sideTime>.1) { lateralRoutes++;Check(maxDeviation>10,"lateral motion changes the path, not just the facing angle"); }
        routes++;
    }
    public static void Run() {
        for(int skin=0;skin<SkinCatalog.All.Length;skin++) foreach(double dt in new[]{.016,.033,.1}) foreach(double size in new[]{.2,1,1.8}) {
            var w=New(31+skin,size,new[]{1,1.5,2}[skin%3],SkinCatalog.All[skin].Id);double s=w.BodyScale;
            double angle=(skin%8)*Math.PI/4;V axis=new V(Math.Cos(angle),Math.Sin(angle))*700;
            Route(w,(new V(1200,900)-axis)*s,(new V(1200,900)+axis)*s,dt);
        }
        Check(longRuns>routes && shortRuns>routes,"the population includes both short probes and long runs");
        Check(longest>280 && longest<460 && shortest<45 && peak>270 && peak<=370,"bouts span small adjustments through fast, bounded cross-screen runs");
        Check(lateralRoutes>routes*.60,"sideways scuttles occur across species and frame rates");
        // The original perimeter route and all four hard desktop edges remain usable.
        for(int corner=0;corner<4;corner++) {
            var w=New(7+corner,1,1,"Bold");Route(w,w.Perch(0,corner),w.Perch(0,(corner+1)%4),.033);
        }
        // Small local repositioning should stay careful, with no long-run overshoot.
        var local=New(8,1,1,"Bold");local.Spider=new V(600,400);local.Target=new V(640,420);local.State=Mood.Walk;
        for(int i=0;i<600 && local.State==Mood.Walk;i++) { local.Tick(.025,away);Check(local.Speed<160,"small adjustments retain a careful speed"); }
        Check((local.Spider-local.Target).Length<2,"short adjustment settles exactly at its target");
        // Exercise sideways/backward rig travel directly, including the slim species.
        foreach(var skin in SkinCatalog.All) foreach(V direction in new[]{new V(1,0),new V(-1,0),new V(.6,-.8),new V(0,1)}) for(int frame=0;frame<20;frame++) {
            var w=New(5,1,1,skin.Id);w.State=Mood.Walk;w.GaitBlend=1;w.GaitDirection=direction;w.StepPhase=frame*.3;
            for(int side=-1;side<=1;side+=2) for(int leg=0;leg<4;leg++) {
                var p=SpiderPose.Leg(w,side,leg,skin);
                Check(Math.Abs((p.Knee3-p.Hip3).Length-SpiderPose.Femur[leg]*skin.LimbScale(leg))<.0001 && Math.Abs((p.Ankle3-p.Knee3).Length-SpiderPose.Lower[leg]*skin.LimbScale(leg))<.0001,"lateral stride preserves both fixed leg segments");
                foreach(V joint in new[]{p.Root,p.Hip,p.Knee,p.Ankle,p.Toe,p.Tip}) Check(joint.Length+2<=SpiderPose.Radius,"sideways stride stays within the desktop safety radius");
            }
        }
        var first=New(91,1,1,"Bold");var second=New(91,1,1,"Bold");
        // Raising the forelegs during a turn must override the walking stride.
        foreach(var skin in SkinCatalog.All) for(int side=-1;side<=1;side+=2) {
            var display=New(5,1,1,skin.Id);display.State=Mood.Inspect;display.StateTime=.8;display.GaitDirection=new V(1,0);
            V raised=SpiderPose.Leg(display,side,0,skin).Tip;display.GaitBlend=1;
            for(int frame=0;frame<20;frame++) { display.StepPhase=frame*.3;Check((SpiderPose.Leg(display,side,0,skin).Tip-raised).Length<.0001,"raised forelegs are not dragged sideways by the walking gait"); }
        }
        first.Spider=second.Spider=new V(300,500);first.Target=second.Target=new V(2100,500);first.State=second.State=Mood.Walk;
        for(int i=0;i<1200;i++) { first.Tick(.025,away);second.Tick(.025,away);Check((first.Spider-second.Spider).Length<.000001 && first.Angle==second.Angle,"seeded travel remains deterministic"); }
        Console.WriteLine("PASS: "+checks+" travel checks / "+routes+" routes, "+lateralRoutes+" with lateral scuttles; moving bouts "+shortest.ToString("F1")+"-"+longest.ToString("F1")+" logical pixels, peak "+peak.ToString("F1")+" px/s (all skins, sizes, DPI, frame rates, directions, edges and fixed leg lengths).");
    }
}
