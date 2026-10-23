using System;
using CornerSpider;

static class BoardingTests {
    static int checks;
    static void Check(bool ok,string why) { checks++;if(!ok) throw new Exception("Finger boarding: "+why); }
    static World Scene(int seed,double size,double dpi,int direction,V approach,out V cursor) {
        var w=new World(5000,3000,seed);w.AutoFeed=false;
        w.ConfigureDisplays(new[]{new Area(0,0,5000,3000,dpi)},0);w.SetScale(size);
        double s=w.BodyScale;cursor=new V(direction<0?140*s:2500,1500);
        w.Spider=cursor+new V(approach.X*direction,approach.Y)*s;
        w.AttentionDistance=400;w.DismissDistance=450;return w;
    }
    static void ToRide(World w,V cursor,double dt) {
        V destination=new V();bool planned=false,catchPlan=false;
        for(int i=0;i<14/dt && w.State!=Mood.Ride;i++) {
            w.Tick(dt,cursor);
            if(w.State==Mood.Reach || w.State==Mood.Crouch || w.State==Mood.Jump) {
                if(!planned) { destination=w.JumpEnd;catchPlan=w.TipBoarding;planned=true; }
                Check((w.JumpEnd-destination).Length<.001 && catchPlan==w.TipBoarding,"chosen spot and boarding style remain committed through takeoff");
            }
        }
        Check(w.State==Mood.Ride && planned,"invitation completes a real boarding jump");
        Check((w.Spider-destination).Length<.001,"touchdown retains the actual jump endpoint");
    }
    public static void Run() {
        int caught=0,landedFromTip=0,surface=0;double low=200,high=0;
        V[] approaches={new V(-140,45),new V(65,40),new V(0,-110),new V(0,110)};
        foreach(double size in new[]{.2,1,1.8}) foreach(double dpi in new[]{1,1.5,2})
        foreach(double dt in new[]{.016,.033,.1}) foreach(int side in new[]{-1,1})
        for(int approach=0;approach<approaches.Length;approach++) for(int seed=0;seed<12;seed++) {
            V cursor;var w=Scene(seed,size,dpi,side,approaches[approach],out cursor);
            ToRide(w,cursor,dt);Check(w.FingerDirection==side,"real edge fitting selects the intended finger orientation");
            if(approach!=0) Check(w.OnFinger && !w.Hanging && !w.Climbing,"base, upper and middle approaches land without crossing to the dangling tip");
            if(w.Hanging) {
                caught++;Check(approach==0 && Math.Abs(World.AngleDelta(w.Angle,0))<.001,"only fingertip-side catches rotate to face their grip");
                Check((w.Spider-w.FingerTip(cursor)).Length<54*w.BodyScale,"catch stays within front-leg reach");
                continue;
            }
            surface++;if(approach==0) landedFromTip++;
            Check(w.OnFinger && w.SettlingOnFinger && w.FingerPosition>=45 && w.FingerPosition<=103,"direct landings use the solid upper surface and settle immediately");
            Check((w.Spider-w.FingerPoint(w.FingerPosition)).Length<.001,"stationary landing is exactly on its chosen perch");
            low=Math.Min(low,w.FingerPosition);high=Math.Max(high,w.FingerPosition);
            double position=w.FingerPosition;bool compressed=false;
            for(int i=0;i<.5/dt;i++) {
                V before=w.Spider;cursor+=new V(dt*12*dpi,0);w.Tick(dt,cursor);
                Check(w.OnFinger && !w.Hanging && !w.Climbing,"surface rider never enters the catch or climb timeline");
                Check((w.Spider-before).Length<dt*13*dpi+.001,"surface carrying has no fixed-position boarding snap");
                Check(Math.Abs(w.FingerPosition-position)<.001,"landing pause holds the chosen spot");compressed|=w.Squash<.98;
            }
            Check(compressed && !w.SettlingOnFinger,"landing absorbs impact and settles before exploring");
            V start=w.Spider;w.Tick(dt,cursor,true);
            Check(w.State==Mood.Dismount && w.Scared==0 && (w.Spider-start).Length<.001 && w.JumpEnd.Y<start.Y,"surface riders keep the calm upward click departure");
        }
        Check(caught>100 && landedFromTip>100 && surface>1000,"both catches and direct landings occur, including choice from the tip side");
        Check(high-low>50,"landings cover distinct tipward, central and baseward surface positions");
        // Repeat the same base-side approach: variety must come from choice, not only geometry.
        low=200;high=0;
        for(int seed=0;seed<50;seed++) { V cursor;var w=Scene(seed,1,1,1,approaches[1],out cursor);ToRide(w,cursor,.025);low=Math.Min(low,w.FingerPosition);high=Math.Max(high,w.FingerPosition); }
        Check(high-low>18,"repeated invitations from the same location vary the landing spot");
        foreach(int side in new[]{-1,1}) foreach(string interrupt in new[]{"click","fast","disabled","display","resume"}) {
            V cursor;var w=Scene(6,1,1,side,approaches[1],out cursor);ToRide(w,cursor,.025);V before=w.Spider;
            if(interrupt=="click") { w.Tick(.025,cursor,true);Check(w.State==Mood.Dismount && w.Scared==0,"click works during surface settling"); }
            if(interrupt=="fast") { w.Tick(.025,cursor+new V(200,0));Check(w.State==Mood.Jump && w.Scared>.9 && (w.Spider-before).Length<.001,"fast surface escape starts at the old position"); }
            if(interrupt=="disabled") { w.CursorPlay=false;w.Tick(.025,cursor);Check(w.State==Mood.Jump,"disabled play releases direct landing"); }
            if(interrupt=="display") { w.ConfigureDisplays(new[]{new Area(0,0,1600,1000,1.5)},0);Check(w.State!=Mood.Ride && w.FingerBlend==0,"display reset clears direct boarding"); }
            if(interrupt=="resume") { w.Resume();Check(w.State!=Mood.Ride && w.FingerBlend==0,"resume clears direct boarding"); }
        }
        foreach(Mood phase in new[]{Mood.Reach,Mood.Crouch,Mood.Jump}) {
            V cursor;var w=Scene(2,1,1,1,approaches[1],out cursor);
            for(int i=0;i<600 && w.State!=phase;i++) w.Tick(.025,cursor);
            Check(w.State==phase,"reaches cancellation phase "+phase);w.Tick(.025,cursor,true);
            for(int i=0;i<40;i++) { w.Tick(.025,cursor);Check(w.State!=Mood.Ride,"click cancels pending surface boarding in "+phase); }
        }
        // A small cursor drift during flight must settle continuously, not pin on the next frame.
        foreach(double dt in new[]{.016,.033,.1}) {
            V cursor;var w=Scene(4,1,1,1,approaches[1],out cursor);
            for(int i=0;i<14/dt && w.State!=Mood.Jump;i++) w.Tick(dt,cursor);
            for(int i=0;i<1/dt && w.State!=Mood.Ride;i++) { cursor+=new V(30*dt,0);w.Tick(dt,cursor); }
            Check(w.OnFinger,"gentle drift during flight still permits a surface landing");
            for(int i=0;i<.5/dt;i++) { V before=w.Spider;w.Tick(dt,cursor);Check((w.Spider-before).Length<85*dt,"landing drift eases onto the surface without a snap"); }
            Check((w.Spider-w.FingerPoint(w.FingerPosition)).Length<.001,"landing adjustment finishes on the surface");
        }
        Console.WriteLine("PASS: "+checks+" finger-boarding checks (varied surface landings, side-only catches, mirrored fingers, sizes/DPI, settling, interruptions and pointer drift).");
    }
}
