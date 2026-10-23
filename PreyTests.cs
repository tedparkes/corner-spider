using System;
using System.Collections.Generic;
using CornerSpider;

static class PreyTests {
    static int checks;
    static readonly V away=new V(-10000,-10000);
    static void Check(bool ok,string why) { checks++;if(!ok) throw new Exception("Portia prey: "+why); }
    static World New(int seed) { var w=new World(3200,1800,seed);w.AutoFeed=w.CursorPlay=false;w.Spider=new V(1600,900);w.SetSpecies("PortiaLabiata");return w; }
    public static void Run() {
        int spiders=0,flies=0;
        for(int seed=0;seed<240;seed++) {
            var w=New(seed);w.SpawnPrey();if(w.Prey==PreyKind.SmallSpider) spiders++;else flies++;
            Check(w.HasFly && (w.Fly-w.Spider).Length>140 && (w.Fly-w.Spider).Length<541,"occasional prey uses existing near/far spawn range");
            V point=w.Fly;PreyKind kind=w.Prey;double next=w.NextFly;
            w.SpawnFly();w.SpawnSmallSpider();w.SpawnPrey();w.Feed();
            Check(w.Prey==kind && (w.Fly-point).Length==0 && w.NextFly==next,"repeated feeding preserves the one occupied prey slot and schedule");
            Check(next>=70 && next<=130,"new prey keeps existing timing variation");
        }
        Check(spiders>100 && flies>40,"Portia automatically receives both prey kinds across seeds");
        foreach(var skin in SkinCatalog.All) {
            var w=New(7);w.SetSpecies(skin.Id);w.Feed();
            Check(w.Prey==(skin.SpiderPrey?PreyKind.SmallSpider:PreyKind.Fly),"manual feeding matches species: "+skin.Id);
            w.HasFly=false;w.SpawnSmallSpider();Check(w.HasFly==skin.SpiderPrey,"only Portia can release spider prey");
            w.HasFly=false;w.NextFly=0;w.AutoFeed=true;w.State=Mood.Rest;w.Tick(.025,away);
            Check(w.HasFly && (skin.SpiderPrey || w.Prey==PreyKind.Fly),"automatic spawner respects selected species");
        }
        foreach(double dt in new[]{.016,.033,.1}) for(int seed=0;seed<32;seed++) {
            var w=New(seed);w.SpawnSmallSpider();var seen=new HashSet<Mood>();
            for(int f=0;f<50/dt && w.State!=Mood.Eat;f++) {
                seen.Add(w.State);w.Tick(dt,away);
                Check(w.InsideAny(w.Fly) && World.Finite(w.PreyAngle),"crawling prey stays finite and on a screen");
            }
            Check(w.State==Mood.Eat && w.Meals==1 && !w.HasFly && w.Meal==PreyKind.SmallSpider,"spider is caught once and remains the correct meal");
            foreach(Mood phase in new[]{Mood.Notice,Mood.Stalk,Mood.Aim,Mood.Reach,Mood.Crouch,Mood.Jump,Mood.Land}) Check(seen.Contains(phase),"spider hunt includes "+phase);
            w.SetSpecies("Bold");w.SpawnFly();Check(w.Prey==PreyKind.Fly && w.Meal==PreyKind.SmallSpider,"switching skins or spawning the next prey does not transform the held meal");
        }
        foreach(double scale in new[]{.2,1,1.8}) foreach(double dpi in new[]{1,1.5,2}) {
            var w=New(8);w.ConfigureDisplays(new[]{new Area(0,0,3200,1800,dpi)},0);w.SetScale(scale);w.Spider=new V(1600,900);w.SpawnSmallSpider();
            w.State=Mood.Sleep;w.StateTime=0;int moves=0,pauses=0;V start=w.Fly;
            for(int f=0;f<240;f++) {
                V before=w.Fly;double phase=w.PreySteps;w.Tick(.025,away);double distance=(w.Fly-before).Length;
                Check(distance<=12.5*.025*scale*dpi+.001,"spider prey crawls at a bounded DPI-scaled speed");
                if(distance>.001) moves++;else {pauses++;Check(w.PreySteps==phase,"stationary prey does not slide its feet");}
                Check(w.InsideAny(w.Fly) && (w.Fly-start).Length<35*scale*dpi,"small local excursions stay near the sighting");
            }
            Check(moves>20 && pauses>60,"spider prey visibly alternates crawling and resting at every size/DPI");
            w.SetSpecies("Regal");Check(w.Prey==PreyKind.SmallSpider && w.HasFly,"skin switching preserves live prey identity");
            w.State=Mood.Crouch;V held=w.Fly;w.Tick(.025,away);Check((w.Fly-held).Length<.001 && !w.PreyWalking,"target holds during the final pounce sequence");
            w.State=Mood.Stalk;w.FlyAge=60;w.Tick(.025,away);Check(!w.HasFly,"uncaught spider expires without an orphan");
        }
        var disabled=New(1);disabled.NextFly=0;for(int f=0;f<100;f++) disabled.Tick(.025,away);Check(!disabled.HasFly,"occasional-prey toggle still suppresses all automatic prey");
        disabled.Feed();disabled.GoHome();Check(!disabled.HasFly,"returning home clears spider prey");
        var layout=New(5);layout.ConfigureDisplays(new[]{new Area(0,0,1600,1000,1),new Area(1600,0,1800,1200,1.5)},1);layout.Spider=new V(2400,600);layout.SpawnSmallSpider();layout.ConfigureDisplays(new[]{new Area(0,0,1600,1000,1)},0);
        for(int f=0;f<100;f++) {layout.Tick(.025,away);Check(layout.InsideAny(layout.Fly),"prey remains on a display after monitor removal");}
        Console.WriteLine("PASS: "+checks+" Portia prey checks (mixed spawn, species routing, single slot, full hunts, crawl/pause, held meal, size/DPI and cleanup).");
    }
}
