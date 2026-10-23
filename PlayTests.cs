using System;
using CornerSpider;

static class PlayTests {
    static int checks;
    static readonly V away=new V(-10000,-10000),cursor=new V(650,350);
    static void Check(bool ok,string why) { checks++;if(!ok) throw new Exception("Play: "+why); }
    static World New(int seed) { var w=new World(1280,720,seed);w.Spider=new V(600,350);w.AutoFeed=false;return w; }
    static void Advance(World w,double seconds,V point,double dt) { for(int i=0;i<Math.Ceiling(seconds/dt);i++) w.Tick(dt,point); }
        static World Board(int seed,double dt,bool roomy=false) {
            var w=New(seed);V point=cursor;w.Spider=point+new V(-140,40);
            if(roomy) { w.ConfigureDisplays(new[]{new Area(0,0,1600,1200,1)},0);point=new V(650,600);w.Spider=point+new V(-140,40); }
            for(int i=0;i<12/dt && w.State!=Mood.Ride;i++) w.Tick(dt,point);
        Check(w.State==Mood.Ride,"five-second invitation leads to voluntary boarding");return w;
    }
    public static void Run() {
        int near=0,far=0;
        for(int seed=0;seed<80;seed++) {
            var hunt=new World(3200,1800,seed);hunt.Spider=new V(1600,900);hunt.AutoFeed=false;hunt.SpawnFly();
            double distance=(hunt.Fly-hunt.Spider).Length;
            if(distance>300) far++;else near++;
            Check(distance>140 && distance<=541,"spawn offers a bounded variety of approach distances");
            V first=hunt.Fly;hunt.SpawnFly();Check((hunt.Fly-first).Length==0,"only one fly exists");
            double stalk=0,travel=0;V previous=hunt.Spider;
            for(int i=0;i<1200 && hunt.Meals==0;i++) {
                hunt.Tick(.025,away);if(hunt.State==Mood.Stalk) stalk+=.025;
                travel+=(hunt.Spider-previous).Length;previous=hunt.Spider;
            }
            Check(hunt.Meals==1,"distant hunt finishes, seed "+seed);
            if(distance>300) Check(stalk>3 && travel>220,"far prey gets an extended stalking approach");
        }
        Check(near>20 && far>20,"near and far hunts both occur");
        var w=New(10);Advance(w,4.4,cursor,.025);
        Check(w.FingerBlend==0 && w.State!=Mood.Ride,"finger waits for a deliberate invitation");
        Advance(w,.45,cursor,.025);Check(w.FingerBlend>0 && w.FingerBlend<1,"finger blends in around five seconds");
        w.Tick(.025,cursor,true);Check(w.InviteTime==0 && w.State!=Mood.Ride,"click cancels a pending invitation");
        foreach(double dt in new[]{.016,.033,.1}) {
            w=Board(12,dt);Check(w.Hanging && w.Spider.Y>w.FingerTip(cursor).Y+30,"initial catch dangles below the fingertip");
            V before=w.Spider,carried=cursor+new V(5,0);w.Tick(dt,carried);
            Check(w.State==Mood.Ride && (w.Spider-before).Length<4,"gentle carrying has suspension rather than a pinned offset");
            Advance(w,3.1,carried,dt);Check(w.OnFinger,"spider climbs onto the finger");
            double min=w.FingerPosition,max=min;bool waved=false;
            for(int i=0;i<14/dt;i++) {
                carried+=new V(dt*8,0);w.Tick(dt,carried);
                Check(w.State==Mood.Ride && w.FingerBlend>.99,"finger stays available during gentle movement");
                Check(w.FingerPosition>=44 && w.FingerPosition<=106,"walk stays on the offered finger");
                Check(World.Finite(w.Spider.X) && World.Finite(w.Spider.Y),"carry is finite at varied frame rates");
                min=Math.Min(min,w.FingerPosition);max=Math.Max(max,w.FingerPosition);
                if(w.FrontReach(-1)>.6) { waved=true;Check(Math.Abs(World.AngleDelta(w.Angle,180))<15,"turns toward viewer before waving"); }
            }
            Check(max-min>20 && waved,"finger has independent exploration and occasional waves");
            before=w.Spider;w.Tick(dt,carried,true);
            Check(w.State==Mood.Dismount && w.LastDismount=="click" && w.Scared==0,"click plans a calm upward departure");
            Check((w.Spider-before).Length<.001 && w.JumpEnd.Y<before.Y-60*w.BodyScale,"click chooses a point above without teleporting");
            for(int i=0;i<2/dt && w.State==Mood.Dismount;i++) w.Tick(dt,carried);
            Check(w.State==Mood.Jump,"planned departure reaches takeoff promptly");
            Check((w.JumpStart-before).Length<.001 && w.SilkOpacity>0,"click departure starts at the pet and trails silk");
            Advance(w,.45,away,dt);Check(w.SilkOpacity>0,"departure silk remains briefly after landing");
            double angle=w.Angle*Math.PI/180;
            V spinneret=w.Spider+new V(-Math.Sin(angle)*34*w.BodyScale,Math.Cos(angle)*34*w.BodyScale-w.Lift);
            Check((w.SilkEnd-spinneret).Length<.001,"fading silk remains attached after the landing rather than floating in midair");
            Advance(w,2,away,dt);Check(w.SilkOpacity==0 && w.FingerBlend==0,"temporary finger and silk clean up");
        }
        w=Board(13,.025);V start=w.Spider;w.Tick(.025,cursor+new V(250,0));
        Check(w.State==Mood.Jump && w.Scared>.9 && w.LastDismount=="fast movement","fast movement triggers a startled escape");
        Check((w.Spider-start).Length<.001 && w.JumpDuration<=.23 && w.RideCooldown>=25,"escape never teleports with the new pointer position");
        w=Board(14,.025);w.CursorPlay=false;w.Tick(.025,cursor);Check(w.State==Mood.Jump,"disabled play releases the pet");
        foreach(V direction in new[]{new V(1,0),new V(0,1),new V(0,-1)}) {
            w=Board(22,.025,true);V moving=new V(650,600);
            for(int i=0;i<48;i++) {
                moving+=direction*6.25;w.Tick(.025,moving);
                Check(w.Hanging && (w.Spider-w.FingerTip(moving)).Length<=54.01*w.BodyScale,"sustained gentle carrying stays attached within leg reach: step "+i+", state "+w.State+", reason "+w.LastDismount);
                Check(w.Spider.Y>w.FingerTip(moving).Y,"suspension remains below the fingertip");
            }
        }
        w=New(25);w.ConfigureDisplays(new[]{new Area(0,0,1280,900,1),new Area(1280,0,1920,1200,1.5)},1);
        w.Spider=new V(1050,400);V seam=new V(1120,400);
        for(int i=0;i<500 && !w.OnFinger;i++) w.Tick(.025,seam);
        Check(w.OnFinger,"boards beside a simulated mixed-DPI seam");
        for(int i=0;i<220;i++) {
            seam+=new V(2,0);w.Tick(.025,seam);
            Check(w.OnFinger && World.Finite(w.Spider.X) && World.Finite(w.Spider.Y),"gentle finger ride crosses mixed-DPI seam without dismount or invalid position");
        }
        Check(w.AreaAt(w.Spider)==1 && w.BodyScale>1.49,"carried spider adopts destination monitor scale");
        w=New(11);w.Spider=new V(1130,350);V right=new V(1180,350);
        for(int i=0;i<450 && w.State!=Mood.Ride;i++) w.Tick(.025,right);
        Check(w.State==Mood.Ride && w.FingerTip(right).X<right.X,"centered finger keeps its tip inside the right edge");
        Check((w.FingerPoint(75)-right).Length<6*w.BodyScale,"middle of the solid finger is centered on the mouse");
        w.ConfigureDisplays(new[]{new Area(0,0,900,600,1.5)},0);
        Check(w.State!=Mood.Ride && w.FingerBlend==0 && w.InviteTime==0,"display changes release play and clear stale offers");
        Console.WriteLine("PASS: "+checks+" play checks (distant hunts, five-second offer, suspension, finger walking/waves, click/fast departures, silk cleanup, edges and frame rates).");
    }
}
