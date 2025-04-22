using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Any;
using Launcher.Components.Skeleton;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Launcher.Components
{
    namespace Skeleton
    {
        public enum ESkeleton { FailChangeState }
        public enum EChange { Exception, RectangleWasNull }
    }

    public class CSkeleton : ContentControl
    {
        static CSkeleton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CSkeleton), new FrameworkPropertyMetadata(typeof(CSkeleton)));
        }

        public CSkeleton()
        {
            this.Loaded += CSkeleton_Loaded;
        }

        #region Переменные
        private LogBox Pref { get; set; } = new("Skeleton");
        #endregion

        #region Свойства
        #region Background
        public new SolidColorBrush Background
        {
            get => (SolidColorBrush)GetValue(BackgroundProperty);
            set { SetValue(BackgroundProperty, value); _ = ChangeColors(); }
        }

        public static readonly new DependencyProperty BackgroundProperty =
            DependencyProperty.Register(nameof(Background), typeof(SolidColorBrush), typeof(CSkeleton),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0x45, 0x45, 0x47))));
        #endregion
        #region LoaderOpacity
        public double LoaderOpacity
        {
            get => (double)GetValue(LoaderOpacityProperty);
            set { SetValue(LoaderOpacityProperty, value); }
        }

        public static readonly DependencyProperty LoaderOpacityProperty =
            DependencyProperty.Register(nameof(LoaderOpacity), typeof(double), typeof(CSkeleton),
                new PropertyMetadata((double)0));
        #endregion
        #region LoaderOpacity
        public new Thickness Padding
        {
            get => (Thickness)GetValue(PaddingProperty);
            set { SetValue(PaddingProperty, value); }
        }

        public static new readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register(nameof(Padding), typeof(Thickness), typeof(CSkeleton),
                new PropertyMetadata(new Thickness(5)));
        #endregion
        #endregion

        private LinearGradientBrush GetBrush()
        {
            var fcolor = Background.Color;
            var scolor = Color.FromArgb(0xFF, 0x98, 0x98, 0x98);

            var stops = new GradientStopCollection()
            {
                new GradientStop(fcolor, -0.5),
                new GradientStop(fcolor, -0.5),
                new GradientStop(scolor, -0.35),
                new GradientStop(fcolor, -0.2),
                new GradientStop(fcolor, 1.5),
            };

            var gradient = new LinearGradientBrush(stops)
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
            };

            return gradient;
        }

        private void CSkeleton_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var background = (Rectangle)GetTemplateChild("gradientRect");
                OpacityMonitor.Monitor(background);
                background.Fill = GetBrush();
            });
        }

        private async Task ChangeColors()
        {
            var background = (Rectangle)GetTemplateChild("gradientRect");
            if (background is not null)
            {
                background.Fill = GetBrush();
                //await ChangeState(true, false);
            }
        }

        public async Task ChangeState(bool visible, bool useAnimation = true, [CallerMemberName] string init = "") => await Dispatcher.InvokeAsync(() => _change(visible, useAnimation, init), DispatcherPriority.Loaded);


        private async Task _change(bool visible, bool useAnimation, string init)
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName()).AddTrace(init);
            var _failinf = $"Не удалось изменить состояние";

            #region try
            try
            {
                if (visible) _ = Dispatcher.InvokeAsync(() => Animation(true));

                var tcs = new TaskCompletionSource<bool>();
                var storyboard = new Storyboard();

                var duration = visible ? TimeSpan.FromMilliseconds(1) : AnimationHelper.AnimationDuration;
                var function = new PowerEase() { EasingMode = EasingMode.EaseInOut };

                var animation = new DoubleAnimation(this.LoaderOpacity, visible ? 1 : 0, duration);
                Storyboard.SetTarget(animation, this);
                Storyboard.SetTargetProperty(animation, new PropertyPath(LoaderOpacityProperty));

                storyboard.Children.Add(animation);
                storyboard.Completed += (sender, e) =>
                {
                    tcs.SetResult(true);
                };
                storyboard.RemoveRequested += (sender, e) => Debugger.Break();
                storyboard.Begin(this, HandoffBehavior.SnapshotAndReplace, true);

                await tcs.Task;
            }
            #endregion
            #region UExcept
            catch (UExcept ex)
            {
                var glerror = new UError(ESkeleton.FailChangeState, _failinf, ex.Error);
                Functions.Error(ex, glerror, glerror.Message, _proc);
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uerror = new UError(EChange.Exception, $"Исключение: {ex.Message}");
                var glerror = new UError(ESkeleton.FailChangeState, $"{_failinf}: исключение", uerror);
                Functions.Error(ex, glerror, glerror.Message, _proc);
            }
            #endregion
        }

        private void Animation(bool show)
        {
            var function = new PowerEase() { EasingMode = EasingMode.EaseInOut };
            var background = (Rectangle)GetTemplateChild("gradientRect");
            if (background.Fill is LinearGradientBrush gradient)
            {
                var points = gradient.GradientStops;

                if (show)
                {
                    var storyboard = new Storyboard();
                    var duration = TimeSpan.FromMilliseconds(1000);

                    #region Вторая точка
                    var animationPoint2 = new DoubleAnimation(-0.5, 1.2, duration)
                    {
                        EasingFunction = function,
                        RepeatBehavior = RepeatBehavior.Forever
                    };
                    Storyboard.SetTarget(animationPoint2, background);
                    Storyboard.SetTargetProperty(animationPoint2, new PropertyPath("Fill.GradientStops[1].Offset"));
                    storyboard.Children.Add(animationPoint2);
                    #endregion
                    #region Третья точка
                    var animationPoint3 = new DoubleAnimation(-0.35, 1.35, duration)
                    {
                        EasingFunction = function,
                        RepeatBehavior = RepeatBehavior.Forever
                    };
                    Storyboard.SetTarget(animationPoint3, background);
                    Storyboard.SetTargetProperty(animationPoint3, new PropertyPath("Fill.GradientStops[2].Offset"));
                    storyboard.Children.Add(animationPoint3);
                    #endregion
                    #region Четвертая точка
                    var animationPoint4 = new DoubleAnimation(-0.2, 1.5, duration)
                    {
                        EasingFunction = function,
                        RepeatBehavior = RepeatBehavior.Forever
                    };
                    Storyboard.SetTarget(animationPoint4, background);
                    Storyboard.SetTargetProperty(animationPoint4, new PropertyPath("Fill.GradientStops[3].Offset"));
                    storyboard.Children.Add(animationPoint4);
                    #endregion

                    try
                    {
                        //storyboard.Begin();
                        storyboard.Begin(background, HandoffBehavior.Compose, true);
                    }
                    catch (Exception ex) { Debugger.Break(); }
                }
                else
                {
                    background.Fill = GetBrush();
                }
            }
        }
    }
}
