using System;
using System.Collections.Generic;

namespace CornerSpider {
    // One shared silk path drives both weaving motion and the visible artwork.
    public struct GuestStrand {
        public V A,B,C,D;
        public int Kind; // 0: frame/anchors, 1: spokes, 2: capture spiral
        public GuestStrand(V a,V b,V c,V d,int kind) { A=a;B=b;C=c;D=d;Kind=kind; }
        public V Point(double t) { double u=1-t;return A*(u*u*u)+B*(3*u*u*t)+C*(3*u*t*t)+D*(t*t*t); }
        public GuestStrand Prefix(double t) {
            V ab=A+(B-A)*t,bc=B+(C-B)*t,cd=C+(D-C)*t;
            V abc=ab+(bc-ab)*t,bcd=bc+(cd-bc)*t;
            return new GuestStrand(A,ab,abc,abc+(bcd-abc)*t,Kind);
        }
    }
    public static class GuestWebPattern {
        public const int Spokes=16,Turns=9;
        public static readonly V Hub=new V(34,36);
        static readonly double[] bridgeTimes={0,.2,.36,.5,.64,.8,1};
        static readonly V[] rim=MakeRim();
        static readonly GuestStrand[] strands=MakeStrands();
        static readonly double[] starts=new double[strands.Length],travel=new double[strands.Length],lengths=new double[strands.Length];
        static readonly double total;
        public static int Count { get { return strands.Length; } }
        public static V Rim(int i) { return rim[(i%Spokes+Spokes)%Spokes]; }
        public static GuestStrand Strand(int i) { return strands[Math.Max(0,Math.Min(Count-1,i))]; }
        static GuestWebPattern() {
            double sum=0;
            for(int i=0;i<Count;i++) {
                starts[i]=sum;travel[i]=i==0?0:(strands[i].A-strands[i-1].D).Length;
                V before=strands[i].A;
                for(int j=1;j<=16;j++) { V at=strands[i].Point(j/16.0);lengths[i]+=(at-before).Length;before=at; }
                sum+=travel[i]+lengths[i];
            }
            total=sum;
        }
        static V[] MakeRim() {
            // The frame is held by both desktop edges, like silk across a
            // window corner. Its open rim bows inward between the two furthest
            // edge anchors; no part bulges past that supported span. Eleven
            // radial endpoints attach directly to an edge instead of leaving
            // a round orb suspended from a handful of distant guy lines.
            var points=new V[Spokes];
            var anchors=new[]{new V(0,188),new V(0,126),new V(0,83),new V(0,48),
                new V(0,18),new V(0,0),new V(22,0),new V(54,0),new V(92,0),
                new V(136,0),new V(183,0)};
            Array.Copy(anchors,0,points,3,anchors.Length);
            for(int i=0;i<bridgeTimes.Length;i++) points[(13+i)%Spokes]=BridgePoint(bridgeTimes[i]);
            return points;
        }
        // Pull the bridge control point one quarter of the way toward the hub.
        // A single quadratic arc keeps the open edge gently concave throughout,
        // with both desktop attachment points fixed. Split it exactly at spoke
        // joins so construction, artwork and plucking use the same curve.
        static V BridgeControl { get { V middle=new V(91.5,94);return middle+(Hub-middle)*.25; } }
        static V BridgePoint(double t) { double u=1-t;return new V(183,0)*(u*u)+BridgeControl*(2*u*t)+new V(0,188)*(t*t); }
        static V BridgeTangent(double t) { return (BridgeControl-new V(183,0))*(2*(1-t))+(new V(0,188)-BridgeControl)*(2*t); }
        static GuestStrand Bridge(double start,double end) {
            V a=BridgePoint(start),b=BridgePoint(end);double span=(end-start)/3;
            return new GuestStrand(a,a+BridgeTangent(start)*span,b-BridgeTangent(end)*span,b,0);
        }
        static GuestStrand Line(V a,V b,int kind) { return new GuestStrand(a,a+(b-a)*(1.0/3),a+(b-a)*(2.0/3),b,kind); }
        static GuestStrand Scallop(V a,V b,int kind,double slack) {
            V pull=(Hub-(a+b)*.5)*slack;
            return new GuestStrand(a,a+(b-a)*(1.0/3)+pull,a+(b-a)*(2.0/3)+pull,b,kind);
        }
        static GuestStrand[] MakeStrands() {
            var list=new List<GuestStrand>();
            // Lay the continuous edge-to-edge bridge first, then close its
            // frame along the two real edges before adding spokes and silk.
            for(int i=0;i<bridgeTimes.Length-1;i++) list.Add(Bridge(bridgeTimes[i],bridgeTimes[i+1]));
            for(int i=3;i<13;i++) list.Add(Scallop(Rim(i),Rim(i+1),0,.018));
            for(int i=0;i<Spokes;i++) list.Add(i%2==0?Line(Hub,Rim(i),1):Line(Rim(i),Hub,1));
            // A continuous, inward winding capture spiral. Each join lies on
            // an actual spoke, with modest spacing variation and inward sag.
            int count=Spokes*Turns;
            for(int i=0;i<count;i++) {
                double r0=SpiralRadius(i,count),r1=SpiralRadius(i+1,count);
                V a=Hub+(Rim(i)-Hub)*r0,b=Hub+(Rim(i+1)-Hub)*r1;
                list.Add(Scallop(a,b,2,.065+.012*Math.Sin(i*1.8)));
            }
            list.Add(Line(list[list.Count-1].D,Hub,1));return list.ToArray();
        }
        static double SpiralRadius(int i,int count) { return .94-.82*i/count+.007*Math.Sin(i*.81); }
        public static int Active(double progress) {
            double distance=Math.Max(0,Math.Min(1,progress))*total;
            int low=0,high=Count-1;
            while(low<high) { int mid=(low+high+1)/2;if(starts[mid]<=distance) low=mid;else high=mid-1; }
            return low;
        }
        public static double Fraction(int index,double progress) {
            index=Math.Max(0,Math.Min(Count-1,index));
            return Math.Max(0,Math.Min(1,(Math.Max(0,Math.Min(1,progress))*total-starts[index]-travel[index])/lengths[index]));
        }
        public static V Builder(double progress) {
            int index=Active(progress);double distance=Math.Max(0,Math.Min(1,progress))*total-starts[index];
            if(distance<travel[index]) {
                V before=index==0?strands[0].A:strands[index-1].D;
                return before+(strands[index].A-before)*(distance/travel[index]);
            }
            return strands[index].Point(Fraction(index,progress));
        }
    }
}
