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
        public double GuestWebFactor { get; private set; }
        public double GuestWebScale { get { return GuestScale*GuestWebFactor; } }
        public double GuestWebExtent { get { return 188*GuestWebScale; } }
        public int GuestPluckSpoke { get; private set; }
        public int GuestAnchorSpoke { get; private set; }
        public int GuestPluckingSide { get; private set; }
        public V GuestPluckPoint { get; private set; }
        public V GuestFootContact { get {
            V axis=GuestCenter-GuestPluckPoint;
            return GuestPluckPoint+axis*(2.5*GuestScale*GuestPulse/Math.Max(.0001,axis.Length));
        } }
        public V GuestPluckPosition { get { return guestStop; } }
        public bool GuestFadeStarted { get; private set; }
        public double NextWebVisit=100;
        V guestStop,guestDetour,guestResidentGoal;
        readonly V[] guestSupportFeet=new V[8];
        public V GuestSupportFoot(int side,int leg) { return guestSupportFeet[(side>0?4:0)+leg]; }
        double guestFadeAge;
        bool guestReachedDetour;
        static readonly double[] guestNodeX={0,76,183,185,156,68,0,0,108,51,127,149,108,48,39},guestNodeY={0,0,0,83,166,188,188,91,108,43,38,91,149,137,85};
        public bool GuestPrey { get { return WebStage!=WebVisit.None && WebStage!=WebVisit.Fading && HasFly; } }
        public bool GuestPassive { get { return WebStage==WebVisit.Building || WebStage==WebVisit.Waiting; } }
        public bool PreyBlocksPlay { get { return HasFly && !GuestPassive; } }
        const double GuestSettleTime=1.25,GuestSignalPeriod=3.6;
        static readonly double[] guestApproachSteps={0,.12,.28,.48,.72,1};
        public double GuestSignalPhase { get { return Math.Max(0,GuestStageAge-GuestSettleTime)%GuestSignalPeriod; } }
        public double GuestPulse { get {
            if(WebStage!=WebVisit.Plucking || GuestStageAge<GuestSettleTime || GuestStageAge>=GuestSettleTime+5*GuestSignalPeriod) return 0;
            double beat=GuestSignalPhase;
            return beat<.65?Math.Pow(Math.Sin(beat/.65*Math.PI),2):0;
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
            return GuestCorner+new V((GuestInward.X>0?1:-1)*guestNodeX[i],(GuestInward.Y>0?1:-1)*guestNodeY[i])*GuestWebScale;
        }
        static bool InGuestBox(V point,V corner,V inward,double extent,double margin) {
            V p=point-corner;double x=p.X*(inward.X>0?1:-1),y=p.Y*(inward.Y>0?1:-1);
            return x>=-margin && y>=-margin && x<extent+margin && y<extent+margin;
        }
        public bool GuestContains(V point,double margin) {
            return WebStage!=WebVisit.None && GuestOpacity>0 && InGuestBox(point,GuestCorner,GuestInward,GuestWebExtent,margin);
        }
        V GuestLocal(double x,double y) { return GuestCorner+new V(x*(GuestInward.X>0?1:-1),y*(GuestInward.Y>0?1:-1)); }
        V SafeGuestPerch(V preferred) {
            if(!GuestContains(preferred,SpiderPose.Radius*BodyScale+12*GuestScale)) return preferred;
            V best=Home;double distance=double.MaxValue;
            for(int area=0;area<Areas.Count;area++) for(int corner=0;corner<4;corner++) {
                V p=Perch(area,corner);if(GuestContains(p,SpiderPose.Radius*Scale*Areas[area].Dpi+12*GuestScale)) continue;
                double d=(p-Spider).Length;if(d<distance) { best=p;distance=d; }
            }
            return best;
        }
        V AvoidGuestWeb(V destination) {
            if(WebStage==WebVisit.None || GuestOpacity<=0 ||
               (State!=Mood.Walk && State!=Mood.Return && State!=Mood.EnterNest && State!=Mood.ExitNest && State!=Mood.WebApproach) ||
               (State==Mood.WebApproach && guestReachedDetour)) return destination;
            double margin=SpiderPose.Radius*Math.Max(BodyScale,GuestScale)+12*GuestScale;
            double edge=GuestWebExtent+margin+2*GuestScale;
            if(GuestContains(Spider,margin)) {
                // A carried pet or the successful hunter steps out by the nearest
                // open edge before resuming its ordinary perimeter route.
                V local=Spider-GuestCorner;double x=local.X*(GuestInward.X>0?1:-1),y=local.Y*(GuestInward.Y>0?1:-1);
                return GuestLocal(x>=y?edge:x,x>=y?y:edge);
            }
            V center=GuestLocal(GuestWebExtent*.5,GuestWebExtent*.5);
            double half=GuestWebExtent*.5+margin;
            if(CrossesNestBox(Spider-center,destination-center,half,half)) return GuestLocal(edge,edge);
            return destination;
        }
        public V GuestPatternPoint(V p) { return GuestLocal(p.X*GuestWebScale,p.Y*GuestWebScale); }
        public V GuestThreadStart(int index) { return GuestPatternPoint(GuestWebPattern.Strand(index).A); }
        public V GuestThreadEnd(int index) { return GuestPatternPoint(GuestWebPattern.Strand(index).D); }
        public bool StartWebVisit() {
            if(!HuntsSpiders || !WebVisits || HasFly || WebStage!=WebVisit.None) return false;
            int nearest=NearestCorner(AreaAt(Spider),Spider);double best=double.MaxValue;bool found=false;
            for(int area=0;area<Areas.Count;area++) for(int corner=0;corner<4;corner++) {
                var a=Areas[area];double s=Scale*a.Dpi;
                if(a.W<450*s || a.H<450*s) continue;
                if(area==AreaAt(Spider) && corner==nearest) continue;
                V inward=new V(corner==0 || corner==3?1:-1,corner<2?1:-1);
                V edge=new V(inward.X>0?a.X:a.X+a.W,inward.Y>0?a.Y:a.Y+a.H);
                // Three times the old span where space permits; retain a clear
                // approach lane on small work areas or at large pet sizes.
                double factor=Math.Min(3,(Math.Min(a.W,a.H)/s-220)/188);
                double extent=188*s*factor;
                if(InGuestBox(Home,edge,inward,extent,150*HomeScale) || InGuestBox(Spider,edge,inward,extent,SpiderPose.Radius*BodyScale+12*s)) continue;
                V hub=GuestWebPattern.Hub;
                V center=edge+new V(inward.X*hub.X,inward.Y*hub.Y)*(s*factor);
                if((center-Home).Length<320*Math.Max(s,HomeScale)) continue;
                // Rank the corner itself, so enlarging its inward web does not
                // unexpectedly prefer a distant corner over an adjacent monitor.
                double distance=(edge+inward*(108*s)-Spider).Length;
                if(distance>=best) continue;
                best=distance;found=true;GuestArea=area;GuestCornerIndex=corner;GuestCorner=edge;GuestCenter=center;GuestInward=inward*(1/Math.Sqrt(2));GuestScale=s;GuestWebFactor=factor;
            }
            if(!found) { NextWebVisit=Clock+120;return false; }
            // Use the outermost safe perch at a supported side. The contact is
            // ahead of the hunter on the spoke leading to the actual edge anchor.
            V topStop,topGoal,topContact,sideStop,sideGoal,sideContact;
            bool top=PlanPlucking(true,out topStop,out topGoal,out topContact);
            bool side=PlanPlucking(false,out sideStop,out sideGoal,out sideContact);
            if(!top && !side) { NextWebVisit=Clock+120;return false; }
            bool useTop=top && (!side || (topStop-Spider).Length<=(sideStop-Spider).Length);
            GuestAnchorSpoke=useTop?GuestWebPattern.TopBridgeAnchor:GuestWebPattern.SideBridgeAnchor;
            GuestPluckSpoke=GuestAnchorSpoke;
            GuestPluckPoint=useTop?topContact:sideContact;
            guestStop=useTop?topStop:sideStop;guestResidentGoal=useTop?topGoal:sideGoal;
            V look=GuestCenter-guestStop,foot=GuestPluckPoint-guestStop;
            GuestPluckingSide=look.X*foot.Y-look.Y*foot.X>0?1:-1;
            GuestAge=GuestStageAge=GuestProgress=GuestLure=0;GuestTaps=0;
            GuestBuildTime=Between(180,240);GuestWaitTime=Between(60,120);GuestOpacity=1;
            GuestFadeStarted=false;guestFadeAge=0;
            WebStage=WebVisit.Building;Prey=PreyKind.SmallSpider;HasFly=true;FlyAge=0;Fly=Areas[GuestArea].Clamp(GuestThreadStart(0),18*SmallSpiderSize*GuestScale);BeginPrey(PreyKind.SmallSpider);
            Target=SafeGuestPerch(Target);cornerWaypoint=null;
            if(State==Mood.Walk || State==Mood.Return || State==Mood.Inspect) PlanCornerRoute(State==Mood.Return?Home:Target);
            NextFly=Math.Max(NextFly,Clock+GuestBuildTime+GuestWaitTime+60);return true;
        }
        public void CancelWebVisit() {
            bool owned=GuestPrey;
            WebStage=WebVisit.None;GuestOpacity=GuestProgress=GuestStageAge=GuestLure=0;GuestTaps=0;
            GuestFadeStarted=false;guestFadeAge=0;
            NextWebVisit=Clock+120;
            if(owned) HasFly=false;
            if(State==Mood.WebApproach || State==Mood.WebPluck || (owned && jumpPurpose=="fly" && (State==Mood.Reach || State==Mood.Crouch || State==Mood.Jump))) {
                jumpPurpose="ground";SilkOpacity=0;Enter(Mood.Rest);decision=2;
            }
        }
        void FinishWebVisit() {
            if(WebStage==WebVisit.None) return;
            WebStage=WebVisit.Fading;GuestStageAge=guestFadeAge=0;GuestFadeStarted=false;NextWebVisit=Clock+Between(360,600);NextFly=Math.Max(NextFly,Clock+FlyInterval);
        }
        void MoveGuest(V point,double dt) {
            point=Areas[GuestArea].Clamp(point,18*SmallSpiderSize*GuestScale);
            V before=Fly,delta=point-Fly;Fly=point;double distance=delta.Length;
            PreyWalking=distance>.001;PreySteps+=distance/Math.Max(.01,1.6*GuestScale*SmallSpiderSize);
            if(PreyWalking) PreyAngle=Math.Atan2(delta.Y,delta.X)*180/Math.PI+90;
        }
        void TickWebVisit(double dt) {
            if(WebStage==WebVisit.None) return;
            if(!HuntsSpiders || !WebVisits || (GuestAge>900 && WebStage!=WebVisit.Fading)) { CancelWebVisit();return; }
            GuestAge+=dt;GuestStageAge+=dt;PreyWalking=false;
            if(WebStage==WebVisit.Fading) {
                if(!GuestFadeStarted) {
                    if(GuestContains(Spider,SpiderPose.Radius*BodyScale+12*GuestScale)) return;
                    GuestFadeStarted=true;guestFadeAge=0;
                }
                guestFadeAge+=dt;GuestOpacity=Math.Max(0,1-guestFadeAge/12);
                if(GuestOpacity<=0) WebStage=WebVisit.None;
                return;
            }
            if(!HasFly) { FinishWebVisit();return; }
            if(WebStage==WebVisit.Building) {
                GuestProgress=Math.Min(1,GuestStageAge/GuestBuildTime);
                // Length-weighted travel prevents short spiral segments and long
                // foundation threads from giving the builder different speeds.
                MoveGuest(GuestPatternPoint(GuestWebPattern.Builder(GuestProgress)),dt);
                if(GuestProgress>=1) { WebStage=WebVisit.Waiting;GuestStageAge=0; }
            } else if(WebStage==WebVisit.Waiting || WebStage==WebVisit.Approach) {
                V delta=GuestCenter-Fly;MoveGuest(Fly+delta*(Math.Min(delta.Length,12*GuestScale*dt)/Math.Max(.001,delta.Length)),dt);
            } else if(WebStage==WebVisit.Plucking) {
                double age=GuestStageAge-GuestSettleTime;
                if(age<0) return;
                int cycle=Math.Min(4,(int)(age/GuestSignalPeriod));double phase=GuestSignalPhase;
                GuestTaps=Math.Min(5,(int)Math.Floor(Math.Max(0,age-.325)/GuestSignalPeriod)+(age>=.325?1:0));
                if(phase>=.325 || GuestTaps>1) {
                    V toward=GuestPluckPoint-Fly;double angle=Math.Atan2(toward.Y,toward.X)*180/Math.PI+90;
                    PreyAngle=(PreyAngle+Math.Max(-220*dt,Math.Min(220*dt,AngleDelta(PreyAngle,angle)))+360)%360;
                }
                // Each signal gets a turn, one short stalking advance, then a
                // still listening pause. Later advances grow a little bolder.
                double step=Math.Max(0,Math.Min(1,(phase-1.25)/1.3));step=step*step*(3-2*step);
                GuestLure=guestApproachSteps[cycle]+(guestApproachSteps[cycle+1]-guestApproachSteps[cycle])*step;
                MoveGuest(GuestCenter+(guestResidentGoal-GuestCenter)*GuestLure,dt);
            }
        }
        public bool GuestOnSilk(V point) {
            if(GuestWebScale<=0) return false;
            V relative=(point-GuestCorner)*(1/GuestWebScale);
            return GuestWebPattern.InsideSilk(new V(relative.X*(GuestInward.X>0?1:-1),relative.Y*(GuestInward.Y>0?1:-1)));
        }
        bool PlanPlucking(bool top,out V stop,out V goal,out V contact) {
            stop=goal=contact=GuestCenter;
            double s=Scale*Areas[GuestArea].Dpi;
            V anchor=GuestPatternPoint(GuestWebPattern.Rim(top?GuestWebPattern.TopBridgeAnchor:GuestWebPattern.SideBridgeAnchor));
            V axis=anchor-GuestCenter;
            // Search inward from the attached end, never from the free middle
            // of the curved bridge. Keep the same full-body desktop clearance.
            for(double edge=GuestWebExtent;edge>SpiderPose.Radius*s;edge-=s) {
                V perch=GuestLocal(top?edge:SpiderPose.Radius*s,top?SpiderPose.Radius*s:edge);
                if(!GuestOnSilk(perch)) continue;
                if((Areas[GuestArea].Clamp(perch,SpiderPose.Radius*s)-perch).Length>.000001) continue;
                V inward=GuestCenter-perch;
                if(inward.Length<65*s) continue;
                V reach=perch+inward*(36*s/inward.Length)-GuestCenter;
                double fraction=(reach.X*axis.X+reach.Y*axis.Y)/(axis.X*axis.X+axis.Y*axis.Y);
                V touch=GuestCenter+axis*fraction,offset=touch-perch;
                if(fraction<=0 || fraction>=1 || offset.Length>55*s || offset.X*inward.X+offset.Y*inward.Y<20*s*inward.Length) continue;
                for(int step=0;step<18;step++) {
                    V candidate=GuestCenter+(touch-GuestCenter)*(.45-step*.025),direction=candidate-perch;
                    if(direction.Length<35*s) continue;
                    V landing=Areas[GuestArea].Clamp(candidate-direction*(25*s/direction.Length),SpiderPose.Radius*s);
                    if((landing-candidate).Length<32*s) { stop=perch;goal=candidate;contact=touch;return true; }
                }
            }
            return false;
        }
        void BeginWebApproach() {
            ClearPlay();WebStage=WebVisit.Approach;GuestStageAge=0;guestReachedDetour=false;
            double s=Scale*Areas[GuestArea].Dpi;
            guestDetour=GuestLocal(GuestWebExtent+110*s,GuestWebExtent+40*s);
            Enter(Mood.WebApproach);
        }
        void SneakToWeb(double dt) {
            if(!GuestPrey) { Wake();return; }
            V target=guestReachedDetour?guestStop:guestDetour;
            double distance=(target-Spider).Length/BodyScale;
            if(BurstMove(target,distance>400?135:distance>140?85:44,dt,true)) {
                if(!guestReachedDetour) guestReachedDetour=true;
                else {
                    Face(Fly);
                    // Finish the smooth inward pivot before sending any signals.
                    if(Math.Abs(AngleDelta(Angle,desiredAngle))>2) return;
                    GaitBlend=0;
                    for(int side=-1;side<=1;side+=2) for(int leg=0;leg<4;leg++) {
                        // A slightly flexed stance leaves joint travel for the
                        // small turn needed as the resident comes closer.
                        V foot=SpiderPose.Leg(this,side,leg,SkinCatalog.Get(Species)).Tip*.88;double angle=Angle*Math.PI/180;
                        guestSupportFeet[(side>0?4:0)+leg]=Spider+new V(foot.X*Math.Cos(angle)-foot.Y*Math.Sin(angle),foot.X*Math.Sin(angle)+foot.Y*Math.Cos(angle))*BodyScale;
                    }
                    WebStage=WebVisit.Plucking;GuestStageAge=0;Enter(Mood.WebPluck);
                }
            }
        }
        void PluckWeb() {
            if(!GuestPrey) { Wake();return; }
            Face(Fly);
            if(GuestLure>=1 && GuestTaps>=5 && GuestSignalPhase>=3.05) { WebStage=WebVisit.Attack;GuestStageAge=0;PrepareJump(Fly,"fly"); }
        }
    }
}
