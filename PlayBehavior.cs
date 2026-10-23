using System;

namespace CornerSpider {
    public partial class World {
        public double InviteTime,FingerBlend,FingerPosition=65,RideAge,FingerWaveAge=-1,Scared,SilkOpacity;
        public int FingerDirection=1;
        public V FingerOrigin,SilkStart,SilkEnd;
        public string LastDismount="";
        V carryVelocity,climbFrom,touchdownOffset;
        bool climbing,tipBoarding=true;
        double boardingPosition=65;
        public bool TipBoarding { get { return tipBoarding; } }
        public bool SettlingOnFinger { get { return State==Mood.Ride && !tipBoarding && RideAge<.24; } }
        double fingerGoal=65,fingerDecision,nextFingerWave;
        V dismountFrom,dismountPerch;
        double dismountClimbTime,dismountReadyAt=-1;
        public double DismountBrace { get {
            if(State!=Mood.Dismount || dismountReadyAt<0) return 0;
            double t=Math.Max(0,Math.Min(1,(StateTime-dismountReadyAt-.06)/.18));return t*t*(3-2*t);
        } }
        public bool Hanging { get { return State==Mood.Ride && tipBoarding && RideAge<1.8; } }
        public bool Climbing { get { return State==Mood.Ride && tipBoarding && RideAge>=1.8 && RideAge<2.8; } }
        public bool OnFinger { get { return State==Mood.Ride && (!tipBoarding || RideAge>=2.8); } }
        public bool FingerActive { get { return InviteTime>=InviteDelay || State==Mood.Ride || (jumpPurpose=="cursor" && (State==Mood.Reach || State==Mood.Crouch || State==Mood.Jump)); } }
        V TipAt(V cursor,int direction) { return cursor+new V(-direction*75,12)*BodyScale; }
        V SurfaceAt(V cursor,double x,int direction) { return TipAt(cursor,direction)+new V(direction*x,-.22*x)*BodyScale; }
        public V FingerTip(V cursor) { return TipAt(cursor,FingerDirection); }
        public V CursorLanding(V cursor) { return tipBoarding?FingerTip(cursor)+new V(0,52*BodyScale):SurfaceAt(cursor,boardingPosition,FingerDirection); }
        public V FingerPoint(double x) { return SurfaceAt(FingerOrigin,x,FingerDirection); }
        void PlanFingerBoarding(V cursor) {
            V from=Spider-FingerTip(cursor);
            double x=from.X*FingerDirection/BodyScale,y=from.Y/BodyScale;
            // Only the exposed side of the tip offers a natural front-leg catch.
            // Above the finger or behind its base, land on the upper surface.
            tipBoarding=x< -18 && y> -12 && y<120 && random.NextDouble()<.6;
            double nearest=Math.Max(45,Math.Min(103,(x-.22*y)/1.0484));
            double low=Math.Max(45,nearest-24),high=Math.Min(103,nearest+24);
            boardingPosition=tipBoarding?65:Between(low,high);
        }
        bool Safe(V point) { return (Clamp(point)-point).Length<.01; }
        bool FitsFinger(V cursor,int direction) {
            return InsideAny(cursor) && Safe(TipAt(cursor,direction)+new V(0,52*BodyScale)) &&
                Safe(SurfaceAt(cursor,45,direction)) && Safe(SurfaceAt(cursor,105,direction));
        }
        bool ChooseFingerSide(V cursor) {
            if(FingerBlend>.01 || State==Mood.Ride) return FitsFinger(cursor,FingerDirection);
            if(FitsFinger(cursor,1)) { FingerDirection=1;return true; }
            if(FitsFinger(cursor,-1)) { FingerDirection=-1;return true; }
            return false;
        }
        void ClearPlay() { InviteTime=FingerBlend=RideAge=0;FingerWaveAge=-1;carryVelocity=touchdownOffset=new V();climbing=false;tipBoarding=true;boardingPosition=65;dismountReadyAt=-1; }
        void UpdateInvitation(double dt,V cursor,double speed,bool safe,bool free,bool clicked) {
            if(clicked) { samples.Clear();StillTime=InviteTime=0; }
            double distance=(cursor-Spider).Length;
            bool offering=CursorPlay && safe && !PreyBlocksPlay && free && !clicked && RideCooldown<=0 && distance>20*BodyScale && distance<AttentionRadius;
            InviteTime=offering?Math.Min(StillTime,InviteTime+dt):0;
            bool committed=State==Mood.Ride || (jumpPurpose=="cursor" && (State==Mood.Reach || State==Mood.Crouch || State==Mood.Jump));
            double target=offering?Math.Max(0,Math.Min(1,(InviteTime-InviteDelay)/.65)):0;
            if(committed && CursorPlay && safe && !clicked && speed<=CarryTolerance*pixelScale) target=1;
            // Keep the last offered finger still until takeoff. The real pointer
            // is released on click because Dismount is no longer FingerActive.
            if(State==Mood.Dismount) target=1;
            FingerBlend+=Math.Max(-dt*4,Math.Min(dt*3,target-FingerBlend));
            if(target>0 && State!=Mood.Ride && State!=Mood.Dismount) FingerOrigin=cursor;
        }
        void BeginRide() {
            rideDuration=Between(35,55);RideAge=0;FingerWaveAge=-1;FingerPosition=fingerGoal=boardingPosition;
            nextFingerWave=Clock+5;fingerDecision=Clock+3;carryVelocity=new V();climbing=false;
            // Small pointer drift during the leap is absorbed by a brief landing
            // adjustment, never by snapping the body to a new finger coordinate.
            touchdownOffset=Spider-SurfaceAt(Cursor,boardingPosition,FingerDirection);
            FingerOrigin=Cursor;FingerBlend=1;LastDismount="";Enter(Mood.Ride);
        }
        void LeaveRide(bool scared,string reason) {
            LastDismount=reason;Scared=scared?1:0;RideCooldown=scared?25:9;InviteTime=0;FingerWaveAge=-1;
            if(reason=="click") {
                // If clicked during the initial catch, climb onto the offered
                // surface first rather than taking off from empty space below it.
                dismountFrom=Spider;dismountClimbTime=OnFinger?0:.4;
                dismountPerch=OnFinger?Spider:Clamp(FingerPoint(65));
                JumpEnd=LandingAboveFinger(dismountPerch);dismountReadyAt=-1;
                carryVelocity=new V();FingerBlend=1;Face(JumpEnd);Enter(Mood.Dismount);return;
            }
            double distance=scared?155:95;
            V dest=Clamp(Spider+new V(-FingerDirection*(scared?80:25),distance)*BodyScale);
            // Preserve the departure point before following any newly sampled cursor.
            Jump(dest,"ground");if(scared) JumpDuration=Math.Min(JumpDuration,.23);
        }
        V LandingAboveFinger(V perch) {
            var area=Areas[AreaAt(perch)];double s=BodyScale;
            double margin=SpiderPose.Radius*Scale*Math.Max(area.Dpi,pixelScale);
            double y=Math.Min(perch.Y,FingerOrigin.Y-34*s)-Between(105,145)*s;
            V landing=area.Clamp(new V(perch.X+FingerDirection*Between(40,85)*s,y),margin);
            if(perch.Y-landing.Y<45*s) {
                // At the top edge, use the highest available safe point with a
                // sideways hop. Do not turn a blocked upward leap into a drop.
                V left=area.Clamp(new V(perch.X-180*s,landing.Y),margin);
                V right=area.Clamp(new V(perch.X+180*s,landing.Y),margin);
                landing=Math.Abs(left.X-perch.X)>=Math.Abs(right.X-perch.X)?left:right;
            }
            return landing;
        }
        void PrepareDismount(double dt) {
            V before=Spider;
            double t=dismountClimbTime>0?Math.Min(1,StateTime/dismountClimbTime):1;t=t*t*(3-2*t);
            Spider=dismountFrom+(dismountPerch-dismountFrom)*t;
            Speed=(Spider-before).Length/dt;StepPhase+=(Spider-before).Length/(4.5*BodyScale);
            Face(JumpEnd);
            if(t>=1 && Math.Abs(AngleDelta(Angle,desiredAngle))<8) {
                if(dismountReadyAt<0) dismountReadyAt=StateTime;
                if(StateTime-dismountReadyAt>=.24) Jump(JumpEnd,"release");
            }
        }
        void Carry(double dt,V cursor,double speed,bool safe,bool clicked) {
            if(clicked) { LeaveRide(false,"click");return; }
            if(speed>CarryTolerance*pixelScale) { LeaveRide(true,"fast movement");return; }
            if(!CursorPlay || !safe || StateTime>rideDuration) { LeaveRide(false,!CursorPlay?"disabled":!safe?"edge":"choice");return; }
            RideAge+=dt;FingerOrigin=cursor;FingerBlend=1;
            if(Hanging) {
                V target=CursorLanding(cursor)+new V(Math.Sin(RideAge*2.4)*2*BodyScale,Math.Sin(RideAge*3)*.5*BodyScale);
                // A damped suspension follows gently with a small lag and sway.
                // Substeps keep it stable at slower display/dispatcher intervals.
                int steps=(int)Math.Ceiling(dt/.016);double h=dt/steps;
                for(int i=0;i<steps;i++) {
                    carryVelocity+=(target-Spider)*(42*h)-carryVelocity*(11*h);
                    Spider+=carryVelocity*h;
                }
                // The forelegs are a physical tether, not elastic arms. Keep the
                // suspension below the tip and within reach during sustained motion.
                V tip=FingerTip(cursor),offset=Spider-tip;offset.Y=Math.Max(36*BodyScale,offset.Y);
                double length=offset.Length;
                if(length>54*BodyScale) {
                    V outward=offset*(1/length);offset=outward*(54*BodyScale);
                    double radial=carryVelocity.X*outward.X+carryVelocity.Y*outward.Y;
                    if(radial>0) carryVelocity-=outward*radial;
                }
                Spider=tip+offset;
                Spider=Clamp(Spider);Face(tip);Speed=0;
            } else if(Climbing) {
                if(!climbing) { climbing=true;climbFrom=Spider-cursor; }
                double t=RideAge-1.8;t=t*t*(3-2*t);
                Spider=cursor+climbFrom*(1-t)+(FingerPoint(65)-cursor)*t;
                desiredAngle=FingerDirection>0?90:270;Speed=35*BodyScale;StepPhase+=dt*7;
            } else if(!tipBoarding && RideAge<=.24) {
                double t=Math.Min(1,RideAge/.24);t=t*t*(3-2*t);
                Spider=FingerPoint(FingerPosition)+touchdownOffset*(1-t);Speed=0;
            } else {
                if(FingerWaveAge<0 && Clock>=nextFingerWave) { FingerWaveAge=0;nextFingerWave=Clock+Between(10,16); }
                if(FingerWaveAge>=0) {
                    FingerWaveAge+=dt;desiredAngle=180;Speed=0;
                    if(FingerWaveAge>2.3) { FingerWaveAge=-1;fingerDecision=Clock+1; }
                } else {
                    if(Clock>fingerDecision) { fingerGoal=FingerPosition<72?Between(85,103):Between(45,60);fingerDecision=Clock+Between(3,5); }
                    double delta=fingerGoal-FingerPosition,step=Math.Max(-18*dt,Math.Min(18*dt,delta));
                    double facing=step*FingerDirection>=0?90:270;
                    if(Math.Abs(delta)>1) desiredAngle=facing;
                    if(Math.Abs(AngleDelta(Angle,desiredAngle))>25) step=0;
                    FingerPosition+=step;Speed=Math.Abs(step)*BodyScale/dt;StepPhase+=Math.Abs(step)/4.5;
                }
                Spider=FingerPoint(FingerPosition);
            }
        }
    }
}
