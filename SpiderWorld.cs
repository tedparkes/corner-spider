using System;
using System.Collections.Generic;

namespace CornerSpider {
    public struct V {
        public double X, Y;
        public V(double x,double y) { X=x; Y=y; }
        public double Length { get { return Math.Sqrt(X*X+Y*Y); } }
        public static V operator +(V a,V b) { return new V(a.X+b.X,a.Y+b.Y); }
        public static V operator -(V a,V b) { return new V(a.X-b.X,a.Y-b.Y); }
        public static V operator *(V a,double b) { return new V(a.X*b,a.Y*b); }
    }
    public struct Area {
        public double X,Y,W,H,Dpi;
        public Area(double x,double y,double w,double h,double dpi) { X=x;Y=y;W=w;H=h;Dpi=dpi; }
        public bool Contains(V p) { return p.X>=X && p.X<=X+W && p.Y>=Y && p.Y<=Y+H; }
        public V Center { get { return new V(X+W/2,Y+H/2); } }
        public V Clamp(V p,double margin) {
            double mx=Math.Min(margin,W/2),my=Math.Min(margin,H/2);
            return new V(Math.Max(X+mx,Math.Min(X+W-mx,p.X)),Math.Max(Y+my,Math.Min(Y+H-my,p.Y)));
        }
    }
    public enum Mood { Rest, Walk, Groom, Return, Sleep, Notice, Stalk, Aim, Crouch, Jump, Land, Eat, Ride, Inspect, Reach, Dismount, EnterNest, ExitNest, WebApproach, WebPluck, SpeciesAct, Struggle }
    public partial class World {
        public V Spider,Home,Target,Fly,Cursor,LastCursor,JumpStart,JumpEnd;
        public Mood State=Mood.Rest;
        public double Width,Height,Clock,StateTime,NextFly=35,FlyAge,FlyPhase;
        public double StillTime,RideCooldown,JumpTime,Angle,LookAngle,Speed,StepPhase,Scale=1,Web=.15;
        public double GaitBlend,AngularSpeed,JumpDuration=.28;
        // Shared by every species and prey type. With the 25-pixel mouth offset,
        // this gives a ~200-pixel leap, about 3.5 times the previous 57 pixels.
        const double PreyPounceRange=225;
        public double FlyInterval=100,InviteDelay=4.5,CarryTolerance=850,AttentionDistance=185,DismissDistance=230;
        public const double DefaultFingerTone=.8;
        public double FingerTone=DefaultFingerTone;
        public double WebVisibility=1;
        public double AttentionRadius { get { return AttentionDistance*pixelScale; } }
        public double DismissRadius { get { return Math.Max(AttentionDistance+20,DismissDistance)*pixelScale; } }
        public bool HasFly,AutoFeed=true,CursorPlay=true;
        public int Meals,HomeArea;
        public string Corner="TopRight";
        public readonly List<Area> Areas=new List<Area>();
        readonly Random random;
        struct Sample { public V P; public double T; public Sample(V p,double t) { P=p;T=t; } }
        readonly Queue<Sample> samples=new Queue<Sample>();
        string jumpPurpose="ground";
        bool directAttack,holdingMeal;
        double decision=12,napAfter=100,desiredAngle,rideDuration=8,pixelScale=1,jumpScale=1,jumpStartAngle;
        bool cursorValid;
        bool cursorNearby,burstMoving=true;
        double nextInspection,burstRemaining=.24,scanAt;
        bool quietInspection;
        Mood inspectionResume=Mood.Rest;
        double attentionRearmAt;
        V? cornerWaypoint;
        public double BodyScale { get { return Scale*pixelScale; } }
        public double HomeScale { get { return Scale*Areas[Math.Min(HomeArea,Areas.Count-1)].Dpi; } }
        // The retreat runs diagonally between the two adjacent desktop edges.
        public double NestAngle { get { return Corner.StartsWith("Top")?(Corner.EndsWith("Right")?135:225):(Corner.EndsWith("Right")?45:315); } }
        double nestRest;
        public double NestTuck { get { return nestRest*nestRest*(3-2*nestRest); } }
        public World(double width,double height,int seed) {
            random=new Random(seed); Width=width;Height=height;
            Areas.Add(new Area(0,0,width,height,1));UpdateHome();Spider=Target=Home;Cursor=LastCursor=new V(-1000,-1000);
        }
        public static bool Finite(double n) { return !double.IsNaN(n) && !double.IsInfinity(n); }
        public double Between(double min,double max) { return min+random.NextDouble()*(max-min); }
        public int AreaAt(V p) {
            int index=0;double distance=double.MaxValue;
            for(int i=0;i<Areas.Count;i++) { if(Areas[i].Contains(p)) return i;double d=(Areas[i].Clamp(p,0)-p).Length;if(d<distance) { distance=d;index=i; } }
            return index;
        }
        public V Clamp(V p) {
            var a=Areas[AreaAt(p)];
            // No inset at a shared seam: the spider can walk across contiguous monitors.
            double margin=SpiderPose.Radius*Scale*Math.Max(a.Dpi,pixelScale);
            V bounded=a.Clamp(p,0),safe=a.Clamp(p,margin);
            if(InsideAny(new V(bounded.X-margin,bounded.Y)) && InsideAny(new V(bounded.X+margin,bounded.Y))) safe.X=bounded.X;
            if(InsideAny(new V(bounded.X,bounded.Y-margin)) && InsideAny(new V(bounded.X,bounded.Y+margin))) safe.Y=bounded.Y;
            return safe;
        }
        public bool InsideAny(V p) { foreach(var a in Areas) if(a.Contains(p)) return true;return false; }
        public V Perch(int area,int corner) {
            var a=Areas[area];double s=Scale*a.Dpi;
            return a.Clamp(new V(corner==0 || corner==3?a.X+72*s:a.X+a.W-72*s,corner<2?a.Y+65*s:a.Y+a.H-65*s),SpiderPose.Radius*s);
        }
        public int NearestCorner(int area,V point) {
            int best=0;double distance=double.MaxValue;
            for(int i=0;i<4;i++) { double d=(Perch(area,i)-point).Length;if(d<distance) { distance=d;best=i; } }return best;
        }
        public double CornerDistance { get { int a=AreaAt(Spider);return (Spider-Perch(a,NearestCorner(a,Spider))).Length; } }
        void ChoosePerch() {
            int area=AreaAt(Spider),corner=NearestCorner(area,Spider);
            double choice=random.NextDouble();
            if(Areas.Count>1 && choice<.10) { area=(area+1)%Areas.Count;corner=NearestCorner(area,Spider); }
            else if(choice<.28) corner=(corner+(random.NextDouble()<.5?1:3))%4;
            // Most excursions are tiny adjustments within the current sheltered corner.
            Target=Clamp(Perch(area,corner)+new V(Between(-22,22),Between(-18,18))*Scale*Areas[area].Dpi);
            if(SpeciesTraits && behavior.HomeAffinity>0 && random.NextDouble()<behavior.HomeAffinity) Target=Home;
            Target=SafeGuestPerch(Target);
            PlanCornerRoute(Target);
            Enter(Mood.Walk);
        }
        void PlanCornerRoute(V target) {
            cornerWaypoint=null;int area=AreaAt(Spider);if(area!=AreaAt(target)) return;
            int start=NearestCorner(area,Spider),end=NearestCorner(area,target);
            if((start+2)%4==end && (target-Perch(area,end)).Length<150*BodyScale) {
                V a=Perch(area,(start+1)%4),b=Perch(area,(start+3)%4);
                double margin=SpiderPose.Radius*BodyScale+12*GuestScale;
                bool safeA=!GuestContains(a,margin),safeB=!GuestContains(b,margin);
                if(safeA || safeB) cornerWaypoint=!safeB || (safeA && (a-Spider).Length+(target-a).Length<(b-Spider).Length+(target-b).Length)?a:b;
            }
        }
        public void UpdateHome() {
            HomeArea=Math.Max(0,Math.Min(Areas.Count-1,HomeArea));var a=Areas[HomeArea];double s=HomeScale;
            // Leave room for a whole spider to line up outside either end. The
            // cocoon itself keeps its size and its silk remains attached to the corner.
            V home=a.Clamp(new V(Corner.EndsWith("Right")?a.X+a.W-160*s:a.X+160*s,Corner.StartsWith("Top")?a.Y+160*s:a.Y+a.H-160*s),160*s);
            if((home-Home).Length>.01) nestInside=false;
            Home=home;ResetNestRoute();
        }
        public void ConfigureDisplays(Area[] areas,int homeArea) {
            if(areas==null || areas.Length==0) return;
            Areas.Clear();Areas.AddRange(areas);HomeArea=homeArea;
            Width=Height=1;foreach(var a in Areas) { Width=Math.Max(Width,a.X+a.W);Height=Math.Max(Height,a.Y+a.H); }
            Reflow();
        }
        public void Resize(double width,double height) {
            Width=Finite(width)?Math.Max(1,width):1280;Height=Finite(height)?Math.Max(1,height):720;
            Areas.Clear();Areas.Add(new Area(0,0,Width,Height,1));HomeArea=0;Reflow();
        }
        void Reflow() {
            CancelWebVisit();ClearPlay();SilkOpacity=0;
            cornerWaypoint=null;
            if(!Finite(Scale)) Scale=1;Scale=Math.Max(.2,Math.Min(1.8,Scale));UpdateHome();
            Spider=Clamp(Spider);Target=Clamp(Target);Fly=Clamp(Fly);JumpStart=Clamp(JumpStart);JumpEnd=Clamp(JumpEnd);
            if(State==Mood.Sleep) { Spider=Home;nestInside=true; }
            if(State==Mood.EnterNest || State==Mood.ExitNest) { nestInside=false;Target=Home;Enter(Mood.Return); }
            if(State==Mood.Return) PlanCornerRoute(Home);
            pixelScale=Areas[AreaAt(Spider)].Dpi;ResetCursor();
            if(State==Mood.Ride || State==Mood.Jump || State==Mood.Crouch || State==Mood.Reach || State==Mood.Dismount) { RideCooldown=25;decision=2;Enter(Mood.Rest); }
        }
        public void SetScale(double scale) { Scale=scale;Reflow(); }
        void Enter(Mood state) {
            State=state;StateTime=0;
            if(state!=Mood.Walk && state!=Mood.Return && state!=Mood.Stalk && state!=Mood.WebApproach) Speed=0;
            else { burstMoving=true;burstRemaining=Between(.19,.34); }
        }
        public void GoHome(bool cancelPrey=true) { if(cancelPrey) { if(WebStage!=WebVisit.Fading) CancelWebVisit();HasFly=false; }ClearPlay();Target=Home;PlanCornerRoute(Home);ResetNestRoute();NextFly=Math.Max(NextFly,Clock+40);Enter(Mood.Return); }
        public void Wake() { napAfter=Clock+Between(100,170);decision=1;if(UnderNestSilk) StartNestExit();else Enter(Mood.Rest); }
        public void ResetCursor() { samples.Clear();StillTime=0;InviteTime=0;cursorValid=false;cursorNearby=false;quietInspection=false;attentionRearmAt=0;nextInspection=Clock+1; }
        void InspectCursor(V cursor) {
            inspectionResume=State==Mood.Walk || State==Mood.Return?State:Mood.Rest;
            // Greeting cooldown limits arm displays, not whether a walking pet notices us.
            quietInspection=cursorNearby || Clock<nextInspection;
            if(!quietInspection) nextInspection=Clock+Between(8,14);
            cursorNearby=true;Face(cursor);Enter(Mood.Inspect);
        }
        void FinishInspection() {
            Mood resume=inspectionResume;inspectionResume=Mood.Rest;quietInspection=false;
            attentionRearmAt=Clock+2.5;
            if(resume==Mood.Walk || resume==Mood.Return) Enter(resume);
            else { decision=Between(3,6);Enter(Mood.Rest); }
        }
        public void Resume() {
            FingerBlend=0;InviteTime=0;inspectionResume=Mood.Rest;
            ResetCursor();
            if(State==Mood.Dismount) { RideCooldown=25;Jump(JumpEnd,"release");return; }
            if(State==Mood.Inspect) { decision=2;Enter(Mood.Rest); }
            if(State==Mood.Ride || (jumpPurpose=="cursor" && (State==Mood.Reach || State==Mood.Crouch || State==Mood.Jump))) { RideCooldown=25;Jump(Clamp(Spider+new V(0,50*BodyScale)),"ground"); }
        }
        public void SpawnFly() { Spawn(PreyKind.Fly); }
        void Spawn(PreyKind kind) {
            if(HasFly) return;
            V best=Spider;double distance=0;bool far=random.NextDouble()<.4;
            for(int i=0;i<12;i++) {
                double a=Between(0,Math.PI*2);V p=Clamp(Spider+new V(Math.Cos(a),Math.Sin(a))*Between(far?320:145,far?540:270)*BodyScale);
                if((p-Spider).Length>distance) { best=p;distance=(p-Spider).Length; }
            }
            Fly=best;HasFly=true;FlyAge=0;FlyPhase=Between(0,6.28);NextFly=Clock+Between(.7,1.3)*FlyInterval;
            BeginPrey(kind);
            if(UnderNestSilk) { StartNestExit();napAfter=Clock+Between(100,170); }
            else if(State!=Mood.Ride && State!=Mood.Jump && State!=Mood.Crouch && State!=Mood.Reach && State!=Mood.Eat && State!=Mood.Struggle && State!=Mood.Land && State!=Mood.Dismount) Enter(Mood.Notice);
        }
        public double JumpProgress { get { return Math.Max(0,Math.Min(1,JumpTime/JumpDuration)); } }
        public bool HoldingMeal { get { return State==Mood.Eat || State==Mood.Struggle || (State==Mood.Land && holdingMeal); } }
        public const double StruggleDuration=1.35;
        public double CaptureStruggle { get { return State==Mood.Struggle?Math.Sin(Math.PI*Math.Min(1,StateTime/StruggleDuration))*(1-.45*StateTime/StruggleDuration):0; } }
        public double Lift { get {
            if(State!=Mood.Jump) return 0;
            double height=jumpPurpose=="release"?46:30;
            if(jumpPurpose=="fly") height+=Math.Min(12,(JumpEnd-JumpStart).Length/jumpScale*.05);
            // Snapshot at takeoff: changing skins in flight cannot change its path.
            if(directAttack) height=5;
            double lift=4*JumpProgress*(1-JumpProgress)*(height+(directAttack?0:Scared*12))*jumpScale;
            // Hunting and upward departures must keep the whole pose below the ceiling.
            if(jumpPurpose=="release" || jumpPurpose=="fly") lift=Math.Min(lift,Math.Max(0,Spider.Y-Areas[AreaAt(Spider)].Y-SpiderPose.Radius*jumpScale));
            return lift;
        } }
        public static double InspectionPhase(double time) { return Math.Max(0,Math.Min(1,(time-.12)/1.32)); }
        public double FrontReach(int side) {
            if(State==Mood.Struggle) return .8;
            if(State==Mood.WebPluck) return 0; // One planted foreleg handles direct silk contact in SpiderPose.
            if(State==Mood.SpeciesAct) return SpeciesReach(side);
            if(State==Mood.Dismount) return .65*DismountBrace;
            if(SettlingOnFinger) return Math.Max(0,1-RideAge/.24);
            if(OnFinger && FingerWaveAge>=.6) { double t=Math.Max(0,Math.Min(1,(FingerWaveAge-.6)/1.4));return Math.Sin(t*Math.PI); }
            if(State==Mood.Reach) return Math.Min(1,StateTime/.22);
            if(State==Mood.Crouch) return .85;
            if(State==Mood.Jump) return .8+.2*JumpProgress;
            if(State==Mood.Land) return Math.Max(0,1-StateTime/.2);
            if(State==Mood.Inspect) {
                if(quietInspection) return 0;
                // A paired forward/up reach, a held inspection, then a gentle lower.
                double t=InspectionPhase(StateTime),r=Math.Min(1,Math.Min(t,1-t)/.28);
                return r*r*(3-2*r);
            }
            return 0;
        }
        public double Squash { get {
            if(State==Mood.Struggle) return 1-.035*CaptureStruggle;
            if(State==Mood.Dismount) return 1-.08*DismountBrace;
            if(State==Mood.Crouch) return 1-.08*Math.Min(1,StateTime/.24);
            if(State==Mood.Land) return 1-.07*Math.Sin(Math.PI*Math.Min(1,StateTime/.20));
            if(SettlingOnFinger) return 1-.07*Math.Sin(Math.PI*RideAge/.24);
            return 1-.065*NestTuck-(State==Mood.SpeciesAct && behavior.Gesture==SpeciesGesture.BarkPause?.06*GestureBlend:0);
        } }
        void PrepareJump(V dest,string purpose) {
            V direction=dest-Spider;
            // Catch prey with the forward legs and mouth, rather than the abdomen.
            if(purpose=="fly" && direction.Length>25*BodyScale) dest=dest-direction*(25*BodyScale/direction.Length);
            JumpEnd=Clamp(dest);jumpPurpose=purpose;Face(JumpEnd);Enter(Mood.Reach);
        }
        void Jump(V dest,string purpose) {
            JumpStart=Spider;JumpEnd=Clamp(dest);JumpTime=0;jumpScale=BodyScale;jumpPurpose=purpose;jumpStartAngle=Angle;
            directAttack=purpose=="fly" && HuntsSpiders;holdingMeal=false;
            SilkStart=Spider;SilkOpacity=1;
            JumpDuration=Math.Max(.20,Math.Min(.32,.18+(JumpEnd-Spider).Length/(1200*BodyScale)));
            if(directAttack) JumpDuration=Math.Max(.13,Math.Min(.19,.10+(JumpEnd-Spider).Length/(2600*BodyScale)));
            if(purpose=="release") JumpDuration=Math.Max(.32,Math.Min(.42,.22+(JumpEnd-Spider).Length/(1000*BodyScale)));
            Face(JumpEnd);Enter(Mood.Jump);
        }
        void Face(V dest) { V d=dest-Spider;if(d.Length>1) desiredAngle=Math.Atan2(d.Y,d.X)*180/Math.PI+90; }
        public static double AngleDelta(double from,double to) { return ((to-from)%360+540)%360-180; }
        V Route(V destination) {
            if(cornerWaypoint.HasValue && (State==Mood.Walk || State==Mood.Return)) {
                if((cornerWaypoint.Value-Spider).Length<3*BodyScale) cornerWaypoint=null;else return cornerWaypoint.Value;
            }
            int from=AreaAt(Spider),to=AreaAt(destination);if(from==to) return destination;
            var a=Areas[from];var b=Areas[to];
            // Steer through the shared edge/overlap instead of straight into a blank desktop gap.
            double lo=Math.Max(a.Y,b.Y)+SpiderPose.Radius*BodyScale,hi=Math.Min(a.Y+a.H,b.Y+b.H)-SpiderPose.Radius*BodyScale;
            if(hi>=lo) {
                double x=b.Center.X>a.Center.X?a.X+a.W:a.X;
                V gate=new V(x,Math.Max(lo,Math.Min(hi,destination.Y)));
                if((gate-Spider).Length>4*BodyScale) return gate;
                return new V(b.Center.X>a.Center.X?b.X+3:b.X+b.W-3,gate.Y);
            }
            lo=Math.Max(a.X,b.X)+SpiderPose.Radius*BodyScale;hi=Math.Min(a.X+a.W,b.X+b.W)-SpiderPose.Radius*BodyScale;
            if(hi>=lo) {
                double y=b.Center.Y>a.Center.Y?a.Y+a.H:a.Y;
                V gate=new V(Math.Max(lo,Math.Min(hi,destination.X)),y);
                if((gate-Spider).Length>4*BodyScale) return gate;
                return new V(gate.X,b.Center.Y>a.Center.Y?b.Y+3:b.Y+b.H-3);
            }
            return destination; // Disconnected/diagonal screens are traversed by a silk hop below.
        }
        bool Move(V destination,double speed,double dt) {
            V dest=AvoidGuestWeb(Route(destination)),d=dest-Spider;double distance=d.Length;Face(dest);
            if((destination-Spider).Length<2*BodyScale) { Spider=destination;Speed=0;return true; }
            if(distance<.01) return false;
            double targetSpeed=Math.Min(speed*BodyScale,Math.Sqrt(2*1300*BodyScale*distance));
            // Pivot before advancing. Quick starts and strong braking remove the gliding feel.
            if(Math.Abs(AngleDelta(Angle,desiredAngle))>28) targetSpeed=0;
            Speed+=Math.Max(-1800*BodyScale*dt,Math.Min(1000*BodyScale*dt,targetSpeed-Speed));
            double step=Math.Min(distance,Speed*dt);V next=Clamp(Spider+d*(step/distance));
            if(step>.01 && (next-Spider).Length<.01 && AreaAt(Spider)!=AreaAt(destination)) {
                // A layout may contain a real gap. Cross it deliberately, never strand the pet.
                // Turning or braking with planted feet is not a blocked screen edge.
                PrepareJump(Areas[AreaAt(destination)].Clamp(dest,SpiderPose.Radius*BodyScale),"transit");Target=destination;return false;
            }
            StepPhase+=(next-Spider).Length/(4.5*BodyScale);Spider=next;return false;
        }
        bool BurstMove(V destination,double speed,double dt,bool stalking) {
            burstRemaining-=dt;
            if(burstRemaining<=0) {
                burstMoving=!burstMoving;
                burstRemaining=burstMoving?Between(stalking?.16:.20,stalking?.30:.38):Between(stalking?.28:.24,stalking?.65:.62);
            }
            return Move(destination,burstMoving?speed:0,dt);
        }
        double TrackCursor(double dt,V cursor) {
            bool valid=Finite(cursor.X) && Finite(cursor.Y) && InsideAny(cursor);
            double speed=cursorValid && valid?(cursor-LastCursor).Length/dt:0;
            if(!valid) { ResetCursor();LastCursor=cursor;return speed; }
            samples.Enqueue(new Sample(cursor,Clock));
            while(samples.Count>1 && Clock-samples.Peek().T>Math.Max(6,InviteDelay+2)) samples.Dequeue();
            bool outside=false;foreach(var s in samples) if((s.P-cursor).Length>7*pixelScale) { outside=true;break; }
            if(outside) { samples.Clear();samples.Enqueue(new Sample(cursor,Clock)); }
            StillTime=Clock-samples.Peek().T;LastCursor=cursor;cursorValid=true;return speed;
        }
        public void Tick(double dt,V cursor) { Tick(dt,cursor,false); }
        public void Tick(double dt,V cursor,bool clicked) {
            if(!Finite(dt) || dt<=0) return;
            if(dt>.5) { Resume();return; }
            dt=Math.Min(.1,dt);Clock+=dt;StateTime+=dt;Cursor=cursor;
            double cursorSpeed=TrackCursor(dt,cursor);RideCooldown=Math.Max(0,RideCooldown-dt);
            if(State!=Mood.Jump) pixelScale+=(Areas[AreaAt(Spider)].Dpi-pixelScale)*Math.Min(1,dt*8);
            double s=BodyScale;
            TickWebVisit(dt);
            bool cursorInside=cursorValid && ChooseFingerSide(cursor);
            Scared=Math.Max(0,Scared-dt*.4);
            if((Spider-Home).Length<28*HomeScale && (State==Mood.Sleep || State==Mood.Groom || State==Mood.Rest)) Web=Math.Min(1,Web+dt/110);
            if(HasFly) {
                FlyAge+=dt;bool linedUp=State==Mood.Aim || State==Mood.Reach || State==Mood.Crouch || State==Mood.Jump;
                if(!GuestPrey) MovePrey(dt,s,linedUp);
                if(FlyAge>60 && !GuestPrey) { HasFly=false;if(State==Mood.Notice || State==Mood.Stalk || State==Mood.Aim) Wake(); }
            }
            bool free=State==Mood.Rest || State==Mood.Walk || State==Mood.Groom || State==Mood.Inspect || State==Mood.SpeciesAct;
            if(AutoFeed && HuntsSpiders && WebVisits && !HasFly && WebStage==WebVisit.None && Clock>=NextWebVisit && free) StartWebVisit();
            if(AutoFeed && !HasFly && Clock>NextFly && free) SpawnPrey();
            if(GuestPrey && WebStage==WebVisit.Waiting && GuestStageAge>=GuestWaitTime && free && !FingerActive) BeginWebApproach();
            else if(HasFly && !GuestPrey && free) Enter(Mood.Notice);
            bool canOffer=free || State==Mood.Return;
            double cursorDistance=(cursor-Spider).Length;
            UpdateInvitation(dt,cursor,cursorSpeed,cursorInside,canOffer,clicked);
            if(!cursorValid || cursorDistance>DismissRadius) { cursorNearby=false;attentionRearmAt=0; }
            bool passing=(State==Mood.Walk || State==Mood.Return) && cursorDistance<AttentionRadius && Clock>=attentionRearmAt;
            bool greeting=free && State!=Mood.Inspect && cursorDistance<AttentionRadius && !cursorNearby && Clock>=nextInspection;
            if(CursorPlay && cursorValid && !clicked && !PreyBlocksPlay && (passing || greeting)) {
                InspectCursor(cursor);
            }
            if(InviteTime>=InviteDelay+.7 && FingerBlend>.94 && canOffer) {
                PlanFingerBoarding(cursor);PrepareJump(CursorLanding(cursor),"cursor");RideCooldown=25;
            }
            bool catchTurning=false;
            switch(State) {
                case Mood.WebApproach: SneakToWeb(dt);break;
                case Mood.WebPluck: PluckWeb();break;
                case Mood.SpeciesAct: SpeciesMoment(dt);break;
                case Mood.Inspect:
                    if(!CursorPlay || !cursorValid || cursorDistance>DismissRadius || clicked) { FinishInspection();break; }
                    Face(cursor);
                    // A still nearby pointer gets enough stationary attention for the
                    // five-second finger invitation, even midway through a long patrol.
                    if(StateTime>1.65 && (StillTime<.15 || StateTime>InviteDelay+2)) FinishInspection();
                    break;
                case Mood.Reach:
                    if(jumpPurpose=="cursor" && (clicked || !CursorPlay || !cursorInside || cursorSpeed>350*pixelScale || (CursorLanding(cursor)-JumpEnd).Length>24*s)) { InviteTime=0;decision=2;Enter(Mood.Rest);break; }
                    Face(JumpEnd);
                    if(StateTime>.36 && Math.Abs(AngleDelta(Angle,desiredAngle))<12) Enter(Mood.Crouch);
                    break;
                case Mood.Crouch:
                    if(jumpPurpose=="cursor" && (clicked || !CursorPlay || !cursorInside || cursorSpeed>350*pixelScale || (CursorLanding(cursor)-JumpEnd).Length>24*s)) { InviteTime=0;decision=2;Enter(Mood.Rest);break; }
                    if(StateTime>=.26) Jump(JumpEnd,jumpPurpose);break;
                case Mood.Jump:
                    if(jumpPurpose=="cursor" && (clicked || !CursorPlay || !cursorInside || cursorSpeed>CarryTolerance*pixelScale)) { jumpPurpose="ground";InviteTime=0; }
                    JumpTime+=dt;double t=JumpProgress;Spider=JumpStart+(JumpEnd-JumpStart)*t;
                    if(jumpPurpose=="cursor" && TipBoarding) {
                        // Finish the airborne turn with the face toward the grip.
                        // Waiting until Ride made the spider hang by its rear legs.
                        V grip=FingerTip(cursor)-JumpEnd;
                        double catchAngle=Math.Atan2(grip.Y,grip.X)*180/Math.PI+90;
                        desiredAngle=jumpStartAngle+AngleDelta(jumpStartAngle,catchAngle)*t*t*(3-2*t);catchTurning=true;
                    }
                    if(t>=1) {
                        if(jumpPurpose=="fly" && HasFly && (Spider-Fly).Length<35*s) { Meal=Prey;HasFly=false;holdingMeal=true;if(WebStage==WebVisit.Attack) FinishWebVisit();if(Meals<int.MaxValue) Meals++;Enter(Mood.Land); }
                        else if(jumpPurpose=="cursor" && CursorPlay && cursorInside && (Spider-CursorLanding(cursor)).Length<40*s && cursorSpeed<350*pixelScale) { BeginRide(); }
                        else { if(jumpPurpose!="transit") jumpPurpose="ground";Enter(Mood.Land); }
                    }break;
                case Mood.Land:
                    if(StateTime>=.20) { if(jumpPurpose=="fly") Enter(directAttack && Meal==PreyKind.SmallSpider?Mood.Struggle:Mood.Eat);else if(jumpPurpose=="transit") Enter(WebStage==WebVisit.Approach?Mood.WebApproach:(Target-Home).Length<1?Mood.Return:Mood.Walk);else { decision=Between(2,5);Enter(Mood.Rest); } }break;
                case Mood.Struggle: if(StateTime>=StruggleDuration) Enter(Mood.Eat);break;
                case Mood.Ride:
                    Carry(dt,cursor,cursorSpeed,cursorInside,clicked);break;
                case Mood.Dismount:
                    PrepareDismount(dt);break;
                case Mood.Notice: Face(Fly);if(StateTime>.65) Enter(Mood.Stalk);break;
                case Mood.Stalk:
                    if(!HasFly) { Wake();break; }Face(Fly);
                    if((Fly-Spider).Length<PreyPounceRange*s) Enter(Mood.Aim);
                    else { double distance=(Fly-Spider).Length/s;BurstMove(Fly,HuntingSpeed(distance),dt,true); }break;
                case Mood.Aim: Face(Fly);if(!HasFly) Wake();else if(StateTime>.6) PrepareJump(Fly,"fly");break;
                case Mood.Eat: if(StateTime>MealDuration) { decision=2;Enter(Mood.Groom); }break;
                case Mood.Return: ReturnToNest(dt);break;
                case Mood.EnterNest: CrawlIntoNest(dt);break;
                case Mood.ExitNest: CrawlOutOfNest(dt);break;
                case Mood.Sleep: desiredAngle=NestAngle;if(StateTime>20) Wake();break;
                case Mood.Walk:
                    if(BurstMove(Target,SpeciesTraits?behavior.PatrolSpeed:155,dt,false)) { decision=Between(12,26);Enter(Mood.Rest); }break;
                case Mood.Groom: if(StateTime>3.5) { decision=Between(10,22);Enter(Mood.Rest); }break;
                default:
                    if(CursorPlay && cursorValid && cursorDistance<DismissRadius) Face(cursor);
                    else if(Clock>scanAt) {
                        // Brief whole-body reorientation, then hold. No swivelling detached head.
                        scanAt=Clock+Between(2.5,6);desiredAngle+=Between(-24,24);
                    }
                    if(Clock>napAfter) GoHome(false);
                    else if(TrySpeciesMoment()) break;
                    else if(StateTime>decision) {
                        double choice=random.NextDouble();
                        if(CornerDistance>140*s) ChoosePerch();
                        else if(choice<.55) { decision=Between(12,26);Enter(Mood.Rest); }
                        else if(choice<.78) Enter(Mood.Groom);
                        else ChoosePerch();
                    }break;
            }
            double turn=AngleDelta(Angle,desiredAngle),delta=catchTurning?turn:Math.Max(-190*dt,Math.Min(190*dt,turn));
            Angle=(Angle+delta+360)%360;AngularSpeed=delta/dt;
            // Turn with planted steps first, then ease into the silk. Keep the
            // same continuous blend on waking so the feet do not snap outward.
            bool settled=State==Mood.Sleep && StateTime>.08 && (Spider-Home).Length<3*HomeScale && Math.Abs(AngleDelta(Angle,NestAngle))<8;
            nestRest=Math.Max(0,Math.Min(1,nestRest+dt*(settled?.8:-1.6)));
            if(Speed<1 && Math.Abs(delta)>.01 && State!=Mood.Jump) StepPhase+=Math.Abs(delta)*.025;
            double active=(Speed>1 || Math.Abs(AngularSpeed)>12) && State!=Mood.Jump?1:0;
            GaitBlend+=Math.Max(-dt*7,Math.Min(dt*12,active-GaitBlend));
            LookAngle=0;
            if(State==Mood.Jump || SilkOpacity>0) {
                double a=Angle*Math.PI/180;SilkEnd=Spider+new V(-Math.Sin(a)*34*s,Math.Cos(a)*34*s-Lift);
                if(State==Mood.Jump) SilkOpacity=1;else SilkOpacity=Math.Max(0,SilkOpacity-dt*1.25);
            }
        }
    }
}
