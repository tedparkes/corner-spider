using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace CornerSpider {
    // Observes button-down edges only. Never captures, suppresses or synthesizes input.
    // The callback does no drawing, file IO or waiting and always calls the next hook.
    public sealed class MouseClicks : IDisposable {
        delegate IntPtr HookProc(int code,IntPtr message,IntPtr data);
        [DllImport("user32.dll",SetLastError=true)] static extern IntPtr SetWindowsHookEx(int type,HookProc proc,IntPtr module,uint thread);
        [DllImport("user32.dll")] static extern IntPtr CallNextHookEx(IntPtr hook,int code,IntPtr message,IntPtr data);
        [DllImport("user32.dll")] static extern bool UnhookWindowsHookEx(IntPtr hook);
        [DllImport("kernel32.dll",CharSet=CharSet.Unicode)] static extern IntPtr GetModuleHandle(string name);
        [DllImport("kernel32.dll")] static extern uint GetCurrentThreadId();
        [DllImport("user32.dll")] static extern short GetAsyncKeyState(int key);
        [StructLayout(LayoutKind.Sequential)]
        struct Message {
            public IntPtr window;public uint message;public UIntPtr wParam;public IntPtr lParam;
            public uint time;public int x,y;public uint privateData;
        }
        [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern bool PeekMessage(out Message message,IntPtr window,uint first,uint last,uint remove);
        [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern int GetMessage(out Message message,IntPtr window,uint first,uint last);
        [DllImport("user32.dll")] static extern bool TranslateMessage(ref Message message);
        [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern IntPtr DispatchMessage(ref Message message);
        [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern bool PostThreadMessage(uint thread,uint message,UIntPtr wParam,IntPtr lParam);
        readonly HookProc callback;
        readonly Thread worker;
        readonly ManualResetEvent ready=new ManualResetEvent(false);
        static readonly int[] keys={1,2,4,5,6};
        volatile IntPtr hook;
        volatile uint threadId;
        volatile bool stopping;
        int pending,lastButtons;
        public bool Installed { get { return hook!=IntPtr.Zero; } }
        public bool WorkerAlive { get { return worker.IsAlive; } }
        public bool Disposed { get;private set; }
        public bool Released { get;private set; }
        public MouseClicks() {
            callback=Observe;lastButtons=Buttons();
            worker=new Thread(ObserveLoop);worker.IsBackground=true;worker.Name="Corner Spider mouse observer";worker.Start();
            if(!ready.WaitOne(2000)) StopLoop();
        }
        void ObserveLoop() {
            // Low-level hooks are delivered to their installing thread. Keep that thread
            // pumping even while WPF renders a frame, saves settings or opens a dialog.
            try {
                Message message;PeekMessage(out message,IntPtr.Zero,0,0,0);
                threadId=GetCurrentThreadId();
                if(!stopping) hook=SetWindowsHookEx(14,callback,GetModuleHandle(null),0);
                ready.Set();
                while(!stopping && hook!=IntPtr.Zero && GetMessage(out message,IntPtr.Zero,0,0)>0) {
                    TranslateMessage(ref message);DispatchMessage(ref message);
                }
            } finally {
                Released=hook==IntPtr.Zero || UnhookWindowsHookEx(hook);hook=IntPtr.Zero;
                ready.Set();
            }
        }
        void StopLoop() {
            stopping=true;
            if(threadId!=0) PostThreadMessage(threadId,0x12,UIntPtr.Zero,IntPtr.Zero);
        }
        static int Buttons() {
            int mask=0;for(int i=0;i<keys.Length;i++) if((GetAsyncKeyState(keys[i])&0x8000)!=0) mask|=1<<i;return mask;
        }
        public static bool IsButtonDown(int message) { return message==0x201 || message==0x204 || message==0x207 || message==0x20b; }
        internal void Record(int code,int message) { if(code>=0 && IsButtonDown(message)) Interlocked.Exchange(ref pending,1); }
        IntPtr Observe(int code,IntPtr message,IntPtr data) { Record(code,message.ToInt32());return CallNextHookEx(hook,code,message,data); }
        public bool Consume() {
            if(Disposed) return false;
            int buttons=Buttons();bool edge=(buttons&~lastButtons)!=0;lastButtons=buttons;
            return Interlocked.Exchange(ref pending,0)!=0 || edge;
        }
        public void Dispose() {
            if(Disposed) return;Disposed=true;StopLoop();
            if(worker.Join(2000)) ready.Dispose();
            Interlocked.Exchange(ref pending,0);
        }
    }
}
