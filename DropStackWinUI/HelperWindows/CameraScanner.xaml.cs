using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Capture.Frames;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace DropStackWinUI.HelperWindows
{
    public sealed partial class CameraScanner : Window
    {
        private MediaCapture mediaCaptureManager;
        private MediaFrameReader mediaFrameReader;
        private bool captureManagerInitialized = false;

        private Image imagePreviewElement;
        private SoftwareBitmap backBitmapBuffer;
        private bool taskFrameRenderRunning = false;

        public CameraScanner()
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBarRectangle);

            //this.StartPreviewAsync();

            Closed += MainWindow_Closed;
        }

        private async void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            await this.CleanupMediaCaptureAsync();
        }

        private async void StartPreviewAsync()
        {
            if (captureManagerInitialized == true)
            {
                return;
            }

            try
            {
                //1. Select frame sources and frame source groups//
                var frameSourceGroups = await MediaFrameSourceGroup.FindAllAsync();
                if (frameSourceGroups.Count <= 0)
                {
                    Debug.WriteLine("No source groups found.");
                    return;
                }

                // find a color camera located on the back of the device
                var selectedGroup = frameSourceGroups
                    .Select(group => new
                    {
                        Group = group,
                        SourceInfo = group.SourceInfos.FirstOrDefault(info =>
                            info.SourceKind == MediaFrameSourceKind.Color &&
                            info.DeviceInformation.EnclosureLocation?.Panel == Windows.Devices.Enumeration.Panel.Back)
                    })
                    .FirstOrDefault(g => g.SourceInfo != null);

                // if there is no such thing as a color camera located on the back of the device, use one on the front
                if (selectedGroup == null)
                {
                    selectedGroup = frameSourceGroups
                    .Select(group => new
                    {
                        Group = group,
                        SourceInfo = group.SourceInfos.FirstOrDefault(info =>
                            info.SourceKind == MediaFrameSourceKind.Color &&
                            info.DeviceInformation.EnclosureLocation?.Panel == Windows.Devices.Enumeration.Panel.Front)
                    })
                    .FirstOrDefault(g => g.SourceInfo != null);

                    // if we can't have that either, start crying...
                    if (selectedGroup == null)
                    {
                        Debug.WriteLine("No rear-facing color camera found.");
                        return;
                    }
                }

                MediaFrameSourceGroup selectedFrameSourceGroup = selectedGroup.Group;
                MediaFrameSourceInfo frameSourceInfo = selectedGroup.SourceInfo;

                //2. Initialize the MediaCapture object to use the selected frame source group//
                mediaCaptureManager = new MediaCapture();
                var settings = new MediaCaptureInitializationSettings
                {
                    SourceGroup = selectedFrameSourceGroup,
                    SharingMode = MediaCaptureSharingMode.SharedReadOnly,
                    StreamingCaptureMode = StreamingCaptureMode.Video,
                    MemoryPreference = MediaCaptureMemoryPreference.Cpu
                };

                await mediaCaptureManager.InitializeAsync(settings);

                //3. Initialize Image Preview Element with xaml Image Element.//
                imagePreviewElement = imagePreview;
                imagePreviewElement.Source = new SoftwareBitmapSource();

                //4. Create a frame reader for the frame source//
                MediaFrameSource mediaFrameSource = mediaCaptureManager.FrameSources[frameSourceInfo.Id];
                mediaFrameReader = await mediaCaptureManager.CreateFrameReaderAsync(mediaFrameSource, MediaEncodingSubtypes.Argb32);
                mediaFrameReader.FrameArrived += FrameReader_FrameArrived;
                await mediaFrameReader.StartAsync();

                captureManagerInitialized = true;
                Debug.WriteLine("Media preview from device: " + selectedFrameSourceGroup.DisplayName);

            }
            catch (Exception Exc)
            {
                Debug.WriteLine("MediaCapture initialization failed: " + Exc.Message);
            }
        }

        private async Task CleanupMediaCaptureAsync()
        {
            if (mediaCaptureManager != null)
            {
                using (var mediaCapture = mediaCaptureManager)
                {
                    mediaCaptureManager = null;

                    mediaFrameReader.FrameArrived -= FrameReader_FrameArrived;
                    await mediaFrameReader.StopAsync();
                    mediaFrameReader.Dispose();
                }
            }

            captureManagerInitialized = false;
            Debug.WriteLine("Media preview has canceled.");
        }

        private void FrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            var mediaFrameReference = sender.TryAcquireLatestFrame();
            var videoMediaFrame = mediaFrameReference?.VideoMediaFrame;
            var softwareBitmap = videoMediaFrame?.SoftwareBitmap;

            if (softwareBitmap != null)
            {
                if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
                    softwareBitmap.BitmapAlphaMode != BitmapAlphaMode.Premultiplied)
                {
                    softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                }

                // Swap the processed frame to backBuffer and dispose of the unused image.
                softwareBitmap = Interlocked.Exchange(ref backBitmapBuffer, softwareBitmap);
                softwareBitmap?.Dispose();

                // Changes to XAML ImageElement must happen on UI thread through Dispatcher
                //var task = imagePreviewElement.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                _ = imagePreviewElement.DispatcherQueue.TryEnqueue(async () =>
                {
                    // Don't let two copies of this task run at the same time.
                    if (taskFrameRenderRunning)
                    {
                        return;
                    }
                    taskFrameRenderRunning = true;

                    // Keep draining frames from the backbuffer until the backbuffer is empty.
                    SoftwareBitmap latestBitmap;
                    while ((latestBitmap = Interlocked.Exchange(ref backBitmapBuffer, null)) != null)
                    {
                        var imageSource = (SoftwareBitmapSource)imagePreviewElement.Source;
                        await imageSource.SetBitmapAsync(latestBitmap);
                        latestBitmap.Dispose();
                    }

                    taskFrameRenderRunning = false;
                });
            }

            if (mediaFrameReference != null)
            {
                mediaFrameReference.Dispose();
            }
        }

        async private void InitCamera_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //mediaCaptureManager = new MediaCapture();
                //await mediaCaptureManager.InitializeAsync();
                //TxtActivityLog.Text = "Camera has Initialized.";

                throw new NotImplementedException();
            }
            catch (Exception Exc)
            {
                Debug.WriteLine(Exc.Message);
            }
        }

        async private void StartCapturePreview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //capturePreview.Source = mediaCaptureManager;
                //await mediaCaptureManager.StartPreviewAsync();   
                //TxtActivityLog.Text = "Media Preview has started.";

                this.StartPreviewAsync();
            }
            catch (Exception Exc)
            {
                Debug.WriteLine(Exc.Message);
            }
        }

        async private void StopCapturePreview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //await mediaCaptureManager.StopPreviewAsync();
                //mediaCaptureManager.Dispose();
                //TxtActivityLog.Text = "Media Preview has canceled.";

                await CleanupMediaCaptureAsync();
            }
            catch (Exception Exc)
            {
                Debug.WriteLine(Exc.Message);
            }
        }

        async private void CapturePhoto_Click(object sender, RoutedEventArgs e)
        {
            if (captureManagerInitialized == false)
            {
                return;
            }

            try
            {
                ImageEncodingProperties imgFormat = ImageEncodingProperties.CreateJpeg();

                // Create storage file in local app storage
                StorageFile storageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                    "TestPhoto.jpg", CreationCollisionOption.GenerateUniqueName);

                // Take photo
                await mediaCaptureManager.CapturePhotoToStorageFileAsync(imgFormat, storageFile);

                // Get photo as a BitmapImage
                BitmapImage bmpImage = new BitmapImage(new Uri(storageFile.Path));

                // ImagePreview is a <Image> object defined in XAML
                imageCapture.Source = bmpImage;

                Debug.WriteLine("Media Photo has Captured.");
            }
            catch (Exception Exc)
            {
                Debug.WriteLine(Exc.Message);
            }
        }
    }
}
