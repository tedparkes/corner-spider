using System;
using System.Collections.Generic;
using CornerSpider;

static class PounceTests {
    static int checks,hunts;
    static double shortest=double.MaxValue,longest;
    static readonly V away=new V(-10000,-10000);
    static void Check(bool ok,string why) { checks++;if(!ok) throw new Exception("Pounce: "+why); }
    static World New(string species,double size,double dpi) {
        double s=size*dpi;var w=new World(4000*s,2500*s,42);
        w.ConfigureDisplays(new[]{new Area(0,0,4000*s,2500*s,dpi)},0);w.SetScale(size);
        w.Spider=new V(2000*s,1250*s);w.SetSpecies(species);w.AutoFeed=w.CursorPlay=false;return w;
    }
    static void FeedAt(World w,bool small,V point) {
        if(small) w.SpawnSmallSpider();else w.SpawnFly();
        w.Fly=point;
        V direction=point-w.Spider;w.Angle=Math.Atan2(direction.Y,direction.X)*180/Math.PI+90;
    }
    static void Hunt(World w,double dt,bool longLeap,bool small,bool ceiling) {
        var seen=new HashSet<Mood>();int jumps=0;double takeoffScale=0,flight=0;V target=new V();
        for(int f=0;f<30/dt && w.State!=Mood.Eat;f++) {
            Mood before=w.State;seen.Add(before);w.Tick(dt,away);
            Check(World.Finite(w.Spider.X) && World.Finite(w.Spider.Y) && World.Finite(w.Lift),"finite motion");
            if(w.State==Mood.Jump && before!=Mood.Jump) {
                jumps++;takeoffScale=w.BodyScale;flight=(w.JumpEnd-w.JumpStart).Length/takeoffScale;target=w.Fly;
                if(longLeap) {
                    // Previous reach was 82 minus a 25-pixel mouth offset = 57.
                    // Check actual takeoff-to-landing displacement, not the aim radius.
                    Check(flight>=57*3 && flight<=57*4,"all skins leap 3-4 times the old distance: "+w.Species+" / "+flight.ToString("F1")+" size="+w.Scale+" dt="+dt+" start="+w.JumpStart.X+","+w.JumpStart.Y+" end="+w.JumpEnd.X+","+w.JumpEnd.Y+" prey="+w.Fly.X+","+w.Fly.Y);
                    shortest=Math.Min(shortest,flight);longest=Math.Max(longest,flight);
                } else Check(flight<115,"already nearby prey does not provoke an artificial long leap");
                Check(w.JumpDuration>=.20 && w.JumpDuration<=.36,"pounce stays brisk");
                Check((w.JumpEnd-target).Length<=30*takeoffScale,"forward legs arrive at prey");
                Check(w.SilkOpacity==1 && (w.SilkStart-w.JumpStart).Length<.001,"takeoff anchors its safety line");
            }
            if(w.State==Mood.Jump) {
                Check(Math.Abs(w.BodyScale-takeoffScale)<.0001,"flight retains takeoff scale across a DPI seam");
                Check((w.Fly-target).Length<.001,"lined-up prey stays put until contact");
                if(ceiling) Check(w.Spider.Y-w.Lift-SpiderPose.Radius*w.BodyScale>=w.Areas[w.AreaAt(w.Spider)].Y-.001,"whole airborne pose stays below the desktop ceiling");
            }
        }
        Check(w.State==Mood.Eat && w.Meals==1 && !w.HasFly,"one successful capture reaches eating");
        Check(w.Meal==(small?PreyKind.SmallSpider:PreyKind.Fly),"captured meal keeps its identity");
        Check(jumps==1,"one deliberate pounce completes the hunt");
        foreach(Mood phase in new[]{Mood.Notice,Mood.Stalk,Mood.Aim,Mood.Reach,Mood.Crouch,Mood.Jump,Mood.Land}) Check(seen.Contains(phase),"retains readable "+phase+" phase");
        hunts++;
    }
    public static void Run() {
        foreach(var skin in SkinCatalog.All) foreach(double size in new[]{.2,1,1.8}) foreach(double dpi in new[]{1,1.5,2}) foreach(double dt in new[]{.016,.033,.1}) for(int direction=0;direction<8;direction++) {
            var w=New(skin.Id,size,dpi);double angle=direction*Math.PI/4;
            FeedAt(w,false,w.Spider+new V(Math.Cos(angle),Math.Sin(angle))*450*w.BodyScale);
            Hunt(w,dt,true,false,true);
            if(skin.SpiderPrey) {
                w=New(skin.Id,size,dpi);FeedAt(w,true,w.Spider+new V(Math.Cos(angle),Math.Sin(angle))*450*w.BodyScale);
                Hunt(w,dt,true,true,true);
            }
        }
        foreach(var skin in SkinCatalog.All) {
            var near=New(skin.Id,1,1);FeedAt(near,false,near.Spider+new V(100,0));Hunt(near,.025,false,false,true);
        }
        foreach(double size in new[]{.2,1,1.8}) foreach(double dt in new[]{.016,.033,.1}) {
            var edge=New("Bold",size,1);edge.Spider=new V(300*size,SpiderPose.Radius*size);
            FeedAt(edge,false,new V(900*size,SpiderPose.Radius*size));Hunt(edge,dt,true,false,true);
            var seam=New("PortiaLabiata",size,1);
            seam.ConfigureDisplays(new[]{new Area(-1800,30,1800,1400,1),new Area(0,30,2400,1800,1.5)},1);
            seam.Spider=new V(-350*size,700);FeedAt(seam,true,new V(50*size,700));Hunt(seam,dt,true,true,true);
            Check(seam.JumpStart.X<0 && seam.JumpEnd.X>0,"long pounce crosses an adjacent mixed-DPI seam");
        }
        Console.WriteLine("PASS: "+checks+" pounce checks / "+hunts+" hunts (all 20 skins, both prey, 8 directions, 20-180% size, 100-200% DPI, 16/33/100 ms steps, ceiling and monitor seam). Actual long leaps: "+shortest.ToString("F1")+"-"+longest.ToString("F1")+" logical pixels at 100%.");
    }
}
