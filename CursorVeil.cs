using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace CornerSpider {
    // A short-lived helper owns the visibility lease. The UI must renew it;
    // parent exit or a stalled UI restores the cursor without changing its scheme.
    public sealed class CursorVeil : IDisposable {
        [DllImport("Magnification.dll")] static extern bool MagInitialize();
        [DllImport("Magnification.dll")] static extern bool MagUninitialize();
        [DllImport("Magnification.dll")] static extern bool MagShowSystemCursor(bool show);
        Process helper;
        EventWaitHandle pulse,stop,ready;
        bool stopping,disposed;
        DateTime retryAfter=DateTime.MinValue;
        public int HelperId { get { return helper==null?0:helper.Id; } }
        public bool Hidden { get { return ready!=null && !stopping && !helper.HasExited && ready.WaitOne(0); } }
        void CloseHandles() {
            if(helper!=null) helper.Dispose();if(pulse!=null) pulse.Dispose();if(stop!=null) stop.Dispose();if(ready!=null) ready.Dispose();
            helper=null;pulse=stop=ready=null;stopping=false;
        }
        public void Update(bool hide) {
            if(disposed) return;
            if(helper!=null && helper.HasExited) { if(helper.ExitCode!=0) { RestoreEmergency();retryAfter=DateTime.UtcNow.AddSeconds(30); }CloseHandles(); }
            if(!hide) { Release();return; }
            if(stopping) return;
            if(helper==null) {
                if(DateTime.UtcNow<retryAfter) return;
                string name="Local\\CornerSpiderCursor_"+Guid.NewGuid().ToString("N");
                pulse=new EventWaitHandle(false,EventResetMode.AutoReset,name+"_pulse");
                stop=new EventWaitHandle(false,EventResetMode.ManualReset,name+"_stop");
                ready=new EventWaitHandle(false,EventResetMode.ManualReset,name+"_ready");
                try {
                    helper=Process.Start(new ProcessStartInfo(Process.GetCurrentProcess().MainModule.FileName,"--cursor-guard "+Process.GetCurrentProcess().Id+" "+name) { UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden });
                } catch { CloseHandles();retryAfter=DateTime.UtcNow.AddSeconds(30);return; }
            }
            pulse.Set();
        }
        public bool WaitUntilHidden(int milliseconds) { return ready!=null && ready.WaitOne(milliseconds) && Hidden; }
        public void Release() { if(stop!=null) { stopping=true;stop.Set(); } }
        public bool WaitForRestore(int milliseconds) { return helper==null || (helper.WaitForExit(milliseconds) && helper.ExitCode==0); }
        public void Dispose() {
            if(disposed) return;disposed=true;Release();
            if(helper!=null && !helper.WaitForExit(1500)) {
                RestoreEmergency();try { helper.Kill();helper.WaitForExit(1000); } catch(InvalidOperationException) {}
            }
            if(helper!=null && helper.HasExited && helper.ExitCode!=0) RestoreEmergency();
            CloseHandles();
        }
        public static void RestoreEmergency() {
            if(MagInitialize()) { try { MagShowSystemCursor(true); } finally { MagUninitialize(); } }
        }
        public static int Guard(string[] args) {
            bool initialized=false,hidden=false,restored=true;
            try {
                using(var parent=Process.GetProcessById(int.Parse(args[1])))
                using(var pulse=EventWaitHandle.OpenExisting(args[2]+"_pulse"))
                using(var stop=EventWaitHandle.OpenExisting(args[2]+"_stop"))
                using(var ready=EventWaitHandle.OpenExisting(args[2]+"_ready")) {
                    initialized=MagInitialize();if(!initialized) return 2;
                    var signals=new WaitHandle[]{stop,pulse};
                    while(!parent.HasExited && WaitHandle.WaitAny(signals,750)==1) {
                        if(!hidden) { hidden=MagShowSystemCursor(false);if(!hidden) return 3;ready.Set(); }
                    }
                }
            } catch { return 2; }
            finally {
                if(initialized) { if(hidden) restored=MagShowSystemCursor(true);MagUninitialize(); }
            }
            return restored?0:4;
        }
        // Explicit diagnostics fixture. No overlay or user's settings are touched.
        public static int Fixture(string path) {
            using(var lease=new CursorVeil()) {
                lease.Update(true);if(!lease.WaitUntilHidden(1500)) return 2;
                File.WriteAllText(path,lease.HelperId.ToString());
                for(int i=0;i<200;i++) { lease.Update(true);Thread.Sleep(50); }
            }return 0;
        }
    }
}
