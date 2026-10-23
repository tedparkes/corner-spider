using System;
using CornerSpider;
class BehaviorTests {
    static void Assert(bool ok,string message) { if(!ok) throw new Exception(message); }
    static V away=new V(-2000,-2000);
    static void Advance(World w,double seconds,V cursor) { for(int i=0;i<(int)(seconds*40);i++) w.Tick(.025,cursor); }
    static World New() { var w=new World(1280,720,42); w.AutoFeed=false; return w; }
    static void Main() {
        try {
            var w=New(); Assert(w.Home.X>1000 && w.Home.Y<180,"Default corner with end-entrance clearance");
            Assert(w.Areas[0].Contains(w.NestEntrance) && w.Areas[0].Contains(w.NestExit) && (w.NestEntrance-w.Home).Length>90,"Both nest ends have room for a full approach");
            foreach(string corner in new string[]{"TopLeft","TopRight","BottomLeft","BottomRight"}) {
                w.Corner=corner; w.UpdateHome(); w.GoHome(); Advance(w,50,away);
                Assert(w.Spider.X>=0 && w.Spider.X<=w.Width && w.Spider.Y>=0 && w.Spider.Y<=w.Height,"Corner bounds");
            }
            w=New(); w.Spider=new V(600,350); w.SpawnFly(); Advance(w,12,away);
            Assert(!w.HasFly && w.Meals==1,"Nearby fly should be caught once");
            w=New(); w.Spider=new V(600,350); w.State=Mood.Groom;
            w.StillTime=5; w.LastCursor=new V(650,350); bool boarded=false;
            for(int i=0;i<1200;i++) { w.Tick(.025,new V(650,350)); if(w.State==Mood.Ride) { boarded=true; break; } }
            Assert(boarded,"Still nearby cursor should allow boarding");
            w.Tick(.025,new V(1000,350)); Assert(w.State==Mood.Jump,"Fast cursor should dismount");
            w=New(); w.CursorPlay=false; w.Spider=new V(600,350);
            Advance(w,60,new V(650,350)); Assert(w.State!=Mood.Ride,"Disabled cursor rides");
            w=New(); w.Spider=w.Home; w.GoHome(); Advance(w,.2,away);
            Assert(w.State==Mood.Sleep,"Home should become sleep"); Advance(w,21,away); Assert(w.State!=Mood.Sleep,"Nap should end");
            w=New(); w.AutoFeed=true;
            for(int i=0;i<24000;i++) {
                if(i%1700==0) { w.Scale=(i%3400==0)?1.8:.7; w.Resize(900,600); }
                w.Tick(.025,away);
                Assert(!double.IsNaN(w.Spider.X) && !double.IsInfinity(w.Spider.Y),"Finite position");
                Assert(w.Spider.X>=0 && w.Spider.X<=w.Width && w.Spider.Y>=0 && w.Spider.Y<=w.Height,"Long run bounds");
            }
            Assert(w.Meals>0,"Automatic feeding should work");
            BehaviorRegressionTests.Run();
            NaturalMotionTests.Run();
            PounceTests.Run();EncounterTests.Run();
            PlayTests.Run();
            PatrolAttentionTests.Run();SettingsTests.Run();MorphologyTests.Run();PreyTests.Run();HammockTests.Run();DismountTests.Run();NestEntryTests.Run();
            Console.WriteLine("PASS: corners, hunting, cursor boarding, fast dismount, disabled rides, naps, 10-minute simulation.");
        } catch(Exception ex) { Console.Error.WriteLine("FAIL: "+ex.Message); Environment.Exit(1); }
    }
}
