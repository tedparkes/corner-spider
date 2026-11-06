using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace CornerSpider {
    public static partial class RuntimeTests {
        static void RenderCornerWebReview(string folder) {
            RenderCornerWebPage(folder,"orbs","ORB WEBS AND PORTIA'S PREY",new[] {
                "GardenOrb","CrossOrb","MarbledOrb","GoldenOrb","GardenOrbMale","Nebula","Crystal","guest","guest-building"
            });
            RenderCornerWebPage(folder,"sheets","COBWEBS, FUNNELS AND RESIDENT NESTS",new[] {
                "SouthernWidow","BrownWidow","RedbackWidow","HouseTangle","BarnFunnel","Copperbot",
                "SouthernWidowMale","BarnFunnelMale","residents"
            });
            RenderCornerWebPage(folder,"retreats","JUMPING SPIDER SILK RETREATS",new[] {
                "Regal","Bold","Zebra","Tan","RedBackFemale","Emerald","GrayWall","Pantropical","Apache",
                "PortiaLabiata","PortiaLabiataMale","HyllusDiardiGreen"
            });
            RenderCornerWebPage(folder,"light","WEBS ON A LIGHT DESKTOP",new[] {
                "CrossOrb","BrownWidow","BarnFunnel","Regal","PortiaLabiata","guest"
            },true);
        }

        static void RenderCornerWebPage(string folder,string name,string title,string[] skins,bool light=false) {
            const int cellWidth=400,cellHeight=390,panel=360,top=68;
            int height=top+((skins.Length+2)/3)*cellHeight+12;
            var plate=new DrawingVisual();
            using(var d=plate.RenderOpen()) {
                d.DrawRectangle(BPreview("#e6ebe1"),null,new Rect(0,0,1200,height));
                Text(d,title,20,14,25,Color.FromRgb(30,50,44));
                Text(d,"App-rendered silk. The marked sides are the desktop edges.",20,45,14,Color.FromRgb(44,65,57));
                for(int i=0;i<skins.Length;i++) {
                    string id=skins[i],label;
                    int corner=i%4;
                    d.PushTransform(new TranslateTransform(i%3*cellWidth+20,top+i/3*cellHeight));
                    var clip=new Rect(0,0,panel,panel);
                    d.DrawRectangle(BPreview(light?"#d3d8cc":"#283740"),null,clip);
                    d.PushClip(new RectangleGeometry(clip));
                    if(id.StartsWith("guest",StringComparison.Ordinal)) {
                        var w=new World(2400,1600,4) { AutoFeed=false,CursorPlay=false };
                        w.SetSpecies("PortiaLabiata");
                        if(!w.StartWebVisit()) throw new InvalidOperationException("Could not set up the prey web preview.");
                        bool building=id=="guest-building";
                        w.GuestProgress=building?(GuestWebPattern.SupportsCompleteAt+GuestWebPattern.SpiralStartsAt)*.5:1;
                        d.PushTransform(new TranslateTransform(corner%2==1?16:0,corner>=2?16:0));
                        DrawGuestAtCorner(d,w,corner,panel-16,false);
                        d.Pop();
                        label=building?"Prey web / frame and spokes":"Prey web / completed capture spiral";
                    } else if(id=="residents") {
                        var w=new World(900,700,42) { AutoFeed=false,CursorPlay=false,KeepBabies=true };
                        w.RestoreResidents(new[] {
                            new ColonyRecord { Species="Bold",Edge=0,Along=.27,Age=7200 },
                            new ColonyRecord { Species="CrossOrb",Edge=0,Along=.72,Age=7200 }
                        });
                        d.PushTransform(new ScaleTransform(.4,.4));
                        new PetCanvas().DrawColonyScene(d,w);
                        d.Pop();
                        label="Two residents with independent edge nests";
                    } else {
                        if(!SkinCatalog.Contains(id)) throw new ArgumentException("Unknown preview skin "+id);
                        var w=new World(panel,panel,42) { AutoFeed=false,CursorPlay=false };
                        w.Corner=Settings.Corners[corner];
                        w.SetSpecies(id);
                        w.SetScale(.75);
                        w.Web=1;
                        w.Life.SilkSeconds=64800;
                        var art=new PetCanvas();
                        art.DrawHammock(d,w,false,id);
                        art.DrawHammock(d,w,true,id);
                        label=SkinCatalog.Get(id).Name;
                    }
                    d.Pop();
                    var edge=new Pen(BPreview(light?"#767f70":"#768a88"),1);
                    double x=corner==1 || corner==3?panel:0,y=corner>=2?panel:0;
                    if(id!="residents") {
                        d.DrawLine(edge,new Point(x,0),new Point(x,panel));
                        d.DrawLine(edge,new Point(0,y),new Point(panel,y));
                    }
                    Text(d,label,0,panel+6,15,Color.FromRgb(30,50,44));
                    d.Pop();
                }
            }
            SavePng(plate,1200,height,Path.Combine(folder,name+".png"));
        }
    }
}
