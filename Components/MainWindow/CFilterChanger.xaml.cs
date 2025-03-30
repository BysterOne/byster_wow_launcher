using Launcher.Any;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Launcher.Components.MainWindow
{
    public enum EFilterChangerType
    {
        Marker,
        Icon
    }
    /// <summary>
    /// Логика взаимодействия для CFilterChanger.xaml
    /// </summary>
    public partial class CFilterChanger : UserControl
    {
        public CFilterChanger()
        {
            InitializeComponent();

            this.DataContext = this;

            //MPTI_border.BorderBrush = GetMarkerFill();
            MouseDown += 
                (s, e) => 
                    IsActive = !IsActive;
        }

        #region Переменные
        public Enum Value { get; set; }
        #endregion

        #region Свойства
        #region Text
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(CFilterChanger),
                new PropertyMetadata(""));
        #endregion
        #region IsActive
        private bool _isActive = false;
        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; ChangeState(value); }
        }
        #endregion
        #region ChangerType
        public EFilterChangerType ChangerType
        {
            get { return (EFilterChangerType)GetValue(ChangerTypeProperty); }
            set { SetValue(ChangerTypeProperty, value); }
        }

        public static readonly DependencyProperty ChangerTypeProperty =
            DependencyProperty.Register("ChangerType", typeof(EFilterChangerType), typeof(CFilterChanger),
                new PropertyMetadata(EFilterChangerType.Marker, OnChangerTypeChanged));

        private static void OnChangerTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (d as CFilterChanger)!;

            sender.MP_type_marker.Visibility = sender.ChangerType is EFilterChangerType.Marker ? Visibility.Visible : Visibility.Collapsed;
            sender.MP_type_icon.Visibility = sender.ChangerType is EFilterChangerType.Icon ? Visibility.Visible : Visibility.Collapsed;

            sender.ChangeState(sender.IsActive);
        }
        #endregion
        #region Icon
        public ImageSource? Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(CFilterChanger),
                new PropertyMetadata(null));
        #endregion
        #region MarkerHeight
        public double MarkerHeight
        {
            get { return (double)GetValue(MarkerHeightProperty); }
            set { SetValue(MarkerHeightProperty, value); }
        }

        public static readonly DependencyProperty MarkerHeightProperty =
            DependencyProperty.Register("MarkerHeight", typeof(double), typeof(CFilterChanger),
                new PropertyMetadata((double)30, OnMarkerHeightChanged));

        private static void OnMarkerHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (d as CFilterChanger)!;
            var newHeight = (double)e.NewValue;            
        }
        #endregion
        #region MarkerWidth
        public double MarkerWidth
        {
            get { return (double)GetValue(MarkerWidthProperty); }
            set { SetValue(MarkerWidthProperty, value); }
        }

        public static readonly DependencyProperty MarkerWidthProperty =
            DependencyProperty.Register("MarkerWidth", typeof(double), typeof(CFilterChanger),
                new PropertyMetadata((double)30, OnMarkerWidthChanged));

        private static void OnMarkerWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (d as CFilterChanger)!;
            var newWidth = (double)e.NewValue;

            if (sender.ChangerType is EFilterChangerType.Marker)
            {
                //sender.MPTM_stroke.Width = newWidth;
                //sender.MPTM_fill.Width = newWidth - (sender.MPTM_stroke.StrokeThickness + 1) * 2;
            }
        }
        #endregion
        #endregion

        #region Функции
        #region ChangeState
        private void ChangeState(bool isActive)
        {
            var storyboard = new Storyboard();
            var duration = AnimationHelper.AnimationDuration;
            var ease = new PowerEase() { EasingMode = EasingMode.EaseInOut };

            if (ChangerType is EFilterChangerType.Marker)
            {
                var animation = new DoubleAnimation(isActive ? 1 : 0, duration) { EasingFunction = ease };
                Storyboard.SetTarget(animation, MPTM_fill);
                Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));
                storyboard.Children.Add(animation);

                storyboard.Begin(MPTM_fill, HandoffBehavior.SnapshotAndReplace, true);
            }
            else
            {
                var animation = new DoubleAnimation(isActive ? 1 : 0, duration) { EasingFunction = ease };
                Storyboard.SetTarget(animation, MPTI_brush);
                Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));
                storyboard.Children.Add(animation);

                storyboard.Begin(MPTI_brush, HandoffBehavior.SnapshotAndReplace, true);
            }           
        }
        #endregion
        #endregion
    }
}
