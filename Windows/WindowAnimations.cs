using Launcher.Any;
using Launcher.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Launcher.Windows
{
    public class WindowAnimations
    {
        private static TimeSpan LocalAnimationDuration = TimeSpan.FromMilliseconds(150);

        public static void ApplyFadeAnimations<T>(ref T window, bool closeWindowIsCloseApp = false) where T : Window
        {
            window.Opacity = 0;

            // Здесь можно будет через WinApi Hook и реализовать полноценно

            window.StateChanged += AStateChanged;
            window.Loaded += ALoaded;
            if (!closeWindowIsCloseApp) window.Closing += AClosing;
            else window.Closing += AClosing;

            window.CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindow));
        }           

        private static void ALoaded(object sender, RoutedEventArgs e)
        {
            var window = sender as Window;
            AnimationHelper.OpacityAnimation(window!, 1, LocalAnimationDuration).Begin(window, HandoffBehavior.SnapshotAndReplace, true);
        }
       
        private static void AClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            var window = sender as Window;
            AnimationHelper.OpacityAnimation(window!, 0, LocalAnimationDuration).Begin(window, HandoffBehavior.SnapshotAndReplace, true);
        }

        private static void AClosingWithApp(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            var window = sender as Window;
            var storyboard = AnimationHelper.OpacityAnimation(window!, 0, LocalAnimationDuration);
            storyboard.Completed += (s, a) => { Application.Current.Shutdown(); };
            storyboard.Begin(window, HandoffBehavior.SnapshotAndReplace, true);
        }

        private static void AStateChanged(object? sender, EventArgs e)
        {            
            var window = sender as Window;
            if (window is null) return;

            if (window.WindowState == WindowState.Normal)
            {
                AnimationHelper.OpacityAnimation(window!, 1, LocalAnimationDuration).Begin(window, HandoffBehavior.SnapshotAndReplace, true);
            }
            else if (window.WindowState == WindowState.Minimized)
            {
                AnimationHelper.OpacityAnimation(window!, 0, LocalAnimationDuration).Begin(window, HandoffBehavior.SnapshotAndReplace, true);
            }
        }

        private static void OnMinimizeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            var window = sender as Window;
            var storyboard = AnimationHelper.OpacityAnimation(window!, 0, LocalAnimationDuration);
            storyboard.Completed += (s, a) =>
            {
                SystemCommands.MinimizeWindow(window);
            };
            storyboard.Begin(window, HandoffBehavior.SnapshotAndReplace, true);
        }
    }
}
