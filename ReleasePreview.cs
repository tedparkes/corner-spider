using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CornerSpider {
    public static partial class RuntimeTests {
        static World NestWorld(string corner,double size,double web) {
            var w=new World(320,280,42);w.AutoFeed=w.CursorPlay=false;w.Corner=corner;w.SetScale(size);w.Spider=w.Home;w.GoHome();
            for(int i=0;i<120;i++) w.Tick(.025,new V(-10000,-10000));w.Web=web;return w;
        }
        public static void RenderHammockGallery(string folder) {
            var canvas=new PetCanvas();var v=new DrawingVisual();
            string[] corners={"TopLeft","TopRight","BottomLeft","BottomRight"};
            using(var d=v.RenderOpen()) {
                d.DrawRectangle(Brushes.White,null,new Rect(0,0,1280,700));
                Text(d,"CORNER SPIDER / SILK RETREATS",24,12,24,Color.FromRgb(30,50,44));
                Text(d,"Four corners, same tucked pose - upper: mature silk / lower: new silk",24,44,14,Color.FromRgb(30,50,44));
                for(int row=0;row<2;row++) for(int c=0;c<4;c++) {
                    d.PushTransform(new TranslateTransform(c*320,80+row*310));
                    d.PushClip(new RectangleGeometry(new Rect(0,0,320,280)));
                    d.DrawRectangle(row==0?BPreview("#e6eadd"):BPreview("#25313b"),null,new Rect(0,0,320,280));
                    World w=NestWorld(corners[c],1.65,row==0?1:.15);
                    canvas.DrawNestScene(d,w,"Bold");
                    d.Pop();Text(d,corners[c],12,284,14,Color.FromRgb(30,50,44));d.Pop();
                }
            }SavePng(v,1280,700,Path.Combine(folder,"hammock-corners.png"));
            v=new DrawingVisual();using(var d=v.RenderOpen()) {
                d.DrawRectangle(BPreview("#25313b"),null,new Rect(0,0,1280,325));
                string[] labels={"Turn into the retreat","Fold feet inward","Tucked between silk layers","Wake and stretch out"};
                for(int i=0;i<4;i++) {
                    d.PushTransform(new TranslateTransform(i*320,15));d.PushClip(new RectangleGeometry(new Rect(0,0,320,280)));
                    var w=new World(320,280,42);w.AutoFeed=w.CursorPlay=false;w.SetScale(1.8);w.Web=.8;w.Spider=w.Home;
                    w.Angle=0;w.GoHome();int frames=i==0?12:i==1?46:120;
                    for(int k=0;k<frames;k++) w.Tick(.025,new V(-10000,-10000));
                    if(i==3) { w.Wake();for(int k=0;k<26;k++) w.Tick(.025,new V(-10000,-10000)); }
                    canvas.DrawNestScene(d,w,"Regal");
                    d.Pop();Text(d,labels[i],12,286,14,Colors.White);d.Pop();
                }
            }SavePng(v,1280,325,Path.Combine(folder,"hammock-settle.png"));
        }
        static Brush BPreview(string color) { return (Brush)new BrushConverter().ConvertFromString(color); }
        public static void RenderSkinGallery(string folder) {
            RenderSkinSizes(folder);
            RenderPalpGallery(folder);int height=100+((SkinCatalog.All.Length+4)/5)*325;
            foreach(bool dark in new[]{false,true}) {
                var v=new DrawingVisual();using(var d=v.RenderOpen()) {
                    Color ink=dark?Color.FromRgb(229,237,227):Color.FromRgb(29,48,44);
                    d.DrawRectangle(new SolidColorBrush(dark?Color.FromRgb(29,41,47):Color.FromRgb(234,238,228)),null,new Rect(0,0,1500,height));
                    Text(d,"CORNER SPIDER / TWENTY COATS",26,18,26,ink);
                    Text(d,"Original procedural artwork  -  species proportions, male/female pairs and articulated palps",28,56,14,ink);
                    for(int i=0;i<SkinCatalog.All.Length;i++) {
                        var skin=SkinCatalog.All[i];double x=150+(i%5)*300,y=200+(i/5)*325;
                        var w=new World(1500,height,42);w.Scale=2.4;w.Spider=new V(x,y);w.Angle=180;w.State=Mood.Rest;
                        PetCanvas.DrawSpider(d,w,skin.Id);Text(d,skin.Name,x-125,y+112,19,ink);Text(d,skin.Scientific,x-125,y+143,13,ink);
                    }
                }SavePng(v,1500,height,Path.Combine(folder,"skins-"+(dark?"dark":"light")+".png"));
            }
            foreach(var skin in SkinCatalog.All) {
                var v=new DrawingVisual();using(var d=v.RenderOpen()) {
                    d.DrawRectangle(new SolidColorBrush(Color.FromRgb(230,235,225)),null,new Rect(0,0,960,360));
                    Text(d,skin.Name+" / orientation and foreleg check",20,16,22,Color.FromRgb(30,50,44));
                    for(int i=0;i<4;i++) { var w=new World(960,360,42);w.Scale=2.3;w.Angle=i*90;w.Spider=new V(120+i*240,185);w.State=i==3?Mood.Inspect:Mood.Rest;w.StateTime=.7;PetCanvas.DrawSpider(d,w,skin.Id); }
                }SavePng(v,960,360,Path.Combine(folder,"skin-"+skin.Id+".png"));
            }
        }
        static void RenderSkinSizes(string folder) {
            var visual=new DrawingVisual();using(var d=visual.RenderOpen()) {
                d.DrawRectangle(BPreview("#e8ede3"),null,new Rect(0,0,1200,1050));
                Text(d,"SKIN READABILITY / 100% AND 20%",20,12,24,Color.FromRgb(30,50,44));
                for(int i=0;i<SkinCatalog.All.Length;i++) {
                    var skin=SkinCatalog.All[i];double x=i%4*300,y=65+i/4*195;
                    d.PushTransform(new TranslateTransform(x,y));
                    d.DrawRectangle(BPreview(i%2==0?"#24313a":"#e0e6d9"),null,new Rect(3,0,294,160));
                    for(int size=0;size<2;size++) {
                        var w=new World(300,195,42);w.SetSpecies(skin.Id);w.Scale=size==0?1:.2;w.Angle=180;w.Spider=new V(size==0?90:220,78);
                        PetCanvas.DrawSpider(d,w,skin.Id);
                    }
                    Color ink=i%2==0?Colors.White:Color.FromRgb(30,50,44);
                    Text(d,"100%",70,137,11,ink);Text(d,"20%",208,137,11,ink);
                    Text(d,skin.Name,12,168,14,Color.FromRgb(30,50,44));d.Pop();
                }
            }SavePng(visual,1200,1050,Path.Combine(folder,"skins-sizes.png"));
        }
        static void CheckSkinRendering() {
            var hashes=new System.Collections.Generic.HashSet<string>();
            var silhouettes=new System.Collections.Generic.HashSet<string>();
            foreach(var skin in SkinCatalog.All) {
                for(int pose=0;pose<4;pose++) {
                var visual=new DrawingVisual();using(var d=visual.RenderOpen()) { var w=new World(400,400,42);w.Scale=2;w.Spider=new V(200,200);w.Angle=pose*90;w.State=pose==0?Mood.Rest:pose==1?Mood.Inspect:Mood.Groom;w.StateTime=pose<3?.78:2.42;PetCanvas.DrawSpider(d,w,skin.Id); }
                var bitmap=new RenderTargetBitmap(400,400,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);byte[] pixels=new byte[400*400*4];bitmap.CopyPixels(pixels,1600,0);
                int painted=0;bool edge=false;for(int y=0;y<400;y++) for(int x=0;x<400;x++) if(pixels[(y*400+x)*4+3]>20) { painted++;if(x==0 || y==0 || x==399 || y==399) edge=true; }
                Check(painted>1500 && !edge,"visible, unclipped skin artwork: "+skin.Id+" pose "+pose);
                if(pose==0) using(var sha=System.Security.Cryptography.SHA256.Create()) {
                    Check(hashes.Add(Convert.ToBase64String(sha.ComputeHash(pixels))),"skin has distinct rendered artwork: "+skin.Id);
                    byte[] alpha=new byte[400*400];for(int i=0;i<alpha.Length;i++) alpha[i]=pixels[i*4+3]>20?(byte)255:(byte)0;
                    Check(silhouettes.Add(Convert.ToBase64String(sha.ComputeHash(alpha))),"skin has distinct geometry, not only color: "+skin.Id);
                }
                }
            }
        }
        static void CaptureSettings(PetHost host,string folder) {
            // A fresh visual tree per image avoids WPF's incremental render state
            // omitting unchanged controls when a live Window is captured repeatedly.
            int tabCount;var probe=new PetSettingsWindow(host);tabCount=probe.Tabs.Items.Count;probe.Close();
            for(int i=0;i<tabCount;i++) {
                var settings=new PetSettingsWindow(host);settings.Tabs.SelectedIndex=i;
                settings.ShowActivated=false;settings.ShowInTaskbar=false;settings.Show();settings.UpdateLayout();
                var content=(FrameworkElement)settings.Content;
                SavePng(settings,(int)Math.Ceiling(content.ActualWidth+content.Margin.Left+content.Margin.Right),(int)Math.Ceiling(content.ActualHeight+content.Margin.Top+content.Margin.Bottom),Path.Combine(folder,"settings-"+i+".png"));
                settings.Close();
            }
        }
    }
}
