using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CornerSpider {
    public partial class PetCanvas {
        // Canonical desktop corner is at (-160,-160), with the pet at (0,0).
        // A resident may only have one nearby edge. Keep its web local and
        // tie the supports and gumfeet to that edge instead of spanning a monitor.
        sealed class SheetCorner {
            readonly double edgeX,edgeY,scaleX,scaleY;
            readonly bool resident;
            internal SheetCorner(double ex,double ey,bool resident) {
                edgeX=ex;edgeY=ey;this.resident=resident;
                scaleX=Math.Min(1,ex/160);scaleY=Math.Min(1,ey/160);
            }
            internal V Point(V p) {
                return new V(p.X<0?p.X*scaleX:p.X,p.Y<0?p.Y*scaleY:p.Y);
            }
            internal V Anchor(bool side,double position) {
                if(resident && side!=(edgeX<=edgeY)) position=position*.78-16;
                if(resident) side=edgeX<=edgeY;
                return side?new V(-edgeX,Point(new V(0,position)).Y):new V(Point(new V(position,0)).X,-edgeY);
            }
            internal V AlongWall(bool side) {
                if(resident) side=edgeX<=edgeY;
                return side?new V(0,1):new V(1,0);
            }
        }
        static readonly string[] CornerSilk={
            "#6ee5e8df","#94edf0e6","#473d4944","#aaeff4e9","#589ba8a2"
        };
        static readonly string[] CopperSilk={
            "#73d6c39c","#94e5d5ad","#563f4742","#aef1e1b9","#5eaaa390"
        };
        static readonly Brush SilkVeil=SoftGlow("#14606a61");
        static readonly Brush SilkLustre=SoftGlow("#16f4f2e5");
        static void DrawCornerSheet(DrawingContext d,SkinProfile skin,double ex,double ey,double growth,int layers,bool resident) {
            var corner=new SheetCorner(ex,ey,resident);
            if(skin.WebKind==WebKind.Sheet) DrawFunnelSheet(d,corner,skin,growth,layers);
            else DrawGumfootWeb(d,corner,skin,growth,layers);
        }
        // Draw projected silk with a modest gravity sag. Endpoints are actual
        // junctions, not merely strokes crossing one another in screen space.
        static void SheetStrand(DrawingContext d,string ink,double width,V a,V b,double sag) {
            V delta=b-a;
            if(delta.Length<.15) return;
            Curve(d,ink,width,new Point(a.X,a.Y),
                new Point(a.X+delta.X/3,a.Y+delta.Y/3+sag),
                new Point(a.X+delta.X*2/3,a.Y+delta.Y*2/3+sag),
                new Point(b.X,b.Y));
        }
        static V SheetLerp(V a,V b,double t) { return a+(b-a)*t; }
        static double SheetJitter(Random r,double range) { return (2*r.NextDouble()-1)*range; }
        static void SheetAttach(DrawingContext d,SheetCorner corner,bool side,double at,V end,string[] silk,bool paired,bool seam=false) {
            V anchor=corner.Anchor(side,at);
            SheetStrand(d,silk[2],.85,anchor,end,1.1);
            SheetStrand(d,silk[0],.36,anchor,end,1.1);
            if(paired) SheetStrand(d,silk[4],.28,corner.Anchor(side,at+5),end,2);
            if(seam) {
                // The thread fans into a short adhesion seam along the surface.
                // Keep the extra silk at the contact instead of thickening the span.
                V tangent=corner.AlongWall(side);
                V first=anchor-tangent*(5.7+1.6*Math.Sin(at));
                V last=anchor+tangent*(5.8+1.3*Math.Cos(at));
                V joint=SheetLerp(anchor,end,.037);
                SheetStrand(d,silk[1],.6,first,last,0);
                SheetStrand(d,silk[4],.34,first,joint,.18);
                SheetStrand(d,silk[4],.32,last,joint,.24);
            }
        }
        static void DrawFunnelSheet(DrawingContext d,SheetCorner corner,SkinProfile skin,double growth,int layers) {
            bool copper=skin.Fictional;
            string[] silk=copper?CopperSilk:CornerSilk;
            var random=new Random(copper?24053:19051);
            V[] apron={
                new V(-149,35),new V(-114,55),new V(-89,75),new V(-55,87),
                new V(-26,103),new V(11,114),new V(46,101),new V(76,89),
                new V(98,59),new V(121,24),new V(116,-8),new V(106,-37),
                new V(98,-66),new V(75,-98),new V(52,-121),new V(38,-147)
            };
            const int rows=7;
            var points=new V[rows,apron.Length];
            V throat=new V(-6,-4);
            for(int row=0;row<rows;row++) {
                double t=(row+1.0)/rows;
                for(int col=0;col<apron.Length;col++) {
                    V mouth=throat+new V(-.65,.76)*(17-col*34.0/(apron.Length-1));
                    V p=SheetLerp(mouth,apron[col],t);
                    if(row!=rows-1) {
                        p.X+=SheetJitter(random,copper?3:11)*t;
                        p.Y+=SheetJitter(random,copper?3:10)*t;
                        p.Y+=11*Math.Sin(Math.PI*t)*Math.Sin(Math.PI*col/(apron.Length-1));
                    }
                    points[row,col]=corner.Point(p);
                }
            }
            for(int col=0;col<apron.Length;col++) {
                V end=points[rows-1,col];
                bool side=col<apron.Length/2;
                if(col<=2 || col>=apron.Length-3 || col==4 || col==12) {
                    double wall=side?end.Y+4+SheetJitter(random,8):end.X+4+SheetJitter(random,8);
                    SheetAttach(d,corner,side,wall,end,silk,col%4==0);
                }
                if(col>0 && col%3!=1) SheetStrand(d,silk[4],.32,points[rows-1,col-1],end,2.8+SheetJitter(random,1.1));
            }
            // The apron is a woven surface. Bias the small connections around
            // the funnel, where the two structures share a continuous floor.
            var lace=new List<V>();
            for(int row=0;row<rows;row++) for(int col=0;col<apron.Length;col++) lace.Add(points[row,col]);
            for(int i=0;i<(skin.Male?240:505);i++) {
                double column=random.NextDouble()*(apron.Length-1.001);
                int col=(int)column;
                double t=.02+.98*Math.Pow(random.NextDouble(),.77);
                V edge=SheetLerp(apron[col],apron[col+1],column-col);
                V start=throat+new V(-.65,.76)*(16-column*32/(apron.Length-1));
                V p=SheetLerp(start,edge,t);
                p.Y+=12*Math.Sin(Math.PI*t)*Math.Sin(Math.PI*column/(apron.Length-1));
                lace.Add(corner.Point(p));
            }
            for(int col=0;col<apron.Length;col+=2) {
                for(int row=1;row<rows;row+=2) {
                    V p=points[row,col];
                    Oval(d,SilkVeil,p.X,p.Y,23+row,18+row);
                    Oval(d,SilkLustre,p.X-2,p.Y-2,27+row,20+row);
                }
            }
            DrawSilkLace(d,lace,silk,random,skin.Male?17:15);
            // The raised trip threads and fixed sleeve bridge into the corners
            // behind the entrance, leaving a dark, open throat for the spider.
            for(int i=0;i<8;i++) {
                bool side=i%2==0;
                int col=side?2+i/2:apron.Length-3-i/2;
                V end=points[3+i%3,col];
                SheetStrand(d,silk[4],.32,corner.Anchor(side,-106+i*13+SheetJitter(random,5)),end,-7+SheetJitter(random,3));
            }
            DrawSilkSleeve(d,corner,throat,new V(-83,-62),skin.Male?16:20,skin.Male?7:9,silk);
            for(int i=0;i<22;i++) {
                double offset=SheetJitter(random,15);
                V rim=corner.Point(throat+new V(-.65,.76)*offset+new V(3,3));
                V floor=points[1+i%2,i*7%apron.Length];
                SheetStrand(d,silk[0],.27,rim,floor,1.9);
            }
            AddSheetRepairs(d,points,silk,growth,layers,31389);
        }
        static void DrawSilkLace(DrawingContext d,List<V> nodes,string[] silk,Random random,double reach) {
            const int links=4;
            var nearest=new int[links];
            var distances=new double[links];
            var strands=new HashSet<int>();
            for(int i=0;i<nodes.Count;i++) {
                V a=nodes[i];
                double axis=SheetJitter(random,1.2),cos=Math.Cos(axis),sin=Math.Sin(axis);
                for(int k=0;k<links;k++) { nearest[k]=-1;distances[k]=reach*reach; }
                for(int j=0;j<nodes.Count;j++) {
                    if(i==j) continue;
                    V v=nodes[j]-a;
                    double distance=v.X*v.X+v.Y*v.Y;
                    if(distance<4 || distance>=reach*reach) continue;
                    int k=(v.X*cos+v.Y*sin>0?1:0)+(v.Y*cos-v.X*sin>0?2:0);
                    if(distance<distances[k]) { nearest[k]=j;distances[k]=distance; }
                }
                for(int k=0;k<links;k++) {
                    int j=nearest[k];
                    if(j<0 || !strands.Add(Math.Min(i,j)*nodes.Count+Math.Max(i,j))) continue;
                    double sag=SheetJitter(random,1.1);
                    SheetStrand(d,i%5==0?silk[0]:silk[4],.24+random.NextDouble()*.11,a,nodes[j],sag);
                    if(i%3==0) SheetStrand(d,silk[2],.37,a,nodes[j],sag+.5);
                }
            }
        }
        static void DrawSilkSleeve(DrawingContext d,SheetCorner corner,V mouth,V rear,double radius,double height,string[] silk) {
            var random=new Random(617);
            var rings=new V[7,24];
            V across=new V(-.67,.74), rise=new V(-.30,-.75);
            for(int row=0;row<7;row++) {
                double t=row/6.0;
                V center=SheetLerp(rear,mouth,t)+new V(-5,6)*Math.Sin(Math.PI*t);
                V veil=corner.Point(center);
                Oval(d,SilkVeil,veil.X,veil.Y,14+7*t,10+8*t);
                Oval(d,SilkLustre,veil.X-2,veil.Y-2,15+9*t,10+7*t);
                for(int col=0;col<24;col++) {
                    double a=col*Math.PI/12;
                    double r=(.52+.48*t*t)*(1+SheetJitter(random,.15));
                    rings[row,col]=corner.Point(center+across*(radius*r*Math.Cos(a))+rise*(height*r*Math.Sin(a)));
                }
            }
            var opening=new StreamGeometry();
            using(var path=opening.Open()) {
                path.BeginFigure(new Point(rings[6,0].X,rings[6,0].Y),true,true);
                for(int col=1;col<24;col++) path.LineTo(new Point(rings[6,col].X,rings[6,col].Y),true,false);
            }
            opening.Freeze();
            for(int row=0;row<7;row++) for(int col=0;col<24;col++) {
                V p=rings[row,col];
                if(row>0 && random.NextDouble()<.76)
                    SheetStrand(d,silk[(row+col)%5==0?1:4],.24+random.NextDouble()*.17,rings[row-1,(col+25+(int)SheetJitter(random,3))%24],p,SheetJitter(random,1.4));
                if(row<6 && random.NextDouble()<.74) SheetStrand(d,silk[0],.29,rings[row,(col+1)%24],p,.2);
            }
            d.DrawGeometry(B("#59273632"),null,opening);
            for(int col=0;col<24;col++) {
                string ink=col<12?silk[1]:silk[0];
                SheetStrand(d,ink,col<12?.46:.31,rings[6,col],rings[6,(col+1)%24],.15);
            }
            for(int i=0;i<4;i++) {
                bool side=i%2==0;
                SheetStrand(d,silk[4],.30,corner.Anchor(side,-116+i*17),rings[1+i%3,i*5],1.9);
            }
        }
        static void DrawGumfootWeb(DrawingContext d,SheetCorner corner,SkinProfile skin,double growth,int layers) {
            bool house=skin.Pattern=="House", brown=skin.Pattern=="BrownWidow", redback=skin.Pattern=="Redback";
            int seed=house?86309:brown?74611:redback?31963:49157;
            var random=new Random(seed);
            string[] silk=CornerSilk;
            V center=house?new V(-10,3):brown?new V(10,36):new V(18,40);
            double spanX=house?115:brown?126:redback?140:130;
            double spanY=house?72:brown?54:redback?39:47;
            double span=skin.Male?.79:1;
            var rim=new V[12];
            for(int i=0;i<rim.Length;i++) {
                double a=2*Math.PI*i/rim.Length;
                double slack=.85+SheetJitter(random,.13);
                rim[i]=center+new V(Math.Cos(a)*spanX*slack,Math.Sin(a)*spanY*slack+Math.Cos(a)*14)*span;
            }
            var sheet=new List<V>();
            for(int i=0;i<rim.Length;i++) sheet.Add(corner.Point(rim[i]));
            for(int i=0;i<(skin.Male?177:412);i++) {
                double section=random.NextDouble()*rim.Length;
                int edge=(int)section;
                V limit=SheetLerp(rim[edge],rim[(edge+1)%rim.Length],section-edge);
                double t=Math.Pow(random.NextDouble(),.67);
                V p=SheetLerp(center,limit,t);
                p.Y+=11*(1-t)*Math.Cos(p.X/43);
                sheet.Add(corner.Point(p));
            }
            // Upper load-bearing strands are drawn behind the catch sheet.
            // Scattered three-way junctions make a tall cage over its shallow
            // floor, with the lower glue feet separated by open space.
            V[] upper={
                corner.Point(new V(-113,-87)),corner.Point(new V(-77,-97)),
                corner.Point(new V(-40,-77)),corner.Point(new V(10,-90)),
                corner.Point(new V(48,-75)),corner.Point(new V(76,-50)),
                corner.Point(new V(-93,-51)),corner.Point(new V(-63,-42)),
                corner.Point(new V(-25,-45)),corner.Point(new V(24,-43))
            };
            for(int i=0;i<6;i++) {
                bool side=i<2;
                double at=side?-98+i*53:-78+(i-2)*47;
                SheetAttach(d,corner,side,at,upper[i],silk,i%3==0,true);
                if(i>0) SheetStrand(d,silk[4],.35,upper[i-1],upper[i],4+SheetJitter(random,3));
                if(i<4) SheetStrand(d,silk[0],.31,upper[i],upper[6+i],2.3);
            }
            for(int i=0;i<upper.Length;i++) {
                int index=(i+5)%rim.Length;
                V floor=corner.Point(rim[index]);
                V mid=SheetLerp(upper[i],floor,.39+random.NextDouble()*.24);
                mid.X+=SheetJitter(random,12);
                mid.Y+=SheetJitter(random,6);
                SheetStrand(d,silk[0],.34,upper[i],mid,2);
                SheetStrand(d,silk[4],.30,mid,floor,3);
                SheetStrand(d,silk[4],.29,mid,upper[(i+3)%upper.Length],4.2);
            }
            for(int i=0;i<rim.Length;i++) {
                V p=corner.Point(rim[i]);
                V next=corner.Point(rim[(i+1)%rim.Length]);
                if(i%3!=1) SheetStrand(d,silk[4],.25,p,SheetLerp(next,corner.Point(center),.13),1.6+SheetJitter(random,2));
                V veil=SheetLerp(corner.Point(center),SheetLerp(p,next,.45),.55);
                Oval(d,SilkVeil,veil.X,veil.Y,41,26);
                Oval(d,SilkLustre,veil.X-2,veil.Y-1,42,25);
            }
            V middle=corner.Point(center);
            Oval(d,SilkVeil,middle.X,middle.Y,spanX*.68,spanY*.79);
            Oval(d,SilkLustre,middle.X-8,middle.Y-5,spanX*.70,spanY*.76);
            SheetAttach(d,corner,true,11,corner.Point(rim[6]),silk,true,true);
            SheetAttach(d,corner,true,70,corner.Point(rim[5]),silk,false,true);
            SheetAttach(d,corner,false,103,corner.Point(rim[11]),silk,false,true);
            DrawSilkLace(d,sheet,silk,random,skin.Male?17:15);
            for(int i=0;i<(skin.Male?23:46);i++) {
                V p=sheet[random.Next(sheet.Count)],q=sheet[random.Next(sheet.Count)];
                double separation=(p-q).Length;
                if(separation>17 && separation<57) SheetStrand(d,silk[4],.28,p,q,SheetJitter(random,3.2));
            }
            int feet=skin.Male?0:house?8:brown?10:redback?7:12;
            for(int i=0;i<feet;i++) {
                double t=i/(double)(feet-1), rimPosition=4.15+t*2.8;
                int r=(int)rimPosition;
                V from=corner.Point(SheetLerp(rim[r],rim[r+1],rimPosition-r));
                V to=corner.Anchor(true,146-t*97+SheetJitter(random,9));
                V foot=SheetLerp(from,to,.90);
                SheetStrand(d,silk[2],.42,from,foot,0);
                SheetStrand(d,silk[1],.38,from,foot,0);
                SheetStrand(d,silk[3],.55,foot,to,0);
                V droplet=SheetLerp(foot,to,.73);
                Oval(d,B(silk[4]),droplet.X,droplet.Y,.75,.55);
            }
            // Theridiids use a dense shelter in the scaffolding, with a low
            // opening rather than the long circular tunnel of a funnel web.
            var shelter=new List<V>();
            int count=skin.Male?46:house?185:145;
            double rx=house?25:brown?31:redback?30:23,ry=house?18:brown?17:15;
            for(int i=0;i<count;i++) {
                double a=random.NextDouble()*2*Math.PI;
                double r=Math.Sqrt(random.NextDouble()),x=Math.Cos(a)*rx*r,y=Math.Sin(a)*ry*r;
                V point=new V(x+y*.38-13,y+x*.48-15);
                if(point.Length<10) continue;
                shelter.Add(corner.Point(point));
            }
            V shelterShade=corner.Point(new V(-15,-17));
            Oval(d,SilkVeil,shelterShade.X,shelterShade.Y,rx,ry);
            DrawSilkLace(d,shelter,silk,random,11);
            for(int i=0;i<upper.Length;i++) {
                V edge=shelter[(i*17)%shelter.Count];
                SheetStrand(d,silk[4],.30,upper[i],edge,2.7);
                if(i%2==0) SheetStrand(d,silk[0],.35,edge,sheet[i*3],2.5);
            }
            int repairs=(int)(growth*45)+layers*5;
            for(int i=0;i<repairs;i++) {
                V p=sheet[random.Next(sheet.Count)],q=sheet[random.Next(sheet.Count)];
                if((p-q).Length<46) SheetStrand(d,silk[4],.31,p,q,2.1);
            }
        }
        static void AddSheetRepairs(DrawingContext d,V[,] points,string[] silk,double growth,int layers,int seed) {
            // Seed and point positions do not depend on the age. A grown web
            // draws the same stitches plus a longer prefix of local repair ties.
            var random=new Random(seed);
            int rows=points.GetLength(0),columns=points.GetLength(1);
            int repairs=(int)(growth*52)+layers*4;
            for(int i=0;i<repairs;i++) {
                int row=random.Next(1,rows),col=random.Next(1,columns);
                double t=.2+random.NextDouble()*.6;
                V a=SheetLerp(points[row-1,col-1],points[row,col-1],t);
                V b=SheetLerp(points[row-1,col],points[row,col],.2+random.NextDouble()*.6);
                SheetStrand(d,silk[4],.29,a,b,SheetJitter(random,1.4));
            }
        }
    }
}
