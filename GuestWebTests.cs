using System;
using CornerSpider;

static class GuestWebTests {
    static int checks;
    static void Check(bool ok,string why) { checks++;if(!ok) throw new Exception("Corner web: "+why); }
    public static void Run() {
        int topAnchors=0,sideAnchors=0;
        for(int i=0;i<GuestWebPattern.Spokes;i++) {
            V rim=GuestWebPattern.Rim(i);if(rim.Y==0) topAnchors++;if(rim.X==0) sideAnchors++;
            int ties=0;
            for(int j=0;j<GuestWebPattern.Count;j++) {
                var frame=GuestWebPattern.Strand(j);if(frame.Kind!=0) continue;
                if((frame.A-rim).Length<.000001 || (frame.D-rim).Length<.000001) ties++;
            }
            Check(ties==2,"every spoke meets two connected frame spans; no floating rim endpoints");
        }
        Check(topAnchors>=5 && sideAnchors>=5,"multiple direct attachments spread along each actual desktop edge");
        var bridge=GuestWebPattern.Strand(0);Check(bridge.A.Y==0 && bridge.A.X>=180,"outer bridge begins on the top edge");
        bool reachesSide=false;
        for(int i=0;i<GuestWebPattern.Spokes;i++) {
            var frame=GuestWebPattern.Strand(i);
            Check((frame.D-GuestWebPattern.Strand((i+1)%GuestWebPattern.Spokes).A).Length<.000001,"foundation is a continuous closed frame");
            if(frame.D.X==0 && frame.D.Y>=185) reachesSide=true;
        }
        Check(reachesSide,"outer bridge finishes on the adjoining side edge");
        for(int i=0;i<5;i++) {
            var a=GuestWebPattern.Strand(i);var b=GuestWebPattern.Strand(i+1);
            V incoming=a.D-a.C,outgoing=b.B-b.A;
            Check(Math.Abs(incoming.X*outgoing.Y-incoming.Y*outgoing.X)<.000001 && incoming.X*outgoing.X+incoming.Y*outgoing.Y>0,
                "outer edge has a continuous tangent through each spoke join, without pointed scallops");
        }
        V openMiddle=GuestWebPattern.Rim(0),chord=new V(-183,188);
        double inset=Math.Abs((openMiddle.X-183)*chord.Y-openMiddle.Y*chord.X)/chord.Length;
        V chordMiddle=new V(91.5,94);
        double chordDepth=183*188/chord.Length;
        Check(inset/chordDepth>.34 && inset/chordDepth<.36,"outer edge bows inward by 35 percent of the full corner-to-chord depth");
        V priorMiddle=chordMiddle+(GuestWebPattern.Hub-chordMiddle)*.125;
        Check(openMiddle.Length/priorMiddle.Length>.69 && openMiddle.Length/priorMiddle.Length<.72,
            "middle edge is about 30 percent shallower than RC8 while its anchors stay fixed");
        Check((openMiddle-GuestWebPattern.Hub).Length>35,"deep curve leaves room around the resident hub");
        foreach(int spoke in new[]{GuestWebPattern.TopBridgeAnchor,GuestWebPattern.SideBridgeAnchor}) {
            V contact=GuestWebPattern.Rim(spoke);
            Check(contact.X==0 || contact.Y==0,"each plucking choice is a real desktop attachment");
            Check((contact-GuestWebPattern.Hub).Length*3+52>250,
                "anchor plucking preserves separation between Portia and the waiting resident on a full-size web");
        }
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
                Check(p.X/183+p.Y/188<=1.000001,"silk stays behind the edge-to-edge support span, without an unsupported outward bulge");
                if(i<6) Check(GuestWebPattern.InsideSilk(p),"perch boundary agrees with every point of the visible concave rim");
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
            Check((w.GuestCenter-w.GuestPatternPoint(GuestWebPattern.Hub)).Length<.000001,"resident perch follows the visible hub after mirroring and scaling");
            var area=w.Areas[w.GuestArea];
            for(int i=0;i<GuestWebPattern.Spokes;i++) {
                V rim=GuestWebPattern.Rim(i),at=w.GuestPatternPoint(rim);
                if(rim.X==0) Check(Math.Min(Math.Abs(at.X-area.X),Math.Abs(at.X-area.X-area.W))<.000001,"side anchor touches the actual work-area boundary");
                if(rim.Y==0) Check(Math.Min(Math.Abs(at.Y-area.Y),Math.Abs(at.Y-area.Y-area.H))<.000001,"top/bottom anchor touches the actual work-area boundary");
            }
            Check(w.GuestAnchorSpoke==GuestWebPattern.TopBridgeAnchor || w.GuestAnchorSpoke==GuestWebPattern.SideBridgeAnchor,"Portia chooses an anchored end, never the bridge middle");
            Check(w.GuestAnchorSpoke==GuestWebPattern.TopBridgeAnchor?w.GuestPluckSpoke==14 || w.GuestPluckSpoke==15:w.GuestPluckSpoke==2 || w.GuestPluckSpoke==1,"direct leg contact uses an existing rim junction beside the chosen anchored end");
            Check((w.GuestPatternPoint(GuestWebPattern.Rim(w.GuestPluckSpoke))-w.GuestPluckPoint).Length<.000001,"Portia plucks the endpoint of a real hub-connected spoke");
            V contact=w.GuestPatternPoint(GuestWebPattern.Rim(w.GuestAnchorSpoke));
            Check(Math.Min(Math.Min(Math.Abs(contact.X-area.X),Math.Abs(contact.X-area.X-area.W)),Math.Min(Math.Abs(contact.Y-area.Y),Math.Abs(contact.Y-area.Y-area.H)))<.000001,
                "chosen anchored end stays on a physical display edge after mirroring and scaling");
            Check(w.GuestOnSilk(w.GuestPluckPosition) && (w.GuestPluckPoint-w.GuestPluckPosition).Length<=55*w.GuestScale,"planned perch is on the silk within foreleg reach of the contacted junction");
            V last=w.Fly;
            while(w.WebStage==WebVisit.Building) {
                w.Tick(.1,new V(-10000,-10000));
                V expected=w.Areas[w.GuestArea].Clamp(w.GuestPatternPoint(GuestWebPattern.Builder(w.GuestProgress)),18*World.SmallSpiderSize*w.GuestScale);
                Check((w.Fly-expected).Length<.000001,"actual builder follows the shared construction curve");
                Check((w.Fly-last).Length<35*w.GuestScale,"length-weighted construction preserves smooth motion");last=w.Fly;
            }
            Check((w.Fly-w.GuestCenter).Length<.000001,"construction finishes at the resident's central perch");
        }
        Console.WriteLine("PASS: "+checks+" corner-web checks (connected spiral, curved spans, true spoke junctions, shared construction path, bounds, sizes/DPI and plucking anchor).");
    }
}
