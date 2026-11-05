using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CornerSpider {
    public sealed class UpdateController : IDisposable {
        readonly PetHost host;readonly CancellationTokenSource cancel=new CancellationTokenSource();
        public event Action Changed;
        public bool Busy,Disposed;public string Status="Update checks are optional. Choose Check now to contact GitHub.";
        public ReleaseInfo Available;
        public readonly string Current=FileVersionInfo.GetVersionInfo(typeof(UpdateController).Assembly.Location).ProductVersion;
        public UpdateController(PetHost owner) {
            host=owner;string updates=Path.Combine(owner.Folder,"updates");
            try { if(Directory.Exists(updates)) { string latest=null;DateTime newest=DateTime.MinValue;foreach(string dir in Directory.GetDirectories(updates)) { string result=Path.Combine(dir,"result.txt");if(File.Exists(result) && File.GetLastWriteTimeUtc(result)>newest) { latest=result;newest=File.GetLastWriteTimeUtc(result); } }if(latest!=null) { string message=File.ReadAllText(latest);Status=message.Length>600?message.Substring(0,600):message; } } }catch(IOException) { }catch(UnauthorizedAccessException) { }
        }
        void Notify() { if(!Disposed && Changed!=null) Changed(); }
        public void Tick() {
            if(Disposed || Busy || host.TestMode || !host.Settings.AutoUpdateCheck) return;
            long last=host.Settings.LastUpdateCheckTicks;var now=DateTime.UtcNow;
            if(last==0 || now.Ticks-last>TimeSpan.TicksPerDay || last>now.AddDays(1).Ticks) CheckNow();
        }
        public async void CheckNow() {
            if(Disposed || Busy) return;if(host.TestMode) { Status="Network checks are disabled in diagnostics.";Notify();return; }
            Busy=true;Available=null;Status="Checking GitHub...";host.Settings.LastUpdateCheckTicks=DateTime.UtcNow.Ticks;host.Save();Notify();
            bool previews=host.Settings.PreviewUpdates;
            try {
                var latest=await Task.Run(delegate { return UpdateCore.SelectRelease(UpdateCore.Text(UpdateCore.Feed,cancel.Token),previews); });
                if(Disposed) return;
                if(latest!=null && latest.Version.CompareTo(ReleaseVersion.Parse(Current))>0) { Available=latest;Status="Version "+latest.Version.Text+" is available. Download and restart when you are ready."; }
                else Status=latest==null?"No compatible published package was found.":"No newer "+(previews?"published":"stable")+" release. GitHub: "+latest.Version.Text+"; installed: "+Current+".";
            } catch(Exception) { if(!Disposed) Status="Could not check GitHub. Your pet keeps running. Check your connection and try again later."; }
            finally { Busy=false;Notify(); }
        }
        public async void Install() {
            if(Disposed || Busy || Available==null || host.TestMode) return;var release=Available;Busy=true;Status="Downloading and verifying "+release.Version.Text+"...";Notify();
            try {
                string stage=await Task.Run(delegate { return UpdateCore.Prepare(release,host.Folder,cancel.Token); });if(Disposed) return;
                host.Save();if(host.Settings.LastError.Length>0) throw new IOException("Could not save your pet before restarting.");
                UpdateCore.StartInstaller(stage,true);host.Dispose();Application.Current.Shutdown();
            } catch(Exception ex) { if(!Disposed) Status="Update was not installed: "+ex.Message+" Your current pet is still running."; }
            finally { Busy=false;Notify(); }
        }
        public void Dispose() { if(Disposed) return;Disposed=true;cancel.Cancel();Changed=null; }
    }
    public static class UpdateSettings {
        public static FrameworkElement Create(PetHost host,out CheckBox automatic,out CheckBox previews,out Button check,out Button install,out Action unsubscribe) {
            var panel=new StackPanel { Margin=new Thickness(22) };
            panel.Children.Add(new TextBlock { Text="Updates from GitHub",FontSize=21,Margin=new Thickness(0,0,0,12) });
            panel.Children.Add(new TextBlock { Text="Installed: "+host.Updates.Current,Margin=new Thickness(0,0,0,10) });
            automatic=new CheckBox { Content="Automatically check GitHub once a day",IsChecked=host.Settings.AutoUpdateCheck,Margin=new Thickness(0,8,0,8) };
            previews=new CheckBox { Content="Include preview releases",IsChecked=host.Settings.PreviewUpdates,Margin=new Thickness(0,8,0,16) };
            var autoBox=automatic;var previewBox=previews;panel.Children.Add(automatic);panel.Children.Add(previews);
            autoBox.Checked+=delegate { host.Settings.AutoUpdateCheck=true;host.Save(); };autoBox.Unchecked+=delegate { host.Settings.AutoUpdateCheck=false;host.Save(); };
            Action channel=delegate { host.Settings.PreviewUpdates=previewBox.IsChecked==true;host.Updates.Available=null;host.Updates.Status="Release channel changed. Check again before installing.";host.Save(); };previewBox.Checked+=delegate { channel(); };previewBox.Unchecked+=delegate { channel(); };
            var buttons=new StackPanel { Orientation=Orientation.Horizontal };panel.Children.Add(buttons);
            check=new Button { Content="Check now",Padding=new Thickness(12,7,12,7),Margin=new Thickness(0,0,10,0) };install=new Button { Content="Download and restart",Padding=new Thickness(12,7,12,7) };
            var checkButton=check;var installButton=install;buttons.Children.Add(check);buttons.Children.Add(install);
            check.Click+=delegate { host.Updates.CheckNow(); };install.Click+=delegate { host.Updates.Install(); };
            var status=new TextBlock { TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,18,0,20) };panel.Children.Add(status);
            Action refresh=delegate { status.Text=host.Updates.Status;checkButton.IsEnabled=!host.Updates.Busy;installButton.IsEnabled=!host.Updates.Busy && host.Updates.Available!=null;autoBox.IsEnabled=previewBox.IsEnabled=!host.Updates.Busy; };
            previewBox.Checked+=delegate { refresh(); };previewBox.Unchecked+=delegate { refresh(); };host.Updates.Changed+=refresh;refresh();unsubscribe=delegate { host.Updates.Changed-=refresh; };
            panel.Children.Add(new TextBlock { TextWrapping=TextWrapping.Wrap,Text="Automatic checks never install or restart on their own. Installing saves your pet, verifies the release checksum, keeps a backup of replaced files, and restarts in the same folder. Your shortcuts and personal save stay in place.",Margin=new Thickness(0,0,0,16) });
            panel.Children.Add(new TextBlock { TextWrapping=TextWrapping.Wrap,Text="Checks contact github.com for tedparkes/corner-spider releases. Downloads also use GitHub's asset servers. No pet history, settings or personal files are uploaded. With automatic checks off, the pet only connects when you press Check now or Download and restart." });
            return panel;
        }
    }
}
