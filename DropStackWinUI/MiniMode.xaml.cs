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
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using System.Numerics;
using System.Xml.Serialization;
using System.Diagnostics;
using DropStackWinUI.FileViews;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DropStackWinUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MiniMode : WinUIEx.WindowEx
    {
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
        public ObservableCollection<FileItem> fileMetadataListCopy;

        int loadedItems = 250;
        int loadedThumbnails = 250;
        int thumbnailResolution = 64;

        bool isCommandBarPinned = false;
        bool openCompactCommandBar = false;

        bool isWindowsHelloRequiredForPins = false;

        public MiniMode()
        {
            this.InitializeComponent();
            this.Activated += OnWindowActivated;

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBarRectangle);
            loadSettings();

            if (string.IsNullOrEmpty(folderToken)) noAccessHandler();
            else { loadFromCache("regular"); }
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
                ContentBackgroundRectangle.Opacity = 0.15;

            }
            else if (!isDarkMode)
            {
                ContentBackgroundRectangle.Opacity = 0.5;
            }
        }

        private void OnWindowActivated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs e)
        {
            adjustDarkLightMode();
            obtainFolderAndFiles("regular", regularFileListView.ItemsSource as ObservableCollection<FileItem>);
        }

        private void loadSettings()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

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

            if (localSettings.Values.ContainsKey("AlwaysShowToolbarInSimpleModeBoolean"))
            {
                if ((bool)localSettings.Values["AlwaysShowToolbarInSimpleModeBoolean"] == true)
                {
                    CommandBarIndicatorPillHitbox.Visibility = Visibility.Collapsed;
                    ContentBackgroundRectangle.Margin = new Thickness(5, 0, 5, 50);
                    regularFileListView.Margin = new Thickness(0, 0, 0, 50);
                    isCommandBarPinned = true;
                }
            }
            if (!isCommandBarPinned)
            {
                FileCommandBar.Translation = new Vector3(0, 60, 0);
                CommandBarIndicatorPill.Opacity = 1;
            }

            if (localSettings.Values.ContainsKey("DefaultLaunchModeIndex"))
            {
                if ((int)localSettings.Values["DefaultLaunchModeIndex"] != 2)
                {
                    MakeSimpleDefaultButton.Visibility = Visibility.Visible;
                }
            }

            if (localSettings.Values.ContainsKey("PinBarBehavior"))
            {
                isWindowsHelloRequiredForPins = (int)localSettings.Values["PinBarBehavior"] == 3;
                if (isWindowsHelloRequiredForPins)
                {
                    FlyoutPinUnpinButton.IsEnabled = false;
                    FlyoutPinUnpinButtonSec.IsEnabled = false;
                }
            }
        }

        public void noAccessHandler()
        {
            switchToNormalView();
        }

        private void LoadMoreFromSimpleViewButton_Click(object sender, RoutedEventArgs e)
        {
            switchToNormalView();
        }

        private void switchToNormalView()
        {
            var mainWindow = new MainWindow();
            mainWindow.Activate();

            this.Close();
        }

        public async void obtainFolderAndFiles(string source, ObservableCollection<FileItem> cachedItems)
        {
            loadSettings();

            ObservableCollection<FileItem> fileMetadataList = new ObservableCollection<FileItem>();
            if (cachedItems == null) regularFileListView.ItemsSource = fileMetadataList;

            // Get the folder from the access token
            StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);

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
                if (totalFiles > 1024) totalFiles = 1024;
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
                            fileMetadataListCopy = fileMetadataList;
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
                if (cachedItems != null)
                {
                    for (int i = 0; i < fileMetadataList.Count; i++)
                    {
                        FileItem item = fileMetadataList.ElementAt(i);
                        if (!System.IO.File.Exists(item.FilePath)) { fileMetadataList.Remove(item); }
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
                // Ah bleh
            }
        }

        public async void loadInThumbnails()
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
            FileItem rightTappedItem = ((FrameworkElement)e.OriginalSource).DataContext as FileItem;

            GlobalClickedItems = new List<object>{ rightTappedItem };

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
            }
            catch { }
        }

        private void regularFileListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            regularFileListView.ItemsSource = fileMetadataListCopy;
            foreach (FileItem file in fileMetadataListCopy) { if (!System.IO.File.Exists(file.FilePath)) fileMetadataListCopy.Remove(file); }
            saveToCache("regular", fileMetadataListCopy);
        }

        private async void RevealInExplorerButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);
            await Launcher.LaunchFolderAsync(folder);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            obtainFolderAndFiles("regular", null);
        }

        private void HideToolbarButton_Click(object sender, RoutedEventArgs e)
        {
            if (isCommandBarPinned)
            {
                isCommandBarPinned = false;
                FileCommandBar.Translation = new Vector3(0, 60, 0);
                CommandBarIndicatorPill.Opacity = 1;
                CommandBarIndicatorPillHitbox.Visibility = Visibility.Visible;
                ContentBackgroundRectangle.Margin = new Thickness(5, 0, 5, 20);
                regularFileListView.Margin = new Thickness(0, 0, 0, 20);
                TitleBarRectangle.Height = 30;
                TitleBarText.Opacity = 1;
            }
            else
            {
                isCommandBarPinned = true;
                FileCommandBar.Translation = new Vector3(0, 0, 0);
                CommandBarIndicatorPill.Opacity = 0;
                CommandBarIndicatorPillHitbox.Visibility = Visibility.Collapsed;
                ContentBackgroundRectangle.Margin = new Thickness(5, 0, 5, 50);
                regularFileListView.Margin = new Thickness(0, 0, 0, 50);
                TitleBarRectangle.Height = 30;
                TitleBarText.Opacity = 1;
            }
        }

        private void CopyRecentFileButton_Click(object sender, RoutedEventArgs e)
        {
            copyMostRecentFile();
        }

        private void CommandBarIndicatorPill_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            CommandBarIndicatorPillHitbox.Visibility = Visibility.Collapsed;
            CommandBarIndicatorPill.Opacity = 0;
            FileCommandBar.Translation = new Vector3(0, 0, 0);
            ContentBackgroundRectangle.Translation = new Vector3(0, -25, 0);
            regularFileListView.Translation = new Vector3(0, -25, 0);
            CommandBarIndicatorPill.Translation = new Vector3(0, -40, 0);
            TitleBarRectangle.Height = 0;
            TitleBarText.Opacity = 0;
        }

        private void FileCommandBar_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (!isCommandBarPinned && !FileCommandBar.IsOpen)
            {
                CommandBarIndicatorPillHitbox.Visibility = Visibility.Visible;
                CommandBarIndicatorPill.Opacity = 1;
                FileCommandBar.Translation = new Vector3(0, 60, 0);
                ContentBackgroundRectangle.Translation = new Vector3(0, 0, 0);
                regularFileListView.Translation = new Vector3(0, 0, 0);
                CommandBarIndicatorPill.Translation = new Vector3(0, 0, 0);
                TitleBarRectangle.Height = 30;
                TitleBarText.Opacity = 1;
            }
        }

        private void FileCommandBar_Closing(object sender, object e)
        {
            if (!isCommandBarPinned)
            {
                CommandBarIndicatorPillHitbox.Visibility = Visibility.Visible;
                CommandBarIndicatorPill.Opacity = 1;
                FileCommandBar.Translation = new Vector3(0, 60, 0);
                ContentBackgroundRectangle.Translation = new Vector3(0, 0, 0);
                regularFileListView.Translation = new Vector3(0, 0, 0);
                CommandBarIndicatorPill.Translation = new Vector3(0, 0, 0);
                TitleBarRectangle.Height = 30;
                TitleBarText.Opacity = 1;
            }
        }

        private void FileCommandBar_Opening(object sender, object e)
        {
            if (isCommandBarPinned) HideToolbarButton.Content = "Hide toolbar";
            else HideToolbarButton.Content = "Pin toolbar";
        }

        private async void fileListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
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
            }
        }

        private async void CopyLastSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (GlobalClickedItems == null) copyMostRecentFile();

            else
            {
                await Task.Run(async () =>
                {
                    // get the selected file item
                    FileItem selectedFile = (FileItem)GlobalClickedItems[0];

                    // get the folder path
                    StorageFile file = await StorageFile.GetFileFromPathAsync(selectedFile.FilePath);

                    // create a new data package
                    var dataPackage = new DataPackage();

                    // add the file to the data package
                    dataPackage.SetStorageItems(new List<IStorageItem> { file });

                    // copy the data package to the clipboard
                    Clipboard.SetContent(dataPackage);
                    Clipboard.Flush();
                });
            }
        }

        private async void copyMostRecentFile()
        {
            await Task.Run(async () =>
            {
                FileItem latestItem = regularFileListView.Items[0] as FileItem;

                StorageFile file = await StorageFile.GetFileFromPathAsync(latestItem.FilePath);

                // create a new data package
                var dataPackage = new DataPackage();

                // add the file to the data package
                dataPackage.SetStorageItems(new List<IStorageItem> { file });

                // copy the data package to the clipboard
                Clipboard.SetContent(dataPackage);
                Clipboard.Flush();
            });
        }

        private void MakeSimpleDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["DefaultLaunchModeIndex"] = 2;
            MakeSimpleDefaultButton.Visibility = Visibility.Collapsed;
        }

        private void setTheme(string themeName)
        {
            ParallaxImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/Themes/" + themeName + ".png"));
        }

        private async void saveToCache(string source, ObservableCollection<FileItem> subjectToCache)
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
                        regularFileListView.ItemsSource = cachedFileMetadataList;
                    }
                    //check for new files
                    obtainFolderAndFiles(source, cachedFileMetadataList);
                    loadInThumbnails();
                }
            }
        }

        private async void regularFileListView_Tapped(object sender, TappedRoutedEventArgs e)
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
            adjustIsPreviewButtonShown();
        }

        private async void FlyoutOpenButton_Click(object sender, RoutedEventArgs e)
        {
            FileItem selectedFile = GlobalClickedItems[0] as FileItem;
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
        }

        private async void FlyoutRevealButton_Click(object sender, RoutedEventArgs e)
        {
            FileItem currentFile = GlobalClickedItems[0] as FileItem;
            StorageFile file = await StorageFile.GetFileFromPathAsync(currentFile.FilePath);
            StorageFolder folder = await file.GetParentAsync();
            await Launcher.LaunchFolderAsync(folder);
        }

        private async void PinUnpinButton_Click(object sender, RoutedEventArgs e)
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
        private async void adjustIsPreviewButtonShown()
        {
            if (regularFileListView.SelectedItems.Count == 1)
            {
                FileItem fileItem = GlobalClickedItems[0] as FileItem;
                StorageFile file = await StorageFile.GetFileFromPathAsync(fileItem.FilePath);
                if (fileItem.TypeTag == "pics" || fileItem.TypeTag == "vids" || file.FileType == ".pdf")
                {
                    previewedItem = GlobalClickedItems[0] as FileItem;
                    FlyoutPreviewButton.IsEnabled = true;
                    FlyoutPreviewButtonSec.IsEnabled = true;
                }
                else
                {
                    FlyoutPreviewButton.IsEnabled = false;
                    FlyoutPreviewButtonSec.IsEnabled = false;
                }
            }
            else
            {
                FlyoutPreviewButton.IsEnabled = false;
                FlyoutPreviewButtonSec.IsEnabled = false;
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
