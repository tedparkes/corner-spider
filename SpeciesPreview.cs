using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace CornerSpider {
    public static partial class RuntimeTests {
        public static void RenderPalpGallery(string folder) {
            var visual=new DrawingVisual();string[] skins={"BoldFemale","Bold","PortiaLabiata","HyllusDiardiGreen"};
            double[] times={0,.48,.78,1.26,2.42,3.5};
            string[] captions={"Rest","Sideways reach","Left eye wipe","Chelicera stroke","Right eye wipe","Rest again"};
            using(var d=visual.RenderOpen()) {
                d.DrawRectangle(BPreview("#e6ebe1"),null,new Rect(0,0,1500,920));
                Text(d,"PALP GROOMING / FACE CLOSE-UPS",20,14,24,Color.FromRgb(30,50,44));
                for(int row=0;row<skins.Length;row++) {
                    Text(d,SkinCatalog.Get(skins[row]).Name,18,60+row*212,18,Color.FromRgb(30,50,44));
                    for(int col=0;col<times.Length;col++) {
                        d.PushTransform(new TranslateTransform(col*250,88+row*212));
                        d.PushClip(new RectangleGeometry(new Rect(4,0,242,158)));
                        var w=new World(1000,1000,42);w.Scale=4.5;w.Spider=new V(125,179);w.Angle=0;w.State=Mood.Groom;w.StateTime=times[col];
                        PetCanvas.DrawSpider(d,w,skins[row]);d.Pop();Text(d,captions[col],18,166,14,Color.FromRgb(30,50,44));d.Pop();
                    }
                }
            }SavePng(visual,1500,920,Path.Combine(folder,"palps-grooming.png"));
        }
    }
}
