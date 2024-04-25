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
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.AccessCache;
using Windows.Storage;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using Windows.Storage.FileProperties;
using Windows.System;
using WinUIEx;
using Windows.ApplicationModel.Core;
using System.Formats.Asn1;
using System.ComponentModel.Design;
using Windows.Management.Deployment;
using Microsoft.UI.Windowing;
using System.Numerics;
using Windows.UI.Core;
using System.Diagnostics;
using Windows.Storage.Search;
using System.Runtime.InteropServices;
using Windows.Devices.PointOfService.Provider;
using Windows.ApplicationModel.VoiceCommands;
using System.Xml.Serialization;
using Microsoft.Win32;
using DropStackWinUI.FileViews;

namespace DropStackWinUI
{
    public sealed partial class SimpleMode : WinUIEx.WindowEx
    {
        [DllImport("User32.dll")]
        public static extern uint GetDpiForWindow(IntPtr hwnd);

        string folderToken = ApplicationData.Current.LocalSettings.Values["FolderToken"] as string;
        string pinnedFolderToken = ApplicationData.Current.LocalSettings.Values["PinnedFolderToken"] as string;

        string secondaryFolderToken1 = ApplicationData.Current.LocalSettings.Values["SecFolderToken1"] as string;
        string secondaryFolderToken2 = ApplicationData.Current.LocalSettings.Values["SecFolderToken2"] as string;
        string secondaryFolderToken3 = ApplicationData.Current.LocalSettings.Values["SecFolderToken3"] as string;
        string secondaryFolderToken4 = ApplicationData.Current.LocalSettings.Values["SecFolderToken4"] as string;
        string secondaryFolderToken5 = ApplicationData.Current.LocalSettings.Values["SecFolderToken5"] as string;

        bool showPrimPortal = true;
        bool showSecPortal1 = false;
        bool showSecPortal2 = false;
        bool showSecPortal3 = false;
        bool showSecPortal4 = false;
        bool showSecPortal5 = false;
        IList<object> GlobalClickedItems = null;
        FileItem previewedItem = null;
        ObservableCollection<FileItem> _filteredFileMetadataList = new ObservableCollection<FileItem>();
        bool isLoading = true;
        bool isSorting = false;
        bool isRefreshRequested = false;
        bool isPinsRefreshRequested = false;
        bool isPinsOnScreen = false;

        int loadedItems = 250;
        int loadedThumbnails = 250;
        int thumbnailResolution = 64;

        int checkboxBehavior = 1;
        bool isWindowsHelloRequiredForPins = false;
        bool isPinsHidden = false;
        string priorSortTag = null;
        bool openCompactCommandBar = false;

        bool enableFreeWindowing = false;

        public SimpleMode()
        {
            this.InitializeComponent();
            
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(DragZone);
            loadSettings();
            loadFromCache("regular");

            this.Activated += OnWindowActivated;

            if (!enableFreeWindowing)
            {
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
                var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Nearest);

                uint dpi = GetDpiForWindow(hWnd);
                double scaleFactor = (double)dpi / 96;

                int displayWidth = displayArea.WorkArea.Width;
                int displayHeight = displayArea.WorkArea.Height;

                int windowWidth = 400;
                int windowHeight = 700;

                if (displayHeight < 900 * scaleFactor) windowHeight = (int)Math.Round(displayHeight * 0.9 / scaleFactor, 0);

                this.MoveAndResize(((displayWidth / 2) - ((windowWidth * scaleFactor) / 2)), (displayHeight - (windowHeight * scaleFactor) - (10 * scaleFactor)), windowWidth, windowHeight);
            }

            EverythingGrid.Translation = new Vector3(0,0,0);
            EverythingGrid.Opacity = 1;
        }

        public string getText(string key)
        {
            Windows.ApplicationModel.Resources.ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();
            return loader.GetString(key);
        }

        private void OnWindowActivated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == WindowActivationState.Deactivated)
            {
                 if (!enableFreeWindowing) closeWithAnimation();
            }
            else if (!isPinsOnScreen && !isSorting) obtainFolderAndFiles("regular", regularFileListView.ItemsSource as ObservableCollection<FileItem>);
        }

        private void loadSettings()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values.ContainsKey("FreeWindowingInSimpleMode"))
            {
                if ((bool)localSettings.Values["FreeWindowingInSimpleMode"] == true)
                {
                    enableFreeWindowing = true;
                    IsAlwaysOnTop = false;
                    WindowStyleGrid.Visibility = Visibility.Visible;
                    FlyoutStyleGrid.Visibility = Visibility.Collapsed;
                    SetTitleBar(TitleBarRectangle);
                    IsTitleBarVisible = true;
                    IsShownInSwitchers = true;
                    IsResizable = true;
                    AltLaunchNormalModeButton.Visibility = Visibility.Visible;
                    AllFilesToggleButton.Margin = new Thickness(5, 0, 0, 0);
                    ToggleButtonStackPanel.Margin = new Thickness(0, -40, 0, 0);
                    regularFileListView.Margin = new Thickness(0, -20, 0, 0);
                }
            }

            if (localSettings.Values.ContainsKey("showPrimaryPortal")) { showPrimPortal = (bool)localSettings.Values["showPrimaryPortal"]; }
            if (localSettings.Values.ContainsKey("showSecondaryPortal1")) { showSecPortal1 = (bool)localSettings.Values["showSecondaryPortal1"]; }
            if (localSettings.Values.ContainsKey("showSecondaryPortal2")) { showSecPortal2 = (bool)localSettings.Values["showSecondaryPortal2"]; }
            if (localSettings.Values.ContainsKey("showSecondaryPortal3")) { showSecPortal3 = (bool)localSettings.Values["showSecondaryPortal3"]; }
            if (localSettings.Values.ContainsKey("showSecondaryPortal4")) { showSecPortal4 = (bool)localSettings.Values["showSecondaryPortal4"]; }
            if (localSettings.Values.ContainsKey("showSecondaryPortal5")) { showSecPortal5 = (bool)localSettings.Values["showSecondaryPortal5"]; }

            if (string.IsNullOrEmpty(folderToken)) showPrimPortal = false;
            if (string.IsNullOrEmpty(secondaryFolderToken1)) showSecPortal1 = false;
            if (string.IsNullOrEmpty(secondaryFolderToken2)) showSecPortal2 = false;
            if (string.IsNullOrEmpty(secondaryFolderToken3)) showSecPortal3 = false;
            if (string.IsNullOrEmpty(secondaryFolderToken4)) showSecPortal4 = false;
            if (string.IsNullOrEmpty(secondaryFolderToken5)) showSecPortal5 = false;

            if (localSettings.Values.ContainsKey("SelectedTheme"))
            {
                string selectedTheme = (string)localSettings.Values["SelectedTheme"];
                setTheme(selectedTheme);
            }
            else
            {
                string selectedTheme = "Default";
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
                checkboxBehavior = (int)localSettings.Values["CheckboxBehavior"];
            }
            adjustCheckboxBehavior();

            if (localSettings.Values.ContainsKey("PinBarBehavior"))
            {
                isWindowsHelloRequiredForPins = (int)localSettings.Values["PinBarBehavior"] == 3;
                if (isWindowsHelloRequiredForPins)
                {
                    FlyoutPinUnpinButton.IsEnabled = false;
                }
            }
            if (localSettings.Values.ContainsKey("PinBarBehavior"))
            {
                isPinsHidden = (int)localSettings.Values["PinBarBehavior"] == 4;
            }

            if (isWindowsHelloRequiredForPins)
            {
                PinnedFilesToggleButton.IsEnabled = false;
                ToolTip toolTip = new ToolTip();
                toolTip.Content = getText("PinnedFilesSimpleModeDisabled");
                ToolTipService.SetToolTip(PinnedFilesToggleButton, toolTip);
            }
            else if (isPinsHidden) PinnedFilesToggleButton.Visibility = Visibility.Collapsed;
        }

        public async void obtainFolderAndFiles(string source, ObservableCollection<FileItem> cachedItems)
        {
            loadSettings();

            ObservableCollection<FileItem> fileMetadataList = new ObservableCollection<FileItem>();

            if (source == "regular")
            {
                if (cachedItems == null) regularFileListView.ItemsSource = fileMetadataList;
                else
                {
                    _filteredFileMetadataList = cachedItems;
                }
            }
            else if (source == "pinned")
            {
                if (cachedItems == null) regularFileListView.ItemsSource = fileMetadataList;
            }

            isLoading = true;

            // Get the folder from the access token
            folderToken = ApplicationData.Current.LocalSettings.Values["FolderToken"] as string;
            StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);
            FolderDisplay.Text = folder.Name;
            if (source == "pinned")
            {
                folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync
                    (ApplicationData.Current.LocalSettings.Values["PinnedFolderToken"] as string);
            }

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
                int currentFile = 1;
                int addIndex = 0;
                bool shouldContinue = true;

                foreach (StorageFile file in files.Take(loadedItems))
                {
                    if (shouldContinue)
                    {
                        BitmapImage bitmapThumbnail = new BitmapImage();
                        BasicProperties basicProperties = await file.GetBasicPropertiesAsync();

                        if (currentFile < (loadedThumbnails + 1))
                        {
                            StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, Convert.ToUInt32(thumbnailResolution));
                            bitmapThumbnail.SetSource(thumbnail);
                        }
                        currentFile++;

                        int filesizecalc = Convert.ToInt32(basicProperties.Size); //size in byte
                        string generativefilesizesuffix = getText("FileSizeUnitsB"); //default file suffix

                        if (filesizecalc >= 1000 && filesizecalc < 1000000)
                        {
                            filesizecalc = Convert.ToInt32(basicProperties.Size) / 1000; //convert to kb
                            generativefilesizesuffix = getText("FileSizeUnitsKB");
                        }

                        else if (filesizecalc >= 1000000 && filesizecalc < 1000000000)
                        {
                            filesizecalc = Convert.ToInt32(basicProperties.Size) / 1000000; //convert to mb
                            generativefilesizesuffix = getText("FileSizeUnitsMB");
                        }

                        else if (filesizecalc >= 1000000000)
                        {
                            filesizecalc = Convert.ToInt32(basicProperties.Size) / 1000000000; //convert to gb
                            generativefilesizesuffix = getText("FileSizeUnitsGB");
                        }

                        string typeTag = "";
                        string typeDisplayName = file.FileType;

                        if (FileTags.DocumentFileTypes.Contains(file.FileType.ToLower()))
                        {
                            typeTag = "docs";
                            typeDisplayName = getText("FileTypeDoc") + " (" + file.FileType + ")";
                        }
                        else if (FileTags.PictureFileTypes.Contains(file.FileType.ToLower()))
                        {
                            typeTag = "pics";
                            typeDisplayName = getText("FileTypePic") + " (" + file.FileType + ")";
                        }
                        else if (FileTags.MusicFileTypes.Contains(file.FileType.ToLower()))
                        {
                            typeTag = "music";
                            typeDisplayName = getText("FileTypeMusic") + " (" + file.FileType + ")";
                        }
                        else if (FileTags.VideoFileTypes.Contains(file.FileType.ToLower()))
                        {
                            typeTag = "vids";
                            typeDisplayName = getText("FileTypeVideo") + " (" + file.FileType + ")";
                        }
                        else if (FileTags.ApplicationFileTypes.Contains(file.FileType.ToLower()))
                        {
                            typeTag = "apps";
                            typeDisplayName = getText("FileTypeApp") + " (" + file.FileType + ")";
                        }
                        else if (FileTags.PresentationFileTypes.Contains(file.FileType.ToLower()))
                        {
                            typeTag = "pres";
                            typeDisplayName = getText("FileTypePresentation") + " (" + file.FileType + ")";
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
                                    FileType = getText("FileTypeDownloadNotify"),
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
                            ObservableCollection<FileItem> collection = regularFileListView.ItemsSource as ObservableCollection<FileItem>;
                            regularFileListView.ItemsSource = collection;
                            int insertIndex = 0;
                            if (fileMetadataList != null)
                            {
                                foreach (FileItem item in fileMetadataList)
                                {
                                    collection.Insert(insertIndex, item);
                                    insertIndex++;
                                }
                            }
                            saveToCache(source, collection);
                            break;
                        }
                        addIndex++;
                    }
                }
                //check if all the files still exist, else remove them, then save to cache
                if (cachedItems != null && source == "regular")
                {
                    var itemsCollection = regularFileListView.ItemsSource as ObservableCollection<FileItem>;
                    regularFileListView.ItemsSource = itemsCollection;
                    for (int i = 0; i < itemsCollection.Count; i++)
                    {
                        FileItem item = itemsCollection.ElementAt(i);
                        if (!System.IO.File.Exists(item.FilePath)) { itemsCollection.Remove(item); }
                    }
                }

                if (cachedItems == null)
                {
                    ObservableCollection<FileItem> cachingCollection = new();
                    foreach (FileItem item in regularFileListView.Items) cachingCollection.Add(item);
                    saveToCache(source, cachingCollection);
                }
            }
            else
            {
                SomethingWentWrongTeachingTip.IsOpen = true;
            }
            isLoading = false;
        }

        public async void loadInThumbnails(string target)
        {
            //load thumbnails
            List<FileThumbnail> thumbnails = new();
            int currentFile = 1;
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
                catch { break; }
            }
        }

        private void fileListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GlobalClickedItems = regularFileListView.SelectedItems;
        }

        private void fileListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
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

            if (GlobalClickedItems.Count != 1) FlyoutRevealButton.IsEnabled = false;
            else if (GlobalClickedItems.Count ==1) FlyoutRevealButton.IsEnabled = true;

            adjustIsPreviewButtonShown();
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
                hideWithAnimation();
            }
            catch { }
        }

        private async void regularFileListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
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
                    if (!enableFreeWindowing) closeWithAnimation();
                }
            }
        }

        private void CloseSimpleModeButton_Click(object sender, RoutedEventArgs e)
        {
            closeWithAnimation();
        }

        private async void closeWithAnimation()
        {
            EverythingGrid.Opacity = 0;
            EverythingGrid.Translation = new Vector3(0, 20, 0);
            await Task.Delay(200);
            var window = this;
            window.Close();
        }

        private async void hideWithAnimation()
        {
            EverythingGrid.Opacity = 0;
            EverythingGrid.Translation = new Vector3(0, 20, 0);
            await Task.Delay(200);
            var window = this;
            window.MoveAndResize(20000, 20000, 0, 0);
        }

        private void SimpleModeMeatballMenu_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Activate();

            this.Close();
        }

        private async void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            string sortTag = toggleButton.Tag.ToString();
            if (sortTag == "all" && !isLoading) regularFileListView.ItemsSource = _filteredFileMetadataList;
            else if (sortTag != "all") { filterListView(sortTag); isLoading = false; }

            if (sortTag == "pins") loadFromCache("pinned");
            if (sortTag == "all" && priorSortTag == "all") obtainFolderAndFiles("regular", null);

            isPinsOnScreen = ((ToggleButton)sender == PinnedFilesToggleButton);

            isSorting = true;
            foreach (ToggleButton listedToggleButton in FilterButtonsStackPanel.Children)
            {
                if (listedToggleButton.Tag.ToString() != toggleButton.Tag.ToString()) listedToggleButton.IsChecked = false;
            }
            isSorting = false;

            try
            {
                folderToken = ApplicationData.Current.LocalSettings.Values["FolderToken"] as string;
                StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);
                if ((ToggleButton)sender == PinnedFilesToggleButton)
                {
                    folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(pinnedFolderToken);
                }
                FolderDisplay.Text = folder.Name;
            }
            catch { SomethingWentWrongTeachingTip.IsOpen = true; }

            isLoading = false;
            priorSortTag = sortTag;
        }

        private void AllFilesToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isSorting)
            {
                if ((ToggleButton)sender == AllFilesToggleButton) isRefreshRequested = true;
                if ((ToggleButton)sender == PinnedFilesToggleButton) isPinsRefreshRequested = true;

                foreach (ToggleButton listedToggleButton in FilterButtonsStackPanel.Children)
                {
                    listedToggleButton.IsChecked = false;
                }
                if (isPinsRefreshRequested) PinnedFilesToggleButton.IsChecked = true;
                else AllFilesToggleButton.IsChecked = true;
            }
        }

        private void filterListView(string filter)
        {
            if (!string.IsNullOrEmpty(filter) && filter != "pins")
            {
                ObservableCollection<FileItem> _filteredItemsCollection = new ObservableCollection<FileItem>
                    (_filteredFileMetadataList.Where(item => item.TypeTag.ToLower().Contains(filter)));
                regularFileListView.ItemsSource = _filteredItemsCollection;
            }
            else if (!string.IsNullOrEmpty(filter) && filter == "pins")
            {
                obtainFolderAndFiles("pinned", null);
                isPinsRefreshRequested = false;
            }
            else
            {
                if (isRefreshRequested)
                {
                    obtainFolderAndFiles("regular", null);
                    isRefreshRequested = false;
                }
                else
                {
                    ObservableCollection<FileItem> _filteredItemsCollection = _filteredFileMetadataList;
                    regularFileListView.ItemsSource = _filteredItemsCollection;
                }
            }
        }

        private async void AllFilesToggleButton_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            try
            {
                StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);
                await Launcher.LaunchFolderAsync(folder);
            }
            catch { }
        }

        private async void PinnedFilesToggleButton_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            try
            {
                StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(pinnedFolderToken);
                await Launcher.LaunchFolderAsync(folder);
            }
            catch { }
        }

        private void TeachingTip_ActionButtonClick(TeachingTip sender, object args)
        {
            SomethingWentWrongTeachingTip.IsOpen = false;
            
            var mainWindow = new MainWindow();
            mainWindow.Activate();

            Close();
        }

        private void PinnedFilesToggleButton_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = getText("PinnedFilesDragOverAvailable");
        }

        private async void PinnedFilesToggleButton_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    var storageFile = items[0] as StorageFile; StorageFolder storageFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(pinnedFolderToken);
                    StorageFile copiedFile = await storageFile.CopyAsync(storageFolder, storageFile.Name, NameCollisionOption.GenerateUniqueName);
                    if (isPinsOnScreen) obtainFolderAndFiles("pinned", null);
                }
            }
        }

        private void regularFileListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            Close();
        }

        private void setTheme(string themeName)
        {
            ParallaxImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/Themes/" + themeName + ".png"));
        }

        private async void saveToCache(string source, ObservableCollection<FileItem> subjectToCache)
        {
            if (subjectToCache != null)
            {
                while (subjectToCache.Count > loadedItems)
                {
                    subjectToCache.RemoveAt(loadedItems);
                }
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
                    cachedFileMetadataList = new();
                    try
                    {
                        ArrayOfFileItem arrayOfFileItem = (ArrayOfFileItem)serializer.Deserialize(reader);
                        // Create an ObservableCollection from the deserialized data
                        cachedFileMetadataList = new ObservableCollection<FileItem>(arrayOfFileItem.Items);
                    }
                    catch { }
                }

                if (cachedFileMetadataList.Count == 0)
                {
                    obtainFolderAndFiles(source, cachedFileMetadataList);
                }
                else
                {
                    if (source == "regular")
                    {
                        regularFileListView.ItemsSource = cachedFileMetadataList;
                    }
                    //check for new files
                    obtainFolderAndFiles(source, cachedFileMetadataList);
                    loadInThumbnails(source);
                }
            }
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
                    FlyoutRevealButton.IsEnabled = true;
                }
            }
            adjustIsPreviewButtonShown();
        }

        private async void FlyoutOpenButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < regularFileListView.SelectedItems.Count; i++) {
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

            if (!enableFreeWindowing) closeWithAnimation();
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
                Clipboard.Flush();
            });
            if (!enableFreeWindowing) closeWithAnimation();
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

        private async void PinUnpinButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)PinnedFilesToggleButton.IsChecked)
            {
                foreach (FileItem item in GlobalClickedItems)
                {
                    var storageFile = await StorageFile.GetFileFromPathAsync(item.FilePath);
                    StorageFolder storageFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(pinnedFolderToken);
                    StorageFile copiedFile = await storageFile.CopyAsync(storageFolder, storageFile.Name, NameCollisionOption.GenerateUniqueName);

                    PinnedFilesToggleButton.IsChecked = true;
                    obtainFolderAndFiles("pinned", null);
                }
            }
            else
            {
                foreach (FileItem item in GlobalClickedItems)
                {
                    ObservableCollection<FileItem> pinnedFileMetadataListCopy = regularFileListView.ItemsSource as ObservableCollection<FileItem>;
                    StorageFile file = await StorageFile.GetFileFromPathAsync(item.FilePath);
                    await file.DeleteAsync();
                    pinnedFileMetadataListCopy.Remove(item);
                    regularFileListView.ItemsSource = pinnedFileMetadataListCopy;
                }
                saveToCache("pinned", regularFileListView.ItemsSource as ObservableCollection<FileItem>);
            }
        }

        private void AltLaunchNormalModeButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Activate();

            this.Close();
        }

        private async void adjustIsPreviewButtonShown()
        {
            if (regularFileListView.SelectedItems.Count == 1)
            {
                FileItem fileItem = GlobalClickedItems.First() as FileItem;
                StorageFile file = await StorageFile.GetFileFromPathAsync(fileItem.FilePath);
                if (fileItem.TypeTag == "pics" || fileItem.TypeTag == "vids" || file.FileType == ".pdf")
                {
                    previewedItem = GlobalClickedItems[0] as FileItem;
                    FlyoutPreviewButton.IsEnabled = true;
                }
                else
                {
                    FlyoutPreviewButton.IsEnabled = false;
                }
            }
            else
            {
                FlyoutPreviewButton.IsEnabled = false;
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
    }
}