using System;
using CornerSpider;

static class PatrolAttentionTests {
    static int checks;
    static readonly V away=new V(-2000,-2000);
    static void Check(bool ok,string why) { checks++;if(!ok) throw new Exception("Patrol attention: "+why); }
    static World New() { var w=new World(1280,720,42);w.AutoFeed=false;w.Spider=new V(700,70);return w; }
    public static void Run() {
        V pointer=new V(740,140);var w=New();
        for(int i=0;i<80;i++) w.Tick(.025,pointer);
        w.State=Mood.Walk;w.Target=new V(1100,70);w.Angle=90;V start=w.Spider;
        w.Tick(.025,pointer);
        Check(w.State==Mood.Inspect && w.Speed==0,"earlier greeting does not make a patrol ignore the pointer");
        Check((w.Spider-start).Length<.001,"patrol stops rather than walking out of invitation range");
        Check(w.FrontReach(-1)==0,"repeat attention does not replay a greeting still on cooldown");
        for(int i=0;i<180;i++) w.Tick(.025,pointer);
        Check(w.State==Mood.Ride,"held pointer can board after interrupting an already-greeted patrol");

        foreach(Mood route in new[]{Mood.Walk,Mood.Return}) {
            w=New();w.Spider=new V(180,70);w.Target=new V(1100,70);
            if(route==Mood.Return) w.GoHome();else w.State=Mood.Walk;
            V destination=w.Target;w.Tick(.025,new V(220,140));
            Check(w.State==Mood.Inspect,"notices pointer during "+route);
            for(int i=0;i<30;i++) w.Tick(.025,new V(220,140));
            w.Tick(.025,away);
            Check(w.State==route && (w.Target-destination).Length<.001,"resumes original "+route+" destination when pointer leaves");
            for(int i=0;i<2400 && w.State!=Mood.Sleep && (w.Spider-destination).Length>2;i++) w.Tick(.025,away);
            Check((w.Spider-destination).Length<2,"interrupted route still reaches its destination");
        }

        foreach(double dpi in new[]{1,1.5,2}) foreach(double scale in new[]{.7,1,1.8}) {
            w=new World(8000,4500,12);w.AutoFeed=false;w.Scale=scale;
            w.ConfigureDisplays(new[]{new Area(0,0,8000,4500,dpi)},0);
            double s=w.BodyScale;w.Spider=new V(700*s,70*s);w.Target=new V(1400*s,70*s);w.State=Mood.Walk;
            pointer=w.Spider+new V(40,70)*s;start=w.Spider;
            bool noticed=false;
            for(int i=0;i<480 && w.State!=Mood.Ride;i++) {
                w.Tick(.025,pointer+new V(Math.Sin(i*.7),Math.Cos(i*.9))*dpi);
                if(w.State==Mood.Inspect) {
                    noticed=true;Check((w.Spider-start).Length<.001,"holds still through the five-second invitation at varied sizes/DPIs");
                }
            }
            Check(noticed && w.State==Mood.Ride,"top-edge patrol accepts jitter-tolerant invitation at size "+scale+" / DPI "+dpi);
        }

        // At the extreme edge, keep looking without offering an unsafe finger.
        w=New();w.State=Mood.Walk;w.Target=new V(1100,70);pointer=new V(740,20);
        w.Tick(.025,pointer);Check(w.State==Mood.Inspect,"notices pointer even where the finger cannot fit");
        for(int i=0;i<160;i++) { w.Tick(.025,pointer);Check(w.State==Mood.Inspect && w.FingerBlend==0,"watches an edge pointer without unsafe boarding"); }
        w.Tick(.025,away);Check(w.State==Mood.Walk,"unsafe invitation can leave the patrol intact");

        w=New();w.CursorPlay=false;w.State=Mood.Walk;w.Target=new V(1100,70);
        for(int i=0;i<60;i++) { w.Tick(.025,new V(740,140));Check(w.State==Mood.Walk,"disabled cursor play leaves patrol alone"); }
        w=New();w.SpawnFly();w.Tick(.025,w.Spider+new V(40,70));Check(w.State==Mood.Notice,"prey attention still takes priority");
        w=New();w.State=Mood.Walk;w.Target=new V(1100,70);w.Tick(.025,new V(700,450));Check(w.State==Mood.Walk,"distant pointer does not interrupt patrols");
        w=New();w.State=Mood.Walk;w.Target=new V(1100,70);w.Tick(.025,new V(740,140));
        w.Tick(.025,new V(740,140),true);Check(w.State==Mood.Walk && w.InviteTime==0,"click dismisses attention and resumes the route");
        Console.WriteLine("PASS: "+checks+" patrol attention checks (prior greetings, return trips, retained routes, stillness/jitter, sizes/DPIs, edges and priority).");
    }
}
