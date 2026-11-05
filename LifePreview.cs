using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace CornerSpider {
    public static partial class RuntimeTests {
        public static void RenderLifePreview(string folder) {
            var foodArt=new DrawingVisual();using(var d=foodArt.RenderOpen()) {
                d.DrawRectangle(BPreview("#e7ebe0"),null,new Rect(0,0,1120,240));Text(d,"CHOOSE FOOD / ONE PREY AT A TIME",22,12,22,Color.FromRgb(30,50,44));
                int index=0;foreach(PreyKind food in Enum.GetValues(typeof(PreyKind))) { var w=new World(1120,240,42);w.Scale=2;w.Feed(food);w.Fly=new V(80+index*160,120);w.Clock=1;PetCanvas.DrawPrey(d,w);Text(d,PetHost.FoodName(food),24+index*160,193,14,Color.FromRgb(30,50,44));index++; }
            }SavePng(foodArt,1120,240,Path.Combine(folder,"foods.png"));
            string[] skins={"Regal","PortiaLabiata","GardenOrb","CrossOrb","MarbledOrb","GoldenOrb","SouthernWidow","BrownWidow","RedbackWidow","HouseTangle","BarnFunnel","Nebula","Crystal","Copperbot","BoldFemale"};
            var visual=new DrawingVisual();using(var d=visual.RenderOpen()) {
                d.DrawRectangle(BPreview("#e7ebe0"),null,new Rect(0,0,1500,1150));Text(d,"CORNER SPIDER / NEW NEIGHBORS",24,16,27,Color.FromRgb(30,50,44));
                for(int i=0;i<skins.Length;i++) {
                    double x=(i%5)*300,y=70+(i/5)*350;d.PushTransform(new TranslateTransform(x,y));
                    var w=new World(300,300,42);w.SetSpecies(skins[i]);w.Scale=1.8;w.Spider=new V(150,128);w.Angle=180;
                    PetCanvas.DrawSpider(d,w,skins[i]);Text(d,SkinCatalog.Get(skins[i]).Name,12,252,16,Color.FromRgb(30,50,44));Text(d,SkinCatalog.Get(skins[i]).Scientific,12,277,12,Color.FromRgb(30,50,44));Text(d,SkinCatalog.Get(skins[i]).Category,12,300,12,Color.FromRgb(60,80,72));d.Pop();
                }
            }SavePng(visual,1500,1150,Path.Combine(folder,"new-families.png"));
            string[] webs={"Regal","GardenOrb","BrownWidow","BarnFunnel","Nebula","Crystal"};visual=new DrawingVisual();
            using(var d=visual.RenderOpen()) {
                d.DrawRectangle(BPreview("#e7ebe0"),null,new Rect(0,0,1500,860));Text(d,"SILK HOMES / FRESH AND LIVED IN",24,15,25,Color.FromRgb(30,50,44));
                for(int i=0;i<webs.Length;i++) for(int age=0;age<2;age++) {
                    double x=(i%3)*500+(age*250),y=60+(i/3)*395;d.PushTransform(new TranslateTransform(x,y));d.DrawRectangle(BPreview("#28363d"),null,new Rect(0,0,248,330));
                    var w=new World(248,330,42);w.SetSpecies(webs[i]);w.Scale=.85;w.Corner="TopRight";w.UpdateHome();w.Spider=w.Home;w.Web=1;w.Life.SilkSeconds=age==0?0:720000;w.GoHome(false);for(int j=0;j<30;j++) w.Tick(.1,new V(-10000,-10000));
                    new PetCanvas().DrawNestScene(d,w,webs[i]);Text(d,(age==0?"Fresh / ":"Lived in / ")+SkinCatalog.Get(webs[i]).Name.Split('-')[0],8,338,12,Color.FromRgb(30,50,44));d.Pop();
                }
            }SavePng(visual,1500,860,Path.Combine(folder,"living-webs.png"));
            LifeStage[] stages={LifeStage.Courting,LifeStage.Mating,LifeStage.Gravid,LifeStage.Incubating,LifeStage.Brood,LifeStage.Dispersing,LifeStage.Molting,LifeStage.Recovering};
            visual=new DrawingVisual();using(var d=visual.RenderOpen()) {
                d.DrawRectangle(BPreview("#e7ebe0"),null,new Rect(0,0,1440,790));Text(d,"LIFE AROUND THE NEST",24,12,26,Color.FromRgb(30,50,44));
                for(int i=0;i<stages.Length;i++) { d.PushTransform(new TranslateTransform((i%4)*360,60+(i/4)*360));d.DrawRectangle(BPreview("#28363d"),null,new Rect(2,0,356,310));
                    var w=new World(360,310,42);w.SetSpecies("Regal");w.SetScale(.9);w.Spider=w.Home;w.Web=1;w.Life.Stage=stages[i];w.Life.StageAge=stages[i]==LifeStage.Gravid?1500:stages[i]==LifeStage.Brood?3500:stages[i]==LifeStage.Incubating?100:18;w.Life.Babies=6;w.Angle=w.NestAngle;
                    if(stages[i]==LifeStage.Recovering) { w.Life.GrowthAt=w.Life.Age; }
                    w.Tick(.1,new V(-10000,-10000));
                    new PetCanvas().DrawNestScene(d,w,"Regal");Text(d,stages[i].ToString(),12,320,16,Color.FromRgb(30,50,44));d.Pop(); }
            }SavePng(visual,1440,790,Path.Combine(folder,"life-cycle.png"));
        }
        static void CheckLivingArt() {
            var canvas=new PetCanvas();
            foreach(var skin in SkinCatalog.All) {
                var w=new World(360,360,42);w.SetSpecies(skin.Id);w.Web=1;
                byte[] fresh=new byte[360*360*4],old=new byte[fresh.Length],hidden=new byte[fresh.Length];
                SilkBitmap(canvas,w,skin.Id).CopyPixels(fresh,1440,0);int builds=canvas.LivingSilkBuilds;
                w.Life.SilkSeconds=720000;SilkBitmap(canvas,w,skin.Id).CopyPixels(old,1440,0);
                long growth=0;bool retained=true;for(int i=3;i<fresh.Length;i+=4) { growth+=old[i]-fresh[i];if(old[i]+1<fresh[i]) retained=false; }
                Check(retained && growth>1000,"long-term silk adds permanent visible repairs: "+skin.Id);
                Check(canvas.LivingSilkBuilds==builds+1,"aging replaces only the current retained silk: "+skin.Id);
                w.WebVisibility=0;SilkBitmap(canvas,w,skin.Id).CopyPixels(hidden,1440,0);long alpha=0;for(int i=3;i<hidden.Length;i+=4) alpha+=hidden[i];
                Check(alpha==0 && canvas.LivingSilkBuilds==builds+1,"web opacity hides aged silk without rebuilding: "+skin.Id);
                if(skin.Category=="Jumping spiders") continue;
                w=new World(360,360,42);w.SetSpecies(skin.Id);w.Spider=new V(180,180);w.Life.GrowthAt=0;
                foreach(Mood pose in new[]{Mood.Rest,Mood.Walk,Mood.Inspect,Mood.Groom}) foreach(double angle in new[]{0.0,45,90,135,180,225,270,315}) {
                    w.State=pose;w.StateTime=.7;w.Angle=angle;w.GaitBlend=pose==Mood.Walk?1:0;w.StepPhase=1.4;
                    var art=new DrawingVisual();using(var d=art.RenderOpen()) PetCanvas.DrawSpider(d,w,skin.Id);
                    var bitmap=new System.Windows.Media.Imaging.RenderTargetBitmap(360,360,96,96,PixelFormats.Pbgra32);bitmap.Render(art);
                    byte[] pixels=new byte[360*360*4];bitmap.CopyPixels(pixels,1440,0);int outside=0,painted=0;double limit=(SpiderPose.Radius+1)*w.BodyScale;
                    for(int y=0;y<360;y++) for(int x=0;x<360;x++) if(pixels[(y*360+x)*4+3]>12) { painted++;if((x-180)*(x-180)+(y-180)*(y-180)>limit*limit) outside++; }
                    Check(painted>300 && outside==0,"actual family artwork fits edge clearance after a molt: "+skin.Id+" / "+pose+" / "+angle);
                }
            }
        }
    }
}
