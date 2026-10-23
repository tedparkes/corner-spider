using System;

namespace CornerSpider {
    // Named regions are kept separate so a future custom-color editor can reuse
    // this palette contract. No custom colors or external skin files are loaded yet.
    public sealed class SkinProfile {
        public string Id,Name,Scientific,Description,Pattern="Spots",Sex="Female",Shape="Rounded";
        public string Body="#293034",BodyShade="#090c10",Head="#30373a",HeadShade="#0b1014";
        public string Legs="#343b3c",LegShade="#0b1012",Hair="#959c94",Mark="#eeeadd";
        public string Mouth="#39b6a2",MouthShade="#063235",Palps="#e7e4d8";
        public string HeadHair,LegFur;
        public double BodyWidth=14,BodyLength=17,HeadWidth=1,LegWidth=1;
        public double HeadLength=1,HeadRear=1,BodyTaper=.08,FrontLeg=1,SideLeg=1,RearLeg=1,Stance=1,Chelicera=1;
        public double CoatDensity=1,HairLength=1,LegHair=1,FrontBulk=1,MiddleEye=.65;
        public bool SpiderPrey;
        public bool Male { get { return Sex=="Male"; } }
        public double PalpWidth { get { return (Male?2.25:1.25)*HeadWidth; } }
        public double LimbScale(int leg) { return leg==0?FrontLeg:leg==3?RearLeg:SideLeg; }
        internal SkinProfile Variant(string id,string name,string sex) { var copy=(SkinProfile)MemberwiseClone();copy.Id=id;copy.Name=name;copy.Sex=sex;return copy; }
        public override string ToString() { return Name; }
    }
    public static class SkinCatalog {
        public static readonly SkinProfile[] All=Create();
        static SkinProfile[] Create() {
        var original=new SkinProfile[]{
            new SkinProfile { Id="Regal",Name="Regal - warm female",Scientific="Phidippus regius",Description="Warm orange-brown scales, pale abdominal marks and violet mouthparts.",Pattern="Regal",Body="#b38659",BodyShade="#553d2d",Head="#996c4c",HeadShade="#392b25",Legs="#74513a",LegShade="#271d18",Hair="#d4b58c",Mark="#f0dfbd",Mouth="#ad80be",MouthShade="#47284d",BodyWidth=15,BodyLength=18 },
            new SkinProfile { Id="Bold",Name="Bold",Scientific="Phidippus audax",Description="Black coat, one large pale abdominal spot, two rear spots and green-blue mouthparts." },
            new SkinProfile { Id="Zebra",Name="Zebra",Scientific="Salticus scenicus",Description="Contrasting transverse ivory bands, ringed legs and a slimmer body.",Pattern="Zebra",BodyWidth=11.8,BodyLength=17,HeadWidth=.91,LegWidth=.66,Mark="#e9e6da",Mouth="#826653",MouthShade="#2b211b" },
            new SkinProfile { Id="RegalMale",Name="Regal - black male",Scientific="Phidippus regius",Description="Black and ivory, a pale basal band, triangular center spot and strong foreleg fringes.",Pattern="Regal",BodyWidth=13,BodyLength=16,Mark="#faf4e7",Hair="#b4b8ae",Palps="#f8f3e8",Mouth="#52b9b0" },
            new SkinProfile { Id="Tan",Name="Tan",Scientific="Platycryptus undatus",Description="Bark-colored mottling and wavy chevrons on a flattened, elongated abdomen.",Pattern="Tan",Body="#aaa28b",BodyShade="#514c40",Head="#857e6b",HeadShade="#393831",Legs="#8c8069",LegShade="#3e382d",Hair="#d1c7af",Mark="#e2d8bd",Mouth="#302d29",MouthShade="#0a0a09",Palps="#e5dfd1",BodyWidth=12.5,BodyLength=18.5,HeadWidth=.94,LegWidth=.72 },
            new SkinProfile { Id="RedBack",Name="Red-backed - male",Scientific="Phidippus johnsoni",Description="A red abdominal coat with a black front body and dark legs.",Pattern="Red",Body="#d34236",BodyShade="#751f22",Hair="#c8826c",BodyWidth=13.5,BodyLength=17.5,Palps="#8c9290" },
            new SkinProfile { Id="Emerald",Name="Emerald",Scientific="Paraphidippus aurantius",Description="Green-gold dorsal sheen with paired pale spots and pale side edging.",Pattern="Emerald",Body="#588c66",BodyShade="#112e28",Head="#638b60",HeadShade="#142f28",Legs="#3a5046",LegShade="#112321",Hair="#a4b991",Mark="#eee7c8",Mouth="#709e74",MouthShade="#183e31",BodyWidth=13.5,BodyLength=17,LegWidth=.84 },
            new SkinProfile { Id="GrayWall",Name="Gray wall - female",Scientific="Menemerus bivittatus",Description="Gray leaf-like abdominal field, dark side bands and a fine pale border on the front body.",Pattern="Wall",Body="#aaa99d",BodyShade="#545950",Head="#72786e",HeadShade="#292e2a",Legs="#777d70",LegShade="#30372f",Hair="#c1c7b7",Mark="#e0e4d5",Mouth="#966d4b",MouthShade="#38271e",Palps="#d6d3bd",BodyWidth=12,BodyLength=18.5,HeadWidth=.92,LegWidth=.69 },
            new SkinProfile { Id="Pantropical",Name="Pantropical - male",Scientific="Plexippus paykulli",Description="A continuous pale central stripe, dark side stripes and paired pale rear spots.",Pattern="Stripe",BodyWidth=12.5,BodyLength=18,HeadWidth=.96,LegWidth=.8,Mouth="#8a7051",MouthShade="#2c241d",Hair="#adb2a7" },
            new SkinProfile { Id="Apache",Name="Apache",Scientific="Phidippus apacheanus",Description="Orange-red scales on both body sections, a dark eye mask and green mouthparts.",Pattern="Apache",Body="#c55f37",BodyShade="#6a2f20",Head="#b95832",HeadShade="#5a2d20",Legs="#352b26",LegShade="#110f0e",Hair="#db9b66",Mark="#d89b6a",Palps="#c7b7a0",BodyWidth=14,BodyLength=17.5 }
        };
        var regal=original[0];regal.BodyTaper=.17;
        var bold=original[1];bold.Name="Bold - male";bold.Sex="Male";bold.BodyWidth=12.6;bold.BodyLength=16;bold.FrontLeg=1.05;
        var zebra=original[2];zebra.Name="Zebra - female";zebra.BodyWidth=11.4;zebra.BodyLength=15;zebra.HeadLength=.88;zebra.HeadRear=.86;zebra.FrontLeg=.82;zebra.SideLeg=.86;zebra.RearLeg=.88;zebra.Stance=.87;
        var regalMale=original[3];regalMale.Sex="Male";regalMale.FrontLeg=1.07;regalMale.BodyTaper=-.08;
        var tan=original[4];tan.Name="Tan - female";tan.Shape="Flat";tan.HeadLength=1.08;tan.HeadRear=.8;tan.BodyTaper=-.12;tan.FrontLeg=.9;tan.SideLeg=.93;tan.RearLeg=1.04;tan.Stance=1.05;
        var red=original[5];red.Sex="Male";red.FrontLeg=1.03;red.BodyTaper=-.07;
        var emerald=original[6];emerald.Name="Emerald - female";emerald.HeadWidth=1.02;emerald.BodyWidth=14.5;emerald.BodyLength=18;emerald.BodyTaper=.13;emerald.FrontLeg=.95;emerald.RearLeg=1.06;
        var wall=original[7];wall.Shape="Flat";wall.HeadLength=1.06;wall.HeadRear=.77;wall.BodyTaper=-.12;wall.FrontLeg=.86;wall.SideLeg=.91;wall.RearLeg=1.02;wall.Stance=1.02;
        var pantropical=original[8];pantropical.Sex="Male";pantropical.HeadLength=1.04;pantropical.HeadRear=.89; pantropical.BodyTaper=-.16;pantropical.FrontLeg=.94;pantropical.RearLeg=1.07;
        var apache=original[9];apache.Name="Apache - male";apache.Sex="Male";apache.BodyWidth=13;apache.BodyLength=16.5;apache.BodyTaper=-.06;apache.FrontLeg=1.05;

        var boldFemale=bold.Variant("BoldFemale","Bold - female","Female");boldFemale.BodyWidth=15;boldFemale.BodyLength=18.5;boldFemale.BodyTaper=.17;boldFemale.FrontLeg=.95;boldFemale.Description="Fuller oval abdomen, pale three-spot pattern, green mouthparts and slender pale palps.";
        var zebraMale=zebra.Variant("ZebraMale","Zebra - male","Male");zebraMale.BodyWidth=9.7;zebraMale.BodyLength=13.4;zebraMale.HeadWidth=.96;zebraMale.Chelicera=1.75;zebraMale.LegWidth=.76;zebraMale.FrontLeg=.9;zebraMale.Description="Compact striped abdomen, darker sturdy legs and the male's noticeably longer chelicerae.";
        var tanMale=tan.Variant("TanMale","Tan - male","Male");tanMale.BodyWidth=10.3;tanMale.BodyLength=17;tanMale.HeadWidth=1;tanMale.FrontLeg=.97;tanMale.Palps="#bba287";tanMale.Description="Low, long bark-colored body, slimmer abdomen, warm face band and larger terminal palp bulbs.";
        var redFemale=red.Variant("RedBackFemale","Red-backed - female","Female");redFemale.BodyWidth=15.4;redFemale.BodyLength=19;redFemale.BodyTaper=.14;redFemale.Pattern="RedStripe";redFemale.FrontLeg=.96;redFemale.Description="A fuller red abdomen with a dark central stripe, black front body and fine palps.";
        var emeraldMale=emerald.Variant("EmeraldMale","Emerald - male","Male");emeraldMale.Body="#253e34";emeraldMale.Head="#304b3b";emeraldMale.BodyWidth=11.8;emeraldMale.BodyLength=16;emeraldMale.HeadWidth=1.05;emeraldMale.BodyTaper=-.08;emeraldMale.FrontLeg=1.02;emeraldMale.Description="Darker green-black sheen, strong pale edging, slimmer abdomen and fuller male palps.";
        var wallMale=wall.Variant("GrayWallMale","Gray wall - male","Male");wallMale.BodyWidth=10.1;wallMale.BodyLength=17;wallMale.HeadWidth=.99;wallMale.Pattern="WallMale";wallMale.Body="#454c43";wallMale.FrontLeg=.94;wallMale.Description="Flat, elongated silhouette with a dark central abdominal field, pale sides and a white head stripe.";
        var pantropicalFemale=pantropical.Variant("PantropicalFemale","Pantropical - female","Female");pantropicalFemale.BodyWidth=13.8;pantropicalFemale.BodyLength=19.5;pantropicalFemale.BodyTaper=.07;pantropicalFemale.FrontLeg=.86;pantropicalFemale.Pattern="StripeFemale";pantropicalFemale.Body="#a69875";pantropicalFemale.BodyShade="#554b36";pantropicalFemale.Head="#887653";pantropicalFemale.HeadShade="#352d23";pantropicalFemale.Legs="#77694d";pantropicalFemale.Description="Fuller brown abdomen with a tan central field, subdued chevrons and paired rear marks; a dark eye field.";
        var apacheFemale=apache.Variant("ApacheFemale","Apache - female","Female");apacheFemale.BodyWidth=15.5;apacheFemale.BodyLength=19;apacheFemale.BodyTaper=.16;apacheFemale.Pattern="ApacheStripe";apacheFemale.FrontLeg=.96;apacheFemale.Description="Broad orange-red abdomen with a dark median band, orange front body and green chelicerae.";
        var portia=new SkinProfile { Id="PortiaLabiata",SpiderPrey=true,Name="Portia labiata - female",Scientific="Portia labiata",Sex="Female",Shape="Tufted",Pattern="Portia",Description="Small angular body, long slender fringed legs, mottled brown coat and the female's pale moustache. Hunts small spiders as well as flies.",Body="#8c7151",BodyShade="#372a24",Head="#8b6240",HeadShade="#322720",Legs="#806143",LegShade="#30251e",Hair="#c5b18d",Palps="#e2d5b7",Mark="#c6b494",Mouth="#69432d",MouthShade="#211914",BodyWidth=10.3,BodyLength=17.5,BodyTaper=.2,HeadWidth=.8,HeadLength=.92,HeadRear=.63,LegWidth=.49,FrontLeg=1.08,SideLeg=1.32,RearLeg=1.5,Stance=1.11 };
        var hyllus=new SkinProfile { Id="HyllusDiardiGreen",Name="Hyllus diardi - green female",Scientific="Hyllus diardi",Sex="Female",Shape="Heavy",Pattern="Hyllus",Description="Broad head, long full abdomen, eye tufts and chevrons. Requested stylized green palette; wild females are normally brown/yellow, not bright green.",Body="#83a853",BodyShade="#344a27",Head="#6e8b40",HeadShade="#26331e",Legs="#67804a",LegShade="#2b3824",Hair="#c3cd8d",Mark="#d4d4a3",Palps="#dbd8ae",Mouth="#8a8146",MouthShade="#353420",BodyWidth=16,BodyLength=23,BodyTaper=.05,HeadWidth=1.07,HeadLength=.96,HeadRear=1.04,LegWidth=1.03,FrontLeg=.9,SideLeg=1.25,RearLeg=1.31,Stance=1.05 };
        // Coat density and silhouette hairs are independent of limb width: fine
        // legs must not disappear inside the same furry outline as a Phidippus.
        foreach(var skin in new[]{zebra,zebraMale}) { skin.CoatDensity=.58;skin.HairLength=.40;skin.LegHair=.35;skin.FrontBulk=.83; }
        foreach(var skin in new[]{tan,tanMale,wall,wallMale}) { skin.CoatDensity=.65;skin.HairLength=.50;skin.LegHair=.38;skin.FrontBulk=.86; }
        tan.BodyWidth=11.7;tan.BodyLength=20;tanMale.BodyWidth=10;tanMale.BodyLength=18.5;
        wall.BodyWidth=11.3;wall.BodyLength=20;wallMale.BodyWidth=9.8;wallMale.BodyLength=18.5;
        wallMale.Head="#454d43";wallMale.HeadShade="#1b241e";
        foreach(var skin in new[]{pantropical,pantropicalFemale}) { skin.CoatDensity=.68;skin.HairLength=.52;skin.LegHair=.48;skin.FrontBulk=.86; }
        foreach(var skin in new[]{emerald,emeraldMale}) { skin.CoatDensity=.75;skin.HairLength=.58;skin.LegHair=.55;skin.Legs="#775d3e";skin.LegShade="#2d251c"; }
        apache.Palps="#29231f";
        foreach(var skin in new[]{red,redFemale,apache,apacheFemale}) { skin.HeadHair="#625a52";skin.LegFur="#625a52"; }
        portia.CoatDensity=.40;portia.HairLength=.48;portia.LegHair=.24;portia.FrontBulk=.70;portia.MiddleEye=1.2;
        portia.BodyWidth=8.5;portia.BodyLength=16;portia.BodyTaper=.24;
        portia.Description="Angular high front body, narrow mottled abdomen and very fine, irregularly brushed legs. Large side eyes and a pale female moustache. Hunts spiders and flies.";
        return new[]{regal,regalMale,boldFemale,bold,zebra,zebraMale,tan,tanMale,redFemale,red,emerald,emeraldMale,wall,wallMale,pantropicalFemale,pantropical,apacheFemale,apache,portia,hyllus};
        }
        public static SkinProfile Get(string id) { foreach(var skin in All) if(skin.Id==id) return skin;return All[0]; }
        public static string[] Names { get { var ids=new string[All.Length];for(int i=0;i<ids.Length;i++) ids[i]=All[i].Id;return ids; } }
        public static bool Contains(string id) { return Array.IndexOf(Names,id)>=0; }
    }
}
