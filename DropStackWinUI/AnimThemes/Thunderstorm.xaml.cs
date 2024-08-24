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
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DropStackWinUI.AnimThemes
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Thunderstorm : Page
    {
        Timer _timer = null;

        public Thunderstorm()
        {
            this.InitializeComponent();
            this.Loaded += Thunderstorm_Loaded;
            this.Unloaded += Thunderstorm_Unloaded;
        }

        private async void Thunderstorm_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(2000);
            _timer = new Timer(CreateStrike, null, 0, 10000);
        }

        private void Thunderstorm_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer = null;
        }

        private async void CreateStrike(object state)
        {
            Random rnd = new Random();
            int decision = rnd.Next(0, 4);
            switch (decision)
            {
                case 0:
                    DispatcherQueue.TryEnqueue(() => { CloudsBloomed.Opacity = 1; });
                    await Task.Delay(200);
                    DispatcherQueue.TryEnqueue(() => { CloudsBloomed.Opacity = 0.3; });
                    await Task.Delay(100);
                    DispatcherQueue.TryEnqueue(() => { CloudsBloomed.Opacity = 0.7; });
                    await Task.Delay(350);
                    DispatcherQueue.TryEnqueue(() => { CloudsBloomed.Opacity = 0; });
                    break;
                case 1:
                    DispatcherQueue.TryEnqueue(() => { CloudsBloomed.Opacity = 0.7; });
                    await Task.Delay(250);
                    DispatcherQueue.TryEnqueue(() => { CloudsBloomed.Opacity = 0.5; });
                    await Task.Delay(100);
                    DispatcherQueue.TryEnqueue(() => { CloudsBloomed.Opacity = 1; });
                    await Task.Delay(350);
                    DispatcherQueue.TryEnqueue(() => { CloudsBloomed.Opacity = 0; });
                    break;
                case 2:
                    DispatcherQueue.TryEnqueue(() => { StrikeNormal.Opacity = 1; });
                    await Task.Delay(100);
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        StrikeBloomed.Opacity = 1;
                        CloudsBloomed.Opacity = 1;
                    });
                    await Task.Delay(200);
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        StrikeNormal.Opacity = 0.8;
                        StrikeBloomed.Opacity = 0;
                        CloudsBloomed.Opacity = 0.5;
                    });
                    await Task.Delay(150);
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        StrikeNormal.Opacity = 0;
                        StrikeBloomed.Opacity = 0.7;
                        CloudsBloomed.Opacity = 0.7;
                    });
                    await Task.Delay(400);
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        StrikeBloomed.Opacity = 0;
                        CloudsBloomed.Opacity = 0;
                    });
                    break;
                case 3:
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        StrikeNormal.Opacity = 0.3;
                        CloudsBloomed.Opacity = 0.3;
                    });
                    await Task.Delay(350);
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        StrikeNormal.Opacity = 0;
                        CloudsBloomed.Opacity = 0;
                    });
                    await Task.Delay(600);
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        StrikeNormal.Opacity = 1;
                        StrikeBloomed.Opacity = 0.3;
                        CloudsBloomed.Opacity = 0.7;
                    });
                    await Task.Delay(200);
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        StrikeNormal.Opacity = 0.3;
                        CloudsBloomed.Opacity = 0.4;
                    });
                    await Task.Delay(150);
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        StrikeNormal.Opacity = 1;
                        StrikeBloomed.Opacity = 1;
                        CloudsBloomed.Opacity = 1;
                    });
                    await Task.Delay(300);
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        StrikeBloomed.Opacity = 0;
                        CloudsBloomed.Opacity = 0.3;
                    });
                    await Task.Delay(500);
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        StrikeNormal.Opacity = 0;
                        CloudsBloomed.Opacity = 0;
                    });
                    break;
                case 4:
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        CloudsBloomed.Opacity = 1;
                        StrikeBloomed.Opacity = 1;
                    });
                    await Task.Delay(250);
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        StrikeBloomed.Opacity = 0;
                        StrikeNormal.Opacity = 0.7;
                        CloudsBloomed.Opacity = 0.4;
                    });
                    await Task.Delay(500);
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        CloudsBloomed.Opacity = 0;
                        StrikeNormal.Opacity = 0;
                    });
                    break;
            }
        }
    }
}
