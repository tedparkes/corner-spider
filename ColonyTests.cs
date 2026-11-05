using System;
using System.Collections.Generic;
using System.IO;
using CornerSpider;
static class ColonyTests {
    static int checks;
    static void Check(bool ok,string why) { checks++;if(!ok) throw new Exception("Colony: "+why); }
    static readonly V away=new V(-10000,-10000);
    static void Brood(World w) { w.Life.Stage=LifeStage.Brood;w.Life.StageAge=World.BroodSeconds-.05;w.Life.Babies=6;w.Tick(.1,away); }
    public static void Run() {
        var w=new World(1920,1080,42) { AutoFeed=false,CursorPlay=false,MoltingEnabled=false,MatingEnabled=false };Brood(w);Check(w.Residents.Count==0 && w.Life.Babies==6,"existing dispersal remains the default");
        w.SetKeepBabies(true);Brood(w);Check(w.Residents.Count==6 && w.Life.Babies==0,"six young establish their own homes when enabled");
        var starts=new List<V>();foreach(var p in w.Residents) starts.Add(p.Actor.Spider);
        for(int tick=0;tick<1800;tick++) w.Tick(.1,away);
        for(int i=0;i<w.Residents.Count;i++) { var pet=w.Residents[i];Check((pet.Actor.Spider-starts[i]).Length>40,"residents travel independently from the original nest");Check((pet.Actor.Home-w.Home).Length>=260*w.HomeScale,"resident homes avoid the main nest");Check(pet.Actor.Web>.4 && !pet.Actor.AutoFeed && !pet.Actor.CursorPlay && !pet.Actor.MatingEnabled,"residents build silk without multiplying prey or cursor owners"); }
        string first=w.Residents[0].Record.Species;w.SetSpecies("BrownWidow");Check(w.Residents[0].Record.Species==first,"changing the main species does not repaint descendants");
        Brood(w);Check(w.Residents.Count==6 && w.Life.Babies==6,"a full colony sends additional young away");
        w.Corner="TopLeft";w.UpdateHome();foreach(var pet in w.Residents) Check(pet.Record.Leaving || (pet.Actor.Home-w.Home).Length>=260*w.HomeScale,"moving the main nest relocates conflicting resident homes");
        var settings=new Settings { KeepBabies=true,ResidentLimit=6,AutoUpdateCheck=true,PreviewUpdates=true,LastUpdateCheckTicks=DateTime.UtcNow.Ticks };foreach(var p in w.Residents) settings.Colonists.Add(p.Record);
        string dir=Path.Combine(Path.GetTempPath(),"CornerSpider-colony-"+Guid.NewGuid().ToString("N"));try {
            Check(settings.Save(dir),"colony saves");var loaded=Settings.Load(dir);Check(loaded.KeepBabies && loaded.Colonists.Count==6 && loaded.Colonists[0].Pack()==settings.Colonists[0].Pack(),"species, homes, age and growth round-trip");Check(loaded.AutoUpdateCheck && loaded.PreviewUpdates && loaded.LastUpdateCheckTicks==settings.LastUpdateCheckTicks,"update preferences and check time round-trip");
            var restored=new World(1920,1080,44) { KeepBabies=loaded.KeepBabies };restored.RestoreResidents(loaded.Colonists);Check(restored.Residents.Count==6,"saved colony reappears on launch");
        } finally { if(Directory.Exists(dir)) Directory.Delete(dir,true); }
        foreach(var p in w.Residents) p.Record.Age=7200;w.Tick(.1,away);foreach(var p in w.Residents) Check(Math.Abs(p.Actor.Scale-w.Scale*.65)<.001,"resident growth reaches a bounded adult size");
        w.ConfigureDisplays(new[]{new Area(0,0,1280,720,1),new Area(1280,-180,1600,1000,1.5)},1);w.SetScale(.2);w.Tick(.1,away);
        foreach(var p in w.Residents) Check(w.InsideAny(p.Actor.Home) && p.Actor.Scale<.2,"tiny residents preserve their homes and relative size after display changes");
        w.SetScale(1);w.SetResidentLimit(2);int leaving=0;foreach(var p in w.Residents) if(p.Record.Leaving) leaving++;Check(leaving>=4,"lowering the limit sends excess residents away");
        w.SetKeepBabies(false);for(int i=0;i<650;i++) w.Tick(.1,away);Check(w.Residents.Count==0,"turning the option off clears residents and nests after graceful departures");
        Check(ColonyRecord.Unpack("Unknown,0,0,.5,20,0,0")==null && ColonyRecord.Unpack("Regal,0,0,NaN,20,0,0")==null,"invalid saved residents are ignored");
        var cap=new World(5000,3000,2) { KeepBabies=true,ResidentLimit=12,AutoFeed=false,CursorPlay=false };for(int i=0;i<10;i++) Brood(cap);Check(cap.Residents.Count==12,"multiple broods cannot exceed the hard cap");
        Console.WriteLine("PASS: "+checks+" colony checks (adoption, separate homes, movement, growth, capacity, saved residents, monitor changes and graceful departure).");
    }
}
