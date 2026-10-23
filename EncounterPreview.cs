using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CornerSpider {
    public static partial class RuntimeTests {
        static World GuestScene(int panel) {
            var w=new World(2400,1600,4);w.AutoFeed=w.CursorPlay=false;w.SetSpecies("PortiaLabiata");w.StartWebVisit();
            for(int frame=0;frame<8500;frame++) {
                w.Tick(.1,new V(-10000,-10000));
                bool ready=panel==0?w.WebStage==WebVisit.Building && w.GuestProgress>.2:
                    panel==1?w.WebStage==WebVisit.Building && w.GuestProgress>.8:
                    panel==2?w.WebStage==WebVisit.Waiting && w.GuestStageAge>40:
                    panel==3?w.WebStage==WebVisit.Approach && (w.Spider-w.GuestCenter).Length<170:
                    panel==4?w.WebStage==WebVisit.Plucking && w.GuestTaps==1 && w.GuestPulse>.5:
                    panel==5?w.WebStage==WebVisit.Plucking && w.GuestLure>.2 && w.GuestPulse>.5:
                    panel==6?w.WebStage==WebVisit.Plucking && w.GuestLure>.8 && w.GuestPulse>.5:
                    panel==7?w.WebStage==WebVisit.Attack && w.State==Mood.Jump && w.JumpProgress>.2:
                    w.WebStage==WebVisit.Fading && w.State==Mood.Eat;
                if(ready) return w;
            }
            throw new InvalidOperationException("Encounter preview did not reach panel "+panel);
        }
        public static void RenderEncounterPreview(string folder) {
            string[] labels={"Weaving the frame","Adding the cross threads","Waiting after construction","A quiet, angled approach","Trying a signal","Repeating the useful plucks","The resident comes closer","A short web-side ambush","Meal; the empty web fades"};
            var image=new DrawingVisual();using(var d=image.RenderOpen()) {
                d.DrawRectangle(BPreview("#e6ebe1"),null,new Rect(0,0,1260,1210));
                Text(d,"PORTIA / A CORNER-WEB ENCOUNTER",20,14,24,Color.FromRgb(30,50,44));
                Text(d,"3–4 minutes of building, 1–2 minutes of waiting, then a cautious approach and web signals.",20,48,16,Color.FromRgb(30,50,44));
                for(int i=0;i<9;i++) {
                    World w=GuestScene(i);V center=w.GuestCenter+w.GuestInward*65;
                    var canvas=new PetCanvas { World=w,Species="PortiaLabiata",Width=420,Height=330,Offset=center-new V(210,165) };
                    canvas.Measure(new Size(420,330));canvas.Arrange(new Rect(0,0,420,330));
                    var bitmap=new RenderTargetBitmap(420,330,96,96,PixelFormats.Pbgra32);bitmap.Render(canvas);
                    d.PushTransform(new TranslateTransform(i%3*420,85+i/3*370));
                    d.DrawRectangle(BPreview("#24313a"),null,new Rect(2,0,416,330));d.DrawImage(bitmap,new Rect(0,0,420,330));
                    Text(d,labels[i],12,338,16,Color.FromRgb(30,50,44));d.Pop();
                }
            }SavePng(image,1260,1210,Path.Combine(folder,"portia-web-encounter.png"));
            image=new DrawingVisual();using(var d=image.RenderOpen()) {
                d.DrawRectangle(BPreview("#e6ebe1"),null,new Rect(0,0,1400,1340));Text(d,"SPECIES PERSONALITIES / SOLO DISPLAY ADAPTATIONS",20,14,24,Color.FromRgb(30,50,44));
                for(int i=0;i<SkinCatalog.All.Length;i++) {
                    var skin=SkinCatalog.All[i];var w=new World(1600,1000,42);w.AutoFeed=w.CursorPlay=false;w.SetSpecies(skin.Id);w.Spider=new V(170,155);w.Angle=180;w.SetScale(1.2);w.StartSpeciesMoment();
                    for(int frame=0;frame<50;frame++) w.Tick(.025,new V(-10000,-10000));
                    d.PushTransform(new TranslateTransform(i%4*350,65+i/4*255));d.PushClip(new RectangleGeometry(new Rect(0,0,350,245)));
                    d.DrawRectangle(BPreview("#24313a"),null,new Rect(2,0,346,200));
                    d.PushTransform(new TranslateTransform(175-w.Spider.X,100-w.Spider.Y));PetCanvas.DrawSpider(d,w,skin.Id);d.Pop();
                    Text(d,skin.Name,12,202,14,Color.FromRgb(30,50,44));Text(d,w.Personality.Title,12,222,12,Color.FromRgb(60,80,70));d.Pop();d.Pop();
                }
            }SavePng(image,1400,1340,Path.Combine(folder,"species-personalities.png"));
        }
        static void CheckEncounterSettings(PetHost host,string folder) {
            host.World.GoHome();host.SetSpecies("PortiaLabiata");var panel=new PetSettingsWindow(host);
            Check(panel.SupportQrButton!=null && SupportBanner.Qr.PixelWidth==545,"original cropped Cash App QR is embedded and displayed");
            Check(SupportBanner.ValidPaymentLink(SupportBanner.PaymentUrl) && !SupportBanner.OpenPayment(true),"support uses the public Cash App recipient; diagnostics never open payment pages");
            foreach(string bad in new[]{"http://cash.app/$tedparkes","https://cash.app.evil.example/$tedparkes","https://other@cash.app/$tedparkes","file:///tmp/pay","https://cash.app/$someoneelse","https://cash.app/$tedparkes?amount=100"}) Check(!SupportBanner.ValidPaymentLink(bad),"reject unexpected support target: "+bad);
            panel.TraitsCheck.IsChecked=false;panel.WebVisitsCheck.IsChecked=false;host.Save();var s=Settings.Load(host.Folder);
            Check(!s.SpeciesTraits && !s.WebVisits,"new behavior controls persist through native UI");
            panel.TraitsCheck.IsChecked=true;panel.WebVisitsCheck.IsChecked=true;
            Check(host.World.StartWebVisit() && host.World.GuestPrey,"native host starts corner web encounter");
            panel.WebVisitsCheck.IsChecked=false;Check(host.World.WebStage==WebVisit.None && !host.World.HasFly,"native toggle clears resident and web");
            panel.ResetPreferences();Check(host.World.SpeciesTraits && host.World.WebVisits,"reset restores personality and web preferences");
            panel.ShowActivated=false;panel.ShowInTaskbar=false;panel.Show();panel.UpdateLayout();
            var root=(FrameworkElement)panel.Content;SavePng(panel,(int)panel.ActualWidth,(int)panel.ActualHeight,Path.Combine(folder,"settings-banner.png"));
            panel.Width=720;panel.Height=520;panel.UpdateLayout();SavePng(panel,(int)panel.ActualWidth,(int)panel.ActualHeight,Path.Combine(folder,"settings-compact.png"));
            var qrBounds=panel.SupportQrButton.TransformToAncestor(root).TransformBounds(new Rect(new Point(),panel.SupportQrButton.RenderSize));
            Check(qrBounds.Left>=0 && qrBounds.Right<=root.ActualWidth && qrBounds.Bottom<=root.ActualHeight,"support QR stays inside compact settings bounds");
            Check(panel.SupportQrButton.ActualWidth>=112 && panel.SupportQrButton.ActualHeight>=112,"compact settings preserve the original QR at readable size");panel.Close();
            RenderEncounterPreview(folder);
        }
    }
}
