using System;
using System.Collections.Generic;
using System.Globalization;

namespace CornerSpider {
    public enum LifeStage { Quiet, PreparingMolt, Molting, Recovering, Courting, Mating, MaleLeaving, MaleMeal, Gravid, Laying, Incubating, Brood, Dispersing }
    public sealed class LifeRecord {
        public double Age,SilkSeconds,StageAge,NextMolt=21600,NextMate=28800,GrowthAt=-1,GrowthSpan=21600;
        public LifeStage Stage;
        public int Molts,Broods,Babies;
        public bool MaleEaten;
        public string Pack() {
            return string.Join(",",new[]{Age.ToString("R",CultureInfo.InvariantCulture),SilkSeconds.ToString("R",CultureInfo.InvariantCulture),StageAge.ToString("R",CultureInfo.InvariantCulture),NextMolt.ToString("R",CultureInfo.InvariantCulture),NextMate.ToString("R",CultureInfo.InvariantCulture),GrowthAt.ToString("R",CultureInfo.InvariantCulture),GrowthSpan.ToString("R",CultureInfo.InvariantCulture),((int)Stage).ToString(),Molts.ToString(),Broods.ToString(),Babies.ToString(),MaleEaten?"1":"0"});
        }
        public static LifeRecord Unpack(string text) {
            string[] parts=text.Split(',');if(parts.Length!=12) return null;
            double[] n=new double[12];for(int i=0;i<n.Length;i++) if(!double.TryParse(parts[i],NumberStyles.Float,CultureInfo.InvariantCulture,out n[i]) || !World.Finite(n[i])) return null;
            if(n[0]<0 || n[0]>1e9 || n[1]<0 || n[1]>1e9 || n[2]<0 || n[2]>2592000 || n[3]<0 || n[3]>1e9 || n[4]<0 || n[4]>1e9 || n[5]<-1 || n[5]>n[0] || n[6]<60 || n[6]>604800) return null;
            for(int i=7;i<12;i++) if(n[i]!=Math.Floor(n[i])) return null;
            if(n[7]<0 || n[7]>12 || n[8]<0 || n[8]>100000 || n[9]<0 || n[9]>100000 || n[10]<0 || n[10]>6 || n[11]<0 || n[11]>1) return null;
            return new LifeRecord { Age=n[0],SilkSeconds=n[1],StageAge=n[2],NextMolt=n[3],NextMate=n[4],GrowthAt=n[5],GrowthSpan=n[6],Stage=(LifeStage)(int)n[7],Molts=(int)n[8],Broods=(int)n[9],Babies=(int)n[10],MaleEaten=n[11]==1 };
        }
    }
    public partial class World {
        public readonly Dictionary<string,LifeRecord> Lives=new Dictionary<string,LifeRecord>();
        public bool MoltingEnabled=true,MatingEnabled=true;
        public double MoltHours=6,MateHours=8;
        public const double GravidSeconds=1800,IncubationSeconds=2700,BroodSeconds=5400,DepartureSeconds=75;
        public LifeRecord Life { get { LifeRecord life;if(!Lives.TryGetValue(Species,out life)) { life=new LifeRecord { NextMolt=MoltHours*3600,NextMate=MateHours*3600 };Lives[Species]=life; }return life; } }
        public bool WebDweller { get { return SkinCatalog.Get(Species).WebDweller; } }
        public int WebLayers { get { return Math.Min(24,(int)Math.Sqrt(Life.SilkSeconds/1800)); } }
        public double LifeSize { get { var l=Life;if(l.GrowthAt<0) return 1;double t=Math.Max(0,Math.Min(1,(l.Age-l.GrowthAt)/l.GrowthSpan));return 1+.16*(1-t*t*(3-2*t)); } }
        public double GravidAmount { get { return Life.Stage==LifeStage.Gravid?Math.Min(1,Life.StageAge/GravidSeconds):Life.Stage==LifeStage.Laying?1:0; } }
        public bool LifeBusy { get { var s=Life.Stage;return s==LifeStage.PreparingMolt || s==LifeStage.Molting || s==LifeStage.Recovering || s==LifeStage.Courting || s==LifeStage.Mating || s==LifeStage.MaleMeal || s==LifeStage.Laying; } }
        public bool MaleVisible { get { var s=Life.Stage;return s==LifeStage.Courting || s==LifeStage.Mating || s==LifeStage.MaleLeaving || (s==LifeStage.MaleMeal && Life.StageAge<6); } }
        public bool HasSac { get { return Life.Stage==LifeStage.Laying || Life.Stage==LifeStage.Incubating || Life.Stage==LifeStage.Brood; } }
        public string LifeStatus { get { return Life.Stage==LifeStage.Quiet?"At home in its world · "+WebLayers+" lived-in silk layers":Life.Stage==LifeStage.PreparingMolt?"Returning for a molt":Life.Stage==LifeStage.Molting?"Shedding the old skin":Life.Stage==LifeStage.Recovering?"Resting after a molt":Life.Stage==LifeStage.Courting?"A same-species male is courting":Life.Stage==LifeStage.Mating?"A quiet mating visit":Life.Stage==LifeStage.MaleLeaving?"The male is leaving":Life.Stage==LifeStage.MaleMeal?"The male became a meal":Life.Stage==LifeStage.Gravid?"Gradually becoming gravid":Life.Stage==LifeStage.Laying?"Weaving an egg sac":Life.Stage==LifeStage.Incubating?"An egg sac is developing":Life.Stage==LifeStage.Brood?"Spiderlings are growing near the nest":"Spiderlings are setting off"; } }
        void LifeEnter(LifeStage stage) { Life.Stage=stage;Life.StageAge=0; }
        public void SetLifeIntervals(double moltHours,double mateHours) {
            if(!Finite(moltHours) || !Finite(mateHours)) return;
            moltHours=Math.Max(2,Math.Min(24,moltHours));mateHours=Math.Max(4,Math.Min(48,mateHours));
            if(MoltHours!=moltHours) { MoltHours=moltHours;Life.NextMolt=Life.Age+MoltHours*3600;
                if(Life.GrowthAt>=0) { if(Life.Age-Life.GrowthAt<Life.GrowthSpan) Life.GrowthSpan=Life.Age-Life.GrowthAt+MoltHours*3600;else Life.GrowthAt=-1; }
            }
            if(MateHours!=mateHours) { MateHours=mateHours;Life.NextMate=Life.Age+MateHours*3600; }
        }
        public void RestoreLife() {
            if(SkinCatalog.Get(Species).Male && Life.Stage>=LifeStage.Courting) { LifeEnter(LifeStage.Quiet);Life.Babies=0; }
            if(LifeBusy) { ClearPlay();HasFly=false;CancelWebVisit();Spider=Home;nestInside=true;Enter(Mood.Sleep); }
        }
        bool LifeAvailable { get { return !HasFly && !HoldingMeal && WebStage==WebVisit.None && !FingerActive && (State==Mood.Rest || State==Mood.Groom || State==Mood.Sleep || State==Mood.Walk); } }
        public bool InviteMate() {
            var skin=SkinCatalog.Get(Species);
            if(!MatingEnabled || skin.Male || skin.MateId.Length==0 || Life.Stage!=LifeStage.Quiet || !LifeAvailable) return false;
            ClearPlay();GoHome(false);LifeEnter(LifeStage.Courting);return true;
        }
        public bool BeginMolt() {
            if(!MoltingEnabled || Life.Stage!=LifeStage.Quiet || !LifeAvailable) return false;
            ClearPlay();GoHome(false);LifeEnter(LifeStage.PreparingMolt);return true;
        }
        // All clocks count active simulation time. Pausing, closed sessions and
        // suspend do not silently skip a visible molt, visit or brood departure.
        bool TickLife(double dt) {
            var l=Life;l.Age=Math.Min(1e9,l.Age+dt);
            if((Spider-Home).Length<220*HomeScale) l.SilkSeconds=Math.Min(1e9,l.SilkSeconds+dt);
            l.StageAge=Math.Min(2592000,l.StageAge+dt);
            if(l.Stage==LifeStage.Quiet) {
                if(MoltingEnabled && l.Age>=l.NextMolt) BeginMolt();
                else if(MatingEnabled && l.Age>=l.NextMate) InviteMate();
            }
            if(l.Stage==LifeStage.PreparingMolt) {
                if(State==Mood.Sleep && (Spider-Home).Length<3*HomeScale) LifeEnter(LifeStage.Molting);
                else { if(State!=Mood.Return && State!=Mood.EnterNest) GoHome(false);return false; }
            }
            if((l.Stage==LifeStage.Courting || l.Stage==LifeStage.Laying) && (Spider-Home).Length>3*HomeScale) {
                l.StageAge=0;if(State!=Mood.Return && State!=Mood.EnterNest) GoHome(false);return false;
            }
            bool exclusive=LifeBusy;
            if(exclusive) { ClearPlay();Spider=Home;Speed=GaitBlend=0;Angle=desiredAngle=NestAngle;State=Mood.Sleep;StateTime=1;nestInside=true;nestRest=Math.Min(1,nestRest+dt); }
            if(l.Stage==LifeStage.Courting || l.Stage==LifeStage.Mating || l.Stage==LifeStage.MaleMeal) {
                nestRest=0;State=l.Stage==LifeStage.Courting?Mood.Inspect:l.Stage==LifeStage.MaleMeal?Mood.Eat:Mood.Groom;StateTime=l.StageAge;
                if(l.Stage==LifeStage.MaleMeal) Spider=Home+NestForward*(12*HomeScale*Math.Min(1,l.StageAge/.22));
            }
            if(l.Stage==LifeStage.Molting) Spider=Home+NestForward*(10*Math.Max(0,(l.StageAge-30)/15)*HomeScale);
            if(l.Stage==LifeStage.Recovering) Spider=Home+NestForward*((10+20*Math.Min(1,l.StageAge/90))*HomeScale);
            switch(l.Stage) {
                case LifeStage.Molting:if(l.StageAge>=45) { l.Molts++;l.GrowthAt=l.Age;l.GrowthSpan=MoltHours*3600;l.NextMolt=l.Age+l.GrowthSpan;LifeEnter(LifeStage.Recovering); }break;
                case LifeStage.Recovering:if(l.StageAge>=90) { LifeEnter(LifeStage.Quiet);Wake(); }break;
                case LifeStage.Courting:if(l.StageAge>=28) LifeEnter(LifeStage.Mating);break;
                case LifeStage.Mating:if(l.StageAge>=22) { l.MaleEaten=random.NextDouble()<SkinCatalog.Get(Species).MateRisk;LifeEnter(l.MaleEaten?LifeStage.MaleMeal:LifeStage.MaleLeaving);if(l.MaleEaten && Meals<int.MaxValue) Meals++; }break;
                case LifeStage.MaleLeaving:case LifeStage.MaleMeal:if(l.StageAge>=12) { LifeEnter(LifeStage.Gravid);Wake(); }break;
                case LifeStage.Gravid:if(l.StageAge>=GravidSeconds && LifeAvailable) { GoHome(false);LifeEnter(LifeStage.Laying); }break;
                case LifeStage.Laying:if((Spider-Home).Length<3*HomeScale && l.StageAge>=30) { l.Broods++;LifeEnter(LifeStage.Incubating);Wake(); }break;
                case LifeStage.Incubating:if(l.StageAge>=IncubationSeconds) { l.Babies=6;LifeEnter(LifeStage.Brood); }break;
                case LifeStage.Brood:if(l.StageAge>=BroodSeconds) { SettleBrood();LifeEnter(LifeStage.Dispersing); }break;
                case LifeStage.Dispersing:if(l.StageAge>=DepartureSeconds) { l.Babies=0;l.NextMate=l.Age+MateHours*3600;LifeEnter(LifeStage.Quiet); }break;
            }
            return exclusive;
        }
        public V MatePosition {
            get { var l=Life;V start=Home+new V(Corner.EndsWith("Right")?-1:1,Corner.StartsWith("Top")?1:-1)*135*HomeScale;
                V contact=Home+NestForward*38*HomeScale;
                if(l.Stage==LifeStage.Courting) { double t=Math.Min(1,l.StageAge/28);return start+(contact-start)*(t*t*(3-2*t)); }
                if(l.Stage==LifeStage.MaleLeaving) return contact+(DeparturePoint(0)-contact)*Math.Min(1,l.StageAge/12);
                return contact;
            }
        }
        V DeparturePoint(int index) {
            var a=Areas[HomeArea];double margin=65*HomeScale,left=a.X,right=a.X+a.W,top=a.Y,bottom=a.Y+a.H;
            foreach(var screen in Areas) { left=Math.Min(left,screen.X);right=Math.Max(right,screen.X+screen.W);top=Math.Min(top,screen.Y);bottom=Math.Max(bottom,screen.Y+screen.H); }
            // Leave the entire desktop, not just the home screen at a shared seam.
            return index%2==0?new V(Corner.EndsWith("Right")?right+margin:left-margin,Math.Max(a.Y+10,Math.Min(a.Y+a.H-10,Home.Y+(index-3)*28*HomeScale))):new V(Math.Max(a.X+10,Math.Min(a.X+a.W-10,Home.X+(index-3)*28*HomeScale)),Corner.StartsWith("Top")?top-margin:bottom+margin);
        }
        public V BabyPosition(int index) {
            double a=index*2.399+Math.Sin(Life.Age*.13+index)*.18,r=(30+index*6)*HomeScale;
            V nest=Home+new V(Math.Cos(a),Math.Sin(a))*r;
            if(Life.Stage!=LifeStage.Dispersing) return Areas[HomeArea].Clamp(nest,10*HomeScale);
            double t=Math.Max(0,Math.Min(1,(Life.StageAge-index*5)/(DepartureSeconds-25)));
            return nest+(DeparturePoint(index)-nest)*(t*t*(3-2*t));
        }
        public double BabySize { get { return .11+.13*Math.Min(1,Life.Stage==LifeStage.Dispersing?1:Life.StageAge/BroodSeconds); } }
    }
}
