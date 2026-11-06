using System;
using System.Collections.Generic;
using System.Globalization;

namespace CornerSpider {
    public sealed class ColonyRecord {
        public string Species="Regal";
        public int Screen,Edge;
        public double Along=.5,Age,ExitAge;
        public bool Leaving;
        public string Pack() { return string.Join(",",new[]{Species,Screen.ToString(CultureInfo.InvariantCulture),Edge.ToString(),Along.ToString("R",CultureInfo.InvariantCulture),Age.ToString("R",CultureInfo.InvariantCulture),Leaving?"1":"0",ExitAge.ToString("R",CultureInfo.InvariantCulture)}); }
        public static ColonyRecord Unpack(string text) {
            string[] p=text.Split(',');if(p.Length!=7 || !SkinCatalog.Contains(p[0])) return null;
            int screen,edge;double along,age,exit;
            if(!int.TryParse(p[1],out screen) || screen<0 || screen>63 || !int.TryParse(p[2],out edge) || edge<0 || edge>3) return null;
            if(!double.TryParse(p[3],NumberStyles.Float,CultureInfo.InvariantCulture,out along) || !World.Finite(along) || along<.05 || along>.95) return null;
            if(!double.TryParse(p[4],NumberStyles.Float,CultureInfo.InvariantCulture,out age) || !World.Finite(age) || age<0 || age>1e9) return null;
            if((p[5]!="0" && p[5]!="1") || !double.TryParse(p[6],NumberStyles.Float,CultureInfo.InvariantCulture,out exit) || !World.Finite(exit) || exit<0 || exit>75) return null;
            return new ColonyRecord { Species=p[0],Screen=screen,Edge=edge,Along=along,Age=age,Leaving=p[5]=="1",ExitAge=exit };
        }
    }
    public sealed class ColonyPet {
        public readonly ColonyRecord Record;
        public readonly World Actor;
        internal V ExitStart;
        public ColonyPet(ColonyRecord record,int seed) { Record=record;Actor=new World(1200,900,seed) { IsColonyResident=true,AutoFeed=false,CursorPlay=false,MoltingEnabled=false,MatingEnabled=false,SpeciesTraits=false,WebVisits=false };Actor.SetSpecies(record.Species); }
    }
    public partial class World {
        public const int MaxResidents=12;
        public bool KeepBabies,IsColonyResident;
        public int ResidentLimit=6;
        public V ResidentHome;
        public double ResidentWebScale=.65;
        public readonly List<ColonyPet> Residents=new List<ColonyPet>();
        internal void CopyResidentPose(Mood state,int frame,int gait,int tuck,int direction) {
            Spider=new V(0,0);Scale=1;pixelScale=1;Angle=0;State=state;Clock=frame*.24;StateTime=frame*.24;
            StepPhase=frame*Math.PI*2/8;GaitBlend=gait*.5;nestRest=tuck*.5;GaitDirection=new V(Math.Cos(direction*Math.PI/4),Math.Sin(direction*Math.PI/4));
        }
        public void SetKeepBabies(bool enabled) { KeepBabies=enabled;if(!enabled) foreach(var p in Residents) BeginResidentExit(p); }
        public void SetResidentLimit(int limit) { ResidentLimit=Math.Max(1,Math.Min(MaxResidents,limit));for(int i=ResidentLimit;i<Residents.Count;i++) BeginResidentExit(Residents[i]); }
        void BeginResidentExit(ColonyPet p) { if(p.Record.Leaving) return;p.Record.Leaving=true;p.Record.ExitAge=0;p.ExitStart=p.Actor.Spider; }
        V ColonyHome(ColonyRecord r) {
            var a=Areas[Math.Min(r.Screen,Areas.Count-1)];double inset=Math.Min(130*Scale*.65*a.Dpi,Math.Min(a.W,a.H)*.25);
            return r.Edge==0?new V(a.X+a.W*r.Along,a.Y+inset):r.Edge==1?new V(a.X+a.W-inset,a.Y+a.H*r.Along):r.Edge==2?new V(a.X+a.W*r.Along,a.Y+a.H-inset):new V(a.X+inset,a.Y+a.H*r.Along);
        }
        bool ColonySiteFree(ColonyRecord r,ColonyPet ignore) {
            V p=ColonyHome(r);var a=Areas[r.Screen];double scale=Scale*a.Dpi;
            if(Math.Min(a.W,a.H)<260*scale*.65 || (p-Home).Length<260*HomeScale || GuestContains(p,170*scale*.65)) return false;
            if(r.Screen==HomeArea && HasLargeOrb) {
                V corner=new V(Corner.EndsWith("Right")?a.X+a.W:a.X,Corner.StartsWith("Top")?a.Y:a.Y+a.H);
                V hub=Home-corner,site=p-corner;
                // Leave room for the resident's own silk outside the main orb's
                // triangular frame, including the two longer wall attachments.
                double reach=Math.Max(Math.Abs(hub.X),Math.Abs(hub.Y))+OrbRoomReach*SilkScale;
                if(Math.Abs(site.X)+Math.Abs(site.Y)<reach+110*scale*.65) return false;
            }
            V outside=r.Edge==0?new V(p.X,a.Y-2):r.Edge==1?new V(a.X+a.W+2,p.Y):r.Edge==2?new V(p.X,a.Y+a.H+2):new V(a.X-2,p.Y);
            if(InsideAny(outside)) return false; // A monitor seam is not a wall to anchor silk to.
            foreach(var other in Residents) if(other!=ignore && (ColonyHome(other.Record)-p).Length<230*Math.Max(scale,Scale*Areas[Math.Min(other.Record.Screen,Areas.Count-1)].Dpi)*.65) return false;
            return true;
        }
        bool FindColonySite(ColonyRecord record,ColonyPet ignore) {
            for(int screen=0;screen<Areas.Count;screen++) for(int edge=0;edge<4;edge++) foreach(double along in new[]{.18,.82,.34,.66,.5}) {
                var candidate=new ColonyRecord { Screen=(screen+HomeArea)%Areas.Count,Edge=edge,Along=along };
                if(!ColonySiteFree(candidate,ignore)) continue;record.Screen=candidate.Screen;record.Edge=edge;record.Along=along;return true;
            }return false;
        }
        void ConfigureResident(ColonyPet p,bool reposition) {
            var r=p.Record;r.Screen=Math.Min(r.Screen,Areas.Count-1);var actor=p.Actor;
            actor.ResidentHome=ColonyHome(r);actor.Scale=Scale*(.24+.41*Math.Min(1,r.Age/7200));
            actor.ResidentWebScale=Scale*.65;
            actor.Corner=(r.Edge==0 || (r.Edge!=2 && r.Along<.5)?"Top":"Bottom")+(r.Edge==1 || (r.Edge!=3 && r.Along>.5)?"Right":"Left");
            actor.ConfigureDisplays(Areas.ToArray(),r.Screen);actor.Home=actor.ResidentHome;
            if(reposition) { actor.Spider=actor.Home;actor.GoHome(false); }p.ExitStart=actor.Spider;
        }
        public void RestoreResidents(IEnumerable<ColonyRecord> records) {
            Residents.Clear();foreach(var r in records) { if(Residents.Count>=MaxResidents) break;var pet=new ColonyPet(r,71+Residents.Count*139);Residents.Add(pet);ConfigureResident(pet,true); }
            ReflowResidents();SetResidentLimit(ResidentLimit);if(!KeepBabies) SetKeepBabies(false);
        }
        void ReflowResidents() {
            foreach(var p in Residents) { p.Record.Screen=Math.Min(p.Record.Screen,Areas.Count-1);if(!ColonySiteFree(p.Record,p) && !FindColonySite(p.Record,p)) BeginResidentExit(p);ConfigureResident(p,true); }
        }
        void SettleBrood() {
            if(!KeepBabies || IsColonyResident) return;
            while(Life.Babies>0 && Residents.Count<ResidentLimit && Residents.Count<MaxResidents) {
                int baby=Life.Babies-1;string skin=baby%2==0?Species:SkinCatalog.Get(Species).MateId;
                var record=new ColonyRecord { Species=skin };if(!FindColonySite(record,null)) break;
                var pet=new ColonyPet(record,71+Residents.Count*139);Residents.Add(pet);ConfigureResident(pet,true);
                pet.Actor.Spider=BabyPosition(baby);pet.Actor.GoHome(false);Life.Babies--;
            }
        }
        void TickResidents(double dt) {
            for(int i=Residents.Count-1;i>=0;i--) {
                var p=Residents[i];var r=p.Record;var actor=p.Actor;r.Age=Math.Min(1e9,r.Age+dt);
                actor.Scale=Scale*(.24+.41*Math.Min(1,r.Age/7200));actor.WebVisibility=WebVisibility;actor.Web=Math.Min(1,.15+r.Age/480);actor.Life.SilkSeconds=r.Age;
                if(r.Leaving) {
                    r.ExitAge+=dt;double t=Math.Min(1,r.ExitAge/45);V end=actor.DeparturePoint(r.Edge==0 || r.Edge==2?1:0);
                    V previous=actor.Spider;actor.Spider=p.ExitStart+(end-p.ExitStart)*(t*t*(3-2*t));V move=actor.Spider-previous;
                    if(move.Length>.001) actor.Angle=Math.Atan2(move.Y,move.X)*180/Math.PI+90;
                    actor.State=Mood.Walk;actor.GaitBlend=.6;actor.StepPhase+=dt*7;actor.Clock+=dt;actor.WebVisibility*=Math.Max(0,1-(r.ExitAge-45)/15);
                    if(r.ExitAge>=60) Residents.RemoveAt(i);
                } else actor.Tick(dt,new V(-100000,-100000));
            }
        }
    }
}
