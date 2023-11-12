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
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Threading;
using System.Xml.Serialization;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials.UI;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Microsoft.Windows.AppLifecycle;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.StartScreen;
using Windows.UI.ViewManagement;
using System.Numerics;
using Microsoft.Windows.System;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI;
using WinUIEx;
using WinRT.Interop;
using System.ComponentModel.Design;
using Windows.Storage.Search;
using static System.Net.WebRequestMethods;
using Windows.ApplicationModel.Contacts;
using System.Diagnostics;
using Windows.Data.Xml.Dom;
using System.Data;
using System.Data.SqlTypes;
using Microsoft.Win32;
using Microsoft.Web.WebView2.Core;
using Microsoft.UI.Xaml.Documents;
using System.ComponentModel;
using Microsoft.UI.Windowing;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic.FileIO;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DropStackWinUI
{
    public class FileItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string FileName { get; set; }
        public string FileDisplayName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public string TypeTag { get; set; }
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
        public static IList<string> MusicFileTypes => pictureFileTypes; static IList<string> musicFileTypes = new List<string> { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma", ".m4a", ".mid", ".amr", ".aiff", ".ape" };
        public static IList<string> VideoFileTypes => videoFileTypes; static IList<string> videoFileTypes = new List<string> { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm", ".3gp", ".m4v", ".mpeg", ".mpg", ".rm", ".vob" };
        public static IList<string> ApplicationFileTypes => applicationFileTypes; static IList<string> applicationFileTypes = new List<string> { ".exe", ".dmg", ".app", ".deb", ".apk", ".msi", ".msix", ".rpm", ".jar", ".bat", ".sh", ".com", ".vb", ".gadget", ".ipa" };
        public static IList<string> PresentationFileTypes => presentationFileTypes; static IList<string> presentationFileTypes = new List<string> { ".ppt", ".pptx", ".key", ".odp" };
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
                PinnedExpanderBackgroundRectangle.Visibility = Visibility.Visible;
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
                            if (OOBEgrid.Visibility == Visibility.Visible) obtainFolderAndFiles("regular", null, true);
                            RegularFolderPath = folder.Path;
                            PrimaryPortalFolderChangeButton.Content = folder.Name;
                            break;
                        case "pinned":
                            ApplicationData.Current.LocalSettings.Values["PinnedFolderToken"] = currentFolderToken;
                            pinnedFolderToken = currentFolderToken;
                            if (OOBEgrid.Visibility == Visibility.Collapsed) enableButtonVisibility();
                            obtainFolderAndFiles("pinned", null, true);
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

        public async void obtainFolderAndFiles(string source, ObservableCollection<FileItem> cachedItems, bool isLoadingNew)
        {
            ObservableCollection<FileItem> fileMetadataList = new ObservableCollection<FileItem>();

            if (source == "regular")
            {
                if (cachedItems != null) fileMetadataList = cachedItems;
                regularFileListView.ItemsSource = fileMetadataList;
                _filteredFileMetadataList = fileMetadataList;
                fileMetadataListCopy = fileMetadataList;
            }
            else if (source == "pinned")
            {
                if (cachedItems != null) fileMetadataList = cachedItems;
                pinnedFileListView.ItemsSource = fileMetadataList;
                _filteredPinnedFileMetadataList = fileMetadataList;
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
                if (source == "regular")
                {
                    PortalFileLoadingProgressBar.Value = 0;
                    PortalFileLoadingProgressBar.Opacity = 1; 
                }
                IProgress<int> progress = new Progress<int>(value => PortalFileLoadingProgressBar.Value = value);
                double totalFiles = Convert.ToDouble(files.Count);
                if (totalFiles > 1024) totalFiles = 1024;
                int currentFile = 1;
                int addIndex = 0;
                bool shouldContinue = true;

                foreach (StorageFile file in files.Take(loadedItems))
                {
                    if (shouldContinue)
                    {
                        if (source == "regular")
                        {
                            double percentageOfFiles = currentFile / totalFiles;
                            percentageOfFiles = percentageOfFiles * 100;
                            progress.Report(Convert.ToInt32(percentageOfFiles));
                        }

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
                            for (int i = 0; i < cachedItems.Count; i++)
                            {
                                FileItem currentFileItem = cachedItems.ElementAt(i);
                                if (currentFileItem.FileName == file.Name)
                                {
                                    //assume that from now on, files are cached
                                    shouldContinue = false;
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

                                fileMetadataList.Insert(addIndex, fileItem);
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

                                fileMetadataList.Insert(addIndex, fileItem);
                            }
                        }
                        else break;
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
                saveToCache(source, fileMetadataList);



                //load thumbnails
                List<FileThumbnail> thumbnails = new();
                currentFile = 1;
                PortalFileLoadingProgressBar.Value = 0;
                foreach (FileItem item in fileMetadataList.Take(loadedThumbnails))
                {
                    if (item.FileIcon == null)
                    {
                        double percentageOfFiles = currentFile / totalFiles;
                        percentageOfFiles = percentageOfFiles * 100;
                        progress.Report(Convert.ToInt32(percentageOfFiles));

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

                PortalFileLoadingProgressBar.Opacity = 0;
                if (source == "regular")
                {
                    
                    _filteredFileMetadataList = fileMetadataList;
                    fileMetadataListCopy = fileMetadataList;

                }
                else if (source == "pinned")
                {
                    _filteredPinnedFileMetadataList = fileMetadataList;
                    pinnedFileMetadataListCopy = fileMetadataList;
                }
            }
            else
            {
                // Ah bleh
            }
        }

        private void fileListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GlobalClickedItems = regularFileListView.SelectedItems;
            updatePreviewArea(GlobalClickedItems[0] as FileItem, regularFileListView.SelectedItems.Count < 1);
        }

        private async void fileListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
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
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            //delete file
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await localFolder.GetFileAsync("cachedfiles.xml");
            await file.DeleteAsync();
            file = await localFolder.GetFileAsync("cachedpins.xml");
            await file.DeleteAsync();

            obtainFolderAndFiles("regular", null, true);
            obtainFolderAndFiles("pinned", null, true);
        }

        private void Query_ContentsChanged(IStorageQueryResultBase sender, object args)
        {
            obtainFolderAndFiles("regular", null, false);
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
                    obtainFolderAndFiles("pinned", pinnedFileMetadataListCopy, false);
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
                regularFileListView.Margin = new Thickness(0, 10, 0, 60);
                SearchTextBox.Focus(FocusState.Programmatic);
            }
            else if (SearchGrid.Opacity == 1)
            {
                SearchGrid.Opacity = 0;
                SearchGrid.Translation = new Vector3(0, -60, 0);
                SearchGrid.Visibility = Visibility.Collapsed;
                RegularAndPinnedFileGrid.Translation = new Vector3(0, 0, 0);
                regularFileListView.Margin = new Thickness(0, 10, 0, 0);
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

            switch (selectedRadioButton.Content)
            {
                case "Always opened":
                    securitySeverityIndex = 0;
                    isWindowsHelloRequiredForPins = false;
                    PinnedFilesExpander.IsExpanded = true;
                    break;
                case "Remember last state":
                    securitySeverityIndex = 1;
                    isWindowsHelloRequiredForPins = false;
                    localSettings.Values["HasPinBarBeenExpanded"] = PinnedFilesExpander.IsExpanded;
                    break;
                case "Always closed":
                    securitySeverityIndex = 2;
                    isWindowsHelloRequiredForPins = false;
                    PinnedFilesExpander.IsExpanded = false;
                    break;
                case "Protect with Windows Hello™️":
                    securitySeverityIndex = 3;
                    isWindowsHelloRequiredForPins = true;
                    PinnedFilesExpander.IsExpanded = false;
                    setPinBarOptionVisibility(false);
                    break;
            }
            localSettings.Values["PinBarBehavior"] = securitySeverityIndex;
        }

        private void setPinBarOptionVisibility(bool shouldBeVisible)
        {
            PinsAlwaysOpenRadioButton.IsEnabled = shouldBeVisible;
            PinsRememberStateRadioButton.IsEnabled = shouldBeVisible;
            PinsAlwaysClosedRadioButton.IsEnabled = shouldBeVisible;
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
                    if (!string.IsNullOrEmpty(pinnedFolderToken)) obtainFolderAndFiles("pinned", null, false);
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
            if (isWindowsHelloRequiredForPins) setPinBarOptionVisibility(false);
            else if (!isWindowsHelloRequiredForPins) setPinBarOptionVisibility(true);
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
            obtainFolderAndFiles("regular", null, true);
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
            catch { obtainFolderAndFiles(source, null, true); }
            if (file == null) obtainFolderAndFiles(source, null, true);
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
                    obtainFolderAndFiles(source, cachedFileMetadataList, true);
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
                    obtainFolderAndFiles(source, cachedFileMetadataList, false);
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
            if (DetailsPaneGrid.Visibility == Visibility.Collapsed) DetailsPaneGrid.Visibility = Visibility.Visible;
            else DetailsPaneGrid.Visibility = Visibility.Collapsed;

        }

        private async void updatePreviewArea(FileItem fileItem, bool isSeveralItems)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(fileItem.FilePath);
            BasicProperties basicProperties = await file.GetBasicPropertiesAsync();

            BitmapImage bitmapThumbnail = new BitmapImage();
            StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, Convert.ToUInt32(128));
            bitmapThumbnail.SetSource(thumbnail);
            DetailsPaneFileThumbnail.Source = bitmapThumbnail;

            sharedFileItem = fileItem;

            DetailsFileNameDisplay.Text = file.Name;
            DetailsFileTypeDisplay.Text = file.DisplayType;
            DetailsFileSizeDisplay.Text = fileItem.FileSize;
            DetailsFileSizeSuffixDisplay.Text = fileItem.FileSizeSuffix;
            DetailsFileModifiedDateDisplay.Text = fileItem.ModifiedDate;

            if (file.FileType == ".pdf")
            {

            }
        }

        FileItem sharedFileItem = null;

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            //DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            //dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            //DataTransferManager.ShowShareUI();
        }

        private async void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.Properties.Title = sharedFileItem.FileDisplayName;
            request.Data.Properties.Description = sharedFileItem.FileType;

            StorageFile file = await StorageFile.GetFileFromPathAsync(sharedFileItem.FilePath);
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
    }
}