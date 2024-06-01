using Microsoft.UI.Xaml;
using System;
using Windows.Storage;
using Windows.UI.ViewManagement;

namespace DropStackWinUI.FileViews
{
    public sealed partial class PDFView : WinUIEx.WindowEx
    {
        public PDFView(string path)
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBarGrid);
            WebViewRendererComponent.Source = new Uri("file:///" + path);
            applyTitlebarText(path);
            if (getText("xRTL") == "true") EverythingGrid.FlowDirection = FlowDirection.RightToLeft;
            WebViewRendererComponent.FlowDirection = FlowDirection.LeftToRight;
        }

        public string getText(string key)
        {
            Windows.ApplicationModel.Resources.ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();
            return loader.GetString(key);
        }

        public async void applyTitlebarText(string path)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(path);
            TitleBarText.Text = file.Name + " - DropStack";
            Title = file.DisplayName;
        }

    }
}
