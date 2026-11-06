using System;
using System.Collections.Generic;

namespace CornerSpider {
    public enum WebKind { Retreat, Orb, Tangle, Sheet, Crystal }
    internal static class OrbRadials {
        internal static int Count(SkinProfile skin) {
            switch(skin.Pattern) {
                case "Golden":return 34;
                case "Cross":case "Marble":return 25;
                case "Garden":return 27;
                default:return skin.WebKind==WebKind.Crystal?20:29;
            }
        }
        internal static double Phase(SkinProfile skin) {
            switch(skin.Pattern) {
                case "Golden":return .38;
                case "Marble":return .73;
                case "Garden":return .17;
                default:return skin.WebKind==WebKind.Crystal?1.26:.51;
            }
        }
        internal static V Ray(int index,int count,double phase) {
            double irregularity=.27*Math.Sin(index*2.39+phase)+.13*Math.Sin(index*4.16);
            double angle=(index+irregularity)*(2*Math.PI/count)+phase;
            return new V(Math.Cos(angle),Math.Sin(angle));
        }
    }
    public static partial class SkinCatalog {
        public static readonly string[] Categories={"Jumping spiders","Orb weavers","Widows","House spiders","Fictional"};
        public static SkinProfile[] InCategory(string category) { return Array.FindAll(All,delegate(SkinProfile s) { return s.Category==category; }); }
        static SkinProfile[] Extend(SkinProfile[] originals) {
            var list=new List<SkinProfile>(originals);
            var portiaMale=originals[18].Variant("PortiaLabiataMale","Portia labiata - male","Male");
            portiaMale.BodyWidth=6.9;portiaMale.BodyLength=13.3;portiaMale.FrontLeg=1.17;portiaMale.HeadWidth=.78;
            portiaMale.Description="Slim angular male, pale palps and long sparse fringes. The same spider-hunting specialist.";list.Add(portiaMale);
            var hyllusMale=originals[19].Variant("HyllusDiardiMale","Hyllus diardi - male","Male");
            hyllusMale.Body="#464039";hyllusMale.Head="#554b40";hyllusMale.BodyWidth=12.4;hyllusMale.BodyLength=18;hyllusMale.FrontLeg=1.09;
            hyllusMale.Legs="#494139";hyllusMale.Hair="#c6b796";hyllusMale.LegShade="#25221e";
            hyllusMale.Description="Dark brown male with pale facial tufts, a slimmer abdomen and sturdy front legs.";list.Add(hyllusMale);
            Pair(list,"GardenOrb","Yellow garden","Argiope aurantia","Orb weavers",WebKind.Orb,"#e9c548","#272824","Garden",.12);
            Pair(list,"CrossOrb","Cross orb-weaver","Araneus diadematus","Orb weavers",WebKind.Orb,"#a3774c","#59402c","Cross",.12);
            Pair(list,"MarbledOrb","Marbled orb-weaver","Araneus marmoreus","Orb weavers",WebKind.Orb,"#e5a13d","#6f3928","Marble",.12);
            Pair(list,"GoldenOrb","Golden silk","Trichonephila clavipes","Orb weavers",WebKind.Orb,"#c6b77b","#555443","Golden",.10);
            Pair(list,"SouthernWidow","Southern black widow","Latrodectus mactans","Widows",WebKind.Tangle,"#27292d","#171b20","Widow",.08);
            Pair(list,"BrownWidow","Brown widow","Latrodectus geometricus","Widows",WebKind.Tangle,"#987b62","#4f4138","BrownWidow",.22);
            Pair(list,"RedbackWidow","Redback","Latrodectus hasselti","Widows",WebKind.Tangle,"#26282b","#16191e","Redback",.55);
            Pair(list,"HouseTangle","Common house","Parasteatoda tepidariorum","House spiders",WebKind.Tangle,"#9e8969","#645744","House",.08);
            Pair(list,"BarnFunnel","Barn funnel weaver","Tegenaria domestica","House spiders",WebKind.Sheet,"#a99673","#655741","Funnel",.06);
            Pair(list,"Nebula","Nebula weaver","Fictional / cosmic silk","Fictional",WebKind.Orb,"#7565c1","#294d70","Nebula",0);
            Pair(list,"Crystal","Crystal spinner","Fictional / crystalline silk","Fictional",WebKind.Crystal,"#88cec2","#4c70a1","Crystal",0);
            Pair(list,"Copperbot","Copper clockwork","Fictional / mechanical spider","Fictional",WebKind.Sheet,"#b47b4d","#44575c","Mech",0);
            foreach(var s in list) foreach(var other in list)
                if(s.Scientific==other.Scientific && s.Male!=other.Male) { s.MateId=other.Id;break; }
            return list.ToArray();
        }
        static void Pair(List<SkinProfile> list,string id,string name,string scientific,string category,WebKind web,string body,string head,string pattern,double risk) {
            bool widow=category=="Widows",funnel=web==WebKind.Sheet;
            var f=new SkinProfile { Id=id,Name=name+" - female",Scientific=scientific,Category=category,WebKind=web,WebDweller=true,
                Fictional=category=="Fictional",MateRisk=risk,Pattern=pattern,Body=body,Head=head,Legs=head,BodyShade="#25292d",HeadShade="#151a20",LegShade="#20242a",
                Hair="#b5a58d",Mark="#f0e4c5",Mouth="#665546",MouthShade="#28242a",BodyWidth=widow?17:funnel?12:18,
                BodyLength=widow?20:funnel?20:23,HeadWidth=.65,HeadLength=.72,LegWidth=.48,CoatDensity=.22,HairLength=.35,LegHair=.25,
                FrontLeg=1.10,SideLeg=1.1,RearLeg=1.2,FrontBulk=.70,
                Description=category=="Fictional"?"An original imaginary spider with a distinct body and silk style.":web==WebKind.Orb?"A small front body, banded legs and a rounded abdomen; waits on a radial capture web.":web==WebKind.Tangle?"A rounded abdomen and slender legs; rests in a three-dimensional tangle of capture silk.":"An elongated house spider with long legs, a sheet web and a funnel-shaped retreat." };
            if(pattern=="Golden") { f.BodyWidth=11;f.BodyLength=26;f.LegWidth=.42; }
            if(pattern=="Garden") { f.BodyWidth=15;f.BodyLength=24;f.Head="#a8aa9b";f.HeadShade="#5b6057"; }
            if(pattern=="Cross") { f.BodyWidth=19;f.BodyLength=21; }
            if(pattern=="Marble") { f.BodyWidth=22;f.BodyLength=22; }
            if(pattern=="BrownWidow") { f.BodyWidth=16;f.BodyLength=19; }
            if(pattern=="Redback") { f.BodyWidth=16;f.BodyLength=21; }
            if(pattern=="House") { f.BodyWidth=13;f.BodyLength=18; }
            if(pattern=="Nebula") { f.BodyWidth=16;f.BodyLength=25; }
            if(pattern=="Crystal") { f.BodyWidth=21;f.BodyLength=19; }
            if(pattern=="Mech") { f.BodyWidth=14;f.BodyLength=22; }
            var m=f.Variant(id+"Male",name+" - male","Male");m.BodyWidth*=.68;m.BodyLength*=.73;m.HeadWidth=.61;m.FrontLeg=1.18;m.MateRisk=0;
            m.Description="Smaller, slimmer male with fuller palps. Roams locally and visits a female of the same species.";
            list.Add(f);list.Add(m);
        }
    }
}
