using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;

namespace CornerSpider {
    internal sealed class HabitatWindow : Window {
        readonly PetHost host;
        readonly HabitatCanvas canvas;
        readonly DispatcherTimer timer=new DispatcherTimer { Interval=TimeSpan.FromMilliseconds(33) };
        readonly Stopwatch clock=Stopwatch.StartNew();
        readonly TextBlock status;
        readonly Button fullScreen;
        double lastTick;
        bool fillingScreen;
        WindowState previousState;
        public readonly string SceneId;
        public readonly bool WhiteIsNear;

        public HabitatWindow(PetHost host,HabitatScene scene,bool whiteIsNear) {
            this.host=host;SceneId=scene.Id;WhiteIsNear=whiteIsNear;
            Title="Corner Spider - "+scene.Name;
            Width=1100;Height=720;MinWidth=340;MinHeight=300;
            WindowStartupLocation=WindowStartupLocation.CenterScreen;
            Background=new SolidColorBrush(Color.FromRgb(22,29,25));
            Foreground=Brushes.WhiteSmoke;
            var root=new DockPanel();Content=root;
            var bar=new DockPanel { Margin=new Thickness(14,7,14,7) };
            DockPanel.SetDock(bar,Dock.Bottom);root.Children.Add(bar);
            fullScreen=new Button { Content="Fill screen (F11)",Padding=new Thickness(10,4,10,4) };
            fullScreen.Click+=delegate { ToggleFullScreen(); };
            DockPanel.SetDock(fullScreen,Dock.Right);bar.Children.Add(fullScreen);
            status=new TextBlock { Margin=new Thickness(0,4,10,4),VerticalAlignment=VerticalAlignment.Center,
                TextTrimming=TextTrimming.CharacterEllipsis };
            bar.Children.Add(status);
            canvas=new HabitatCanvas(scene);root.Children.Add(canvas);
            canvas.Apply(host.Settings.Species,host.World.Scale,host.World.WebVisibility);
            status.Text=canvas.Status;
            timer.Tick+=Tick;
            Loaded+=delegate { lastTick=clock.Elapsed.TotalSeconds;timer.Start(); };
            Closed+=delegate { timer.Stop();timer.Tick-=Tick; };
            PreviewKeyDown+=delegate(object sender,KeyEventArgs e) {
                if(e.Key==Key.F11 || (e.Key==Key.Escape && fillingScreen)) { ToggleFullScreen();e.Handled=true; }
            };
        }

        void Tick(object sender,EventArgs e) {
            double now=clock.Elapsed.TotalSeconds,dt=now-lastTick;lastTick=now;
            if(WindowState==WindowState.Minimized || !host.HabitatMayAnimate) return;
            canvas.Apply(host.Settings.Species,host.World.Scale,host.World.WebVisibility);
            canvas.Tick(dt);
            if(status.Text!=canvas.Status) status.Text=canvas.Status;
        }

        void ToggleFullScreen() {
            if(fillingScreen) {
                WindowState=WindowState.Normal;
                WindowStyle=WindowStyle.SingleBorderWindow;
                ResizeMode=ResizeMode.CanResize;
                WindowState=previousState;
                fullScreen.Content="Fill screen (F11)";
            } else {
                previousState=WindowState;
                WindowState=WindowState.Normal;
                WindowStyle=WindowStyle.None;
                ResizeMode=ResizeMode.NoResize;
                WindowState=WindowState.Maximized;
                fullScreen.Content="Leave full screen (Esc)";
            }
            fillingScreen=!fillingScreen;
        }
    }

    internal sealed class HabitatSettingsPanel : StackPanel {
        readonly PetHost host;
        readonly ComboBox library;
        readonly ComboBox polarity;
        readonly Button open;
        readonly TextBlock status;

        public HabitatSettingsPanel(PetHost host) {
            this.host=host;Margin=new Thickness(20);
            Children.Add(new TextBlock { Text="Depth habitats",FontSize=21,Margin=new Thickness(0,0,0,6) });
            AddNote("Import a photo and a matching grayscale depth image from the same viewpoint. The spider explores the visible surfaces, gets smaller in the distance and lays silk between parts of the scene.");
            AddNote("The habitat opens in its own window. Close it to return to the desktop. Use F11 to fill the screen.");
            Children.Add(new TextBlock { Text="Saved habitats",Margin=new Thickness(0,13,0,5) });
            library=new ComboBox { MinWidth=240,MaxWidth=510,HorizontalAlignment=HorizontalAlignment.Left,
                DisplayMemberPath="Name" };
            Children.Add(library);
            Children.Add(new TextBlock { Text="Depth image convention",Margin=new Thickness(0,13,0,5) });
            polarity=new ComboBox { MinWidth=200,HorizontalAlignment=HorizontalAlignment.Left,
                ItemsSource=new[]{"White is near","Black is near"},SelectedIndex=host.Settings.HabitatWhiteNear?0:1 };
            Children.Add(polarity);
            var buttons=new StackPanel { Orientation=Orientation.Horizontal,Margin=new Thickness(0,15,0,0) };
            var import=new Button { Content="Import image pair...",Padding=new Thickness(12,6,12,6),Margin=new Thickness(0,0,8,0) };
            import.Click+=delegate { Import(); };buttons.Children.Add(import);
            open=new Button { Content="Open habitat",Padding=new Thickness(12,6,12,6) };
            open.Click+=delegate { Open(); };buttons.Children.Add(open);Children.Add(buttons);
            status=new TextBlock { Margin=new Thickness(0,10,0,0),TextWrapping=TextWrapping.Wrap };Children.Add(status);
            library.SelectionChanged+=delegate {
                var selected=library.SelectedItem as HabitatLibraryEntry;
                open.IsEnabled=selected!=null;
                if(selected!=null) polarity.SelectedIndex=selected.WhiteIsNear?0:1;
            };
            try { Refresh(host.Settings.HabitatId); }
            catch(Exception ex) { ShowError(ex); }
        }

        void AddNote(string text) {
            Children.Add(new TextBlock { Text=text,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,4,0,2) });
        }

        void Refresh(string selected) {
            var entries=HabitatLibrary.List(host.Folder);
            library.ItemsSource=entries;library.SelectedIndex=entries.Length==0?-1:0;
            foreach(var entry in entries) if(entry.Id==selected) { library.SelectedItem=entry;break; }
            open.IsEnabled=library.SelectedItem!=null;
            status.Text=entries.Length==0?"No habitats imported yet.":"Image pairs are saved on this PC.";
        }

        string Pick(string title) {
            var dialog=new OpenFileDialog { Title=title,Filter="PNG or JPEG image|*.png;*.jpg;*.jpeg",
                CheckFileExists=true,Multiselect=false };
            return dialog.ShowDialog(Window.GetWindow(this))==true?dialog.FileName:null;
        }

        void Import() {
            string color=Pick("Choose the habitat photo");
            if(color==null) return;
            string depth=Pick("Choose the matching grayscale depth image");
            if(depth==null) return;
            try {
                bool whiteIsNear=polarity.SelectedIndex==0;
                var scene=HabitatLibrary.Import(host.Folder,color,depth,whiteIsNear);
                Refresh(scene.Id);
                host.OpenHabitat(scene,whiteIsNear);
            } catch(Exception ex) { ShowError(ex); }
        }

        void Open() {
            var selected=library.SelectedItem as HabitatLibraryEntry;
            if(selected==null) return;
            try {
                bool whiteIsNear=polarity.SelectedIndex==0;
                var scene=HabitatLibrary.Load(host.Folder,selected.Id,whiteIsNear);
                if(whiteIsNear!=selected.WhiteIsNear) {
                    HabitatLibrary.SetPolarity(host.Folder,selected.Id,whiteIsNear);
                    Refresh(selected.Id);
                }
                host.OpenHabitat(scene,whiteIsNear);
                status.Text="Habitat opened.";
            } catch(Exception ex) { ShowError(ex); }
        }

        void ShowError(Exception error) { status.Text="Couldn't open this habitat. "+error.Message; }
    }
}
