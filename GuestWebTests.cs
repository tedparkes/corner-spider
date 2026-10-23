using System;
using CornerSpider;

static class GuestWebTests {
    static int checks;
    static void Check(bool ok,string why) { checks++;if(!ok) throw new Exception("Orb web: "+why); }
    public static void Run() {
        int anchors=0,curved=0,spiral=0;V previous=new V();bool first=true;
        for(int i=0;i<GuestWebPattern.Count;i++) {
            var strand=GuestWebPattern.Strand(i);
            if(strand.A.X==0 || strand.A.Y==0) anchors++;
            if(strand.Kind==2) {
                spiral++;if(!first) Check((previous-strand.A).Length<.000001,"capture spiral remains connected between turns");
                first=false;previous=strand.D;
                Check((strand.Point(.5)-(strand.A+strand.D)*.5).Length>.1,"capture segments have visible inward curvature");
                for(int end=0;end<2;end++) {
                    V point=(end==0?strand.A:strand.D)-GuestWebPattern.Hub;double distance=double.MaxValue;
                    for(int spoke=0;spoke<GuestWebPattern.Spokes;spoke++) { V axis=GuestWebPattern.Rim(spoke)-GuestWebPattern.Hub;distance=Math.Min(distance,Math.Abs(point.X*axis.Y-point.Y*axis.X)/axis.Length); }
                    Check(distance<.000001,"every capture-spiral junction meets an actual radial spoke");
                }
            }
            if((strand.Point(.5)-(strand.A+strand.D)*.5).Length>.1) curved++;
            for(int j=0;j<=20;j++) {
                V p=strand.Point(j/20.0);Check(World.Finite(p.X) && p.X>=0 && p.X<=188 && p.Y>=0 && p.Y<=188,"curves stay inside the existing reserved footprint");
                var partial=strand.Prefix(j/20.0);Check((partial.D-p).Length<.000001,"growing silk ends exactly at the builder's curve position");
            }
        }
        Check(anchors>=5 && spiral>=128 && curved>=128,"anchored radial web has enough curved winding silk to read clearly");
        Check((GuestWebPattern.Builder(1)-GuestWebPattern.Hub).Length<.000001,"finished builder reaches its central waiting hub");
        int active=-1;
        for(int j=0;j<=10000;j++) {
            double progress=j/10000.0;int next=GuestWebPattern.Active(progress);
            Check(next>=active,"construction never removes or rearranges earlier threads");active=next;
            double fraction=GuestWebPattern.Fraction(next,progress);
            if(fraction>0) Check((GuestWebPattern.Builder(progress)-GuestWebPattern.Strand(next).Point(fraction)).Length<.000001,"weaving motion and visible tip share the same curved path");
        }
        foreach(double size in new[]{.2,1,1.8}) foreach(double dpi in new[]{1,1.5,2}) for(int corner=0;corner<4;corner++) {
            double s=size*dpi;var w=new World(2400*s,1600*s,corner);w.ConfigureDisplays(new[]{new Area(0,0,2400*s,1600*s,dpi)},0);w.SetScale(size);
            w.Corner=Settings.Corners[corner];w.UpdateHome();w.Spider=w.Home;w.SetSpecies("PortiaLabiata");w.AutoFeed=w.CursorPlay=false;
            Check(w.StartWebVisit(),"real corner encounter starts at all sizes and DPIs");
            Check((w.GuestPatternPoint(GuestWebPattern.Rim(0))-w.GuestPluckPoint).Length<.000001,"Portia plucks the endpoint of a real hub-connected spoke");
            V last=w.Fly;
            while(w.WebStage==WebVisit.Building) {
                w.Tick(.1,new V(-10000,-10000));
                V expected=w.Areas[w.GuestArea].Clamp(w.GuestPatternPoint(GuestWebPattern.Builder(w.GuestProgress)),18*World.SmallSpiderSize*w.GuestScale);
                Check((w.Fly-expected).Length<.000001,"actual builder follows the shared construction curve");
                Check((w.Fly-last).Length<35*w.GuestScale,"length-weighted construction preserves smooth motion");last=w.Fly;
            }
            Check((w.Fly-w.GuestCenter).Length<.000001,"construction finishes at the resident's central perch");
        }
        Console.WriteLine("PASS: "+checks+" orb-web checks (connected spiral, curved spans, true spoke junctions, shared construction path, bounds, sizes/DPI and plucking anchor).");
    }
}
