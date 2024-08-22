using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Ozora;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DropStackWinUI.AnimThemes
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Colorful : Page
    {
        OzoraEngine ozoraEngine;

        public Colorful()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            this.Unloaded += PhysicsCloudsSimulation_Unloaded;

            // make sure to wait for the CloudsGrid to be loaded so it has an actual width that can be used to compute the cloud distribution
            CloudsGrid.Loaded += CloudsGrid_Loaded;
            MouseViewModel.Instance.PropertyChanged += Instance_PropertyChanged;
        }

        private void PhysicsCloudsSimulation_Unloaded(object sender, RoutedEventArgs e)
        {
            ozoraEngine.Physics.MouseCursorEngaged = false;
            ozoraEngine.Physics.InterruptSimulation();
            ozoraEngine = null;
            Debug.WriteLine("Unloaded Ozora Cloud Simulation Model");
        }

        private void CloudsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Loading clouds");
            loadClouds();
        }

        private void loadClouds()
        {
            CloudsGrid.Children.Clear();

            // the initializer is used to generate clouds, it doesn't need to be passed into the actual engine
            Ozora.Initializer initializer = new();
            Ozora.CloudSettings settings = new()
            {
                AreaWidth = CloudsGrid.ActualWidth,
                AreaHeight = CloudsGrid.ActualHeight,
                ImageWidth = 50,
                ImageHeight = 50,
                DensityModifier = 5
            };
            System.Numerics.Vector3[] _vectorsList = initializer.GenerateCloudPositions(settings);


            Random rnd = new Random();
            foreach (System.Numerics.Vector3 position in _vectorsList)
            {
                Microsoft.UI.Xaml.Shapes.Rectangle cloud = new();
                cloud.Height = 50;
                cloud.Width = 50;
                cloud.RadiusX = 25;
                cloud.RadiusY = 25;
                cloud.HorizontalAlignment = HorizontalAlignment.Left;
                cloud.VerticalAlignment = VerticalAlignment.Top;
                cloud.Translation = position;
                cloud.CenterPoint = new System.Numerics.Vector3(50, 50, 0);

                Random _random = new();
                byte r = (byte)_random.Next(256);
                byte g = (byte)_random.Next(256);
                byte b = (byte)_random.Next(256);
                cloud.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, r, g, b));

                CloudsGrid.Children.Add(cloud);
                cloud.Opacity = 0.5;
            }


            ozoraEngine = new OzoraEngine();

            OzoraSettings ozoraSettings = new OzoraSettings()
            {
                SimulationStyle = SimulationStyle.Clouds,
                FrameRate = 60,
            };

            ozoraEngine.Physics.Interface = new OzoraInterface()
            {
                AreaDimensions = new Windows.Foundation.Point(CloudsGrid.ActualWidth, CloudsGrid.ActualHeight),
                CloudGrid = CloudsGrid,
                UIDispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread(),
                Settings = ozoraSettings
            };

            /// Very important to set this property, otherwise, the physics simulation will not engage!
            /// Equally important to call the method below. This allows the simulation to disengage while it 
            /// is still active to avoid having to restart, though this is mainly used for the sun simulation.
            ozoraEngine.Physics.MouseCursorEngaged = true;
            ozoraEngine.Physics.StartSimulation();
        }

        private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ozoraEngine != null) ozoraEngine.Physics.Interface.PointerLocation = MouseViewModel.Instance.MousePosition;
        }

        private void CloudsGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            loadClouds();
        }
    }
}
