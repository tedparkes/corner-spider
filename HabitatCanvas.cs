using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CornerSpider {
    internal sealed class HabitatCanvas : FrameworkElement {
        enum Visit { Roaming, Approaching, Weaving, Returning }

        readonly HabitatScene scene;
        readonly HabitatExplorer explorer;
        readonly World pose=new World(1280,720,18);
        HabitatSilkPlan web;
        HabitatSilkConstruction construction;
        Visit visit;
        V spider, silkLanding;
        double depth, heading, clock, untilVisit=5, visitAge, lift, hop;
        double silkPosition, silkRest;
        HabitatSilkSegment entrySupport;
        bool moving, airborne, hasEntrySupport;
        string species="", pendingSpecies;
        double size=1, opacity=1;
        DrawingGroup completedSilk;
        int completedStrands;
        Rect silkViewport;

        public HabitatCanvas(HabitatScene scene) {
            this.scene=scene;
            explorer=new HabitatExplorer(scene.Field,71,new V(.52,.78));
            spider=explorer.Position;depth=explorer.Depth;
            ClipToBounds=true;
        }

        internal string Status {
            get {
                if(!explorer.HasSurface) return "No stable surface found in this depth map";
                if(web==null) return "Exploring the habitat";
                if(visit==Visit.Approaching) return "Finding a route to the silk anchors";
                if(visit==Visit.Returning) return "Heading back to the surface";
                if(visit==Visit.Weaving) {
                    if(!construction.Complete) return "Laying silk";
                    if(!hasEntrySupport) return "Waiting at the silk anchor";
                    return silkRest>0?"Resting on the silk":"Crossing the silk";
                }
                return construction.Complete?"Exploring near its silk home":"Exploring the habitat";
            }
        }
        internal V Position { get { return spider; } }
        internal double Depth { get { return depth; } }
        internal bool WebComplete { get { return construction!=null && construction.Complete; } }
        internal int VisibleSilk { get { return construction==null?0:construction.VisibleCount; } }
        internal double SilkPosition { get { return silkPosition; } }

        public void Apply(string skin,double scale,double webOpacity) {
            size=scale;opacity=webOpacity;
            if(species==skin) { pendingSpecies=null;return; }
            if(silkPosition>0) {
                pendingSpecies=skin;
                if(visit!=Visit.Returning) ChangeVisit(Visit.Returning);
                return;
            }
            SelectSpecies(skin);
        }

        void SelectSpecies(string skin) {
            species=skin;pendingSpecies=null;pose.SetSpecies(skin);
            web=explorer.HasSurface?HabitatSilk.Plan(scene.Field,skin,explorer.Position):null;
            construction=web==null?null:new HabitatSilkConstruction(web);
            silkPosition=silkRest=0;hasEntrySupport=false;
            if(web!=null) {
                silkLanding=web.Landing.Position;
                if(web.Segments.Count>0) {
                    entrySupport=web.Segments[0];
                    V start=entrySupport.Start.Position;
                    HabitatDepth field=scene.Field;
                    hasEntrySupport=entrySupport.Role==HabitatSilkRole.Support &&
                        field.Distance(start,silkLanding)<=.025 && field.HasFooting(start) &&
                        field.CanWalk(silkLanding,start) && Math.Abs(entrySupport.Start.Depth-field.DepthAt(start))<.002 &&
                        CanFollowSupport(field,entrySupport);
                    if(hasEntrySupport) silkLanding=start;
                }
            }
            completedSilk=null;ChangeVisit(Visit.Roaming);untilVisit=5;
        }

        static bool CanFollowSupport(HabitatDepth field,HabitatSilkSegment support) {
            double distance=field.Distance(support.Start.Position,support.End.Position);
            if(distance<=.00002) return false;
            int samples=Math.Max(1,(int)Math.Ceiling(distance*field.Height*2));
            for(int i=1;i<=samples;i++) {
                HabitatSilkPoint point=HabitatSilkPoint.Lerp(support.Start,support.End,(double)i/samples);
                if(field.VisibilityAt(point.Position,point.Depth)<.5) return false;
            }
            return true;
        }

        public void Tick(double seconds) {
            if(!World.Finite(seconds) || seconds<=0) return;
            double dt=Math.Min(.08,seconds);
            clock+=dt;visitAge+=dt;
            V old=spider;
            double phase=pose.StepPhase;

            if(visit==Visit.Weaving || visit==Visit.Returning) {
                UpdateOnSilk(dt);
            } else {
                explorer.Tick(dt);
                spider=explorer.Position;depth=explorer.Depth;heading=explorer.Heading;
                moving=explorer.Mood==HabitatMotion.Walk;
                airborne=explorer.Mood==HabitatMotion.Hop;
                lift=explorer.HopLift;hop=explorer.HopProgress;

                if(visit==Visit.Approaching) {
                    if(AtSilkLanding()) {
                        BeginWeaving();
                    } else if(!explorer.HasRoute || visitAge>30) {
                        ChangeVisit(Visit.Roaming);untilVisit=12;
                    }
                } else if(web!=null && (untilVisit-=dt)<=0 && !airborne) {
                    if(AtSilkLanding()) BeginWeaving();
                    else if(explorer.SetDestination(silkLanding)) ChangeVisit(Visit.Approaching);
                    else untilVisit=12;
                }
            }

            V displacement=new V((spider.X-old.X)*scene.Field.Aspect,spider.Y-old.Y)*720;
            if(moving) phase+=displacement.Length/(4.5*HabitatDepth.ScaleAt(depth)*size);
            pose.StepPhase=phase;
            pose.Angle=heading;pose.State=airborne?Mood.Jump:moving?Mood.Walk:Mood.Rest;
            pose.UpdateGaitDirection(displacement,dt);
            double gaitTarget=moving?1:0;
            pose.GaitBlend+=(gaitTarget-pose.GaitBlend)*Math.Min(1,dt*8);
            pose.Clock=clock;
            InvalidateVisual();
        }

        void ChangeVisit(Visit next) { visit=next;visitAge=0; }

        bool AtSilkLanding() {
            return !airborne && scene.Field.Distance(spider,silkLanding)<=.00002 &&
                Math.Abs(depth-scene.Field.DepthAt(silkLanding))<.002;
        }

        void BeginWeaving() {
            silkRest=0;ChangeVisit(Visit.Weaving);
        }

        void UpdateOnSilk(double dt) {
            airborne=false;lift=hop=0;
            if(visit==Visit.Returning) {
                if(MoveOnSilk(0,dt)) {
                    ChangeVisit(Visit.Roaming);untilVisit=46;
                    if(pendingSpecies!=null) SelectSpecies(pendingSpecies);
                }
                return;
            }

            construction.Advance(dt);
            // Only the anchored support is a known route. Its drawn prefix limits the climb.
            double available=hasEntrySupport?construction.SegmentProgress(0):0;
            bool atEnd=MoveOnSilk(available,dt);
            if(construction.Complete && atEnd && (silkRest+=dt)>=4) ChangeVisit(Visit.Returning);
        }

        bool MoveOnSilk(double target,double dt) {
            moving=false;
            if(!hasEntrySupport || target==silkPosition) return true;
            V strand=entrySupport.End.Position-entrySupport.Start.Position;
            double projected=scene.Field.Distance(entrySupport.Start.Position,entrySupport.End.Position);
            double depthSpan=(entrySupport.End.Depth-entrySupport.Start.Depth)*.32;
            double length=Math.Sqrt(projected*projected+depthSpan*depthSpan);
            double direction=Math.Sign(target-silkPosition);
            double advance=Math.Min(Math.Abs(target-silkPosition),dt*.24/length);
            moving=advance*projected>.00001;
            if(moving) {
                double targetAngle=Math.Atan2(strand.X*scene.Field.Aspect*direction,-strand.Y*direction)*180/Math.PI;
                double turn=(targetAngle-heading+540)%360-180;
                heading=(heading+Math.Max(-360*dt,Math.Min(360*dt,turn))+360)%360;
            }
            silkPosition+=direction*advance;
            if(Math.Abs(target-silkPosition)<.000001) silkPosition=target;
            HabitatSilkPoint point=HabitatSilkPoint.Lerp(entrySupport.Start,entrySupport.End,silkPosition);
            spider=point.Position;depth=point.Depth;
            return silkPosition==target;
        }

        internal HabitatRouteOverlay RouteOverlay { get; set; }
        internal Rect ImageViewport { get { return FitImage(new Rect(0,0,ActualWidth,ActualHeight)); } }

        internal bool TryGetInspectionOrigin(out V origin,out bool atSpider) {
            origin=explorer.GroundPoint;
            atSpider=!airborne && visit!=Visit.Weaving && visit!=Visit.Returning;
            return explorer.HasSurface && scene.Field.HasFooting(origin);
        }

        protected override void OnRender(DrawingContext d) {
            base.OnRender(d);
            if(ActualWidth<=1 || ActualHeight<=1) return;
            Draw(d,new Rect(0,0,ActualWidth,ActualHeight));
        }

        Rect FitImage(Rect bounds) {
            double ratio=Math.Min(bounds.Width/scene.Color.PixelWidth,bounds.Height/scene.Color.PixelHeight);
            double width=scene.Color.PixelWidth*ratio,height=scene.Color.PixelHeight*ratio;
            return new Rect(bounds.X+(bounds.Width-width)*.5,bounds.Y+(bounds.Height-height)*.5,width,height);
        }

        internal void Draw(DrawingContext d,Rect bounds) {
            d.DrawRectangle(Brushes.Black,null,bounds);
            var viewport=FitImage(bounds);
            d.DrawImage(scene.Color,viewport);
            d.PushClip(new RectangleGeometry(viewport));
            if(construction!=null && construction.VisibleCount>0 && opacity>0) {
                d.PushOpacity(opacity);
                DrawSilk(d,viewport);
                d.Pop();
            }
            if(species.Length>0 && explorer.HasSurface) DrawPet(d,viewport);
            if(RouteOverlay!=null) RouteOverlay.Draw(d,viewport);
            d.Pop();
        }

        void DrawSilk(DrawingContext d,Rect viewport) {
            if(completedSilk==null || viewport!=silkViewport) {
                completedSilk=new DrawingGroup();completedStrands=0;silkViewport=viewport;
            }
            int count=construction.VisibleCount;
            double fraction=construction.SegmentProgress(count-1);
            int complete=fraction>=1?count:count-1;
            while(completedStrands<complete) {
                var placed=web.Segments[completedStrands++];
                var drawing=new DrawingGroup();
                using(var silk=drawing.Open())
                    HabitatArt.DrawStrand(silk,scene.Field,viewport,placed.Start,placed.End,placed.Weight,placed.Role);
                drawing.Freeze();completedSilk.Children.Add(drawing);
            }
            d.DrawDrawing(completedSilk);
            if(complete<count && fraction>0) {
                var active=web.Segments[complete];
                HabitatArt.DrawStrand(d,scene.Field,viewport,active.Start,
                    HabitatSilkPoint.Lerp(active.Start,active.End,fraction),active.Weight,active.Role);
            }
        }

        void DrawPet(DrawingContext d,Rect viewport) {
            Point projected=HabitatArt.Project(spider,viewport);
            double liftPixels=lift*viewport.Height;
            double viewScale=Math.Max(.28,Math.Min(1.35,viewport.Height/720));
            pose.Spider=new V(projected.X,projected.Y);
            pose.Scale=HabitatDepth.ScaleAt(depth)*size*viewScale;
            pose.JumpTime=hop*pose.JumpDuration;
            double radius=SpiderPose.Radius*pose.Scale+5;
            var bodyBounds=new Rect(projected.X-radius,projected.Y-radius-liftPixels,radius*2,radius*2+liftPixels);
            Brush mask;
            if(!HabitatArt.DepthMask(scene.Field,viewport,bodyBounds,Math.Min(1,depth+.034),out mask)) return;
            if(mask!=null) d.PushOpacityMask(mask);
            PetCanvas.DrawSpider(d,pose,species,liftPixels);
            if(mask!=null) d.Pop();
        }
    }

    internal static class HabitatArt {
        public static Point Project(V position,Rect viewport) {
            return new Point(viewport.X+position.X*viewport.Width,viewport.Y+position.Y*viewport.Height);
        }

        static V Unproject(double x,double y,Rect viewport) {
            return new V((x-viewport.X)/viewport.Width,(y-viewport.Y)/viewport.Height);
        }

        public static bool DepthMask(HabitatDepth field,Rect viewport,Rect bounds,double depth,out Brush brush) {
            brush=null;bounds.Intersect(viewport);
            if(bounds.IsEmpty || bounds.Width<=0 || bounds.Height<=0) return false;
            int width=Math.Max(1,Math.Min(144,(int)Math.Ceiling(bounds.Width/1.6)));
            int height=Math.Max(1,Math.Min(144,(int)Math.Ceiling(bounds.Height/1.6)));
            byte[] pixels=null;
            bool visible=false;
            for(int y=0;y<height;y++) for(int x=0;x<width;x++) {
                V p=Unproject(bounds.X+(x+.5)*bounds.Width/width,bounds.Y+(y+.5)*bounds.Height/height,viewport);
                byte alpha=(byte)Math.Round(field.VisibilityAt(p,depth)*255);
                visible|=alpha>0;
                int pixel=(y*width+x)*4+3;
                if(pixels==null && alpha<255) {
                    pixels=new byte[width*height*4];
                    for(int a=3;a<pixel;a+=4) pixels[a]=255;
                }
                if(pixels!=null) pixels[pixel]=alpha;
            }
            if(!visible) return false;
            if(pixels!=null) {
                var image=BitmapSource.Create(width,height,96,96,PixelFormats.Pbgra32,null,pixels,width*4);
                image.Freeze();
                var mask=new ImageBrush(image) {
                    ViewportUnits=BrushMappingMode.Absolute,Viewport=bounds,Stretch=Stretch.Fill
                };
                mask.Freeze();brush=mask;
            }
            return true;
        }

        public static void DrawStrand(DrawingContext d,HabitatDepth field,Rect viewport,HabitatSilkPoint start,
            HabitatSilkPoint end,double weight,HabitatSilkRole role) {
            Point first=Project(start.Position,viewport),last=Project(end.Position,viewport);
            int count=Math.Max(1,Math.Min(420,(int)Math.Ceiling((last-first).Length/2)));
            var line=new StreamGeometry();
            using(var path=line.Open()) {
                bool penDown=false;
                for(int i=0;i<count;i++) {
                    double t=(i+.5)/count;
                    var middle=HabitatSilkPoint.Lerp(start,end,t);
                    if(field.VisibilityAt(middle.Position,Math.Min(1,middle.Depth+.013))<.48) { penDown=false;continue; }
                    if(!penDown) path.BeginFigure(first+(last-first)*((double)i/count),false,false);
                    path.LineTo(first+(last-first)*((double)(i+1)/count),true,false);penDown=true;
                }
            }
            line.Freeze();
            double width=Math.Max(.40,weight)*Math.Max(.66,Math.Min(1.4,viewport.Height/640));
            byte alpha=role==HabitatSilkRole.Support?(byte)195:(byte)148;
            var halo=new Pen(new SolidColorBrush(Color.FromArgb(120,23,38,29)),width+1.05);
            var silk=new Pen(new SolidColorBrush(Color.FromArgb(alpha,233,245,224)),width);
            halo.Freeze();silk.Freeze();
            d.DrawGeometry(null,halo,line);d.DrawGeometry(null,silk,line);
        }
    }
}
