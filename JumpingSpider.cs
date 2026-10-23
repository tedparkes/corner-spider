using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Interop;
using Forms=System.Windows.Forms;

[assembly:System.Runtime.Versioning.TargetFramework(".NETFramework,Version=v4.8")]
[assembly:System.Reflection.AssemblyVersion("1.0.0.0")]
[assembly:System.Reflection.AssemblyFileVersion("1.0.0.0")]
[assembly:System.Reflection.AssemblyInformationalVersion("1.0.0-rc.9")]
[assembly:System.Reflection.AssemblyTitle("Corner Spider")]
[assembly:System.Reflection.AssemblyProduct("Corner Spider")]
[assembly:System.Reflection.AssemblyDescription("A local jumping-spider companion for the Windows desktop.")]
[assembly:System.Reflection.AssemblyCopyright("Copyright (c) 2026 Corner Spider contributors")]
namespace CornerSpider {
    public static class Native {
        [StructLayout(LayoutKind.Sequential)] public struct Point { public int X,Y; }
        [StructLayout(LayoutKind.Sequential)] public struct Rect { public int Left,Top,Right,Bottom; }
        public delegate bool EnumProc(IntPtr h,IntPtr p);
        [DllImport("user32.dll")] public static extern int GetWindowLong(IntPtr h,int index);
        [DllImport("user32.dll")] public static extern int SetWindowLong(IntPtr h,int index,int value);
        [DllImport("user32.dll")] public static extern bool GetCursorPos(out Point p);
        [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr h,IntPtr after,int x,int y,int w,int height,uint flags);
        [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h,out Rect rect);
        [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] public static extern IntPtr WindowFromPoint(Point p);
        [DllImport("user32.dll")] public static extern IntPtr MonitorFromPoint(Point p,uint flags);
        [DllImport("shcore.dll")] public static extern int GetDpiForMonitor(IntPtr h,int type,out uint x,out uint y);
        [DllImport("user32.dll")] public static extern bool DestroyIcon(IntPtr h);
        [DllImport("user32.dll")] public static extern int GetGuiResources(IntPtr process,int flags);
        [DllImport("user32.dll")] public static extern bool EnumWindows(EnumProc callback,IntPtr p);
        [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h,out uint pid);
        [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr h,uint message,IntPtr w,IntPtr l);
        [DllImport("kernel32.dll")] public static extern bool GetExitCodeProcess(IntPtr process,out uint code);
        public const int ExStyle=-20,Transparent=0x20,ToolWindow=0x80,NoActivate=0x08000000,Layered=0x80000;
        public static double Dpi(Forms.Screen screen) {
            uint x,y;try { var p=new Point { X=screen.Bounds.Left+screen.Bounds.Width/2,Y=screen.Bounds.Top+screen.Bounds.Height/2 };
                if(GetDpiForMonitor(MonitorFromPoint(p,2),0,out x,out y)==0) return x/96.0;
            } catch(DllNotFoundException) {} catch(EntryPointNotFoundException) {}return 1;
        }
    }
    public class PetWindow : Window {
        public readonly PetCanvas Canvas;
        public readonly Forms.Screen Screen;
        readonly PetHost host;
        HwndSource source;
        public IntPtr Handle { get { return new WindowInteropHelper(this).Handle; } }
        public PetWindow(PetHost owner,Forms.Screen screen) {
            host=owner;Screen=screen;Title="Corner Spider";
            WindowStyle=WindowStyle.None;ResizeMode=ResizeMode.NoResize;AllowsTransparency=true;
            Background=Brushes.Transparent;Topmost=true;ShowInTaskbar=false;ShowActivated=false;Focusable=false;IsHitTestVisible=false;
            Canvas=new PetCanvas { World=host.World,Species=host.Settings.Species };Content=Canvas;
            // Set a small initial rectangle, then place in physical pixels after the HWND exists.
            Width=100;Height=100;
            SourceInitialized+=delegate {
                source=(HwndSource)PresentationSource.FromVisual(this);
                Native.SetWindowLong(Handle,Native.ExStyle,Native.GetWindowLong(Handle,Native.ExStyle)|Native.Transparent|Native.ToolWindow|Native.NoActivate);
                source.AddHook(Hook);Place();
            };
            SizeChanged+=delegate { SyncCanvas(); };
            Closed+=delegate { if(source!=null) source.RemoveHook(Hook); };
        }
        IntPtr Hook(IntPtr h,int msg,IntPtr w,IntPtr l,ref bool handled) {
            if(msg==0x21) { handled=true;return new IntPtr(3); } // MA_NOACTIVATE
            if(msg==0x84) { handled=true;return new IntPtr(-1); } // HTTRANSPARENT (layered style handles other threads)
            if(msg==0x2e0) host.QueueDisplayRefresh();
            if(msg==0x1a) host.QueueDisplayRefresh(); // taskbar/work-area changes
            return IntPtr.Zero;
        }
        public void Place() {
            var r=Screen.WorkingArea;
            Native.SetWindowPos(Handle,new IntPtr(-1),r.Left,r.Top,r.Width,r.Height,0x10|0x40);
            SyncCanvas();
        }
        public void SyncCanvas() {
            if(source==null || source.CompositionTarget==null) return;
            Canvas.DeviceScale=source.CompositionTarget.TransformToDevice.M11;
            Canvas.Offset=new V(Screen.WorkingArea.Left-host.Origin.X,Screen.WorkingArea.Top-host.Origin.Y);
            Canvas.InvalidateVisual();
        }
    }
    public class PetHost : IDisposable {
        public readonly World World;
        public readonly Settings Settings;
        public readonly List<PetWindow> Windows=new List<PetWindow>();
        public V Origin;
        public bool Paused,Disposed;
        public Forms.ContextMenuStrip Menu;
        public Forms.NotifyIcon Tray;
        public string Folder;
        public readonly Dictionary<string,Forms.ToolStripMenuItem> Commands=new Dictionary<string,Forms.ToolStripMenuItem>();
        readonly Dispatcher dispatcher;
        readonly DispatcherTimer timer=new DispatcherTimer(),displayTimer=new DispatcherTimer();
        readonly Stopwatch clock=Stopwatch.StartNew();
        System.Drawing.Icon icon;
        double lastTime,lastSave;
        V previousSpider,previousFly,previousHome;
        bool previousHasFly,previousFinger,previousSilk,previousGuest;
        V previousGuestCenter;
        int previousWeb=-1;
        double previousScale;
        string previousSpecies="";
        bool suspended,sessionLocked;
        public bool TestMode;
        public PetSettingsWindow SettingsWindow;
        public readonly MouseClicks Clicks=new MouseClicks();
        public readonly CursorVeil CursorVisibility=new CursorVeil();
        public PetHost(string folder,bool testMode) {
            Folder=folder;TestMode=testMode;dispatcher=Dispatcher.CurrentDispatcher;Settings=Settings.Load(folder);
            World=new World(1280,720,Environment.TickCount);
            World.SetSpecies(Settings.Species);
            World.Scale=Settings.Scale;World.Web=Settings.Web;World.Meals=Settings.Meals;World.Corner=Settings.Corner;World.AutoFeed=Settings.AutoFeed;World.CursorPlay=Settings.CursorPlay;
            World.SpeciesTraits=Settings.SpeciesTraits;World.WebVisits=Settings.WebVisits;
            World.FlyInterval=Settings.FlyInterval;World.InviteDelay=Settings.InviteDelay;World.CarryTolerance=Settings.CarryTolerance;
            World.FingerTone=Settings.FingerTone;
            World.AttentionDistance=Settings.AttentionDistance;World.DismissDistance=Settings.DismissDistance;World.NextFly=Math.Min(35,World.FlyInterval);
            BuildTray();RefreshDisplays();World.Spider=World.Home;
            timer.Interval=TimeSpan.FromMilliseconds(33);timer.Tick+=OnTick;
            displayTimer.Interval=TimeSpan.FromMilliseconds(350);displayTimer.Tick+=delegate { displayTimer.Stop();if(!Disposed) RefreshDisplays(); };
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged+=DisplayChanged;
            Microsoft.Win32.SystemEvents.PowerModeChanged+=PowerChanged;
            Microsoft.Win32.SystemEvents.SessionSwitch+=SessionChanged;
            Application.Current.SessionEnding+=SessionEnding;
            lastTime=clock.Elapsed.TotalSeconds;timer.Start();
        }
        public void QueueDisplayRefresh() { if(Disposed) return;dispatcher.BeginInvoke(new Action(delegate { if(!Disposed) { displayTimer.Stop();displayTimer.Start(); } })); }
        void DisplayChanged(object sender,EventArgs e) { QueueDisplayRefresh(); }
        void PowerChanged(object sender,Microsoft.Win32.PowerModeChangedEventArgs e) {
            dispatcher.BeginInvoke(new Action(delegate { if(Disposed) return;
                if(e.Mode==Microsoft.Win32.PowerModes.Suspend) { suspended=true;CursorVisibility.Release();timer.Stop();Save(); }
                else if(e.Mode==Microsoft.Win32.PowerModes.Resume) { suspended=false;ResumeClock();QueueDisplayRefresh(); }
            }));
        }
        void SessionChanged(object sender,Microsoft.Win32.SessionSwitchEventArgs e) {
            dispatcher.BeginInvoke(new Action(delegate { if(Disposed) return;
                if(e.Reason==Microsoft.Win32.SessionSwitchReason.SessionLock) { sessionLocked=true;CursorVisibility.Release();timer.Stop();Save(); }
                else if(e.Reason==Microsoft.Win32.SessionSwitchReason.SessionUnlock) { sessionLocked=false;ResumeClock();QueueDisplayRefresh(); }
            }));
        }
        void SessionEnding(object sender,SessionEndingCancelEventArgs e) { CursorVisibility.Release();Save(); }
        public void ResumeClock() { CursorVisibility.Release();Clicks.Consume();World.Resume();lastTime=clock.Elapsed.TotalSeconds;if(!suspended && !sessionLocked) timer.Start(); }
        public void RefreshDisplays() {
            if(Disposed) return;
            var screens=Forms.Screen.AllScreens;if(screens.Length==0) return;
            Array.Sort(screens,delegate(Forms.Screen a,Forms.Screen b) { return a.Bounds.Left.CompareTo(b.Bounds.Left); });
            Forms.Screen home=screens[screens.Length-1];
            foreach(var s in screens) if(s.DeviceName==Settings.HomeMonitor) home=s;
            var selected=new List<Forms.Screen>();if(Settings.AllMonitors) selected.AddRange(screens);else selected.Add(home);
            double minX=double.MaxValue,minY=double.MaxValue;foreach(var s in selected) { minX=Math.Min(minX,s.WorkingArea.Left);minY=Math.Min(minY,s.WorkingArea.Top); }
            V oldOrigin=Origin;Origin=new V(minX,minY);
            World.Spider+=oldOrigin-Origin;World.Target+=oldOrigin-Origin;World.Fly+=oldOrigin-Origin;
            var areas=new List<Area>();int homeIndex=0;
            for(int i=0;i<selected.Count;i++) { var s=selected[i];var r=s.WorkingArea;areas.Add(new Area(r.Left-Origin.X,r.Top-Origin.Y,r.Width,r.Height,Native.Dpi(s)));if(s.DeviceName==home.DeviceName) homeIndex=i; }
            World.ConfigureDisplays(areas.ToArray(),homeIndex);
            // Reuse existing HWNDs when possible. A display-change event never activates a window.
            var removed=new List<PetWindow>(Windows);
            foreach(var s in selected) {
                PetWindow found=null;
                foreach(var w in Windows) if(w.Screen.DeviceName==s.DeviceName && w.Screen.WorkingArea==s.WorkingArea) { found=w;break; }
                if(found!=null) { removed.Remove(found);found.Place(); }
                else { var w=new PetWindow(this,s);Windows.Add(w);w.Show(); }
            }
            foreach(var w in removed) { Windows.Remove(w);w.Close(); }
            foreach(var w in Windows) w.SyncCanvas();
            previousWeb=-1;ResumeClock();Render();
        }
        void OnTick(object sender,EventArgs e) {
            double now=clock.Elapsed.TotalSeconds,dt=now-lastTime;lastTime=now;
            bool clicked=Clicks.Consume();
            if(!Paused && !suspended && !sessionLocked) {
                Native.Point p;V cursor=new V(-10000,-10000);
                if((SettingsWindow==null || !SettingsWindow.IsVisible) && !Menu.Visible && Native.GetCursorPos(out p)) cursor=new V(p.X-Origin.X,p.Y-Origin.Y);
                World.Tick(dt,cursor,clicked);Render();
            }
            if(now-lastSave>30) { Save();lastSave=now; }
            double ms=Paused?250:World.State==Mood.Sleep?83:World.State==Mood.Rest || World.State==Mood.Groom?50:33;
            if(Math.Abs(timer.Interval.TotalMilliseconds-ms)>1) timer.Interval=TimeSpan.FromMilliseconds(ms);
        }
        public void Render() {
            CursorVisibility.Update(!Paused && !suspended && !sessionLocked && World.CursorPlay && World.FingerActive && World.FingerBlend>=.65 && (World.Cursor-World.FingerOrigin).Length<12*World.BodyScale && !Menu.Visible && (SettingsWindow==null || !SettingsWindow.IsVisible));
            V spider=new V(World.Spider.X,World.Spider.Y-World.Lift);
            int web=(int)(World.Web*30);
            bool all=web!=previousWeb || (World.Home-previousHome).Length>0 || World.Scale!=previousScale || Settings.Species!=previousSpecies || World.State==Mood.Jump || World.FingerBlend>0 || previousFinger || World.SilkOpacity>0 || previousSilk;
            double margin=130;
            foreach(var area in World.Areas) margin=Math.Max(margin,SpiderPose.Radius*World.Scale*area.Dpi+8);
            foreach(var w in Windows) {
                var r=w.Screen.WorkingArea;
                var visible=new Rect(r.Left-Origin.X-margin,r.Top-Origin.Y-margin,r.Width+margin*2,r.Height+margin*2);
                bool animate=visible.Contains(new Point(spider.X,spider.Y)) || visible.Contains(new Point(previousSpider.X,previousSpider.Y));
                if(World.HasFly) animate|=visible.Contains(new Point(World.Fly.X,World.Fly.Y));
                if(World.GuestOpacity>0) animate|=visible.Contains(new Point(World.GuestCenter.X,World.GuestCenter.Y));
                if(previousGuest) animate|=visible.Contains(new Point(previousGuestCenter.X,previousGuestCenter.Y));
                if(previousHasFly) animate|=visible.Contains(new Point(previousFly.X,previousFly.Y));
                w.Canvas.Species=Settings.Species;if(all || animate) w.Canvas.InvalidateVisual();
            }
            previousSpider=spider;previousFly=World.Fly;previousHasFly=World.HasFly;previousHome=World.Home;previousWeb=web;previousScale=World.Scale;previousSpecies=Settings.Species;
            previousGuest=World.GuestOpacity>0;previousGuestCenter=World.GuestCenter;
            previousFinger=World.FingerBlend>0;previousSilk=World.SilkOpacity>0;
        }
        public void Save() {
            Settings.Scale=World.Scale;Settings.Corner=World.Corner;Settings.Web=World.Web;Settings.Meals=World.Meals;
            Settings.SpeciesTraits=World.SpeciesTraits;Settings.WebVisits=World.WebVisits;
            Settings.AutoFeed=World.AutoFeed;Settings.CursorPlay=World.CursorPlay;
            Settings.FlyInterval=World.FlyInterval;Settings.InviteDelay=World.InviteDelay;Settings.CarryTolerance=World.CarryTolerance;
            Settings.FingerTone=World.FingerTone;
            Settings.AttentionDistance=World.AttentionDistance;Settings.DismissDistance=World.DismissDistance;Settings.Save(Folder);
        }
        Forms.ToolStripMenuItem Item(string id,string text,Action action) {
            var i=new Forms.ToolStripMenuItem(text);i.Click+=delegate { action();Render(); };Commands[id]=i;return i;
        }
        public void SetSpecies(string species) { if(!SkinCatalog.Contains(species)) return;Settings.Species=species;World.SetSpecies(species);Commands["feed"].Text=World.HuntsSpiders?"Release a small spider":"Release a fly";UpdateIcon();Save();Render(); }
        public void SetCorner(string corner) { if(Array.IndexOf(Settings.Corners,corner)<0) return;World.Corner=corner;World.UpdateHome();World.GoHome();Save();Render(); }
        public void SetAllMonitors(bool all) { Settings.AllMonitors=all;RefreshDisplays();Save(); }
        public void SetHomeMonitor(string monitor) { Settings.HomeMonitor=monitor;RefreshDisplays();World.GoHome();Save(); }
        public void SetCursorPlay(bool enabled) { World.CursorPlay=enabled;if(!enabled) World.Resume();Save();Render(); }
        public bool SetStartup(bool enabled) {
            if(TestMode) return false;
            try { using(var key=Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run")) {
                if(enabled) key.SetValue("CornerSpider","\""+System.Reflection.Assembly.GetExecutingAssembly().Location+"\"");else key.DeleteValue("CornerSpider",false);
            }return true; } catch(Exception ex) { Forms.MessageBox.Show("Couldn't change startup: "+ex.Message,"Corner Spider");return false; }
        }
        public void OpenSettings() {
            if(SettingsWindow==null) {
                CursorVisibility.Release();World.Resume();SettingsWindow=new PetSettingsWindow(this);SettingsWindow.Closed+=delegate { SettingsWindow=null; };SettingsWindow.Show();
            } else SettingsWindow.Activate();
        }
        void UpdateIcon() {
            var old=icon;icon=MakeIcon(Settings.Species);if(Tray!=null) Tray.Icon=icon;if(old!=null) old.Dispose();
        }
        public static System.Drawing.Icon MakeIcon(string species) {
            // Small, native tray silhouette: avoid allocating WPF render targets for
            // each icon replacement. Every bitmap, pen, icon and borrowed HICON is freed.
            using(var bitmap=new System.Drawing.Bitmap(32,32)) {
                using(var g=System.Drawing.Graphics.FromImage(bitmap)) {
                    g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    var palette=SkinCatalog.Get(species);var color=System.Drawing.ColorTranslator.FromHtml(palette.Hair);
                    using(var pen=new System.Drawing.Pen(color,1.5f)) using(var fill=new System.Drawing.SolidBrush(color)) {
                        for(int side=-1;side<=1;side+=2) for(int leg=0;leg<4;leg++) {
                            float y=8+leg*2.5f;
                            g.DrawLines(pen,new[]{new System.Drawing.PointF(16+side*5,y),new System.Drawing.PointF(16+side*11,y+(leg<2?-2:2)),new System.Drawing.PointF(16+side*12,leg<2?2+leg*5:18+(leg-2)*7)});
                        }
                        using(var bodyBrush=new System.Drawing.SolidBrush(System.Drawing.ColorTranslator.FromHtml(palette.Body))) g.FillEllipse(bodyBrush,11,18,10,13);
                        using(var headBrush=new System.Drawing.SolidBrush(System.Drawing.ColorTranslator.FromHtml(palette.Head))) g.FillEllipse(headBrush,7,5,18,16);
                        g.DrawEllipse(pen,11,18,10,13);g.DrawEllipse(pen,7,5,18,16);
                        g.FillEllipse(System.Drawing.Brushes.Black,10,6,5,5);g.FillEllipse(System.Drawing.Brushes.Black,17,6,5,5);
                        g.FillEllipse(System.Drawing.Brushes.WhiteSmoke,11,7,1.5f,1.2f);g.FillEllipse(System.Drawing.Brushes.WhiteSmoke,18,7,1.5f,1.2f);
                        if(palette.Pattern=="Spots" || palette.Pattern=="Regal" || palette.Pattern=="Emerald") { g.FillEllipse(System.Drawing.Brushes.WhiteSmoke,14,22,4,3);g.FillEllipse(System.Drawing.Brushes.WhiteSmoke,12,27,2,2);g.FillEllipse(System.Drawing.Brushes.WhiteSmoke,18,27,2,2); }
                        if(palette.Pattern=="Stripe") g.FillRectangle(System.Drawing.Brushes.WhiteSmoke,15,19,2,10);
                        if(palette.Pattern=="Tan" || palette.Pattern=="Wall") { g.DrawLine(System.Drawing.Pens.DimGray,12,24,16,26);g.DrawLine(System.Drawing.Pens.DimGray,16,26,20,24); }
                        using(var mouthBrush=new System.Drawing.SolidBrush(System.Drawing.ColorTranslator.FromHtml(palette.Mouth))) { g.FillEllipse(mouthBrush,14,3,2,3);g.FillEllipse(mouthBrush,17,3,2,3); }
                        g.FillEllipse(System.Drawing.Brushes.Silver,11,2,2,4);g.FillEllipse(System.Drawing.Brushes.Silver,20,2,2,4);
                        if(palette.Pattern=="Zebra") { g.DrawLine(System.Drawing.Pens.WhiteSmoke,12,23,20,23);g.DrawLine(System.Drawing.Pens.WhiteSmoke,12,27,20,27); }
                    }
                }
                IntPtr handle=bitmap.GetHicon();try { using(var borrowed=System.Drawing.Icon.FromHandle(handle)) return (System.Drawing.Icon)borrowed.Clone(); } finally { Native.DestroyIcon(handle); }
            }
        }
        void BuildTray() {
            Menu=new Forms.ContextMenuStrip();Menu.Opening+=delegate { CursorVisibility.Release(); };Menu.Items.Add(new Forms.ToolStripMenuItem("CORNER SPIDER  1.0 RC9") { Enabled=false });
            var status=new Forms.ToolStripMenuItem { Enabled=false };Menu.Items.Add(status);
            Menu.Opening+=delegate { status.Text=(World.GuestStatus.Length>0?World.GuestStatus:World.State.ToString())+"  |  "+World.Meals+" prey caught"+(Settings.LastError.Length>0?"  |  Save failed":""); };
            Menu.Items.Add(new Forms.ToolStripSeparator());
            var feed=Item("feed",World.HuntsSpiders?"Release a small spider":"Release a fly",delegate { World.Feed(); });Menu.Items.Add(feed);Menu.Opening+=delegate { feed.Enabled=!World.HasFly && !Paused; };
            Menu.Items.Add(Item("home","Go to hammock",delegate { World.GoHome(); }));
            Menu.Items.Add(Item("wake","Wake up",delegate { World.Wake(); }));
            var pause=Item("pause","Pause",delegate { Paused=!Paused;ResumeClock(); });Menu.Items.Add(pause);Menu.Opening+=delegate { pause.Text=Paused?"Resume":"Pause"; };
            Menu.Items.Add(new Forms.ToolStripSeparator());
            Menu.Items.Add(Item("settings","Settings...",OpenSettings));
            Menu.Items.Add(new Forms.ToolStripSeparator());
            Menu.Items.Add(Item("quit","Quit",delegate { Dispose();Application.Current.Shutdown(); }));
            UpdateIcon();Tray=new Forms.NotifyIcon { Text="Corner Spider - right-click for controls",Icon=icon,ContextMenuStrip=Menu,Visible=true };
            Tray.DoubleClick+=delegate { OpenSettings(); };
        }
        public static bool ReadStartup() {
            try { using(var key=Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run")) return key!=null && key.GetValue("CornerSpider")!=null; } catch { return false; }
        }
        public void Dispose() {
            if(Disposed) return;Disposed=true;CursorVisibility.Dispose();timer.Stop();displayTimer.Stop();timer.Tick-=OnTick;Clicks.Dispose();
            if(SettingsWindow!=null) SettingsWindow.Close();
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged-=DisplayChanged;Microsoft.Win32.SystemEvents.PowerModeChanged-=PowerChanged;Microsoft.Win32.SystemEvents.SessionSwitch-=SessionChanged;Application.Current.SessionEnding-=SessionEnding;
            Save();if(Tray!=null) { Tray.Visible=false;Tray.Dispose(); }if(Menu!=null) Menu.Dispose();if(icon!=null) icon.Dispose();
            foreach(var w in Windows) w.Close();Windows.Clear();
        }
    }
    static class Program {
        [STAThread] static int Main(string[] args) {
            if(args.Length==3 && args[0]=="--cursor-guard") return CursorVeil.Guard(args);
            if(args.Length==2 && args[0]=="--cursor-lease-fixture") return CursorVeil.Fixture(args[1]);
            const string quitEvent="Local\\CornerSpiderPet_Quit_v2";
            if(args.Length>0 && args[0]=="--quit") {
                try { using(var signal=EventWaitHandle.OpenExisting(quitEvent)) signal.Set(); } catch(WaitHandleCannotBeOpenedException) {}return 0;
            }
            bool test=args.Length>0 && (args[0]=="--self-test" || args[0]=="--desktop-test" || args[0]=="--render-preview" || args[0]=="--finger-preview" || args[0]=="--prey-preview" || args[0]=="--encounter-preview" || args[0]=="--release-preview" || args[0]=="--hammock-preview" || args[0]=="--dismount-preview" || args[0]=="--nest-preview");
            bool created;using(var mutex=new Mutex(true,test?"Local\\CornerSpiderPet_Tests":"Local\\CornerSpiderPet_v1",out created)) {
                if(!created) return 0;
                PetHost host=null;try {
                    var app=new Application { ShutdownMode=ShutdownMode.OnExplicitShutdown };
                    if(test) return RuntimeTests.Run(app,args);
                    string folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"CornerSpider");
                    using(var signal=new EventWaitHandle(false,EventResetMode.AutoReset,quitEvent)) {
                        host=new PetHost(folder,false);app.Exit+=delegate { host.Dispose(); };
                        var registration=ThreadPool.RegisterWaitForSingleObject(signal,delegate { app.Dispatcher.BeginInvoke(new Action(delegate { host.Dispose();app.Shutdown(); })); },null,Timeout.Infinite,false);
                        try { app.Run(); } finally { registration.Unregister(null); }
                    }return 0;
                } catch(Exception ex) {
                    if(host!=null) host.Dispose();
                    string message=ex.ToString();
                    if(test && args.Length>1) { Directory.CreateDirectory(args[1]);File.WriteAllText(Path.Combine(args[1],"runtime-error.txt"),message); }
                    else Forms.MessageBox.Show("Corner Spider couldn't start:\n\n"+message,"Corner Spider");
                    return 1;
                }
            }
        }
    }
}
