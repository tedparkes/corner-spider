using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;

namespace CornerSpider {
    public class Settings {
        public string Corner="TopRight",Species="Regal",HomeMonitor="";
        public bool AutoFeed=true,CursorPlay=true,AllMonitors=true,SpeciesTraits=true,WebVisits=true;
        public double Scale=1,Web=.15;
        public double FingerTone=World.DefaultFingerTone,WebVisibility=1;
        public double FlyInterval=100,InviteDelay=4.5,CarryTolerance=850,AttentionDistance=185,DismissDistance=230;
        public int Meals;
        public bool MoltingEnabled=true,MatingEnabled=true;
        public double MoltHours=6,MateHours=8;
        public readonly Dictionary<string,LifeRecord> Lives=new Dictionary<string,LifeRecord>();
        public bool KeepBabies,AutoUpdateCheck,PreviewUpdates;
        public bool? TrayUpdateShortcut;
        public string HabitatId="";
        public bool HabitatWhiteNear=true;
        public int ResidentLimit=6;
        public long LastUpdateCheckTicks;
        public readonly List<ColonyRecord> Colonists=new List<ColonyRecord>();
        public static readonly string[] SpeciesNames=SkinCatalog.Names;
        public static readonly string[] Corners={"TopLeft","TopRight","BottomLeft","BottomRight"};
        public string LastError="";
        internal static bool DefaultTrayUpdateShortcut(string version) {
            var release=ReleaseVersion.Parse(version);
            return release!=null && release.Preview.Length>0;
        }
        internal bool TrayUpdateShortcutEnabled(string version) {
            return TrayUpdateShortcut ?? DefaultTrayUpdateShortcut(version);
        }
        public static Settings Load(string folder) {
            var s=new Settings();string path=Path.Combine(folder,"settings.txt");
            try { if(File.Exists(path)) s.Parse(File.ReadAllLines(path));else if(File.Exists(path+".bak")) s.Parse(File.ReadAllLines(path+".bak")); }
            catch(IOException ex) { s.LastError=ex.Message;TryBackup(s,path); }
            catch(UnauthorizedAccessException ex) { s.LastError=ex.Message; }
            return s;
        }
        static void TryBackup(Settings s,string path) { try { if(File.Exists(path+".bak")) s.Parse(File.ReadAllLines(path+".bak")); } catch(IOException) {} catch(UnauthorizedAccessException) {} }
        public void Parse(string[] lines) {
            foreach(string line in lines) {
                int split=line.IndexOf('=');if(split<0) continue;string k=line.Substring(0,split),v=line.Substring(split+1);double n;int count;
                if(k=="corner" && Array.IndexOf(Corners,v)>=0) Corner=v;
                if(k=="species" && Array.IndexOf(SpeciesNames,v)>=0) Species=v;
                if(k=="scale" && double.TryParse(v,NumberStyles.Float,CultureInfo.InvariantCulture,out n) && World.Finite(n)) Scale=Math.Max(.2,Math.Min(1.8,n));
                if(k=="webVisibility" && double.TryParse(v,NumberStyles.Float,CultureInfo.InvariantCulture,out n) && World.Finite(n)) WebVisibility=Math.Max(0,Math.Min(1,n));
                if(k=="fingerTone" && double.TryParse(v,NumberStyles.Float,CultureInfo.InvariantCulture,out n) && World.Finite(n)) FingerTone=Math.Max(0,Math.Min(1,n));
                if(k=="flyInterval" && double.TryParse(v,NumberStyles.Float,CultureInfo.InvariantCulture,out n) && World.Finite(n)) FlyInterval=Math.Max(30,Math.Min(300,n));
                if(k=="inviteDelay" && double.TryParse(v,NumberStyles.Float,CultureInfo.InvariantCulture,out n) && World.Finite(n)) InviteDelay=Math.Max(2,Math.Min(10,n));
                if(k=="carryTolerance" && double.TryParse(v,NumberStyles.Float,CultureInfo.InvariantCulture,out n) && World.Finite(n)) CarryTolerance=Math.Max(300,Math.Min(1500,n));
                if(k=="attentionDistance" && double.TryParse(v,NumberStyles.Float,CultureInfo.InvariantCulture,out n) && World.Finite(n)) AttentionDistance=Math.Max(60,Math.Min(400,n));
                if(k=="dismissDistance" && double.TryParse(v,NumberStyles.Float,CultureInfo.InvariantCulture,out n) && World.Finite(n)) DismissDistance=Math.Max(80,Math.Min(600,n));
                if(k=="web" && double.TryParse(v,NumberStyles.Float,CultureInfo.InvariantCulture,out n) && World.Finite(n)) Web=Math.Max(.15,Math.Min(1,n));
                if(k=="speciesTraits" && (v=="true" || v=="false")) SpeciesTraits=v=="true";
                if(k=="webVisits" && (v=="true" || v=="false")) WebVisits=v=="true";
                if(k=="flies" && (v=="true" || v=="false")) AutoFeed=v=="true";
                if(k=="rides" && (v=="true" || v=="false")) CursorPlay=v=="true";
                if(k=="allMonitors" && (v=="true" || v=="false")) AllMonitors=v=="true";
                // v1's monitor key is intentionally not the home preference: v2 defaults to the right screen.
                if(k=="homeMonitor") HomeMonitor=v;
                if(k=="meals" && int.TryParse(v,out count)) Meals=Math.Max(0,count);
                if(k=="molting" && (v=="true" || v=="false")) MoltingEnabled=v=="true";
                if(k=="mating" && (v=="true" || v=="false")) MatingEnabled=v=="true";
                if(k=="moltHours" && double.TryParse(v,NumberStyles.Float,CultureInfo.InvariantCulture,out n) && World.Finite(n)) MoltHours=Math.Max(2,Math.Min(24,n));
                if(k=="mateHours" && double.TryParse(v,NumberStyles.Float,CultureInfo.InvariantCulture,out n) && World.Finite(n)) MateHours=Math.Max(4,Math.Min(48,n));
                if(k.StartsWith("life.") && SkinCatalog.Contains(k.Substring(5))) { var life=LifeRecord.Unpack(v);if(life!=null) Lives[k.Substring(5)]=life; }
                if(k=="keepBabies" && (v=="true" || v=="false")) KeepBabies=v=="true";
                if(k=="residentLimit" && int.TryParse(v,out count)) ResidentLimit=Math.Max(1,Math.Min(World.MaxResidents,count));
                if(k=="autoUpdateCheck" && (v=="true" || v=="false")) AutoUpdateCheck=v=="true";
                if(k=="previewUpdates" && (v=="true" || v=="false")) PreviewUpdates=v=="true";
                if(k=="trayUpdateShortcut" && (v=="true" || v=="false")) TrayUpdateShortcut=v=="true";
                Guid habitat;if(k=="habitatId" && Guid.TryParseExact(v,"N",out habitat)) HabitatId=habitat.ToString("N");
                if(k=="habitatWhiteNear" && (v=="true" || v=="false")) HabitatWhiteNear=v=="true";
                long ticks;if(k=="lastUpdateCheck" && long.TryParse(v,out ticks) && ticks>=0 && ticks<=DateTime.MaxValue.Ticks) LastUpdateCheckTicks=ticks;
                if(k.StartsWith("colony.") && Colonists.Count<World.MaxResidents) { var resident=ColonyRecord.Unpack(v);if(resident!=null) Colonists.Add(resident); }
            }
            DismissDistance=Math.Max(AttentionDistance+20,DismissDistance);
        }
        public bool Save(string folder) {
            try {
                Directory.CreateDirectory(folder);string path=Path.Combine(folder,"settings.txt"),temp=path+".new";
                File.WriteAllLines(temp,new string[]{"version=8","webVisibility="+WebVisibility.ToString(CultureInfo.InvariantCulture),"speciesTraits="+SpeciesTraits.ToString().ToLowerInvariant(),"webVisits="+WebVisits.ToString().ToLowerInvariant(),"corner="+Corner,"species="+Species,"homeMonitor="+HomeMonitor,"allMonitors="+AllMonitors.ToString().ToLowerInvariant(),"scale="+Scale.ToString(CultureInfo.InvariantCulture),"fingerTone="+FingerTone.ToString(CultureInfo.InvariantCulture),"web="+Web.ToString(CultureInfo.InvariantCulture),"flies="+AutoFeed.ToString().ToLowerInvariant(),"rides="+CursorPlay.ToString().ToLowerInvariant(),"meals="+Meals,"flyInterval="+FlyInterval.ToString(CultureInfo.InvariantCulture),"inviteDelay="+InviteDelay.ToString(CultureInfo.InvariantCulture),"carryTolerance="+CarryTolerance.ToString(CultureInfo.InvariantCulture),"attentionDistance="+AttentionDistance.ToString(CultureInfo.InvariantCulture),"dismissDistance="+DismissDistance.ToString(CultureInfo.InvariantCulture)});
                var lifeLines=new List<string>();
                lifeLines.Add("keepBabies="+KeepBabies.ToString().ToLowerInvariant());lifeLines.Add("residentLimit="+ResidentLimit);
                lifeLines.Add("autoUpdateCheck="+AutoUpdateCheck.ToString().ToLowerInvariant());lifeLines.Add("previewUpdates="+PreviewUpdates.ToString().ToLowerInvariant());lifeLines.Add("lastUpdateCheck="+LastUpdateCheckTicks);
                if(TrayUpdateShortcut.HasValue) lifeLines.Add("trayUpdateShortcut="+TrayUpdateShortcut.Value.ToString().ToLowerInvariant());
                if(HabitatId.Length>0) lifeLines.Add("habitatId="+HabitatId);
                lifeLines.Add("habitatWhiteNear="+HabitatWhiteNear.ToString().ToLowerInvariant());
                for(int i=0;i<Math.Min(World.MaxResidents,Colonists.Count);i++) lifeLines.Add("colony."+i+"="+Colonists[i].Pack());
                lifeLines.Add("molting="+MoltingEnabled.ToString().ToLowerInvariant());lifeLines.Add("mating="+MatingEnabled.ToString().ToLowerInvariant());
                lifeLines.Add("moltHours="+MoltHours.ToString(CultureInfo.InvariantCulture));lifeLines.Add("mateHours="+MateHours.ToString(CultureInfo.InvariantCulture));
                foreach(var entry in Lives) if(SkinCatalog.Contains(entry.Key)) lifeLines.Add("life."+entry.Key+"="+entry.Value.Pack());
                File.AppendAllLines(temp,lifeLines);
                if(File.Exists(path)) File.Replace(temp,path,path+".bak");else File.Move(temp,path);
                LastError="";return true;
            } catch(IOException ex) { LastError=ex.Message;return false; }
              catch(UnauthorizedAccessException ex) { LastError=ex.Message;return false; }
        }
    }
}
