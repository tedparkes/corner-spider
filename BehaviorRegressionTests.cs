using System;
using System.Collections.Generic;
using System.IO;
using CornerSpider;

static class BehaviorRegressionTests {
    static int checks;
    static V away=new V(-10000,-10000);
    static void Check(bool b,string message) { checks++;if(!b) throw new Exception("Regression: "+message); }
    static World New(int seed) { var w=new World(1280,720,seed);w.AutoFeed=false;w.Spider=new V(600,350);return w; }
    static void Tick(World w,double seconds) { for(int i=0;i<seconds*40;i++) w.Tick(.025,away); }
    public static void Run() {
        var w=New(42);w.SpawnFly();V first=w.Fly;w.SpawnFly();Check((w.Fly-first).Length==0,"feeding twice must keep the same prey");
        var seen=new HashSet<Mood>();seen.Add(w.State);
        for(int i=0;i<480;i++) { w.Tick(.025,away);seen.Add(w.State); }
        foreach(var s in new[]{Mood.Notice,Mood.Stalk,Mood.Aim,Mood.Crouch,Mood.Jump,Mood.Land,Mood.Eat}) Check(seen.Contains(s),"readable hunt phase "+s);
        Check(w.Meals==1 && !w.HasFly,"one capture, one meal");
        for(int seed=0;seed<30;seed++) { w=New(seed);w.SpawnFly();Tick(w,16);Check(w.Meals==1,"hunting seed "+seed); }
        w=New(42);w.State=Mood.Groom;bool boarded=false;
        for(int i=0;i<800;i++) { V cursor=new V(650+Math.Sin(i*.8)*2,350+Math.Cos(i*.7)*2);w.Tick(.025,cursor);if(w.State==Mood.Ride) { boarded=true;break; } }
        Check(boarded,"2-pixel hand jitter must still allow a voluntary ride");
        V gentle=w.LastCursor;
        for(int i=0;i<60;i++) { gentle+=new V(1,.15);w.Tick(.025,gentle);Check(w.State==Mood.Ride,"gentle movement retains rider"); }
        V before=w.Spider;w.Tick(.025,gentle+new V(250,0));Check(w.State==Mood.Jump,"fast movement dismounts");Check((w.Spider-before).Length<1,"dismount starts at previous perch");
        Tick(w,1);Check(w.State!=Mood.Ride && w.RideCooldown>20,"landing and cooldown");
        w=New(2);w.CursorPlay=false;
        for(int i=0;i<160;i++) w.Tick(.025,new V(500+i*.5,350));Check(w.StillTime<.5,"slow continuous drift is not a stationary cursor");
        double oldClock=w.Clock;w.Tick(600,new V(900,300));Check(w.Clock==oldClock && w.StillTime==0,"long stall must not advance world or use stale cursor samples");
        w.Tick(double.NaN,away);w.Tick(double.PositiveInfinity,away);Check(w.Clock==oldClock,"invalid time ignored");
        w=New(9);double web=w.Web;Tick(w,2);Check(w.Web==web,"silk only builds at the retreat");
        w.Spider=w.Home;w.GoHome();Tick(w,3);Check(w.State==Mood.Sleep && w.Web>web,"home nap adds silk");
        w=New(7);w.State=Mood.Walk;w.Target=new V(100,350);w.Angle=0;w.Tick(.025,away);Check(Math.Abs(World.AngleDelta(0,w.Angle))<=4.76,"turn speed bounded");
        Check(Math.Abs(World.AngleDelta(359,1)-2)<.01,"shortest facing wrap");
        w.Resize(30,20);Tick(w,2);Check(w.Spider.X>=0 && w.Spider.X<=30 && w.Spider.Y>=0 && w.Spider.Y<=20,"tiny work area stays bounded");
        foreach(var areas in new[]{new[]{new Area(0,0,1280,720,1),new Area(1280,0,1600,900,1.5)},new[]{new Area(0,300,1280,720,1),new Area(1280,0,1600,900,1.5)},new[]{new Area(0,0,1280,720,1),new Area(1400,0,1600,900,1.5)}}) {
            w=New(12);w.ConfigureDisplays(areas,1);w.Spider=new V(1100,600);w.GoHome();bool slept=false;
            for(int i=0;i<4000;i++) { w.Tick(.025,away);if(w.State==Mood.Sleep) { slept=true;break; } }
            Check(slept,"cross-monitor return reaches right-side retreat, including gap/offset");Check(w.AreaAt(w.Home)==1,"right monitor nest");
            w.ConfigureDisplays(new[]{new Area(0,0,1024,768,1)},0);Check(w.InsideAny(w.Spider),"unplugged monitor safely rehomes");
        }
        // A long simulation exercises state transitions, all corners, DPI and changed monitor layouts.
        w=New(99);w.AutoFeed=true;
        for(int i=0;i<96000;i++) {
            if(i%6000==0) { w.Corner=Settings.Corners[(i/6000)%4];w.SetScale(i%12000==0?1.8:.7);w.ConfigureDisplays(new[]{new Area(0,0,1280,720,1),new Area(1280,0,1920,1080,1.5)},1); }
            w.Tick(.025,away);Check(World.Finite(w.Spider.X) && World.Finite(w.Spider.Y),"finite extended simulation");
            if(w.State!=Mood.Jump) Check(w.InsideAny(w.Spider),"extended simulation stays on a display");
        }
        Check(w.Meals>3,"automatic occasional hunting continues");
        double cornerSeconds=0,quietSeconds=0,totalSeconds=0;int walkPauses=0;
        for(int seed=0;seed<8;seed++) {
            w=New(seed);w.CursorPlay=false;w.ConfigureDisplays(new[]{new Area(0,0,1280,720,1),new Area(1280,0,1280,720,1)},1);w.Spider=w.Home;
            for(int i=0;i<24000;i++) {
                w.Tick(.025,away);totalSeconds+=.025;
                if(w.CornerDistance<160*w.BodyScale) cornerSeconds+=.025;
                if(w.Speed<1 && w.State!=Mood.Jump) quietSeconds+=.025;
                if(w.State==Mood.Walk && w.Speed<1) walkPauses++;
            }
        }
        Console.WriteLine("CORNER BEHAVIOR: "+(cornerSeconds/totalSeconds*100).ToString("F1")+"% near a corner; "+(quietSeconds/totalSeconds*100).ToString("F1")+"% stationary (8 seeds, 80 simulated minutes, no prey/cursor).");
        Check(cornerSeconds/totalSeconds>.65,"corner dwelling dominates unoccupied behavior");
        Check(quietSeconds/totalSeconds>.60,"watching and resting dominate unoccupied behavior");
        Check(walkPauses>100,"patrols include deliberate motionless pauses");
        w=New(15);w.Spider=w.Perch(0,3);w.GoHome();
        for(int i=0;i<2400;i++) { w.Tick(.025,away);Check(w.Spider.X<150 || w.Spider.X>1130 || w.Spider.Y<145 || w.Spider.Y>575 || (w.Spider-w.Home).Length<145*w.HomeScale,"opposite-corner return follows the perimeter before the local end-entrance approach");if(w.State==Mood.Sleep) break; }
        var settings=new Settings();settings.Parse(new[]{"scale=NaN","web=Infinity","meals=-9","species=bogus","corner=bad","flies=nonsense"});
        Check(settings.Scale==1 && settings.Web==.15 && settings.Meals==0 && settings.Species=="Regal" && settings.AutoFeed,"malformed settings cannot poison simulation");
        string folder=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"test-settings-"+Guid.NewGuid().ToString("N"));
        try {
            settings.Species="Zebra";settings.Web=.72;settings.Meals=14;settings.HomeMonitor="test-right";Check(settings.Save(folder),"initial settings write");
            var loaded=Settings.Load(folder);Check(loaded.Species=="Zebra" && loaded.Web==.72 && loaded.Meals==14 && loaded.HomeMonitor=="test-right","settings roundtrip");
            settings.Web=.81;Check(settings.Save(folder),"atomic replacement");Check(File.Exists(Path.Combine(folder,"settings.txt.bak")),"previous settings preserved");
        } finally { foreach(string name in new[]{"settings.txt","settings.txt.bak","settings.txt.new"}) { string path=Path.Combine(folder,name);if(File.Exists(path)) File.Delete(path); }if(Directory.Exists(folder)) Directory.Delete(folder); }
        Console.WriteLine("PASS: "+checks+" regression assertions (hunt phases, jitter, rides, nest, mixed-DPI geometry, unplug, 40-minute simulation, persistence).");
    }
}
