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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DropStackWinUI
{
    public class FileItem
    {
        public string FileName { get; set; }
        public string FileDisplayName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public string TypeTag { get; set; }
        public string FileSize { get; set; }
        public string FileSizeSuffix { get; set; }
        public string ModifiedDate { get; set; }
        [XmlIgnore]
        public BitmapImage FileIcon { get; set; }
        public double IconOpacity { get; set; }
        public double TextOpacity { get; set; }
        public double TextOpacityDate { get; set; }
        public double PillOpacity { get; set; }
        public bool ProgressActivity { get; set; }
    }

    public sealed partial class MainWindow : WinUIEx.WindowEx
    {
        public string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;
            return $"Version {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        string folderToken = ApplicationData.Current.LocalSettings.Values["FolderToken"] as string;
        string pinnedFolderToken = ApplicationData.Current.LocalSettings.Values["PinnedFolderToken"] as string;

        string secondaryFolderToken1 = ApplicationData.Current.LocalSettings.Values["SecFolderToken1"] as string;
        string secondaryFolderToken2 = ApplicationData.Current.LocalSettings.Values["SecFolderToken2"] as string;
        string secondaryFolderToken3 = ApplicationData.Current.LocalSettings.Values["SecFolderToken3"] as string;
        string secondaryFolderToken4 = ApplicationData.Current.LocalSettings.Values["SecFolderToken4"] as string;
        string secondaryFolderToken5 = ApplicationData.Current.LocalSettings.Values["SecFolderToken5"] as string;

        IList<string> downloadFileTypes = new List<string> { ".crdownload", ".part" };

        IList<object> GlobalClickedItems = null;

        private ObservableCollection<FileItem> _filteredFileMetadataList;
        private ObservableCollection<FileItem> _filteredPinnedFileMetadataList;
        public ObservableCollection<FileItem> fileMetadataListCopy;

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

        
        
        public MainWindow()
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBarRectangle);

            if (string.IsNullOrEmpty(folderToken) && string.IsNullOrEmpty(pinnedFolderToken))
            {
                RegularAndPinnedFileGrid.Visibility = Visibility.Collapsed;
                disableButtonVisibility();
                launchOnboarding();
            }

            loadSettings();

            if (!string.IsNullOrEmpty(folderToken)) { enableButtonVisibility(); obtainFolderAndFiles("regular"); setFolderPath("Regular"); }
            if (!string.IsNullOrEmpty(pinnedFolderToken)) { setFolderPath("Pin"); }
            else if (string.IsNullOrEmpty(pinnedFolderToken) && !string.IsNullOrEmpty(folderToken)) { NoPinnedFolderStackpanel.Visibility = Visibility.Visible; }
        }

        private async void loadSettings()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values.ContainsKey("LoadSimpleViewBoolean"))
            {
                UseSimpleViewByDefaultToggle.IsOn = (bool)localSettings.Values["LoadSimpleViewBoolean"];
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
            }
            else
            {
                PrimaryPortalFolderCheckBox.Content = "Set New...";
                showPrimPortal = false;
            }

            if (!string.IsNullOrEmpty(secondaryFolderToken1))
            {
                StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(secondaryFolderToken1);
                SecondaryPortalFolder1ChangeButton.Content = folder.Name;
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
            }
            else
            {
                SecondaryPortalFolder2ChangeButton.Content = "Set New...";
                showSecPortal2 = false;
            }

            if (!string.IsNullOrEmpty(secondaryFolderToken3))
            {
                StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(secondaryFolderToken3);
                SecondaryPortalFolder3ChangeButton.Content = folder.Name;
            }
            else
            {
                SecondaryPortalFolder3ChangeButton.Content = "Set New...";
                showSecPortal3 = false;
            }

            if (!string.IsNullOrEmpty(secondaryFolderToken4))
            {
                StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(secondaryFolderToken4);
                SecondaryPortalFolder4ChangeButton.Content = folder.Name;
            }
            else
            {
                SecondaryPortalFolder4ChangeButton.Content = "Set New...";
                showSecPortal4 = false;
            }

            if (!string.IsNullOrEmpty(secondaryFolderToken5))
            {
                StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(secondaryFolderToken5);
                SecondaryPortalFolder5ChangeButton.Content = folder.Name;
            }
            else
            {
                SecondaryPortalFolder5ChangeButton.Content = "Set New...";
                showSecPortal5 = false;
            }
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
                            if (OOBEgrid.Visibility == Visibility.Visible) obtainFolderAndFiles("regular");
                            RegularFolderPath = folder.Path;
                            PrimaryPortalFolderChangeButton.Content = folder.Name;
                            break;
                        case "pinned":
                            ApplicationData.Current.LocalSettings.Values["PinnedFolderToken"] = currentFolderToken;
                            pinnedFolderToken = currentFolderToken;
                            if (OOBEgrid.Visibility == Visibility.Collapsed) enableButtonVisibility();
                            obtainFolderAndFiles("pinned");
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
                }
                catch { }
            }
            else
            {
                //canceled operation, do nothing
            }
        }

        public async void obtainFolderAndFiles(string source)
        {
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

            ObservableCollection<FileItem> fileMetadataList = new ObservableCollection<FileItem>();

            if (source == "regular") regularFileListView.ItemsSource = fileMetadataList;
            else if (source == "pinned") pinnedFileListView.ItemsSource = fileMetadataList;

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

                foreach (StorageFile file in files.Take(1024))
                {
                    if (source == "regular")
                    {
                        double percentageOfFiles = currentFile / totalFiles;
                        percentageOfFiles = percentageOfFiles * 100;
                        progress.Report(Convert.ToInt32(percentageOfFiles));
                        currentFile++;
                    }

                    BasicProperties basicProperties = await file.GetBasicPropertiesAsync();
                    StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 256);
                    BitmapImage bitmapThumbnail = new BitmapImage();
                    bitmapThumbnail.SetSource(thumbnail);

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

                    string modifiedDateFormatted = "n/a";
                    if (DateTime.Now.ToString("d") == basicProperties.DateModified.ToString("d")) modifiedDateFormatted = basicProperties.DateModified.ToString("t");
                    else modifiedDateFormatted = basicProperties.DateModified.ToString("g");

                    if (downloadFileTypes.Contains(file.FileType))
                    {

                        fileMetadataList.Add(new FileItem()
                        {
                            FileName = file.Name,
                            FileDisplayName = file.DisplayName,
                            FilePath = file.Path,
                            FileType = "This file is still being downloaded",
                            FileSize = "",
                            FileSizeSuffix = "",
                            ModifiedDate = "",
                            FileIcon = bitmapThumbnail,
                            IconOpacity = 0.25,
                            TextOpacity = 0.5,
                            ProgressActivity = true
                        });
                    }
                    else
                    {
                        fileMetadataList.Add(new FileItem()
                        {
                            FileName = file.Name,
                            FileDisplayName = file.DisplayName,
                            FilePath = file.Path,
                            FileType = file.DisplayType,
                            FileSize = filesizecalc.ToString(),
                            FileSizeSuffix = " " + generativefilesizesuffix,
                            ModifiedDate = modifiedDateFormatted,
                            FileIcon = bitmapThumbnail,
                            IconOpacity = 1,
                            TextOpacity = 1,
                            ProgressActivity = false
                        });
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
                }
            }
            else
            {
                // Ah bleh
            }
        }

        private void fileListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                GlobalClickedItems = e.AddedItems;
            }
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
                var selectedFile = e.Items[0] as FileItem;
                StorageFile file = await StorageFile.GetFileFromPathAsync(selectedFile.FilePath);

                // Create a list of storage items with the file
                var storageItems = new List<StorageFile> { file };

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

        private void noFolderpathTechingTip_ActionButtonClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            askForAccess("regular");
        }

        private void QuickSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            applySettingsToMenu();
            if (!quickSettingsFlyoutTeachingTip.IsOpen) quickSettingsFlyoutTeachingTip.IsOpen = true;
            else quickSettingsFlyoutTeachingTip.IsOpen = false;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            obtainFolderAndFiles("regular");
            obtainFolderAndFiles("pinned");
        }

        private void Query_ContentsChanged(IStorageQueryResultBase sender, object args)
        {
            obtainFolderAndFiles("regular");
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

            await Task.Delay(1000);

            var newMainWindow = new MainWindow();
            newMainWindow.Show();
            this.Close();
        }

        private async void RevealInExplorerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);
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
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "Drop to add to pinned files";
        }

        private async void PinnedPivotSection_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    var storageFile = items[0] as StorageFile; StorageFolder storageFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(pinnedFolderToken);
                    StorageFile copiedFile = await storageFile.CopyAsync(storageFolder, storageFile.Name, NameCollisionOption.GenerateUniqueName);
                    pinnedFileListView.Items.Remove(0);
                    obtainFolderAndFiles("pinned");
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
            if (OOBEpivot.SelectedIndex == OOBEpivot.Items.Count - 1)
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
                    localSettings.Values["LoadSimpleViewBoolean"] = false;
                }
                else if (OOBEsimpleViewOfferCheckBox.IsChecked == true)
                {
                    localSettings.Values["LoadSimpleViewBoolean"] = true;

                    var simpleWindow = new SimpleMode();
                    simpleWindow.Activate();

                    this.Close();
                }
                localSettings.Values["KeepSimpleModeRunning"] = OOBEsimpleViewOfferCheckBox.IsChecked;
            }
            else
            {
                nextOOBEpage();
            }

        }

        private void nextOOBEpage()
        {
            if (OOBEpivot.SelectedIndex == 1 & OOBEsetupPivot.SelectedIndex < OOBEsetupPivot.Items.Count - 1) OOBEsetupPivot.SelectedIndex = OOBEsetupPivot.SelectedIndex + 1;
            else OOBEpivot.SelectedIndex = OOBEpivot.SelectedIndex + 1;
        }

        private void OOBEpivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OOBEpivot.SelectedIndex == 0) OOBEgoBackButton.IsEnabled = false;
            else OOBEgoBackButton.IsEnabled = true;

            OOBEgoNextButton.IsEnabled = checkIfNextButtonShouldBeEnabled();

            if (OOBEpivot.SelectedIndex == OOBEpivot.Items.Count - 1) OOBEgoNextButton.Content = "Finish setup!";
            else OOBEgoNextButton.Content = "Next ›";
        }

        private void OOBEsetupPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OOBEgoNextButton.IsEnabled = checkIfNextButtonShouldBeEnabled();
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

        private void UseSimpleViewByDefaultToggle_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            localSettings.Values["LoadSimpleViewBoolean"] = UseSimpleViewByDefaultToggle.IsOn;
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
                regularFilePusher.Visibility = Visibility.Visible;
                SearchTextBox.Focus(FocusState.Programmatic);
            }
            else if (SearchGrid.Opacity == 1)
            {
                SearchGrid.Opacity = 0;
                SearchGrid.Translation = new Vector3(0, -60, 0);
                SearchGrid.Visibility = Visibility.Collapsed;
                RegularAndPinnedFileGrid.Translation = new Vector3(0, 0, 0);
                regularFilePusher.Visibility = Visibility.Collapsed;
            }
            SearchTextBox.Text = "";
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            filterListView(SearchTextBox.Text);
        }

        private void filterListView(string filter)
        {
            var filteredItems = _filteredFileMetadataList.Where(item => item.FileName.ToLower().Contains(filter) || item.FilePath.ToLower().Contains(filter) || item.FileType.ToLower().Contains(filter));
            regularFileListView.ItemsSource = new ObservableCollection<FileItem>(filteredItems);

            var filteredPinnedItems = _filteredPinnedFileMetadataList.Where(item => item.FileName.ToLower().Contains(filter) || item.FilePath.ToLower().Contains(filter) || item.FileType.ToLower().Contains(filter));
            pinnedFileListView.ItemsSource = new ObservableCollection<FileItem>(filteredPinnedItems);
        }

        private void regularFileListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            openLastSelectedFile();
        }

        private void pinnedFileListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            openLastSelectedFile();
        }

        private async void CopyLastSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (GlobalClickedItems == null) copyMostRecentFile();

            else
            {
                // get the selected file item
                FileItem selectedFile = (FileItem)GlobalClickedItems[0];

                await Task.Run(async () =>
                {
                    // get the folder path
                    StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);
                    string folderPath = folder.Path;

                    // construct the full file path
                    string filePath = Path.Combine(folderPath, selectedFile.FileName);

                    // get the file
                    StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);

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
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            openLastSelectedFile();
        }

        private async void openLastSelectedFile()
        {
            if (GlobalClickedItems != null)
            {
                FileItem selectedFile = (FileItem)GlobalClickedItems[0];

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

            if (!isWindowsHelloRequiredForPins && !string.IsNullOrEmpty(pinnedFolderToken)) { obtainFolderAndFiles("pinned"); pinnedFileGrid.Visibility = Visibility.Visible; }
            else if (isWindowsHelloRequiredForPins)
            {
                pinnedFileGrid.Visibility = Visibility.Collapsed;
                WinHelloProgressRing.IsActive = true;
                WinHelloWaitingIndicator.Visibility = Visibility.Visible;
                bool isVerified = await VerifyUserWithWindowsHelloAsync("You need to verify with Windows Hello™️ to access your pinned files.");

                if (isVerified)
                {
                    pinnedFileGrid.Visibility = Visibility.Visible;
                    if (!string.IsNullOrEmpty(pinnedFolderToken)) obtainFolderAndFiles("pinned");
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

        private void quickSettingsFlyoutTeachingTip_Closed(Microsoft.UI.Xaml.Controls.TeachingTip sender, Microsoft.UI.Xaml.Controls.TeachingTipClosedEventArgs args)
        {
            ExpanderSettingsExpander.IsExpanded = false;
            SecondaryFolderSettingsExpander.IsExpanded = false;
            if (isWindowsHelloRequiredForPins) setPinBarOptionVisibility(false);
            else if (!isWindowsHelloRequiredForPins) setPinBarOptionVisibility(true);
        }

        private void PickPinnedFolderHyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            askForAccess("pinned");
        }

        private void AboutDropStackButton_Click(object sender, RoutedEventArgs e)
        {
            AboutDropStackGrid.Visibility = Visibility.Visible;
            AboutDropStackGrid.Opacity = 1;
            AboutDropStackContentGrid.Translation = new Vector3(0, 0, 0);
        }

        private async void AboutDropStackCloseButton_Click(object sender, RoutedEventArgs e)
        {
            AboutDropStackGrid.Opacity = 0;
            AboutDropStackContentGrid.Translation = new Vector3(0, 50, 0);
            await Task.Delay(500);
            AboutDropStackGrid.Visibility = Visibility.Collapsed;
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
            obtainFolderAndFiles("regular");
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
                PinnedExpanderBackgroundRectangle.Opacity = 0.3;
                ContentBackgroundRectangle.Opacity = 0.35;
            }
            else
            {
                PinnedExpanderBackgroundRectangle.Opacity = 0.1;
                ContentBackgroundRectangle.Opacity = 0.15;
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
    }
}