using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Xml.Serialization;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Security.Credentials.UI;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.ViewManagement;
using System.Numerics;
using WinUIEx;
using WinRT.Interop;
using Windows.Storage.Search;
using System.Diagnostics;
using Windows.Data.Xml.Dom;
using System.Data;
using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.Media.Core;
using Windows.Media.Playback;
using DropStackWinUI.FileViews;
using Microsoft.UI.Composition;
using System.Drawing;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.Windows.AppNotifications;
using WinUIEx.Messaging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DropStackWinUI
{
    public class FileItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        [XmlIgnore]
        private string _fileName;
        public string FileName
            { get { return _fileName; } set {
        if (value != _fileName){
            _fileName = value;
            OnPropertyChanged(nameof(FileName)); }}}
        [XmlIgnore]
        private string _fileDisplayName;
        public string FileDisplayName
            { get { return _fileDisplayName; } set {
        if (value != _fileDisplayName){
            _fileDisplayName = value;
            OnPropertyChanged(nameof(FileDisplayName)); }}}
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public string TypeTag { get; set; }
        // may assume "docs", "pics", "music", "vids", "apps", "pres"
        public string FileSize { get; set; }
        public string FileSizeSuffix { get; set; }
        public string ModifiedDate { get; set; }
        [XmlIgnore]
        private BitmapImage _fileIcon; // Backing field
        [XmlIgnore]
        public BitmapImage FileIcon 
        { get { return _fileIcon; } set {
        if (value != _fileIcon){
            _fileIcon = value;
            OnPropertyChanged(nameof(FileIcon)); }}}
        public double IconOpacity { get; set; }
        public double TextOpacity { get; set; }
        public double TextOpacityDate { get; set; }
        public double PillOpacity { get; set; }
        public bool ProgressActivity { get; set; }
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class FileThumbnail
    {
        public string Type { get; set; }
        public BitmapImage Image { get; set; }
    }

    public class FileTags
    {
        public static IList<string> DownloadFileTypes => downloadFileTypes; static IList<string> downloadFileTypes = new List<string> { ".crdownload", ".part" };
        public static IList<string> DocumentFileTypes => documentFileTypes; static IList<string> documentFileTypes = new List<string> { ".pdf", ".doc", ".docx", ".txt", ".html", ".htm", ".xls", ".xlsx", ".odt", ".fodt", ".ods", ".fods", ".rtf", ".xml" };
        public static IList<string> PictureFileTypes => pictureFileTypes; static IList<string> pictureFileTypes = new List<string> { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".svg", ".ico", ".webp", ".raw", ".psd", ".ai" };
        public static IList<string> MusicFileTypes => musicFileTypes; static IList<string> musicFileTypes = new List<string> { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma", ".m4a", ".mid", ".amr", ".aiff", ".ape" };
        public static IList<string> VideoFileTypes => videoFileTypes; static IList<string> videoFileTypes = new List<string> { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm", ".3gp", ".m4v", ".mpeg", ".mpg", ".rm", ".vob" };
        public static IList<string> ApplicationFileTypes => applicationFileTypes; static IList<string> applicationFileTypes = new List<string> { ".exe", ".dmg", ".app", ".deb", ".apk", ".msi", ".msix", ".rpm", ".jar", ".bat", ".sh", ".com", ".vb", ".gadget", ".ipa" };
        public static IList<string> PresentationFileTypes => presentationFileTypes; static IList<string> presentationFileTypes = new List<string> { ".ppt", ".pptx", ".key", ".odp" };
    }

    public class ImageViewerSettings
    {
        public string filePath { get; set; }
        public bool isVideo { get; set; } = false;
        public bool isAnnotating { get; set; } = false;
        public bool isCropping { get; set; } = false;
    }

    [XmlRoot("ArrayOfFileItem")]
    public class ArrayOfFileItem
    {
        [XmlElement("FileItem")]
        public List<FileItem> Items { get; set; }
    }

    public sealed partial class MainWindow : WinUIEx.WindowEx
    {
        [DllImport("User32.dll")]
        public static extern uint GetDpiForWindow(IntPtr hwnd);
        public string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;
            return $"Version {version.Major}.{version.Minor}.{version.Build}";
        }

        bool appLaunchComplete = false;

        string folderToken = ApplicationData.Current.LocalSettings.Values["FolderToken"] as string;
        string pinnedFolderToken = ApplicationData.Current.LocalSettings.Values["PinnedFolderToken"] as string;

        string secondaryFolderToken1 = ApplicationData.Current.LocalSettings.Values["SecFolderToken1"] as string;
        string secondaryFolderToken2 = ApplicationData.Current.LocalSettings.Values["SecFolderToken2"] as string;
        string secondaryFolderToken3 = ApplicationData.Current.LocalSettings.Values["SecFolderToken3"] as string;
        string secondaryFolderToken4 = ApplicationData.Current.LocalSettings.Values["SecFolderToken4"] as string;
        string secondaryFolderToken5 = ApplicationData.Current.LocalSettings.Values["SecFolderToken5"] as string;

        IList<object> GlobalClickedItems = null;

        private ObservableCollection<FileItem> _filteredFileMetadataList;
        private ObservableCollection<FileItem> _filteredPinnedFileMetadataList;
        public ObservableCollection<FileItem> fileMetadataListCopy;
        public ObservableCollection<FileItem> pinnedFileMetadataListCopy;

        string RegularFolderPath { get; set; }
        string PinnedFolderPath { get; set; }

        int pinBarBehaviorIndex = 0;
        bool isWindowsHelloRequiredForPins = false;

        bool showPrimPortal = true;
        bool showSecPortal1 = false;
        bool showSecPortal2 = false;
        bool showSecPortal3 = false;
        bool showSecPortal4 = false;
        bool showSecPortal5 = false;

        int loadedItems = 1000;
        int loadedThumbnails = 250;
        int thumbnailResolution = 64;

        int checkboxBehavior = 1;
        int searchThreshold = 3;

        bool showDetailsPane = false;
        FileItem previewedItem = null;

        Compositor _compositor = null;
        SpringVector3NaturalMotionAnimation _springAnimation;

        bool openCompactCommandBar = false;
        bool hasDoubleTapped = false;

        int totalFilesAmount = 1024;

        public MainWindow()
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBarRectangle);
            adjustDarkLightMode();
            loadSettings();
            this.Activated += OnWindowActivated;


            if (string.IsNullOrEmpty(folderToken) || string.IsNullOrEmpty(pinnedFolderToken))
            {
                RegularAndPinnedFileGrid.Visibility = Visibility.Collapsed;
                disableButtonVisibility();
                launchOnboarding();
            }
            else
            {
                enableButtonVisibility();
                loadFromCache("regular");
                setFolderPath("Regular");
                setFolderPath("Pin");
                loadFromCache("pinned");
            }
            var window = this;
            _compositor = window.Compositor;
            FileCommandBar.Focus(FocusState.Programmatic);
        }

        private void adjustDarkLightMode()
        {
            // Get the current UI settings
            var uiSettings = new UISettings();

            // Get the background and foreground colors
            var background = uiSettings.GetColorValue(UIColorType.Background);
            var foreground = uiSettings.GetColorValue(UIColorType.Foreground);

            // Check if the background is darker than the foreground
            bool isDarkMode = background.R + background.G + background.B < foreground.R + foreground.G + foreground.B;

            if (isDarkMode)
            {
                if (pinBarBehaviorIndex != 4) PinnedExpanderBackgroundRectangle.Visibility = Visibility.Visible;
                ContentBackgroundRectangle.Opacity = 0.15;

            }
            else if (!isDarkMode)
            {
                PinnedExpanderBackgroundRectangle.Visibility = Visibility.Collapsed;
                ContentBackgroundRectangle.Opacity = 0.5;
            }
        }

        private void OnWindowActivated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs e)
        {
            adjustDarkLightMode();
            if (appLaunchComplete) obtainFolderAndFiles("regular", regularFileListView.ItemsSource as ObservableCollection<FileItem>);
        }

        private async void loadSettings()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values.ContainsKey("LoadSimpleViewBoolean"))
            {
                if (localSettings.Values.ContainsKey("DefaultLaunchModeIndex"))
                {
                    LaunchModePickerComboBox.SelectedIndex = (int)localSettings.Values["DefaultLaunchModeIndex"]; ;
                }
                else
                {
                    if ((bool)localSettings.Values["LoadSimpleViewBoolean"]) LaunchModePickerComboBox.SelectedIndex = 1;
                    else LaunchModePickerComboBox.SelectedIndex = 0;
                }
            }
            
            var WinHelloAvailability = await UserConsentVerifier.CheckAvailabilityAsync();
            if (WinHelloAvailability != UserConsentVerifierAvailability.Available) PinsProtectedRadioButton.IsEnabled = false;
            if (localSettings.Values.ContainsKey("PinBarBehavior"))
            {
                pinBarBehaviorIndex = (int)localSettings.Values["PinBarBehavior"];
            }
            switch (pinBarBehaviorIndex)
            {
                case 0:
                    PinnedFilesExpander.IsExpanded = true;
                    PinsAlwaysOpenRadioButton.IsChecked = true;
                    break;
                case 1:
                    if (localSettings.Values.ContainsKey("HasPinBarBeenExpanded"))
                    {
                        PinnedFilesExpander.IsExpanded = (bool)localSettings.Values["HasPinBarBeenExpanded"];
                    }
                    PinsRememberStateRadioButton.IsChecked = true;
                    break;
                case 2:
                    PinnedFilesExpander.IsExpanded = false;
                    PinsAlwaysClosedRadioButton.IsChecked = true;
                    break;
                case 3:
                    isWindowsHelloRequiredForPins = true;
                    PinnedFilesExpander.IsExpanded = false;
                    PinsProtectedRadioButton.IsChecked = true;
                    setPinBarOptionVisibility(false);
                    break;
                case 4:
                    isWindowsHelloRequiredForPins = false;
                    PinnedFilesExpander.IsExpanded = false;
                    PinsHiddenRadioButton.IsChecked = true;
                    PinnedFilesExpander.Visibility = Visibility.Collapsed;
                    PinnedExpanderBackgroundRectangle.Visibility = Visibility.Collapsed;
                    break;
            }

            if (localSettings.Values.ContainsKey("showPrimaryPortal")) { showPrimPortal = (bool)localSettings.Values["showPrimaryPortal"]; }

            if (localSettings.Values.ContainsKey("showSecondaryPortal1")) { showSecPortal1 = (bool)localSettings.Values["showSecondaryPortal1"]; }
            if (localSettings.Values.ContainsKey("showSecondaryPortal2")) { showSecPortal2 = (bool)localSettings.Values["showSecondaryPortal2"]; }
            if (localSettings.Values.ContainsKey("showSecondaryPortal3")) { showSecPortal3 = (bool)localSettings.Values["showSecondaryPortal3"]; }
            if (localSettings.Values.ContainsKey("showSecondaryPortal4")) { showSecPortal4 = (bool)localSettings.Values["showSecondaryPortal4"]; }
            if (localSettings.Values.ContainsKey("showSecondaryPortal5")) { showSecPortal5 = (bool)localSettings.Values["showSecondaryPortal5"]; }

            refreshFolderNames();

            if (localSettings.Values.ContainsKey("IsPanosUnlocked"))
            {
                if ((bool)localSettings.Values["IsPanosUnlocked"]) unlockPanos(false);
            }

            if (localSettings.Values.ContainsKey("SelectedTheme"))
            {
                string selectedTheme = (string)localSettings.Values["SelectedTheme"];
                ThemePickerCombobox.SelectedItem = selectedTheme;
                setTheme(selectedTheme);
            }
            else
            {
                string selectedTheme = "Default";
                ThemePickerCombobox.SelectedItem = selectedTheme;
                setTheme(selectedTheme);
            }

            if (localSettings.Values.ContainsKey("NormalLoadedItems"))
            {
                loadedItems = (int)localSettings.Values["NormalLoadedItems"];
            }
            if (localSettings.Values.ContainsKey("LoadedThumbnails"))
            {
                loadedThumbnails = (int)localSettings.Values["LoadedThumbnails"];
            }
            if (localSettings.Values.ContainsKey("ThumbnailResolution"))
            {
                thumbnailResolution = (int)localSettings.Values["ThumbnailResolution"];
            }

            if (localSettings.Values.ContainsKey("CheckboxBehavior"))
            {
                CheckBoxBehaviorCombobox.SelectedIndex = (int)localSettings.Values["CheckboxBehavior"];
            }
            else { CheckBoxBehaviorCombobox.SelectedIndex = 1; }

            RegularFileCapNumberBox.Value = Convert.ToDouble(loadedItems);
            ThumbnailCapNumberBox.Value = Convert.ToDouble(loadedThumbnails);
            ThumbnailResolutionNumberBox.Value = Convert.ToDouble(thumbnailResolution);

            if (localSettings.Values.ContainsKey("AlwaysShowToolbarInSimpleModeBoolean"))
            {
                if ((bool)localSettings.Values["AlwaysShowToolbarInSimpleModeBoolean"] == true) PinToolbarInSimpleModeToggleSwitch.IsOn = true;
            }

            if (localSettings.Values.ContainsKey("FreeWindowingInSimpleMode"))
            {
                if ((bool)localSettings.Values["FreeWindowingInSimpleMode"] == true) SimpleModeWindowingToggle.IsOn = true;
            }
            
            if (localSettings.Values.ContainsKey("SearchThreshold"))
            {
                searchThreshold = (int)localSettings.Values["SearchThreshold"];
            }
            SearchThresholdNumberBox.Value = searchThreshold;

            if (localSettings.Values.ContainsKey("ShowDetailsPane"))
            {
                showDetailsPane = (bool)localSettings.Values["ShowDetailsPane"];
            }
            ShowDetailsPaneToggleSwitch.IsOn = showDetailsPane;
        }

        public void applySettingsToMenu()
        {
            PrimaryPortalFolderCheckBox.IsChecked = showPrimPortal;
            SecondaryPortalFolder1CheckBox.IsChecked = showSecPortal1;
            SecondaryPortalFolder2CheckBox.IsChecked = showSecPortal2;
            SecondaryPortalFolder3CheckBox.IsChecked = showSecPortal3;
            SecondaryPortalFolder4CheckBox.IsChecked = showSecPortal4;
            SecondaryPortalFolder5CheckBox.IsChecked = showSecPortal5;
            refreshFolderNames();
        }

        public async void refreshFolderNames()
        {
            if (!string.IsNullOrEmpty(folderToken))
            {
                StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);
                PrimaryPortalFolderChangeButton.Content = folder.Name;
                PrimLinkToExplorerDisplay.Text = folder.Name;
                PrimLinkToExplorerDisplay.Visibility = Visibility.Visible;
            }
            else
            {
                PrimaryPortalFolderCheckBox.Content = "Set New...";
                showPrimPortal = false;
                PrimLinkToExplorerDisplay.Visibility = Visibility.Collapsed;
            }

            if (!string.IsNullOrEmpty(secondaryFolderToken1))
            {
                StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(secondaryFolderToken1);
                SecondaryPortalFolder1ChangeButton.Content = folder.Name;
                Sec1LinkToExplorerDisplay.Text = folder.Name;
                Sec1LinkToExplorerDisplay.Visibility = Visibility.Visible;
            }
            else
            {
                SecondaryPortalFolder1ChangeButton.Content = "Set New...";
                showSecPortal1 = false;
            }

            if (!string.IsNullOrEmpty(secondaryFolderToken2))
            {
                StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(secondaryFolderToken2);
                SecondaryPortalFolder2ChangeButton.Content = folder.Name;
                Sec2LinkToExplorerDisplay.Text = folder.Name;
                Sec2LinkToExplorerDisplay.Visibility = Visibility.Visible;
            }
            else
            {
                SecondaryPortalFolder2ChangeButton.Content = "Set New...";
                showSecPortal2 = false;
                Sec2LinkToExplorerDisplay.Visibility = Visibility.Collapsed;
            }

            if (!string.IsNullOrEmpty(secondaryFolderToken3))
            {
                StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(secondaryFolderToken3);
                SecondaryPortalFolder3ChangeButton.Content = folder.Name;
                Sec3LinkToExplorerDisplay.Text = folder.Name;
                Sec3LinkToExplorerDisplay.Visibility = Visibility.Visible;
            }
            else
            {
                SecondaryPortalFolder3ChangeButton.Content = "Set New...";
                showSecPortal3 = false;
                Sec3LinkToExplorerDisplay.Visibility = Visibility.Collapsed;
            }

            if (!string.IsNullOrEmpty(secondaryFolderToken4))
            {
                StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(secondaryFolderToken4);
                SecondaryPortalFolder4ChangeButton.Content = folder.Name;
                Sec4LinkToExplorerDisplay.Text = folder.Name;
                Sec4LinkToExplorerDisplay.Visibility = Visibility.Visible;
            }
            else
            {
                SecondaryPortalFolder4ChangeButton.Content = "Set New...";
                showSecPortal4 = false;
                Sec4LinkToExplorerDisplay.Visibility = Visibility.Collapsed;
            }

            if (!string.IsNullOrEmpty(secondaryFolderToken5))
            {
                StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(secondaryFolderToken5);
                SecondaryPortalFolder5ChangeButton.Content = folder.Name;
                Sec5LinkToExplorerDisplay.Text = folder.Name;
                Sec5LinkToExplorerDisplay.Visibility = Visibility.Visible;
            }
            else
            {
                SecondaryPortalFolder5ChangeButton.Content = "Set New...";
                showSecPortal5 = false;
                Sec5LinkToExplorerDisplay.Visibility = Visibility.Collapsed;
            }

            if (!string.IsNullOrEmpty(pinnedFolderToken))
            {
                StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(pinnedFolderToken);
                PinsLinkToExplorerDisplay.Text = folder.Name;
                PinsLinkToExplorerDisplay.Visibility = Visibility.Visible;
            }
            else PinsLinkToExplorerDisplay.Visibility = Visibility.Collapsed;
        }

        private async void setFolderPath(string folderToSet)
        {
            if (folderToSet == "Regular")
            {
                StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);
                RegularFolderPath = folder.Path;
            }
            if (folderToSet == "Pin")
            {
                StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(pinnedFolderToken);
                PinnedFolderPath = folder.Path;
            }
        }

        private void enableButtonVisibility()
        {
            FileCommandBar.Visibility = Visibility.Visible;
        }

        private void disableButtonVisibility()
        {
            FileCommandBar.Visibility = Visibility.Collapsed;
        }

        public void noAccessHandler()
        {
            noFolderpathTechingTip.IsOpen = true;
        }

        public async void askForAccess(string purpose)
        {
            // Close the teaching tip
            if (purpose == "regular") noFolderpathTechingTip.IsOpen = false;
            else if (purpose == "pinned")
            {
                noPinnedFolderpathTechingTip.IsOpen = false;
                NoPinnedFolderStackpanel.Visibility = Visibility.Collapsed;
            }

            var folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add("*");
            if (purpose == "regular") folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
            else if (purpose == "pinned") folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            folderPicker.ViewMode = PickerViewMode.List;

            // Get the window handle of the app window
            var windowHandle = WindowNative.GetWindowHandle(this);
            // Associate the picker with the app window
            InitializeWithWindow.Initialize(folderPicker, windowHandle);

            // Pick a folder
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Request access to the selected folder
                try
                {
                    string currentFolderToken = StorageApplicationPermissions.FutureAccessList.Add(folder);
                    nextOOBEpage();

                    switch (purpose)
                    {
                        case "regular":
                            ApplicationData.Current.LocalSettings.Values["FolderToken"] = currentFolderToken;
                            folderToken = currentFolderToken;
                            if (OOBEgrid.Visibility == Visibility.Collapsed) enableButtonVisibility();
                            if (OOBEgrid.Visibility == Visibility.Visible) obtainFolderAndFiles("regular", null);
                            RegularFolderPath = folder.Path;
                            PrimaryPortalFolderChangeButton.Content = folder.Name;
                            break;
                        case "pinned":
                            ApplicationData.Current.LocalSettings.Values["PinnedFolderToken"] = currentFolderToken;
                            pinnedFolderToken = currentFolderToken;
                            if (OOBEgrid.Visibility == Visibility.Collapsed) enableButtonVisibility();
                            obtainFolderAndFiles("pinned", null);
                            PinnedFolderPath = folder.Path;
                            break;
                        case "Sec1":
                            ApplicationData.Current.LocalSettings.Values["SecFolderToken1"] = currentFolderToken;
                            secondaryFolderToken1 = currentFolderToken;
                            SecondaryPortalFolder1ChangeButton.Content = folder.Name;
                            break;
                        case "Sec2":
                            ApplicationData.Current.LocalSettings.Values["SecFolderToken2"] = currentFolderToken;
                            secondaryFolderToken2 = currentFolderToken;
                            SecondaryPortalFolder2ChangeButton.Content = folder.Name;
                            break;
                        case "Sec3":
                            ApplicationData.Current.LocalSettings.Values["SecFolderToken3"] = currentFolderToken;
                            secondaryFolderToken3 = currentFolderToken;
                            SecondaryPortalFolder3ChangeButton.Content = folder.Name;
                            break;
                        case "Sec4":
                            ApplicationData.Current.LocalSettings.Values["SecFolderToken4"] = currentFolderToken;
                            secondaryFolderToken4 = currentFolderToken;
                            SecondaryPortalFolder4ChangeButton.Content = folder.Name;
                            break;
                        case "Sec5":
                            ApplicationData.Current.LocalSettings.Values["SecFolderToken5"] = currentFolderToken;
                            secondaryFolderToken5 = currentFolderToken;
                            SecondaryPortalFolder5ChangeButton.Content = folder.Name;
                            break;
                    }
                    refreshFolderNames();
                }
                catch { }
            }
            else
            {
                //canceled operation, do nothing
            }
        }

        public async void obtainFolderAndFiles(string source, ObservableCollection<FileItem> cachedItems)
        {
            ObservableCollection<FileItem> fileMetadataList = new ObservableCollection<FileItem>();

            if (source == "regular")
            {
                if (cachedItems == null) regularFileListView.ItemsSource = fileMetadataList;
                else
                {
                    _filteredFileMetadataList = cachedItems;
                    fileMetadataListCopy = cachedItems;
                }
            }
            else if (source == "pinned")
            {
                if (cachedItems == null) pinnedFileListView.ItemsSource = fileMetadataList;
                else
                {
                    _filteredPinnedFileMetadataList = cachedItems;
                }
            }

            // Get the folder from the access token
            StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);
            if (source == "pinned") folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(pinnedFolderToken);

            // Access the selected folder
            IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();

            // Create a list of StorageFile to add more files to
            List<StorageFile> fileList = files.ToList();

            if (!showPrimPortal && source == "regular") fileList.Clear();

            if (source == "regular") 
            {
                if (showSecPortal1 || showSecPortal2 || showSecPortal3 || showSecPortal4 || showSecPortal5)
                {                    
                    if (showSecPortal1)
                    {
                        StorageFolder thisFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(secondaryFolderToken1);
                        var folderFiles = await thisFolder.GetFilesAsync();
                        fileList.AddRange(folderFiles);
                    }
                    if (showSecPortal2)
                    {
                        StorageFolder thisFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(secondaryFolderToken2);
                        var folderFiles = await thisFolder.GetFilesAsync();
                        fileList.AddRange(folderFiles);
                    }
                    if (showSecPortal3)
                    {
                        StorageFolder thisFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(secondaryFolderToken3);
                        var folderFiles = await thisFolder.GetFilesAsync();
                        fileList.AddRange(folderFiles);
                    }
                    if (showSecPortal4)
                    {
                        StorageFolder thisFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(secondaryFolderToken4);
                        var folderFiles = await thisFolder.GetFilesAsync();
                        fileList.AddRange(folderFiles);
                    }
                    if (showSecPortal5)
                    {
                        StorageFolder thisFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(secondaryFolderToken5);
                        var folderFiles = await thisFolder.GetFilesAsync();
                        fileList.AddRange(folderFiles);
                    }

                    files = fileList.Cast<StorageFile>().ToList();
                }
            }

            // Sort the files by modification date in descending order
            files = files.OrderByDescending(f => f.DateCreated).ToList();

            //check for user's way to note the date and time
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            string shortDatePattern = currentCulture.DateTimeFormat.ShortDatePattern;
            string shortTimePattern = currentCulture.DateTimeFormat.ShortTimePattern;

            if (folder != null)
            {
                double totalFiles = Convert.ToDouble(files.Count);
                totalFilesAmount = Convert.ToInt32(totalFiles);
                int currentFile = 1;
                int addIndex = 0;
                bool shouldContinue = true;

                foreach (StorageFile file in files.Take(loadedItems))
                {
                    if (shouldContinue)
                    {
                        BasicProperties basicProperties = await file.GetBasicPropertiesAsync();

                        BitmapImage bitmapThumbnail = new BitmapImage();

                        if (currentFile < (loadedThumbnails + 1))
                        {
                            StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, Convert.ToUInt32(thumbnailResolution));
                            bitmapThumbnail.SetSource(thumbnail);
                        }

                        currentFile++;

                        int filesizecalc = Convert.ToInt32(basicProperties.Size); //size in byte
                        string generativefilesizesuffix = "B"; //default file suffix

                        if (filesizecalc >= 1000 && filesizecalc < 1000000)
                        {
                            filesizecalc = Convert.ToInt32(basicProperties.Size) / 1000; //convert to kb
                            generativefilesizesuffix = "KB";
                        }

                        else if (filesizecalc >= 1000000 && filesizecalc < 1000000000)
                        {
                            filesizecalc = Convert.ToInt32(basicProperties.Size) / 1000000; //convert to mb
                            generativefilesizesuffix = "MB";
                        }

                        else if (filesizecalc >= 1000000000)
                        {
                            filesizecalc = Convert.ToInt32(basicProperties.Size) / 1000000000; //convert to gb
                            generativefilesizesuffix = "GB";
                        }

                        string typeTag = "";
                        string typeDisplayName = file.FileType;

                        if (FileTags.DocumentFileTypes.Contains(file.FileType.ToLower()))
                        {
                            typeTag = "docs";
                            typeDisplayName = "Document (" + file.FileType + ")";
                        }
                        else if (FileTags.PictureFileTypes.Contains(file.FileType.ToLower()))
                        {
                            typeTag = "pics";
                            typeDisplayName = "Picture (" + file.FileType + ")";
                        }
                        else if (FileTags.MusicFileTypes.Contains(file.FileType.ToLower()))
                        {
                            typeTag = "music";
                            typeDisplayName = "Music (" + file.FileType + ")";
                        }
                        else if (FileTags.VideoFileTypes.Contains(file.FileType.ToLower()))
                        {
                            typeTag = "vids";
                            typeDisplayName = "Video (" + file.FileType + ")";
                        }
                        else if (FileTags.ApplicationFileTypes.Contains(file.FileType.ToLower()))
                        {
                            typeTag = "apps";
                            typeDisplayName = "Application (" + file.FileType + ")";
                        }
                        else if (FileTags.PresentationFileTypes.Contains(file.FileType.ToLower()))
                        {
                            typeTag = "pres";
                            typeDisplayName = "Presentation (" + file.FileType + ")";
                        }

                        if (cachedItems != null)
                        {
                            foreach (FileItem currentFileItem in cachedItems.Take(5))
                            {
                                if (currentFileItem.FileName == file.Name)
                                {
                                    //assume that from now on, files are cached
                                    shouldContinue = false;
                                    break;
                                }
                            }
                        }

                        FileItem fileItem = null;

                        if (shouldContinue)
                        {
                            if (FileTags.DownloadFileTypes.Contains(file.FileType))
                            {
                                fileItem = new FileItem()
                                {
                                    FileName = file.Name,
                                    FileDisplayName = file.DisplayName,
                                    FilePath = file.Path,
                                    FileType = "This file is still being downloaded",
                                    TypeTag = typeTag,
                                    FileSize = "",
                                    FileSizeSuffix = "",
                                    ModifiedDate = "",
                                    FileIcon = bitmapThumbnail,
                                    IconOpacity = 0.25,
                                    PillOpacity = 0,
                                    TextOpacity = 0.5,
                                    ProgressActivity = true,
                                    TextOpacityDate = 0
                                };

                                if (addIndex == 0)
                                {
                                    fileMetadataList.Clear();
                                }
                                fileMetadataList.Add(fileItem);
                            }
                            else
                            {
                                fileItem = new FileItem()
                                {
                                    FileName = file.Name,
                                    FileDisplayName = file.DisplayName,
                                    FilePath = file.Path,
                                    FileType = typeDisplayName,
                                    TypeTag = typeTag,
                                    FileSize = filesizecalc.ToString(),
                                    FileSizeSuffix = " " + generativefilesizesuffix,
                                    ModifiedDate = basicProperties.DateModified.ToString("g"),
                                    FileIcon = bitmapThumbnail,
                                    IconOpacity = 1,
                                    PillOpacity = 0.25,
                                    TextOpacity = 1,
                                    ProgressActivity = false,
                                    TextOpacityDate = 0.25
                                };

                                if (addIndex == 0)
                                {
                                    fileMetadataList.Clear();
                                }
                                fileMetadataList.Add(fileItem);
                            }
                        }
                        else
                        {
                            if (source == "regular")
                            {
                                fileMetadataList.Reverse();
                                ObservableCollection<FileItem> collection = regularFileListView.ItemsSource as ObservableCollection<FileItem>;
                                regularFileListView.ItemsSource = collection;
                                foreach (FileItem item in fileMetadataList) collection.Insert(0, item);
                            }
                            else if (source == "pinned")
                            {
                                pinnedFileListView.Items.Prepend(fileMetadataList);
                            }
                            break;
                        }
                        addIndex++;
                        if (source == "regular")
                        {
                            _filteredFileMetadataList = fileMetadataList;
                            fileMetadataListCopy = fileMetadataList;
                        }
                        else if (source=="pinned") _filteredPinnedFileMetadataList = fileMetadataList;
                    }
                }

                //check if all the files still exist, else remove them, then save to cache
                if (cachedItems != null)
                {
                    for (int i = 0; i < fileMetadataList.Count; i++)
                    {
                        FileItem item = fileMetadataList.ElementAt(i);
                        if (!System.IO.File.Exists(item.FilePath)) { fileMetadataList.Remove(item); }
                    }
                }

                ObservableCollection<FileItem> cachingCollection = new();

                if (cachedItems != null)
                {
                    if (source == "regular") foreach (FileItem item in regularFileListView.Items) cachingCollection.Add(item);
                    else if (source == "pinned") foreach (FileItem item in pinnedFileListView.Items) cachingCollection.Add(item);
                    saveToCache(source, cachingCollection);
                }
                else saveToCache(source, fileMetadataList);

                if (source == "regular")
                {
                    
                    _filteredFileMetadataList = cachingCollection;
                    fileMetadataListCopy = cachingCollection;

                }
                else if (source == "pinned")
                {
                    _filteredPinnedFileMetadataList = cachingCollection;
                    pinnedFileMetadataListCopy = cachingCollection;
                }
            }
            else
            {
                // Ah bleh
            }
            appLaunchComplete = true;
        }

        public async void loadInThumbnails(string target)
        {
            //load thumbnails
            List<FileThumbnail> thumbnails = new();
            int currentFile = 1;

            if (target == "regular")
            {
                for (int i = 0; i < loadedThumbnails; i++)
                {
                    try
                    {
                        FileItem item = regularFileListView.Items.ElementAt(i) as FileItem;
                        if (item.FileIcon == null)
                        {
                            if (item.TypeTag == "pics" || item.TypeTag == "vids" || item.TypeTag == "apps")
                            {
                                BitmapImage bitmapThumbnail = new BitmapImage();
                                StorageFile file = await StorageFile.GetFileFromPathAsync(item.FilePath);
                                StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, Convert.ToUInt32(thumbnailResolution));
                                bitmapThumbnail.SetSource(thumbnail);
                                item.FileIcon = bitmapThumbnail;
                            }
                            else
                            {
                                BitmapImage bitmapThumbnail = new BitmapImage();
                                // look for thumbnail entry in list
                                FileThumbnail cachedThumbnailEntry = thumbnails.Where(f => f.Type == item.FileType).FirstOrDefault();
                                // if entry exists, take stored image
                                if (cachedThumbnailEntry != null) item.FileIcon = cachedThumbnailEntry.Image;
                                else
                                {
                                    try
                                    {
                                        StorageFile file = await StorageFile.GetFileFromPathAsync(item.FilePath);
                                        StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, Convert.ToUInt32(thumbnailResolution));
                                        bitmapThumbnail.SetSource(thumbnail);
                                        item.FileIcon = bitmapThumbnail;
                                        // add new thumbnail entry to list
                                        thumbnails.Add(new FileThumbnail { Type = item.FileType, Image = bitmapThumbnail });
                                    }
                                    catch { }
                                }

                            }
                            currentFile++;
                        }
                    }
                    catch { break; /*there are no more files, this fails when the index exceeds the file count*/ }
                }
            }
            else if (target == "pinned")
            {
                foreach (FileItem item in pinnedFileListView.Items.Take(loadedThumbnails))
                {
                    if (item.FileIcon == null)
                    {
                        if (item.TypeTag == "pics" || item.TypeTag == "vids" || item.TypeTag == "apps")
                        {
                            BitmapImage bitmapThumbnail = new BitmapImage();
                            StorageFile file = await StorageFile.GetFileFromPathAsync(item.FilePath);
                            StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, Convert.ToUInt32(thumbnailResolution));
                            bitmapThumbnail.SetSource(thumbnail);
                            item.FileIcon = bitmapThumbnail;
                        }
                        else
                        {
                            BitmapImage bitmapThumbnail = new BitmapImage();
                            // look for thumbnail entry in list
                            FileThumbnail cachedThumbnailEntry = thumbnails.Where(f => f.Type == item.FileType).FirstOrDefault();
                            // if entry exists, take stored image
                            if (cachedThumbnailEntry != null) item.FileIcon = cachedThumbnailEntry.Image;
                            else
                            {
                                StorageFile file = await StorageFile.GetFileFromPathAsync(item.FilePath);
                                StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, Convert.ToUInt32(thumbnailResolution));
                                bitmapThumbnail.SetSource(thumbnail);
                                item.FileIcon = bitmapThumbnail;
                                // add new thumbnail entry to list
                                thumbnails.Add(new FileThumbnail { Type = item.FileType, Image = bitmapThumbnail });
                            }

                        }
                        currentFile++;
                    }
                }
            }
        }

        private void fileListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GlobalClickedItems = regularFileListView.SelectedItems;
            try { updatePreviewArea(GlobalClickedItems[0] as FileItem, regularFileListView.SelectedItems.Count < 1); }
            catch{ updatePreviewArea(null, false); }
        }

        private async void fileListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if ((object)sender == pinnedFileListView)
            {
                // get the selected file item
                FileItem selectedFile = (FileItem)((FrameworkElement)e.OriginalSource).DataContext;

                await Task.Run(async () =>
                {
                    // get the file
                    StorageFile file = await StorageFile.GetFileFromPathAsync(selectedFile.FilePath);

                    // create a new data package
                    var dataPackage = new DataPackage();

                    // add the file to the data package
                    dataPackage.SetStorageItems(new List<IStorageItem> { file });

                    // copy the data package to the clipboard
                    Clipboard.SetContent(dataPackage);
                    Clipboard.Flush();
                });

                //show teaching tip
                fileInClipboardReminder.IsOpen = true;
                for (int i = 0; i < 100; i++)
                {
                    await Task.Delay(10);
                    reminderTimer.Value = i;
                }
                fileInClipboardReminder.IsOpen = false;
                await Task.Delay(100);
                reminderTimer.Value = 0;
            }
            else if ((object)sender == regularFileListView)
            {
                FileItem rightTappedItem = (FileItem)((FrameworkElement)e.OriginalSource).DataContext;

                if (!regularFileListView.SelectedItems.Contains(rightTappedItem))
                {
                    regularFileListView.SelectedItems.Clear();
                    //I have to first set it to an instance or it crashes... this sucks!
                    GlobalClickedItems = regularFileListView.SelectedItems;
                    GlobalClickedItems.Clear();
                    GlobalClickedItems.Add(rightTappedItem);
                }

                if (GlobalClickedItems.Count == 1) previewedItem = GlobalClickedItems.First() as FileItem;

                updateShownContextMenuItems();
            }
        }

        private async void fileListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            try
            {
                // Create a list of storage items with the file
                var storageItems = new List<StorageFile>();

                foreach (FileItem selectedFile in e.Items)
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(selectedFile.FilePath);

                    storageItems.Add(file);
                }

                // Set the data package on the event args using SetData
                e.Data.SetData(StandardDataFormats.StorageItems, storageItems);
            }
            catch
            {
                //show teaching tip
                failedToDragTeachingTip.IsOpen = true;
                for (int i = 0; i < 100; i++)
                {
                    await Task.Delay(10);
                    failureReminderTimer.Value = i;
                }
                failedToDragTeachingTip.IsOpen = false;
                await Task.Delay(100);
                failureReminderTimer.Value = 0;
            }
        }

        private void regularFileListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            fileMetadataListCopy = sender.ItemsSource as ObservableCollection<FileItem>;
            regularFileListView.ItemsSource = fileMetadataListCopy;
            foreach (FileItem file in fileMetadataListCopy) { if (!System.IO.File.Exists(file.FilePath)) fileMetadataListCopy.Remove(file); }
            _filteredFileMetadataList = fileMetadataListCopy;
            saveToCache("regular", fileMetadataListCopy);
        }

        private void pinnedFileListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            pinnedFileListView.ItemsSource = pinnedFileMetadataListCopy;
            foreach (FileItem file in pinnedFileMetadataListCopy) { if (!System.IO.File.Exists(file.FilePath)) pinnedFileMetadataListCopy.Remove(file); }
            _filteredPinnedFileMetadataList = pinnedFileMetadataListCopy;
            saveToCache("pinned", pinnedFileMetadataListCopy);
        }

        private void noFolderpathTechingTip_ActionButtonClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            askForAccess("regular");
        }

        private void QuickSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            applySettingsToMenu();
            AboutDropStackGrid.Visibility = Visibility.Visible;
            AboutDropStackGrid.Opacity = 1;
            AboutDropStackContentGrid.Translation = new Vector3(0, 0, 0);
            showOrHideDetailsPane(false);
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            //delete file
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await localFolder.GetFileAsync("cachedfiles.xml");
            await file.DeleteAsync();
            file = await localFolder.GetFileAsync("cachedpins.xml");
            await file.DeleteAsync();

            obtainFolderAndFiles("regular", null);
            obtainFolderAndFiles("pinned", null);
        }

        private void Query_ContentsChanged(IStorageQueryResultBase sender, object args)
        {
            obtainFolderAndFiles("regular", null);
        }

        private async void disconnectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values["FolderToken"] = null;
            ApplicationData.Current.LocalSettings.Values["PinnedFolderToken"] = null;

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

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await localFolder.GetFileAsync("cachedfiles.xml");
            StorageFile pinsfile = await localFolder.GetFileAsync("cachedpins.xml");
            await file.DeleteAsync();
            await pinsfile.DeleteAsync();

            await Task.Delay(1000);

            var newMainWindow = new MainWindow();
            newMainWindow.Show();
            this.Close();
        }

        private async void RevealInExplorerButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);
            MenuFlyoutItem menuFlyoutItem = sender as MenuFlyoutItem;
            string tag = menuFlyoutItem.Tag.ToString();
            switch (tag)
            {
                case "sec1":
                    folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(secondaryFolderToken1);
                    break;
                case "sec2":
                    folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(secondaryFolderToken2);
                    break;
                case "sec3":
                    folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(secondaryFolderToken3);
                    break;
                case "sec4":
                    folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(secondaryFolderToken4);
                    break;
                case "sec5":
                    folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(secondaryFolderToken5);
                    break;
                case "pins":
                    folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(pinnedFolderToken);
                    break;
            }

            try
            {
                await Launcher.LaunchFolderAsync(folder);
            }
            catch { cannotOpenPinnedFolderBecauseThereIsNoneTeachingTip.IsOpen = true; }
        }

        private void noPinnedFolderpathTechingTip_ActionButtonClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            askForAccess("pinned");
        }

        private void cannotOpenPinnedFolderBecauseThereIsNoneTeachingTip_ActionButtonClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            askForAccess("pinned");
        }

        private void cannotOpenRegularFolderBecauseThereIsNoneTeachingTip_ActionButtonClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            askForAccess("regular");
        }

        private void PinnedPivotSection_DragOver(object sender, DragEventArgs e)
        {
            
            if (isWindowsHelloRequiredForPins && !PinnedFilesExpander.IsExpanded)
            {
                e.AcceptedOperation = DataPackageOperation.None;
                e.DragUIOverride.Caption = "Cannot save to pins since they are locked through Windows Hello";
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
                e.DragUIOverride.Caption = "Drop to add to pinned files";
            }
        }

        private async void PinnedPivotSection_Drop(object sender, DragEventArgs e)
        {
            if (isWindowsHelloRequiredForPins && !PinnedFilesExpander.IsExpanded) { }
            else if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    var storageFile = items[0] as StorageFile; StorageFolder storageFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(pinnedFolderToken);
                    StorageFile copiedFile = await storageFile.CopyAsync(storageFolder, storageFile.Name, NameCollisionOption.GenerateUniqueName);
                    obtainFolderAndFiles("pinned", pinnedFileMetadataListCopy);
                }
            }
        }

        public void launchOnboarding()
        {
            OOBEgrid.Visibility = Visibility.Visible;
            OOBEgrid.Opacity = 1;
            OOBEgrid.Translation = new Vector3(0, 0, 0);
        }

        private void OOBEgoBackButton_Click(object sender, RoutedEventArgs e)
        {
            if (OOBEpivot.SelectedIndex == 1 & OOBEsetupPivot.SelectedIndex != 0) OOBEsetupPivot.SelectedIndex = OOBEsetupPivot.SelectedIndex - 1;
            else OOBEpivot.SelectedIndex = OOBEpivot.SelectedIndex - 1;
        }

        private async void OOBEgoNextButton_Click(object sender, RoutedEventArgs e)
        {
            if (allowOOBEExit())
            {
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                if (OOBEsimpleViewOfferCheckBox.IsChecked == false) 
                {
                    OOBEgrid.Opacity = 0;
                    OOBEgrid.Translation = new Vector3(0, 100, 0);
                    RegularAndPinnedFileGrid.Visibility = Visibility.Visible;
                    if (string.IsNullOrEmpty(folderToken) && string.IsNullOrEmpty(pinnedFolderToken)) noAccessHandler();
                    else enableButtonVisibility();
                    await Task.Delay(1000);
                    OOBEgrid.Visibility = Visibility.Collapsed;
                    localSettings.Values["DefaultLaunchModeIndex"] = 0;
                }
                else if (OOBEsimpleViewOfferCheckBox.IsChecked == true)
                {
                    localSettings.Values["DefaultLaunchModeIndex"] = 1;

                    var simpleWindow = new SimpleMode();
                    simpleWindow.Activate();

                    this.Close();
                }
            }
            else
            {
                nextOOBEpage();
            }

        }

        private void nextOOBEpage()
        {
            if (OOBEpivot.SelectedIndex == 1 & OOBEsetupPivot.SelectedIndex < OOBEsetupPivot.Items.Count - 1) OOBEsetupPivot.SelectedIndex = OOBEsetupPivot.SelectedIndex + 1;
            else
            {
                if (OOBEpivot.SelectedIndex < OOBEpivot.Items.Count - 1) { OOBEpivot.SelectedIndex = OOBEpivot.SelectedIndex + 1; }
                else
                {
                    OOBEpivot.SelectedIndex = 1;
                    if (String.IsNullOrEmpty(pinnedFolderToken)) OOBEsetupPivot.SelectedIndex = 1;
                    if (String.IsNullOrEmpty(folderToken)) OOBEsetupPivot.SelectedIndex = 0;
                }
            }
        }

        private void OOBEpivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OOBEpivot.SelectedIndex == 0) OOBEgoBackButton.IsEnabled = false;
            else OOBEgoBackButton.IsEnabled = true;
            
            OOBEgoNextButton.IsEnabled = checkIfNextButtonShouldBeEnabled();
        }

        private void OOBEsetupPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OOBEgoNextButton.IsEnabled = checkIfNextButtonShouldBeEnabled();
        }

        private bool allowOOBEExit() 
        {
            bool allowExit = false;
            if (OOBEpivot.SelectedIndex == OOBEpivot.Items.Count - 1 && !String.IsNullOrEmpty(folderToken) && !String.IsNullOrEmpty(pinnedFolderToken)) allowExit = true;
            return allowExit;
        }

        public bool checkIfNextButtonShouldBeEnabled()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (OOBEpivot.SelectedIndex == 1)
            {
                switch (OOBEsetupPivot.SelectedIndex)
                {
                    case 0:
                        if (localSettings.Values.ContainsKey("FolderToken"))
                        {
                            if (!string.IsNullOrEmpty("FolderToken")) return true;
                            else return false;
                        }
                        else return false;
                    case 1:
                        if (localSettings.Values.ContainsKey("PinnedFolderToken"))
                        {
                            if (!string.IsNullOrEmpty("PinnedFolderToken")) return true;
                            else return false;
                        }
                        else return false;
                    default: return false;
                }
            }
            else return true;
        }

        private void OOBEportalFileAccessRequestButton_Click(object sender, RoutedEventArgs e)
        {
            askForAccess("regular");
        }

        private void OOBEpinnedFileAccessRequestButton_Click(object sender, RoutedEventArgs e)
        {
            askForAccess("pinned");
        }

        private void LaunchSimpleModeButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new SimpleMode();
            mainWindow.Activate();

            this.Close();
        }

        private void CopyRecentFileButton_Click(object sender, RoutedEventArgs e)
        {
            copyMostRecentFile();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (SearchGrid.Opacity == 0)
            {
                SearchGrid.Visibility = Visibility.Visible;
                SearchGrid.Opacity = 1;
                SearchGrid.Translation = new Vector3(0, 0, 0);
                RegularAndPinnedFileGrid.Translation = new Vector3(0, 60, 0);
                regularFileListView.Margin = new Thickness(0, 2, 0, 60);
                SearchTextBox.Focus(FocusState.Programmatic);
            }
            else if (SearchGrid.Opacity == 1)
            {
                SearchGrid.Opacity = 0;
                SearchGrid.Translation = new Vector3(0, -60, 0);
                SearchGrid.Visibility = Visibility.Collapsed;
                RegularAndPinnedFileGrid.Translation = new Vector3(0, 0, 0);
                regularFileListView.Margin = new Thickness(0, 2, 0, 0);
            }
            SearchTextBox.Text = "";
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTextBox.Text.Length == 0)
            {
                regularFileListView.ItemsSource = fileMetadataListCopy;
                pinnedFileListView.ItemsSource = pinnedFileMetadataListCopy;
            }
            else
            {
                // store search and check later if it still matches up
                string searchTerm = SearchTextBox.Text;
                await Task.Delay(200);
                if (searchTerm == SearchTextBox.Text) filterListView(SearchTextBox.Text);
            }
        }

        private async void filterListView(string filter)
        {
            IEnumerable<FileItem> filteredItems = null;
            IEnumerable<FileItem> filteredPinnedItems = null;

            await Task.Run(() =>
            {
                filteredItems = fileMetadataListCopy
                .Where(item => FuzzyMatch(item.FileName.ToLower(), filter.ToLower())
                            || FuzzyMatch(item.FilePath.ToLower(), filter.ToLower())
                            || FuzzyMatch(item.FileType.ToLower(), filter.ToLower()));

                filteredPinnedItems = pinnedFileMetadataListCopy
                .Where(item => FuzzyMatch(item.FileName.ToLower(), filter.ToLower())
                            || FuzzyMatch(item.FilePath.ToLower(), filter.ToLower())
                            || FuzzyMatch(item.FileType.ToLower(), filter.ToLower()));
            });

            regularFileListView.ItemsSource = new ObservableCollection<FileItem>(filteredItems);
            pinnedFileListView.ItemsSource = new ObservableCollection<FileItem>(filteredPinnedItems);
        }

        public bool FuzzyMatch(string item, string filter)
        {
            int distance = LevenshteinDistance(item, filter);
            // Adjust threshold
            int threshold = searchThreshold;
            if (filter.Length <= threshold) threshold = filter.Length - 1;
            return distance <= threshold;
        }


        public static int LevenshteinDistance(string s, string t)
        {
            // What is the Levenshtein distance?
            // It is basically the amount of characters that need to be changed in a string to create another string.
            // Here it is used to figure out how far two strings are apart, so strings that are one typo away from correct will also count,
            // which is important for search.

            if (s == t) return 0;

            try { s = s.Substring(0, t.Length); }
            catch {}

            int[,] distance = new int[s.Length + 1, t.Length + 1];
            for (int i = 0; i <= s.Length; i++) distance[i, 0] = i;
            for (int j = 0; j <= t.Length; j++) distance[0, j] = j;

            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                    distance[i, j] = Math.Min(
                        Math.Min(
                            distance[i - 1, j] + 1,
                            distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            return distance[s.Length, t.Length];
        }

        private async void regularFileListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            hasDoubleTapped = true;
            openCompactCommandBar = false;
            
            DependencyObject tappedElement = e.OriginalSource as DependencyObject;

            while (tappedElement != null && !(tappedElement is ListViewItem))
            {
                tappedElement = VisualTreeHelper.GetParent(tappedElement);
            }

            if (tappedElement is ListViewItem item)
            {
                FileItem selectedFile = item.Content as FileItem;

                try
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(selectedFile.FilePath);

                    // launch the file
                    var success = await Launcher.LaunchFileAsync(file);
                }

                catch
                {
                    // handle the exception
                }
                finally
                {
                    // clear the selection after a short delay
                    await Task.Delay(250);
                    regularFileListView.SelectedItem = null;
                    pinnedFileListView.SelectedItem = null;
                }
            }
        }

        private async void DeletePinnedItemConfirmationButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            FileItem item = button.DataContext as FileItem;
            StorageFile file = await StorageFile.GetFileFromPathAsync(item.FilePath);
            await file.DeleteAsync();

            pinnedFileMetadataListCopy.Remove(item);
            pinnedFileListView.ItemsSource = pinnedFileMetadataListCopy;
            _filteredPinnedFileMetadataList = pinnedFileMetadataListCopy;
            saveToCache("pinned", pinnedFileMetadataListCopy);
        }

        private async void CopyLastSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            GlobalClickedItems = regularFileListView.SelectedItems;

            if (GlobalClickedItems == null) copyMostRecentFile();

            else
            {
                IList<object> itemsToCopy = GlobalClickedItems;

                await Task.Run(async () =>
                {
                    List<IStorageItem> fileList = new List<IStorageItem>();

                    // create a capture variable that is a value type
                    FileItem[] filesToCopy = new FileItem[itemsToCopy.Count];

                    // copy the elements of itemsToCopy to the capture variable
                    for (int i = 0; i < itemsToCopy.Count; i++)
                    {
                        filesToCopy[i] = (FileItem)itemsToCopy[i];
                    }

                    foreach (FileItem selectedFile in filesToCopy)
                    {
                        string filePath = selectedFile.FilePath;

                        // get the file
                        StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);

                        fileList.Add(file);
                    }

                    // create a new data package
                    var dataPackage = new DataPackage();

                    // add the file to the data package
                    dataPackage.SetStorageItems(fileList);

                    // copy the data package to the clipboard
                    Clipboard.SetContent(dataPackage);
                    Clipboard.Flush();
                });

                //show teaching tip
                fileInClipboardReminder.IsOpen = true;
                for (int i = 0; i < 100; i++)
                {
                    await Task.Delay(10);
                    reminderTimer.Value = i;
                }
                fileInClipboardReminder.IsOpen = false;
                await Task.Delay(100);
                reminderTimer.Value = 0;
            }
        }

        private async void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (FileItem selectedFile in regularFileListView.SelectedItems)
            {
                try
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(selectedFile.FilePath);
                    var success = await Launcher.LaunchFileAsync(file);
                }
                catch{}
            }
            await Task.Delay(250);
            regularFileListView.SelectedItem = null;
            pinnedFileListView.SelectedItem = null;
        }

        private async void copyMostRecentFile()
        {
            await Task.Run(async () =>
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(fileMetadataListCopy[0].FilePath);
                StorageFile recentFile = file;

                // create a new data package
                var dataPackage = new DataPackage();

                // add the file to the data package
                dataPackage.SetStorageItems(new List<IStorageItem> { recentFile });

                // copy the data package to the clipboard
                Clipboard.SetContent(dataPackage);
                Clipboard.Flush();
            });

            //show teaching tip
            fileInClipboardReminder.IsOpen = true;
            for (int i = 0; i < 100; i++)
            {
                await Task.Delay(10);
                reminderTimer.Value = i;
            }
            fileInClipboardReminder.IsOpen = false;
            await Task.Delay(100);
            reminderTimer.Value = 0;
        }

        private void CopyRegularFolderPathButton_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(RegularFolderPath);
            Clipboard.SetContent(dataPackage);
            Clipboard.Flush();
        }

        private void CopyPinnedFolderPathButton_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(PinnedFolderPath);
            Clipboard.SetContent(dataPackage);
            Clipboard.Flush();
        }

        private void Flyout_Opened(object sender, object e)
        {
            RegularFolderPathDisplay.Text = RegularFolderPath;
            PinnedFolderPathDisplay.Text = PinnedFolderPath;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            RadioButton selectedRadioButton = sender as RadioButton;
            int securitySeverityIndex = 0;


            switch (selectedRadioButton.Tag.ToString())
            {
                case "opened":
                    securitySeverityIndex = 0;
                    isWindowsHelloRequiredForPins = false;
                    PinnedFilesExpander.IsExpanded = true;
                    PinnedFilesExpander.Visibility = Visibility.Visible;
                    PinnedExpanderBackgroundRectangle.Visibility = Visibility.Visible;
                    break;
                case "remember":
                    securitySeverityIndex = 1;
                    isWindowsHelloRequiredForPins = false;
                    PinnedFilesExpander.Visibility = Visibility.Visible;
                    PinnedExpanderBackgroundRectangle.Visibility = Visibility.Visible;
                    localSettings.Values["HasPinBarBeenExpanded"] = PinnedFilesExpander.IsExpanded;
                    break;
                case "closed":
                    securitySeverityIndex = 2;
                    isWindowsHelloRequiredForPins = false;
                    PinnedFilesExpander.IsExpanded = false;
                    PinnedFilesExpander.Visibility = Visibility.Visible;
                    PinnedExpanderBackgroundRectangle.Visibility = Visibility.Visible;
                    break;
                case "protected":
                    securitySeverityIndex = 3;
                    isWindowsHelloRequiredForPins = true;
                    PinnedFilesExpander.IsExpanded = false;
                    PinnedFilesExpander.Visibility = Visibility.Visible;
                    PinnedExpanderBackgroundRectangle.Visibility = Visibility.Visible;
                    setPinBarOptionVisibility(false);
                    break;
                case "hidden":
                    securitySeverityIndex = 4;
                    isWindowsHelloRequiredForPins = true;
                    PinnedFilesExpander.Visibility = Visibility.Collapsed;
                    PinnedExpanderBackgroundRectangle.Visibility = Visibility.Collapsed;
                    break;
            }
            localSettings.Values["PinBarBehavior"] = securitySeverityIndex;
        }

        private void setPinBarOptionVisibility(bool shouldBeVisible)
        {
            PinsAlwaysOpenRadioButton.IsEnabled = shouldBeVisible;
            PinsRememberStateRadioButton.IsEnabled = shouldBeVisible;
            PinsAlwaysClosedRadioButton.IsEnabled = shouldBeVisible;
            PinsHiddenRadioButton.IsEnabled = shouldBeVisible;
            if (shouldBeVisible) EnableAllOptionsForPinsButton.Visibility = Visibility.Collapsed;
            else if (!shouldBeVisible) EnableAllOptionsForPinsButton.Visibility = Visibility.Visible;
        }

        private async void Expander_Expanding(Microsoft.UI.Xaml.Controls.Expander sender, Microsoft.UI.Xaml.Controls.ExpanderExpandingEventArgs args)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["HasPinBarBeenExpanded"] = PinnedFilesExpander.IsExpanded;

            if (!isWindowsHelloRequiredForPins && !string.IsNullOrEmpty(pinnedFolderToken)) { loadFromCache("pinned"); pinnedFileGrid.Visibility = Visibility.Visible; }
            else if (isWindowsHelloRequiredForPins)
            {
                pinnedFileGrid.Visibility = Visibility.Collapsed;
                WinHelloProgressRing.IsActive = true;
                WinHelloWaitingIndicator.Visibility = Visibility.Visible;
                bool isVerified = await VerifyUserWithWindowsHelloAsync("You need to verify with Windows Hello™️ to access your pinned files.");

                if (isVerified)
                {
                    pinnedFileGrid.Visibility = Visibility.Visible;
                    if (!string.IsNullOrEmpty(pinnedFolderToken)) obtainFolderAndFiles("pinned", null);
                }
                else PinnedFilesExpander.IsExpanded = false;

                WinHelloProgressRing.IsActive = false;
                WinHelloWaitingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void PinnedFilesExpander_Collapsed(Microsoft.UI.Xaml.Controls.Expander sender, Microsoft.UI.Xaml.Controls.ExpanderCollapsedEventArgs args)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["HasPinBarBeenExpanded"] = PinnedFilesExpander.IsExpanded;
        }


        public async Task<bool> VerifyUserWithWindowsHelloAsync(string message)
        {
            var availability = await UserConsentVerifier.CheckAvailabilityAsync();

            if (availability == UserConsentVerifierAvailability.Available)
            {
                var result = await UserConsentVerifier.RequestVerificationAsync(message);
                switch (result)
                {

                    case UserConsentVerificationResult.Verified:
                        return true;

                    case UserConsentVerificationResult.Canceled:
                        return false;

                    case UserConsentVerificationResult.DeviceBusy:
                        return false;

                    case UserConsentVerificationResult.DeviceNotPresent:
                        return false;

                    case UserConsentVerificationResult.DisabledByPolicy:
                        return false;

                    case UserConsentVerificationResult.NotConfiguredForUser:
                        return false;

                    case UserConsentVerificationResult.RetriesExhausted:
                        return false;

                    default:
                        return false;
                }
            }
            else
            {
                // the device does not support Windows Hello, display an error message
                return false;
            }
        }

        private async void EnableAllOptionsForPinsButton_Click(object sender, RoutedEventArgs e)
        {
            bool isVerified = await VerifyUserWithWindowsHelloAsync("You need to verify with Windows Hello™️ to access your pinned files.");
            if (isVerified) setPinBarOptionVisibility(true);
        }

        private void PickPinnedFolderHyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            askForAccess("pinned");
        }

        private async void AboutDropStackCloseButton_Click(object sender, RoutedEventArgs e)
        {
            AboutDropStackGrid.Opacity = 0;
            AboutDropStackContentGrid.Translation = new Vector3(0, 50, 0);
            await Task.Delay(500);
            AboutDropStackGrid.Visibility = Visibility.Collapsed;

            PerformanceSettingsExpander.IsExpanded = false;
            ExpanderSettingsExpander.IsExpanded = false;
            SecondaryFolderSettingsExpander.IsExpanded = false;
            AppearanceAndBehaviorExpander.IsExpanded = false;
            AboutDropStackExpander.IsExpanded = false;
            PrivacyStamentExpander.IsExpanded = false;
        }

        private void PrimaryPortalFolderCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["showPrimaryPortal"] = true;
            showPrimPortal = true;
        }

        private void PrimaryPortalFolderCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["showPrimaryPortal"] = false;
            showPrimPortal = false;
        }

        private void SecondaryPortalFolderCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            CheckBox thisCheckBox = sender as CheckBox;

            switch (thisCheckBox.Tag.ToString())
            {
                case "1":
                    localSettings.Values["showSecondaryPortal1"] = true;
                    showSecPortal1 = true;
                    break;
                case "2":
                    localSettings.Values["showSecondaryPortal2"] = true;
                    showSecPortal2 = true;
                    break;
                case "3":
                    localSettings.Values["showSecondaryPortal3"] = true;
                    showSecPortal3 = true;
                    break;
                case "4":
                    localSettings.Values["showSecondaryPortal4"] = true;
                    showSecPortal4 = true;
                    break;
                case "5":
                    localSettings.Values["showSecondaryPortal5"] = true;
                    showSecPortal5 = true;
                    break;
            }
        }

        private void SecondaryPortalFolderCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            CheckBox thisCheckBox = sender as CheckBox;

            switch (thisCheckBox.Tag.ToString())
            {
                case "1":
                    localSettings.Values["showSecondaryPortal1"] = false;
                    showSecPortal1 = false;
                    break;
                case "2":
                    localSettings.Values["showSecondaryPortal2"] = false;
                    showSecPortal2 = false;
                    break;
                case "3":
                    localSettings.Values["showSecondaryPortal3"] = false;
                    showSecPortal3 = false;
                    break;
                case "4":
                    localSettings.Values["showSecondaryPortal4"] = false;
                    showSecPortal4 = false;
                    break;
                case "5":
                    localSettings.Values["showSecondaryPortal5"] = false;
                    showSecPortal5 = false;
                    break;
            }
        }

        private void PrimaryPortalFolderChangeButton_Click(object sender, RoutedEventArgs e)
        {
            askForAccess("regular");
        }

        private void SecondaryPortalFolderChangeButton_Click(object sender, RoutedEventArgs e)
        {
            Button thisButton = sender as Button;
            
            switch (thisButton.Tag.ToString())
            {
                case "1":
                    askForAccess("Sec1");
                    break;
                case "2":
                    askForAccess("Sec2");
                    break;
                case "3":
                    askForAccess("Sec3");
                    break;
                case "4":
                    askForAccess("Sec4");
                    break;
                case "5":
                    askForAccess("Sec5");
                    break;
            }
        }

        private void ApplyMultiFolderSettings_Click(object sender, RoutedEventArgs e)
        {
            obtainFolderAndFiles("regular", null);
        }

        private void ThemePickerCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            string selectedItem = comboBox.SelectedItem.ToString();
            setTheme(selectedItem);
            ApplicationData.Current.LocalSettings.Values["SelectedTheme"] = selectedItem;
        }

        private void PrivacyStamentExpander_Expanding(Expander sender, ExpanderExpandingEventArgs args)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["IsPanosUnlocked"] = true;
            unlockPanos(true);
        }

        private void setTheme(string themeName)
        {
            ParallaxImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/Themes/" + themeName + ".png"));

            if (themeName != "Default")
            {
                PinnedExpanderBackgroundRectangle.Opacity = 0.25;
            }
            else
            {
                PinnedExpanderBackgroundRectangle.Opacity = 0.15;
            }
        }

        private async void unlockPanos(bool showNotification)
        {
            if (!ThemePickerCombobox.Items.Contains("Panos"))
            {
                ThemePickerCombobox.Items.Add("Panos");

                if (showNotification)
                {
                    //show teaching tip
                    panosUnlockedTeachingTip.IsOpen = true;
                    for (int i = 0; i < 100; i++)
                    {
                        await Task.Delay(10);
                        panosUnlockedTimer.Value = i;
                    }
                    panosUnlockedTeachingTip.IsOpen = false;
                    await Task.Delay(100);
                    panosUnlockedTimer.Value = 0;
                }
            }
        }

        private void panosUnlockedTeachingTip_ActionButtonClick(TeachingTip sender, object args)
        {
            ThemePickerCombobox.SelectedItem = "Panos";
            setTheme("Panos");
        }

        private void RegularFileCapNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            loadedItems = Convert.ToInt32(RegularFileCapNumberBox.Value);
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["NormalLoadedItems"] = Convert.ToInt32(RegularFileCapNumberBox.Value);
        }

        private void ThumbnailCapNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            loadedThumbnails = Convert.ToInt32(ThumbnailCapNumberBox.Value);
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["LoadedThumbnails"] = Convert.ToInt32(ThumbnailCapNumberBox.Value);
        }

        private void ThumbnailResolutionNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            thumbnailResolution = Convert.ToInt32(ThumbnailResolutionNumberBox.Value);
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["ThumbnailResolution"] = Convert.ToInt32(ThumbnailResolutionNumberBox.Value);
        }

        private void ResetCapsToDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            RegularFileCapNumberBox.Value = Convert.ToDouble(1000);
            ThumbnailCapNumberBox.Value = Convert.ToDouble(250);
            ThumbnailResolutionNumberBox.Value = Convert.ToDouble(64);
        }

        private async void saveToCache(string source, ObservableCollection<FileItem> subjectToCache)
        {
            if (subjectToCache.Count > loadedItems) foreach (FileItem item in subjectToCache) { if (subjectToCache.IndexOf(item) > loadedItems) subjectToCache.Remove(item); }
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<FileItem>));
            StringWriter writer = new StringWriter();
            serializer.Serialize(writer, subjectToCache);
            string xmlContent = writer.ToString();
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            if (source == "regular")
            {
                StorageFile file = await localFolder.CreateFileAsync("cachedfiles.xml", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, xmlContent);
            }
            else if (source == "pinned")
            {
                StorageFile file = await localFolder.CreateFileAsync("cachedpins.xml", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, xmlContent);
            }
        }

        private async void loadFromCache(string source)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            string fileName = "cachedfiles.xml";
            if (source == "pinned") fileName = "cachedpins.xml";
            StorageFile file = null;
            try { file = await localFolder.GetFileAsync(fileName); }
            catch { obtainFolderAndFiles(source, null); }
            if (file == null) obtainFolderAndFiles(source, null);
            else
            {
                string xmlContent = await FileIO.ReadTextAsync(file);
                ObservableCollection<FileItem> cachedFileMetadataList = null;


                // Deserialize the XML string into ArrayOfFileItem
                XmlSerializer serializer = new XmlSerializer(typeof(ArrayOfFileItem));
                using (TextReader reader = new StringReader(xmlContent))
                {
                    ArrayOfFileItem arrayOfFileItem = (ArrayOfFileItem)serializer.Deserialize(reader);

                    // Create an ObservableCollection from the deserialized data
                    cachedFileMetadataList = new ObservableCollection<FileItem>(arrayOfFileItem.Items);
                }

                if (cachedFileMetadataList.Count == 0)
                {
                    obtainFolderAndFiles(source, cachedFileMetadataList);
                }
                else
                {
                    if (source == "regular")
                    {
                        fileMetadataListCopy = cachedFileMetadataList;
                        regularFileListView.ItemsSource = cachedFileMetadataList;
                    }
                    else if (source == "pinned")
                    {
                        pinnedFileListView.ItemsSource = cachedFileMetadataList;
                    }
                    //check for new files
                    obtainFolderAndFiles(source, cachedFileMetadataList);
                    loadInThumbnails(source);
                }
            }
        }

        private void CheckBoxBehaviorCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkboxBehavior = CheckBoxBehaviorCombobox.SelectedIndex;
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["CheckboxBehavior"] = checkboxBehavior;
            adjustCheckboxBehavior();
        }

        private void adjustCheckboxBehavior()
        {
            RegistryKey explorerKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced");

            switch (checkboxBehavior)
            {
                case 0:
                    regularFileListView.SelectionMode = ListViewSelectionMode.Extended;
                    break;
                case 1:
                    int autoCheckSelect = (int)explorerKey.GetValue("AutoCheckSelect", 0);
                    if (autoCheckSelect == 0) regularFileListView.SelectionMode = ListViewSelectionMode.Extended;
                    else regularFileListView.SelectionMode = ListViewSelectionMode.Multiple;
                    break;
                case 2:
                    regularFileListView.SelectionMode = ListViewSelectionMode.Multiple;
                    break;
            }

            // legend:
            // 0: never show checkboxes
            // 1: show checkboxes according to file explorer setting
            // 2: always show checkboxes
        }

        private void SelectModeButton_Click(object sender, RoutedEventArgs e)
        {
            if      (regularFileListView.SelectionMode == ListViewSelectionMode.Extended) regularFileListView.SelectionMode = ListViewSelectionMode.Multiple;
            else if (regularFileListView.SelectionMode == ListViewSelectionMode.Multiple) regularFileListView.SelectionMode = ListViewSelectionMode.Extended;
        }

        private void LaunchModePickerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["DefaultLaunchModeIndex"] = Convert.ToInt32(LaunchModePickerComboBox.SelectedIndex);
        }

        private void PinToolbarInSimpleModeToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["AlwaysShowToolbarInSimpleModeBoolean"] = PinToolbarInSimpleModeToggleSwitch.IsOn;
        }

        private void LaunchMiniModeButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MiniMode();
            mainWindow.Activate();

            this.Close();
        }

        private void SimpleModeWindowingToggle_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["FreeWindowingInSimpleMode"] = SimpleModeWindowingToggle.IsOn;
        }

        private void SearchThresholdNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["SearchThreshold"] = Convert.ToInt32(SearchThresholdNumberBox.Value);
            searchThreshold = Convert.ToInt32(SearchThresholdNumberBox.Value);
        }

        private void ShowDetailsPaneFlagButton_Click(object sender, RoutedEventArgs e)
        {
            /*int detailsPaneHeight = (int)DetailsPaneGrid.Height;

            if (DetailsPaneGrid.Visibility == Visibility.Collapsed)
            {
                DetailsPaneGrid.Visibility = Visibility.Visible;
                regularFileGrid.Translation = new Vector3(0, -detailsPaneHeight, 0);
                regularFileGrid.Height = regularFileGrid.Height - detailsPaneHeight;
            }
            else
            {
                DetailsPaneGrid.Visibility = Visibility.Collapsed;
                regularFileGrid.Translation = new Vector3(0, 0, 0);
                regularFileGrid.Height = regularFileGrid.Height + detailsPaneHeight;
            }*/
        }

        private async void updatePreviewArea(FileItem fileItem, bool isSeveralItems)
        {
            if (showDetailsPane)
            {
                if (fileItem != null && regularFileListView.SelectedItems.Count == 1)
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(fileItem.FilePath);
                    BasicProperties basicProperties = await file.GetBasicPropertiesAsync();

                    BitmapImage bitmapThumbnail = new BitmapImage();
                    StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, Convert.ToUInt32(128));
                    bitmapThumbnail.SetSource(thumbnail);
                    DetailsPaneFileThumbnail.Source = bitmapThumbnail;
                    DetailsPaneFileThumbnail.Rotation = 0;

                    previewedItem = fileItem;

                    DetailsFileNameDisplay.Text = file.DisplayName;
                    DetailsFileTypeDisplay.Text = file.DisplayType;
                    DetailsFileSizeDisplay.Text = fileItem.FileSize;
                    DetailsFileSizeSuffixDisplay.Text = fileItem.FileSizeSuffix;
                    DetailsFileModifiedDateDisplay.Text = fileItem.ModifiedDate;

                    adjustShownDetailsButtons(fileItem.TypeTag, file.FileType);

                    if (regularFileListView.SelectionMode != ListViewSelectionMode.Multiple)
                    {
                        showOrHideDetailsPane(true);
                    }
                }
                else
                {
                    showOrHideDetailsPane(false);
                    await Task.Delay(200);
                    adjustShownDetailsButtons(null, null);
                }

                DetailsPaneFileThumbnail.Visibility = Visibility.Visible;
                DetailsPaneVideoPlayer.Visibility = Visibility.Collapsed;
                mediaPlayer.Pause();
                DetailsPaneVideoPlayer.SetMediaPlayer(null);

                DetailsPaneFileThumbnail.CenterPoint = new Vector3(
                    (float)DetailsPaneFileThumbnail.ActualWidth / 2,
                    (float)DetailsPaneFileThumbnail.ActualHeight / 2,
                    0);
            }
        }

        private void adjustShownDetailsButtons(string typeTag, string type)
        {
            foreach (AppBarButton button in DetailsPaneCommandBar.PrimaryCommands.OfType<AppBarButton>())
            {
                AppBarButton btn = button as AppBarButton;
                string btnTag = btn.Tag as string;
                if (btnTag == "alwaysshow") button.IsEnabled = false;
                else button.Visibility = Visibility.Collapsed;
            }
            DetailsPaneSeparator.Visibility = Visibility.Collapsed;

            switch (typeTag)
            {
                // modify these
                case null:
                    break;
                case "pics":
                    DetailsPaneDeleteFlyoutButton.IsEnabled = true;
                    DetailsPaneOpenWithButton.IsEnabled = true;
                    DetailsPanePreviewButton.IsEnabled = true;
                    DetailsPaneCropButton.IsEnabled = true;
                    DetailsPaneRotateButton.Visibility = Visibility.Visible;
                    DetailsPaneSeparator.Visibility = Visibility.Visible;
                    break;
                case "vids":
                    DetailsPaneDeleteFlyoutButton.IsEnabled = true;
                    DetailsPaneOpenWithButton.IsEnabled = true;
                    DetailsPanePreviewButton.IsEnabled = true;
                    DetailsPanePlayButton.Visibility = Visibility.Visible;
                    DetailsPaneSeparator.Visibility = Visibility.Visible;
                    break;
                case "music":
                    DetailsPaneDeleteFlyoutButton.IsEnabled = true;
                    DetailsPaneOpenWithButton.IsEnabled = true;
                    DetailsPanePlayButton.Visibility = Visibility.Visible;
                    DetailsPaneSeparator.Visibility = Visibility.Visible;
                    break;
                default:
                    switch (type)
                    {
                        case ".pdf":
                            DetailsPaneDeleteFlyoutButton.IsEnabled = true;
                            DetailsPaneOpenWithButton.IsEnabled = true;
                            DetailsPanePreviewButton.IsEnabled = true;
                            break;
                        default:
                            DetailsPaneDeleteFlyoutButton.IsEnabled = true;
                            DetailsPaneOpenWithButton.IsEnabled = true;
                            break;
                    }
                    break;
            }
        }

        private async void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.Properties.Title = previewedItem.FileDisplayName;
            request.Data.Properties.Description = previewedItem.FileType;

            StorageFile file = await StorageFile.GetFileFromPathAsync(previewedItem.FilePath);
            if (file != null)
            {
                List<IStorageItem> storageItems = new List<IStorageItem>();
                storageItems.Add(file);
                request.Data.SetStorageItems(storageItems);
            }
            else
            {
                request.FailWithDisplayText("File not found.");
            }
        }

        private void DetailsPaneFileThumbnail_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        MediaPlayer mediaPlayer = new MediaPlayer();
        private async void DetailsPanePlayButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(previewedItem.FilePath);
            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

            if (previewedItem.TypeTag == "vids" || previewedItem.TypeTag == "music")
            {
                if (DetailsPaneVideoPlayer.Visibility == Visibility.Collapsed)
                {
                    if (previewedItem.TypeTag == "vids")
                    {
                        DetailsPaneFileThumbnail.Visibility = Visibility.Collapsed;
                        DetailsPaneVideoPlayer.Visibility = Visibility.Visible;
                    }

                    mediaPlayer.Source = MediaSource.CreateFromStream(stream, file.ContentType);
                    if (previewedItem.TypeTag == "vids") DetailsPaneVideoPlayer.SetMediaPlayer(mediaPlayer);
                    mediaPlayer.Play();                    
                }
                else
                {
                    if (mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                    {
                        mediaPlayer.Pause();
                    }
                    else if (mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Paused)
                    {
                        mediaPlayer.Play();
                    }
                }
            }
        }

        private async void DetailsPanePreviewButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(previewedItem.FilePath);
            switch (previewedItem.TypeTag)
            {
                case "pics":
                    var quickImageViewer = new ImageView(new ImageViewerSettings { filePath = previewedItem.FilePath });
                    quickImageViewer.Activate();
                    break;
                case "vids":
                    var quickVideoViewer = new VideoView(previewedItem.FilePath);
                    quickVideoViewer.Activate();
                    break;
                default:
                    switch (file.FileType)
                    {
                        case ".pdf":
                            var quickPDFViewer = new PDFView(previewedItem.FilePath);
                            quickPDFViewer.Activate();
                            break;
                    }
                    break;
            }
        }

        private void DetailsPaneAnnotateButton_Click(object sender, RoutedEventArgs e)
        {
            var quickImageViewer = new ImageView(new ImageViewerSettings { filePath = previewedItem.FilePath, isAnnotating = true });
            quickImageViewer.Activate();
        }

        private void DetailsPaneCropButton_Click(object sender, RoutedEventArgs e)
        {
            var quickImageViewer = new ImageView(new ImageViewerSettings { filePath = previewedItem.FilePath, isCropping = true });
            quickImageViewer.Activate();
        }

        private async void DetailsPaneRotateButton_Click(object sender, RoutedEventArgs e)
        {
            System.Drawing.Image image = System.Drawing.Image.FromFile(previewedItem.FilePath);
            image.RotateFlip(RotateFlipType.Rotate90FlipNone);
            image.Save(previewedItem.FilePath);

            DetailsPaneFileThumbnail.RotationTransition = new ScalarTransition();
            DetailsPaneFileThumbnail.Rotation = DetailsPaneFileThumbnail.Rotation + 90;
            DetailsPaneFileThumbnail.RotationTransition = null;

            int index = regularFileListView.SelectedIndex;
            StorageFile file = await StorageFile.GetFileFromPathAsync(previewedItem.FilePath);

            FileItem fileItem = regularFileListView.Items[index] as FileItem;
            BitmapImage bitmapThumbnail = new BitmapImage();
            StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, Convert.ToUInt32(thumbnailResolution));
            bitmapThumbnail.SetSource(thumbnail);
            fileItem.FileIcon = bitmapThumbnail;
        }

        private void DetailsPaneOpenWithButton_Click(object sender, RoutedEventArgs e)
        {
            ShowOpenWithDialog(previewedItem.FilePath);
        }

        public static void ShowOpenWithDialog(string path)
        {
            var args = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll");
            args += ",OpenAs_RunDLL " + path;
            Process.Start("rundll32.exe", args);
        }

        private async void FileDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int deleteIndex = regularFileListView.SelectedIndex;
            fileMetadataListCopy.RemoveAt(deleteIndex);
            StorageFile file = await StorageFile.GetFileFromPathAsync(previewedItem.FilePath);
            await file.DeleteAsync();
            saveToCache("regular", fileMetadataListCopy);
            regularFileListView.ItemsSource = fileMetadataListCopy;
            DeleteFlyout.Hide();
        }

        private async void EnterKeyPressFileNameEdit_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            // this is called on textcompositionended event for the DetailsFileNameDisplay
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(previewedItem.FilePath);
                await file.RenameAsync(DetailsFileNameDisplay.Text + file.FileType);
                StorageFolder folder = await file.GetParentAsync();
                // get file again to make sure renaming has been adopted by the system
                regularFileListView.ItemsSource = fileMetadataListCopy;
                foreach (FileItem item in fileMetadataListCopy) { if (item.FilePath == previewedItem.FilePath) {
                        item.FileName = DetailsFileNameDisplay.Text + file.FileType;
                        item.FileDisplayName = DetailsFileNameDisplay.Text;
                        item.FilePath = folder.Path + "\\" + previewedItem.FileName;
                    } }
                saveToCache("regular", fileMetadataListCopy);
            }
            catch { }
        }

        private void CreateOrUpdateSpringAnimation(float finalValue)
        {
            if (_springAnimation == null)
            {
                _springAnimation = _compositor.CreateSpringVector3Animation();
                _springAnimation.Target = "Scale";
            }

            _springAnimation.FinalValue = new Vector3(finalValue);
        }

        private void DetailsPaneFileThumbnail_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            // Scale up to 1.5
            CreateOrUpdateSpringAnimation(1.5f);

            (sender as UIElement).StartAnimation(_springAnimation);
        }

        private void DetailsPaneFileThumbnail_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            // Scale back down to 1.0
            CreateOrUpdateSpringAnimation(1.0f);

            (sender as UIElement).StartAnimation(_springAnimation);
        }

        private void ShowDetailsPaneToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["ShowDetailsPane"] = ShowDetailsPaneToggleSwitch.IsOn;
            showDetailsPane = ShowDetailsPaneToggleSwitch.IsOn;
        }

        private async void DetailsPaneFileThumbnail_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(previewedItem.FilePath);
                var success = await Launcher.LaunchFileAsync(file);
            }
            catch { }
            await Task.Delay(250);
            regularFileListView.SelectedItem = null;
            pinnedFileListView.SelectedItem = null;
        }

        private void DetailsSheetGrid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var grid = sender as Grid;
            var newHeight = grid.Height - e.Delta.Translation.Y;
            if (newHeight < 20) newHeight = 20;
            if (newHeight > 400) newHeight = 400;
            grid.Height = newHeight;
        }

        private void DetailsSheetGrid_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            var grid = sender as Grid;

            var velocity = e.Velocities.Linear.Y;
            var currentHeight = grid.Height;
            var newHeight = currentHeight;
            if (velocity > 0)
            {
                if (currentHeight > 300) newHeight = 300;
                else newHeight = 120;
            }
            else
            {
                newHeight = 300;
            }

            if (newHeight == 120) WindowGrid.Opacity = 1;

            animateDetailsPane(newHeight);
        }

        public async void showOrHideDetailsPane(bool show)
        {
            if (show)
            {
                await Task.Delay(150);
                if (!hasDoubleTapped)
                {
                    animateDetailsPane(300);
                    WindowGrid.Opacity = 0.8;
                }
                else
                {
                    await Task.Delay(200);
                    hasDoubleTapped = false;
                }
            }
            else
            {
                animateDetailsPane(120);
                WindowGrid.Opacity = 1;
            }
        }

        public void animateDetailsPane(double height)
        {
            Grid grid = DetailsSheetGrid;

            // the adjusted difference that needs to be covered by an animation
            double adjustedDifference = height - grid.Height;
            grid.Height = height;

            grid.TranslationTransition = null;
            Vector3 oldTranslation = grid.Translation;
            grid.Translation = oldTranslation + new Vector3(0, (float)adjustedDifference, 0);

            grid.TranslationTransition = new Vector3Transition();
            Vector3 unadjustedTranslation = grid.Translation;
            grid.Translation = unadjustedTranslation + new Vector3(0, (float)(-adjustedDifference), 0);
        }

        private void DismissDetailsSheetButton_Click(object sender, RoutedEventArgs e)
        {
            showOrHideDetailsPane(false);
        }

        private async void regularFileListView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (regularFileListView.SelectedItems.Count == 1)
            {
                openCompactCommandBar = true;
                await Task.Delay(500);
                if (openCompactCommandBar)
                {
                    var flyout = FlyoutBase.GetAttachedFlyout((FrameworkElement)sender);
                    var options = new FlyoutShowOptions()
                    {
                        Position = e.GetPosition((FrameworkElement)sender),
                        ShowMode = FlyoutShowMode.Transient
                    };
                    flyout?.ShowAt((FrameworkElement)sender, options);
                }
            }
            updateShownContextMenuItems();
        }
        
        private async void updateShownContextMenuItems()
        {
            if (GlobalClickedItems.Count > 1)
            {
                FlyoutPreviewButton.IsEnabled = false;
                FlyoutPreviewButtonSec.IsEnabled = false;
                FlyoutRotateButton.Visibility = Visibility.Collapsed;
                FlyoutRotateButtonSec.Visibility = Visibility.Collapsed;
                FlyoutOpenWithButton.IsEnabled = false;
                FlyoutRevealButton.IsEnabled = false;
                FlyoutDeleteButton.IsEnabled = false;
            }
            else if (GlobalClickedItems.Count == 1) 
            {
                previewedItem = GlobalClickedItems.First() as FileItem;
                StorageFile file = await StorageFile.GetFileFromPathAsync(previewedItem.FilePath);

                FlyoutOpenWithButton.IsEnabled = true;
                FlyoutRevealButton.IsEnabled = true;
                FlyoutDeleteButton.IsEnabled = true;

                switch (previewedItem.TypeTag)
                {
                    // modify these
                    case null:
                        break;
                    case "pics":
                        FlyoutPreviewButton.IsEnabled = true;
                        FlyoutPreviewButtonSec.IsEnabled = true;
                        FlyoutRotateButton.Visibility = Visibility.Visible;
                        FlyoutRotateButtonSec.Visibility = Visibility.Visible;
                        break;
                    case "vids":
                        FlyoutPreviewButton.IsEnabled = true;
                        FlyoutPreviewButtonSec.IsEnabled = true;
                        FlyoutRotateButton.Visibility = Visibility.Collapsed;
                        FlyoutRotateButtonSec.Visibility = Visibility.Collapsed;
                        break;
                    case "music":
                        FlyoutPreviewButton.IsEnabled = true;
                        FlyoutPreviewButtonSec.IsEnabled = true;
                        FlyoutRotateButton.Visibility = Visibility.Collapsed;
                        FlyoutRotateButtonSec.Visibility = Visibility.Collapsed;
                        break;
                    default:
                        switch (file.FileType)
                        {
                            case ".pdf":
                                FlyoutPreviewButton.IsEnabled = true;
                                FlyoutPreviewButtonSec.IsEnabled = true;
                                FlyoutRotateButton.Visibility = Visibility.Collapsed;
                                FlyoutRotateButtonSec.Visibility = Visibility.Collapsed;
                                break;
                            default:
                                FlyoutPreviewButton.IsEnabled = false;
                                FlyoutPreviewButtonSec.IsEnabled = false;
                                FlyoutRotateButton.Visibility = Visibility.Collapsed;
                                FlyoutRotateButtonSec.Visibility = Visibility.Collapsed;
                                break;
                        }
                        break;

                }
            }
        }

        private async void FlyoutOpenButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < regularFileListView.SelectedItems.Count; i++)
            {
                FileItem selectedFile = (FileItem)regularFileListView.SelectedItems[i] as FileItem;

                try
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(selectedFile.FilePath);

                    // launch the file
                    var success = await Launcher.LaunchFileAsync(file);
                }

                catch
                {
                    // handle the exception
                }
            }
        }

        private async void FlyoutCopyButton_Click(object sender, RoutedEventArgs e)
        {
            IList<object> itemsToCopy = GlobalClickedItems;

            await Task.Run(async () =>
            {
                List<IStorageItem> fileList = new List<IStorageItem>();

                // create a capture variable that is a value type
                FileItem[] filesToCopy = new FileItem[itemsToCopy.Count];

                // copy the elements of itemsToCopy to the capture variable
                for (int i = 0; i < itemsToCopy.Count; i++)
                {
                    filesToCopy[i] = (FileItem)itemsToCopy[i];
                }

                foreach (FileItem selectedFile in filesToCopy)
                {
                    string filePath = selectedFile.FilePath;

                    // get the file
                    StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);

                    fileList.Add(file);
                }

                // create a new data package
                var dataPackage = new DataPackage();

                // add the file to the data package
                dataPackage.SetStorageItems(fileList);

                // copy the data package to the clipboard
                Clipboard.SetContent(dataPackage);

                //attempting to flush the clipboard several times
                bool worked = false;
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        Clipboard.Flush();
                        worked = true;
                        break;
                    }
                    catch { }
                    await Task.Delay(10);
                }
                if (!worked)
                {
                    var xmlPayload = new string($@"<toast>
                        <visual>
                            <binding template=""ToastGeneric"">
                                <text>{"Copy failed"}</text>
                                <text>{"Windows refused to grant permission to the clipboard."}</text>
                            </binding>
                        </visual>
                    </toast>");
                    var toast = new AppNotification(xmlPayload);
                    AppNotificationManager.Default.Show(toast);
                }
            });
        }

        private async void FlyoutRevealButton_Click(object sender, RoutedEventArgs e)
        {
            FileItem currentFile = GlobalClickedItems[0] as FileItem;
            StorageFile file = await StorageFile.GetFileFromPathAsync(currentFile.FilePath);
            StorageFolder folder = await file.GetParentAsync();
            await Launcher.LaunchFolderAsync(folder);
        }

        private void FlyoutSelectButton_Click(object sender, RoutedEventArgs e)
        {
            regularFileListView.SelectionMode = ListViewSelectionMode.Multiple;
        }
    }
}