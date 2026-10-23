using System;
using System.Windows.Media;

namespace CornerSpider {
    // Neutral shade choices, without assigning colors to ethnicity or nationality.
    // The original artwork is the 80% anchor; old saves keep that appearance.
    internal sealed class FingerPalette {
        public readonly string Light,Base,Shade,Outline,Highlight,Blush,NailLight,NailShade,NailEdge,Crease;
        public static int Key(double tone) { return (int)Math.Round(Math.Max(0,Math.Min(1,World.Finite(tone)?tone:World.DefaultFingerTone))*100); }
        static Color ColorOf(string hex) { return (Color)ColorConverter.ConvertFromString(hex); }
        static string ShadeAt(double tone,string deep,string medium,string original,string pale) {
            Color a,b;double t;
            if(tone<=.45) { a=ColorOf(deep);b=ColorOf(medium);t=tone/.45; }
            else if(tone<=.8) { a=ColorOf(medium);b=ColorOf(original);t=(tone-.45)/.35; }
            else { a=ColorOf(original);b=ColorOf(pale);t=(tone-.8)/.2; }
            return Color.FromRgb((byte)Math.Round(a.R+(b.R-a.R)*t),(byte)Math.Round(a.G+(b.G-a.G)*t),(byte)Math.Round(a.B+(b.B-a.B)*t)).ToString();
        }
        public static string Alpha(string color,byte alpha) { Color c=ColorOf(color);c.A=alpha;return c.ToString(); }
        public FingerPalette(int key) {
            double t=Math.Max(0,Math.Min(100,key))/100.0;
            Light=ShadeAt(t,"#715045","#c08f6f","#ffe7d6","#fff5ef");
            Base=ShadeAt(t,"#36241f","#946141","#e8bba6","#f7ded6");
            Shade=ShadeAt(t,"#1f1613","#65412d","#bf8b7b","#dab4aa");
            Outline=ShadeAt(t,"#271a16","#5c3c2b","#aa7e70","#bd9691");
            Highlight=ShadeAt(t,"#ba947c","#e3b88d","#fff1de","#fffaf5");
            Blush=ShadeAt(t,"#57322a","#9c5942","#b66d64","#d59a96");
            NailLight=ShadeAt(t,"#ad897c","#d4af9a","#f6e8df","#fff2ec");
            NailShade=ShadeAt(t,"#71534a","#a87965","#d5aca0","#e3bdb6");
            NailEdge=ShadeAt(t,"#50372f","#8b5f48","#ae887e","#bd9690");
            Crease=ShadeAt(t,"#1d1311","#5a3929","#986356","#b78c84");
        }
    }
}
