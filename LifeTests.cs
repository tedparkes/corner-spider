using System;
using System.IO;
using System.Collections.Generic;
using CornerSpider;

static class LifeTests {
    static int checks;
    static readonly V away=new V(-10000,-10000);
    static void Check(bool ok,string why) { checks++;if(!ok) throw new Exception("Living world: "+why); }
    static World Pet(string skin,int seed=42) { var w=new World(1800,1200,seed) { AutoFeed=false,CursorPlay=false };w.SetSpecies(skin);w.Spider=w.Home;return w; }
    static void Step(World w,double seconds) { for(int i=0;i<(int)Math.Ceiling(seconds*10);i++) w.Tick(.1,away); }
    public static void Run() {
        Check(SkinCatalog.All.Length==46 && SkinCatalog.Categories.Length==5,"complete expanded catalog");
        foreach(var s in SkinCatalog.All) {
            Check(s.MateId.Length>0,"every species has both sexes: "+s.Id);
            var mate=SkinCatalog.Get(s.MateId);Check(mate.Male!=s.Male && mate.Scientific==s.Scientific && mate.MateId==s.Id,"paired mate is the same species");
            Check(Array.IndexOf(SkinCatalog.InCategory(s.Category),s)>=0,"category contains its skins");
            foreach(PreyKind food in Enum.GetValues(typeof(PreyKind))) {
                var hunt=Pet(s.Id);hunt.Feed(food);Check(hunt.HasFly && hunt.Prey==food,"all seven menu foods spawn: "+s.Id+" "+food);
                V position=hunt.Fly;hunt.Feed(PreyKind.Fly);Check(hunt.Prey==food && (hunt.Fly-position).Length==0,"single prey slot retained");
                bool jumped=false;for(int i=0;i<600 && hunt.Meals==0;i++) { hunt.Tick(.1,away);jumped|=hunt.State==Mood.Jump; }
                Check(hunt.Meals==1 && hunt.HoldingMeal,"every food can be captured: "+s.Id+" "+food);
                Check(!s.WebDweller || !jumped,"web dwellers walk the silk to prey, without a hunting hop");
            }
        }
        var w=Pet("Regal");Check(w.LifeSize==1 && w.WebLayers==0,"new pet starts at selected size and fresh long-term web");
        w.Life.SilkSeconds=1800;Check(w.WebLayers==1,"first reinforcement after half an active hour near home");
        w.Life.SilkSeconds=1e8;Check(w.WebLayers==24,"long-lived web remains bounded");
        double age=w.Life.Age;w.Tick(600,away);Check(w.Life.Age==age,"sleep/resume gap does not fast-forward life events");
        w.Life.SilkSeconds=7200;w.Life.NextMolt=2;Step(w,1);Check(w.Life.Stage==LifeStage.Quiet,"molt never starts before due");
        Step(w,50);Check(w.Life.Molts==1 && w.Life.Stage==LifeStage.Recovering && w.LifeSize>1.15,"molt finishes with a visible controlled growth step");
        Check(Math.Abs(w.Scale-1)<.00001,"molt does not mutate the size preference");
        w.Feed(PreyKind.Cricket);Check(!w.HasFly,"new food waits during the vulnerable molt");
        Step(w,92);Check(w.Life.Stage==LifeStage.Quiet,"recovery completes");
        double larger=w.LifeSize;Step(w,10);Check(w.LifeSize<=larger && larger-w.LifeSize<.001,"size settles imperceptibly over a short interval");
        w.Life.Age=w.Life.GrowthAt+w.Life.GrowthSpan;Check(Math.Abs(w.LifeSize-1)<.000001,"returns exactly to baseline before another growth step");
        w.Life.NextMolt=w.Life.Age+99999;w.Life.NextMate=w.Life.Age+99999;
        w.Spider=w.Home;w.State=Mood.Rest;Check(w.InviteMate(),"female accepts a same-species visit when free");
        Step(w,29);Check(w.Life.Stage==LifeStage.Mating,"readable courtship precedes mating");
        double facing=w.Angle*Math.PI/180;
        V meeting=w.MatePosition-w.Home;Check(meeting.X*Math.Sin(facing)-meeting.Y*Math.Cos(facing)>0,"male reaches the female's face");
        Step(w,24);Check(w.Life.Stage==LifeStage.MaleLeaving || w.Life.Stage==LifeStage.MaleMeal,"male departure or cannibalism follows mating");
        Step(w,14);Check(w.Life.Stage==LifeStage.Gravid,"gravid development begins after the visit");
        w.Life.StageAge=World.GravidSeconds/2;Check(w.GravidAmount>.49 && w.GravidAmount<.51,"abdomen fills gradually");
        w.Life.StageAge=World.GravidSeconds;w.Spider=w.Home;w.State=Mood.Rest;Step(w,33);Check(w.Life.Stage==LifeStage.Incubating && w.HasSac,"female lays a persistent sac");
        w.Life.StageAge=World.IncubationSeconds-.3;Step(w,1);Check(w.Life.Stage==LifeStage.Brood && w.Life.Babies==6,"sac hatches a bounded brood");
        double tiny=w.BabySize;w.Life.StageAge=World.BroodSeconds-.3;Check(w.BabySize>tiny,"spiderlings grow before dispersing");Step(w,1);
        Check(w.Life.Stage==LifeStage.Dispersing,"older brood starts departure");w.Life.StageAge=World.DepartureSeconds-1;
        for(int i=0;i<6;i++) Check(!w.Areas[w.HomeArea].Contains(w.BabyPosition(i)),"each baby actually leaves the screen");
        Step(w,2);Check(w.Life.Babies==0 && w.Life.Stage==LifeStage.Quiet && w.Life.NextMate>w.Life.Age,"brood cleaned up and visits spaced apart");
        var male=Pet("Bold");Check(!male.InviteMate(),"male pets do not become gravid");
        var busy=Pet("Regal");busy.Feed();Check(!busy.InviteMate() && !busy.BeginMolt(),"feeding and life events do not overlap");
        bool eaten=false,left=false;
        for(int seed=1;seed<30;seed++) { var visitor=Pet("RedbackWidow",seed);Check(visitor.InviteMate(),"redback visit starts");Step(visitor,51);eaten|=visitor.Life.Stage==LifeStage.MaleMeal;left|=visitor.Life.Stage==LifeStage.MaleLeaving; }
        Check(eaten && left,"both mating outcomes occur across deterministic seeds");
        var fantasy=Pet("Nebula");fantasy.InviteMate();Step(fantasy,51);Check(fantasy.Life.Stage==LifeStage.MaleLeaving,"fictional male always departs");
        var saved=Pet("Regal");saved.Life.Age=4444;saved.Life.SilkSeconds=9000;saved.Life.Stage=LifeStage.Gravid;saved.Life.StageAge=700;
        saved.SetSpecies("BoldFemale");Check(saved.Life.Stage==LifeStage.Quiet,"a different skin does not inherit a pregnancy");saved.SetSpecies("Regal");Check(saved.Life.Stage==LifeStage.Gravid && saved.Life.StageAge==700,"switching back restores progress");
        string folder=Path.Combine(Path.GetTempPath(),"CornerSpider-life-"+Guid.NewGuid().ToString("N"));
        try { var settings=new Settings { MoltHours=12,MateHours=24 };foreach(var pair in saved.Lives) settings.Lives[pair.Key]=pair.Value;
            Check(settings.Save(folder),"life save succeeds");var loaded=Settings.Load(folder);
            Check(loaded.Lives["Regal"].Pack()==saved.Life.Pack() && loaded.MoltHours==12 && loaded.MateHours==24,"all life state and intervals survive reload exactly");
            loaded.Parse(new[]{"life.Regal=NaN","life.Unknown="+saved.Life.Pack(),"moltHours=NaN","mateHours=Infinity"});Check(loaded.Lives.Count==2 && loaded.MoltHours==12,"invalid life input cannot corrupt a valid save");
        } finally { foreach(string name in new[]{"settings.txt","settings.txt.bak","settings.txt.new"}) { string p=Path.Combine(folder,name);if(File.Exists(p)) File.Delete(p); }if(Directory.Exists(folder)) Directory.Delete(folder); }
        foreach(LifeStage stage in Enum.GetValues(typeof(LifeStage))) { var l=new LifeRecord { Stage=stage,Age=500,StageAge=12,Babies=stage>=LifeStage.Brood?6:0 };Check(LifeRecord.Unpack(l.Pack()).Pack()==l.Pack(),"every event resumes from serialized state: "+stage); }
        var oldPet=Pet("Regal");oldPet.Life.Age=900000;oldPet.Life.GrowthAt=1;oldPet.SetLifeIntervals(12,24);
        Check(oldPet.LifeSize==1 && LifeRecord.Unpack(oldPet.Life.Pack())!=null,"changing a long-idle molt interval neither enlarges the pet nor invalidates its save");
        var screens=Pet("Regal");screens.ConfigureDisplays(new[]{new Area(-1600,0,1600,900,1),new Area(0,0,1920,1080,1.5),new Area(0,-1000,1920,1000,1)},1);
        foreach(string corner in Settings.Corners) { screens.Corner=corner;screens.UpdateHome();screens.Life.Stage=LifeStage.Dispersing;screens.Life.StageAge=World.DepartureSeconds-1;
            for(int i=0;i<6;i++) Check(!screens.InsideAny(screens.BabyPosition(i)),"young leave all monitors, including an internal nest corner");
        }
        var longLife=Pet("Regal");var phases=new HashSet<LifeStage>();
        for(int frame=0;frame<1728000;frame++) {
            longLife.Tick(.1,away);phases.Add(longLife.Life.Stage);
            if(frame%1000==0) { Check(longLife.LifeSize>=1 && longLife.LifeSize<=1.160001 && longLife.Scale==1,"48-hour run keeps size bounded");Check(longLife.Life.Babies<=6 && World.Finite(longLife.Spider.X) && World.Finite(longLife.Spider.Y),"48-hour run keeps population and geometry bounded"); }
        }
        Check(longLife.Life.Molts>=5 && longLife.Life.Broods>=3,"48 active hours complete repeated molts and broods");
        Check(phases.Contains(LifeStage.Courting) && phases.Contains(LifeStage.Gravid) && phases.Contains(LifeStage.Incubating) && phases.Contains(LifeStage.Brood) && phases.Contains(LifeStage.Dispersing),"full schedules naturally reach every family phase");
        Console.WriteLine("PASS: "+checks+" life/family/food checks (46 skins, 7 foods, 5 categories, full molt and brood cycle, both mating outcomes, per-skin saves and bounded growth).");
    }
}
