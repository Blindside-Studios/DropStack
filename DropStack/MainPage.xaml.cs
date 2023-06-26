using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using Windows.UI.ViewManagement;
using Windows.Foundation;

namespace DropStack
{
    /// <summary>
    /// Code behind the main UI thread
    /// </summary>

    public class FileItem
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public string FileSize { get; set; }
        public string FileSizeSuffix { get; set; }
        public string ModifiedDate { get; set; }
        public BitmapImage FileIcon { get; set; }
    }

    public sealed partial class MainPage : Page
    {
        string folderToken = ApplicationData.Current.LocalSettings.Values["FolderToken"] as string;
        string pinnedFolderToken = ApplicationData.Current.LocalSettings.Values["PinnedFolderToken"] as string;
        int defaultPage = 0;
        int loadedItemsNumber = 1024;

        public MainPage()
        {
            this.InitializeComponent();

            //CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = false;

            OOBEgoNextButton.Focus(FocusState.Keyboard);

            if (string.IsNullOrEmpty(folderToken) & string.IsNullOrEmpty(pinnedFolderToken))
            {
                PivotViewSwitcher.Visibility = Visibility.Collapsed;
                disableButtonVisibility();
                launchOnboarding();
            }

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("LoadSimpleViewBoolean"))
            {
                if ((bool)localSettings.Values["LoadSimpleViewBoolean"] == true) UseSimpleViewByDefaultToggle.IsOn = true;
            }
            SimpleViewRelauncherButton.Visibility = Visibility.Collapsed;
            if (localSettings.Values.ContainsKey("AlwaysShowToolbarInSimpleModeBoolean"))
            {
                if ((bool)localSettings.Values["AlwaysShowToolbarInSimpleModeBoolean"] == true) PinToolbarInSimpleModeToggleSwitch.IsOn = true;
            }

            PivotViewSwitcher.SelectedIndex = defaultPage;

            if (string.IsNullOrEmpty(folderToken) && !string.IsNullOrEmpty(pinnedFolderToken)) PivotViewSwitcher.SelectedIndex = 1;
            else if (!string.IsNullOrEmpty(folderToken)) { enableButtonVisibility(); obtainFolderAndFiles(); createListener(); }
        }

        private void PivotViewSwitcher_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PivotViewSwitcher.SelectedIndex == 0)
            {
                ConnectionDisruptorDisplay.Text = "Folder Portal";
                noPinnedFolderpathTechingTip.IsOpen = false;
                if (regularFileListView.Items.Count == 0 && !string.IsNullOrEmpty(folderToken)) obtainFolderAndFiles();
                if (string.IsNullOrEmpty(folderToken) && string.IsNullOrEmpty(pinnedFolderToken) && OOBEgrid.Visibility == Visibility.Collapsed) { noAccessHandler(); disableButtonVisibility(); }
                else if (string.IsNullOrEmpty(folderToken) && string.IsNullOrEmpty(pinnedFolderToken))
                {
                    noAccessHandler();
                    disableButtonVisibility();
                    noFolderpathTechingTip.IsOpen = false;
                    noPinnedFolderpathTechingTip.IsOpen = false;
                }
                else enableButtonVisibility();
            }

            else if (PivotViewSwitcher.SelectedIndex == 1)
            {
                pinnedFolderToken = ApplicationData.Current.LocalSettings.Values["PinnedFolderToken"] as string;
                noFolderpathTechingTip.IsOpen = false;
                if (string.IsNullOrEmpty(pinnedFolderToken)) { noPinnedFolderpathTechingTip.IsOpen = true; disableButtonVisibility(); }
                else
                {
                    if (pinnedFileListView.Items.Count < 1)
                    {
                        { enableButtonVisibility(); obtainPinnedFiles(); }
                    }
                }
                ConnectionDisruptorDisplay.Text = "Pinned Folder Portal (your pinned files will persists and only the link to the app will be removed)";
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

        public async void askForAccess()
        {
            // Close the teaching tip
            noFolderpathTechingTip.IsOpen = false;

            // Create a new instance of the folder picker
            FolderPicker folderPicker = new FolderPicker();

            // Configure the folder picker
            folderPicker.FileTypeFilter.Add("*");
            folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;

            // Show the folder picker and wait for the user's response
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();

            // Request access to the selected folder
            try { folderToken = StorageApplicationPermissions.FutureAccessList.Add(folder); 
                nextOOBEpage();

                // Save the folder access token to local settings
                ApplicationData.Current.LocalSettings.Values["FolderToken"] = folderToken;

                if (OOBEgrid.Visibility == Visibility.Collapsed) enableButtonVisibility();
                createListener();
                obtainFolderAndFiles();
            }
            catch { } //user cancelled the folder picker, requires rework, currently, the app still crashes

            //add acccess to recent files to jumplist (currently under construction)
            if (!string.IsNullOrEmpty(folderToken))
            {
                JumpList jumpList = await JumpList.LoadCurrentAsync();
                jumpList.Items.Clear(); }/*
                jumpList.Items.Add(JumpListItem.CreateWithArguments("copyRecentFile", "Copy recent file"));
                await jumpList.SaveAsync();
            }
            */
        }

        public async void obtainFolderAndFiles()
        {
            // Get the folder from the access token
            folderToken = ApplicationData.Current.LocalSettings.Values["FolderToken"] as string;
            StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);

            // Access the selected folder
            IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
            ObservableCollection<object> fileMetadataList = new ObservableCollection<object>();

            regularFileListView.ItemsSource = fileMetadataList;

            // Sort the files by modification date in descending order
            files = files.OrderByDescending(f => f.DateCreated).ToList();

            //check for user's way to note the date and time
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            string shortDatePattern = currentCulture.DateTimeFormat.ShortDatePattern;
            string shortTimePattern = currentCulture.DateTimeFormat.ShortTimePattern;

            if (folder != null)
            {
                foreach (StorageFile file in files.Take(loadedItemsNumber))
                {
                    BasicProperties basicProperties = await file.GetBasicPropertiesAsync();

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


                    StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 256);
                    BitmapImage bitmapThumbnail = new BitmapImage();
                    bitmapThumbnail.SetSource(thumbnail);

                    string modifiedDateFormatted = "n/a";
                    if (DateTime.Now.ToString("d") == basicProperties.DateModified.ToString("d")) modifiedDateFormatted = basicProperties.DateModified.ToString("t");
                    else modifiedDateFormatted = basicProperties.DateModified.ToString("g");

                    fileMetadataList.Add(new FileItem()
                    {
                        FileName = file.Name,
                        FilePath = file.Path,
                        FileType = file.DisplayType,
                        FileSize = filesizecalc.ToString(),
                        FileSizeSuffix = " " + generativefilesizesuffix,
                        ModifiedDate = modifiedDateFormatted,
                        FileIcon = bitmapThumbnail,
                    });

                }
            }
            else
            {
                // Ah bleh
            }
        }

        private async void fileListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                FileItem selectedFile = (FileItem)e.AddedItems[0];
                string selectedFileName = selectedFile.FileName;

                // get the folder path
                StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);
                string folderPath = folder.Path;

                // construct the full file path
                string filePath = Path.Combine(folderPath, selectedFileName);

                try
                {
                    // get the file
                    var file = await folder.GetFileAsync(selectedFileName);

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
                }
            }
        }

        private async void fileListView_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            // get the selected file item
            FileItem selectedFile = (FileItem)((FrameworkElement)e.OriginalSource).DataContext;

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
                var folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);
                var file = await folder.GetFileAsync(selectedFile.FileName);

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
            askForAccess();
        }

        private void QuickSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!quickSettingsFlyoutTeachingTip.IsOpen) quickSettingsFlyoutTeachingTip.IsOpen = true;
            else quickSettingsFlyoutTeachingTip.IsOpen = false;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (PivotViewSwitcher.SelectedIndex == 0)
            {
                obtainFolderAndFiles();
            }

            if (PivotViewSwitcher.SelectedIndex == 1)
            {
                obtainPinnedFiles();
            }
        }

        private async void createListener()
        {
            try
            {
                //obtain the folder path
                StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);
                string folderPath = folder.Path;

                // create a new FileSystemWatcher object to monitor the directory
                var watcher = new FileSystemWatcher
                {
                    Path = folderPath,
                    Filter = "*", // monitor all files
                    NotifyFilter = NotifyFilters.FileName, // only notify on file name changes
                    EnableRaisingEvents = true // start watching the directory
                };

                // add an event handler to respond to file system changes
                watcher.Created += async (sender, e) =>
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        obtainFolderAndFiles();
                    });
                };
            }
            catch { }; //failed to create listener
        }

        private async void disconnectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (PivotViewSwitcher.SelectedIndex == 0) ApplicationData.Current.LocalSettings.Values["FolderToken"] = null;
            if (PivotViewSwitcher.SelectedIndex == 1) ApplicationData.Current.LocalSettings.Values["PinnedFolderToken"] = null;
            if (PivotViewSwitcher.SelectedIndex == 2) ApplicationData.Current.LocalSettings.Values["FolderToken"] = null;
            var result = await CoreApplication.RequestRestartAsync("");
        }

        private async void RevealInExplorerButton_Click(object sender, RoutedEventArgs e)
        {
            //obtain the folder
            StorageFolder folder;
            if (PivotViewSwitcher.SelectedIndex == 0) try
                {
                    folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);
                    await Launcher.LaunchFolderAsync(folder);
                }
                catch { cannotOpenPinnedFolderBecauseThereIsNoneTeachingTip.IsOpen = true; }

            if (PivotViewSwitcher.SelectedIndex == 1) try
                {
                    folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(pinnedFolderToken);
                    await Launcher.LaunchFolderAsync(folder);
                }
                catch { cannotOpenPinnedFolderBecauseThereIsNoneTeachingTip.IsOpen = true; }

            //open the folder
        }

        private async void pinnedFileListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                FileItem selectedFile = (FileItem)e.AddedItems[0];
                string selectedFileName = selectedFile.FileName;

                // get the folder path
                StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(pinnedFolderToken);
                string folderPath = folder.Path;

                // construct the full file path
                string filePath = Path.Combine(folderPath, selectedFileName);

                try
                {
                    // get the file
                    var file = await folder.GetFileAsync(selectedFileName);

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
                }
            }
        }

        private async void pinnedFileListView_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            // get the selected file item
            FileItem selectedFile = (FileItem)((FrameworkElement)e.OriginalSource).DataContext;

            // get the folder path
            StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(pinnedFolderToken);
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

        private async void pinnedFileListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            try
            {
                var selectedFile = e.Items[0] as FileItem;
                var folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(pinnedFolderToken);
                var file = await folder.GetFileAsync(selectedFile.FileName);

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

        private void noPinnedFolderpathTechingTip_ActionButtonClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            PickPinnedFolder();
        }

        private async void PickPinnedFolder()
        {
            // Create a new instance of the folder picker
            FolderPicker folderPicker = new FolderPicker();

            // Configure the folder picker
            folderPicker.FileTypeFilter.Add("*");
            folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;

            // Show the folder picker and wait for the user's response
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();

            // Request access to the selected folder
            try { folderToken = StorageApplicationPermissions.FutureAccessList.Add(folder); 
                nextOOBEpage();
                
                // Save the folder access token to local settings
                ApplicationData.Current.LocalSettings.Values["PinnedFolderToken"] = folderToken;

                noPinnedFolderpathTechingTip.IsOpen = false;
                obtainPinnedFiles();
                if (OOBEgrid.Visibility == Visibility.Collapsed) enableButtonVisibility();
            }
            catch { } //user canceled the folder picker

            
        }

        private async void obtainPinnedFiles()
        {
            pinnedFolderToken = ApplicationData.Current.LocalSettings.Values["PinnedFolderToken"] as string;
            StorageFolder pinnedFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(pinnedFolderToken);
            if (pinnedFolder != null)
            {
                // Access the selected folder
                IReadOnlyList<StorageFile> pinnedFiles = await pinnedFolder.GetFilesAsync();
                ObservableCollection<object> pinnedFileMetadataList = new ObservableCollection<object>();

                pinnedFileListView.ItemsSource = pinnedFileMetadataList;

                // Sort the files by modification date in descending order
                pinnedFiles = pinnedFiles.OrderByDescending(f => f.DateCreated).ToList();

                foreach (StorageFile pinnedFile in pinnedFiles)
                {
                    BasicProperties basicProperties = await pinnedFile.GetBasicPropertiesAsync();
                    StorageItemThumbnail thumbnail = await pinnedFile.GetThumbnailAsync(ThumbnailMode.SingleItem, 256);
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

                    pinnedFileMetadataList.Add(new FileItem()
                    {
                        FileName = pinnedFile.Name,
                        FilePath = pinnedFile.Path,
                        FileType = pinnedFile.DisplayType,
                        FileSize = filesizecalc.ToString(),
                        FileSizeSuffix = " "+generativefilesizesuffix,
                        ModifiedDate = modifiedDateFormatted,
                        FileIcon = bitmapThumbnail,
                    }) ;
                }

                pinnedFileListView.ItemsSource = pinnedFileMetadataList;


            }
            else
            {
                //there was an error fetching the pinned files
            }
        }

        private void cannotOpenPinnedFolderBecauseThereIsNoneTeachingTip_ActionButtonClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            PickPinnedFolder();
        }

        private void cannotOpenRegularFolderBecauseThereIsNoneTeachingTip_ActionButtonClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            askForAccess();
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
                    obtainPinnedFiles();
                }
            }
        }

        private void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            if (PivotViewSwitcher.SelectedIndex == 0)
            {
                obtainFolderAndFiles();
            }

            if (PivotViewSwitcher.SelectedIndex == 1)
            {
                obtainPinnedFiles();
            }
        }

        public void launchOnboarding()
        {
            ApplicationView.PreferredLaunchViewSize = new Size(500, 850);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;
            OOBEgrid.Visibility = Visibility.Visible;
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
                OOBEgrid.Opacity = 0;
                OOBEgrid.Translation = new Vector3(0, 100, 0);
                PivotViewSwitcher.SelectedIndex = defaultPage;
                PivotViewSwitcher.Visibility = Visibility.Visible;
                if (string.IsNullOrEmpty(folderToken) && string.IsNullOrEmpty(pinnedFolderToken)) noAccessHandler();
                else if (string.IsNullOrEmpty(folderToken)) PivotViewSwitcher.SelectedIndex = 1;
                else enableButtonVisibility();
                await Task.Delay(1000);
                OOBEgrid.Visibility = Visibility.Collapsed;
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

            if (OOBEpivot.SelectedIndex == OOBEpivot.Items.Count - 1) OOBEgoNextButton.Content = "Finish setup!";
            else OOBEgoNextButton.Content = "Next ›";
        }

        private void OOBEportalFileAccessRequestButton_Click(object sender, RoutedEventArgs e)
        {
            askForAccess();
        }

        private void OOBEpinnedFileAccessRequestButton_Click(object sender, RoutedEventArgs e)
        {
            PickPinnedFolder();
        }

        private void LoadMoreFromSimpleViewButton_Click(object sender, RoutedEventArgs e)
        {
            PivotViewSwitcher.SelectedIndex = 0;
        }

        private void UseSimpleViewByDefaultToggle_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            SimpleViewRelauncherButton.Visibility = Visibility.Visible;

            localSettings.Values["LoadSimpleViewBoolean"] = UseSimpleViewByDefaultToggle.IsOn;
        }

        private async void LaunchSimpleModeButton_Click(object sender, RoutedEventArgs e)
        {
            await CoreApplication.RequestRestartAsync("forceSimpleView");
        }

        private async void CopyRecentFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the folder from the access token
            string folderToken = ApplicationData.Current.LocalSettings.Values["FolderToken"] as string;
            folderToken = ApplicationData.Current.LocalSettings.Values["FolderToken"] as string;
            StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);

            // Access the selected folder
            IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();

            // Sort the files by modification date in descending order
            files = files.OrderByDescending(f => f.DateCreated).ToList();

            StorageFile recentFile = files[0];

            // create a new data package
            var dataPackage = new DataPackage();

            // add the file to the data package
            dataPackage.SetStorageItems(new List<IStorageItem> { recentFile });

            // copy the data package to the clipboard
            Clipboard.SetContent(dataPackage);

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

        private async void SimpleViewRelauncherButton_Click(object sender, RoutedEventArgs e)
        {
            SimpleViewRelauncherButton.Visibility = Visibility.Collapsed;
            await CoreApplication.RequestRestartAsync("");
        }

        private void PinToolbarInSimpleModeToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["AlwaysShowToolbarInSimpleModeBoolean"] = PinToolbarInSimpleModeToggleSwitch.IsOn;
        }
    }
}