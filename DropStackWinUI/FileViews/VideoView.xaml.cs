using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DropStackWinUI.FileViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoView : WinUIEx.WindowEx
    {
        public VideoView(string source)
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBarRectangle);
            if (getText("xRTL") == "true") EverythingGrid.FlowDirection = FlowDirection.RightToLeft;
            VideoViewerComponent.FlowDirection = FlowDirection.LeftToRight;

            setImageSource(source);
        }

        public string getText(string key)
        {
            Windows.ApplicationModel.Resources.ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();
            return loader.GetString(key);
        }

        public async void setImageSource(string source)
        {
            MediaPlayer mediaPlayer = new MediaPlayer();
            StorageFile file = await StorageFile.GetFileFromPathAsync(source);
            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            mediaPlayer.Source = MediaSource.CreateFromStream(stream, file.ContentType);
            VideoViewerComponent.SetMediaPlayer(mediaPlayer);
            mediaPlayer.Play();
        }
    }
}
