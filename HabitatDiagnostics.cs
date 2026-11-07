using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CornerSpider {
    internal static class HabitatDiagnostics {
        public static int Run(string folder,string[] args) {
            var log=new StringBuilder();
            try {
                if(args.Length!=4) throw new ArgumentException("Pass output folder, photo and depth image.");
                string profile=Path.Combine(folder,"profile");
                var scene=HabitatLibrary.Import(profile,args[2],args[3],true);
                var listed=HabitatLibrary.List(profile);
                Check(log,listed.Length==1 && listed[0].Id==scene.Id && listed[0].WhiteIsNear && scene.Color.IsFrozen,"matching pair imports into the local library");
                var inverse=HabitatLibrary.Load(profile,scene.Id,false);
                var point=new V(.3,.55);
                Check(log,Math.Abs(scene.Field.DepthAt(point)+inverse.Field.DepthAt(point)-1)<.001,"saved depth convention can be inverted");
                HabitatLibrary.SetPolarity(profile,scene.Id,false);
                Check(log,!HabitatLibrary.List(profile)[0].WhiteIsNear,"each saved habitat remembers its own depth convention");
                HabitatLibrary.SetPolarity(profile,scene.Id,true);
                CheckOcclusion(log);
                CheckRoutes(log,folder,scene);
                foreach(string species in new[]{"Regal","CrossOrb","SouthernWidow","BarnFunnel","CrossOrbMale"})
                    TestExploration(log,folder,scene,species);

                using(var host=new PetHost(profile,true)) {
                    host.Commands["habitat"].PerformClick();
                    Check(log,host.SettingsWindow!=null && ((System.Windows.Controls.TabItem)host.SettingsWindow.Tabs.SelectedItem).Header.ToString()=="Habitat",
                        "tray command opens the Habitat settings tab");
                    host.OpenHabitat(scene,true);
                    var window=FindHabitat();
                    Check(log,window!=null && window.IsVisible && !window.Topmost && AllOverlays(host,false),"habitat opens normally and hides desktop overlays");
                    window.RouteInspectionEnabled=true;
                    Check(log,window.RouteInspectionEnabled && !host.Paused,"route inspection pauses the habitat without changing the pet pause setting");
                    window.RouteInspectionEnabled=false;
                    SendKey(window,Key.F11);
                    Check(log,window.WindowState==WindowState.Maximized && window.WindowStyle==WindowStyle.None,"F11 fills the screen");
                    SendKey(window,Key.Escape);
                    Check(log,window.WindowState==WindowState.Normal && window.WindowStyle==WindowStyle.SingleBorderWindow,"Escape restores the standard window");
                    host.RefreshDisplays();
                    Check(log,AllOverlays(host,false),"display refresh keeps overlays hidden during habitat use");
                    host.OpenHabitat(scene,true);
                    Check(log,FindHabitat()==window,"opening the same scene reuses the window");
                    window.Close();
                    Check(log,FindHabitat()==null && AllOverlays(host,true),"closing habitat restores regular desktop overlays");
                    var loaded=Settings.Load(profile);
                    Check(log,loaded.HabitatId==scene.Id && loaded.HabitatWhiteNear,"scene selection persists in the existing settings");
                }
                log.AppendLine("RESULT: PASS (0 failures).");
                return 0;
            } catch(Exception ex) {
                log.AppendLine("FAIL: "+ex);
                return 1;
            } finally { File.WriteAllText(Path.Combine(folder,"habitat-checks.txt"),log.ToString()); }
        }

        static HabitatWindow FindHabitat() {
            foreach(Window window in Application.Current.Windows)
                if(window is HabitatWindow) return (HabitatWindow)window;
            return null;
        }

        static void SendKey(Window window,Key key) {
            window.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice,PresentationSource.FromVisual(window),
                Environment.TickCount,key) { RoutedEvent=Keyboard.PreviewKeyDownEvent });
        }

        static bool AllOverlays(PetHost host,bool visible) {
            if(host.Windows.Count==0) return false;
            foreach(var overlay in host.Windows) if(overlay.IsVisible!=visible) return false;
            return true;
        }

        static void TestExploration(StringBuilder log,string folder,HabitatScene scene,string species) {
            var canvas=new HabitatCanvas(scene);canvas.Apply(species,1,1);
            var from=canvas.Position;double near=canvas.Depth,far=canvas.Depth,travel=0;
            var plan=HabitatSilk.Plan(scene.Field,species,from);
            int silkFrames=0;
            bool silk=false,validFooting=true,validSilk=true;
            for(int i=0;i<2600;i++) {
                var before=canvas.Position;
                canvas.Tick(.05);near=Math.Max(near,canvas.Depth);far=Math.Min(far,canvas.Depth);
                travel+=(canvas.Position-before).Length;
                V ground;bool atSpider;
                validFooting&=canvas.TryGetInspectionOrigin(out ground,out atSpider);
                if(canvas.SilkPosition>0) {
                    silkFrames++;
                    var support=plan.Segments[0];
                    var onStrand=HabitatSilkPoint.Lerp(support.Start,support.End,canvas.SilkPosition);
                    validSilk&=(onStrand.Position-canvas.Position).Length<.00002 &&
                        Math.Abs(onStrand.Depth-canvas.Depth)<.00002 && !atSpider;
                }
                if(!silk && canvas.VisibleSilk>4 && !canvas.WebComplete) {
                    silk=true;Snapshot(folder,species+"-building",canvas);
                }
            }
            Check(log,travel>.18 && near-far>.10,"pet explores different depth surfaces: "+species);
            Check(log,validFooting && validSilk,"surface footholds and laid silk support every move: "+species);
            if(SkinCatalog.Get(species).Category=="Orb weavers" && SkinCatalog.Get(species).Male)
                Check(log,canvas.VisibleSilk==0,"male orb weaver searches without building a capture web");
            else
                Check(log,silk && canvas.WebComplete,"anchors and progressive silk finish: "+species);
            log.AppendLine(String.Format("{0} depth {1:F2} to {2:F2}, traveled {3:F2}, strands {4}, silk-supported frames {5}",
                species,far,near,travel,canvas.VisibleSilk,silkFrames));
            Snapshot(folder,species+"-habitat",canvas);
        }

        static void CheckRoutes(StringBuilder log,string folder,HabitatScene scene) {
            var canvas=new HabitatCanvas(scene);canvas.Apply("CrossOrbMale",1,1);
            var overlay=new HabitatRouteOverlay(scene.Field);canvas.RouteOverlay=overlay;
            V start;bool atSpider;
            Check(log,canvas.TryGetInspectionOrigin(out start,out atSpider) && atSpider,"route inspector starts at the spider's measured surface");
            V[] destinations={new V(.73,.31),new V(.68,.40),new V(.62,.83),new V(.56,.87)};
            foreach(var target in destinations) {
                overlay.Inspect(start,target,atSpider);
                if(overlay.Reachable) break;
            }
            Check(log,overlay.Reachable,"forest route inspector finds a supported destination");
            Snapshot(folder,"forest-route-supported",canvas);
            overlay.Inspect(start,new V(.72091,.24607),atSpider);
            Check(log,!overlay.Reachable,"forest route inspector rejects the previously unsupported pause point");
            Snapshot(folder,"forest-route-blocked",canvas);
        }

        static void Snapshot(string folder,string name,HabitatCanvas canvas) {
            var visual=new DrawingVisual();
            using(var d=visual.RenderOpen()) canvas.Draw(d,new Rect(0,0,1280,720));
            var image=new RenderTargetBitmap(1280,720,96,96,PixelFormats.Pbgra32);
            image.Render(visual);
            var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(image));
            using(var file=File.Create(Path.Combine(folder,name+".png"))) encoder.Save(file);
        }

        static void CheckOcclusion(StringBuilder log) {
            var samples=new byte[65*65];
            for(int y=0;y<65;y++) for(int x=0;x<65;x++) samples[y*65+x]=x>=29 && x<=35?(byte)246:(byte)60;
            var field=new HabitatDepth(65,65,samples);
            var viewport=new Rect(0,0,180,100);
            var visual=new DrawingVisual();
            using(var d=visual.RenderOpen()) {
                d.DrawRectangle(Brushes.Black,null,viewport);
                var body=new Rect(25,15,130,32);Brush mask;
                Check(log,HabitatArt.DepthMask(field,viewport,body,.5,out mask) && mask!=null,"near obstacle produces a partial sprite opacity mask");
                d.PushOpacityMask(mask);d.DrawRectangle(Brushes.White,null,body);d.Pop();
                HabitatArt.DrawStrand(d,field,viewport,new HabitatSilkPoint(new V(.15,.76),.45),
                    new HabitatSilkPoint(new V(.85,.76),.52),1,HabitatSilkRole.Support);
            }
            var bitmap=new RenderTargetBitmap(180,100,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);
            Check(log,Light(bitmap,45,27)>200 && Light(bitmap,90,27)<18 && Light(bitmap,135,27)>200,
                "native rendering hides only the portion of a sprite behind the obstacle");
            Check(log,Light(bitmap,42,75)>45 && Light(bitmap,90,75)<18 && Light(bitmap,138,75)>45,
                "native rendering clips only the silk between visible spans");
        }

        static byte Light(BitmapSource bitmap,int x,int y) {
            var pixel=new byte[4];bitmap.CopyPixels(new Int32Rect(x,y,1,1),pixel,4,0);return pixel[1];
        }

        static void Check(StringBuilder log,bool success,string message) {
            if(!success) throw new InvalidOperationException(message);
            log.AppendLine("PASS: "+message);
        }
    }
}
