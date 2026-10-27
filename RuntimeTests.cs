using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Interop;
using Forms=System.Windows.Forms;

namespace CornerSpider {
    // Explicit opt-in diagnostics use an isolated settings directory and never change autostart.
    public static partial class RuntimeTests {
        static StringBuilder log;
        static int failures;
        static void Check(bool b,string message) { log.AppendLine((b?"PASS: ":"FAIL: ")+message);if(!b) failures++; }
        public static int Run(Application app,string[] args) {
            string folder=args.Length>1?Path.GetFullPath(args[1]):Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"diagnostics");Directory.CreateDirectory(folder);
            if(args[0]=="--desktop-test") return Desktop(app,folder);
            if(args[0]=="--encounter-preview") { RenderEncounterPreview(folder);return 0; }
            if(args[0]=="--travel-preview") { RenderTravelPreview(folder);return 0; }
            if(args[0]=="--prey-preview") { RenderPreyPreview(folder);return 0; }
            if(args[0]=="--release-preview") { RenderSkinGallery(folder);RenderFingerPreview(folder);RenderHammockGallery(folder);return 0; }
            if(args[0]=="--hammock-preview") { RenderHammockGallery(folder);RenderSilkGallery(folder);return 0; }
            if(args[0]=="--silk-test") {
                log=new StringBuilder("Corner Spider retreat rendering checks\n");failures=0;
                try { CheckSilkArt(folder);CheckRetreatVariety(folder);RenderHammockGallery(folder); }
                catch(Exception ex) { Check(false,ex.ToString()); }
                log.AppendLine("RESULT: "+(failures==0?"PASS":"FAIL")+" ("+failures+" failures).");
                File.WriteAllText(Path.Combine(folder,"silk-render-checks.txt"),log.ToString());return failures==0?0:1;
            }
            if(args[0]=="--finger-preview") { RenderFingerPreview(folder);return 0; }
            if(args[0]=="--nest-preview") { RenderNestEntry(folder);RenderHammockGallery(folder);return 0; }
            if(args[0]=="--dismount-preview") { RenderDismountPreview(folder);return 0; }
            if(args[0]=="--render-preview") {
                RenderPreview(Path.Combine(folder,"species-preview.png"));RenderBoldPreview(Path.Combine(folder,"bold-preview.png"));
                RenderOrientationPreview(Path.Combine(folder,"orientation-preview.png"));RenderMotionPreview(folder);RenderPlayPreview(folder);RenderFingerPreview(folder);
                using(var icon=PetHost.MakeIcon("Bold")) using(var file=File.Create(Path.Combine(folder,"CornerSpider.ico"))) icon.Save(file);
                return 0;
            }
            log=new StringBuilder();log.AppendLine("Corner Spider 1.0 RC14 Windows runtime checks "+DateTime.Now.ToString("s"));
            string testSettings=Path.Combine(folder,"settings");Check(new Settings { FingerTone=.37,Species="PortiaLabiata" }.Save(testSettings),"seed isolated finger-tone and Portia preferences");
            IntPtr foreground=Native.GetForegroundWindow();var host=new PetHost(testSettings,true);
            IntPtr afterCreation=Native.GetForegroundWindow();uint foregroundPid,afterCreationPid;
            Native.GetWindowThreadProcessId(foreground,out foregroundPid);Native.GetWindowThreadProcessId(afterCreation,out afterCreationPid);
            log.AppendLine("FOCUS: HWND "+foreground+" -> "+afterCreation+", PID "+foregroundPid+" -> "+afterCreationPid+", pet PID "+Process.GetCurrentProcess().Id+".");
            Check(host.World.FingerTone==.37,"saved finger tone is loaded into the desktop world at startup");
            Check(host.World.HuntsSpiders && host.World.Species=="PortiaLabiata" && host.Commands["feed"].Text=="Release a small spider","saved Portia selection restores prey behavior and tray action on startup");
            Check(afterCreation==foreground,"overlay creation preserves foreground window");
            var timer=new DispatcherTimer { Interval=TimeSpan.FromSeconds(2) };int stage=0,gdi=0,user=0;double startCpu=0,startTime=0;long startPrivate=0;
            var elapsed=Stopwatch.StartNew();var process=Process.GetCurrentProcess();
            timer.Tick+=delegate {
                try {
                    if(stage==0) {
                        CheckCursorVisibility(folder);
                        Check(host.Clicks.Installed,"read-only mouse button hook installed on Windows");
                        host.Clicks.Consume();host.Clicks.Record(0,0x201);Check(host.Clicks.Consume(),"button-down notification reaches the tick consumer");
                        Check(!host.Clicks.Consume(),"button notification is consumed once");
                        host.Clicks.Record(-1,0x201);host.Clicks.Record(0,0x202);host.Clicks.Record(0,0x200);
                        Check(!host.Clicks.Consume(),"ignored hook codes, releases and mouse movement do not act as clicks");
                        Check(MouseClicks.IsButtonDown(0x204) && MouseClicks.IsButtonDown(0x207) && MouseClicks.IsButtonDown(0x20b),"right, middle and extra button-down messages are recognized");
                        Check(host.Windows.Count==Forms.Screen.AllScreens.Length,"one overlay per connected monitor");
                        int right=0;for(int i=0;i<host.World.Areas.Count;i++) if(host.World.Areas[i].X>host.World.Areas[right].X) right=i;
                        Check(host.World.HomeArea==right,"default hammock is on rightmost screen");
                        foreach(var w in host.Windows) {
                            int style=Native.GetWindowLong(w.Handle,Native.ExStyle);Native.Rect r;Native.GetWindowRect(w.Handle,out r);var expected=w.Screen.WorkingArea;
                            Check((style&(Native.Transparent|Native.NoActivate|Native.ToolWindow|Native.Layered))==(Native.Transparent|Native.NoActivate|Native.ToolWindow|Native.Layered),"layered, click-through, nonactivating tool-window styles");
                            Check(r.Left==expected.Left && r.Top==expected.Top && r.Right==expected.Right && r.Bottom==expected.Bottom,"physical overlay rectangle matches "+w.Screen.DeviceName);
                            Check(Math.Abs(w.Canvas.DeviceScale-Native.Dpi(w.Screen))<.02,"WPF pixel transform matches monitor DPI");
                            log.AppendLine("DISPLAY: "+w.Screen.DeviceName+" "+expected+" DPI="+(w.Canvas.DeviceScale*96));
                        }
                        var hit=Native.WindowFromPoint(new Native.Point { X=(int)(host.Origin.X+host.World.Spider.X),Y=(int)(host.Origin.Y+host.World.Spider.Y) });bool own=false;foreach(var w in host.Windows) if(w.Handle==hit) own=true;
                        Check(!own,"OS hit testing passes through visible spider position");
                        host.World.FingerOrigin=host.World.Spider;host.World.FingerBlend=1;host.Render();
                        hit=Native.WindowFromPoint(new Native.Point { X=(int)(host.Origin.X+host.World.FingerOrigin.X+65*host.World.BodyScale),Y=(int)(host.Origin.Y+host.World.FingerOrigin.Y) });
                        own=false;foreach(var w in host.Windows) if(w.Handle==hit) own=true;
                        Check(!own,"OS hit testing passes through the illustrated finger");host.World.FingerBlend=0;host.Render();
                        host.Commands["pause"].PerformClick();Check(host.Paused,"pause tray handler");host.Commands["pause"].PerformClick();Check(!host.Paused,"resume tray handler");
                        host.Commands["feed"].PerformClick();Check(host.World.HasFly,"feed tray handler");host.Commands["home"].PerformClick();Check(host.World.State==Mood.Return,"home tray handler");
                        Check(host.Commands.Count==6 && host.Commands.ContainsKey("settings"),"tray contains quick actions and a single settings entry");
                        var settingsPanel=new PetSettingsWindow(host);
                        settingsPanel.CornerSelector.SelectedIndex=2;Check(host.World.Corner=="BottomLeft","settings corner selector");settingsPanel.CornerSelector.SelectedIndex=1;
                        settingsPanel.SizeSlider.Value=125;Check(Math.Abs(host.World.Scale-1.25)<.001,"live size slider updates the shared pet scale");
                        settingsPanel.SizeSlider.Value=180;Check(host.World.Scale==1.8,"size slider upper limit");
                        settingsPanel.SizeSlider.Value=20;Check(host.World.Scale==.2 && settingsPanel.Preview.PetScale==.2 && !settingsPanel.Preview.Enlarged,"size slider and preview accept 20 percent");
                        settingsPanel.FingerToneSlider.Value=0;Check(host.World.FingerTone==0 && settingsPanel.FingerPreview.Tone==0,"deep-brown finger endpoint updates world and preview");
                        settingsPanel.FingerToneSlider.Value=100;Check(host.World.FingerTone==1 && settingsPanel.FingerPreview.Tone==1,"very-pale finger endpoint updates world and preview");
                        settingsPanel.FingerToneSlider.Value=37;Check(host.World.FingerTone==.37 && settingsPanel.FingerPreview.Tone==.37,"intermediate finger shade updates live");
                        settingsPanel.WebSlider.Value=0;Check(host.World.WebVisibility==0 && settingsPanel.Preview.WebVisibility==0,"web slider can hide silk in world and preview");
                        settingsPanel.WebSlider.Value=100;Check(host.World.WebVisibility==1 && settingsPanel.Preview.WebVisibility==1,"100 percent retains original silk strength");
                        settingsPanel.WebSlider.Value=45;Check(host.World.WebVisibility==.45 && settingsPanel.Preview.WebVisibility==.45,"web visibility applies live to world and preview");
                        settingsPanel.AllMonitorsCheck.IsChecked=false;Check(host.Windows.Count==1,"settings single-monitor option");settingsPanel.AllMonitorsCheck.IsChecked=true;Check(host.Windows.Count==Forms.Screen.AllScreens.Length,"settings restore all-monitor roaming");
                        foreach(var skin in SkinCatalog.All) {
                            settingsPanel.SpeciesSelector.SelectedItem=skin;Check(host.Settings.Species==skin.Id && host.World.Species==skin.Id && settingsPanel.Preview.Species==skin.Id,"skin selection applies to pet, behavior and preview: "+skin.Id);
                            host.Save();Check(Settings.Load(host.Folder).Species==skin.Id,"selected skin survives native settings reload: "+skin.Id);
                        }
                        settingsPanel.FlySlider.Value=200;settingsPanel.InviteSlider.Value=10;settingsPanel.ToleranceSlider.Value=300;settingsPanel.AttentionSlider.Value=400;settingsPanel.DismissSlider.Value=80;
                        Check(host.World.FlyInterval==200 && host.World.InviteDelay==10 && host.World.CarryTolerance==300,"behavior sliders update actual world preferences");
                        Check(host.World.AttentionDistance==400 && host.World.DismissDistance==420 && settingsPanel.DismissSlider.Value==420,"distance sliders enforce a stable exit margin");
                        settingsPanel.FliesCheck.IsChecked=false;Check(!host.World.AutoFeed && !settingsPanel.FlySlider.IsEnabled,"flies toggle updates world and slider");
                        settingsPanel.RidesCheck.IsChecked=false;Check(!host.World.CursorPlay && !settingsPanel.InviteSlider.IsEnabled,"rides toggle updates world and sliders");
                        Check(!settingsPanel.StartupCheck.IsEnabled && !host.SetStartup(true),"diagnostics cannot alter autostart");
                        host.Save();var saved=Settings.Load(host.Folder);Check(saved.Species==((SkinProfile)settingsPanel.SpeciesSelector.SelectedItem).Id && saved.AllMonitors && saved.Scale==.2 && saved.InviteDelay==10 && saved.DismissDistance==420,"new settings persist through reload");
                        Check(saved.FingerTone==.37,"finger tone persists with other settings");
                        Check(saved.WebVisibility==.45,"web visibility persists through native UI");
                        double web=host.World.Web;int meals=host.World.Meals;settingsPanel.ResetPreferences();
                        Check(host.World.Scale==1 && host.World.InviteDelay==4.5 && host.World.AttentionDistance==185 && host.World.DismissDistance==230 && host.World.AutoFeed && host.World.CursorPlay && host.Settings.Species=="Regal","reset restores preference defaults");
                        Check(host.World.Web==web && host.World.Meals==meals,"preference reset preserves nest and meals");
                        Check(host.World.FingerTone==World.DefaultFingerTone && settingsPanel.FingerPreview.Tone==World.DefaultFingerTone,"reset restores original finger shade in world and preview");
                        Check(host.World.WebVisibility==1 && settingsPanel.Preview.WebVisibility==1,"reset restores original silk strength without resetting progress");
                        CaptureSettings(host,folder);settingsPanel.SizeSlider.Value=125;settingsPanel.Close();
                        Check(Settings.Load(host.Folder).Scale==1.25,"closing settings saves slider size");
                        Check(settingsPanel.IsClosed && !settingsPanel.Preview.Running,"closing settings stops its preview timer");host.World.SetScale(1);
                        CheckSkinRendering();CheckFingerTones();RenderSkinGallery(folder);RenderFingerPreview(folder);RenderHammockGallery(folder);
                        CheckPortiaPrey(host,folder);CheckEncounterSettings(host,folder);CheckClickDismount(host);
                        CheckNestEntry(host);CheckSilkArt(folder);
                        host.World.Cursor=host.World.FingerOrigin=host.World.Spider;host.World.InviteTime=5;host.World.FingerBlend=1;host.Render();
                        Check(host.CursorVisibility.WaitUntilHidden(1500),"visible centered finger requests native cursor hiding");
                        host.Commands["pause"].PerformClick();Check(host.CursorVisibility.WaitForRestore(1500),"pause releases the cursor visibility lease");host.Commands["pause"].PerformClick();
                        RenderPreview(Path.Combine(folder,"species-preview.png"));RenderBoldPreview(Path.Combine(folder,"bold-preview.png"));RenderOrientationPreview(Path.Combine(folder,"orientation-preview.png"));CheckTransparency();
                        host.World.Spider=host.World.Home;host.World.GoHome();
                        gdi=Native.GetGuiResources(process.Handle,0);user=Native.GetGuiResources(process.Handle,1);process.Refresh();startPrivate=process.PrivateMemorySize64;startCpu=process.TotalProcessorTime.TotalSeconds;startTime=elapsed.Elapsed.TotalSeconds;
                    } else if(stage==1) {
                        for(int i=0;i<60;i++) host.SetSpecies(Settings.SpeciesNames[i%Settings.SpeciesNames.Length]);
                        host.Settings.Species="Regal";host.Render();host.World.SpawnFly();
                    } else if(stage==2) {
                        // Measure steady operation after warmup and the icon/IO stress burst.
                        process.Refresh();startPrivate=process.PrivateMemorySize64;startCpu=process.TotalProcessorTime.TotalSeconds;startTime=elapsed.Elapsed.TotalSeconds;
                    } else if(stage==3 && host.World.Areas.Count==2) {
                        var left=host.World.Areas[0];var right=host.World.Areas[1];
                        double y=Math.Max(left.Y,right.Y)+Math.Min(left.H,right.H)/2;
                        host.World.HasFly=false;host.World.AutoFeed=false;host.World.CursorPlay=false;host.World.Spider=new V(left.X+left.W-45,y);host.World.Target=new V(right.X+110,y);host.World.State=Mood.Walk;
                    } else if(stage==6 && host.World.Areas.Count==2) {
                        Check(host.World.AreaAt(host.World.Spider)==1,"running desktop pet walks across the real two-monitor seam");
                    } else if(stage==10) {
                        process.Refresh();double duration=elapsed.Elapsed.TotalSeconds-startTime;
                        log.AppendLine("MEASURED: "+duration.ToString("F1")+" seconds, CPU="+((process.TotalProcessorTime.TotalSeconds-startCpu)/duration*100).ToString("F2")+"% of one logical CPU, working set="+(process.WorkingSet64/1048576.0).ToString("F1")+" MiB, private="+(process.PrivateMemorySize64/1048576.0).ToString("F1")+" MiB (delta "+((process.PrivateMemorySize64-startPrivate)/1048576.0).ToString("F1")+").");
                        int afterGdi=Native.GetGuiResources(process.Handle,0),afterUser=Native.GetGuiResources(process.Handle,1);
                        log.AppendLine("RESOURCES: GDI "+gdi+" -> "+afterGdi+", USER "+user+" -> "+afterUser+" after 60 icon replacements.");
                        Check(afterGdi<=gdi+4 && afterUser<=user+4,"replacing species icons does not leak native handles");
                        host.World.State=Mood.Ride;host.ResumeClock();Check(host.World.State==Mood.Jump && host.World.StillTime==0,"resume path resets cursor history and dismounts");
                        host.Save();Check(File.Exists(Path.Combine(host.Folder,"settings.txt.bak")),"previous-save backup exists");
                        host.World.CursorPlay=true;host.World.Cursor=host.World.FingerOrigin=host.World.Spider;host.World.InviteTime=5;host.World.FingerBlend=1;host.Render();
                        Check(host.CursorVisibility.WaitUntilHidden(1500),"exit cleanup tested with an active cursor visibility lease");
                        host.Dispose();Check(host.Disposed && host.Windows.Count==0 && !host.Tray.Visible,"normal cleanup removes overlays and tray icon");
                        Check(host.CursorVisibility.HelperId==0,"exit cleanup restores cursor and closes the helper");
                        Check(host.Clicks.Disposed && host.Clicks.Released && !host.Clicks.Installed,"cleanup successfully unhooks the native mouse observer");
                        Check(!host.Clicks.WorkerAlive,"cleanup stops the mouse observer message loop");
                        log.AppendLine("RESULT: "+(failures==0?"PASS":"FAIL")+" ("+failures+" failures). Physical mixed-DPI changes, actual sleep/unplug and hands-on cursor feel still require manual QA.");
                        File.WriteAllText(Path.Combine(folder,"runtime-tests.txt"),log.ToString());timer.Stop();app.Shutdown(failures==0?0:1);
                    }
                    stage++;
                } catch(Exception ex) { log.AppendLine(ex.ToString());File.WriteAllText(Path.Combine(folder,"runtime-tests.txt"),log.ToString());timer.Stop();host.Dispose();app.Shutdown(1); }
            };timer.Start();return app.Run();
        }
        static void CheckCursorVisibility(string folder) {
            using(var veil=new CursorVeil()) {
                Check(veil.HelperId==0,"no cursor helper runs while idle");
                veil.Update(true);Check(veil.WaitUntilHidden(1500),"Windows cursor-hide API acknowledged by the visibility helper");
                veil.Release();Check(veil.WaitForRestore(1500),"explicit release restores cursor through Windows API and exits helper");
            }
            using(var veil=new CursorVeil()) {
                veil.Update(true);Check(veil.WaitUntilHidden(1500),"cursor lease can start again");
                Check(veil.WaitForRestore(2000),"lost UI heartbeat restores cursor and exits helper");
            }
            string signal=Path.Combine(folder,"cursor-fixture-"+Guid.NewGuid().ToString("N")+".txt");
            Process fixture=null,guard=null;
            try {
                fixture=Process.Start(new ProcessStartInfo(Process.GetCurrentProcess().MainModule.FileName,"--cursor-lease-fixture \""+signal+"\"") { UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden });
                var watch=Stopwatch.StartNew();while(!File.Exists(signal) && !fixture.HasExited && watch.ElapsedMilliseconds<3000) System.Threading.Thread.Sleep(20);
                Check(File.Exists(signal),"crash-recovery fixture acquired a cursor lease");
                if(File.Exists(signal)) {
                    guard=Process.GetProcessById(int.Parse(File.ReadAllText(signal)));
                    IntPtr guardHandle=guard.Handle;
                    fixture.Kill();fixture.WaitForExit(1500);
                    uint exitCode;Check(guard.WaitForExit(2000) && Native.GetExitCodeProcess(guardHandle,out exitCode) && exitCode==0,"abrupt owner termination restores cursor and leaves no guard process");
                }
            } finally {
                if(fixture!=null) { if(!fixture.HasExited) { fixture.Kill();fixture.WaitForExit(1500); }fixture.Dispose(); }
                if(guard!=null) { if(!guard.WaitForExit(2000)) { guard.Kill();CursorVeil.RestoreEmergency(); }guard.Dispose(); }
            }
        }
        static void CheckTransparency() {
            var w=new World(400,250,42);var canvas=new PetCanvas { World=w,Width=400,Height=250 };canvas.Measure(new Size(400,250));canvas.Arrange(new Rect(0,0,400,250));
            var bitmap=new RenderTargetBitmap(400,250,96,96,PixelFormats.Pbgra32);bitmap.Render(canvas);byte[] pixels=new byte[400*250*4];bitmap.CopyPixels(pixels,1600,0);int clear=0,opaque=0;
            for(int i=3;i<pixels.Length;i+=4) { if(pixels[i]==0) clear++;if(pixels[i]>220) opaque++; }
            Check(clear>90000 && opaque>100,"rendered overlay has transparent background and visible vector artwork");
        }
        public static void RenderPreview(string path) {
            var drawing=new DrawingVisual();using(var d=drawing.RenderOpen()) {
                d.DrawRectangle(new SolidColorBrush(Color.FromRgb(25,34,43)),null,new Rect(0,0,1000,430));
                Text(d,"CORNER SPIDER",34,28,25,Color.FromRgb(234,234,218));Text(d,"Three jumping-spider coats  /  procedural vector artwork",34,63,14,Color.FromRgb(155,178,183));
                for(int i=0;i<3;i++) {
                    var w=new World(1000,430,42);w.Scale=2.5;w.Spider=new V(175+i*325,218);w.State=Mood.Rest;w.Angle=0;
                    PetCanvas.DrawSpider(d,w,Settings.SpeciesNames[i]);Text(d,Settings.SpeciesNames[i],122+i*325,333,24,Color.FromRgb(234,234,218));
                    Text(d,i==0?"warm cinnamon + violet":i==1?"black + emerald green":"silver bands + dark jaws",88+i*325,371,14,Color.FromRgb(155,178,183));
                }
            }
            var bitmap=new RenderTargetBitmap(1000,430,96,96,PixelFormats.Pbgra32);bitmap.Render(drawing);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));using(var file=File.Create(path)) png.Save(file);
        }
        public static void RenderBoldPreview(string path) {
            var drawing=new DrawingVisual();using(var d=drawing.RenderOpen()) {
                d.DrawRectangle(new SolidColorBrush(Color.FromRgb(229,236,231)),null,new Rect(0,0,550,370));
                Text(d,"Bold jumping spider",26,18,26,Color.FromRgb(25,43,40));
                Text(d,"Phidippus audax  /  Missouri-inspired skin",28,54,14,Color.FromRgb(72,97,89));
                var w=new World(550,370,42);w.Scale=3;w.Spider=new V(275,202);w.State=Mood.Rest;
                PetCanvas.DrawSpider(d,w,"Bold");
                Text(d,"Black velvet coat  -  pale spots  -  emerald mouthparts",54,336,14,Color.FromRgb(49,83,70));
            }
            var bitmap=new RenderTargetBitmap(550,370,96,96,PixelFormats.Pbgra32);bitmap.Render(drawing);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));using(var file=File.Create(path)) png.Save(file);
        }
        public static void RenderOrientationPreview(string path) {
            var drawing=new DrawingVisual();using(var d=drawing.RenderOpen()) {
                var ink=Color.FromRgb(25,43,40);
                d.DrawRectangle(new SolidColorBrush(Color.FromRgb(229,236,231)),null,new Rect(0,0,960,1070));
                Text(d,"Face direction and pose checks",24,14,24,ink);
                string[] directions={"Facing up","Facing right","Facing down","Facing left"};
                for(int row=0;row<4;row++) for(int col=0;col<4;col++) {
                    var w=new World(960,1070,42);w.Scale=2;w.Spider=new V(120+col*240,155+row*255);w.Clock=.4;w.StateTime=.4;
                    string species=row<3?Settings.SpeciesNames[row]:"Bold";
                    w.Angle=row<3?col*90:0;
                    w.State=row<3?Mood.Rest:new[]{Mood.Groom,Mood.Eat,Mood.Jump,Mood.Sleep}[col];
                    if(w.State==Mood.Jump) { w.JumpTime=.24;w.Spider+=new V(0,w.Lift); }
                    PetCanvas.DrawSpider(d,w,species);
                    Text(d,row<3?species+" / "+directions[col]:"Bold / "+w.State,24+col*240,278+row*255,16,ink);
                }
            }
            var bitmap=new RenderTargetBitmap(960,1070,96,96,PixelFormats.Pbgra32);bitmap.Render(drawing);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));using(var file=File.Create(path)) png.Save(file);
        }
        public static void RenderMotionPreview(string folder) {
            var poses=new[]{Mood.Rest,Mood.Inspect,Mood.Reach,Mood.Crouch,Mood.Jump,Mood.Land,Mood.Walk,Mood.Groom};
            var visual=new DrawingVisual();using(var d=visual.RenderOpen()) {
                d.DrawRectangle(new SolidColorBrush(Color.FromRgb(229,236,231)),null,new Rect(0,0,1000,650));
                Text(d,"ARTICULATED POSES / same rig as the desktop pet",24,18,22,Color.FromRgb(25,43,40));
                for(int i=0;i<poses.Length;i++) {
                    var w=new World(1000,650,1);w.Scale=2.3;w.Spider=new V(125+(i%4)*250,205+(i/4)*300);
                    w.State=poses[i];w.StateTime=w.State==Mood.Inspect?.45:.23;w.Clock=.7;
                    if(w.State==Mood.Jump) { w.JumpTime=.13;w.Spider+=new V(0,w.Lift); }
                    if(w.State==Mood.Walk) { w.Speed=120;w.GaitBlend=1;w.StepPhase=1.1; }
                    PetCanvas.DrawSpider(d,w,"Bold");Text(d,w.State.ToString(),88+(i%4)*250,310+(i/4)*300,17,Color.FromRgb(25,43,40));
                }
            }
            SavePng(visual,1000,650,Path.Combine(folder,"motion-poses.png"));
            // Consecutive frames use the actual behavior and drawing code. They can
            // be encoded into a GIF for timing review without screen capture.
            string frames=Path.Combine(folder,"motion-frames");Directory.CreateDirectory(frames);
            var world=new World(640,400,42);world.AutoFeed=false;world.Scale=2;world.Spider=new V(320,230);
            var csv=new StringBuilder("frame,state,x,y,speed,frontLeft,frontRight\n");
            for(int frame=0;frame<400;frame++) {
                V cursor=frame<50?new V(320,125):new V(-10000,-10000);
                if(frame==65) { world.SpawnFly();world.Fly=new V(400,155); }
                if(frame==260) { world.HasFly=false;world.State=Mood.Walk;world.Target=new V(150,280); }
                world.Tick(.04,cursor);
                var v=new DrawingVisual();using(var d=v.RenderOpen()) {
                    d.DrawRectangle(new SolidColorBrush(Color.FromRgb(229,236,231)),null,new Rect(0,0,640,400));
                    Text(d,"Corner Spider 0.5 / "+world.State,18,14,19,Color.FromRgb(25,43,40));
                    var canvas=new PetCanvas { World=world,Species="Bold",Width=640,Height=400 };
                    canvas.Measure(new Size(640,400));canvas.Arrange(new Rect(0,0,640,400));
                    var layer=new RenderTargetBitmap(640,400,96,96,PixelFormats.Pbgra32);layer.Render(canvas);
                    d.DrawImage(layer,new Rect(0,0,640,400));
                    if(frame<50) { d.DrawLine(new Pen(Brushes.SlateGray,1),new Point(316,125),new Point(324,125));d.DrawLine(new Pen(Brushes.SlateGray,1),new Point(320,121),new Point(320,129)); }
                }
                SavePng(v,640,400,Path.Combine(frames,frame.ToString("D4")+".png"));
                csv.AppendLine(frame+","+world.State+","+world.Spider.X.ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+","+world.Spider.Y.ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+","+world.Speed.ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+","+world.FrontReach(-1).ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+","+world.FrontReach(1).ToString("F2",System.Globalization.CultureInfo.InvariantCulture));
            }
            File.WriteAllText(Path.Combine(folder,"motion-trace.csv"),csv.ToString());
        }
        static void SavePng(Visual visual,int width,int height,string path) {
            var bitmap=new RenderTargetBitmap(width,height,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);
            var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using(var file=File.Create(path)) encoder.Save(file);
        }
        static void Text(DrawingContext d,string text,double x,double y,double size,Color color) { d.DrawText(new FormattedText(text,System.Globalization.CultureInfo.InvariantCulture,FlowDirection.LeftToRight,new Typeface("Segoe UI"),size,new SolidColorBrush(color),1),new Point(x,y)); }
        static int Desktop(Application app,string folder) {
            var fixture=new Window { Title="Corner Spider desktop verification",Width=720,Height=440,WindowStartupLocation=WindowStartupLocation.CenterScreen,Background=new SolidColorBrush(Color.FromRgb(28,37,47)) };
            var panel=new StackPanel { Margin=new Thickness(30) };fixture.Content=panel;
            panel.Children.Add(new TextBlock { Text="Corner Spider desktop verification",FontSize=24,Foreground=Brushes.White,Margin=new Thickness(0,0,0,16) });
            var status=new TextBlock { Text="Click the button beneath the spider to check click-through.",Foreground=Brushes.White,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,20) };panel.Children.Add(status);
            var button=new Button { Content="Click through the spider",Height=90,FontSize=20,Margin=new Thickness(0,0,0,12) };panel.Children.Add(button);
            var menuButton=new Button { Content="Open the actual tray menu",Height=36,Margin=new Thickness(0,0,0,12) };panel.Children.Add(menuButton);
            var ride=new Button { Content="Start cursor ride check",Height=36,Margin=new Thickness(0,0,0,12) };panel.Children.Add(ride);
            PetHost host=null;DispatcherTimer watch=null;int clicks=0;
            fixture.Loaded+=delegate {
                host=new PetHost(Path.Combine(folder,"desktop-settings"),true);host.Paused=true;
                Point screen=button.PointToScreen(new Point(button.ActualWidth/2,button.ActualHeight/2));host.World.Spider=new V(screen.X-host.Origin.X,screen.Y-host.Origin.Y);host.World.SetScale(1.8);host.World.State=Mood.Rest;host.Render();
                watch=new DispatcherTimer { Interval=TimeSpan.FromMilliseconds(250) };watch.Tick+=delegate { if(!host.Paused) { status.Text="State: "+host.World.State+" | stillness: "+host.World.StillTime.ToString("F1")+" s | clicks passed: "+clicks;File.WriteAllText(Path.Combine(folder,"desktop-state.txt"),status.Text); } };watch.Start();
            };
            button.Click+=delegate { clicks++;status.Text="PASS: click reached the underlying button. Count: "+clicks;File.WriteAllText(Path.Combine(folder,"click-through.txt"),"PASS: underlying WPF button received "+clicks+" click(s) through the visible spider. Foreground="+Native.GetForegroundWindow()); };
            menuButton.Click+=delegate { Native.Point p;Native.GetCursorPos(out p);host.Menu.Show(p.X,p.Y); };
            ride.Click+=delegate { Native.Point p;Native.GetCursorPos(out p);host.World.Spider=new V(p.X-host.Origin.X-75,p.Y-host.Origin.Y);host.World.AutoFeed=false;host.World.CursorPlay=true;host.World.Wake();host.World.ResetCursor();host.Paused=false;host.ResumeClock(); };
            fixture.Closed+=delegate { if(watch!=null) watch.Stop();if(host!=null) host.Dispose();app.Shutdown(); };fixture.Show();return app.Run();
        }
    }
}
