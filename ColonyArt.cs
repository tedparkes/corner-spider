using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
namespace CornerSpider {
    public partial class PetCanvas {
        sealed class ColonyVisual {
            public readonly ContainerVisual Root=new ContainerVisual();
            public readonly DrawingVisual Back=new DrawingVisual(),Body=new DrawingVisual(),Front=new DrawingVisual();
            public readonly PetCanvas Art=new PetCanvas();
            public string SilkKey,BodyKey;
            public bool Tucked=true;
            public ColonyVisual() { Root.Children.Add(Back);Root.Children.Add(Body);Root.Children.Add(Front); }
        }
        readonly Dictionary<ColonyPet,ColonyVisual> colonyVisuals=new Dictionary<ColonyPet,ColonyVisual>();
        ContainerVisual colonyLayer;
        double colonyDeviceScale=-1;
        V colonyOffset;
        protected override int VisualChildrenCount { get { return colonyLayer==null?0:1; } }
        protected override Visual GetVisualChild(int index) { if(index!=0 || colonyLayer==null) throw new ArgumentOutOfRangeException("index");return colonyLayer; }
        internal void UpdateColonyVisuals() {
            if(World==null) return;
            if(colonyLayer==null && World.Residents.Count>0) { colonyLayer=new ContainerVisual();AddVisualChild(colonyLayer); }
            if(colonyLayer==null) return;
            if(colonyDeviceScale!=DeviceScale || (colonyOffset-Offset).Length>0) {
                colonyLayer.Transform=new MatrixTransform(1/DeviceScale,0,0,1/DeviceScale,-Offset.X/DeviceScale,-Offset.Y/DeviceScale);colonyDeviceScale=DeviceScale;colonyOffset=Offset;
            }
            foreach(var old in new List<ColonyPet>(colonyVisuals.Keys)) if(!World.Residents.Contains(old)) { colonyLayer.Children.Remove(colonyVisuals[old].Root);colonyVisuals.Remove(old); }
            foreach(var pet in World.Residents) {
                ColonyVisual visual;if(!colonyVisuals.TryGetValue(pet,out visual)) { visual=new ColonyVisual();colonyVisuals[pet]=visual;colonyLayer.Children.Add(visual.Root); }
                var w=pet.Actor;var area=w.Areas[w.HomeArea];
                if(visual.Tucked!=w.UnderNestSilk) { visual.Tucked=w.UnderNestSilk;visual.Root.Children.Clear();visual.Root.Children.Add(visual.Back);visual.Root.Children.Add(visual.Tucked?visual.Body:visual.Front);visual.Root.Children.Add(visual.Tucked?visual.Front:visual.Body); }
                string silkKey=pet.Record.Species+":"+w.Home.X+":"+w.Home.Y+":"+w.HomeScale+":"+w.Corner+":"+area.X+":"+area.Y+":"+area.W+":"+area.H+":"+(int)(w.Web*30)+":"+w.WebLayers+":"+w.WebVisibility;
                if(visual.SilkKey!=silkKey) {
                    visual.SilkKey=silkKey;
                    using(var d=visual.Back.RenderOpen()) visual.Art.DrawHammock(d,w,false,pet.Record.Species);
                    using(var d=visual.Front.RenderOpen()) visual.Art.DrawHammock(d,w,true,pet.Record.Species);
                }
                // Independent visual bounds keep a small moving body from dirtying
                // every stationary nest and the entire span between desktop edges.
                string bodyKey=pet.Record.Species+":"+w.Spider.X+":"+w.Spider.Y+":"+Math.Round(w.Angle,1)+":"+Math.Round(w.BodyScale*512)+":"+w.State+":"+Math.Round(w.NestTuck*2)+":"+Math.Round(w.GaitBlend*2)+":"+(w.GaitBlend>.1?Math.Floor(w.StepPhase*8/(Math.PI*2)):w.State==Mood.Groom?Math.Floor(w.StateTime/.24):0)+":"+Math.Round(w.GaitDirection.X*2);
                if(visual.BodyKey!=bodyKey) { visual.BodyKey=bodyKey;using(var d=visual.Body.RenderOpen()) visual.Art.DrawResidentBody(d,w,pet.Record.Species); }
            }
        }
        readonly Dictionary<ColonyPet,PetCanvas> colonyArt=new Dictionary<ColonyPet,PetCanvas>();
        static readonly Dictionary<string,CachedArt> residentPoses=new Dictionary<string,CachedArt>();
        static readonly Queue<string> residentPoseOrder=new Queue<string>();
        internal static int ResidentPoseBuilds;
        void DrawResidentBody(DrawingContext d,World w,string species) {
            bool moving=w.GaitBlend>.1;int gait=moving?2:0,tuck=moving?0:(int)Math.Round(w.NestTuck*2);
            int frame=moving?((int)(w.StepPhase*8/(Math.PI*2))%8+8)%8:w.State==Mood.Groom?(int)(w.StateTime/.24)%8:0;
            int direction=moving?(w.GaitDirection.X>.4?0:w.GaitDirection.X<-.4?4:6):6;
            Mood pose=moving?Mood.Walk:w.State==Mood.Groom?Mood.Groom:w.State==Mood.Sleep?Mood.Sleep:Mood.Rest;
            string key=species+":"+pose+":"+frame+":"+gait+":"+tuck+":"+direction;CachedArt art;
            if(!residentPoses.TryGetValue(key,out art)) {
                ResidentPoseBuilds++;
                visitorArt.SetSpecies(species);visitorArt.CopyResidentPose(pose,frame,gait,tuck,direction);
                var drawing=new DrawingGroup();using(var c=drawing.Open()) DrawSpider(c,visitorArt,species);drawing.Freeze();art=CacheArt(drawing,1.5);
                // A bounded pose bank is shared by siblings. Position, facing and
                // growth remain continuous; only these small secondary leg poses are sampled.
                if(residentPoses.Count>=128) residentPoses.Remove(residentPoseOrder.Dequeue());residentPoses[key]=art;residentPoseOrder.Enqueue(key);
            }
            d.PushTransform(new TranslateTransform(w.Spider.X,w.Spider.Y-w.Lift));d.PushTransform(new ScaleTransform(w.BodyScale,w.BodyScale));d.PushTransform(new RotateTransform(w.Angle));d.DrawImage(art.Image,art.Bounds);d.Pop();d.Pop();d.Pop();
        }
        public void DrawColonyScene(DrawingContext d,World w) { DrawColony(d,w);DrawNestScene(d,w,w.Species); }
        void DrawColony(DrawingContext d,World w) {
            foreach(var old in new List<ColonyPet>(colonyArt.Keys)) if(!w.Residents.Contains(old)) colonyArt.Remove(old);
            foreach(var p in w.Residents) { PetCanvas art;if(!colonyArt.TryGetValue(p,out art)) { art=new PetCanvas();colonyArt[p]=art; }art.DrawNestScene(d,p.Actor,p.Record.Species); }
        }
        static V ColonyAnchor(double ex,double ey,double t,double spread) { return ex<ey?new V(-ex,(t*2-1)*spread):new V((t*2-1)*spread,-ey); }
        DrawingGroup colonyBack,colonyFront;
        SilkImage colonyBackImage,colonyFrontImage;
        double colonySilkScale=-1;
        string colonyKey;
        void DrawColonyRetreat(DrawingContext d,World w,bool front,string species) {
            var a=w.Areas[w.HomeArea];double scale=w.HomeScale,ex=Math.Abs((w.Corner.EndsWith("Right")?a.X+a.W:a.X)-w.Home.X)/scale,ey=Math.Abs((w.Corner.StartsWith("Top")?a.Y:a.Y+a.H)-w.Home.Y)/scale;
            var profile=SilkProfile.Get(species);int growth=(int)(w.Web*30);string key=species+":"+growth+":"+w.WebLayers+":"+ex.ToString("F0")+":"+ey.ToString("F0");
            if(key!=colonyKey) {
                colonyKey=key;colonyBack=new DrawingGroup();colonyFront=new DrawingGroup();
                using(var c=colonyBack.Open()) for(int i=0;i<14+w.WebLayers;i++) { V anchor=ColonyAnchor(ex,ey,i/(double)(13+w.WebLayers),105);V end=new V(Math.Cos(i*2.4)*25,Math.Sin(i*2.4)*36);Line(c,"#b4e5eee6",.65,anchor.X,anchor.Y,end.X,end.Y); }
                using(var c=colonyFront.Open()) {
                    c.PushTransform(new RotateTransform(45));c.PushTransform(new ScaleTransform(profile.Width,profile.Length));var shape=Pocket(profile);
                    c.DrawGeometry(B("#1aeef4e6"),P("#a0e4eee1",.7),shape);c.PushClip(shape);var random=new Random(profile.Seed);
                    for(int i=0;i<24+growth+w.WebLayers;i++) { double x=R(random,-50,50);Line(c,"#64edf3e8",.35,x,-50,x+R(random,-18,18),58); }
                    RetreatDetails(c,profile,w.Web);c.Pop();c.Pop();c.Pop();
                }colonyBack.Freeze();colonyFront.Freeze();colonySilkScale=-1;
            }
            if(colonySilkScale!=scale) { colonySilkScale=scale;colonyBackImage=CacheSilk(colonyBack,scale);colonyFrontImage=CacheSilk(colonyFront,scale); }
            d.PushOpacity(w.WebVisibility);d.PushTransform(new TranslateTransform(w.Home.X,w.Home.Y));d.PushTransform(new ScaleTransform(w.Corner.EndsWith("Right")?-scale:scale,w.Corner.StartsWith("Top")?scale:-scale));d.DrawDrawing((front?colonyFrontImage:colonyBackImage).Drawing);d.Pop();d.Pop();d.Pop();
        }
    }
}
