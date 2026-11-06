using System;
using System.Collections.Generic;

namespace CornerSpider {
    public partial class World {
        bool nestInside;
        List<V> nestRoute;
        int nestWaypoint;
        public bool HasCornerSleeve { get { return !IsColonyResident && !WebDweller && !HuntsSpiders; } }
        public V NestForward { get { double a=NestAngle*Math.PI/180;return new V(Math.Sin(a),-Math.Cos(a)); } }
        public V NestRoomDirection { get { return NestForward*(HasCornerSleeve?-1:1); } }
        double NestClearance { get { return SpiderPose.Radius*Math.Max(HomeScale*LifeSize,BodyScale); } }
        public double DrawBodyScale {
            get {
                double scale=BodyScale;
                if(!HasCornerSleeve || (!nestInside && (Spider-Home).Length>.001)) return scale;
                var area=Areas[HomeArea];
                double x=Math.Min(Spider.X-area.X,area.X+area.W-Spider.X);
                double y=Math.Min(Spider.Y-area.Y,area.Y+area.H-Spider.Y);
                // Folded legs need less clearance than a walking stance.
                double radius=SpiderPose.Radius-20*NestTuck;
                return Math.Min(scale,Math.Max(.001,Math.Min(x,y)/radius));
            }
        }
        // A real entrance, measured on the retreat's long axis. Both ends stay
        // on that axis even on an unusually small work area.
        double NestRun {
            get {
                var a=Areas[HomeArea];double margin=NestClearance;
                if(HasCornerSleeve) {
                    V room=NestRoomDirection;
                    double horizontal=room.X>0?a.X+a.W-Home.X:Home.X-a.X;
                    double vertical=room.Y>0?a.Y+a.H-Home.Y:Home.Y-a.Y;
                    return Math.Max(0,Math.Min(96*HomeScale,Math.Min(
                        horizontal-Math.Min(margin,a.W/2),vertical-Math.Min(margin,a.H/2))*Math.Sqrt(2)));
                }
                double x=Math.Min(Home.X-a.X,a.X+a.W-Home.X)-Math.Min(margin,a.W/2);
                double y=Math.Min(Home.Y-a.Y,a.Y+a.H-Home.Y)-Math.Min(margin,a.H/2);
                return Math.Max(0,Math.Min(96*HomeScale,Math.Min(x,y)*Math.Sqrt(2)));
            }
        }
        public V NestEntrance {
            get {
                V mouth=Home-NestForward*NestRun;
                return HasCornerSleeve?Areas[HomeArea].Clamp(mouth,NestClearance):mouth;
            }
        }
        public V NestExit { get { return HasCornerSleeve?NestEntrance:Home+NestForward*NestRun; } }
        public bool UnderNestSilk { get { return nestInside && (Spider-Home).Length<150*HomeScale; } }
        void ResetNestRoute() { nestRoute=null;nestWaypoint=0; }
        void StartNestExit() { cornerWaypoint=null;ResetNestRoute();Enter(Mood.ExitNest); }
        // The visible cocoon is not a surface to cut across. Route around a
        // clearance rectangle, then approach its rear mouth along the long axis.
        // A tiny visibility graph also handles starting on the far side of it.
        void PlanNestEntry() {
            nestRoute=new List<V>();nestWaypoint=0;
            if(HasCornerSleeve) {
                V approach=SleeveApproach(Areas[HomeArea]);
                if((approach-Spider).Length>.001) nestRoute.Add(approach);
                nestRoute.Add(NestEntrance);
                return;
            }
            if(!WebDweller && !IsColonyResident && NestRun>=65*HomeScale && PlanSleeveEntry()) return;
            var a=Areas[HomeArea];double margin=NestClearance;
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
            var path=NestPath(nodes,finish,(from,to)=>CrossesNestBox(from-Home,to-Home,hx,hy));
            if(path!=null) nestRoute.AddRange(path);
            else nestRoute.Add(nodes[finish]);
            nestRoute.Add(NestEntrance);
        }
        bool PlanSleeveEntry() {
            // The diagonal sleeve takes less room than its axis-aligned bounds.
            // Clip its four bypass sides to the space the entire spider can use.
            var area=Areas[HomeArea];
            double halfX=area.W*.5-NestClearance,halfY=area.H*.5-NestClearance;
            if(halfX<=0 || halfY<=0) return false;
            double length=64*HomeScale,width=39*HomeScale;
            V forward=NestForward,across=new V(-forward.Y,forward.X);
            V end=forward*length,side=across*width;
            V[] corners={Home-end-side,Home+end-side,Home+end+side,Home-end+side};
            V approach=SleeveApproach(area);
            var nodes=new List<V> { approach };
            V nearest=approach;
            double best=double.MaxValue;
            for(int i=0;i<corners.Length;i++) {
                V start=corners[i],delta=corners[(i+1)%corners.Length]-start;
                V offset=start-area.Center;
                double lo=0,hi=1;
                if(!NestSlab(offset.X,delta.X,halfX,ref lo,ref hi) ||
                   !NestSlab(offset.Y,delta.Y,halfY,ref lo,ref hi)) continue;
                nodes.Add(start+delta*lo);
                nodes.Add(start+delta*hi);
                V fromStart=approach-start;
                double t=(fromStart.X*delta.X+fromStart.Y*delta.Y)/(delta.Length*delta.Length);
                V point=start+delta*Math.Max(lo,Math.Min(hi,t));
                double distance=(point-approach).Length;
                if(distance<best) { best=distance;nearest=point; }
            }
            if(nodes.Count==1) return false;
            V local=SleevePoint(approach,forward,across);
            bool inside=Math.Abs(local.X)<length && Math.Abs(local.Y)<width;
            if(inside) nodes[0]=nearest;
            nodes.Add(NestEntrance);
            var path=NestPath(nodes,nodes.Count-1,(from,to)=>CrossesNestBox(
                SleevePoint(from,forward,across),SleevePoint(to,forward,across),length,width));
            if(path==null) return false;
            if((approach-Spider).Length>.001) nestRoute.Add(approach);
            if(inside) nestRoute.Add(nearest);
            nestRoute.AddRange(path);
            return true;
        }
        V SleeveApproach(Area area) {
            // A long perimeter return stays beside the edge until it is
            // close enough to turn around the sleeve.
            double margin=NestClearance,range=116*HomeScale;
            double wallX=Corner.EndsWith("Right")?area.X+area.W-margin:area.X+margin;
            double wallY=Corner.StartsWith("Top")?area.Y+margin:area.Y+area.H-margin;
            if(Math.Abs(Spider.X-wallX)<12*HomeScale && Math.Abs(Spider.Y-Home.Y)>range)
                return area.Clamp(new V(Spider.X,Home.Y+Math.Sign(Spider.Y-Home.Y)*range),margin);
            if(Math.Abs(Spider.Y-wallY)<12*HomeScale && Math.Abs(Spider.X-Home.X)>range)
                return area.Clamp(new V(Home.X+Math.Sign(Spider.X-Home.X)*range,Spider.Y),margin);
            return Spider;
        }
        V SleevePoint(V point,V forward,V across) {
            V offset=point-Home;
            return new V(offset.X*forward.X+offset.Y*forward.Y,offset.X*across.X+offset.Y*across.Y);
        }
        static List<V> NestPath(IList<V> nodes,int finish,Func<V,V,bool> blocked) {
            int count=nodes.Count;
            var costs=new double[count];
            var previous=new int[count];
            var used=new bool[count];
            for(int i=1;i<count;i++) { costs[i]=double.MaxValue;previous[i]=-1; }
            for(int k=0;k<count;k++) {
                int at=-1;for(int i=0;i<count;i++) if(!used[i] && (at<0 || costs[i]<costs[at])) at=i;
                if(at<0 || costs[at]==double.MaxValue) break;used[at]=true;
                for(int next=1;next<count;next++) {
                    if(used[next] || blocked(nodes[at],nodes[next])) continue;
                    double cost=costs[at]+(nodes[next]-nodes[at]).Length;
                    if(cost<costs[next]) { costs[next]=cost;previous[next]=at; }
                }
            }
            if(previous[finish]<0) return null;
            var path=new List<V>();
            for(int at=finish;at>0;at=previous[at]) path.Add(nodes[at]);
            path.Reverse();
            return path;
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
            if(AreaAt(Spider)!=HomeArea || cornerWaypoint.HasValue) {
                BurstMove(HasCornerSleeve?NestEntrance:Home,190,dt,false);
                return;
            }
            if(nestRoute==null) PlanNestEntry();
            if(nestWaypoint<nestRoute.Count) {
                if(BurstMove(nestRoute[nestWaypoint],150,dt,false)) nestWaypoint++;
                return;
            }
            nestInside=true;Face(Home);Enter(Mood.EnterNest);
        }
        void CrawlIntoNest(double dt) {
            if(HasCornerSleeve) desiredAngle=NestAngle;
            else Face(Home);
            // Hold at the mouth to finish turning, then keep a deliberate gait.
            if(StateTime<.3 || Math.Abs(AngleDelta(Angle,desiredAngle))>8) { Speed=0;return; }
            bool arrived=HasCornerSleeve?CrawlSleeve(Home,38,dt):Move(Home,38,dt);
            if(arrived) { desiredAngle=NestAngle;Enter(Mood.Sleep); }
        }
        void CrawlOutOfNest(double dt) {
            desiredAngle=NestAngle;
            if(nestRest>0 || StateTime<.3 || Math.Abs(AngleDelta(Angle,desiredAngle))>8) { Speed=0;return; }
            bool arrived=HasCornerSleeve?CrawlSleeve(NestExit,42,dt):Move(NestExit,42,dt);
            if(arrived) { nestInside=false;decision=Between(4,8);Enter(HasFly && !GuestPrey?Mood.Notice:Mood.Rest); }
        }
        bool CrawlSleeve(V destination,double pace,double dt) {
            // The short corridor stays in the home area. Use the sleeve's
            // fitted pose here so the normal walking margin does not block it.
            V delta=destination-Spider;
            double distance=delta.Length;
            Speed=pace*HomeScale;
            double step=Math.Min(distance,Speed*dt);
            if(distance>.000001) Spider+=delta*(step/distance);
            StepPhase+=step/(4.5*DrawBodyScale);
            if(step<distance) return false;
            Spider=destination;
            Speed=0;
            return true;
        }
    }
}
