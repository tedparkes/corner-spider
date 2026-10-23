using System;

namespace CornerSpider {
    public enum WebVisit { None, Building, Waiting, Approach, Plucking, Attack, Fading }
    public partial class World {
        public bool WebVisits=true;
        public WebVisit WebStage { get; private set; }
        public V GuestCorner,GuestCenter,GuestInward;
        public double GuestScale,GuestProgress,GuestOpacity,GuestAge,GuestStageAge,GuestBuildTime,GuestWaitTime;
        public int GuestArea,GuestCornerIndex,GuestTaps;
        public double GuestLure { get; private set; }
        public double NextWebVisit=100;
        V guestStop,guestDetour,guestResidentGoal;
        double guestResponse,previousPulse;
        bool guestReachedDetour;
        static readonly double[] guestNodeX={0,76,183,185,156,68,0,0,108,51,127,149,108,48,39},guestNodeY={0,0,0,83,166,188,188,91,108,43,38,91,149,137,85};
        static readonly int[] threadFrom={1,10,3,5,13,7,9,2,0,6,7,2,3,4,4,5,5,6,7,0,9,10,11,12,13,14,8,8,8,8,8,8,9,10,11,12,13,14,8,8};
        static readonly int[] threadTo={10,3,5,13,7,9,2,4,9,13,10,10,11,11,12,12,13,13,14,14,10,12,13,14,9,11,9,10,11,12,13,14,11,12,13,14,10,10,0,4};
        public bool GuestPrey { get { return WebStage!=WebVisit.None && WebStage!=WebVisit.Fading && HasFly; } }
        public bool GuestPassive { get { return WebStage==WebVisit.Building || WebStage==WebVisit.Waiting; } }
        public bool PreyBlocksPlay { get { return HasFly && !GuestPassive; } }
        public double GuestPulse { get {
            if(WebStage!=WebVisit.Plucking) return 0;
            double beat=GuestStageAge%3.2;
            if(beat<.32) return Math.Sin(beat/.32*Math.PI);
            if(GuestStageAge>4 && beat>=.55 && beat<.87) return Math.Sin((beat-.55)/.32*Math.PI);
            return 0;
        } }
        public string GuestStatus { get {
            switch(WebStage) {
                case WebVisit.Building:return "A neighbor is weaving";
                case WebVisit.Waiting:return "A quiet corner web";
                case WebVisit.Approach:return "Portia is sneaking closer";
                case WebVisit.Plucking:return "Portia is plucking the web";
                case WebVisit.Attack:return "Portia's ambush";
                default:return "";
            }
        } }
        public V GuestNode(int index) {
            int i=(index%15+15)%15;
            return GuestCorner+new V((GuestInward.X>0?1:-1)*guestNodeX[i],(GuestInward.Y>0?1:-1)*guestNodeY[i])*GuestScale;
        }
        public V GuestThreadStart(int index) {
            return GuestNode(threadFrom[Math.Max(0,Math.Min(39,index))]);
        }
        public V GuestThreadEnd(int index) {
            return GuestNode(threadTo[Math.Max(0,Math.Min(39,index))]);
        }
        public bool StartWebVisit() {
            if(!HuntsSpiders || !WebVisits || HasFly || WebStage!=WebVisit.None) return false;
            int nearest=NearestCorner(AreaAt(Spider),Spider);double best=double.MaxValue;bool found=false;
            for(int area=0;area<Areas.Count;area++) for(int corner=0;corner<4;corner++) {
                var a=Areas[area];double s=Scale*a.Dpi;
                if(a.W<450*s || a.H<450*s) continue;
                if(area==AreaAt(Spider) && corner==nearest) continue;
                V inward=new V(corner==0 || corner==3?1:-1,corner<2?1:-1);
                V edge=new V(inward.X>0?a.X:a.X+a.W,inward.Y>0?a.Y:a.Y+a.H);
                V center=edge+inward*(108*s);
                if((center-Home).Length<320*Math.Max(s,HomeScale)) continue;
                double distance=(center-Spider).Length;
                if(distance>=best) continue;
                best=distance;found=true;GuestArea=area;GuestCornerIndex=corner;GuestCorner=edge;GuestCenter=center;GuestInward=inward*(1/Math.Sqrt(2));GuestScale=s;
            }
            if(!found) { NextWebVisit=Clock+120;return false; }
            GuestAge=GuestStageAge=GuestProgress=GuestLure=guestResponse=previousPulse=0;GuestTaps=0;
            GuestBuildTime=Between(180,240);GuestWaitTime=Between(60,120);GuestOpacity=1;
            WebStage=WebVisit.Building;Prey=PreyKind.SmallSpider;HasFly=true;FlyAge=0;Fly=GuestThreadStart(0);BeginPrey(PreyKind.SmallSpider);
            NextFly=Math.Max(NextFly,Clock+GuestBuildTime+GuestWaitTime+60);return true;
        }
        public void CancelWebVisit() {
            bool owned=GuestPrey;
            WebStage=WebVisit.None;GuestOpacity=GuestProgress=GuestStageAge=GuestLure=previousPulse=0;
            NextWebVisit=Clock+120;
            if(owned) HasFly=false;
            if(State==Mood.WebApproach || State==Mood.WebPluck || (owned && jumpPurpose=="fly" && (State==Mood.Reach || State==Mood.Crouch || State==Mood.Jump))) {
                jumpPurpose="ground";SilkOpacity=0;Enter(Mood.Rest);decision=2;
            }
        }
        void FinishWebVisit() {
            if(WebStage==WebVisit.None) return;
            WebStage=WebVisit.Fading;GuestStageAge=0;NextWebVisit=Clock+Between(360,600);NextFly=Math.Max(NextFly,Clock+FlyInterval);
        }
        void MoveGuest(V point,double dt) {
            V before=Fly,delta=point-Fly;Fly=point;double distance=delta.Length;
            PreyWalking=distance>.001;PreySteps+=distance/Math.Max(.01,1.6*GuestScale);
            if(PreyWalking) PreyAngle=Math.Atan2(delta.Y,delta.X)*180/Math.PI+90;
        }
        void TickWebVisit(double dt) {
            if(WebStage==WebVisit.None) return;
            if(!HuntsSpiders || !WebVisits || GuestAge>900) { CancelWebVisit();return; }
            GuestAge+=dt;GuestStageAge+=dt;PreyWalking=false;
            if(WebStage==WebVisit.Fading) {
                GuestOpacity=Math.Max(0,1-GuestStageAge/12);
                if(GuestOpacity<=0) WebStage=WebVisit.None;
                return;
            }
            if(!HasFly) { FinishWebVisit();return; }
            if(WebStage==WebVisit.Building) {
                GuestProgress=Math.Min(1,GuestStageAge/GuestBuildTime);
                double path=Math.Min(39.999999,GuestProgress*40),part=path-Math.Floor(path);int thread=(int)path;
                V start=GuestThreadStart(thread),end=GuestThreadEnd(thread),previous=thread==0?start:GuestThreadEnd(thread-1);
                MoveGuest(part<.35?previous+(start-previous)*(part/.35):start+(end-start)*((part-.35)/.65),dt);
                if(GuestProgress>=1) { WebStage=WebVisit.Waiting;GuestStageAge=0; }
            } else if(WebStage==WebVisit.Waiting || WebStage==WebVisit.Approach) {
                V delta=GuestCenter-Fly;MoveGuest(Fly+delta*(Math.Min(delta.Length,12*GuestScale*dt)/Math.Max(.001,delta.Length)),dt);
            } else if(WebStage==WebVisit.Plucking) {
                double pulse=GuestPulse;
                if(pulse>.5 && previousPulse<=.5) {
                    GuestTaps++;
                    // Several probes get no response. Repeat the useful double taps.
                    if(GuestTaps>=4) guestResponse=Math.Min(1,guestResponse+.2);
                }
                previousPulse=pulse;GuestLure=Math.Min(guestResponse,GuestLure+dt*.16);
                MoveGuest(GuestCenter+(guestResidentGoal-GuestCenter)*GuestLure,dt);
            }
        }
        void BeginWebApproach() {
            ClearPlay();WebStage=WebVisit.Approach;GuestStageAge=0;guestReachedDetour=false;
            double s=Scale*Areas[GuestArea].Dpi;
            guestStop=Clamp(GuestCenter+GuestInward*(84*GuestScale+42*s));
            V tangent=new V(-GuestInward.Y,GuestInward.X);
            guestDetour=Areas[GuestArea].Clamp(guestStop+GuestInward*(90*s)+tangent*(80*s),SpiderPose.Radius*s);
            guestResidentGoal=GuestCenter+GuestInward*(68*GuestScale);
            Enter(Mood.WebApproach);
        }
        void SneakToWeb(double dt) {
            if(!GuestPrey) { Wake();return; }
            V target=guestReachedDetour?guestStop:guestDetour;
            double distance=(target-Spider).Length/BodyScale;
            if(BurstMove(target,distance>400?135:distance>140?85:44,dt,true)) {
                if(!guestReachedDetour) guestReachedDetour=true;
                else { WebStage=WebVisit.Plucking;GuestStageAge=0;Face(GuestCenter);Enter(Mood.WebPluck); }
            }
        }
        void PluckWeb() {
            if(!GuestPrey) { Wake();return; }
            Face(Fly);
            if(GuestLure>=1 && GuestTaps>=8) { WebStage=WebVisit.Attack;GuestStageAge=0;PrepareJump(Fly,"fly"); }
        }
    }
}
