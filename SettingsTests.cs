using System;
using System.IO;
using CornerSpider;
static class SettingsTests {
    static int checks;
    static void Check(bool condition,string why) { checks++;if(!condition) throw new Exception("Settings: "+why); }
    public static void Run() {
        Check(SkinCatalog.All.Length==20,"twenty skin profiles");
        var ids=new System.Collections.Generic.HashSet<string>();
        var oldSave=new Settings();oldSave.Parse(new[]{"version=3","web=.76","meals=12","species=Bold"});
        Check(oldSave.FingerTone==World.DefaultFingerTone && oldSave.Web==.76 && oldSave.Meals==12 && oldSave.Species=="Bold","old saves keep the original finger tone and pet progress");
        foreach(string value in new[]{"NaN","Infinity","-Infinity","invalid",""}) {
            var invalidTone=new Settings();invalidTone.Parse(new[]{"fingerTone="+value});Check(invalidTone.FingerTone==World.DefaultFingerTone,"invalid finger tone keeps safe default");
        }
        var clippedTone=new Settings();clippedTone.Parse(new[]{"fingerTone=-4"});Check(clippedTone.FingerTone==0,"dark end clamps");clippedTone.Parse(new[]{"fingerTone=4"});Check(clippedTone.FingerTone==1,"pale end clamps");
        foreach(var skin in SkinCatalog.All) { Check(ids.Add(skin.Id),"unique skin ID");var s=new Settings();s.Parse(new[]{"species="+skin.Id});Check(s.Species==skin.Id,"every skin can be loaded"); }
        foreach(double size in new[]{.2,.35,.7,1,1.8}) foreach(double dpi in new[]{1,1.5,2}) foreach(double delay in new[]{2,4.5,10}) {
            var w=new World(4000,2000,42);w.ConfigureDisplays(new[]{new Area(0,0,4000,2000,dpi)},0);w.SetScale(size);w.AutoFeed=false;w.InviteDelay=delay;w.Spider=new V(1600,800);
            V cursor=w.Spider+new V(120*dpi,0);double start=w.Clock;bool appeared=false;
            for(int i=0;i<800 && w.State!=Mood.Ride;i++) {
                w.Tick(.025,cursor+new V(Math.Sin(i*.5),Math.Cos(i*.4))*dpi);
                if(w.FingerBlend>0 && !appeared) { appeared=true;Check(w.Clock-start>=delay,"finger never appears before the selected delay");Check(w.Clock-start<delay+.3,"finger appears promptly after the delay"); }
            }
            Check(w.State==Mood.Ride,"tiny/large pet accepts long/short invitation at mixed DPI");
            w.Tick(.025,cursor,true);Check(w.State==Mood.Dismount && w.LastDismount=="click","tiny/large pet plans a safe departure");
            for(int i=0;i<80 && w.State==Mood.Dismount;i++) w.Tick(.025,cursor);
            Check(w.State==Mood.Jump,"tiny/large pet completes planned departure");
            for(int i=0;i<80;i++) w.Tick(.025,new V(-1000,-1000));Check(w.FingerBlend==0,"finger fades after departure");
            Check(w.Scale==size,"new scale bound preserves requested size");
        }
        foreach(double size in new[]{.2,1,1.8}) {
            var w=new World(1600,1000,1);w.SetScale(size);w.AutoFeed=false;w.Spider=new V(700,400);w.State=Mood.Walk;w.Target=new V(1200,400);w.AttentionDistance=100;w.DismissDistance=180;
            w.Tick(.025,new V(850,400));Check(w.State==Mood.Walk,"outside approach range does not interrupt");
            w.Tick(.025,new V(780,400));Check(w.State==Mood.Inspect,"configured approach radius interrupts at every size");
            w.Tick(.025,new V(850,400));Check(w.State==Mood.Inspect,"dismissal distance provides hysteresis");
            w.Tick(.025,new V(910,400));Check(w.State==Mood.Walk,"leaving dismissal radius resumes route");
        }
        foreach(double tolerance in new[]{300,850,1500}) {
            var w=new World(3000,1600,42);w.AutoFeed=false;w.Spider=new V(1200,600);w.InviteDelay=2;w.CarryTolerance=tolerance;V point=new V(1300,600);
            for(int i=0;i<400 && w.State!=Mood.Ride;i++) w.Tick(.025,point);
            Check(w.State==Mood.Ride,"boards for tolerance check");point+=new V(tolerance*.5*.025,0);w.Tick(.025,point);Check(w.State==Mood.Ride,"motion below chosen threshold remains gentle");
            point+=new V((tolerance+100)*.025,0);w.Tick(.025,point);Check(w.LastDismount=="fast movement","motion above chosen threshold dismounts");
        }
        foreach(double interval in new[]{30,100,300}) { var w=new World(1280,720,1);w.FlyInterval=interval;w.SpawnFly();Check(w.NextFly>=interval*.7 && w.NextFly<=interval*1.3,"fly timing obeys configured interval");V fly=w.Fly;w.SpawnFly();Check((w.Fly-fly).Length==0,"new settings retain one-prey limit"); }
        var parsed=new Settings();parsed.Parse(new[]{"scale=.2","inviteDelay=10","attentionDistance=400","dismissDistance=80","flyInterval=300","carryTolerance=1500"});
        Check(parsed.Scale==.2 && parsed.InviteDelay==10 && parsed.DismissDistance==420,"20 percent, long delay and safe hysteresis parse");
        var bad=new Settings();bad.Parse(new[]{"scale=-4","inviteDelay=Infinity","attentionDistance=NaN","flyInterval=0","carryTolerance=-10"});Check(bad.Scale==.2 && bad.InviteDelay==4.5 && bad.AttentionDistance==185 && bad.FlyInterval==30 && bad.CarryTolerance==300,"numeric validation clamps ranges and rejects nonfinite values");
        string folder=Path.Combine(Path.GetTempPath(),"CornerSpider-settings-test-"+Guid.NewGuid().ToString("N"));
        try {
            parsed.Species="Emerald";Check(parsed.Save(folder),"save new preferences");var loaded=Settings.Load(folder);Check(loaded.Scale==.2 && loaded.InviteDelay==10 && loaded.AttentionDistance==400 && loaded.DismissDistance==420 && loaded.Species=="Emerald" && loaded.FlyInterval==300 && loaded.CarryTolerance==1500,"all new preferences survive restart");
            foreach(var skin in SkinCatalog.All) { parsed.Species=skin.Id;Check(parsed.Save(folder),"save species "+skin.Id);Check(Settings.Load(folder).Species==skin.Id,"reload species "+skin.Id); }
            parsed.Species="Emerald";
            foreach(double tone in new[]{0,.01,.37,.8,.99,1}) { parsed.FingerTone=tone;Check(parsed.Save(folder),"save finger shade");loaded=Settings.Load(folder);Check(loaded.FingerTone==tone && loaded.Species=="Emerald" && loaded.Scale==.2,"exact finger shade survives reload alongside existing preferences"); }
        }
        finally { foreach(string file in new[]{"settings.txt","settings.txt.bak","settings.txt.new"}) { string path=Path.Combine(folder,file);if(File.Exists(path)) File.Delete(path); }if(Directory.Exists(folder)) Directory.Delete(folder); }
        Console.WriteLine("PASS: "+checks+" settings checks (twenty skins, finger tones and migration, 20% size, configured delay/distance/tolerance, mixed DPI and persistence).");
    }
}
