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
                    panel==3?w.WebStage==WebVisit.Approach && (w.Spider-w.GuestPluckPosition).Length<75*w.GuestScale:
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
            string[] labels={"Anchors, frame and radial spokes","Weaving the curved capture spiral","The resident waits at the center","A quiet approach to the edge","Plucking the outer anchor","Repeating the useful plucks","The resident comes closer","The web-side ambush","Meal; silk remains until departure"};
            var image=new DrawingVisual();using(var d=image.RenderOpen()) {
                d.DrawRectangle(BPreview("#e6ebe1"),null,new Rect(0,0,1260,1210));
                Text(d,"PORTIA / A CORNER-WEB ENCOUNTER",20,14,24,Color.FromRgb(30,50,44));
                Text(d,"3–4 minutes of building, 1–2 minutes of waiting, then a cautious approach and web signals.",20,48,16,Color.FromRgb(30,50,44));
                for(int i=0;i<9;i++) {
                    World w=GuestScene(i);double span=w.GuestWebExtent+210*w.GuestScale,zoom=320/span;
                    V center=w.GuestCorner+w.GuestInward*(span/Math.Sqrt(2));
                    var canvas=new PetCanvas { World=w,Species="PortiaLabiata",Width=420,Height=330,DeviceScale=1/zoom,Offset=center-new V(210/zoom,165/zoom) };
                    canvas.Measure(new Size(420,330));canvas.Arrange(new Rect(0,0,420,330));
                    var bitmap=new RenderTargetBitmap(420,330,96,96,PixelFormats.Pbgra32);bitmap.Render(canvas);
                    d.PushTransform(new TranslateTransform(i%3*420,85+i/3*370));
                    d.DrawRectangle(BPreview("#24313a"),null,new Rect(2,0,416,330));d.DrawImage(bitmap,new Rect(0,0,420,330));
                    Text(d,labels[i],12,338,16,Color.FromRgb(30,50,44));d.Pop();
                }
            }SavePng(image,1260,1210,Path.Combine(folder,"portia-web-encounter.png"));
            var detailWorld=GuestScene(4);double detailSpan=detailWorld.GuestWebExtent+210*detailWorld.GuestScale,detailZoom=760/detailSpan;
            V detailCenter=detailWorld.GuestCorner+detailWorld.GuestInward*(detailSpan/Math.Sqrt(2));
            var detail=new PetCanvas { World=detailWorld,Species="PortiaLabiata",Width=800,Height=800,DeviceScale=1/detailZoom,Offset=detailCenter-new V(400/detailZoom,400/detailZoom) };
            detail.Measure(new Size(800,800));detail.Arrange(new Rect(0,0,800,800));
            var detailImage=new DrawingVisual();using(var d=detailImage.RenderOpen()) {
                d.DrawRectangle(BPreview("#24313a"),null,new Rect(0,0,800,800));
                var bitmap=new RenderTargetBitmap(800,800,96,96,PixelFormats.Pbgra32);bitmap.Render(detail);d.DrawImage(bitmap,new Rect(0,0,800,800));
            }SavePng(detailImage,800,800,Path.Combine(folder,"portia-web-edge.png"));
            var resting=GuestScene(2);var contrast=new DrawingVisual();using(var d=contrast.RenderOpen()) {
                for(int side=0;side<2;side++) {
                    bool dark=side==0;d.PushTransform(new TranslateTransform(side*600,0));
                    d.DrawRectangle(BPreview(dark?"#24313a":"#e7eadd"),null,new Rect(0,0,600,650));
                    Text(d,"Resident web / "+(dark?"dark":"light")+" desktop",20,18,22,dark?Colors.White:Color.FromRgb(30,50,44));
                    d.PushTransform(new TranslateTransform(22,75));
                    double zoom=530/(resting.GuestWebExtent+25*resting.GuestScale);d.PushTransform(new ScaleTransform(zoom,zoom));
                    d.PushTransform(new TranslateTransform(-resting.GuestCorner.X,-resting.GuestCorner.Y));
                    d.PushTransform(new ScaleTransform(resting.GuestInward.X>0?1:-1,resting.GuestInward.Y>0?1:-1,resting.GuestCorner.X,resting.GuestCorner.Y));
                    PetCanvas.DrawGuestWeb(d,resting);PetCanvas.DrawPrey(d,resting);d.Pop();d.Pop();d.Pop();d.Pop();
                    Text(d,"Edge anchors, radial spokes and curved silk",20,607,17,dark?Colors.White:Color.FromRgb(30,50,44));d.Pop();
                }
            }SavePng(contrast,1200,650,Path.Combine(folder,"prey-web-light-dark.png"));
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
            CheckGuestSilkRendering();RenderEncounterPreview(folder);
        }
        static void CheckGuestSilkRendering() {
            var w=GuestScene(2);var visual=new DrawingVisual();
            using(var d=visual.RenderOpen()) { PetCanvas.DrawGuestWeb(d,w); }
            int count=PetCanvas.GuestSilkBuilds(w);
            for(int i=0;i<180;i++) using(var d=visual.RenderOpen()) { PetCanvas.DrawGuestWeb(d,w); }
            Check(count==1 && PetCanvas.GuestSilkBuilds(w)==count,"completed orb web reuses one generated silk image across 180 redraws");
            w.GuestOpacity=.5;using(var d=visual.RenderOpen()) { PetCanvas.DrawGuestWeb(d,w); }
            Check(PetCanvas.GuestSilkBuilds(w)==count,"departure fade reuses the web artwork without rebuilding geometry");
            w.GuestOpacity=1;w.GuestProgress=.35;using(var d=visual.RenderOpen()) { PetCanvas.DrawGuestWeb(d,w); }
            Check(PetCanvas.GuestSilkBuilds(w)==count+1,"construction progress replaces the single retained silk layer");
            w.GuestProgress=1;var bounds=new DrawingVisual();using(var d=bounds.RenderOpen()) {
                d.PushTransform(new TranslateTransform(10,10));double zoom=370/w.GuestWebExtent;d.PushTransform(new ScaleTransform(zoom,zoom));
                d.PushTransform(new TranslateTransform(-w.GuestCorner.X,-w.GuestCorner.Y));
                d.PushTransform(new ScaleTransform(w.GuestInward.X>0?1:-1,w.GuestInward.Y>0?1:-1,w.GuestCorner.X,w.GuestCorner.Y));
                PetCanvas.DrawGuestWeb(d,w);d.Pop();d.Pop();d.Pop();d.Pop();
            }
            var bitmap=new RenderTargetBitmap(400,400,96,96,PixelFormats.Pbgra32);bitmap.Render(bounds);
            byte[] pixels=new byte[400*400*4];bitmap.CopyPixels(pixels,1600,0);int visible=0,outside=0;
            for(int y=0;y<400;y++) for(int x=0;x<400;x++) if(pixels[(y*400+x)*4+3]>8) { visible++;if(x<8 || y<8 || x>382 || y>382) outside++; }
            Check(visible>3000,"rendered orb has clearly visible spoke and capture silk");
            Check(outside==0 && pixels[(399*400+399)*4+3]==0,"rendered orb preserves transparency outside the reserved corner");
            int builds=PetCanvas.GuestSilkBuilds(w);
            for(int i=0;i<=360;i++) { w.GuestProgress=i/360.0;using(var d=visual.RenderOpen()) { PetCanvas.DrawGuestWeb(d,w); } }
            Check(PetCanvas.GuestSilkBuilds(w)-builds<=14,"full construction batches image updates instead of allocating a bitmap per strand or frame");
        }
    }
}
