using DropStackWinUI.HelperWindows;
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
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using System.Diagnostics;

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
        bool shouldLaunchWindow = true;

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            if (args.Arguments.Contains("resetSecondaryPortal")) resetSecondaryPortalFolders();
            if (args.Arguments.Contains("resetDropStack")) { resetSecondaryPortalFolders(); resetAllOtherSettings(); }
            if (args.Arguments.Contains("resetPerformanceSettings")) resetPerformanceSettings();
            if (args.Arguments.Contains("lowerPerformanceSettings")) engageUltraResourceSaverMode();

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            string folderToken = ApplicationData.Current.LocalSettings.Values["FolderToken"] as string;
            string pinnedFolderToken = ApplicationData.Current.LocalSettings.Values["PinnedFolderToken"] as string;

            int selectedView = 0;

            //checks for toggle state, this is kept to keep the user's choice from old app versions
            //also converts to new settings so it can be removed in future revisions
            if (localSettings.Values.ContainsKey("LoadSimpleViewBoolean"))
            {
                if ((bool)localSettings.Values["LoadSimpleViewBoolean"] == false)
                {
                    selectedView = 0; 
                    if (!localSettings.Values.ContainsKey("DefaultLaunchModeIndex"))
                    {
                        localSettings.Values["DefaultLaunchModeIndex"] = 0;
                    }
                }
                else if ((bool)localSettings.Values["LoadSimpleViewBoolean"] == true)
                {
                    selectedView = 1; 
                    if (!localSettings.Values.ContainsKey("DefaultLaunchModeIndex"))
                    {
                        localSettings.Values["DefaultLaunchModeIndex"] = 1;
                    }
                }
            }

            //checks the new settings
            if (localSettings.Values.ContainsKey("DefaultLaunchModeIndex"))
            {
                selectedView = (int)localSettings.Values["DefaultLaunchModeIndex"]; ;
            }

            if (string.IsNullOrEmpty(folderToken) || string.IsNullOrEmpty(pinnedFolderToken)) selectedView = 0;

            if (args.Arguments.Contains("forceNormalView")) selectedView = 0;
            else if (args.Arguments.Contains("forceSimpleView")) selectedView = 1;
            else if (args.Arguments.Contains("forceMiniView")) selectedView = 2;

            Debug.WriteLine("Arguments:");
            Debug.WriteLine(args.Arguments);
            if (args.Arguments.Contains("LaunchCameraExperience"))
            {
                m_window = new CameraScanner();
                m_window.Activate();
            }
            else
            {
                if (shouldLaunchWindow)
                {
                    switch (selectedView)
                    {
                        case 0: m_window = new MainWindow(); break;
                        case 1: m_window = new SimpleMode(); break;
                        case 2: m_window = new MiniMode(); break;
                    }
                    m_window.Activate();
                }
                else if (!shouldLaunchWindow)
                {
                    CoreApplication.Exit();
                }
            }
        }

        private void resetSecondaryPortalFolders()
        {
            shouldLaunchWindow = false;

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
            shouldLaunchWindow = false;

            ApplicationData.Current.LocalSettings.Values["FolderToken"] = null;
            ApplicationData.Current.LocalSettings.Values["PinnedFolderToken"] = null;
            ApplicationData.Current.LocalSettings.Values["LoadSimpleViewBoolean"] = false;
            ApplicationData.Current.LocalSettings.Values["PinBarBehavior"] = 0;
            ApplicationData.Current.LocalSettings.Values["HasPinBarBeenExpanded"] = true;
            ApplicationData.Current.LocalSettings.Values["IsPanosUnlocked"] = false;
            ApplicationData.Current.LocalSettings.Values["SelectedTheme"] = "Default";
            ApplicationData.Current.LocalSettings.Values["NormalLoadedItems"] = 1000;
            ApplicationData.Current.LocalSettings.Values["LoadedThumbnails"] = 250;
            ApplicationData.Current.LocalSettings.Values["ThumbnailResolution"] = 64;
        }

        private void resetPerformanceSettings()
        {
            shouldLaunchWindow = false;
            
            ApplicationData.Current.LocalSettings.Values["NormalLoadedItems"] = 1000;
            ApplicationData.Current.LocalSettings.Values["LoadedThumbnails"] = 250;
            ApplicationData.Current.LocalSettings.Values["ThumbnailResolution"] = 64;
        }

        private void engageUltraResourceSaverMode()
        {
            shouldLaunchWindow = false;
            
            ApplicationData.Current.LocalSettings.Values["NormalLoadedItems"] = 1;
            ApplicationData.Current.LocalSettings.Values["LoadedThumbnails"] = 1;
            ApplicationData.Current.LocalSettings.Values["ThumbnailResolution"] = 20;
        }

        private static Window m_window;
    }
}
