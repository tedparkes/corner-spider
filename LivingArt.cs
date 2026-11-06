using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CornerSpider {
    public partial class PetCanvas {
        static readonly Dictionary<string,DrawingGroup> familyBodies=new Dictionary<string,DrawingGroup>();
        readonly World visitorArt=new World(1200,900,42) { AutoFeed=false,CursorPlay=false };
        DrawingGroup livingSilk;
        SilkImage livingSilkImage;
        double livingSilkScale=-1;
        string livingSilkKey;
        internal int LivingSilkBuilds;
        static DrawingGroup FamilyBody(SkinProfile skin) {
            DrawingGroup art;if(familyBodies.TryGetValue(skin.Id,out art)) return art;
            art=new DrawingGroup();using(var d=art.Open()) {
                double rx=skin.BodyWidth,ry=skin.BodyLength;
                Oval(d,Glow(skin.Body,skin.BodyShade),0,16,rx,ry);
                d.PushClip(new EllipseGeometry(new Point(0,16),rx,ry));
                switch(skin.Pattern) {
                    case "Garden":for(int i=0;i<5;i++) { double y=3+i*7;Curve(d,"#303027",4,new Point(-rx,y),new Point(-rx*.4,y+2),new Point(rx*.4,y-2),new Point(rx,y)); }break;
                    case "Cross":for(int i=0;i<5;i++) Oval(d,B(skin.Mark),0,3+i*4,1.6,2);for(int i=-2;i<=2;i++) Oval(d,B(skin.Mark),i*4,10,1.8,1.5);break;
                    case "Marble":for(int i=0;i<14;i++) { double y=-3+i*3;Curve(d,"#714332",1.1,new Point(-rx,y),new Point(-rx*.3+(i%3)*3,y-5),new Point(rx*.4,y+6),new Point(rx,y-2)); }break;
                    case "Golden":for(int i=0;i<8;i++) { double y=-3+i*6;Oval(d,B("#eae6bc"),-rx*.45,y,2,2.4);Oval(d,B("#eae6bc"),rx*.45,y,2,2.4); }break;
                    case "Redback":Curve(d,"#d44934",4,new Point(0,0),new Point(-2,10),new Point(2,23),new Point(0,33));break;
                    case "BrownWidow":case "House":case "Funnel":for(int i=0;i<5;i++) { double y=3+i*6;Curve(d,"#574637",1.7,new Point(-rx*.7,y),new Point(-4,y+5),new Point(4,y+5),new Point(rx*.7,y)); }break;
                    case "Nebula":for(int i=0;i<18;i++) Oval(d,B(i%3==0?"#f9d88e":"#a4d9f5"),Math.Sin(i*2.4)*rx*.8,16+Math.Cos(i*1.7)*ry*.8,.6+i%3*.3,.7);break;
                    case "Crystal":for(int i=-2;i<=2;i++) { Line(d,"#d3fff2",.8,i*5,16-ry,i*3,16+ry);Line(d,"#7a8ce1",.7,-rx,16+i*5,rx,16-i*5); }break;
                    case "Mech":for(int i=0;i<5;i++) Line(d,"#edb778",1.1,-rx,3+i*6,rx,3+i*6);Oval(d,B("#67dce0"),0,15,4,7);break;
                }
                if(skin.Male && skin.Category=="Widows") for(int i=0;i<4;i++) { Oval(d,B("#dfc292"),-rx*.7,4+i*5,1.3,2);Oval(d,B("#dfc292"),rx*.7,4+i*5,1.3,2); }
                d.Pop();
                if(skin.Category!="Widows") {
                    var random=new Random(616);for(int i=0;i<90;i++) { double a=random.NextDouble()*Math.PI*2,x=Math.Cos(a)*rx,y=16+Math.Sin(a)*ry;Line(d,skin.Hair,.18,x,y,x+Math.Cos(a)*1.1,y+Math.Sin(a)*1.1); }
                }
            }art.Freeze();familyBodies[skin.Id]=art;return art;
        }
        static void DrawFamilySpider(DrawingContext d,World w,SkinProfile skin) {
            d.PushTransform(new TranslateTransform(w.Spider.X,w.Spider.Y-w.Lift));d.PushTransform(new ScaleTransform(w.BodyScale,w.BodyScale));d.PushTransform(new RotateTransform(w.Angle));
            for(int side=-1;side<=1;side+=2) for(int i=3;i>=0;i--) {
                double phase=Math.Sin(w.StepPhase+(i%2)*Math.PI+(side<0?0:Math.PI))*w.GaitBlend*4;
                double fold=w.NestTuck*.15,reach=i==0?w.FrontReach(side):0;
                V a=new V(side*5,-15+i*5),b=new V(side*(i==0?19:26)*(1-fold),-42+i*21),c=new V(side*(i==0?34:50)*(1-fold),-64+i*40+phase);
                if(i==0) { b.X*=1-reach*.45;c.X*=1-reach*.60;b.Y-=reach*6;c.Y-=reach*8; }
                c.Y=Math.Max(-66,Math.Min(66,c.Y));
                if(c.Length>67) c=c*(67/c.Length);
                if(i==0 && w.Hanging) { V grip=(w.FingerTip(w.FingerOrigin)-w.Spider)*(1/w.BodyScale);double a2=-w.Angle*Math.PI/180;c=new V(grip.X*Math.Cos(a2)-grip.Y*Math.Sin(a2)+side*2,grip.X*Math.Sin(a2)+grip.Y*Math.Cos(a2));b=(a+c)*.5+new V(side*9,0); }
                double width=skin.Category=="Widows"?1.5:skin.Pattern=="Mech"?3:2.2;
                Line(d,skin.LegShade,width+1,a.X,a.Y,b.X,b.Y);Line(d,skin.Legs,width,a.X,a.Y,b.X,b.Y);
                Line(d,skin.LegShade,width,b.X,b.Y,c.X,c.Y);Line(d,skin.Legs,Math.Max(.6,width-.6),b.X,b.Y,c.X,c.Y);
                if(skin.Category!="Widows" || skin.Pattern=="BrownWidow" || skin.Male) for(int band=1;band<=3;band++) { V p=b+(c-b)*(band/4.0),q=b+(c-b)*(band/4.0+.05);Line(d,skin.Mark,width*.7,p.X,p.Y,q.X,q.Y); }
            }
            d.PushTransform(new ScaleTransform(1+w.GravidAmount*.18,1+w.GravidAmount*.12,0,16));DrawArt(d,FamilyBody(skin));d.Pop();
            Oval(d,Glow(skin.Head,skin.HeadShade),0,-9,9*skin.HeadWidth/.65,11*skin.HeadLength/.72);
            for(int side=-1;side<=1;side+=2) {
                for(int i=0;i<4;i++) Eye(d,side*(1.5+i%2*2.5),-16+i/2*2.4,skin.Fictional?1.3:.8);
                double groom=w.State==Mood.Groom?Math.Sin(w.StateTime*5+side)*3:0;
                Line(d,skin.Legs,1.4,side*4,-17,side*(6+groom),-24);Oval(d,B(skin.Palps),side*(6+groom),-24,skin.Male?2:1,1.5);
            }
            DrawMeal(d,w);d.Pop();d.Pop();d.Pop();
        }
        void DrawLivingSilk(DrawingContext d,World w,string species) {
            var skin=SkinCatalog.Get(species);var area=w.Areas[w.HomeArea];double scale=w.SilkScale;
            double threadWidth=Math.Min(1,w.HomeScale/scale);
            double ex=Math.Abs((w.Corner.EndsWith("Right")?area.X+area.W:area.X)-w.Home.X)/scale;
            double ey=Math.Abs((w.Corner.StartsWith("Top")?area.Y:area.Y+area.H)-w.Home.Y)/scale;
            int layers=w.WebLayers,baseLevel=(int)(w.Web*30);
            string key=species+":"+layers+":"+baseLevel+":"+ex.ToString("F1")+":"+ey.ToString("F1")+":"+threadWidth.ToString("F3")+":"+w.IsColonyResident;
            if(key!=livingSilkKey) {
                LivingSilkBuilds++;
                livingSilkKey=key;livingSilk=new DrawingGroup();using(var c=livingSilk.Open()) {
                    double growth=.35+.65*w.Web;
                    if(skin.WebKind==WebKind.Orb || skin.WebKind==WebKind.Crystal)
                        DrawCornerOrb(c,skin,ex,ey,growth,layers,w.IsColonyResident,threadWidth);
                    else if(skin.WebKind!=WebKind.Retreat)
                        DrawCornerSheet(c,skin,ex,ey,growth,layers,w.IsColonyResident);
                }livingSilk.Freeze();livingSilkImage=new SilkImage();
            }
            if(livingSilkImage.Drawing==null || livingSilkScale!=scale) { livingSilkScale=scale;livingSilkImage=CacheSilk(livingSilk,scale); }
            d.PushOpacity(w.WebVisibility);d.PushTransform(new TranslateTransform(w.Home.X,w.Home.Y));d.PushTransform(new ScaleTransform((w.Corner.EndsWith("Right")?-1:1)*scale,(w.Corner.StartsWith("Top")?1:-1)*scale));
            d.DrawDrawing(livingSilkImage.Drawing);d.Pop();d.Pop();d.Pop();
        }
        void DrawLifeScene(DrawingContext d,World w) {
            var l=w.Life;
            if(w.MaleVisible) {
                string mate=SkinCatalog.Get(w.Species).MateId;
                if(mate.Length>0) { visitorArt.SetSpecies(mate);visitorArt.Scale=w.HomeScale*(SkinCatalog.Get(w.Species).Category=="Widows"?.48:.72);visitorArt.Spider=w.MatePosition;V face=w.Home-w.MatePosition;visitorArt.Angle=Math.Atan2(face.Y,face.X)*180/Math.PI+90;visitorArt.Clock=w.Clock;
                    visitorArt.State=l.Stage==LifeStage.Courting?Mood.Inspect:l.Stage==LifeStage.Mating?Mood.Groom:Mood.Rest;visitorArt.StateTime=l.StageAge%1.65;visitorArt.StepPhase=l.StageAge*5;visitorArt.GaitBlend=l.Stage==LifeStage.Courting || l.Stage==LifeStage.MaleLeaving?.6:0;
                    if(l.Stage==LifeStage.MaleMeal) { visitorArt.Spider+=new V(Math.Sin(l.StageAge*25)*2,Math.Cos(l.StageAge*22));visitorArt.Angle+=90;visitorArt.GaitBlend=.35*Math.Max(0,1-l.StageAge/3);visitorArt.StepPhase=l.StageAge*25; }
                    d.PushOpacity(l.Stage==LifeStage.MaleMeal?Math.Max(0,1-l.StageAge/6):1);DrawSpider(d,visitorArt,mate);d.Pop(); }
            }
            if(w.HasSac) {
                V p=w.Home+w.NestRoomDirection*48*w.HomeScale;double s=w.HomeScale;
                double grow=l.Stage==LifeStage.Laying?Math.Min(1,l.StageAge/30):1;
                d.PushOpacity(grow);Oval(d,Glow("#ece6d2","#a69f8b"),p.X,p.Y,10*s,13*s);
                if(SkinCatalog.Get(w.Species).Pattern=="BrownWidow") for(int i=0;i<13;i++) { double a=i*Math.PI*2/13;Line(d,"#ddd4bc",1.5*s,p.X+Math.Cos(a)*9*s,p.Y+Math.Sin(a)*12*s,p.X+Math.Cos(a)*13*s,p.Y+Math.Sin(a)*16*s); }
                for(int i=0;i<8;i++) Curve(d,"#8cfbf4da",.35*s,new Point(p.X-8*s,p.Y+(i-4)*2*s),new Point(p.X,p.Y-10*s),new Point(p.X+7*s,p.Y+8*s),new Point(p.X+8*s,p.Y+(i-4)*2*s));d.Pop();
            }
            if(l.Stage==LifeStage.Brood || l.Stage==LifeStage.Dispersing) for(int i=0;i<l.Babies;i++) {
                visitorArt.SetSpecies(w.Species);visitorArt.Scale=w.HomeScale*w.BabySize;visitorArt.Spider=w.BabyPosition(i);visitorArt.Angle=i*61+w.Clock*2;visitorArt.State=Mood.Rest;visitorArt.GaitBlend=.25;visitorArt.StepPhase=w.Clock*3+i;
                if(l.Stage==LifeStage.Dispersing) { d.PushOpacity(w.WebVisibility*.5);SilkLine(d,.3*w.HomeScale,w.Home,visitorArt.Spider);d.Pop(); }
                DrawSpider(d,visitorArt,w.Species);
            }
            if(l.Stage==LifeStage.Molting || l.Stage==LifeStage.Recovering) {
                V p=w.Home-w.NestForward*10*w.HomeScale;
                d.PushOpacity(l.Stage==LifeStage.Molting?Math.Min(.5,l.StageAge/45):.5*Math.Max(0,1-l.StageAge/90));
                d.PushTransform(new TranslateTransform(p.X,p.Y));d.PushTransform(new ScaleTransform(w.HomeScale*.8,w.HomeScale*.8));d.PushTransform(new RotateTransform(w.NestAngle));
                var profile=SkinCatalog.Get(w.Species);
                visitorArt.State=Mood.Rest;visitorArt.GaitBlend=0;visitorArt.StateTime=0;
                d.DrawEllipse(null,P("#e4c69a",.6),new Point(0,18),profile.BodyWidth,profile.BodyLength);
                d.DrawEllipse(null,P("#e4c69a",.6),new Point(0,-8),12*profile.HeadWidth,13*profile.HeadLength);
                for(int side=-1;side<=1;side+=2) for(int leg=0;leg<4;leg++) { var pose=SpiderPose.Leg(visitorArt,side,leg,profile);Line(d,"#c2aa83",.55,pose.Root.X,pose.Root.Y,pose.Knee.X,pose.Knee.Y);Line(d,"#c2aa83",.45,pose.Knee.X,pose.Knee.Y,pose.Tip.X*.75,pose.Tip.Y*.75); }
                d.Pop();d.Pop();d.Pop();d.Pop();
            }
        }
    }
}
