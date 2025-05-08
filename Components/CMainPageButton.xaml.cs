using Launcher.Any;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Launcher.Components
{
    /// <summary>
    /// Логика взаимодействия для CMainPageButton.xaml
    /// </summary>
    public partial class CMainPageButton : UserControl
    {
        public CMainPageButton()
        {
            InitializeComponent();

            this.DataContext = this;
            Cursor = Cursors.Hand;
            TextBlock.Foreground = Foreground;
        }

        #region Переменные
        private bool _isActive = false;
        public bool IsActive
        {
            get => _isActive;
            set 
            {
                _isActive = value;
                ChangeState(value);
            }
        }
        #endregion

        #region Свойства
        #region Text
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set
            {
                SetValue(TextProperty, value);
            }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(CMainPageButton),
                new PropertyMetadata(""));
        #endregion
        #region Foreground
        public new SolidColorBrush Foreground
        {
            get { return (SolidColorBrush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }
        public static new readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(SolidColorBrush), typeof(CMainPageButton),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(200, 255, 255, 255))));
        #endregion
        #region ForegroundActive
        public SolidColorBrush ForegroundActive
        {
            get { return (SolidColorBrush)GetValue(ForegroundActiveProperty); }
            set { SetValue(ForegroundActiveProperty, value); }
        }
        public static readonly DependencyProperty ForegroundActiveProperty =
            DependencyProperty.Register("ForegroundActive", typeof(SolidColorBrush), typeof(CMainPageButton),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))));
        #endregion
        #region Padding
        public new Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        public static new readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register("Padding", typeof(Thickness), typeof(CMainPageButton),
                new PropertyMetadata(new Thickness(10, 6, 10, 6)));
        #endregion
        #endregion

        #region Функции
        #region ChangeState
        private void ChangeState(bool active)
        {
            var storyboard = new Storyboard();
            var duration = TimeSpan.FromMilliseconds(200);
            var ease = new PowerEase() { EasingMode = EasingMode.EaseInOut };

            #region Анимация ширины декорации
            if (TextBlock.ActualWidth is not 0)
            {
                var decWidth = Decoration.ActualWidth is double.NaN ? Decoration.Width : Decoration.ActualWidth;

                var widthAnimation = new DoubleAnimation
                (
                    active ? Decoration.Width is double.NaN ? TextBlock.ActualWidth : decWidth : decWidth,
                    active ? TextBlock.ActualWidth + TextBlock.Margin.Right - TextBlock.Margin.Left : 0,
                    duration
                )
                {
                    EasingFunction = ease
                };

                Storyboard.SetTarget(widthAnimation, Decoration);
                Storyboard.SetTargetProperty(widthAnimation, new PropertyPath(WidthProperty));

                storyboard.Children.Add(widthAnimation);
            }
            else
            {
                Decoration.Width = active ? double.NaN : 0;
            }
            #endregion
            #region Анимация видимости декорации
            var opacityAnimation = new DoubleAnimation(active ? 1 : 0, AnimationHelper.AnimationDuration) { EasingFunction = ease };

            Storyboard.SetTarget(opacityAnimation, Decoration);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(OpacityProperty));

            storyboard.Children.Add(opacityAnimation);
            #endregion
            #region Анимация цвета текста
            var colorAnimation = new ColorAnimation
            (
                (TextBlock.Foreground as SolidColorBrush)!.Color,
                active ? ForegroundActive.Color : Foreground.Color,
                duration
            )
            {
                EasingFunction = ease
            };

            Storyboard.SetTarget(colorAnimation, TextBlock);
            Storyboard.SetTargetProperty(colorAnimation, new PropertyPath("(TextBlock.Foreground).(SolidColorBrush.Color)"));

            storyboard.Children.Add(colorAnimation);
            #endregion

            storyboard.Begin(Decoration, HandoffBehavior.SnapshotAndReplace, true);
        }
        #endregion
        #endregion

    }
}
