using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Forms=System.Windows.Forms;

namespace CornerSpider {
    public sealed class PetSettingsWindow : Window {
        readonly PetHost host;
        readonly DispatcherTimer saveTimer=new DispatcherTimer { Interval=TimeSpan.FromMilliseconds(500) };
        readonly DispatcherTimer encounterTimer=new DispatcherTimer { Interval=TimeSpan.FromMilliseconds(500) };
        public readonly Slider SizeSlider,WebSlider,FingerToneSlider,FlySlider,InviteSlider,ToleranceSlider,AttentionSlider,DismissSlider;
        public readonly ComboBox SpeciesSelector,CornerSelector,MonitorSelector;
        public readonly CheckBox FliesCheck,RidesCheck,AllMonitorsCheck,StartupCheck,TraitsCheck,WebVisitsCheck;
        public readonly Button WebVisitButton,SupportButton;
        public readonly SettingsPreview Preview=new SettingsPreview();
        public readonly FingerPreview FingerPreview=new FingerPreview();
        public readonly TabControl Tabs=new TabControl();
        readonly TextBlock speciesNote,saveStatus;
        bool syncing;
        public bool IsClosed { get;private set; }
        static readonly Brush ink=new SolidColorBrush(Color.FromRgb(33,49,50));
        static readonly Brush muted=new SolidColorBrush(Color.FromRgb(88,106,104));
        static TextBlock Text(string value,double size) { return new TextBlock { Text=value,FontSize=size,TextWrapping=TextWrapping.Wrap,Foreground=ink }; }
        static StackPanel NewPanel() { return new StackPanel { Margin=new Thickness(22) }; }
        static void Note(Panel panel,string value) { var t=Text(value,12);t.Foreground=muted;t.Margin=new Thickness(0,4,0,16);panel.Children.Add(t); }
        static ComboBox Select(Panel panel,string label) {
            panel.Children.Add(Text(label,14));var box=new ComboBox { MinHeight=34,Margin=new Thickness(0,6,0,12),FontSize=14,Foreground=ink };
            System.Windows.Automation.AutomationProperties.SetName(box,label);panel.Children.Add(box);return box;
        }
        static CheckBox Toggle(Panel panel,string label,bool value) {
            var box=new CheckBox { Content=label,IsChecked=value,FontSize=14,Margin=new Thickness(0,8,0,8),Foreground=ink };
            System.Windows.Automation.AutomationProperties.SetName(box,label);panel.Children.Add(box);return box;
        }
        Slider Range(Panel panel,string title,double min,double max,double step,double value,string unit,Action<double> change) {
            var label=Text("",14);label.Margin=new Thickness(0,12,0,3);panel.Children.Add(label);
            var slider=new Slider { Minimum=min,Maximum=max,Value=value,TickFrequency=step,SmallChange=step,LargeChange=step*5,IsSnapToTickEnabled=true,MinHeight=25 };
            Action update=delegate { label.Text=title+"  "+slider.Value.ToString(step<1?"0.0":"0")+unit; };
            update();System.Windows.Automation.AutomationProperties.SetName(slider,title);panel.Children.Add(slider);
            slider.ValueChanged+=delegate { update();if(!syncing) { change(slider.Value);Changed(); } };
            return slider;
        }
        void AddTab(string title,UIElement content) {
            Tabs.Items.Add(new TabItem { Header=title,Padding=new Thickness(17,10,17,10),Content=new ScrollViewer { VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled,Content=content } });
        }
        public PetSettingsWindow(PetHost owner) {
            host=owner;Title="Corner Spider - Settings";Width=940;Height=730;MinWidth=720;MinHeight=520;
            MaxHeight=SystemParameters.WorkArea.Height;Height=Math.Min(Height,SystemParameters.WorkArea.Height-35);
            WindowStartupLocation=WindowStartupLocation.CenterScreen;Background=new SolidColorBrush(Color.FromRgb(244,246,239));Foreground=ink;FontFamily=new FontFamily("Segoe UI");FontSize=14;
            var root=new DockPanel { Margin=new Thickness(22) };Content=root;
            var header=new Grid();header.ColumnDefinitions.Add(new ColumnDefinition());header.ColumnDefinitions.Add(new ColumnDefinition { Width=GridLength.Auto });DockPanel.SetDock(header,Dock.Top);root.Children.Add(header);
            var heading=new StackPanel { Margin=new Thickness(0,10,0,6),VerticalAlignment=VerticalAlignment.Center };header.Children.Add(heading);
            heading.Children.Add(Text("Your corner companion",27));Note(heading,"Appearance, gentle interactions and a place to call home.");
            Button supportButton;var support=SupportBanner.Create(this,host.TestMode,out supportButton);SupportButton=supportButton;Grid.SetColumn(support,1);header.Children.Add(support);
            var bottom=new DockPanel { Margin=new Thickness(0,14,0,0) };DockPanel.SetDock(bottom,Dock.Bottom);root.Children.Add(bottom);
            var done=new Button { Content="Done",Width=90,Height=34,IsCancel=true };DockPanel.SetDock(done,Dock.Right);bottom.Children.Add(done);done.Click+=delegate { Close(); };
            saveStatus=Text("Changes apply live and save automatically.",12);saveStatus.VerticalAlignment=VerticalAlignment.Center;bottom.Children.Add(saveStatus);
            Tabs.BorderBrush=new SolidColorBrush(Color.FromRgb(213,222,210));Tabs.Background=Brushes.White;root.Children.Add(Tabs);

            var appearance=new Grid { Margin=new Thickness(20) };
            appearance.ColumnDefinitions.Add(new ColumnDefinition { Width=new GridLength(285) });appearance.ColumnDefinitions.Add(new ColumnDefinition { Width=new GridLength(1,GridUnitType.Star) });
            var options=new StackPanel { Margin=new Thickness(0,0,20,0) };appearance.Children.Add(options);
            SpeciesSelector=Select(options,"Skin");SpeciesSelector.MaxDropDownHeight=340;SpeciesSelector.ItemsSource=SkinCatalog.All;SpeciesSelector.SelectedItem=SkinCatalog.Get(host.Settings.Species);
            speciesNote=Text("",13);speciesNote.MinHeight=105;options.Children.Add(speciesNote);
            CheckBox detail=null;
            SizeSlider=Range(options,"Desktop size",20,180,5,host.World.Scale*100,"%",delegate(double v) { host.World.SetScale(v/100);Preview.PetScale=v/100;if(detail!=null) detail.IsChecked=false;host.Render();Preview.InvalidateVisual(); });
            Note(options,"20% tiny  /  100% original  /  180% large");
            Preview.WebVisibility=host.World.WebVisibility;
            WebSlider=Range(options,"Web visibility",0,100,5,host.World.WebVisibility*100,"%",delegate(double v) { host.World.WebVisibility=v/100;Preview.WebVisibility=v/100;Preview.InvalidateVisual(); });
            Note(options,"100% original silk  /  0% invisible. Applies to both webs and jump threads; nest progress is kept.");
            detail=Toggle(options,"Enlarge preview for detail",true);detail.Checked+=delegate { Preview.Enlarged=true;Preview.InvalidateVisual(); };detail.Unchecked+=delegate { Preview.Enlarged=false;Preview.InvalidateVisual(); };
            Note(options,"Changing size switches to a desktop-size preview. Turn this on for close detail; it does not resize your pet.");
            var dark=Toggle(options,"Dark preview background",true);dark.Checked+=delegate { Preview.Dark=true;Preview.InvalidateVisual(); };dark.Unchecked+=delegate { Preview.Dark=false;Preview.InvalidateVisual(); };
            var previewColumn=new StackPanel();Grid.SetColumn(previewColumn,1);appearance.Children.Add(previewColumn);
            Preview.Height=325;previewColumn.Children.Add(Preview);Preview.PetScale=host.World.Scale;
            Range(previewColumn,"Preview angle",0,360,15,180," degrees",delegate(double v) { Preview.Angle=v;Preview.InvalidateVisual(); });
            Note(previewColumn,"The selected species' silk retreat is fitted to the upper-right corner. Preview angle turns the spider; size and background update both.");
            Action refreshSkin=delegate { var skin=(SkinProfile)SpeciesSelector.SelectedItem;Preview.Species=skin.Id;var personality=BehaviorCatalog.Get(skin.Id);speciesNote.Text=skin.Scientific+"\n\n"+skin.Description+"\n\n"+personality.Title+" — "+personality.Note+"\n\nSilk: "+SilkProfile.Get(skin.Id).Note;Preview.InvalidateVisual(); };
            refreshSkin();SpeciesSelector.SelectionChanged+=delegate { if(SpeciesSelector.SelectedItem==null || syncing) return;refreshSkin();host.SetSpecies(((SkinProfile)SpeciesSelector.SelectedItem).Id); };
            AddTab("Appearance",appearance);

            var fingerPanel=new Grid { Margin=new Thickness(20) };
            fingerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width=new GridLength(265) });fingerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width=new GridLength(1,GridUnitType.Star) });
            var fingerOptions=new StackPanel { Margin=new Thickness(0,0,22,0) };fingerPanel.Children.Add(fingerOptions);
            fingerOptions.Children.Add(Text("A finger to climb onto",21));
            FingerToneSlider=Range(fingerOptions,"Finger skin tone",0,100,1,host.World.FingerTone*100,"%",delegate(double v) { host.World.FingerTone=v/100;FingerPreview.Tone=v/100;FingerPreview.InvalidateVisual(); });
            Note(fingerOptions,"0% deep brown  /  100% very pale");
            Note(fingerOptions,"Choose a shade that feels right to you. Highlights, shadows and the nail adjust together. Changes apply live and are saved automatically.");
            var fingerDark=Toggle(fingerOptions,"Dark preview background",true);fingerDark.Checked+=delegate { FingerPreview.Dark=true;FingerPreview.InvalidateVisual(); };fingerDark.Unchecked+=delegate { FingerPreview.Dark=false;FingerPreview.InvalidateVisual(); };
            Note(fingerOptions,"This is an enlarged preview. The finger on your desktop follows the pet's size setting. Use Behavior to adjust when it appears.");
            var fingerColumn=new StackPanel();Grid.SetColumn(fingerColumn,1);fingerPanel.Children.Add(fingerColumn);
            FingerPreview.Height=330;FingerPreview.Tone=host.World.FingerTone;fingerColumn.Children.Add(FingerPreview);
            Note(fingerColumn,"The same finger artwork used during play, including its transparent hand fade.");
            AddTab("Finger",fingerPanel);

            var behavior=NewPanel();
            TraitsCheck=Toggle(behavior,"Species personalities",host.World.SpeciesTraits);
            TraitsCheck.Checked+=delegate { host.World.SpeciesTraits=true;Changed(); };TraitsCheck.Unchecked+=delegate { host.World.SpeciesTraits=false;Changed(); };
            Note(behavior,"Distinct pauses, displays, stalking and retreat habits. Appearance describes each personality. Research notes distinguish observations from stylized adaptations.");
            WebVisitsCheck=Toggle(behavior,"Portia's corner-web encounters",host.World.WebVisits);
            WebVisitsCheck.Checked+=delegate { host.World.WebVisits=true;Changed(); };WebVisitsCheck.Unchecked+=delegate { host.World.WebVisits=false;host.World.CancelWebVisit();Changed(); };
            WebVisitButton=new Button { Content="Invite a web-building spider",HorizontalAlignment=HorizontalAlignment.Left,Padding=new Thickness(12,6,12,6),Margin=new Thickness(0,4,0,8) };
            behavior.Children.Add(WebVisitButton);WebVisitButton.IsEnabled=host.World.HuntsSpiders && host.World.WebVisits && !host.World.HasFly && host.World.WebStage==WebVisit.None;
            WebVisitButton.Click+=delegate { bool started=host.World.StartWebVisit();saveStatus.Text=started?"A neighbor is building in another corner. Portia will wait before approaching.":"Choose Portia, enable web encounters and wait until the current prey is gone. Leave enough desktop space for another corner web.";host.Render(); };
            Tabs.SelectionChanged+=delegate { WebVisitButton.IsEnabled=host.World.HuntsSpiders && host.World.WebVisits && !host.World.HasFly && host.World.WebStage==WebVisit.None; };
            Note(behavior,"With Portia selected, occasional prey can include a corner web: 3–4 minutes to build, then a 1–2 minute wait. Portia sneaks over and tries plucking signals. Only one prey is present. The invitation button starts the same full-length encounter; Go to hammock clears it.");
            var encounterStatus=Text("",12);behavior.Children.Add(encounterStatus);
            encounterTimer.Tick+=delegate { WebVisitButton.IsEnabled=host.World.HuntsSpiders && host.World.WebVisits && !host.World.HasFly && host.World.WebStage==WebVisit.None;encounterStatus.Text=host.World.GuestStatus+(host.World.WebStage==WebVisit.Building?" — "+(host.World.GuestProgress*100).ToString("F0")+"% woven":""); };encounterTimer.Start();
            FliesCheck=Toggle(behavior,"Occasional prey",host.World.AutoFeed);
            FlySlider=Range(behavior,"Time between prey",30,300,5,host.World.FlyInterval," s",delegate(double v) { host.World.FlyInterval=v;host.World.NextFly=host.World.Clock+v; });
            Note(behavior,"Timing varies around this value. Portia gets a mix of small spiders and flies; other skins get flies. Only one prey animal is present at a time.");
            RidesCheck=Toggle(behavior,"Cursor attention and finger rides",host.World.CursorPlay);
            InviteSlider=Range(behavior,"Finger appears after",2,10,.1,host.World.InviteDelay," s",delegate(double v) { host.World.InviteDelay=v;host.World.Resume(); });
            AttentionSlider=Range(behavior,"Approach distance",60,400,5,host.World.AttentionDistance," px",delegate(double v) {
                host.World.AttentionDistance=v;if(DismissSlider!=null && DismissSlider.Value<v+20) DismissSlider.Value=v+20;
            });
            DismissSlider=Range(behavior,"Lose interest beyond",80,600,5,host.World.DismissDistance," px",delegate(double v) {
                if(v<AttentionSlider.Value+20) { DismissSlider.Value=AttentionSlider.Value+20;return; }host.World.DismissDistance=v;
            });
            Note(behavior,"Distances stay consistent at every pet size. The second distance stays at least 20 px farther away to prevent flickering attention. Hold inside the approach distance to offer your finger.");
            ToleranceSlider=Range(behavior,"Fast-movement escape threshold",300,1500,50,host.World.CarryTolerance," px/s",delegate(double v) { host.World.CarryTolerance=v; });
            Note(behavior,"Lower values make it more easily startled while riding. Clicking makes it turn, brace and jump toward a spot above the finger; near the top edge it hops sideways.");
            FliesCheck.Checked+=delegate { host.World.AutoFeed=true;FlySlider.IsEnabled=true;Changed(); };FliesCheck.Unchecked+=delegate { host.World.AutoFeed=false;FlySlider.IsEnabled=false;Changed(); };FlySlider.IsEnabled=host.World.AutoFeed;
            Action rideChanged=delegate { bool enabled=RidesCheck.IsChecked==true;host.SetCursorPlay(enabled);InviteSlider.IsEnabled=AttentionSlider.IsEnabled=DismissSlider.IsEnabled=ToleranceSlider.IsEnabled=enabled;Changed(); };
            RidesCheck.Checked+=delegate { rideChanged(); };RidesCheck.Unchecked+=delegate { rideChanged(); };
            InviteSlider.IsEnabled=AttentionSlider.IsEnabled=DismissSlider.IsEnabled=ToleranceSlider.IsEnabled=host.World.CursorPlay;
            AddTab("Behavior",behavior);

            var desktop=NewPanel();CornerSelector=Select(desktop,"Hammock corner");CornerSelector.ItemsSource=new[]{"Top left","Top right","Bottom left","Bottom right"};CornerSelector.SelectedIndex=Array.IndexOf(Settings.Corners,host.World.Corner);
            CornerSelector.SelectionChanged+=delegate { if(!syncing && CornerSelector.SelectedIndex>=0) host.SetCorner(Settings.Corners[CornerSelector.SelectedIndex]); };
            AllMonitorsCheck=Toggle(desktop,"Explore all connected monitors",host.Settings.AllMonitors);AllMonitorsCheck.Checked+=delegate { if(!syncing) host.SetAllMonitors(true); };AllMonitorsCheck.Unchecked+=delegate { if(!syncing) host.SetAllMonitors(false); };
            MonitorSelector=Select(desktop,"Hammock screen");RefreshMonitors();MonitorSelector.DropDownOpened+=delegate { RefreshMonitors(); };
            MonitorSelector.SelectionChanged+=delegate { if(!syncing && MonitorSelector.SelectedItem!=null) host.SetHomeMonitor((string)((ComboBoxItem)MonitorSelector.SelectedItem).Tag); };
            Note(desktop,"Automatic keeps the hammock on the rightmost screen. An unavailable chosen screen falls back to the rightmost one until it returns.");
            StartupCheck=Toggle(desktop,"Start with Windows",!host.TestMode && PetHost.ReadStartup());StartupCheck.IsEnabled=!host.TestMode;
            Action startupChanged=delegate { if(syncing) return;if(!host.SetStartup(StartupCheck.IsChecked==true)) { syncing=true;StartupCheck.IsChecked=PetHost.ReadStartup();syncing=false; } };
            StartupCheck.Checked+=delegate { startupChanged(); };StartupCheck.Unchecked+=delegate { startupChanged(); };
            Note(desktop,"Off by default. Startup uses your Windows account only and needs no administrator rights. Disable it before moving or removing the app.");
            var reset=new Button { Content="Reset preferences",HorizontalAlignment=HorizontalAlignment.Left,Padding=new Thickness(14,7,14,7),Margin=new Thickness(0,12,0,8) };desktop.Children.Add(reset);reset.Click+=delegate { ResetPreferences(); };
            Note(desktop,"Restores appearance, interaction and screen defaults. Keeps your hammock progress, meals and Windows startup choice.");
            AddTab("Desktop",desktop);
            var help=NewPanel();help.Children.Add(Text("A quiet little desktop companion",21));
            Note(help,"Approach an awake spider and hold still for the configured invitation delay. The finger fades in; the spider can jump on, hang, climb and wander. Move gently to carry it. A click lets it plan an upward hop; a fast move startles it. The finger holds still until takeoff, and your pointer returns immediately. Your clicks still reach your apps.");
            Note(help,"It watches from corners, stalks occasional flies and returns to its silk retreat. Use the tray's quick actions to feed, call it home, wake or pause it. Double-click the tray icon to reopen these settings.");
            Note(help,"Corner Spider 1.0 RC11\nFree software under the MIT License. The pet works offline with no accounts or telemetry. Optional Cash App support opens your browser only when clicked. Preferences are stored in LocalAppData\\CornerSpider. See README.txt for removal and manual checks.");
            Note(help,"Future ideas: custom colors by body region; crickets, mealworms and other prey; additional spider species; an optional background with depth to explore.");AddTab("About & play",help);
            saveTimer.Tick+=delegate { saveTimer.Stop();host.Save();saveStatus.Text=host.Settings.LastError.Length==0?"Saved. Changes are live on your desktop.":"Could not save preferences. Check that your save folder is writable."; };
            Closed+=delegate { IsClosed=true;saveTimer.Stop();encounterTimer.Stop();Preview.Dispose();host.Save(); };
        }
        void Changed() { if(syncing) return;saveStatus.Text="Changes are live. Saving...";saveTimer.Stop();saveTimer.Start();host.Render(); }
        public void RefreshMonitors() {
            syncing=true;MonitorSelector.Items.Clear();
            MonitorSelector.Items.Add(new ComboBoxItem { Content="Rightmost screen (automatic)",Tag="" });int selected=0;
            var screens=Forms.Screen.AllScreens;Array.Sort(screens,delegate(Forms.Screen a,Forms.Screen b) { return a.Bounds.Left.CompareTo(b.Bounds.Left); });
            for(int i=0;i<screens.Length;i++) { MonitorSelector.Items.Add(new ComboBoxItem { Content="Screen "+(i+1)+(i==screens.Length-1?" - rightmost":"")+(screens[i].Primary?" (primary)":""),Tag=screens[i].DeviceName });if(screens[i].DeviceName==host.Settings.HomeMonitor) selected=i+1; }
            if(selected==0 && host.Settings.HomeMonitor.Length>0) { MonitorSelector.Items.Add(new ComboBoxItem { Content="Previously selected screen (disconnected)",Tag=host.Settings.HomeMonitor });selected=MonitorSelector.Items.Count-1; }
            MonitorSelector.SelectedIndex=selected;syncing=false;
        }
        public void ResetPreferences() {
            TraitsCheck.IsChecked=true;WebVisitsCheck.IsChecked=true;SpeciesSelector.SelectedItem=SkinCatalog.Get("Regal");SizeSlider.Value=100;WebSlider.Value=100;FingerToneSlider.Value=World.DefaultFingerTone*100;FlySlider.Value=100;InviteSlider.Value=4.5;ToleranceSlider.Value=850;
            AttentionSlider.Value=185;DismissSlider.Value=230;FliesCheck.IsChecked=true;RidesCheck.IsChecked=true;CornerSelector.SelectedIndex=1;AllMonitorsCheck.IsChecked=true;MonitorSelector.SelectedIndex=0;Changed();
        }
    }
}
