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

namespace DropStackWinUI
{
    public sealed partial class SimpleMode : WinUIEx.WindowEx
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

        public SimpleMode()
        {
            this.InitializeComponent();
            
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(DragZone);
            loadSettings();
            obtainFolderAndFiles("regular");

            this.Activated += OnWindowActivated;

            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Nearest);

            int displayWidth = displayArea.WorkArea.Width;
            int displayHeight = displayArea.WorkArea.Height;

            int windowWidth = 400;
            int windowHeight = 700;

            if (displayHeight < 900) windowHeight = (int)Math.Round(displayHeight * 0.9,0);

            this.MoveAndResize((displayWidth / 2) - (windowWidth/2), displayHeight - (windowHeight + 10), windowWidth, windowHeight);

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
        }

        public async void obtainFolderAndFiles(string source)
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

            regularFileListView.ItemsSource = fileMetadataList;

            // Sort the files by modification date in descending order
            files = files.OrderByDescending(f => f.DateCreated).ToList();

            //check for user's way to note the date and time
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            string shortDatePattern = currentCulture.DateTimeFormat.ShortDatePattern;
            string shortTimePattern = currentCulture.DateTimeFormat.ShortTimePattern;

            if (folder != null)
            {
                foreach (StorageFile file in files.Take(256))
                {
                    BasicProperties basicProperties = await file.GetBasicPropertiesAsync();
                    StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 256);
                    BitmapImage bitmapThumbnail = new BitmapImage();
                    bitmapThumbnail.SetSource(thumbnail);

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
                            TextOpacityDate = 0,
                            PillOpacity = 0,
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
                            FileType = typeDisplayName,
                            TypeTag = typeTag,
                            FileSize = filesizecalc.ToString(),
                            FileSizeSuffix = " " + generativefilesizesuffix,
                            ModifiedDate = modifiedDateFormatted,
                            FileIcon = bitmapThumbnail,
                            IconOpacity = 1,
                            TextOpacity = 1,
                            TextOpacityDate = 0.5,
                            PillOpacity = 0.25,
                            ProgressActivity = false
                        });
                    }
                    if (source == "regular") _filteredFileMetadataList = fileMetadataList;
                }
            }
            else
            {
                SomethingWentWrongTeachingTip.IsOpen = true;
            }
            isLoading = false;
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
                var selectedFile = e.Items[0] as FileItem;
                var folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);
                if (isPinsOnScreen) folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(pinnedFolderToken);
                var file = await folder.GetFileAsync(selectedFile.FileName);

                // Create a list of storage items with the file
                var storageItems = new List<StorageFile> { file };

                // Set the data package on the event args using SetData
                e.Data.SetData(StandardDataFormats.StorageItems, storageItems);

                minimizeWithAnimation();
            }
            catch { }
        }

        private void regularFileListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            openLastSelectedFile();
        }

        private async void openLastSelectedFile()
        {
            FileItem selectedFile = (FileItem)GlobalClickedItems[0];
            string selectedFileName = selectedFile.FileName;
            StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);

            try
            {
                // get the file
                var file = await folder.GetFileAsync(selectedFileName);

                // launch the file
                var success = await Launcher.LaunchFileAsync(file);
            }

            catch
            {
                // handle the exception. ehm.
            }
            finally
            {
                closeWithAnimation();
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
            this.Close();
        }

        private async void minimizeWithAnimation()
        {
            EverythingGrid.Opacity = 0;
            EverythingGrid.Translation = new Vector3(0, 20, 0);
            await Task.Delay(200);
            var window = this;
            window.Minimize();
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

            if (sortTag == "pins") obtainFolderAndFiles("pinned");

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
                obtainFolderAndFiles("pinned");
                isPinsRefreshRequested = false;
            }
            else
            {
                if (isRefreshRequested)
                {
                    obtainFolderAndFiles("regular");
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

            this.Close();
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
                    if (isPinsOnScreen) obtainFolderAndFiles("pinned");
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
    }
}