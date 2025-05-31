using Launcher.Any;
using Launcher.Any.CGitHelperAny;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Launcher.Components
{
    /// <summary>
    /// Логика взаимодействия для CGitSyncProgress.xaml
    /// </summary>
    public partial class CGitSyncProgress : UserControl
    {
        public CGitSyncProgress()
        {
            InitializeComponent();

            this.Opacity = 0;
            this.Loaded += ELoaded;

            CGitHelper.GitTaskCompletionStageChanged += EGitTaskCompletionStageChanged;
        }



        #region Переменные
        private bool IsLoadedStoryBoard { get; set; } = false;
        private Storyboard UpdatingRotationStoryBoard { get; set; } = new Storyboard();
        #endregion


        #region Обработчики событий
        #region EGitTaskCompletionStageChanged
        private void EGitTaskCompletionStageChanged(CGitTaskCompletion completion, int completedCount, int totalCount, EGitTaskCompletionStage stage, int queueCount)
            => Dispatcher.Invoke(() => UpdateView(completion, completedCount, totalCount, stage, queueCount));
        #endregion
        #region ELoaded
        private void ELoaded(object sender, RoutedEventArgs e)
        {
            var anim = new DoubleAnimation(0, 360, TimeSpan.FromMilliseconds(1000))
            {
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(anim, IconComponent);
            Storyboard.SetTargetProperty(anim, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));

            UpdatingRotationStoryBoard.Children.Add(anim);
        }
        #endregion
        #endregion

        #region Функции
        #region UpdateView
        private void UpdateView(CGitTaskCompletion completion, int completedCount, int totalCount, EGitTaskCompletionStage stage, int queueCount)
        {
            switch (stage)
            {
                case EGitTaskCompletionStage.Started:
                    {
                        this.Visibility = Visibility.Visible;

                        var animShow = AnimationHelper.OpacityAnimationStoryBoard(this, 1);
                        animShow.Begin(this, HandoffBehavior.SnapshotAndReplace, true);

                        UpdatingRotationStoryBoard.Begin(IconComponent, HandoffBehavior.SnapshotAndReplace, true);

                        IsLoadedStoryBoard = true;

                        ProgressFore.Width = 0;
                        ProgressValue.Text = $"0/{totalCount}";
                    }
                    break;
                case EGitTaskCompletionStage.Progress:
                    {
                        if (this.Opacity != 1)
                        {
                            var animShow = AnimationHelper.OpacityAnimationStoryBoard(this, 1);
                            animShow.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
                        }

                        if (!IsLoadedStoryBoard)
                        {
                            UpdatingRotationStoryBoard.Begin(IconComponent, HandoffBehavior.SnapshotAndReplace, true);
                            IsLoadedStoryBoard = true;
                        }

                        ProgressFore.Width = ((double)completedCount / totalCount) * ProgressBack.ActualWidth;
                        ProgressValue.Text = $"{completedCount}/{totalCount}";
                    }
                    break;

                case EGitTaskCompletionStage.Completed:
                    {
                        if (queueCount > 0) return;

                        var animShow = AnimationHelper.OpacityAnimationStoryBoard(this, 0);
                        animShow.Completed += (_, __) => this.Visibility = Visibility.Hidden;
                        animShow.Begin(this, HandoffBehavior.SnapshotAndReplace, true);

                        UpdatingRotationStoryBoard.Stop(IconComponent);

                        ProgressFore.Width = ProgressBack.ActualWidth;
                        ProgressValue.Text = $"{completedCount}/{totalCount}";
                    }
                    break;
            }
        }
        #endregion
        #endregion
    }
}
