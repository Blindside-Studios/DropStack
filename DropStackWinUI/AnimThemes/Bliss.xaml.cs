using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Ozora;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public sealed partial class Bliss : Page
    {
        OzoraEngine Ozora = new OzoraEngine();

        public Bliss()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            this.Loaded += PhysicsSunSimulation_Loaded;
            this.Unloaded += PhysicsSunSimulation_Unloaded;
        }

        private void PhysicsSunSimulation_Unloaded(object sender, RoutedEventArgs e)
        {
            Ozora.Physics.MouseCursorEngaged = false;
            Ozora.Physics.InterruptSimulation();
            Ozora = null;
            //Debug.WriteLine("Unloaded Ozora Sun Simulation Model");
        }

        private void PhysicsSunSimulation_Loaded(object sender, RoutedEventArgs e)
        {
            Ozora = new OzoraEngine();

            OzoraSettings SunSettings = new OzoraSettings()
            {
                SimulationStyle = SimulationStyle.Sun,
                FrameRate = 60,
                MaxVectorDeltaPerFrame = 1.5f,
                RubberBandingModifier = 0.2f,
                EnableBorderCollision = true,
                EnableBounceOnCollision = true,
                BounceMomentumRetention = 0.8f,
                TrailingDragCoefficient = 0.01f,
                TrailingType = TrailingType.Vector
            };

            Ozora.Physics.Interface = new OzoraInterface()
            {
                ObjectWidth = (float)SunObject.ActualWidth,
                ObjectHeight = (float)SunObject.ActualHeight,
                Settings = SunSettings,
                AreaDimensions = new Windows.Foundation.Point(SunGrid.ActualWidth, SunGrid.ActualHeight)
            };

            Ozora.Physics.ObjectPositionCalculated += Physics_ObjectPositionCalculated;
            MouseViewModel.Instance.PropertyChanged += MouseViewModel_PropertyChanged;

            Ozora.Physics.StartSimulation();
            Ozora.Physics.MouseCursorEngaged = true;
        }

        private void MouseViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (Ozora != null)
            {
                Ozora.Physics.Interface.PointerLocation = MouseViewModel.Instance.MousePosition;
                Ozora.Physics.MouseCursorEngaged = MouseViewModel.Instance.MouseEngaged;
            }
        }

        private void Physics_ObjectPositionCalculated(object sender, ObjectPositionUpdatedEvent e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                this.SunObject.Translation = e.NewTranslationVector;
                if (SunObject.Translation.Y > SunGrid.ActualHeight * 0.33)
                {
                    double nightFactor = Math.Clamp((SunObject.Translation.Y - (SunGrid.ActualHeight * 0.33)) / (SunGrid.ActualHeight * 0.5), 0, 1);
                    double expoNightFactor = Math.Pow(2, nightFactor) - 1;
                    GroundDarkObject.Opacity = expoNightFactor;
                    NightSky.Opacity = expoNightFactor;
                    EveningSky.Opacity = Math.Clamp(nightFactor * 2, 0, 1);
                }
            });
        }

        private void SunGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            /// Null check as this event is fired when the page loads, 
            /// briefly before the code in the PageLoaded event is executed, 
            /// which initializes the Interface
            if (Ozora.Physics.Interface != null)
            {
                Ozora.Physics.Interface.AreaDimensions =
                    new Windows.Foundation.Point(SunGrid.ActualWidth, SunGrid.ActualHeight);
            }
        }
    }
}
