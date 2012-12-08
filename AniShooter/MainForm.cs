using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;

namespace AniShooter
{
    public partial class MainForm : Form
    {
        IDisposable subscription;
        private readonly KeyboardHookListener _keyboardHookManager;

        public MainForm()
        {
            InitializeComponent();
            double sec = 0.5;

            _keyboardHookManager = new KeyboardHookListener(new GlobalHooker());
            _keyboardHookManager.Enabled = true;

            var hotKey = Observable.FromEventPattern<KeyPressEventArgs>(_keyboardHookManager, "KeyPress").Where(ep => ep.EventArgs.KeyChar == 'a').Do(ep => { ep.EventArgs.Handled = true; Trace.WriteLine("hotkey handled"); });
            var generator = Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(sec)).Select(s => CaptureScreen());
            subscription = generator.TakeUntil(hotKey.Throttle(TimeSpan.FromSeconds(sec * 2))).TakeLast(5)
                                    .Subscribe(bmp => saveBmp(bmp));
        }

        public void saveBmp(Bitmap bmp)
        {
            try
            {
                bmp.Save("D:\\shot" + DateTime.Now.Ticks + ".png", ImageFormat.Png);
                Trace.WriteLine("saved");
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public static Bitmap CaptureScreen()
        {
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
            return bmpScreenshot;
        }
    }
}
