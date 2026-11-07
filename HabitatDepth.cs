using System;
using System.Collections.Generic;

namespace CornerSpider {
    public sealed class HabitatDepth {
        const double FootRadius=.008;
        const double SampleSpacing=.0035;
        internal const double HopHeight=.018;
        static readonly V[] FootProbes={
            new V(1,0),new V(-1,0),new V(0,1),new V(0,-1),
            new V(.707,.707),new V(-.707,.707),new V(.707,-.707),new V(-.707,-.707)
        };
        readonly byte[] samples;
        HabitatRoute router;

        public HabitatDepth(int width,int height,byte[] nearWhiteDepth) {
            if(width<2 || height<2 || nearWhiteDepth==null || (long)width*height!=nearWhiteDepth.Length)
                throw new ArgumentException("The depth field needs one sample per image pixel.");
            Width=width;
            Height=height;
            samples=(byte[])nearWhiteDepth.Clone();
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public double Aspect { get { return (double)Width/Height; } }
        internal HabitatRoute Router { get { return router??(router=new HabitatRoute(this)); } }

        static double Unit(double value) { return Math.Max(0,Math.Min(1,value)); }
        internal static bool Inside(V point) { return point.X>=0 && point.X<=1 && point.Y>=0 && point.Y<=1; }
        internal static double Ease(double value) { double t=Unit(value);return t*t*(3-2*t); }
        internal double Distance(V a,V b) { return new V((a.X-b.X)*Aspect,a.Y-b.Y).Length; }

        public double DepthAt(V normalized) {
            if(!World.Finite(normalized.X) || !World.Finite(normalized.Y)) return 0;
            double x=Unit(normalized.X)*(Width-1),y=Unit(normalized.Y)*(Height-1);
            int left=(int)x,top=(int)y;
            int right=Math.Min(left+1,Width-1),bottom=Math.Min(top+1,Height-1);
            double u=x-left,v=y-top;
            double a=samples[top*Width+left]*(1-u)+samples[top*Width+right]*u;
            double b=samples[bottom*Width+left]*(1-u)+samples[bottom*Width+right]*u;
            return (a*(1-v)+b*v)/255;
        }

        public static double ScaleAt(double depth) {
            return .60+1.02*Unit(World.Finite(depth)?depth:0);
        }

        public double VisibilityAt(V normalized,double actorDepth) {
            if(!Inside(normalized) || !World.Finite(actorDepth)) return 0;
            // The tolerance keeps relief on the actor's own surface from
            // punching tiny holes through its silhouette.
            return 1-Ease((DepthAt(normalized)-actorDepth-.028)/.075);
        }

        internal bool HasFooting(V point) {
            return HasFooting(point,DepthAt(point));
        }

        bool HasFooting(V point,double center,double margin=1) {
            if(!Inside(point) || point.X<.022 || point.X>.978 || point.Y<.022 || point.Y>.978) return false;
            double horizontal=FootRadius/Aspect;
            double low=center,high=center;
            foreach(var probe in FootProbes) {
                double depth=DepthAt(point+new V(probe.X*horizontal,probe.Y*FootRadius));
                if(Math.Abs(depth-center)>.082*margin) return false;
                low=Math.Min(low,depth);high=Math.Max(high,depth);
            }
            return high-low<=.125*margin;
        }

        public bool CanWalk(V from,V to) {
            if(!Inside(from) || !Inside(to)) return false;
            double distance=Distance(from,to);
            if(distance<.000001) return HasFooting(from);
            int count=Math.Max(1,(int)Math.Ceiling(distance/SampleSpacing));
            int shortSpan=Math.Min(count,Math.Max(1,(int)Math.Round(count*.018/Math.Max(.018,distance))));
            double previous=DepthAt(from);
            if(!HasFooting(from,previous)) return false;
            V step=(to-from)*(1.0/count);
            for(int i=1;i<=count;i++) {
                V point=from+step*i;
                double depth=DepthAt(point);
                if(Math.Abs(depth-previous)>.065 || !HasFooting(point,depth,i==count?1:.96)) return false;
                if(i>=shortSpan && Math.Abs(depth-DepthAt(from+step*(i-shortSpan)))>.115) return false;
                // Sample the footprint more finely than the slope. A thin
                // edge can hide between two valid centerline samples.
                for(int part=1;part<=3;part++) {
                    V contact=from+step*(i-part*.25);
                    if(!HasFooting(contact,DepthAt(contact),.96)) return false;
                }
                previous=depth;
            }
            return true;
        }

        internal bool CanHop(V from,V to) {
            double distance=Distance(from,to);
            if(distance<.022 || distance>.116 || !HasFooting(from) || !HasFooting(to)) return false;
            double start=DepthAt(from),end=DepthAt(to);
            if(Math.Abs(start-end)>.22) return false;
            V delta=to-from;
            V shoulder=new V(-delta.Y/Aspect,delta.X*Aspect)*(FootRadius*.44/distance);
            int count=(int)Math.Ceiling(distance/SampleSpacing);
            for(int i=1;i<count;i++) {
                double t=(double)i/count;
                double arc=4*t*(1-t);
                V position=from+delta*t-new V(0,HopHeight*arc);
                double clear=start+(end-start)*t+.031+.064*arc;
                if(!Inside(position) || DepthAt(position)>clear ||
                    DepthAt(position-shoulder)>clear || DepthAt(position+shoulder)>clear) return false;
            }
            return true;
        }
    }

    public struct HabitatStep {
        public readonly V Destination;
        public readonly bool Hop;
        internal HabitatStep(V destination,bool hop) { Destination=destination;Hop=hop; }
    }

    public sealed class HabitatRoute {
        struct Edge {
            public int To;
            public bool Hop;
            public double Cost;
            public Edge(int to,bool hop,double cost) { To=to;Hop=hop;Cost=cost; }
        }

        const double Border=.045;
        readonly HabitatDepth field;
        readonly V[] points;
        readonly bool[] footing;
        readonly List<Edge>[] edges;

        internal HabitatRoute(HabitatDepth field) {
            this.field=field;
            const int rows=27;
            int columns=Math.Max(11,Math.Min(62,(int)Math.Round((rows-1)*field.Aspect)+1));
            points=new V[rows*columns];
            footing=new bool[points.Length];
            edges=new List<Edge>[points.Length];
            for(int row=0;row<rows;row++) for(int column=0;column<columns;column++) {
                int index=row*columns+column;
                points[index]=new V(Border+(1-2*Border)*column/(columns-1),Border+(1-2*Border)*row/(rows-1));
                footing[index]=field.HasFooting(points[index]);
            }
            for(int row=0;row<rows;row++) for(int column=0;column<columns;column++) {
                int from=row*columns+column;
                if(!footing[from]) continue;
                for(int dy=0;dy<=2;dy++) for(int dx=-2;dx<=2;dx++) {
                    if(dy==0 && dx<=0) continue;
                    int span=Math.Max(Math.Abs(dx),dy);
                    if(span==2 && dx!=0 && dy!=0 && Math.Abs(dx)!=dy) continue;
                    int x=column+dx,y=row+dy;
                    if(x<0 || x>=columns || y>=rows) continue;
                    int to=y*columns+x;
                    if(!footing[to]) continue;
                    bool walk=field.CanWalk(points[from],points[to]);
                    if(walk?span==1:field.CanHop(points[from],points[to])) {
                        double cost=Cost(points[from],points[to],!walk);
                        Add(from,new Edge(to,!walk,cost));
                        Add(to,new Edge(from,!walk,cost));
                    }
                }
            }
        }

        void Add(int from,Edge edge) {
            if(edges[from]==null) edges[from]=new List<Edge>();
            edges[from].Add(edge);
        }

        double Cost(V from,V to,bool hop) {
            double distance=field.Distance(from,to);
            return hop?distance*2+.055:distance+.08*Math.Abs(field.DepthAt(from)-field.DepthAt(to));
        }

        public static bool TryPlan(HabitatDepth field,V from,V to,out List<HabitatStep> steps) {
            steps=null;
            if(field==null || !field.HasFooting(from) || !field.HasFooting(to)) return false;
            if(field.CanWalk(from,to)) {
                steps=new List<HabitatStep> { new HabitatStep(to,false) };
                return true;
            }
            return field.Router.Find(from,to,out steps);
        }

        bool Connect(V from,V to,out bool hop) {
            hop=false;
            if(field.Distance(from,to)>.12) return false;
            if(field.CanWalk(from,to)) return true;
            hop=field.CanHop(from,to);
            return hop;
        }

        bool Find(V from,V to,out List<HabitatStep> steps) {
            steps=null;
            int end=points.Length;
            var distance=new double[end+1];
            var predecessor=new int[end+1];
            var closed=new bool[end+1];
            var viaHop=new bool[end+1];
            var estimate=new double[end+1];
            var goalEdges=new Edge?[end];
            bool directHop=field.CanHop(from,to);
            distance[end]=directHop?Cost(from,to,true):double.PositiveInfinity;
            viaHop[end]=directHop;
            predecessor[end]=-1;
            for(int i=0;i<end;i++) {
                distance[i]=double.PositiveInfinity;
                predecessor[i]=-1;
                estimate[i]=field.Distance(points[i],to);
                if(edges[i]==null) continue;
                bool hop;
                if(Connect(from,points[i],out hop)) { distance[i]=Cost(from,points[i],hop);viaHop[i]=hop; }
                if(Connect(points[i],to,out hop)) goalEdges[i]=new Edge(end,hop,Cost(points[i],to,hop));
            }
            while(true) {
                int at=-1;
                double best=double.PositiveInfinity;
                for(int i=0;i<=end;i++) {
                    double score=distance[i]+estimate[i];
                    if(!closed[i] && score<best) { best=score;at=i; }
                }
                if(at<0) return false;
                if(at==end) break;
                closed[at]=true;
                foreach(var edge in edges[at]) Relax(at,edge,distance,predecessor,viaHop);
                if(goalEdges[at].HasValue) Relax(at,goalEdges[at].Value,distance,predecessor,viaHop);
            }
            var route=new List<HabitatStep>();
            for(int at=end;at>=0;at=predecessor[at]) route.Add(new HabitatStep(at==end?to:points[at],viaHop[at]));
            route.Reverse();
            steps=Simplify(from,route);
            return true;
        }

        static void Relax(int from,Edge edge,double[] distance,int[] predecessor,bool[] viaHop) {
            double cost=distance[from]+edge.Cost;
            if(cost>=distance[edge.To]) return;
            distance[edge.To]=cost;
            predecessor[edge.To]=from;
            viaHop[edge.To]=edge.Hop;
        }

        List<HabitatStep> Simplify(V from,List<HabitatStep> route) {
            var steps=new List<HabitatStep>();
            for(int i=0;i<route.Count;i++) {
                int last=i;
                if(!route[i].Hop) {
                    for(int next=i+1;next<route.Count && next<=i+7 && !route[next].Hop;next++)
                        if(field.CanWalk(from,route[next].Destination)) last=next;
                }
                if(field.Distance(from,route[last].Destination)>.000001) steps.Add(route[last]);
                from=route[last].Destination;
                i=last;
            }
            return steps;
        }

        internal bool TryFindStart(V preferred,out V start) {
            if(field.HasFooting(preferred)) {
                for(int i=0;i<points.Length;i++) if(edges[i]!=null && field.Distance(preferred,points[i])<.10 &&
                    field.CanWalk(preferred,points[i])) { start=preferred;return true; }
            }
            start=preferred;
            double distance=double.MaxValue;
            for(int i=0;i<points.Length;i++) if(edges[i]!=null) {
                double error=field.Distance(preferred,points[i]);
                if(error<distance) { distance=error;start=points[i]; }
            }
            if(distance<double.MaxValue || field.HasFooting(preferred)) return true;
            for(int i=0;i<points.Length;i++) if(footing[i]) {
                double error=field.Distance(preferred,points[i]);
                if(error<distance) { distance=error;start=points[i]; }
            }
            return distance<double.MaxValue;
        }
    }

    public enum HabitatMotion { Pause,Walk,Hop }

    public sealed class HabitatExplorer {
        readonly HabitatDepth field;
        readonly Random random;
        List<HabitatStep> route=new List<HabitatStep>();
        int nextStep;
        double pause,lookAngle,walkBudget,hopTime,hopDuration,hopStartDepth;
        V hopStart;

        public HabitatExplorer(HabitatDepth field,int seed,V? start=null) {
            if(field==null) throw new ArgumentNullException("field");
            this.field=field;
            random=new Random(seed);
            V preferred=start??new V(.52,.77);
            V position;
            HasSurface=field.Router.TryFindStart(HabitatDepth.Inside(preferred)?preferred:new V(.52,.77),out position);
            Position=position;
            Target=Position;
            Depth=field.DepthAt(Position);
            BeginPause(.65);
        }

        public V Position { get; private set; }
        public V Target { get; private set; }
        public double Depth { get; private set; }
        public double Heading { get; private set; }
        public double Speed { get; private set; }
        public double WalkDistance { get; private set; }
        public HabitatMotion Mood { get; private set; }
        public bool HasSurface { get; private set; }
        public bool HasRoute { get { return nextStep<route.Count; } }
        public double PauseRemaining { get { return Math.Max(0,pause); } }
        public double HopProgress { get { return Mood==HabitatMotion.Hop?Math.Min(1,hopTime/hopDuration):0; } }
        public double HopLift { get { double t=HopProgress;return HabitatDepth.HopHeight*4*t*(1-t); } }
        internal V GroundPoint { get { return Mood==HabitatMotion.Hop?hopStart:Position; } }

        public bool SetDestination(V normalized) {
            if(!HasSurface || Mood==HabitatMotion.Hop || field.Distance(Position,normalized)<.00001) return false;
            List<HabitatStep> steps;
            if(!HabitatRoute.TryPlan(field,Position,normalized,out steps) || steps.Count==0) return false;
            SetRoute(normalized,steps);
            pause=Math.Min(pause,.45);
            return true;
        }

        void SetRoute(V destination,List<HabitatStep> steps) {
            route=steps;
            nextStep=0;
            Target=destination;
            walkBudget=Between(1.15,2.5);
        }

        double Between(double min,double max) { return min+(max-min)*random.NextDouble(); }

        void BeginPause(double seconds) {
            Mood=HabitatMotion.Pause;
            pause=seconds;
            Speed=0;
            lookAngle=Heading+Between(-38,38);
        }

        void Turn(V direction,double dt) {
            TurnTo(Math.Atan2(direction.Y,direction.X*field.Aspect)*180/Math.PI+90,dt,235);
        }

        void TurnTo(double desired,double dt,double rate) {
            double change=World.AngleDelta(Heading,desired);
            Heading=(Heading+Math.Max(-rate*dt,Math.Min(rate*dt,change))+360)%360;
        }

        bool ChooseRoute() {
            var choices=new List<Tuple<double,V>>();
            for(int i=0;i<66;i++) {
                V candidate=new V(Between(.075,.925),Between(.075,.925));
                double distance=field.Distance(Position,candidate);
                if(distance<.13 || distance>.70 || !field.HasFooting(candidate)) continue;
                double change=Math.Abs(field.DepthAt(candidate)-Depth);
                double score=change*2.6-Math.Abs(distance-.39)*.38;
                choices.Add(Tuple.Create(score,candidate));
            }
            choices.Sort((a,b)=>b.Item1.CompareTo(a.Item1));
            for(int i=0;i<Math.Min(14,choices.Count);i++) {
                V candidate=choices[i].Item2;
                List<HabitatStep> steps;
                if(!HabitatRoute.TryPlan(field,Position,candidate,out steps) || steps.Count==0) continue;
                double length=0;
                V at=Position;
                foreach(var step in steps) { length+=field.Distance(at,step.Destination);at=step.Destination; }
                if(length>field.Distance(Position,candidate)*2.6+.16) continue;
                SetRoute(candidate,steps);
                return true;
            }
            // Small connected shelves may not have a suitable distant target.
            for(int i=0;i<16;i++) {
                double direction=i*(Math.PI/8),length=i<8?.075:.037;
                V candidate=Position+new V(Math.Cos(direction)/field.Aspect,Math.Sin(direction))*length;
                if(!field.HasFooting(candidate) || !field.CanWalk(Position,candidate)) continue;
                SetRoute(candidate,new List<HabitatStep> { new HabitatStep(candidate,false) });
                return true;
            }
            return false;
        }

        public void Tick(double dt) {
            if(!HasSurface || !World.Finite(dt) || dt<=0) return;
            double remaining=Math.Min(.25,dt);
            while(remaining>.000001) {
                double step=Math.Min(1.0/60,remaining);
                Advance(step);
                remaining-=step;
            }
        }

        void Advance(double dt) {
            if(pause>0) {
                pause-=dt;
                TurnTo(lookAngle,dt,95);
                return;
            }
            if(!HasRoute && !ChooseRoute()) { BeginPause(1.1);return; }
            HabitatStep step=route[nextStep];
            V delta=step.Destination-Position;
            Turn(delta,dt);
            if(step.Hop) {
                if(Mood!=HabitatMotion.Hop) {
                    Mood=HabitatMotion.Hop;
                    hopTime=0;
                    hopDuration=.26+.75*field.Distance(Position,step.Destination);
                    hopStart=Position;
                    hopStartDepth=Depth;
                }
                hopTime+=dt;
                double t=HopProgress;
                Position=hopStart+(step.Destination-hopStart)*t;
                Depth=hopStartDepth+(field.DepthAt(step.Destination)-hopStartDepth)*t;
                Speed=field.Distance(hopStart,step.Destination)/hopDuration;
                if(t>=1) { nextStep++;BeginPause(.26); }
                return;
            }
            Mood=HabitatMotion.Walk;
            double distance=field.Distance(Position,step.Destination);
            double advance=Math.Min(distance,dt*(.085+.051*Depth));
            V next=distance<=advance+.000001?step.Destination:Position+delta*(advance/distance);
            // Validate the exact position too. A narrow edge can fall between
            // the samples taken when this route was planned.
            if(!field.HasFooting(next)) {
                route.Clear();nextStep=0;Target=Position;
                BeginPause(.38);
                return;
            }
            Position=next;
            Depth=field.DepthAt(Position);
            WalkDistance+=advance;
            Speed=advance/dt;
            walkBudget-=dt;
            if(distance<=advance+.000001) nextStep++;
            if(!HasRoute || walkBudget<=0) {
                BeginPause(HasRoute?Between(.48,1.04):Between(.80,1.50));
                walkBudget=Between(1.15,2.5);
            }
        }
    }
}
