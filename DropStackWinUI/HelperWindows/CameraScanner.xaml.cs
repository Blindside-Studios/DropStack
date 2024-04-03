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
using Windows.Devices.Perception;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using Windows.UI.Core;
using Microsoft.Web.WebView2.Core;

namespace DropStackWinUI.HelperWindows
{
    public class ScannedImage: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private BitmapImage _displayedImage;
        public BitmapImage DisplayedImage
        {
            get { return _displayedImage; }
            set
            {
                if (value != _displayedImage)
                {
                    _displayedImage = value;
                    OnPropertyChanged(nameof(DisplayedImage));
                }
            }
        }

        private double _gridOpacity;
        public double GridOpacity
        {
            get { return _gridOpacity; }
            set
            {
                if (value != _gridOpacity)
                {
                    _gridOpacity = value;
                    OnPropertyChanged(nameof(GridOpacity));
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        private double _animationTargetXPosition;
        public double AnimationTargetXPosition
        {
            get { return _animationTargetXPosition; }
            set
            {
                if (_animationTargetXPosition != value)
                {
                    _animationTargetXPosition = value;
                    OnPropertyChanged(nameof(AnimationTargetXPosition));
                }
            }
        }

        private double _animationTargetYPosition;
        public double AnimationTargetYPosition
        {
            get { return _animationTargetYPosition; }
            set
            {
                if (_animationTargetYPosition != value)
                {
                    _animationTargetYPosition = value;
                    OnPropertyChanged(nameof(AnimationTargetYPosition));
                }
            }
        }

        private double _animationTargetXScale;
        public double AnimationTargetXScale
        {
            get { return _animationTargetXScale; }
            set
            {
                if (_animationTargetXScale != value)
                {
                    _animationTargetXScale = value;
                    OnPropertyChanged(nameof(AnimationTargetXScale));
                }
            }
        }

        private double _animationTargetYScale;
        public double AnimationTargetYScale
        {
            get { return _animationTargetYScale; }
            set
            {
                if (_animationTargetYScale != value)
                {
                    _animationTargetYScale = value;
                    OnPropertyChanged(nameof(AnimationTargetYScale));
                }
            }
        }

        private double _animationRotation;
        public double AnimationRotation
        {
            get { return _animationRotation; }
            set
            {
                if (_animationRotation != value)
                {
                    _animationRotation = value;
                    OnPropertyChanged(nameof(AnimationRotation));
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public sealed partial class CameraScanner : Window
    {
        private MediaCapture mediaCaptureManager;
        private MediaFrameReader mediaFrameReader;
        private bool captureManagerInitialized = false;

        private Image imagePreviewElement;
        private SoftwareBitmap backBitmapBuffer;
        private bool taskFrameRenderRunning = false;

        private bool liveUpdatesRequired = true;

        public ViewModel CameraScannerViewModel { get; set; }

        public CameraScanner()
        {
            this.InitializeComponent();
            CameraScannerViewModel = new ViewModel();
            
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBarRectangle);

            this.StartPreviewAsync();

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
                    SharingMode = MediaCaptureSharingMode.ExclusiveControl,
                    StreamingCaptureMode = StreamingCaptureMode.Video,
                    MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                    MediaCategory = MediaCategory.Media,
                    AlwaysPlaySystemShutterSound = false
                };

                await mediaCaptureManager.InitializeAsync(settings);

                // Get the highest resolution
                var resolutions = mediaCaptureManager.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview).Select(x => x as VideoEncodingProperties).OrderByDescending(x => x.Width);
                var highestResolution = resolutions.First();

                // Set the highest resolution
                await mediaCaptureManager.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, highestResolution);

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
            if (liveUpdatesRequired)
            {
                var mediaFrameReference = sender.TryAcquireLatestFrame();
                var videoMediaFrame = mediaFrameReference?.VideoMediaFrame;
                var softwareBitmap = videoMediaFrame?.SoftwareBitmap;

                if (softwareBitmap != null)
                {
                    if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode != BitmapAlphaMode.Premultiplied)
                    {
                        // Trust me, disposing of the old frame here is important because otherwise the app will run into a memory leak!
                        // The garbage collector doesn't immediately dispose of the object, it just knows it's there - and then for some
                        // reason isn't fast enough when it would need to be deleted.
                        // Basically, think of it like your trash can overflowing because the garbage truck didn't come by your house yet.
                        var oldSoftwareBitmap = softwareBitmap;
                        softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                        oldSoftwareBitmap.Dispose();
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
            else
            {
                // The app is currently freezing a frame that is going to be transported into the image viewer carousel.
            }
        }

        async private void InitCamera_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mediaCaptureManager = new MediaCapture();
                await mediaCaptureManager.InitializeAsync();
                Debug.WriteLine("Camera has Initialized.");
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
                // don't allow the user to take a picture now, it might mess with the animation and visibility of GridViewItems
                CapturePhotoButton.IsEnabled = false;
                FlashAnimation.Begin();
                liveUpdatesRequired = false;

                ImageEncodingProperties imgFormat = ImageEncodingProperties.CreateJpeg();

                // Create storage file in local app storage
                StorageFile storageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                    "TestPhoto.jpg", CreationCollisionOption.GenerateUniqueName);

                // Take photo
                await mediaCaptureManager.CapturePhotoToStorageFileAsync(imgFormat, storageFile);

                // Get photo as a BitmapImage
                BitmapImage bmpImage = new BitmapImage(new Uri(storageFile.Path));


                // add image to collection at the bottom
                if (GalleryGridView.Items.Count == 0) GalleryGridView.ItemsSource = new ObservableCollection<ScannedImage>();

                var collection = GalleryGridView.ItemsSource as ObservableCollection<ScannedImage>;

                collection.Add(new ScannedImage() { DisplayedImage = bmpImage, GridOpacity = 0 });

                animatedImage.Source = bmpImage;

                await Task.Delay(200);

                // Scroll the scrollviewer to the end
                double horizontalOffset = GalleryScrollViewer.ScrollableWidth; // The maximum horizontal offset
                double verticalOffset = GalleryScrollViewer.ScrollableHeight; // The maximum vertical offset
                bool disableAnimation = false; // scroll with animation

                GalleryScrollViewer.ChangeView(horizontalOffset, verticalOffset, null, disableAnimation);


                await Task.Delay(500);
                // Now get the container position
                getContainerPosition();

                // Continue with setting the source and starting the animation
                liveUpdatesRequired = true;
                // this may have been collapsed before so make it visible again
                animationPreview.Visibility = Visibility.Visible;
                CameraFeedToGalleryAnimation.Begin();
            }
            catch (Exception Exc)
            {
                Debug.WriteLine(Exc.Message);
            }
        }
        private void getContainerPosition()
        {
            var newestIndex = GalleryGridView.Items.Count - 1;
            if (newestIndex > -1)
            {
                var gridItem = GalleryGridView.ContainerFromIndex(newestIndex) as GridViewItem;

                GeneralTransform transform = gridItem.TransformToVisual(MainGrid);
                Point position = transform.TransformPoint(new Point(0, 0));

                GeneralTransform transformImage = imagePreview.TransformToVisual(MainGrid);
                Point positionImage = transformImage.TransformPoint(new Point(0, 0));

                CameraScannerViewModel.AnimationTargetXScale = gridItem.ActualSize.X / imagePreview.ActualSize.X;
                CameraScannerViewModel.AnimationTargetYScale = gridItem.ActualSize.Y / imagePreview.ActualSize.Y;

                // hardcoded added values are because this seems to ignore margins
                CameraScannerViewModel.AnimationTargetXPosition = ((position.X - positionImage.X) - (imagePreview.ActualSize.X / 2) + (gridItem.ActualSize.X / 2)) / CameraScannerViewModel.AnimationTargetXScale + 25;
                CameraScannerViewModel.AnimationTargetYPosition = position.Y - positionImage.Y + 30;

                var rotation = - 5 * (2 / Math.PI) * Math.Atan(CameraScannerViewModel.AnimationTargetXPosition/100);
                CameraScannerViewModel.AnimationRotation = rotation;
                Debug.WriteLine(rotation);
            }
            else
            {
                CameraScannerViewModel.AnimationTargetXPosition = 0;
                CameraScannerViewModel.AnimationTargetYPosition = 0;
                CameraScannerViewModel.AnimationTargetXScale = 1;
                CameraScannerViewModel.AnimationTargetYScale = 1;
            }
        }

        private void CameraFeedToGalleryAnimation_Completed(object sender, object e)
        {
            var collection = GalleryGridView.ItemsSource as ObservableCollection<ScannedImage>;
            collection.Last().GridOpacity = 1;
            animationPreview.Opacity = 0;
            // collapse visibility so this is no longer in the way of the carousel
            animationPreview.Visibility = Visibility.Collapsed;
            // has previously been disabled to not interfere with the animation
            CapturePhotoButton.IsEnabled = true;
        }
    }
}
