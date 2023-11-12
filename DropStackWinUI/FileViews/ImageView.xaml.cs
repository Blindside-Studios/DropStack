using Microsoft.UI.Windowing;
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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.FileProperties;
using WinUIEx;

namespace DropStackWinUI.FileViews
{
    public sealed partial class ImageView : WinUIEx.WindowEx
    {
        [DllImport("User32.dll")]
        public static extern uint GetDpiForWindow(IntPtr hwnd);

        public ImageView(ImageViewerSettings settings)
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBarRectangle);

            setImageSource(settings);
        }

        public async void setImageSource(ImageViewerSettings settings)
        {
            BitmapImage viewedImage = new BitmapImage();

            StorageFile file = await StorageFile.GetFileFromPathAsync(settings.filePath);
            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            viewedImage.SetSource(stream);
            ImageViewerComponent.Source = viewedImage;


            int contentWidth = viewedImage.PixelWidth;
            int contentHeight = viewedImage.PixelHeight;

            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Nearest);

            uint dpi = GetDpiForWindow(hWnd);
            double scaleFactor = (double)dpi / 96;

            int displayWidth = displayArea.WorkArea.Width;
            int displayHeight = displayArea.WorkArea.Height;

            ImageViewerComponent.Height = contentHeight;
            ImageViewerComponent.Width = contentWidth;

            if (displayHeight < ImageViewerComponent.Height * scaleFactor)
            {
                ImageViewerComponent.Height = (int)Math.Round(displayHeight * 0.9 / scaleFactor, 0);
                ImageViewerComponent.Width = ImageViewerComponent.Height * contentWidth / contentHeight;
            }
            if (displayWidth < ImageViewerComponent.Width * scaleFactor)
            {
                ImageViewerComponent.Width = (int)Math.Round(displayWidth * 0.9 / scaleFactor, 0);
                ImageViewerComponent.Height = ImageViewerComponent.Width * contentHeight / contentWidth;
            }

            /*this.MoveAndResize(
                (displayWidth / 2) - ((ImageViewerComponent.Width * scaleFactor) / 2),
                displayHeight - (ImageViewerComponent.Height * scaleFactor) - (10 * scaleFactor),
                ImageViewerComponent.Width,
                ImageViewerComponent.Height);*/

            this.MoveAndResize( 100, 100, ImageViewerComponent.Width, ImageViewerComponent.Height);
        }
    }
}
