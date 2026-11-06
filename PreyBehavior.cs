using System;

namespace CornerSpider {
    public enum PreyKind { Fly, SmallSpider, Gnat, Moth, Cricket, Mealworm, Beetle }
    public partial class World {
        public const double SmallSpiderSize=1.5;
        // Fly/HasFly retain the original single-slot API used by saves/tests.
        // Prey identity is captured separately, so feeding during Eat cannot
        // change the meal already held by the pet.
        string species="Regal";
        public string Species { get { return species; } }
        public PreyKind Prey { get; private set; }
        public PreyKind Meal { get; private set; }
        public double PreyAngle { get; private set; }
        public double PreySteps { get; private set; }
        public bool PreyWalking { get; private set; }
        V preyOrigin;
        double orbSenseStart=-1;
        public double WebSense {
            get {
                if(State!=Mood.Notice || orbSenseStart<0) return 0;
                double phase=Math.Max(0,Math.Min(1,(StateTime-orbSenseStart)/.44));
                double pulse=Math.Sin(Math.PI*phase);
                return pulse*pulse;
            }
        }
        void NoticePrey() {
            Face(Fly);
            if(HasLargeOrb && (Spider-Home).Length<8*OrbMovementScale) {
                // Turn first, then dip into the web on planted feet before following the signal.
                if(orbSenseStart<0 && Math.Abs(AngleDelta(Angle,desiredAngle))<3 && GaitBlend<.04)
                    orbSenseStart=StateTime;
                if(orbSenseStart<0 || StateTime-orbSenseStart<.44) return;
            }
            if(StateTime>.65) Enter(Mood.Stalk);
        }
        double WebHuntSpeed {
            get {
                switch(SkinCatalog.Get(Species).WebKind) {
                    case WebKind.Tangle:return 70;
                    case WebKind.Sheet:return 150;
                    default:return 90;
                }
            }
        }
        public bool HuntsSpiders { get { return SkinCatalog.Get(Species).SpiderPrey; } }
        public void SetSpecies(string id) {
            if(!SkinCatalog.Contains(id) || species==id) return;
            if(WebStage!=WebVisit.None) CancelWebVisit();
            if(State==Mood.SpeciesAct) { decision=2;Enter(Mood.Rest); }
            bool previousBusy=LifeBusy,previousWebDweller=WebDweller,previousCornerSleeve=HasCornerSleeve,previousOrbLayout=HasOrbHomeLayout,previousOrbMate=OrbMateSeeker;
            species=id;behavior=BehaviorCatalog.Get(id);nextSpeciesMoment=Clock+28;NextWebVisit=Clock+100;
            ResetOrbEscort();
            if(previousBusy) { nestInside=false;Wake(); }
            if((previousWebDweller!=WebDweller || previousCornerSleeve!=HasCornerSleeve || previousOrbLayout!=HasOrbHomeLayout || previousOrbMate!=OrbMateSeeker) && !IsColonyResident) {
                bool goingHome=State==Mood.Return || State==Mood.EnterNest || State==Mood.ExitNest || State==Mood.Sleep;
                UpdateHome();
                if(goingHome) GoHome(false);
            }
            RestoreLife();
        }
        public void Feed() { if(HuntsSpiders) SpawnSmallSpider();else SpawnFly(); }
        public void SpawnSmallSpider() { if(HuntsSpiders) Spawn(PreyKind.SmallSpider); }
        public void Feed(PreyKind kind) { if(Enum.IsDefined(typeof(PreyKind),kind)) Spawn(kind); }
        public void SpawnPrey() {
            if(HasFly) return;
            if(HuntsSpiders && random.NextDouble()<.65) { Spawn(PreyKind.SmallSpider);return; }
            PreyKind[] choices=WebDweller?new[]{PreyKind.Fly,PreyKind.Gnat,PreyKind.Moth,PreyKind.Beetle}:new[]{PreyKind.Fly,PreyKind.Gnat,PreyKind.Moth,PreyKind.Cricket,PreyKind.Mealworm,PreyKind.Beetle};
            Spawn(choices[random.Next(choices.Length)]);
        }
        void BeginPrey(PreyKind kind) {
            Prey=kind;preyOrigin=Fly;PreyAngle=FlyPhase*180/Math.PI;PreySteps=0;PreyWalking=false;
        }
        void MovePrey(double dt,double scale,bool linedUp) {
            PreyWalking=false;
            if(linedUp) return;
            if(WebDweller && !OrbMateSeeker) { PreyWalking=true;PreySteps+=dt*8;return; } // trapped silk vibration, no jumping hunt
            if(Prey!=PreyKind.Fly && Prey!=PreyKind.SmallSpider) { MoveFood(dt,scale);return; }
            if(Prey==PreyKind.Fly) {
                Fly=Clamp(Fly+new V(Math.Sin(Clock*4+FlyPhase)*18,Math.Cos(Clock*5+FlyPhase)*14)*(dt*scale));return;
            }
            // A tiny ground spider takes a few steps, then holds its perch.
            // Bounded local excursions keep it near the original sighting.
            double s=Scale*Areas[AreaAt(Fly)].Dpi,phase=(FlyAge+FlyPhase*.2)%2.6;
            if(phase<.9 || phase>1.85) return;
            V home=preyOrigin-Fly;
            double desired=home.Length>26*s?Math.Atan2(home.Y,home.X)*180/Math.PI+90:FlyPhase*180/Math.PI+Math.Sin(FlyAge*.6)*36;
            double turn=AngleDelta(PreyAngle,desired);
            PreyAngle=(PreyAngle+Math.Max(-150*dt,Math.Min(150*dt,turn))+360)%360;
            if(Math.Abs(turn)>22) return;
            double a=PreyAngle*Math.PI/180;
            V next=Clamp(Fly+new V(Math.Sin(a),-Math.Cos(a))*(12.5*dt*s));
            double distance=(next-Fly).Length;Fly=next;PreyWalking=distance>.001;
            PreySteps+=distance/Math.Max(.01,1.6*s*SmallSpiderSize);
        }
        void MoveFood(double dt,double scale) {
            double phase=(FlyAge+FlyPhase)%3.1;
            if(Prey==PreyKind.Gnat || Prey==PreyKind.Moth) {
                double speed=Prey==PreyKind.Gnat?23:12;
                Fly=Clamp(Fly+new V(Math.Sin(FlyAge*3+FlyPhase),Math.Cos(FlyAge*4))*speed*dt*scale);return;
            }
            if(phase<.7) return;
            V toward=preyOrigin-Fly;double a=toward.Length>48*scale?Math.Atan2(toward.Y,toward.X):FlyPhase+Math.Sin(FlyAge*.3)*.4;
            double speed2=Prey==PreyKind.Cricket?(phase>2.5?95:12):Prey==PreyKind.Mealworm?5:9;
            Fly=Clamp(Fly+new V(Math.Cos(a),Math.Sin(a))*speed2*dt*scale);PreyAngle=a*180/Math.PI+90;PreyWalking=true;PreySteps+=dt*speed2/4;
        }
    }
}
