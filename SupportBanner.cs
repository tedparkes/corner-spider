using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CornerSpider {
    public static class SupportBanner {
        // The creator's original payment QR is the only support destination.
        // No recipient text, browser launch, payment credentials or SDK.
        static BitmapImage qr;
        public static BitmapImage Qr { get {
            if(qr==null) using(var stream=typeof(SupportBanner).Assembly.GetManifestResourceStream("CornerSpider.SupportQr.png")) {
                if(stream==null) throw new InvalidOperationException("The original support QR resource is missing.");
                var image=new BitmapImage();image.BeginInit();image.CacheOption=BitmapCacheOption.OnLoad;image.StreamSource=stream;image.EndInit();image.Freeze();qr=image;
            }
            return qr;
        } }
        static TextBlock Label(string text,double size) { return new TextBlock { Text=text,FontSize=size,TextWrapping=TextWrapping.Wrap,Foreground=new SolidColorBrush(Color.FromRgb(32,55,43)),Margin=new Thickness(0,0,0,5) }; }
        public static FrameworkElement Create(Window owner,out Button supportButton) {
            var button=new Button { Content="Buy me a coffee / Monster",FontSize=12,Padding=new Thickness(10,6,10,6),Margin=new Thickness(18,0,0,10),VerticalAlignment=VerticalAlignment.Center,ToolTip="Optional support — show the Cash App QR code" };
            supportButton=button;
            System.Windows.Automation.AutomationProperties.SetName(button,"Buy me a coffee / Monster");
            var popup=new Popup { PlacementTarget=button,Placement=PlacementMode.Bottom,AllowsTransparency=true,StaysOpen=false };
            button.Tag=popup;
            var panel=new StackPanel { Width=272 };
            panel.Children.Add(Label("Buy me a coffee / Monster",19));
            panel.Children.Add(Label("Optional support. Corner Spider stays free.",12));
            var code=new Image { Source=Qr,Width=272,Height=272,Stretch=Stretch.Uniform,Margin=new Thickness(0,8,0,12) };
            System.Windows.Automation.AutomationProperties.SetName(code,"Cash App support QR code. Scan with your phone.");panel.Children.Add(code);
            panel.Children.Add(Label("Scan with your phone to support Corner Spider.",12));
            var close=new Button { Content="Close",Margin=new Thickness(0,10,0,0),Padding=new Thickness(12,5,12,5),HorizontalAlignment=HorizontalAlignment.Right };
            close.Click+=delegate { popup.IsOpen=false;button.Focus(); };panel.Children.Add(close);
            popup.Child=new Border { Child=panel,Padding=new Thickness(18),CornerRadius=new CornerRadius(9),Background=Brushes.White,BorderBrush=new SolidColorBrush(Color.FromRgb(185,200,185)),BorderThickness=new Thickness(1) };
            popup.Child.PreviewKeyDown+=delegate(object sender,KeyEventArgs e) { if(e.Key==Key.Escape) { popup.IsOpen=false;button.Focus();e.Handled=true; } };
            popup.Opened+=delegate { close.Focus(); };
            button.Click+=delegate { popup.IsOpen=!popup.IsOpen; };
            owner.Deactivated+=delegate { popup.IsOpen=false; };
            owner.Closed+=delegate { popup.IsOpen=false; };
            return button;
        }
    }
}
