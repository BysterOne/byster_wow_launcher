using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Launcher.Components
{
    /// <summary>
    /// Логика взаимодействия для CButton.xaml
    /// </summary>
    public partial class CButton : UserControl
    {
        public CButton()
        {
            InitializeComponent();
            this.Focusable = true;
            this.DataContext = this;
            this.PreviewMouseLeftButtonDown += ThisClicked;
        }

        #region Параметры
        #region FontSize
        public static readonly new DependencyProperty FontSizeProperty 
            = DependencyProperty.Register( "FontSize", typeof(double), typeof(CButton),
                new PropertyMetadata(18.0));

        public new double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }
        #endregion
        #region BorderRadius
        public double BorderRadius
        {
            get { return (double)GetValue(BorderRadiusProperty); }
            set { SetValue(BorderRadiusProperty, value); }
        }
        public static readonly DependencyProperty BorderRadiusProperty =
            DependencyProperty.Register("BorderRadius", typeof(double), typeof(CButton),
                new PropertyMetadata((double)25));
        #endregion
        #region Gap
        public double Gap
        {
            get { return (double)GetValue(GapProperty); }
            set { SetValue(GapProperty, value); }
        }
        public static readonly DependencyProperty GapProperty =
            DependencyProperty.Register("Gap", typeof(double), typeof(CButton),
                new PropertyMetadata((double)0, OnGapChanged));

        private static void OnGapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CButton)d;
            control.image.Margin =
                new Thickness(control.image.Margin.Left, control.image.Margin.Top, (double)e.NewValue, control.image.Margin.Bottom);
        }
        #endregion
        #region Text
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(CButton),
                new PropertyMetadata("button", OnTextChanged));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CButton)d;
            var newValue = e.NewValue as string;
            control.text.Visibility = String.IsNullOrWhiteSpace(newValue) ? Visibility.Collapsed : Visibility.Visible;
        }
        #endregion
        #region Background
        public Brush BackColor
        {
            get { return (Brush)GetValue(BackColorProperty); }
            set { SetValue(BackColorProperty, value); }
        }
        public static readonly DependencyProperty BackColorProperty =
            DependencyProperty.Register("BackColor", typeof(Brush), typeof(CButton),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb((byte)(255 * 0.15), 255, 255, 255))));
        #endregion
        #region Foreground
        public new Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }
        public static new readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(Brush), typeof(CButton),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))));
        #endregion
        #region Icon
        public ImageSource? Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set
            {
                image.Visibility = value is null ? Visibility.Collapsed : Visibility.Visible;
                SetValue(IconProperty, value);
            }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(CButton),
                new PropertyMetadata(null, OnIconChanged));
        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CButton)d;
            var newValue = (ImageSource?)e.NewValue;

            control.image.Visibility = newValue == null ? Visibility.Collapsed : Visibility.Visible;
        }
        #endregion
        #region IconHeight
        public double IconHeight
        {
            get { return (double)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }
        public static readonly DependencyProperty IconHeightProperty =
            DependencyProperty.Register("IconHeight", typeof(double), typeof(CButton),
                new PropertyMetadata((double)30));
        #endregion
        #region IconWidth
        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }
        public static readonly DependencyProperty IconWidthProperty =
            DependencyProperty.Register("IconWidth", typeof(double), typeof(CButton),
                new PropertyMetadata((double)30));
        #endregion
        #region Padding
        public new Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }
        public static readonly new DependencyProperty PaddingProperty =
            DependencyProperty.Register("Padding", typeof(Thickness), typeof(CButton),
                new PropertyMetadata(new Thickness(15, 8, 15, 8)));
        #endregion
        #endregion

        #region События
        private void N_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var grid = sender as Grid;
            var background_rectangles = grid!.Children.OfType<Rectangle>().ToList();
            if (background_rectangles.Count == 0) return;
            var back_rect = background_rectangles.First();
            ChangeOpacity(back_rect, 0);
        }
        private void N_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var grid = sender as Grid;
            var background_rectangles = grid!.Children.OfType<Rectangle>().ToList();
            if (background_rectangles.Count == 0) return;
            var back_rect = background_rectangles.First();
            ChangeOpacity(back_rect, 0.2);
        }
        private void ThisClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Focus();
        }
        #endregion


        #region Анимация
        #region ChangeOpacity
        public void ChangeOpacity(UIElement element, double opacity)
        {
            Dispatcher.BeginInvoke(() =>
            {
                var dur_ = new Duration(TimeSpan.FromMilliseconds(200));
                var ease_ = new PowerEase() { EasingMode = EasingMode.EaseInOut };
                var anim_ = new DoubleAnimation(opacity, dur_) { EasingFunction = ease_ };
                element.BeginAnimation(OpacityProperty, anim_);
            });
        }
        #endregion
        #endregion       
    }
}
