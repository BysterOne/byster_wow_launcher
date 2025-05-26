using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Any;
using Launcher.Any.UDialogBox;
using Launcher.Api.Models;
using Launcher.Cls;
using Launcher.Components.MainWindow.MediaViewerAny;
using Launcher.Windows;
using Launcher.Windows.AnyMain.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Launcher.Components.MainWindow
{
    namespace MediaViewerAny
    {
        public enum EStatus 
        { 
            Played,
            Stopped
        }
        public enum EShow
        {
            NoObjectOfTheRequiredType,
            EmptyMediaList
        }
    }
    /// <summary>
    /// Логика взаимодействия для CMediaViewer.xaml
    /// </summary>
    public partial class CMediaViewer : UserControl, IUDialogBox
    {
        public CMediaViewer()
        {
            InitializeComponent();

            this.Opacity = 0;

            Application.Current.Windows.OfType<Main>().First().PreviewKeyDown += EKeyDown;

            this.MG_back_button.MouseEnter += MG_back_button_MouseEnter;
            this.MG_back_button.MouseLeave += MG_back_button_MouseLeave;
            this.MG_back_button.MouseLeftButtonDown += MG_back_button_MouseLeftButtonDown;

            this.MG_next_button.MouseEnter += MG_next_button_MouseEnter;
            this.MG_next_button.MouseLeave += MG_next_button_MouseLeave;
            this.MG_next_button.MouseLeftButtonDown += MG_next_button_MouseLeftButtonDown;
        }

        #region Переменные
        private bool CanFullScreen { get; set; }
        private bool IsFullScreenImage { get; set; } = false;
        private bool IsPresedTimeControl { get; set; } = false;
        private bool CanChangeMedia { get; set; } = true;
        private DispatcherTimer? Timer { get; set; }
        public static LogBox Pref { get; set; } = new("Media Viewer");
        private TaskCompletionSource<EDialogResponse> TaskCompletionSource { get; set; } = new();

        public List<Media> Medias { get; set; } = [];
        public Media? CurrentMedia { get; set; } = null;
        public int CurrentMediaIndex { get; set; } = 0;

        private Thickness DefaultMargin { get; set; } = new Thickness(80, 80, 80, 80);
        private Thickness DefaultAnimationMargin { get; set; } = new Thickness(80, 60, 80, 40);
        private Thickness ScaledMargin { get; set; } = new Thickness(20, 20, 20, 20);
        #endregion

        #region Свойства
        #region Status
        private EStatus Status
        {
            get => (EStatus)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        private static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(EStatus), typeof(CMediaViewer),
                new PropertyMetadata(EStatus.Stopped, OnStatusChanged));

        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CMediaViewer sender) sender.UpdateVideoPlayerView();
        }
        #endregion
        #endregion

        #region Обработчики событий
        #region ETimerTick
        private void ETimerTick(object? sender, EventArgs e)
        {
            Timer!.Stop();
            SetTimeLineAndTime();
            Timer.Start();
        }
        #endregion
        #region EKeyDown
        private void EKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape: TaskCompletionSource.TrySetResult(EDialogResponse.Closed); break;
                case Key.Left:
                    if (CurrentMedia is not null && CurrentMedia.Type is EMediaType.Video)
                        Rewind(TimeSpan.FromSeconds(-10)); 
                    break;
                case Key.Right:
                    if (CurrentMedia is not null && CurrentMedia.Type is EMediaType.Video)
                        Rewind(TimeSpan.FromSeconds(10));
                    break;
                case Key.Space: Status = Status == EStatus.Played ? EStatus.Stopped : EStatus.Played; break;
                default: base.OnKeyDown(e); break;
            }
        }
        #endregion
        #region MG_close_button_MouseLeftButtonDown
        private void MG_close_button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => TaskCompletionSource.TrySetResult(EDialogResponse.Closed);
        #endregion
        #region MG_image_block_PreviewMouseLeftButtonUp
        private void MG_image_block_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!CanFullScreen) return;
            IsFullScreenImage = !IsFullScreenImage;
            UpdateFullScreenView();
        }
        #endregion
        #region MG_back_button_MouseEnter
        private void MG_back_button_MouseEnter(object sender, MouseEventArgs e) => ChangeButtonsBackground(MGBB_background, 1);
        #endregion
        #region MG_back_button_MouseLeave
        private void MG_back_button_MouseLeave(object sender, MouseEventArgs e) => ChangeButtonsBackground(MGBB_background, 0);
        #endregion
        #region MG_next_button_MouseEnter
        private void MG_next_button_MouseEnter(object sender, MouseEventArgs e) => ChangeButtonsBackground(MGNB_background, 1);
        #endregion
        #region MG_next_button_MouseLeave
        private void MG_next_button_MouseLeave(object sender, MouseEventArgs e) => ChangeButtonsBackground(MGNB_background, 0);
        #endregion
        #region MG_back_button_MouseLeftButtonDown
        private void MG_back_button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!CanChangeMedia) return;

            if (CurrentMediaIndex > 0) { CurrentMediaIndex--; }
            else { CurrentMediaIndex = Medias.Count - 1; }

            CurrentMedia = Medias[CurrentMediaIndex];

            SetMedia();
        }
        #endregion
        #region MG_next_button_MouseLeftButtonDown
        private void MG_next_button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!CanChangeMedia) return;

            if (CurrentMediaIndex < Medias.Count - 1) { CurrentMediaIndex++; }
            else { CurrentMediaIndex = 0; }

            CurrentMedia = Medias[CurrentMediaIndex];

            SetMedia();
        }
        #endregion
        #region MGBVCP_timeline_MouseLeave
        private void MGBVCP_timeline_MouseLeave(object sender, MouseEventArgs e)
        {
            if (IsPresedTimeControl)
            {
                Point mousePosition = e.GetPosition(sender as UIElement);
                SetNewTimeLinePosition(mousePosition.X);
                IsPresedTimeControl = false;
            }
        }
        #endregion
        #region MGBVCP_timeline_PreviewMouseMove
        private void MGBVCP_timeline_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (IsPresedTimeControl)
            {
                Point mousePosition = e.GetPosition(sender as UIElement);
                SetPreviewTimeLinePosition(mousePosition.X);
            }
        }
        #endregion
        #region MGBVCP_timeline_MouseLeftButtonUp
        private void MGBVCP_timeline_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsPresedTimeControl)
            {
                Point mousePosition = e.GetPosition(sender as UIElement);
                SetNewTimeLinePosition(mousePosition.X);
                IsPresedTimeControl = false;
            }
        }
        #endregion
        #region MGBVCP_timeline_PreviewMouseLeftButtonDown
        private void MGBVCP_timeline_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsPresedTimeControl = true;
            Point mousePosition = e.GetPosition(sender as UIElement);
            SetPreviewTimeLinePosition(mousePosition.X);
        }
        #endregion
        #region MGBVCPB_behind_MouseLeftButtonDown
        private void MGBVCPB_behind_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Rewind(TimeSpan.FromSeconds(-10));
        #endregion
        #region MGBVCPB_forward_MouseLeftButtonDown
        private void MGBVCPB_forward_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Rewind(TimeSpan.FromSeconds(10));
        #endregion
        #region MGVB_video_MediaOpened
        private void MGVB_video_MediaOpened(object sender, RoutedEventArgs e)
        {
            var lastStat = Status;

            Status = EStatus.Played;
            if (lastStat == Status) UpdateVideoPlayerView();

            SetTimeLineAndTime();
        }
        #endregion
        #region MGVB_video_MediaEnded
        private void MGVB_video_MediaEnded(object sender, RoutedEventArgs e) => Status = EStatus.Stopped;
        #endregion
        #region MGVB_video_PreviewMouseLeftButtonDown
        private void MGVB_video_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Status = Status == EStatus.Played ? EStatus.Stopped : EStatus.Played;
        #endregion
        #endregion


        #region Функции
        #region Show
        public async Task<UResponse<EDialogResponse>> Show(params object[] pars)
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Ошибка при показе окна";

            #region try
            try
            {
                #region Проверка объекта
                var t = pars.First().GetType();
                var medias = pars.OfType<List<Media>>().FirstOrDefault();
                if (medias is null) throw new UExcept(EShow.NoObjectOfTheRequiredType, $"Требуется объект типа {typeof(List<Media>).FullName}");
                var selectedIndex = pars.OfType<int>().Select(i => (int?)i).FirstOrDefault();
                #endregion
                #region Появление окна                
                var fadeIn = AnimationHelper.OpacityAnimationStoryBoard(this, 1);
                fadeIn.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
                #endregion
                #region Загрузка медиа
                if (medias.Count is 0) throw new UExcept(EShow.EmptyMediaList, $"Список медиа пуст");
                Medias = medias;
                CanChangeMedia = true;
                CurrentMediaIndex = selectedIndex is not null ? (int)selectedIndex : 0;
                CurrentMedia = Medias[CurrentMediaIndex];                
                SetMedia(true);
                #endregion

                return new(await TaskCompletionSource.Task);
            }
            #endregion
            #region UExcept
            catch (UExcept ex)
            {
                return new(ex);
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                return new(new UExcept(GlobalErrors.Exception, $"Исключение: {ex.Message}", ex));
            }
            #endregion
        }
        #endregion
        #region Hide
        public async Task Hide()
        {
            var tcs = new TaskCompletionSource<object?>();

            var anim = AnimationHelper.OpacityAnimationStoryBoard(this, 0);
            anim.Completed += (_, __) => tcs.SetResult(null);
            anim.Begin(this, HandoffBehavior.SnapshotAndReplace, true);           

            await tcs.Task;

            if (MGVB_video.CanPause) { MGVB_video.Pause(); MGVB_video.Source = null; }
            MGIB_image.Source = null;

            Application.Current.Windows.OfType<Main>().First().KeyDown -= EKeyDown;
        }
        #endregion
        #region SetMedia
        private async void SetMedia(bool isfirst = false)
        {
            if (!CanChangeMedia) return;
            CanChangeMedia = false;

            CanFullScreen = false;
            IsFullScreenImage = false;
            if (CurrentMedia is null) return;

            if (!isfirst)
            {
                #region Изображение
                var newThicknessImageBlock = DefaultAnimationMargin;
                if (MG_image_block.Margin != newThicknessImageBlock || MG_image_block.Opacity != 0)
                {
                    var story = AnimationHelper.OpacityAnimationStoryBoard(MG_image_block, 0);
                    story.Children.Add(AnimationHelper.ThicknessAnimation(MG_image_block, newThicknessImageBlock));
                    story.Begin(MG_image_block, HandoffBehavior.SnapshotAndReplace, true);
                }
                #endregion

                #region Видео
                var newThicknessVideoBlock = DefaultAnimationMargin;
                if (MG_video_block.Margin != newThicknessVideoBlock || MG_video_block.Opacity != 0)
                {
                    var story = AnimationHelper.OpacityAnimationStoryBoard(MG_video_block, 0);
                    story.Children.Add(AnimationHelper.ThicknessAnimation(MG_video_block, newThicknessVideoBlock));
                    story.Begin(MG_video_block, HandoffBehavior.SnapshotAndReplace, true);

                    MGVB_video.Pause();
                    MGVB_video.Source = null;

                    Timer?.Stop();
                }
                #endregion

                await Task.Run(() => { Thread.Sleep(AnimationHelper.AnimationDuration); });

                Grid.SetZIndex(MG_image_block, -1);
                Grid.SetZIndex(MG_video_block, -1);
            }
            else
            {
                Grid.SetZIndex(MG_image_block, -1);
                MG_image_block.BeginAnimation(OpacityProperty, null);
                MG_image_block.BeginAnimation(MarginProperty, null);
                MG_image_block.Opacity = 0;
                MG_image_block.Margin = DefaultMargin;

                Grid.SetZIndex(MG_video_block, -1);
                MG_video_block.BeginAnimation(OpacityProperty, null);
                MG_video_block.BeginAnimation(MarginProperty, null);                
                MG_video_block.Opacity = 0;
                MG_video_block.Margin = DefaultMargin;
            }

            if (CurrentMedia.Type is EMediaType.Image)
            {
                await ImageControlHelper.LoadImageAsync
                (
                    CurrentMedia.Url, 
                    -1,
                    -1, 
                    CancellationToken.None, (imgSource) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            MGIB_image.Source = imgSource;
                        },
                        DispatcherPriority.Background);
                    }
                );

                Grid.SetZIndex(MG_image_block, 9);

                var story = AnimationHelper.OpacityAnimationStoryBoard(MG_image_block, 1);
                story.Completed += (_, __) => CanChangeMedia = true;
                story.Children.Add(AnimationHelper.ThicknessAnimation(MG_image_block, DefaultMargin));
                story.Begin(MG_image_block, HandoffBehavior.SnapshotAndReplace, true);
            }
            else if (CurrentMedia.Type is EMediaType.Video)
            {
                MGBVCPT_line.BeginAnimation(WidthProperty, null);
                MGBVCPT_line.Width = 0;
                MGVB_video.Source = new Uri(CurrentMedia.Url);

                Timer = new DispatcherTimer();
                Timer.Interval = TimeSpan.FromMilliseconds(300);
                Timer.Tick += ETimerTick;

                Status = EStatus.Played;

                Grid.SetZIndex(MG_video_block, 9);
                var story = AnimationHelper.OpacityAnimationStoryBoard(MG_video_block, 1);
                story.Completed += (_, __) => CanChangeMedia = true;
                story.Children.Add(AnimationHelper.ThicknessAnimation(MG_video_block, DefaultMargin));
                story.Begin(MG_video_block, HandoffBehavior.SnapshotAndReplace, true);
            }

            CanFullScreen = true;
        }
        #endregion
        #region SetNewTimeLinePosition
        private void SetNewTimeLinePosition(double x)
        {
            var timer = x / MGBV_control_panel.ActualWidth;

            TimeSpan duration = MGVB_video.NaturalDuration.TimeSpan;
            TimeSpan position = TimeSpan.FromMilliseconds(timer * duration.TotalMilliseconds);
            MGVB_video.Position = position;

            Status = EStatus.Played;
        }
        #endregion
        #region SetPreviewTimeLinePosition
        private void SetPreviewTimeLinePosition(double x)
        {
            var timer = x / MGBV_control_panel.ActualWidth;

            TimeSpan duration = MGVB_video.NaturalDuration.TimeSpan;
            TimeSpan position = TimeSpan.FromMilliseconds(timer * duration.TotalMilliseconds);
            var timeline = (position.TotalMilliseconds / duration.TotalMilliseconds) * MGBV_control_panel.ActualWidth;

            MGBVCPT_line.BeginAnimation(WidthProperty, null);
            MGBVCPT_line.Width = Math.Round(timeline, 2);
            MGBVCPB_time.Text = $"{position.ToString(@"mm\:ss")}/{duration.ToString(@"mm\:ss")}";
        }
        #endregion
        #region SetTimeLineAndTime
        private void SetTimeLineAndTime()
        {
            if (MGVB_video.Source == null) return;

            if (!IsPresedTimeControl)
            {
                if (!MGVB_video.NaturalDuration.HasTimeSpan) return;
                TimeSpan duration = MGVB_video.NaturalDuration.TimeSpan;
                TimeSpan position = MGVB_video.Position;
                var timeline = (position.TotalMilliseconds / duration.TotalMilliseconds) * MGBV_control_panel.ActualWidth;

                AnimationHelper.WidthAnimationStoryBoard(MGBVCPT_line, timeline, easing: AnimationHelper.EaseIn)
                    .Begin(MGBVCPT_line, HandoffBehavior.SnapshotAndReplace, true);

                MGBVCPB_time.Text = $"{position.ToString(@"mm\:ss")}/{duration.ToString(@"mm\:ss")}";
            }
        }
        #endregion
        #region UpdateVideoPlayerView
        private void UpdateVideoPlayerView()
        {
            if (Status == EStatus.Stopped)
            {
                Timer?.Stop();
                MGBVCPB_play.Icon = BitmapFrame.Create(Functions.GetSourceFromResource($"Media/play_video_icon.png"));
                if (MGVB_video.CanPause) MGVB_video.Pause();
            }
            else if (Status == EStatus.Played)
            {
                Timer?.Start();
                MGBVCPB_play.Icon = BitmapFrame.Create(Functions.GetSourceFromResource($"Media/pause_icon.png"));

                MGVB_video.Play();
                if (MGVB_video.NaturalDuration.HasTimeSpan)
                {
                    TimeSpan duration = MGVB_video.NaturalDuration.TimeSpan;
                    TimeSpan position = MGVB_video.Position;
                    if (duration.TotalSeconds == position.TotalSeconds) { MGVB_video.Position = TimeSpan.FromSeconds(0); }
                }
            }
        }
        #endregion
        #region UpdateFullScreenView
        private void UpdateFullScreenView()
        {
            var story = new Storyboard();

            var anim = new ThicknessAnimation
            (
                IsFullScreenImage ? new Thickness(20) : DefaultMargin,
                AnimationHelper.AnimationDuration
            )
            {
                EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(anim, MG_image_block);
            Storyboard.SetTargetProperty(anim, new PropertyPath(MarginProperty));

            story.Children.Add(anim);

            story.Begin(MG_image_block, HandoffBehavior.SnapshotAndReplace, true);
        }
        #endregion
        #region ChangeButtonsBackground
        private void ChangeButtonsBackground(FrameworkElement element, double opacity) 
            => AnimationHelper.OpacityAnimationStoryBoard(element, opacity).
                Begin(element, HandoffBehavior.SnapshotAndReplace, true);
        #endregion
        #region Rewind
        private void Rewind(TimeSpan time)
        {
            TimeSpan duration = MGVB_video.NaturalDuration.TimeSpan;
            TimeSpan position = MGVB_video.Position;

            var newTimeLine = position + time;
            if (newTimeLine < TimeSpan.Zero) newTimeLine = TimeSpan.Zero;
            else if (newTimeLine > duration) newTimeLine = TimeSpan.Zero;

            MGVB_video.Play();
            MGVB_video.Position = newTimeLine;

            Status = EStatus.Played;
        }
        #endregion
        #endregion

    }
}
