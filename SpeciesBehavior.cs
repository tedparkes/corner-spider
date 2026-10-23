using System;

namespace CornerSpider {
    public enum SpeciesGesture { Lookout, Drum, BarkPause, Measure, NestCheck, Zigzag, WallWatch, SilkTap, Survey, WebWatch, HeavyWatch }
    public sealed class BehaviorProfile {
        public string Title,Note;
        public SpeciesGesture Gesture;
        public double PatrolSpeed=155,MealSeconds=4,HomeAffinity,Duration=4;
        public BehaviorProfile(string title,string note,SpeciesGesture gesture) { Title=title;Note=note;Gesture=gesture; }
    }
    public static class BehaviorCatalog {
        public static BehaviorProfile Get(string id) {
            switch(id) {
                case "RegalMale":return new BehaviorProfile("Silk-tapping gentleman","Small foreleg taps echo documented nest courtship. This solo display is a playful adaptation.",SpeciesGesture.SilkTap);
                case "Regal":return new BehaviorProfile("Palm-frond lookout","Deliberate look-around pauses, inspired by its elevated plant and palm-frond retreats.",SpeciesGesture.Lookout);
                case "Bold":return new BehaviorProfile("Foreleg drummer","A brief alternating foreleg display inspired by male courtship, with shorter meals.",SpeciesGesture.Drum) { MealSeconds=3.4 };
                case "BoldFemale":return new BehaviorProfile("Patient hunter","Longer meal handling and attentive pauses echo reported female feeding differences.",SpeciesGesture.Lookout) { MealSeconds=5.5,Duration=5 };
                case "Zebra":case "ZebraMale":return new BehaviorProfile("Careful range-finder","Slows its final stalking steps as it approaches prey, as observed in zebra-spider hunting studies.",SpeciesGesture.Measure) { PatrolSpeed=170 };
                case "Tan":case "TanMale":return new BehaviorProfile("Bark sitter","Lowers its body and holds still between short trips, inspired by its flat bark-dwelling form and less active foraging.",SpeciesGesture.BarkPause) { PatrolSpeed=135,Duration=6 };
                case "RedBack":return new BehaviorProfile("Zigzag dancer","A short raised-foreleg sidestep echoes the male's documented visual courtship dance.",SpeciesGesture.Zigzag);
                case "RedBackFemale":return new BehaviorProfile("Retreat regular","Returns to inspect its silk retreat more often, inspired by documented nest fidelity.",SpeciesGesture.NestCheck) { HomeAffinity=.45 };
                case "Emerald":return new BehaviorProfile("Nest-side sentinel","Checks and tends its retreat. Inspired by female nest guarding; no eggs or breeding simulation are added.",SpeciesGesture.NestCheck) { HomeAffinity=.65,Duration=5 };
                case "EmeraldMale":return new BehaviorProfile("Silk-side visitor","Pauses to inspect silk, inspired by observations of males visiting female molting retreats.",SpeciesGesture.SilkTap) { HomeAffinity=.2 };
                case "GrayWall":case "GrayWallMale":return new BehaviorProfile("Wall-side watcher","Short wall-like edge patrols and watchful pauses, inspired by observations around insect nest entrances.",SpeciesGesture.WallWatch) { PatrolSpeed=180 };
                case "Pantropical":return new BehaviorProfile("Face-forward salutes","Brief foreleg salutes echo its visual displays. Its dense-nest lifestyle inspires silk checks.",SpeciesGesture.Drum) { HomeAffinity=.2 };
                case "PantropicalFemale":return new BehaviorProfile("Silk-house keeper","Inspects and tends the hammock, inspired by this species' dense, structurally complex nests.",SpeciesGesture.NestCheck) { HomeAffinity=.4 };
                case "Apache":return new BehaviorProfile("Twig explorer","Makes more short scouting trips, inspired by observations of males wandering on leaves and twigs.",SpeciesGesture.Survey) { PatrolSpeed=175,Duration=3 };
                case "ApacheFemale":return new BehaviorProfile("Sheltered homebody","More frequent returns to the hammock echo observations of females staying in plant retreats.",SpeciesGesture.NestCheck) { HomeAffinity=.7,Duration=6 };
                case "PortiaLabiata":return new BehaviorProfile("Web deceiver","Steps onto the anchored edge and plucks with one foreleg; the resident stalks closer and pauses after each signal.",SpeciesGesture.WebWatch) { PatrolSpeed=125,Duration=6 };
                case "HyllusDiardiGreen":return new BehaviorProfile("Weighty pauses","A deliberate, slower tempo inspired by its large build. This personality is stylized; species-specific behavior data are limited.",SpeciesGesture.HeavyWatch) { PatrolSpeed=115,Duration=6 };
                default:return Get("Regal");
            }
        }
    }
    public partial class World {
        public bool SpeciesTraits=true;
        BehaviorProfile behavior=BehaviorCatalog.Get("Regal");
        public BehaviorProfile Personality { get { return behavior; } }
        public double MealDuration { get { return SpeciesTraits?behavior.MealSeconds:4; } }
        double nextSpeciesMoment=28,gestureHeading;
        V gestureOrigin;
        public double GestureBlend { get { return State!=Mood.SpeciesAct?0:Math.Max(0,Math.Min(1,Math.Min(StateTime,behavior.Duration-StateTime)/.6)); } }
        public double SpeciesReach(int side) {
            if(State!=Mood.SpeciesAct || !SpeciesTraits) return 0;
            double blend=GestureBlend;
            switch(behavior.Gesture) {
                case SpeciesGesture.Drum:return blend*(.35+.5*Math.Max(0,Math.Sin(StateTime*8+side*1.3)));
                case SpeciesGesture.Zigzag:return blend*.65;
                case SpeciesGesture.SilkTap:return blend*(.25+.35*Math.Max(0,Math.Sin(StateTime*11+side)));
                default:return 0;
            }
        }
        double HuntingSpeed(double distance) {
            if(SpeciesTraits && behavior.Gesture==SpeciesGesture.Measure) return Math.Max(65,Math.Min(200,(distance-PreyPounceRange)*.65+65));
            return distance>240?210:distance>145?135:82;
        }
        bool TrySpeciesMoment() {
            if(!SpeciesTraits || Clock<nextSpeciesMoment || PreyBlocksPlay || CornerDistance>150*BodyScale) return false;
            nextSpeciesMoment=Clock+Between(32,58);StartSpeciesMoment();return true;
        }
        public bool StartSpeciesMoment() {
            if(!SpeciesTraits || PreyBlocksPlay || !(State==Mood.Rest || State==Mood.Walk || State==Mood.Groom || State==Mood.SpeciesAct)) return false;
            gestureOrigin=Spider;gestureHeading=Angle;Enter(Mood.SpeciesAct);return true;
        }
        void SpeciesMoment(double dt) {
            if(!SpeciesTraits || StateTime>=behavior.Duration) { decision=Between(12,22);Enter(Mood.Rest);return; }
            var kind=behavior.Gesture;
            double angle=gestureHeading*Math.PI/180;
            if(kind==SpeciesGesture.Zigzag || kind==SpeciesGesture.Survey || kind==SpeciesGesture.WallWatch) {
                double step=kind==SpeciesGesture.Zigzag?Math.Sin(StateTime*Math.PI*1.25)*12:Math.Sin(Math.Min(1,StateTime/behavior.Duration)*Math.PI)*22;
                V before=Spider;Spider=Clamp(gestureOrigin+new V(Math.Cos(angle),Math.Sin(angle))*(step*BodyScale*GestureBlend));
                Speed=(Spider-before).Length/dt;StepPhase+=(Spider-before).Length/(4.5*BodyScale);
            }
            if(kind==SpeciesGesture.NestCheck) {
                Face(Home);
                if((Spider-Home).Length<180*HomeScale) Web=Math.Min(1,Web+dt/160);
            } else if(kind==SpeciesGesture.Zigzag || kind==SpeciesGesture.Drum || kind==SpeciesGesture.SilkTap) desiredAngle=gestureHeading;
            else desiredAngle=gestureHeading+Math.Sin(StateTime*1.5)*GestureBlend*(kind==SpeciesGesture.HeavyWatch?12:kind==SpeciesGesture.WallWatch?35:24);
        }
    }
}
