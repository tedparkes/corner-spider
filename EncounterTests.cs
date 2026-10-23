using System;
using System.Collections.Generic;
using System.IO;
using CornerSpider;

static class EncounterTests {
    static int checks,complete;
    static readonly V away=new V(-10000,-10000);
    static void Check(bool ok,string message) { checks++;if(!ok) throw new Exception("Encounters: "+message); }
    static World New(int seed,double size,double dpi) {
        double s=size*dpi;var w=new World(2400*s,1600*s,seed);w.ConfigureDisplays(new[]{new Area(0,0,2400*s,1600*s,dpi)},0);w.SetScale(size);
        w.Corner=Settings.Corners[seed%4];w.UpdateHome();w.Spider=w.Home;w.SetSpecies("PortiaLabiata");w.AutoFeed=w.CursorPlay=false;return w;
    }
    public static void Run() {
        foreach(double size in new[]{.2,1,1.8}) foreach(double dt in new[]{.033,.1}) for(int seed=0;seed<8;seed++) {
            var w=New(seed,size,new[]{1,1.5,2}[seed%3]);Check(w.StartWebVisit(),"Portia can invite a web builder");
            Check(w.GuestBuildTime>=180 && w.GuestBuildTime<=240 && w.GuestWaitTime>=60 && w.GuestWaitTime<=120,"real build and post-build wait durations");
            Check((w.GuestCenter-w.Home).Length>320*w.HomeScale,"guest web is separate from the hammock");
            V fly=w.Fly;w.SpawnFly();w.SpawnSmallSpider();w.Feed();Check(w.Prey==PreyKind.SmallSpider && (w.Fly-fly).Length==0 && !w.StartWebVisit(),"web resident exclusively occupies the prey slot");
            var seen=new HashSet<WebVisit>();double approach=-1,pluck=-1;bool ignoredProbe=false;V lastFly=w.Fly;
            for(int frame=0;frame<850/dt && w.Meals==0;frame++) {
                w.Tick(dt,away);seen.Add(w.WebStage);
                Check(w.InsideAny(w.Spider) && w.InsideAny(w.Fly) && World.Finite(w.GuestProgress),"pet and builder remain finite on the desktop");
                Check(w.Meals==0 || w.WebStage==WebVisit.Fading,"no premature capture while web is being built");
                Check((w.Fly-lastFly).Length<35*w.GuestScale,"resident walks continuously between threads and into the lure");lastFly=w.Fly;
                if(w.GuestPassive) Check(w.State!=Mood.Notice && w.State!=Mood.Stalk && w.State!=Mood.Aim,"pet leaves the building/resting resident alone, including after its nap");
                if(w.WebStage==WebVisit.Approach && approach<0) approach=w.Clock;
                if(w.WebStage==WebVisit.Plucking) {
                    if(pluck<0) pluck=w.Clock;
                    if(w.GuestTaps>0 && w.GuestTaps<4) { Check(w.GuestLure==0,"initial probes do not immediately summon resident");ignoredProbe=true; }
                    if(w.GuestLure>0) Check(w.GuestTaps>=4,"resident responds only after exploratory signals");
                }
            }
            Check(w.Meals==1 && w.Meal==PreyKind.SmallSpider && !w.HasFly,"complete web encounter captures exactly one resident, seed "+seed+" size "+size+" state "+w.State+" visit "+w.WebStage);
            foreach(WebVisit state in new[]{WebVisit.Building,WebVisit.Waiting,WebVisit.Approach,WebVisit.Plucking,WebVisit.Attack,WebVisit.Fading}) Check(seen.Contains(state),"full encounter includes "+state);
            Check(approach>=w.GuestBuildTime+w.GuestWaitTime && pluck>approach && ignoredProbe && w.GuestTaps>=8,"quiet wait, approach and signal trial sequence are distinct");
            for(int i=0;i<14/dt;i++) w.Tick(dt,away);
            Check(w.WebStage==WebVisit.None && w.GuestOpacity==0,"spent guest web fades and releases resources");complete++;
        }
        // Real scheduler, without forcing mood or bypassing the multi-minute timers.
        var auto=New(2,1,1);auto.AutoFeed=true;auto.NextFly=9999;auto.NextWebVisit=1;
        for(int i=0;i<7000 && auto.Meals==0;i++) auto.Tick(.1,away);
        Check(auto.Meals==1 && auto.Meal==PreyKind.SmallSpider,"automatic Portia visit finishes through ordinary rests and naps");
        var gap=New(1,1,1);gap.Corner="TopLeft";gap.ConfigureDisplays(new[]{new Area(0,0,1600,1000,1),new Area(1700,0,1600,1000,1)},0);gap.Spider=new V(1500,200);
        Check(gap.StartWebVisit() && gap.GuestArea==1,"a nearby guest can use a different monitor corner");
        for(int i=0;i<8500 && gap.Meals==0;i++) gap.Tick(.1,away);
        Check(gap.Meals==1 && gap.Meal==PreyKind.SmallSpider,"sneaking resumes after a deliberate hop across a real monitor gap");
        foreach(string skin in Settings.SpeciesNames) {
            var w=New(1,1,1);w.SetSpecies(skin);Check(w.StartWebVisit()==w.HuntsSpiders,"web visit is Portia-specific: "+skin);
            if(w.HuntsSpiders) w.CancelWebVisit();
            Check(BehaviorCatalog.Get(skin).Title.Length>0 && BehaviorCatalog.Get(skin).Note.Length>0,"all skins have an explained personality");
            w.State=Mood.Rest;w.Spider=w.Home;Check(w.StartSpeciesMoment(),"personality display starts for "+skin);
            double initial=w.Angle,maxTurn=0,maxReach=0,moved=0;V start=w.Spider;
            for(int i=0;i<300 && w.State==Mood.SpeciesAct;i++) { w.Tick(.025,away);maxTurn=Math.Max(maxTurn,Math.Abs(World.AngleDelta(initial,w.Angle)));maxReach=Math.Max(maxReach,w.FrontReach(1));moved=Math.Max(moved,(w.Spider-start).Length);Check(w.InsideAny(w.Spider),"personality stays on desktop"); }
            Check(w.State==Mood.Rest,"each personality gesture finishes");
            if(w.Personality.Gesture==SpeciesGesture.Zigzag) Check(moved>5 && maxReach>.4,"red-backed male actually sidesteps and raises forelegs");
            if(w.Personality.Gesture==SpeciesGesture.Drum) Check(maxReach>.5,"male display actually waves its forelegs");
            w.SpeciesTraits=false;Check(!w.StartSpeciesMoment() && w.MealDuration==4,"personality toggle restores shared behavior");
        }
        var disabled=New(4,1,1);disabled.AutoFeed=false;disabled.NextWebVisit=0;for(int i=0;i<1000;i++) disabled.Tick(.1,away);Check(disabled.WebStage==WebVisit.None,"automatic prey switch disables automatic web visits");
        disabled.WebVisits=false;Check(!disabled.StartWebVisit(),"web preference disables manual invitation");
        foreach(WebVisit phase in new[]{WebVisit.Building,WebVisit.Waiting,WebVisit.Approach,WebVisit.Plucking,WebVisit.Attack}) foreach(string interruption in new[]{"skin","home","display","size","toggle"}) {
            var w=New(3,1,1);w.StartWebVisit();for(int i=0;i<8000 && w.WebStage!=phase;i++) w.Tick(.1,away);
            Check(w.WebStage==phase,"reaches interruption fixture "+phase);
            if(interruption=="skin") w.SetSpecies("Bold");else if(interruption=="home") w.GoHome();else if(interruption=="display") w.ConfigureDisplays(new[]{new Area(0,0,1600,1000,1)},0);else if(interruption=="size") w.SetScale(.2);else { w.WebVisits=false;w.Tick(.025,away); }
            Check(w.WebStage==WebVisit.None && !w.HasFly && w.GuestOpacity==0,"interruption clears encounter without orphan: "+phase+" / "+interruption);
            Check(w.State!=Mood.WebApproach && w.State!=Mood.WebPluck,"interruption releases exclusive encounter state");
        }
        var play=New(1,1,1);play.Spider=new V(1100,800);play.CursorPlay=true;play.StartWebVisit();V cursor=play.Spider+new V(110,0);
        for(int i=0;i<500 && play.State!=Mood.Ride;i++) play.Tick(.025,cursor);
        Check(play.State==Mood.Ride && play.WebStage==WebVisit.Building,"finger play remains available while guest builds");
        double age=play.GuestAge;play.Tick(300,away);Check(play.GuestAge==age,"suspend-sized time step does not skip the build or wait");
        var tiny=New(2,1,1);tiny.ConfigureDisplays(new[]{new Area(0,0,300,250,1)},0);Check(!tiny.StartWebVisit() && !tiny.HasFly,"cramped desktop declines the guest instead of clipping");
        string folder=Path.Combine(Path.GetTempPath(),"CornerSpiderTraits-"+Guid.NewGuid().ToString("N"));
        try { var s=new Settings { SpeciesTraits=false,WebVisits=false };Check(s.Save(folder),"trait preferences save");var reload=Settings.Load(folder);Check(!reload.SpeciesTraits && !reload.WebVisits,"trait preferences reload"); }
        finally { if(Directory.Exists(folder)) Directory.Delete(folder,true); }
        Console.WriteLine("PASS: "+checks+" species/encounter checks, "+complete+" full-length web hunts (timing, original prey limit, all skins, poses, naps, finger play, cleanup and persistence).");
    }
}
