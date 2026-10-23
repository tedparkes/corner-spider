using System;
using System.Collections.Generic;

namespace CornerSpider {
    public partial class World {
        bool nestInside;
        List<V> nestRoute;
        int nestWaypoint;
        public V NestForward { get { double a=NestAngle*Math.PI/180;return new V(Math.Sin(a),-Math.Cos(a)); } }
        // A real entrance, measured on the retreat's long axis. Both ends stay
        // on that axis even on an unusually small work area.
        double NestRun {
            get {
                var a=Areas[HomeArea];double margin=SpiderPose.Radius*HomeScale;
                double x=Math.Min(Home.X-a.X,a.X+a.W-Home.X)-Math.Min(margin,a.W/2);
                double y=Math.Min(Home.Y-a.Y,a.Y+a.H-Home.Y)-Math.Min(margin,a.H/2);
                return Math.Max(0,Math.Min(96*HomeScale,Math.Min(x,y)*Math.Sqrt(2)));
            }
        }
        public V NestEntrance { get { return Home-NestForward*NestRun; } }
        public V NestExit { get { return Home+NestForward*NestRun; } }
        public bool UnderNestSilk { get { return nestInside && (Spider-Home).Length<150*HomeScale; } }
        void ResetNestRoute() { nestRoute=null;nestWaypoint=0; }
        void StartNestExit() { cornerWaypoint=null;ResetNestRoute();Enter(Mood.ExitNest); }
        // The visible cocoon is not a surface to cut across. Route around a
        // clearance rectangle, then approach its rear mouth along the long axis.
        // A tiny visibility graph also handles starting on the far side of it.
        void PlanNestEntry() {
            nestRoute=new List<V>();nestWaypoint=0;
            var a=Areas[HomeArea];double margin=SpiderPose.Radius*HomeScale;
            double hx=Math.Max(0,Math.Min(90*HomeScale,Math.Min(Home.X-a.X,a.X+a.W-Home.X)-Math.Min(margin,a.W/2)));
            double hy=Math.Max(0,Math.Min(90*HomeScale,Math.Min(Home.Y-a.Y,a.Y+a.H-Home.Y)-Math.Min(margin,a.H/2)));
            V start=Spider,relative=start-Home;
            if(hx>.01 && hy>.01 && Math.Abs(relative.X)<hx && Math.Abs(relative.Y)<hy) {
                // Already next to the silk: step outward first, never through its centre.
                double extent=Math.Max(Math.Abs(relative.X)/hx,Math.Abs(relative.Y)/hy);
                if(extent>.001) { start=Home+relative*(1/extent);nestRoute.Add(start); }
            }
            V[] nodes={start,Home+new V(-hx,-hy),Home+new V(hx,-hy),Home+new V(hx,hy),Home+new V(-hx,hy)};
            V rear=NestForward*(-1);int finish=rear.Y<0?(rear.X<0?1:2):(rear.X>0?3:4);
            double[] costs={0,double.MaxValue,double.MaxValue,double.MaxValue,double.MaxValue};
            int[] previous={-1,-1,-1,-1,-1};bool[] used=new bool[5];
            for(int k=0;k<5;k++) {
                int at=-1;for(int i=0;i<5;i++) if(!used[i] && (at<0 || costs[i]<costs[at])) at=i;
                if(at<0 || costs[at]==double.MaxValue) break;used[at]=true;
                for(int next=1;next<5;next++) {
                    if(used[next] || CrossesNestBox(nodes[at]-Home,nodes[next]-Home,hx,hy)) continue;
                    double cost=costs[at]+(nodes[next]-nodes[at]).Length;
                    if(cost<costs[next]) { costs[next]=cost;previous[next]=at; }
                }
            }
            var path=new List<V>();for(int at=finish;at>0;at=previous[at]) { path.Add(nodes[at]);if(previous[at]<0) break; }
            path.Reverse();nestRoute.AddRange(path);nestRoute.Add(NestEntrance);
        }
        static bool CrossesNestBox(V a,V b,double hx,double hy) {
            // Open interior only: walking along a clearance edge is allowed.
            hx=Math.Max(0,hx-.001);hy=Math.Max(0,hy-.001);double low=0,high=1;
            if(!NestSlab(a.X,b.X-a.X,hx,ref low,ref high) || !NestSlab(a.Y,b.Y-a.Y,hy,ref low,ref high)) return false;
            return high>low;
        }
        static bool NestSlab(double p,double delta,double half,ref double low,ref double high) {
            if(Math.Abs(delta)<.000001) return Math.Abs(p)<half;
            double a=(-half-p)/delta,b=(half-p)/delta;
            low=Math.Max(low,Math.Min(a,b));high=Math.Min(high,Math.Max(a,b));return high>low;
        }
        void ReturnToNest(double dt) {
            if((Spider-Home).Length<2*HomeScale) { Spider=Home;nestInside=true;desiredAngle=NestAngle;Enter(Mood.Sleep);return; }
            if(UnderNestSilk) { ResetNestRoute();Enter(Mood.EnterNest);return; }
            // Keep the existing perimeter and cross-monitor route until local.
            if(AreaAt(Spider)!=HomeArea || cornerWaypoint.HasValue) { BurstMove(Home,190,dt,false);return; }
            if(nestRoute==null) PlanNestEntry();
            if(nestWaypoint<nestRoute.Count) {
                if(BurstMove(nestRoute[nestWaypoint],150,dt,false)) nestWaypoint++;
                return;
            }
            nestInside=true;Face(Home);Enter(Mood.EnterNest);
        }
        void CrawlIntoNest(double dt) {
            Face(Home);
            // Hold at the mouth to finish turning, then keep a deliberate gait.
            if(StateTime<.3 || Math.Abs(AngleDelta(Angle,desiredAngle))>8) { Speed=0;return; }
            if(Move(Home,38,dt)) { desiredAngle=NestAngle;Enter(Mood.Sleep); }
        }
        void CrawlOutOfNest(double dt) {
            desiredAngle=NestAngle;
            if(nestRest>0 || StateTime<.3 || Math.Abs(AngleDelta(Angle,desiredAngle))>8) { Speed=0;return; }
            if(Move(NestExit,42,dt)) { nestInside=false;decision=Between(4,8);Enter(HasFly && !GuestPrey?Mood.Notice:Mood.Rest); }
        }
    }
}
