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
                    panel==5?w.WebStage==WebVisit.Plucking && w.GuestLure>.2 && w.PreyWalking:
                    panel==6?w.WebStage==WebVisit.Plucking && w.GuestLure>.8 && w.PreyWalking:
                    panel==7?w.WebStage==WebVisit.Attack && w.State==Mood.Jump && w.JumpProgress>.2:
                    w.WebStage==WebVisit.Fading && w.State==Mood.Struggle && w.StateTime>.4;
                if(ready) return w;
            }
            throw new InvalidOperationException("Encounter preview did not reach panel "+panel);
        }
        public static void RenderEncounterPreview(string folder) {
            string[] labels={"Anchors, frame and radial spokes","Weaving the curved capture spiral","The resident waits at the center","Stepping onto the anchored edge","One foreleg plucks the silk","The prey advances, then listens","Another pluck draws it closer","The web-side ambush","A brief struggle; silk stays until departure"};
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
                    Text(d,"Silk fixed along both desktop edges",20,607,17,dark?Colors.White:Color.FromRgb(30,50,44));d.Pop();
                }
            }SavePng(contrast,1200,650,Path.Combine(folder,"prey-web-light-dark.png"));
            var corners=new DrawingVisual();using(var d=corners.RenderOpen()) {
                d.DrawRectangle(BPreview("#e6ebe1"),null,new Rect(0,0,800,850));
                Text(d,"RESIDENT WEB / ATTACHED TO BOTH EDGES",20,12,22,Color.FromRgb(30,50,44));
                for(int i=0;i<4;i++) {
                    d.PushTransform(new TranslateTransform(i%2*400+20,50+i/2*400));
                    d.DrawRectangle(BPreview("#24313a"),null,new Rect(0,0,360,360));
                    DrawGuestAtCorner(d,resting,i,360,true);
                    Text(d,Settings.Corners[i],0,367,16,Color.FromRgb(30,50,44));d.Pop();
                }
            }SavePng(corners,800,850,Path.Combine(folder,"prey-web-corners.png"));
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
        static void DrawGuestAtCorner(DrawingContext d,World w,int corner,double size,bool prey) {
            bool right=corner==1 || corner==3,bottom=corner>=2;
            double zoom=size/w.GuestWebExtent;
            d.PushTransform(new TranslateTransform(right?size:0,bottom?size:0));
            d.PushTransform(new ScaleTransform((right?-1:1)*zoom,(bottom?-1:1)*zoom));
            d.PushTransform(new TranslateTransform(-w.GuestCorner.X,-w.GuestCorner.Y));
            d.PushTransform(new ScaleTransform(w.GuestInward.X>0?1:-1,w.GuestInward.Y>0?1:-1,w.GuestCorner.X,w.GuestCorner.Y));
            PetCanvas.DrawGuestWeb(d,w);if(prey) PetCanvas.DrawPrey(d,w);d.Pop();d.Pop();d.Pop();d.Pop();
        }
        static void CheckEncounterSettings(PetHost host,string folder) {
            host.World.GoHome();host.SetSpecies("PortiaLabiata");var panel=new PetSettingsWindow(host);
            var popup=(System.Windows.Controls.Primitives.Popup)panel.SupportButton.Tag;
            Check(panel.SupportButton.Content is string && !popup.IsOpen && SupportBanner.Qr.PixelWidth==545,"support starts as a quiet text button and retains the original QR");
            panel.TraitsCheck.IsChecked=false;panel.WebVisitsCheck.IsChecked=false;host.Save();var s=Settings.Load(host.Folder);
            Check(!s.SpeciesTraits && !s.WebVisits,"new behavior controls persist through native UI");
            panel.TraitsCheck.IsChecked=true;panel.WebVisitsCheck.IsChecked=true;
            Check(host.World.StartWebVisit() && host.World.GuestPrey,"native host starts corner web encounter");
            panel.WebVisitsCheck.IsChecked=false;Check(host.World.WebStage==WebVisit.None && !host.World.HasFly,"native toggle clears resident and web");
            panel.ResetPreferences();Check(host.World.SpeciesTraits && host.World.WebVisits,"reset restores personality and web preferences");
            panel.ShowActivated=false;panel.ShowInTaskbar=false;panel.Show();panel.UpdateLayout();
            var root=(FrameworkElement)panel.Content;SavePng(panel,(int)panel.ActualWidth,(int)panel.ActualHeight,Path.Combine(folder,"settings-banner.png"));
            panel.Width=720;panel.Height=520;panel.UpdateLayout();SavePng(panel,(int)panel.ActualWidth,(int)panel.ActualHeight,Path.Combine(folder,"settings-compact.png"));
            var buttonBounds=panel.SupportButton.TransformToAncestor(root).TransformBounds(new Rect(new Point(),panel.SupportButton.RenderSize));
            Check(buttonBounds.Left>=0 && buttonBounds.Right<=root.ActualWidth && buttonBounds.Bottom<=root.ActualHeight,"support button stays inside compact settings bounds");
            panel.SupportButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));panel.UpdateLayout();
            var flyout=(FrameworkElement)popup.Child;flyout.Measure(new Size(400,600));flyout.Arrange(new Rect(new Point(),flyout.DesiredSize));flyout.UpdateLayout();
            Check(popup.IsOpen && !popup.StaysOpen,"button opens a dismissible QR-only flyout");
            SavePng(flyout,(int)Math.Ceiling(flyout.ActualWidth),(int)Math.Ceiling(flyout.ActualHeight),Path.Combine(folder,"support-flyout.png"));
            var contents=(StackPanel)((Border)popup.Child).Child;var image=(Image)contents.Children[2];
            Check(image.ActualWidth==272 && image.Source==SupportBanner.Qr,"flyout displays the original recognizable QR at scanning size");
            int actions=0;foreach(UIElement child in contents.Children) if(child is Button) {
                actions++;Check(((Button)child).Content as string=="Close","QR flyout has no payment-link action");
            }
            Check(actions==1,"Close is the only action inside the QR flyout");
            ((Button)contents.Children[contents.Children.Count-1]).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Check(!popup.IsOpen,"flyout close button works");
            panel.SupportButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));panel.Close();Check(!popup.IsOpen,"closing settings also closes its flyout");
            CheckGuestSilkRendering();RenderEncounterPreview(folder);
        }
        static void CheckGuestSilkRendering() {
            var w=GuestScene(2);var visual=new DrawingVisual();
            long previous=long.MaxValue;
            foreach(double visibility in new[]{1,.5,0}) {
                w.WebVisibility=visibility;var opacityVisual=new DrawingVisual();
                using(var d=opacityVisual.RenderOpen()) DrawGuestAtCorner(d,w,0,360,false);
                var opacityBitmap=new RenderTargetBitmap(360,360,96,96,PixelFormats.Pbgra32);opacityBitmap.Render(opacityVisual);byte[] opacityPixels=new byte[360*360*4];opacityBitmap.CopyPixels(opacityPixels,1440,0);
                long alpha=0;for(int i=3;i<opacityPixels.Length;i+=4) alpha+=opacityPixels[i];
                Check(visibility==0?alpha==0:alpha>0 && alpha<previous,"resident web obeys visibility without affecting its construction: "+visibility);previous=alpha;
            }
            w.WebVisibility=1;
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
            w.GuestProgress=w.GuestOpacity=1;
            for(int corner=0;corner<4;corner++) {
                var edges=new DrawingVisual();using(var d=edges.RenderOpen()) {
                    d.PushTransform(new TranslateTransform(16,16));DrawGuestAtCorner(d,w,corner,360,false);d.Pop();
                }
                var rendered=new RenderTargetBitmap(392,392,96,96,PixelFormats.Pbgra32);rendered.Render(edges);
                byte[] edgePixels=new byte[392*392*4];rendered.CopyPixels(edgePixels,392*4,0);
                bool right=corner==1 || corner==3,bottom=corner>=2;int horizontal=0,vertical=0;
                for(int j=20;j<372;j++) for(int k=-1;k<=1;k++) {
                    if(edgePixels[(((bottom?376:16)+k)*392+j)*4+3]>8) horizontal++;
                    if(edgePixels[(j*392+(right?376:16)+k)*4+3]>8) vertical++;
                }
                Check(horizontal>=12 && vertical>=12,"rendered silk touches both desktop edges in "+Settings.Corners[corner]);
            }
            var plucking=GuestScene(4);var pluckVisual=new DrawingVisual();
            for(int frame=0;frame<180;frame++) {
                plucking.GuestStageAge=1.25+frame/30.0;
                using(var d=pluckVisual.RenderOpen()) { d.PushTransform(new TranslateTransform(16,16));DrawGuestAtCorner(d,plucking,0,360,false);d.Pop(); }
            }
            Check(PetCanvas.GuestSilkBuilds(plucking)==1,"repeated plucks reuse the retained web image while only the contacted spoke ripples");
            plucking.GuestStageAge=1.575;
            using(var d=pluckVisual.RenderOpen()) { d.PushTransform(new TranslateTransform(16,16));DrawGuestAtCorner(d,plucking,0,360,false);d.Pop(); }
            var pluckBitmap=new RenderTargetBitmap(392,392,96,96,PixelFormats.Pbgra32);pluckBitmap.Render(pluckVisual);
            byte[] pluckPixels=new byte[392*392*4];pluckBitmap.CopyPixels(pluckPixels,392*4,0);int unsupported=0;
            // Compare to the unchanged RC9 artwork: this checks newly drawn
            // plucking lines rather than imposing a different web silhouette.
            plucking.State=Mood.Rest;using(var d=pluckVisual.RenderOpen()) { d.PushTransform(new TranslateTransform(16,16));DrawGuestAtCorner(d,plucking,0,360,false);d.Pop(); }
            var baselineBitmap=new RenderTargetBitmap(392,392,96,96,PixelFormats.Pbgra32);baselineBitmap.Render(pluckVisual);
            byte[] baseline=new byte[392*392*4];baselineBitmap.CopyPixels(baseline,392*4,0);
            for(int y=20;y<372;y++) for(int x=20;x<372;x++) if(pluckPixels[(y*392+x)*4+3]>baseline[(y*392+x)*4+3]+8) {
                // Allow the existing thin rim two rendered pixels of antialiasing.
                V insideProbe=new V((x-18)*188/360.0,(y-18)*188/360.0);
                if(!GuestWebPattern.InsideSilk(insideProbe)) unsupported++;
            }
            Check(unsupported==0,"plucking adds no tether strands outside the existing concave web");
        }
    }
}
