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
using System.Windows.Threading;

namespace Launcher.Components
{
    /// <summary>
    /// Логика взаимодействия для CScrollingTextBox.xaml
    /// </summary>
    public enum EScrollingType
    {
        Scrolling,
        None
    }

    public partial class CScrollingTextBlock : UserControl
    {
        public CScrollingTextBlock()
        {
            InitializeComponent();

            this.DataContext = this;
            this.SizeChanged += (e, b) =>
            {
                Dispatcher.BeginInvoke(UpdateViewModel, DispatcherPriority.Loaded);
            };
        }

        #region Переменные
        private Storyboard StoryBoardScrolling { get; set; }
        private float FadeWidth { get; set; } = 10;
        #endregion


        #region Свойства
        #region ScrollState
        public EScrollingType ScrollingType
        {
            get => (EScrollingType)GetValue(ScrollingTypeProperty);
            set => SetValue(ScrollingTypeProperty, value);
        }

        public static readonly DependencyProperty ScrollingTypeProperty =
            DependencyProperty.Register(nameof(ScrollingType), typeof(EScrollingType), typeof(CScrollingTextBlock),
                new(EScrollingType.None, OnScrollingTypeChanged));

        private static void OnScrollingTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (CScrollingTextBlock)d;
            sender.Dispatcher.BeginInvoke(sender.UpdateViewModel, DispatcherPriority.Loaded);
        }
        #endregion
        #region ScrollSpeed
        public double ScrollSpeed
        {
            get => (double)GetValue(ScrollSpeedProperty);
            set => SetValue(ScrollSpeedProperty, value);
        }

        public static readonly DependencyProperty ScrollSpeedProperty =
         DependencyProperty.Register(nameof(ScrollSpeed), typeof(double), typeof(CScrollingTextBlock),
             new PropertyMetadata(60.0, OnFontChanged));
        #endregion
        #region Padding
        public new Thickness Padding
        {
            get => (Thickness)GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }

        public new static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register(nameof(Padding), typeof(Thickness), typeof(CScrollingTextBlock),
                new(new Thickness(10, 0, 10, 0), OnFontChanged));
        #endregion
        #region Text
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(CScrollingTextBlock),
                new("", OnFontChanged));
        #endregion
        #region FontSize
        public new double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public new static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register(nameof(FontSize), typeof(double), typeof(CScrollingTextBlock),
                new (18.0, OnFontChanged));
        #endregion
        #region FontFamily
        public new FontFamily FontFamily
        {
            get => (FontFamily)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        public new static readonly DependencyProperty FontFamilyProperty =
            DependencyProperty.Register(nameof(FontFamily), typeof(FontFamily), typeof(CScrollingTextBlock),
                new(new FontFamily("Calibri"), OnFontChanged));
        #endregion
        #region FontWeight
        public new FontWeight FontWeight
        {
            get => (FontWeight)GetValue(FontWeightProperty);
            set => SetValue(FontWeightProperty, value);
        }

        public new static readonly DependencyProperty FontWeightProperty =
            DependencyProperty.Register(nameof(FontWeight), typeof(FontWeight), typeof(CScrollingTextBlock),
                new(FontWeights.Normal, OnFontChanged));
        #endregion
        #region FontStyle
        public new FontStyle FontStyle
        {
            get => (FontStyle)GetValue(FontStyleProperty);
            set => SetValue(FontStyleProperty, value);
        }

        public new static readonly DependencyProperty FontStyleProperty =
            DependencyProperty.Register(nameof(FontStyle), typeof(FontStyle), typeof(CScrollingTextBlock),
                new(FontStyles.Normal, OnFontChanged));
        #endregion

        #region OnFontChanged
        private static void OnFontChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (CScrollingTextBlock)d;
            sender.Dispatcher.BeginInvoke(sender.UpdateViewModel, DispatcherPriority.Loaded);
        }
        #endregion
        #endregion

        #region Функции
        #region UpdateViewModel
        private void UpdateViewModel()
        {
            if (ActualWidth is 0) return;

            var textSize = CalculateTextSize(Text, FontFamily, FontSize, FontWeight, FontStyle);

            #region Настройка canvas
            var totalWidth = textSize.Width + Padding.Left + Padding.Right;
            canvas_block.Height = textSize.Height;
            #region Если меньше ширины
            if (totalWidth <= ActualWidth)
            {
                ScrollingType = EScrollingType.None;

                canvas_block.HorizontalAlignment = HorizontalAlignment.Center;
                canvas_block.Width = textSize.Width;
                canvas_block.OpacityMask = null;

                CB_grid.Margin = new(0);

                tb_second.Visibility = Visibility.Collapsed;

                StopScrolling();
            }
            #endregion
            #region Если больше
            else
            {
                ScrollingType = EScrollingType.Scrolling;

                canvas_block.HorizontalAlignment = HorizontalAlignment.Stretch;
                canvas_block.Width = double.NaN;

                CB_grid.Margin = new(3, 0, 0, 0);

                tb_second.Visibility = Visibility.Visible;

                SetCanvasOpacityMask();

                StartScrolling();
            }
            #endregion
            #endregion
        }
        #endregion
        #region CalculateTextSize
        private Size CalculateTextSize(string text, FontFamily fontFamily, double fontSize, FontWeight fontWeight, FontStyle fontStyle)
        {
            var formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(fontFamily, fontStyle, fontWeight, FontStretches.Normal),
                fontSize,
                Brushes.Black,
                new NumberSubstitution(),
                TextFormattingMode.Display,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            return new(formattedText.Width, formattedText.Height);
        }
        #endregion
        #region UpdateCanvasOpacityMask
        private void SetCanvasOpacityMask()
        {
            if (canvas_block.ActualWidth is 0) return;

            var containerWidth = (float)canvas_block.ActualWidth;
            float fadeOffset = FadeWidth / containerWidth;

            var maskBrush = new LinearGradientBrush
            {
                MappingMode = BrushMappingMode.Absolute,
                StartPoint = new Point(0, 1),
                EndPoint = new Point(CB_grid.Margin.Left + CB_grid.ActualWidth, 1)
            };

            RenderOptions.SetBitmapScalingMode(maskBrush, BitmapScalingMode.HighQuality);

            maskBrush.GradientStops.Add(new (Colors.Transparent, 0));
            maskBrush.GradientStops.Add(new (Colors.Black, fadeOffset));
            maskBrush.GradientStops.Add(new (Colors.Black, 1 - fadeOffset));
            maskBrush.GradientStops.Add(new (Colors.Transparent, 1));

            canvas_block.OpacityMask = maskBrush;
        }
        #endregion
        #region StartScrolling
        public void StartScrolling()
        {
            if (ScrollingType is not EScrollingType.Scrolling) return;

            #region Сброс позиции
            var from = new Thickness(0, 0, 0, 0);
            CB_stack_panel.Margin = from;
            #endregion
            #region Создание и настройка анимации
            var duration = TimeSpan.FromSeconds(tb_first.ActualWidth / ScrollSpeed);            
            var to = new Thickness(-(tb_first.ActualWidth + tb_second.Margin.Left), 0, 0, 0);
            var animation = new ThicknessAnimationUsingKeyFrames();
            animation.KeyFrames.Add(new EasingThicknessKeyFrame(from, TimeSpan.FromMilliseconds(0)) { EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut } });
            animation.KeyFrames.Add(new EasingThicknessKeyFrame(to, duration) { EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut } });
            animation.KeyFrames.Add(new EasingThicknessKeyFrame(to, duration.Add(TimeSpan.FromSeconds(2))));
            animation.RepeatBehavior = RepeatBehavior.Forever;

            StoryBoardScrolling = new Storyboard();
            StoryBoardScrolling.Children.Add(animation);
            Storyboard.SetTarget(animation, CB_stack_panel);
            Storyboard.SetTargetProperty(animation, new PropertyPath(StackPanel.MarginProperty));
            #endregion
            #region Запуск
            StoryBoardScrolling.Begin(CB_stack_panel, HandoffBehavior.Compose, true);
            #endregion
        }
        #endregion
        #region StopScrolling
        public void StopScrolling()
        {
            if (StoryBoardScrolling != null)
            {
                StoryBoardScrolling.Stop();
                StoryBoardScrolling = null;
            }

            #region Сброс позиции
            var from = new Thickness(0, 0, 0, 0);
            CB_stack_panel.Margin = from;
            #endregion
        }
        #endregion
        #endregion
    }
}
