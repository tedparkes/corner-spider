using System;
using CornerSpider;

static class DismountTests {
    static int checks;
    static readonly V away=new V(-10000,-10000);
    static void Check(bool value,string why) { checks++;if(!value) throw new Exception("Click dismount: "+why); }
    static World Riding(double size,double dpi,int direction,double age,V origin) {
        var w=new World(4000,2400,63);w.AutoFeed=false;w.ConfigureDisplays(new[]{new Area(0,0,4000,2400,dpi)},0);w.SetScale(size);
        w.State=Mood.Ride;w.RideAge=age;w.FingerBlend=1;w.FingerDirection=direction;w.FingerOrigin=origin;w.FingerPosition=65;
        w.Spider=age<1.8?w.CursorLanding(origin):age<2.8?(w.CursorLanding(origin)+w.FingerPoint(65))*.5:w.FingerPoint(65);
        w.Angle=direction>0?90:270;return w;
    }
    static void Complete(World w,V origin,double dt,bool roomAbove) {
        V start=w.Spider;w.Tick(dt,origin,true);
        Check(w.State==Mood.Dismount && w.LastDismount=="click" && w.Scared==0,"click enters intentional preparation without fear");
        Check((w.Spider-start).Length<.001,"click never teleports the departure point");
        Check(!w.FingerActive,"native pointer can reappear immediately after click");
        V destination=w.JumpEnd;double scale=w.BodyScale;
        Check(destination.Y<=start.Y+.001,"planned landing is never below the original perch");
        if(roomAbove) Check(destination.Y<origin.Y-100*scale,"roomy landing is above the entire finger");
        bool braced=false;int count=0;V last=w.Spider;
        while(w.State==Mood.Dismount && count++<Math.Ceiling(2/dt)) {
            // Pointer motion and further clicks must not drag the departure pad,
            // retarget the committed jump, or restart its preparation timer.
            w.Tick(dt,new V(origin.X+count*60,origin.Y+120),count%3==0);
            Check((w.FingerOrigin-origin).Length<.001 && (w.JumpEnd-destination).Length<.001,"click freezes the offered pad and planned landing despite pointer movement");
            Check(w.FingerBlend>.99,"finger remains solid through preparation");
            Check((w.Spider-last).Length<=400*scale*dt+.01,"climbing onto the launch surface is continuous");last=w.Spider;
            braced|=w.DismountBrace>.1;
        }
        Check(w.State==Mood.Jump && braced,"turn and brace finish before takeoff");
        double face=Math.Atan2(destination.Y-w.JumpStart.Y,destination.X-w.JumpStart.X)*180/Math.PI+90;
        Check(Math.Abs(World.AngleDelta(w.Angle,face))<8,"takeoff faces its chosen landing point");
        Check(w.JumpDuration>=.32 && w.JumpDuration<=.42 && w.SilkOpacity==1,"calm upward arc has readable timing and a silk tether");
        V launch=w.JumpStart;
        for(int i=0;i<Math.Ceiling(.6/dt) && w.State==Mood.Jump;i++) {
            w.Tick(dt,away);
            Check(w.Spider.Y-w.Lift<=launch.Y+.001,"visible jump goes upward or stays level at the ceiling");
            var area=w.Areas[w.AreaAt(w.Spider)];
            Check(w.Spider.Y-w.Lift>=area.Y+SpiderPose.Radius*w.BodyScale-.01,"jump arc keeps the whole pose below the top boundary");
            Check(w.InsideAny(w.Spider) && (w.Clamp(w.Spider)-w.Spider).Length<.01,"airborne body stays on safe desktop coordinates");
        }
        Check(w.State==Mood.Land && (w.Spider-destination).Length<.001,"lands exactly at its planned point");
        for(int i=0;i<Math.Ceiling(1.5/dt);i++) w.Tick(dt,away);
        Check(w.FingerBlend==0 && w.SilkOpacity==0 && w.State!=Mood.Dismount && w.State!=Mood.Ride,"finger and silk clean up after landing");
    }
    public static void Run() {
        foreach(double size in new[]{.2,1,1.8}) foreach(double dpi in new[]{1,1.5,2}) foreach(double dt in new[]{.016,.033,.1}) foreach(int side in new[]{-1,1}) foreach(double age in new[]{.5,2.3,6.0}) {
            V origin=new V(2000,1200);Complete(Riding(size,dpi,side,age,origin),origin,dt,true);
        }
        foreach(double size in new[]{.2,1,1.8}) foreach(double dpi in new[]{1,2}) foreach(int side in new[]{-1,1}) {
            double s=size*dpi;
            foreach(double x in new[]{200*s,2000.0,4000-200*s}) {
                V origin=new V(x,100*s);var w=Riding(size,dpi,side,6,origin);
                Complete(w,origin,.025,false);
            }
            V top=new V(2000,73*s);var ceiling=Riding(size,dpi,side,6,top);ceiling.Spider=ceiling.Clamp(ceiling.Spider);
            V before=ceiling.Spider;ceiling.Tick(.025,top,true);
            Check(Math.Abs(ceiling.JumpEnd.X-before.X)>100*s && ceiling.JumpEnd.Y<=before.Y+.001,"ceiling fallback is a purposeful side hop, never a drop");
        }
        foreach(string interrupt in new[]{"resize","resume","disable","home","feed"}) {
            V origin=new V(2000,1200);var w=Riding(1,1,1,6,origin);w.Tick(.025,origin,true);
            if(interrupt=="resize") { w.ConfigureDisplays(new[]{new Area(0,0,1280,900,1.5)},0);Check(w.State==Mood.Rest && w.FingerBlend==0,"display changes cancel stale preparation"); }
            if(interrupt=="resume") { w.Resume();Check(w.State==Mood.Jump && w.FingerBlend==0,"resume safely releases pending departure"); }
            if(interrupt=="disable") { w.CursorPlay=false;w.Resume();Check(w.State==Mood.Jump && w.FingerBlend==0,"disabling interaction releases pending departure"); }
            if(interrupt=="home") { w.GoHome();Check(w.State==Mood.Return && w.FingerBlend==0,"home command replaces pending departure"); }
            if(interrupt=="feed") { w.SpawnFly();Check(w.State==Mood.Dismount && w.HasFly,"feeding does not interrupt preparation midair"); }
        }
        // A genuine mixed-DPI ride, rather than just a world initialized at that DPI.
        var seam=new World(3200,1600,42);seam.ConfigureDisplays(new[]{new Area(0,0,1280,900,1),new Area(1280,0,1920,1600,2)},1);seam.AutoFeed=false;seam.Spider=new V(1050,400);V cursor=new V(1120,400);
        for(int i=0;i<500 && !seam.OnFinger;i++) seam.Tick(.025,cursor);
        Check(seam.OnFinger,"boards for mixed-DPI departure test");
        for(int i=0;i<220;i++) { cursor+=new V(2,0);seam.Tick(.025,cursor); }
        Check(seam.OnFinger && seam.BodyScale>1.99,"ride reaches second monitor before click");
        Complete(seam,cursor,.025,true);
        Console.WriteLine("PASS: "+checks+" planned-dismount checks (upward targets, turn/brace, catch/climb exits, frozen pad, ceiling/edges, silk, interruption and mixed DPI).");
    }
}
