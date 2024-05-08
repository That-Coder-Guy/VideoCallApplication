using Emgu.CV;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace VideoCallApplication
{
    public class Webcam
    {
        private VideoCapture _videoCapture = new VideoCapture();
        private DispatcherTimer _frameTimer;
        private int _framesPerSecond = 60;

        public delegate void OnNewFrameHandler(Bitmap frame);

        public event OnNewFrameHandler? OnNewFrame;

        public Webcam()
        {
            _frameTimer = new DispatcherTimer();
            _frameTimer.Interval = TimeSpan.FromMilliseconds(1000 / _framesPerSecond);
            _frameTimer.Tick += TriggerNewFrameEvent;
        }

        public Bitmap GetFrame()
        {
            return _videoCapture.QueryFrame().ToBitmap();
        }

        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = memory;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                return image;
            }
        }

        public void Start()
        {
            _frameTimer.Start();
        }

        public void Stop()
        {
            _frameTimer.Stop();
        }

        private void TriggerNewFrameEvent(object? sender, EventArgs e)
        {
            if (OnNewFrame != null)
            {
                // Get the current frame from the webcam
                Bitmap frame = GetFrame();

                // Get all attached event handlers
                Delegate[] handlers = OnNewFrame.GetInvocationList();

                // Invoke each event handler on the UI thread
                foreach (OnNewFrameHandler handler in handlers)
                {
                    handler.Invoke(frame);
                }
            }
        }
    }
}
