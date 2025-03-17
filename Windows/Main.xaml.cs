using Launcher.Any;
using Launcher.Any.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Launcher.Windows
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : Window
    {
        public Main()
        {
            InitializeComponent();

            Loaded += Main_Loaded;
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Loader(ELoaderState.Show);
        }


        #region Функции
        #region Notify
        public void Notify(string message)
        {
            NP_message_block.Text = message;

            #region Анимация
            var storyboard = new Storyboard();
            var animation = new DoubleAnimationUsingKeyFrames();
            animation.KeyFrames.Add(new EasingDoubleKeyFrame(1, TimeSpan.FromMilliseconds(150)));
            animation.KeyFrames.Add(new EasingDoubleKeyFrame(1, TimeSpan.FromMilliseconds(3150)));
            animation.KeyFrames.Add(new EasingDoubleKeyFrame(0, TimeSpan.FromMilliseconds(3300)));

            Storyboard.SetTarget(animation, NotifyPanel);
            Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));

            storyboard.Children.Add(animation);

            storyboard.Begin(NP_message_block, HandoffBehavior.SnapshotAndReplace, true);
            #endregion
        }
        #endregion
        #region Loader
        public async Task Loader(ELoaderState newState)
        {
            var tcs = new TaskCompletionSource<object?>();

            if (newState == ELoaderState.Show)
            {
                LoaderPanel.Visibility = Visibility.Visible;
                var animation = AnimationHelper.OpacityAnimation(LoaderPanel, 1);
                animation.Completed += (s, e) => tcs.SetResult(null);
                animation.Begin(LoaderPanel, HandoffBehavior.SnapshotAndReplace, true);
                LP_loader.StartAnimation();
            }
            else
            {
                var animation = AnimationHelper.OpacityAnimation(LoaderPanel, 0);
                animation.Completed += (s, e) =>
                {
                    Dispatcher.Invoke(() => LoaderPanel.Visibility = Visibility.Hidden);
                    tcs.SetResult(null);
                };
                LP_loader.StopAnimation();
                animation.Begin(LoaderPanel, HandoffBehavior.SnapshotAndReplace, true);
            }

            await tcs.Task;
        }
        #endregion
        #endregion
    }
}
