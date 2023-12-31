﻿using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DropStack
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public static AppWindow CompactOverlayWindow { get; set; }
        
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            // Create an AppWindow object for CompactOverlay
            CompactOverlayWindow = await AppWindow.TryCreateAsync();
            // Attach a Closed event handler to set it to null when closed
            CompactOverlayWindow.Closed += (s, args) => CompactOverlayWindow = null;
            // Attach a Closed event handler to set it to null when closed
            CompactOverlayWindow.Closed += (s, args) => CompactOverlayWindow = null;
            // Handle the case when the user closes the main window while CompactOverlay is still open
            CoreApplication.Exiting += async (s, args) =>
            {
                if (CompactOverlayWindow != null)
                {
                    await App.CompactOverlayWindow.CloseAsync();
                }
            };

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            bool shouldBeSimpleView = false;
            if (localSettings.Values.ContainsKey("LoadSimpleViewBoolean"))
            {
                if ((bool)localSettings.Values["LoadSimpleViewBoolean"] == false) shouldBeSimpleView = false;
                else if ((bool)localSettings.Values["LoadSimpleViewBoolean"] == true) shouldBeSimpleView = true;
            }

            //override according to launch arguments
            if (e.Arguments == "forceNormalView") shouldBeSimpleView = false;
            else if (e.Arguments == "forceSimpleView") shouldBeSimpleView = true;

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter

                    if (shouldBeSimpleView)
                    {
                        rootFrame.Navigate(typeof(SimplePage), ApplicationViewMode.CompactOverlay);
                    }

                    else rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
