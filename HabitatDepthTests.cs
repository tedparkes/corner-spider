using System;
using System.Collections.Generic;

namespace CornerSpider {
    public static class HabitatDepthTests {
        static int checks;
        static void Check(bool condition,string message) {
            checks++;
            if(!condition) throw new Exception("Habitat depth: "+message);
        }

        static HabitatDepth Field(Func<double,double,double> depth,int width=128,int height=128) {
            var pixels=new byte[width*height];
            for(int y=0;y<height;y++) for(int x=0;x<width;x++)
                pixels[y*width+x]=(byte)Math.Round(255*depth((double)x/(width-1),(double)y/(height-1)));
            return new HabitatDepth(width,height,pixels);
        }

        static void CheckRoute(HabitatDepth field,V start,V end,List<HabitatStep> steps) {
            Check(steps.Count>0 && (steps[steps.Count-1].Destination-end).Length<.00001,"route keeps its requested endpoint");
            foreach(var step in steps) {
                Check(step.Hop?field.CanHop(start,step.Destination):field.CanWalk(start,step.Destination),
                    "each segment has measured support or a short safe hop");
                start=step.Destination;
            }
        }

        public static void Run() {
            checks=0;
            var pixels=new byte[] {0,255,255,0};
            var bilinear=new HabitatDepth(2,2,pixels);
            pixels[0]=255;
            Check(bilinear.DepthAt(new V(0,0))==0 && bilinear.DepthAt(new V(1,0))==1 &&
                Math.Abs(bilinear.DepthAt(new V(.5,.5))-.5)<.000001,"samples retain near-white encoding and interpolate continuously");
            Check(HabitatDepth.ScaleAt(.9)>HabitatDepth.ScaleAt(.3)*1.5,"foreground spiders render measurably larger");
            Check(bilinear.VisibilityAt(new V(1,0),.5)==0 && bilinear.VisibilityAt(new V(0,0),.5)==1 &&
                bilinear.VisibilityAt(new V(.5,.5),.44)>0 && bilinear.VisibilityAt(new V(.5,.5),.44)<1,
                "near surfaces occlude, far surfaces show through, and depth edges blend");

            var ramp=Field((x,y)=>.08+.81*y,160,96);
            V start=new V(.53,.76),end=new V(.60,.35);
            List<HabitatStep> steps;
            Check(HabitatRoute.TryPlan(ramp,start,end,out steps) && steps.Count==1 && !steps[0].Hop,
                "a continuous depth slope is a walkable surface");
            CheckRoute(ramp,start,end,steps);
            var explorer=new HabitatExplorer(ramp,42,start);
            Check(explorer.SetDestination(end),"directed exploration accepts a reachable point");
            bool paused=false,moved=false;
            double oldDepth=explorer.Depth;
            for(int frame=0;frame<1800 && explorer.HasRoute;frame++) {
                explorer.Tick(.025);
                paused|=explorer.Mood==HabitatMotion.Pause && moved && explorer.HasRoute;
                moved|=explorer.Mood==HabitatMotion.Walk;
                Check(HabitatDepth.Inside(explorer.Position) && explorer.Depth<=oldDepth+.000001,
                    "climbing away follows the measured depth continuously");
                oldDepth=explorer.Depth;
            }
            Check(moved && paused && !explorer.HasRoute && (explorer.Position-end).Length<.0001 &&
                ramp.DepthAt(start)-explorer.Depth>.30,"a real depth change includes pauses and reaches the original destination");

            var gap=Field((x,y)=>x>.477 && x<.523?.10:.54);
            start=new V(.452,.5);end=new V(.548,.5);
            Check(!gap.CanWalk(start,end) && HabitatRoute.TryPlan(gap,start,end,out steps),
                "a narrow floor break is reachable without walking through it");
            CheckRoute(gap,start,end,steps);
            Check(steps.Exists(step=>step.Hop),"routing marks the narrow gap as a hop");
            explorer=new HabitatExplorer(gap,7,start);
            Check(explorer.SetDestination(end),"a planned hop is usable during exploration");
            bool lifted=false;
            for(int frame=0;frame<500 && explorer.HasRoute;frame++) {
                explorer.Tick(.02);
                if(explorer.Mood!=HabitatMotion.Hop) continue;
                lifted|=explorer.HopLift>.010;
                Check(explorer.Depth>.45,"a hop stays at the shoreline depth above the gap");
            }
            Check(lifted && (explorer.Position-end).Length<.0001,"a short visible hop lands at the planned surface");

            var wall=Field((x,y)=>x>.44 && x<.56 && y<.65?.92:.30);
            start=new V(.33,.50);end=new V(.67,.50);
            Check(!wall.CanWalk(start,end) && HabitatRoute.TryPlan(wall,start,end,out steps),"routing finds the opening around a foreground obstruction");
            CheckRoute(wall,start,end,steps);
            Check(steps.Exists(step=>step.Destination.Y>.65),"the route detours around the blocking depth boundary");
            var cliff=Field((x,y)=>x<.5?.15:.90);
            Check(!HabitatRoute.TryPlan(cliff,new V(.30,.52),new V(.70,.52),out steps),"a hard depth cliff is never crossed by a walk or long hop");

            var first=new HabitatExplorer(ramp,19);
            var second=new HabitatExplorer(ramp,19);
            for(int i=0;i<55;i++) { first.Tick(.02);second.Tick(.02); }
            Check(first.HasRoute && Math.Abs(ramp.DepthAt(first.Target)-ramp.DepthAt(first.Position))>.18,
                "automatic exploration prefers endpoints with meaningful depth changes");
            for(int i=0;i<800;i++) {
                first.Tick(.02);second.Tick(.02);
                Check((first.Position-second.Position).Length==0 && first.Heading==second.Heading && first.Mood==second.Mood,
                    "a seeded route and its pauses replay deterministically");
            }
            Console.WriteLine("PASS: "+checks+" habitat depth checks (bilinear depth, scale, occlusion, routed slopes, safe hops, pauses and seeded exploration).");
        }
    }
}
