using System;
using System.IO;
using System.Globalization;

namespace CornerSpider {
    public class Settings {
        public string Corner="TopRight",Species="Regal",HomeMonitor="";
        public bool AutoFeed=true,CursorPlay=true,AllMonitors=true,SpeciesTraits=true,WebVisits=true;
        public double Scale=1,Web=.15;
        public double FingerTone=World.DefaultFingerTone,WebVisibility=1;
        public double FlyInterval=100,InviteDelay=4.5,CarryTolerance=850,AttentionDistance=185,DismissDistance=230;
        public int Meals;
        public static readonly string[] SpeciesNames=SkinCatalog.Names;
        public static readonly string[] Corners={"TopLeft","TopRight","BottomLeft","BottomRight"};
        public string LastError="";
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
            }
            DismissDistance=Math.Max(AttentionDistance+20,DismissDistance);
        }
        public bool Save(string folder) {
            try {
                Directory.CreateDirectory(folder);string path=Path.Combine(folder,"settings.txt"),temp=path+".new";
                File.WriteAllLines(temp,new string[]{"version=6","webVisibility="+WebVisibility.ToString(CultureInfo.InvariantCulture),"speciesTraits="+SpeciesTraits.ToString().ToLowerInvariant(),"webVisits="+WebVisits.ToString().ToLowerInvariant(),"corner="+Corner,"species="+Species,"homeMonitor="+HomeMonitor,"allMonitors="+AllMonitors.ToString().ToLowerInvariant(),"scale="+Scale.ToString(CultureInfo.InvariantCulture),"fingerTone="+FingerTone.ToString(CultureInfo.InvariantCulture),"web="+Web.ToString(CultureInfo.InvariantCulture),"flies="+AutoFeed.ToString().ToLowerInvariant(),"rides="+CursorPlay.ToString().ToLowerInvariant(),"meals="+Meals,"flyInterval="+FlyInterval.ToString(CultureInfo.InvariantCulture),"inviteDelay="+InviteDelay.ToString(CultureInfo.InvariantCulture),"carryTolerance="+CarryTolerance.ToString(CultureInfo.InvariantCulture),"attentionDistance="+AttentionDistance.ToString(CultureInfo.InvariantCulture),"dismissDistance="+DismissDistance.ToString(CultureInfo.InvariantCulture)});
                if(File.Exists(path)) File.Replace(temp,path,path+".bak");else File.Move(temp,path);
                LastError="";return true;
            } catch(IOException ex) { LastError=ex.Message;return false; }
              catch(UnauthorizedAccessException ex) { LastError=ex.Message;return false; }
        }
    }
}
