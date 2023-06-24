using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace DropStack
{   
    public sealed partial class SimplePage : Page
    {
        string folderToken = ApplicationData.Current.LocalSettings.Values["FolderToken"] as string;
        int simplePageLoadedItemsAmount = 10;

        public SimplePage()
        {
            this.InitializeComponent();

            this.Loaded += CompactOverlayPage_Loaded;

            if (string.IsNullOrEmpty(folderToken)) noAccessHandler();
            else { obtainFolderAndFiles(); createListener(); }
        }

        private async void CompactOverlayPage_Loaded(object sender, RoutedEventArgs e)
        {
            var preferences = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
            preferences.CustomSize = new Windows.Foundation.Size(300, 500);
            await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, preferences);

            //hide title bar NO DO NOT DO THAT RESTORING IT WILL CAUSE A BUG RELATED TO THE TITLE BAR COLOR
            //CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
        }

        public void noAccessHandler()
        {
            switchToNormalView();
        }

        private void LoadMoreFromSimpleViewButton_Click(object sender, RoutedEventArgs e)
        {
            switchToNormalView();
        }

        private async void switchToNormalView()
        {
            await CoreApplication.RequestRestartAsync("forceNormalView");
        }

        public async void obtainFolderAndFiles()
        {
            // Get the folder from the access token
            folderToken = ApplicationData.Current.LocalSettings.Values["FolderToken"] as string;
            StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);
            int itemsNumber = simplePageLoadedItemsAmount;

            // Access the selected folder
            IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
            ObservableCollection<object> fileMetadataList = new ObservableCollection<object>();

            simpleFileListView.ItemsSource = fileMetadataList;

            // Sort the files by modification date in descending order
            files = files.OrderByDescending(f => f.DateCreated).ToList();

            //check for user's way to note the date and time
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            string shortDatePattern = currentCulture.DateTimeFormat.ShortDatePattern;
            string shortTimePattern = currentCulture.DateTimeFormat.ShortTimePattern;

            if (folder != null)
            {
                foreach (StorageFile file in files.Take(itemsNumber))
                {
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

                    fileMetadataList.Add(new FileItem()
                    {
                        FileName = file.Name,
                        FilePath = file.Path,
                        FileType = file.DisplayType,
                        FileSize = filesizecalc.ToString(),
                        FileSizeSuffix = " "+generativefilesizesuffix,
                        ModifiedDate = modifiedDateFormatted,
                        FileIcon = bitmapThumbnail,
                    });

                }
            }
            else
            {
                // Ah bleh
            }
            LoadMoreFromSimpleViewButton.Visibility = Visibility.Visible;
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
                    simpleFileListView.SelectedItem = null;
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

        private void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            obtainFolderAndFiles();
        }
    }
}
