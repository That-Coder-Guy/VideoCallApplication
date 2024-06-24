/*
 * Webcam.cs
 * Author: Henry Glenn
 */


using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;

namespace VideoCallApplication
{
    public class Webcam
    {
        public VideoCaptureDevice? SelectedVideoDevice;

        public FilterInfo? SelectedInput;

        private KeyValuePair<FilterInfo, VideoCaptureDevice> _input = new();

        public List<FilterInfo> Inputs => FilterCollectionToList(new FilterInfoCollection(FilterCategory.VideoInputDevice));

        public delegate void NewFrameHandler(Bitmap frame);

        public event NewFrameHandler? OnNewFrame;

        public delegate void SelectedInputEjectedHandler(VideoCaptureDevice? device);

        public event SelectedInputEjectedHandler? OnSelectedInputEjected;

        private static List<FilterInfo> FilterCollectionToList(FilterInfoCollection collection)
        {
            List<FilterInfo> filters = new List<FilterInfo>();
            foreach (FilterInfo filter in collection)
            {
                filters.Add(filter);
            }
            return filters;
        }

        public static bool TryRemoveFilter(FilterInfo target, List<FilterInfo> collection)
        {
            foreach (FilterInfo filter in collection)
            {
                if (filter.MonikerString == target.MonikerString)
                {
                    collection.Remove(filter);
                    return true;
                }
            }
            return false;
        }

        private void OnPlayingFinished(object sender, ReasonToFinishPlaying reason)
        {
            if (reason == ReasonToFinishPlaying.DeviceLost)
            {
                TriggerSelectedInputEjectedEvent();
                SelectedInput = null;
                SelectedVideoDevice = null;
            }
        }

        public void Select(FilterInfo device)
        {
            SelectedInput = device;

            if (SelectedVideoDevice != null)
            {
                SelectedVideoDevice.SignalToStop();
                SelectedVideoDevice.WaitForStop();
            }

            SelectedVideoDevice = new VideoCaptureDevice(device.MonikerString);

            // Set video resolution to lowest possible
            VideoCapabilities? lowestResolution = SelectedVideoDevice.VideoCapabilities.OrderBy(cap => cap.FrameSize.Width * cap.FrameSize.Height).FirstOrDefault();
            Debug.Print(SelectedVideoDevice.VideoCapabilities.Length.ToString());
            if (lowestResolution != null)
            {
                SelectedVideoDevice.VideoResolution = lowestResolution;
            }

            SelectedVideoDevice.NewFrame += TriggerNewFrameEvent;
            SelectedVideoDevice.VideoSourceError += OnVideoSourceError;
            SelectedVideoDevice.PlayingFinished += OnPlayingFinished;
            SelectedVideoDevice.Start();
        }

        public void Deselect()
        {
            SelectedInput = null;

            if (SelectedVideoDevice != null)
            {
                SelectedVideoDevice.SignalToStop();
                SelectedVideoDevice.WaitForStop();
            }
            SelectedVideoDevice = null;
        }

        private void OnVideoSourceError(object sender, VideoSourceErrorEventArgs e)
        {
            Console.WriteLine("Video source error: " + e.Description);
            Deselect();
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

        private static Bitmap CompressBitmap(Bitmap bitmap)
        {
            // Calculate the scaling factor
            double scale = Math.Min(300.0 / bitmap.Width, 300.0 / bitmap.Height);

            // Calculate the new width and height
            int newWidth = (int)(bitmap.Width * scale);
            int newHeight = (int)(bitmap.Height * scale);

            // Resize the bitmap
            Bitmap resizedBitmap = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(resizedBitmap))
            {
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                g.DrawImage(bitmap, 0, 0, newWidth, newHeight);
            }
            return resizedBitmap;
        }

        private void TriggerNewFrameEvent(object? sender, NewFrameEventArgs e)
        {
            if (OnNewFrame != null)
            {
                // Get all attached event handlers
                Delegate[] handlers = OnNewFrame.GetInvocationList();

                // Compress original frame
                Bitmap frame = CompressBitmap((Bitmap)e.Frame.Clone());

                // Invoke each event handler on the UI thread
                foreach (NewFrameHandler handler in handlers)
                {
                    handler.Invoke((Bitmap)frame.Clone());
                }
            }
        }

        private void TriggerSelectedInputEjectedEvent()
        {
            if (OnSelectedInputEjected != null)
            {
                // Get all attached event handlers
                Delegate[] handlers = OnSelectedInputEjected.GetInvocationList();

                // Invoke each event handler on the UI thread
                foreach (SelectedInputEjectedHandler handler in handlers)
                {
                    handler.Invoke(SelectedVideoDevice);
                }
            }
        }
    }
}
