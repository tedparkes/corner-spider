using System;

namespace CornerSpider {
    public enum RetreatStyle { Oval,Tube,Crevice,Bark,LongTube,Leaf,Cell,Tangle,Taper,Platform,Pocket }
    // Species-inspired illustration, not diagnostic species keys. Both open
    // ends remain centered on the existing entry/exit axis and inside its clearance.
    public sealed class SilkProfile {
        public readonly string Name,Note;
        public readonly double Width,Length,Density,Weave;
        public readonly int Seed;
        public readonly bool Scaffold;
        public readonly RetreatStyle Style;
        SilkProfile(string name,string note,double width,double length,double density,double weave,int seed,RetreatStyle style) {
            Name=name;Note=note;Width=width;Length=length;Density=density;Weave=weave;Seed=seed;Style=style;Scaffold=style==RetreatStyle.Platform;
        }
        public double Edge(double t,int side) {
            t=Math.Max(0,Math.Min(1,t));double s=Math.Sin(Math.PI*t),width,center=0;
            switch(Style) {
                case RetreatStyle.Tube:width=20+12*Math.Pow(s,.35);break;
                case RetreatStyle.Crevice:width=12+11*Math.Pow(s,1.7);break;
                case RetreatStyle.Bark:width=10+10*Math.Pow(s,.45);center=3*Math.Sin(2*Math.PI*t);break;
                case RetreatStyle.LongTube:width=16+8*Math.Pow(s,.25);break;
                case RetreatStyle.Leaf:width=15+24*Math.Pow(s,.7);center=-5*s;break;
                case RetreatStyle.Cell:width=11+16*Math.Min(1,3*s);center=2*s;break;
                case RetreatStyle.Tangle:width=14+26*Math.Pow(s,.8)+s*Math.Sin(t*8*Math.PI);center=4*s*Math.Sin(2*Math.PI*t);break;
                case RetreatStyle.Taper:width=12+30*s*(1-.65*t);center=3*s;break;
                case RetreatStyle.Platform:width=18+29*Math.Pow(s,.55);center=-7*s;break;
                case RetreatStyle.Pocket:width=20+22*Math.Pow(s,.65)*( .65+.5*t);break;
                default:width=16+15*Math.Pow(s,.75);break;
            }
            return center+side*width;
        }
        public double Along(double t) { return -43+93*t; }
        static readonly SilkProfile regal=new SilkProfile("Elliptical retreat","A soft ellipsoid with open end rims and a fine lengthwise veil, inspired by Regal retreats.",1,1.03,1,.5,3791,RetreatStyle.Oval);
        static readonly SilkProfile bold=new SilkProfile("Layered silk tube","A broad, double-walled tube with rolled end cuffs; the layering is an artistic interpretation.",1,1.05,1.1,.6,5827,RetreatStyle.Tube);
        static readonly SilkProfile zebra=new SilkProfile("Small crevice sleeve","A short, pinched crevice sleeve with a light cover; exact proportions are illustrative.",.86,.80,.8,.3,6911,RetreatStyle.Crevice);
        static readonly SilkProfile tan=new SilkProfile("Low bark sleeve","A long, flattened slit with lengthwise seams, inspired by bark and crevice use.",1,1.26,.67,.28,7393,RetreatStyle.Bark);
        static readonly SilkProfile red=new SilkProfile("Tubular retreat","A long, narrow tube with layered walls, inspired by documented P. johnsoni nests.",1,1.28,1.1,.44,8117,RetreatStyle.LongTube);
        static readonly SilkProfile redMale=new SilkProfile("Light tubular retreat","The same long tube with lighter silk, reflecting observations of less dense male P. johnsoni nests.",1,1.28,.70,.44,8117,RetreatStyle.LongTube);
        static readonly SilkProfile emerald=new SilkProfile("Layered leaf shelter","A broad folded envelope with an overlapping silk seam, inspired by leaf-side shelters; no eggs are simulated.",1,1.03,1.12,.65,9127,RetreatStyle.Leaf);
        static readonly SilkProfile wall=new SilkProfile("Flat crevice cell","A short, flat-sided cell with a low cover; the outline is a habitat-inspired illustration.",1,.87,.77,.32,10103,RetreatStyle.Cell);
        static readonly SilkProfile pantropical=new SilkProfile("Tangled tube shelter","A rounded tube enclosed in an irregular silk mesh, inspired by complex P. paykulli nests.",1,.96,1.22,1.35,11213,RetreatStyle.Tangle);
        static readonly SilkProfile apache=new SilkProfile("Plant-side tube","A tapered sleeve with a narrow end and converging seams; this is an artistic variation.",1,1.18,.96,.58,12049,RetreatStyle.Taper);
        static readonly SilkProfile portia=new SilkProfile("Platform and scaffold","An open, asymmetric silk platform with a funnel-like corner scaffold, inspired by Portia webs.",1,1.05,.55,1.15,13109,RetreatStyle.Platform);
        static readonly SilkProfile hyllus=new SilkProfile("Broad silk pocket","A roomy, rounded pouch with a reinforced rim, fitted to the larger body; exact architecture is illustrative.",1,1.18,1.05,.7,14207,RetreatStyle.Pocket);
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
