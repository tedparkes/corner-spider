using System;

namespace CornerSpider {
    // Modest visual adaptations of retreat structure, not diagnostic species keys.
    // Width/length change the silk only; both openings retain the existing axis.
    public sealed class SilkProfile {
        public readonly string Name,Note;
        public readonly double Width,Length,Density,Weave;
        public readonly int Seed;
        public readonly bool Scaffold;
        SilkProfile(string name,string note,double width,double length,double density,double weave,int seed,bool scaffold=false) {
            Name=name;Note=note;Width=width;Length=length;Density=density;Weave=weave;Seed=seed;Scaffold=scaffold;
        }
        static readonly SilkProfile regal=new SilkProfile("Elliptical retreat","An elliptical sleeve with two openings, inspired by Regal silk retreats.",1,1.03,1,.5,3791);
        static readonly SilkProfile bold=new SilkProfile("Layered silk tube","A substantial tubular shelter; the weave is a species-inspired illustration.",1.02,1,1,.6,5827);
        static readonly SilkProfile zebra=new SilkProfile("Small crevice sleeve","A compact silk sleeve scaled to this small jumper; exact weave is illustrative.",.76,.88,.8,.3,6911);
        static readonly SilkProfile tan=new SilkProfile("Low bark sleeve","A flatter, lighter shelter inspired by bark and crevice use.",.91,1.08,.67,.28,7393);
        static readonly SilkProfile red=new SilkProfile("Tubular retreat","A lengthwise tube with layered silk, inspired by documented P. johnsoni nests.",.96,1.1,1.1,.44,8117);
        static readonly SilkProfile redMale=new SilkProfile("Light tubular retreat","A lighter tube, reflecting observations of less dense male P. johnsoni nests.",.92,1.06,.78,.44,8117);
        static readonly SilkProfile emerald=new SilkProfile("Layered leaf shelter","A layered sleeve inspired by leaf-side retreats; no egg chamber is simulated.",1.04,1.06,1.12,.65,9127);
        static readonly SilkProfile wall=new SilkProfile("Flat crevice cell","A low, narrow cell suited to the illustrated wall-dwelling form.",.89,1.08,.77,.32,10103);
        static readonly SilkProfile pantropical=new SilkProfile("Tangled tube shelter","A tube wrapped in a denser irregular silk mesh, inspired by P. paykulli nests.",1.02,1.08,1.22,1.35,11213);
        static readonly SilkProfile apache=new SilkProfile("Plant-side tube","A gently tapered sleeve; its exact silk pattern is an artistic variation.",.98,1.05,.96,.58,12049);
        static readonly SilkProfile portia=new SilkProfile("Platform and scaffold","An open platform with an irregular funnel-like scaffold, inspired by Portia webs.",.91,1.09,.62,1.15,13109,true);
        static readonly SilkProfile hyllus=new SilkProfile("Broad silk pocket","A broader shelter fitted to the large body; species-specific silk details are unconfirmed.",1.13,1.17,1.05,.7,14207);
        public static SilkProfile Get(string species) {
            switch(species) {
                case "Bold":case "BoldFemale":return bold;
                case "Zebra":case "ZebraMale":return zebra;
                case "Tan":case "TanMale":return tan;
                case "RedBack":return redMale;
                case "RedBackFemale":return red;
                case "Emerald":case "EmeraldMale":return emerald;
                case "GrayWall":case "GrayWallMale":return wall;
                case "Pantropical":case "PantropicalFemale":return pantropical;
                case "Apache":case "ApacheFemale":return apache;
                case "PortiaLabiata":return portia;
                case "HyllusDiardiGreen":return hyllus;
                default:return regal;
            }
        }
    }
}
