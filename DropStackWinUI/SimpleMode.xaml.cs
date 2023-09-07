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

        IList<string> downloadFileTypes = new List<string> { ".crdownload", ".part" };
        IList<string> documentFileTypes = new List<string> { ".pdf", ".doc", ".docx", ".txt", ".html", ".htm", ".xls", ".xlsx", ".odt", ".fodt", ".ods", ".fods", ".rtf", ".xml" };
        IList<string> pictureFileTypes = new List<string> { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".svg", ".ico", ".webp", ".raw", ".psd", ".ai" };
        IList<string> musicFileTypes = new List<string> { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma", ".m4a", ".mid", ".amr", ".aiff", ".ape" };
        IList<string> videoFileTypes = new List<string> { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm", ".3gp", ".m4v", ".mpeg", ".mpg", ".rm", ".vob" };
        IList<string> applicationFileTypes = new List<string> { ".exe", ".dmg", ".app", ".deb", ".apk", ".msi", ".msix", ".rpm", ".jar", ".bat", ".sh", ".com", ".vb", ".gadget", ".ipa" };
        IList<string> presentationFileTypes = new List<string> { ".ppt", ".pptx", ".key", ".odp" };
        IList<object> GlobalClickedItems = null;
        ObservableCollection<FileItem> _filteredFileMetadataList = new ObservableCollection<FileItem>();
        bool isLoading = true;
        bool isSorting = false;
        bool isRefreshRequested = false;
        bool isPinsRefreshRequested = false;
        bool isPinsOnScreen = false;

        int loadedItemsSimple = 250;
        int loadedThumbnails = 250;
        int thumbnailResolution = 64;

        int checkboxBehavior = 1;

        public SimpleMode()
        {
            this.InitializeComponent();
            
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(DragZone);
            loadSettings();
            loadFromCache();

            this.Activated += OnWindowActivated;

            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Nearest);

            uint dpi = GetDpiForWindow(hWnd);
            double scaleFactor = (double)dpi / 96;

            int displayWidth = displayArea.WorkArea.Width;
            int displayHeight = displayArea.WorkArea.Height;

            int windowWidth = 400;
            int windowHeight = 700;

            if (displayHeight < 900*scaleFactor) windowHeight = (int)Math.Round(displayHeight * 0.9 /scaleFactor , 0);

            this.MoveAndResize(((displayWidth / 2) - ((windowWidth*scaleFactor)/2)), (displayHeight - (windowHeight * scaleFactor) - (10 * scaleFactor)), windowWidth, windowHeight);

            EverythingGrid.Translation = new Vector3(0,0,0);
            EverythingGrid.Opacity = 1;
        }

        private void OnWindowActivated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == WindowActivationState.Deactivated)
            {
                closeWithAnimation();
            }
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
                loadedItemsSimple = (int)localSettings.Values["NormalLoadedItems"];
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
        }

        public async void obtainFolderAndFiles(string source, ObservableCollection<FileItem> cachedItems)
        {
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

            ObservableCollection<FileItem> fileMetadataList = new ObservableCollection<FileItem>();
            if (cachedItems != null) fileMetadataList = cachedItems;

            regularFileListView.ItemsSource = fileMetadataList;

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
                
                foreach (StorageFile file in files.Take(loadedItemsSimple))
                {
                    BitmapImage bitmapThumbnail = new BitmapImage();
                    BasicProperties basicProperties = await file.GetBasicPropertiesAsync();

                    if (currentFile < (loadedThumbnails + 1))
                    {
                        StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, Convert.ToUInt32(thumbnailResolution));
                        bitmapThumbnail.SetSource(thumbnail);
                    }
                    currentFile++;

                    double filesizecalc = Convert.ToDouble(basicProperties.Size); //size in byte
                    string generativefilesizesuffix = "B"; //default file suffix

                    if (filesizecalc >= 1000 && filesizecalc < 1000000)
                    {
                        filesizecalc = Convert.ToDouble(basicProperties.Size) / 1000; //convert to kb
                        filesizecalc = Math.Round(filesizecalc, 0);
                        generativefilesizesuffix = "KB";
                    }

                    else if (filesizecalc >= 1000000 && filesizecalc < 1000000000)
                    {
                        filesizecalc = Convert.ToDouble(basicProperties.Size) / 1000000; //convert to mb
                        filesizecalc = Math.Round(filesizecalc, 1);
                        generativefilesizesuffix = "MB";
                    }

                    else if (filesizecalc >= 1000000000)
                    {
                        filesizecalc = Convert.ToDouble(basicProperties.Size) / 1000000000; //convert to gb
                        filesizecalc = Math.Round(filesizecalc, 2);
                        generativefilesizesuffix = "GB";
                    }

                    else if (filesizecalc >= 1000000000000)
                    {
                        filesizecalc = Convert.ToDouble(basicProperties.Size) / 1000000000000; //convert to gb
                        filesizecalc = Math.Round(filesizecalc, 3);
                        generativefilesizesuffix = "TB";
                    }

                    string modifiedDateFormatted = "n/a";
                    if (DateTime.Now.ToString("d") == basicProperties.DateModified.ToString("d")) modifiedDateFormatted = basicProperties.DateModified.ToString("t");
                    else modifiedDateFormatted = basicProperties.DateModified.ToString("g");

                    string typeTag = "";
                    string typeDisplayName = file.FileType;

                    if (documentFileTypes.Contains(file.FileType.ToLower()))
                    {
                        typeTag = "docs";
                        typeDisplayName = "Document (" + file.FileType + ")";
                    }
                    else if (pictureFileTypes.Contains(file.FileType.ToLower()))
                    {
                        typeTag = "pics";
                        typeDisplayName = "Picture (" + file.FileType + ")";
                    }
                    else if (musicFileTypes.Contains(file.FileType.ToLower()))
                    {
                        typeTag = "music";
                        typeDisplayName = "Music (" + file.FileType + ")";
                    }
                    else if (videoFileTypes.Contains(file.FileType.ToLower()))
                    {
                        typeTag = "vids";
                        typeDisplayName = "Video (" + file.FileType + ")";
                    }
                    else if (applicationFileTypes.Contains(file.FileType.ToLower()))
                    {
                        typeTag = "apps";
                        typeDisplayName = "Application (" + file.FileType + ")";
                    }
                    else if (presentationFileTypes.Contains(file.FileType.ToLower()))
                    {
                        typeTag = "pres";
                        typeDisplayName = "Presentation (" + file.FileType + ")";
                    }

                    bool shouldAdd = true;
                    if (cachedItems != null)
                    {
                        for (int i = 0; i < fileMetadataList.Count; i++)
                        {
                            FileItem fileItem = fileMetadataList.ElementAt(i);
                            if (fileItem.FilePath == file.Path)
                            {
                                shouldAdd = false;
                                int indexToReplace = fileMetadataList.IndexOf(fileItem);
                                FileItem newFileItem = new FileItem();

                                if (downloadFileTypes.Contains(file.FileType))
                                {
                                    newFileItem = new FileItem()
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
                                        PillOpacity = 0,
                                        TextOpacity = 0.5,
                                        ProgressActivity = true,
                                    };
                                }
                                else
                                {
                                    newFileItem = new FileItem()
                                    {
                                        FileName = file.Name,
                                        FileDisplayName = file.DisplayName,
                                        FilePath = file.Path,
                                        FileType = file.DisplayType,
                                        TypeTag = typeTag,
                                        FileSize = filesizecalc.ToString(),
                                        FileSizeSuffix = " " + generativefilesizesuffix,
                                        ModifiedDate = modifiedDateFormatted,
                                        FileIcon = bitmapThumbnail,
                                        IconOpacity = 1,
                                        PillOpacity = 0.25,
                                        TextOpacity = 1,
                                        ProgressActivity = false,
                                    };
                                }

                                fileMetadataList.RemoveAt(indexToReplace);
                                fileMetadataList.Insert(indexToReplace, newFileItem);

                            }
                        }
                    }

                    if (shouldAdd)
                    {
                        if (downloadFileTypes.Contains(file.FileType))
                        {
                            fileMetadataList.Insert(addIndex, new FileItem()
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
                                PillOpacity = 0,
                                TextOpacity = 0.5,
                                ProgressActivity = true
                            });
                        }
                        else
                        {
                            fileMetadataList.Insert(addIndex, new FileItem()
                            {
                                FileName = file.Name,
                                FileDisplayName = file.DisplayName,
                                FilePath = file.Path,
                                FileType = file.DisplayType,
                                TypeTag = typeTag,
                                FileSize = filesizecalc.ToString(),
                                FileSizeSuffix = " " + generativefilesizesuffix,
                                ModifiedDate = modifiedDateFormatted,
                                FileIcon = bitmapThumbnail,
                                IconOpacity = 1,
                                PillOpacity = 0.25,
                                TextOpacity = 1,
                                ProgressActivity = false
                            });
                        }
                        addIndex++;
                    }

                }
                if (source == "regular")
                {
                    if (cachedItems != null)
                    {
                        //check if all the files still exist, else remove them
                        foreach (FileItem item in cachedItems)
                        {
                            if (!System.IO.File.Exists(item.FilePath)) cachedItems.Remove(item);
                        }
                    }
                    _filteredFileMetadataList = fileMetadataList;
                }
                saveToCache(source, fileMetadataList);

            }
            else
            {
                SomethingWentWrongTeachingTip.IsOpen = true;
            }
            isLoading = false;
        }

        private void fileListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GlobalClickedItems = regularFileListView.SelectedItems;
        }

        private async void fileListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
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
                await Task.Delay(100);
                Clipboard.Flush();
            });
            closeWithAnimation();
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
                    closeWithAnimation();
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
            if (sortTag == "all" && !isLoading) filterListView("");
            else if (sortTag != "all") filterListView(sortTag);

            if (sortTag == "pins") obtainFolderAndFiles("pinned", null);

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
            e.DragUIOverride.Caption = "Drop to add to pinned files";
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
            if (subjectToCache.Count > loadedItemsSimple) foreach (FileItem item in subjectToCache) { if (subjectToCache.IndexOf(item) > loadedItemsSimple) subjectToCache.Remove(item); }
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

        private async void loadFromCache()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await localFolder.GetFileAsync("cachedfiles.xml");
            if (file == null) obtainFolderAndFiles("regular", null);
            else
            {
                string xmlContent = await FileIO.ReadTextAsync(file);

                // Deserialize the XML string into ArrayOfFileItem
                XmlSerializer serializer = new XmlSerializer(typeof(ArrayOfFileItem));
                using (TextReader reader = new StringReader(xmlContent))
                {
                    ArrayOfFileItem arrayOfFileItem = (ArrayOfFileItem)serializer.Deserialize(reader);

                    // Create an ObservableCollection from the deserialized data
                    ObservableCollection<FileItem> cachedFileMetadataList = new ObservableCollection<FileItem>(arrayOfFileItem.Items);

                    regularFileListView.ItemsSource = cachedFileMetadataList;

                    //check for new files
                    obtainFolderAndFiles("regular", cachedFileMetadataList);
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
                    int convertibleSlateMode = (int)explorerKey.GetValue("ConvertibleSlateMode", 1);
                    if (convertibleSlateMode == 0) regularFileListView.SelectionMode = ListViewSelectionMode.Multiple;
                    else regularFileListView.SelectionMode = ListViewSelectionMode.Extended;
                    SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
                    break;
                case 3:
                    regularFileListView.SelectionMode = ListViewSelectionMode.Multiple;
                    break;
            }

            // legend:
            // 0: never show checkboxes
            // 1: show checkboxes according to file explorer setting
            // 2: show checkboxes when device is used as a tablet
            // 3: always show checkboxes
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (checkboxBehavior == 2)
            {
                if (e.Category == UserPreferenceCategory.General)
                {
                    RegistryKey explorerKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced");
                    int convertibleSlateMode = (int)explorerKey.GetValue("ConvertibleSlateMode", 1);
                    if (convertibleSlateMode == 0) regularFileListView.SelectionMode = ListViewSelectionMode.Multiple;
                    else regularFileListView.SelectionMode = ListViewSelectionMode.Extended;
                }
            }
        }
    }
}