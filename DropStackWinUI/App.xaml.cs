using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DropStackWinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        public static Window Window { get { return m_window; } }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            if (args.Arguments.Contains("resetSecondaryPortal")) resetSecondaryPortalFolders();
            if (args.Arguments.Contains("resetDropStack"))
            {
                resetSecondaryPortalFolders();
                resetAllOtherSettings();
            }
            
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            string folderToken = ApplicationData.Current.LocalSettings.Values["FolderToken"] as string;
            string pinnedFolderToken = ApplicationData.Current.LocalSettings.Values["PinnedFolderToken"] as string;

            bool shouldBeSimpleView = false;

            if (localSettings.Values.ContainsKey("LoadSimpleViewBoolean"))
            {
                if ((bool)localSettings.Values["LoadSimpleViewBoolean"] == false) shouldBeSimpleView = false;
                else if ((bool)localSettings.Values["LoadSimpleViewBoolean"] == true) shouldBeSimpleView = true;
            }

            if (string.IsNullOrEmpty(folderToken) || string.IsNullOrEmpty(pinnedFolderToken)) shouldBeSimpleView = false;

            if (args.Arguments.Contains("forceNormalView")) shouldBeSimpleView = false;
            else if (args.Arguments.Contains("forceSimpleView")) shouldBeSimpleView = true;

            if (shouldBeSimpleView) m_window = new SimpleMode();
            else if (!shouldBeSimpleView) m_window = new MainWindow();
            m_window.Activate();
        }

        private void resetSecondaryPortalFolders()
        {
            ApplicationData.Current.LocalSettings.Values["SecFolderToken1"] = null;
            ApplicationData.Current.LocalSettings.Values["SecFolderToken2"] = null;
            ApplicationData.Current.LocalSettings.Values["SecFolderToken3"] = null;
            ApplicationData.Current.LocalSettings.Values["SecFolderToken4"] = null;
            ApplicationData.Current.LocalSettings.Values["SecFolderToken5"] = null;

            ApplicationData.Current.LocalSettings.Values["showSecondaryPortal1"] = false;
            ApplicationData.Current.LocalSettings.Values["showSecondaryPortal2"] = false;
            ApplicationData.Current.LocalSettings.Values["showSecondaryPortal3"] = false;
            ApplicationData.Current.LocalSettings.Values["showSecondaryPortal4"] = false;
            ApplicationData.Current.LocalSettings.Values["showSecondaryPortal5"] = false;
        }

        private void resetAllOtherSettings()
        {
            ApplicationData.Current.LocalSettings.Values["FolderToken"] = null;
            ApplicationData.Current.LocalSettings.Values["PinnedFolderToken"] = null;
            ApplicationData.Current.LocalSettings.Values["LoadSimpleViewBoolean"] = false;
            ApplicationData.Current.LocalSettings.Values["PinBarBehavior"] = 0;
            ApplicationData.Current.LocalSettings.Values["HasPinBarBeenExpanded"] = true;
            ApplicationData.Current.LocalSettings.Values["IsPanosUnlocked"] = false;
            ApplicationData.Current.LocalSettings.Values["SelectedTheme"] = "Default";


        }

        private static Window m_window;
    }
}
