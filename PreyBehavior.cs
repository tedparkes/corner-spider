using System;

namespace CornerSpider {
    public enum PreyKind { Fly, SmallSpider }
    public partial class World {
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
        public bool HuntsSpiders { get { return SkinCatalog.Get(Species).SpiderPrey; } }
        public void SetSpecies(string id) {
            if(!SkinCatalog.Contains(id) || species==id) return;
            if(WebStage!=WebVisit.None) CancelWebVisit();
            if(State==Mood.SpeciesAct) { decision=2;Enter(Mood.Rest); }
            species=id;behavior=BehaviorCatalog.Get(id);nextSpeciesMoment=Clock+28;NextWebVisit=Clock+100;
        }
        public void Feed() { if(HuntsSpiders) SpawnSmallSpider();else SpawnFly(); }
        public void SpawnSmallSpider() { if(HuntsSpiders) Spawn(PreyKind.SmallSpider); }
        public void SpawnPrey() {
            if(HasFly) return;
            Spawn(HuntsSpiders && random.NextDouble()<.65?PreyKind.SmallSpider:PreyKind.Fly);
        }
        void BeginPrey(PreyKind kind) {
            Prey=kind;preyOrigin=Fly;PreyAngle=FlyPhase*180/Math.PI;PreySteps=0;PreyWalking=false;
        }
        void MovePrey(double dt,double scale,bool linedUp) {
            PreyWalking=false;
            if(linedUp) return;
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
            PreySteps+=distance/Math.Max(.01,1.6*s);
        }
    }
}
