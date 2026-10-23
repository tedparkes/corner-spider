using System;
using CornerSpider;

static class NestEntryTests {
    static int checks;
    static readonly V away=new V(-10000,-10000);
    static void Check(bool ok,string why) { checks++;if(!ok) throw new Exception("Nest entry: "+why); }
    static double Along(World w,V p) { V d=(p-w.Home)*(1/w.HomeScale),f=w.NestForward;return d.X*f.X+d.Y*f.Y; }
    static double Across(World w,V p) { V d=(p-w.Home)*(1/w.HomeScale),f=w.NestForward;return d.X*f.Y-d.Y*f.X; }
    static void Return(World w,double dt,bool requireEntrance) {
        bool entered=false;int crawlSteps=0;double crawlPhase=0,lastAlong=-10000;
        for(int i=0;i<12000 && w.State!=Mood.Sleep;i++) {
            V before=w.Spider;Mood state=w.State;w.Tick(dt,away);
            Check((w.Spider-before).Length<200*w.BodyScale*dt+2*w.BodyScale,"return has no teleport or abrupt position change");
            Check(w.InsideAny(w.Spider),"approach stays on the display");
            if(w.State==Mood.EnterNest) {
                if(!entered) { if(requireEntrance) Check((w.Spider-w.NestEntrance).Length<.01,"crawl begins at an end, not the cocoon side");crawlPhase=w.StepPhase; }
                entered=true;crawlSteps++;
                Check(Math.Abs(Across(w,w.Spider))<.01,"crawl follows the long axis through the opening");
                double along=Along(w,w.Spider);Check(along>=lastAlong-.001,"entrance crawl progresses head first");lastAlong=along;
                Check(w.UnderNestSilk,"only entering spider receives the silk cover");
                Check(w.NestTuck==0,"legs continue walking until inside");
                if((w.Spider-before).Length>.01 && state==Mood.EnterNest) {
                    Check(Math.Abs(World.AngleDelta(w.Angle,w.NestAngle))<8.1,"head is aligned with opening before crawling");
                    Check((w.Spider-before).Length<=38*w.HomeScale*dt+2*w.HomeScale,"entrance speed is a deliberate crawl");
                }
                double angle=w.Angle*Math.PI/180;
                for(int side=-1;side<=1;side+=2) for(int leg=0;leg<4;leg++) {
                    var pose=SpiderPose.Leg(w,side,leg);
                    foreach(V joint in new[]{pose.Knee,pose.Ankle,pose.Tip}) {
                        V at=w.Spider+new V(joint.X*Math.Cos(angle)-joint.Y*Math.Sin(angle),joint.X*Math.Sin(angle)+joint.Y*Math.Cos(angle))*w.BodyScale;
                        Check(w.Areas[w.HomeArea].Contains(at),"entrance keeps the whole leg pose on the home display");
                    }
                }
            } else if(w.State==Mood.Return) {
                Check(!w.UnderNestSilk,"approaching from outside does not fade under the silk");
                // The centre of the body must never cross a side wall on its way to the mouth.
                Check(Math.Abs(Across(w,w.Spider))>32 || Math.Abs(Along(w,w.Spider))>60,"return route stays outside the cocoon body");
            }
        }
        Check(w.State==Mood.Sleep && (w.Spider-w.Home).Length<.01,"return finishes inside the retreat");
        Check(entered && crawlSteps>15 && w.StepPhase>crawlPhase+5,"a visible walking sequence precedes sleep");
    }
    public static void Run() {
        foreach(string corner in Settings.Corners) foreach(double scale in new[]{.2,1,1.8}) foreach(double dpi in new[]{1,1.5,2}) for(int origin=0;origin<4;origin++) {
            var w=new World(1920,1080,39);w.AutoFeed=w.CursorPlay=false;w.Corner=corner;
            w.ConfigureDisplays(new[]{new Area(-1920,120,1920,1080,1),new Area(0,0,1920,1080,dpi)},1);w.SetScale(scale);
            V f=w.NestForward,r=new V(-f.Y,f.X);double s=w.HomeScale;
            w.Spider=origin==0?w.Areas[1].Center:w.Clamp(w.Home+(origin==1?f*140:origin==2?r*155:r*(-155))*s);
            w.Angle=31;w.GoHome();Return(w,.025,true);
            for(int i=0;i<120;i++) w.Tick(.025,away);
            double web=w.Web;w.Wake();Check(w.State==Mood.ExitNest && w.NestTuck==1,"wake starts by unfolding in the pocket");
            for(int i=0;i<320 && w.State==Mood.ExitNest;i++) {
                V before=w.Spider;double tuck=w.NestTuck;w.Tick(.025,away);
                Check(Math.Abs(Across(w,w.Spider))<.01,"waking spider leaves through the other end");
                if(tuck>0) Check((w.Spider-before).Length<.001,"unfolds before walking out");
            }
            Check(w.State==Mood.Rest && (w.Spider-w.NestExit).Length<.01 && !w.UnderNestSilk,"exit clears the silk completely");
            Check(w.Web>=web,"entry and exit preserve accumulated silk");
        }
        foreach(double dt in new[]{.016,.033,.1}) {
            var w=new World(1280,720,7);w.AutoFeed=w.CursorPlay=false;w.Spider=new V(640,360);w.GoHome();Return(w,dt,true);
        }
        foreach(string change in new[]{"size","display","corner","resume","feed"}) {
            var w=new World(1920,1080,37);w.AutoFeed=w.CursorPlay=false;w.Spider=new V(960,540);w.GoHome();
            for(int i=0;i<4000 && !(w.State==Mood.EnterNest && Along(w,w.Spider)>-45);i++) w.Tick(.025,away);
            Check(w.State==Mood.EnterNest,"interruption fixture enters the retreat");
            if(change=="size") w.SetScale(.6);
            if(change=="display") w.ConfigureDisplays(new[]{new Area(-1280,0,1280,720,1.5)},0);
            if(change=="corner") { w.Corner="BottomLeft";w.UpdateHome();w.GoHome(); }
            if(change=="resume") w.Tick(600,away);
            if(change=="feed") {
                w.SpawnFly();Check(w.State==Mood.ExitNest,"feeding asks an inside spider to leave through an end");
                for(int i=0;i<1600 && w.Meals==0;i++) w.Tick(.025,away);
                Check(w.Meals==1 && !w.UnderNestSilk,"hunt proceeds after leaving the silk");continue;
            }
            for(int i=0;i<6000 && w.State!=Mood.Sleep;i++) { w.Tick(.025,away);Check(w.InsideAny(w.Spider),"interrupted entry remains on a display"); }
            Check(w.State==Mood.Sleep && (w.Spider-w.Home).Length<.01,"interrupted route reaches current hammock: "+change);
        }
        foreach(double width in new[]{30,220,360}) {
            var w=new World(width,width*.75,8);w.AutoFeed=w.CursorPlay=false;w.SetScale(1.8);w.Spider=w.Clamp(new V(0,0));w.GoHome();
            for(int i=0;i<2000 && w.State!=Mood.Sleep;i++) w.Tick(.025,away);
            Check(w.State==Mood.Sleep && w.InsideAny(w.Spider),"tiny work area degrades to a bounded retreat instead of stranding the pet");
        }
        Console.WriteLine("PASS: "+checks+" end-entry checks (approach sides, axis alignment, crawl, silk occlusion, exit, four corners, size/DPI/frame steps and interruptions).");
    }
}
