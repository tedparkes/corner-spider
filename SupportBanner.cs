using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CornerSpider {
    public static class SupportBanner {
        // Public recipient supplied by the creator. No payment credentials or SDK.
        public const string CashTag="$tedparkes";
        public const string PaymentUrl="https://cash.app/$tedparkes?qr=1";
        static BitmapImage qr;
        public static BitmapImage Qr { get {
            if(qr==null) using(var stream=typeof(SupportBanner).Assembly.GetManifestResourceStream("CornerSpider.SupportQr.png")) {
                if(stream==null) throw new InvalidOperationException("The original support QR resource is missing.");
                var image=new BitmapImage();image.BeginInit();image.CacheOption=BitmapCacheOption.OnLoad;image.StreamSource=stream;image.EndInit();image.Freeze();qr=image;
            }
            return qr;
        } }
        public static bool ValidPaymentLink(string value) {
            Uri uri;
            return Uri.TryCreate(value,UriKind.Absolute,out uri) && uri.Scheme=="https" && uri.Host=="cash.app" && uri.IsDefaultPort && uri.UserInfo.Length==0 && uri.Fragment.Length==0 && uri.AbsolutePath=="/$tedparkes" && (uri.Query.Length==0 || uri.Query=="?qr=1");
        }
        public static bool OpenPayment(bool diagnostic) {
            if(diagnostic || !ValidPaymentLink(PaymentUrl)) return false;
            try { Process.Start(new ProcessStartInfo(PaymentUrl) { UseShellExecute=true });return true; }
            catch(System.ComponentModel.Win32Exception) { return false; }
            catch(InvalidOperationException) { return false; }
        }
        static TextBlock Label(string text,double size) { return new TextBlock { Text=text,FontSize=size,TextWrapping=TextWrapping.Wrap,Foreground=new SolidColorBrush(Color.FromRgb(32,55,43)),Margin=new Thickness(0,0,0,5) }; }
        public static FrameworkElement Create(Window owner,bool diagnostic,out Button qrButton) {
            var row=new DockPanel();
            qrButton=new Button { Content=new Image { Source=Qr,Width=112,Height=112,Stretch=Stretch.Uniform },Padding=new Thickness(0),BorderThickness=new Thickness(0),Background=Brushes.White,ToolTip="Enlarge the original Cash App QR code",Margin=new Thickness(0,0,10,0) };
            System.Windows.Automation.AutomationProperties.SetName(qrButton,"Enlarge Cash App QR for "+CashTag);
            DockPanel.SetDock(qrButton,Dock.Left);row.Children.Add(qrButton);
            var words=new StackPanel { Width=158,VerticalAlignment=VerticalAlignment.Center };
            words.Children.Add(Label("Buy me a coffee / Monster",17));words.Children.Add(Label("Optional support via Cash App",11));
            var link=new Button { Content=CashTag+"  ↗",HorizontalAlignment=HorizontalAlignment.Left,Padding=new Thickness(9,4,9,4),ToolTip="Open cash.app in your browser" };
            link.Click+=delegate { if(!OpenPayment(diagnostic) && !diagnostic) MessageBox.Show(owner,"Couldn't open your browser. You can scan the QR or visit "+PaymentUrl,"Cash App"); };
            words.Children.Add(link);row.Children.Add(words);
            qrButton.Click+=delegate { if(!diagnostic) ShowQr(owner); };
            return new Border { Child=row,Padding=new Thickness(8),CornerRadius=new CornerRadius(10),Background=new SolidColorBrush(Color.FromRgb(226,240,220)),Margin=new Thickness(18,0,0,10) };
        }
        public static void ShowQr(Window owner) {
            var window=new Window { Title="Optional support - Cash App",Owner=owner,Width=480,Height=590,ResizeMode=ResizeMode.NoResize,WindowStartupLocation=WindowStartupLocation.CenterOwner,Background=Brushes.White };
            var panel=new StackPanel { Margin=new Thickness(24) };window.Content=panel;
            panel.Children.Add(Label("Buy me a coffee / Monster",23));
            panel.Children.Add(new Image { Source=Qr,Width=400,Height=400,Stretch=Stretch.Uniform });
            panel.Children.Add(Label(CashTag+" · Optional. Corner Spider stays free.",14));
            var close=new Button { Content="Done",IsCancel=true,Width=90,Height=32,HorizontalAlignment=HorizontalAlignment.Right };close.Click+=delegate { window.Close(); };panel.Children.Add(close);window.ShowDialog();
        }
    }
}
