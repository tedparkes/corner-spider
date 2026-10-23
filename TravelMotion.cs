using System;

namespace CornerSpider {
    public partial class World {
        readonly Random travelRandom;
        bool travelReady,travelMoving,travelSideways,lastTravelSideways;
        V travelRoute,travelDestination,travelEnd;
        double travelPause,travelFactor,travelHeading,travelLength;
        public V GaitDirection=new V(0,-1);
        public bool SideStepping { get { return State==Mood.Walk && travelMoving && travelSideways && Speed>1; } }

        double TravelRange(double min,double max) { return min+travelRandom.NextDouble()*(max-min); }
        void ResetTravel() { travelReady=false;travelMoving=false;travelPause=0;travelSideways=false; }

        bool SafeTravelSegment(V end) {
            if((Clamp(end)-end).Length>.01*BodyScale) return false;
            if(WebStage==WebVisit.None || GuestOpacity<=0) return true;
            double margin=SpiderPose.Radius*Math.Max(BodyScale,GuestScale)+12*GuestScale;
            if(GuestContains(Spider,margin)) return false; // Leave occupied silk directly.
            V center=GuestLocal(GuestWebExtent*.5,GuestWebExtent*.5);
            double half=GuestWebExtent*.5+margin;
            return !CrossesNestBox(Spider-center,end-center,half,half);
        }

        void PlanTravel(V destination,V routed) {
            double s=BodyScale,distance=(routed-Spider).Length/s;
            V forward=(routed-Spider)*(1/Math.Max(.0001,(routed-Spider).Length));
            V lateral=new V(-forward.Y,forward.X);
            travelDestination=destination;travelRoute=routed;travelReady=travelMoving=true;
            double choice=travelRandom.NextDouble(),run;
            if(distance<90) { run=TravelRange(14,44);travelFactor=TravelRange(.60,.95); }
            else if(distance<280) { run=TravelRange(40,115);travelFactor=TravelRange(.85,1.25); }
            else if(choice<.23) { run=TravelRange(28,70);travelFactor=TravelRange(.85,1.15); }
            else if(choice<.65) { run=TravelRange(115,245);travelFactor=TravelRange(1.20,1.60); }
            else { run=TravelRange(245,420);travelFactor=TravelRange(1.65,2.10); }

            // A separate motion RNG keeps optional route variation from changing
            // prey timing, invitations and the existing behavior decisions.
            bool mayDeviate=State==Mood.Walk && distance>140 && AreaAt(Spider)==AreaAt(routed);
            travelSideways=mayDeviate && !lastTravelSideways && travelRandom.NextDouble()<.27;
            double sideways=0;
            if(travelSideways) { run=TravelRange(10,24);sideways=TravelRange(24,46);travelFactor=TravelRange(.85,1.20); }
            else if(mayDeviate && distance>run+90) sideways=TravelRange(-24,24);
            if(travelRandom.NextDouble()<.5) sideways=-sideways;
            travelEnd=run>=distance?routed:Spider+forward*(run*s)+lateral*(sideways*s);
            if(Math.Abs(sideways)>.01 && !SafeTravelSegment(travelEnd)) {
                travelEnd=Spider+forward*(run*s)-lateral*(sideways*s);
                if(!SafeTravelSegment(travelEnd)) { travelSideways=false;travelEnd=Spider+forward*(Math.Min(run,distance)*s); }
            }
            // A shared monitor seam can end partway along a run. Its next
            // stopping point must fit the hard edge beyond that seam too.
            if(AreaAt(destination)==AreaAt(Spider)) travelEnd=Clamp(travelEnd);
            lastTravelSideways=travelSideways;
            travelHeading=Math.Atan2(forward.Y,forward.X)*180/Math.PI+90;
            travelLength=(travelEnd-Spider).Length/s;
        }

        bool Travel(V destination,double speed,double dt) {
            V routed=AvoidGuestWeb(Route(destination));double s=BodyScale;
            if((destination-Spider).Length<2*s) { Spider=destination;Speed=0;ResetTravel();return true; }
            if(!travelReady || (destination-travelDestination).Length>2*s || (routed-travelRoute).Length>2*s) PlanTravel(destination,routed);
            if(!travelMoving) {
                Speed=0;Face(routed);travelPause-=dt;
                if(travelPause>0) return false;
                PlanTravel(destination,routed);
            }
            // Retain the deliberate homeward pace around the nest-entry route.
            bool arrived=MoveResolved(destination,travelEnd,Math.Min(State==Mood.Return?190:370,speed*travelFactor),dt,travelSideways?(double?)travelHeading:null);
            if(arrived) { ResetTravel();return true; }
            if((travelEnd-Spider).Length<.01*s) {
                Spider=travelEnd;Speed=0;travelMoving=false;
                travelPause=travelSideways?TravelRange(.16,.42):travelLength>100?TravelRange(.20,.65):TravelRange(.28,.85);
            }
            return false;
        }

        void UpdateGaitDirection(V displacement,double dt) {
            bool ground=State==Mood.Walk || State==Mood.Return || State==Mood.Stalk || State==Mood.WebApproach || State==Mood.EnterNest || State==Mood.ExitNest || State==Mood.SpeciesAct;
            V direction=new V(0,-1);
            if(ground && displacement.Length>.001*BodyScale) {
                double a=Angle*Math.PI/180;
                direction=new V(displacement.X*Math.Cos(a)+displacement.Y*Math.Sin(a),-displacement.X*Math.Sin(a)+displacement.Y*Math.Cos(a))*(1/displacement.Length);
            }
            // At speed the feet track actual motion, including a lateral scuttle.
            // Blend only as they settle so a pause does not snap the stance.
            if(ground && displacement.Length>.001*BodyScale) GaitDirection=direction;
            else GaitDirection+=(direction-GaitDirection)*Math.Min(1,dt*9);
        }
    }
}
