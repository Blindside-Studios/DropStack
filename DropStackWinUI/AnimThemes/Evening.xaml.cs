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
    public sealed partial class Evening : Page
    {
        bool _finishedLoading = false;

        ParulAI parulAI;
        public Evening()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            this.Loaded += BirdsSimulation_Loaded;
            this.Unloaded += BirdsSimulation_Unloaded;
        }

        private void BirdsSimulation_Unloaded(object sender, RoutedEventArgs e)
        {
            parulAI.StopSpawningBirds();
            parulAI = null;
            RootGrid.Children.Clear();

            // important cleanup to prevent ghost birds from being cached and flickering when reloading the page
            CurrentBirdSimulation.Instance.CleanUp();

            Debug.WriteLine("Unloaded ParulAI Model");
        }

        private void BirdsSimulation_Loaded(object sender, RoutedEventArgs e)
        {
            parulAI = new ParulAI();

            CurrentBirdSimulation.Instance.RestingSpots = new List<RestingSpot>();
            CurrentBirdSimulation.Instance.RestingSpots.Add(new RestingSpot() { Position = new System.Numerics.Vector3((float)RootGrid.ActualWidth / 1714 * 275    , (float)RootGrid.ActualHeight / 3002 * 1070 - 70, 0), IsOccupied = false }); // top left
            CurrentBirdSimulation.Instance.RestingSpots.Add(new RestingSpot() { Position = new System.Numerics.Vector3((float)RootGrid.ActualWidth / 1714 * 1000   , (float)RootGrid.ActualHeight / 3002 * 1266 - 70, 0), IsOccupied = false }); // top center
            CurrentBirdSimulation.Instance.RestingSpots.Add(new RestingSpot() { Position = new System.Numerics.Vector3((float)RootGrid.ActualWidth / 1714 * 1216   , (float)RootGrid.ActualHeight / 3002 * 1392 - 70, 0), IsOccupied = false }); // top right
            CurrentBirdSimulation.Instance.RestingSpots.Add(new RestingSpot() { Position = new System.Numerics.Vector3((float)RootGrid.ActualWidth / 1714 * 755    , (float)RootGrid.ActualHeight / 3002 * 2011 - 70, 0), IsOccupied = false }); // bottom left
            CurrentBirdSimulation.Instance.RestingSpots.Add(new RestingSpot() { Position = new System.Numerics.Vector3((float)RootGrid.ActualWidth / 1714 * 1050   , (float)RootGrid.ActualHeight / 3002 * 1894 - 70, 0), IsOccupied = false }); // bottom right

            Bird1Rect.Translation = new System.Numerics.Vector3((float)RootGrid.ActualWidth / 1714 * 275    , (float)RootGrid.ActualHeight / 3002 * 1070 - 70, 0)   ;
            Bird2Rect.Translation = new System.Numerics.Vector3((float)RootGrid.ActualWidth / 1714 * 1000   , (float)RootGrid.ActualHeight / 3002 * 1266 - 70, 0)   ;
            Bird3Rect.Translation = new System.Numerics.Vector3((float)RootGrid.ActualWidth / 1714 * 1216   , (float)RootGrid.ActualHeight / 3002 * 1392 - 70, 0)   ;
            Bird4Rect.Translation = new System.Numerics.Vector3((float)RootGrid.ActualWidth / 1714 * 755    , (float)RootGrid.ActualHeight / 3002 * 2011 - 70, 0)   ;
            Bird5Rect.Translation = new System.Numerics.Vector3((float)RootGrid.ActualWidth / 1714 * 1050   , (float)RootGrid.ActualHeight / 3002 * 1894 - 70, 0)   ;

            CurrentBirdSimulation.Instance.RestingSpots[1].SetNeighborRestingSpots(CurrentBirdSimulation.Instance.RestingSpots[0], CurrentBirdSimulation.Instance.RestingSpots[2]);
            CurrentBirdSimulation.Instance.RestingSpots[4].SetNeighborRestingSpots(CurrentBirdSimulation.Instance.RestingSpots[3], null);

            CurrentBirdSimulation.Instance.RootGrid = RootGrid;
            CurrentBirdSimulation.Instance.UIDispatcherQueue = DispatcherQueue;

            parulAI.StartSpawningBirds(100, 15000);
            
            _finishedLoading = true;
        }

        private void RootGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_finishedLoading)
            {
                
                CurrentBirdSimulation.Instance.RestingSpots[0].Position = new System.Numerics.Vector3((float)RootGrid.ActualWidth / 1714 * 275    , (float)RootGrid.ActualHeight / 3002 * 1070 - 70, 0)   ;
                CurrentBirdSimulation.Instance.RestingSpots[1].Position = new System.Numerics.Vector3((float)RootGrid.ActualWidth / 1714 * 1000   , (float)RootGrid.ActualHeight / 3002 * 1266 - 70, 0)   ;
                CurrentBirdSimulation.Instance.RestingSpots[2].Position = new System.Numerics.Vector3((float)RootGrid.ActualWidth / 1714 * 1216   , (float)RootGrid.ActualHeight / 3002 * 1392 - 70, 0)   ;
                CurrentBirdSimulation.Instance.RestingSpots[3].Position = new System.Numerics.Vector3((float)RootGrid.ActualWidth / 1714 * 755    , (float)RootGrid.ActualHeight / 3002 * 2011 - 70, 0)   ;
                CurrentBirdSimulation.Instance.RestingSpots[4].Position = new System.Numerics.Vector3((float)RootGrid.ActualWidth / 1714 * 1050   , (float)RootGrid.ActualHeight / 3002 * 1894 - 70, 0)   ;

                Bird1Rect.Translation = new System.Numerics.Vector3((float)RootGrid.ActualWidth / 1714 * 275    , (float)RootGrid.ActualHeight / 3002 * 1070 - 70, 0);
                Bird2Rect.Translation = new System.Numerics.Vector3((float)RootGrid.ActualWidth / 1714 * 1000   , (float)RootGrid.ActualHeight / 3002 * 1266 - 70, 0);
                Bird3Rect.Translation = new System.Numerics.Vector3((float)RootGrid.ActualWidth / 1714 * 1216   , (float)RootGrid.ActualHeight / 3002 * 1392 - 70, 0);
                Bird4Rect.Translation = new System.Numerics.Vector3((float)RootGrid.ActualWidth / 1714 * 755    , (float)RootGrid.ActualHeight / 3002 * 2011 - 70, 0);
                Bird5Rect.Translation = new System.Numerics.Vector3((float)RootGrid.ActualWidth / 1714 * 1050   , (float)RootGrid.ActualHeight / 3002 * 1894 - 70, 0);

                foreach (Bird bird in CurrentBirdSimulation.Instance.Birds)
                {
                    if (bird.State != BirdState.Flying1 && bird.State != BirdState.Flying2 && bird.State != BirdState.Flying3 && bird.State != BirdState.Flying4)
                    {
                        bird.Position = bird.RestingSpot.Position;
                        bird.BirdSprite.Translation = bird.Position;
                    }
                    else if (bird.IsTargetedLocationRestingSpot)
                    {
                        bird.TargetPosition = bird.TargetedRestingSpot.Position;
                    }
                }
            }
        }
    }
}
