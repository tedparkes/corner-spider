using System;
using CornerSpider;

static class HammockTests {
    static int checks;
    static V away=new V(-10000,-10000);
    static void Check(bool ok,string message) { checks++;if(!ok) throw new Exception("Hammock: "+message); }
    public static void Run() {
        string[] corners={"TopLeft","TopRight","BottomLeft","BottomRight"};
        double[] angles={225,135,315,45};
        foreach(double size in new[]{.2,1.0,1.8}) foreach(double dpi in new[]{1.0,1.5,2.0}) for(int c=0;c<4;c++) {
            var w=new World(1920,1080,14);w.AutoFeed=w.CursorPlay=false;
            w.ConfigureDisplays(new[]{new Area(-1920,160,1920,1080,1),new Area(0,0,1920,1080,dpi)},1);
            w.Corner=corners[c];w.SetScale(size);w.Spider=w.Home;w.Angle=angles[c]+180;w.GoHome();
            Check(w.NestAngle==angles[c],"diagonal mirrors correctly at "+corners[c]);
            double before=w.Web,lastTuck=0;V[] resting=new V[4];
            for(int leg=0;leg<4;leg++) resting[leg]=SpiderPose.Leg(w,1,leg).Tip;
            for(int i=0;i<160;i++) {
                w.Tick(.025,away);
                Check((w.Spider-w.Home).Length<.001,"settling holds position inside its retreat");
                Check(w.NestTuck>=lastTuck && w.NestTuck-lastTuck<.031,"tuck eases continuously");lastTuck=w.NestTuck;
                if(w.NestTuck>0) Check(Math.Abs(World.AngleDelta(w.Angle,w.NestAngle))<8.01,"turn precedes leg tuck");
                for(int side=-1;side<=1;side+=2) for(int leg=0;leg<4;leg++) {
                    var p=SpiderPose.Leg(w,side,leg);
                    Check(Math.Abs((p.Knee3-p.Hip3).Length-SpiderPose.Femur[leg])<.0001 && Math.Abs((p.Ankle3-p.Knee3).Length-SpiderPose.Lower[leg])<.0001,"folded legs retain both segment lengths");
                    if(leg==0) { V3 axis=p.Ankle3-p.Hip3,knee=p.Knee3-p.Hip3;Check(Math.Abs(axis.X*knee.Y-axis.Y*knee.X)<.0001,"folded front hinge stays in its radial plane"); }
                    double angle=w.Angle*Math.PI/180;
                    foreach(V joint in new[]{p.Root,p.Hip,p.Knee,p.Ankle,p.Toe,p.Tip}) {
                        V at=w.Spider+new V(joint.X*Math.Cos(angle)-joint.Y*Math.Sin(angle),joint.X*Math.Sin(angle)+joint.Y*Math.Cos(angle))*w.BodyScale;
                        Check(w.Areas[1].Contains(at),"folded limbs stay within the home monitor at every size and DPI");
                    }
                }
            }
            Check(w.State==Mood.Sleep && w.NestTuck==1 && Math.Abs(World.AngleDelta(w.Angle,angles[c]))<.001,"settles diagonally in all four corners");
            Check(w.Web>before,"sleep adds silk to existing progress");
            for(int leg=0;leg<4;leg++) Check(SpiderPose.Leg(w,1,leg).Tip.X<resting[leg].X-3,"resting feet fold close to body");
            w.Wake();Check(w.NestTuck==1,"waking does not pop feet to an open stance");
            for(int i=0;i<30;i++) { w.Tick(.025,away);Check(w.NestTuck<=lastTuck && lastTuck-w.NestTuck<.061,"wake unfolds smoothly");lastTuck=w.NestTuck; }
            Check(w.NestTuck==0,"wake completely releases the tuck");
        }
        foreach(string corner in corners) {
            var w=new World(1280,720,19);w.AutoFeed=w.CursorPlay=false;w.Corner=corner;w.UpdateHome();w.Spider=new V(640,360);w.GoHome();
            for(int i=0;i<2400 && w.State!=Mood.Sleep;i++) {
                V previous=w.Spider;w.Tick(.025,away);
                Check((w.Spider-previous).Length<6,"return reaches the pocket without teleporting");
                Check(w.NestTuck==0,"does not curl up en route");
            }
            Check(w.State==Mood.Sleep && (w.Spider-w.Home).Length<.01,"visible return arrives before sleeping");
            for(int i=0;i<160;i++) w.Tick(.025,away);
            w.ConfigureDisplays(new[]{new Area(0,0,1600,900,1.5)},0);
            Check((w.Spider-w.Home).Length<.01 && w.NestTuck==1,"display reflow keeps a sleeping spider inside its retreat");
            for(int i=0;i<800;i++) w.Tick(.025,away);
            Check(w.State!=Mood.Sleep,"nap still ends normally");
        }
        Console.WriteLine("PASS: "+checks+" hammock checks (four corners, turn then tuck, fixed hinges, 20-180% sizes, mixed DPI, return, wake and reflow).");
    }
}
