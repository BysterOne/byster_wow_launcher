using Launcher.Any;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Launcher.Components
{
    public class CScrollPanel : ContentControl
    {
        static CScrollPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CScrollPanel), new FrameworkPropertyMetadata(typeof(CScrollPanel)));
        }

        #region Компоненты
        private ScrollViewer? _scrollViewer { get; set; } = null;
        private ScrollBar? _verticalBar { get; set; } = null;
        private double _targetOffset { get; set; } = 0;
        #endregion

        #region Свойства
        #region AnimationOffset
        public static readonly DependencyProperty AnimationOffsetProperty =
            DependencyProperty.Register(
                nameof(AnimationOffset),
                typeof(double),
                typeof(CScrollPanel),
                new PropertyMetadata(0d, OnAnimationOffsetChanged));

        public double AnimationOffset
        {
            get => (double)GetValue(AnimationOffsetProperty);
            set => SetValue(AnimationOffsetProperty, value);
        }

        private static void OnAnimationOffsetChanged(
                DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CScrollPanel p && p._scrollViewer != null)
                p._scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
        }
        #endregion
        #endregion

        #region Функции       
        #region ScrollTo
        public void ScrollTo(int y)
        {
            var scroll_viewer = (ScrollViewer)GetTemplateChild("scroll_viewer");
            scroll_viewer.ScrollToVerticalOffset(y);
        }
        #endregion
        #region OnApplyTemplate
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_scrollViewer != null)
            {
                _scrollViewer.MouseEnter -= OnScrollViewerMouseEnter;
                _scrollViewer.MouseLeave -= OnScrollViewerMouseLeave;
            }

            _scrollViewer = GetTemplateChild("scroll_viewer") as ScrollViewer;

            if (_scrollViewer != null)
            {
                _scrollViewer.MouseEnter += OnScrollViewerMouseEnter;
                _scrollViewer.MouseLeave += OnScrollViewerMouseLeave;
                                
                _verticalBar = GetScrollBar(_scrollViewer, Orientation.Vertical);
                if (_verticalBar != null) _verticalBar.Opacity = 0;
            }
        }
        #endregion
        #region AnimateToOffset
        private void AnimateToOffset(double to, double ms)
        {
            if (_scrollViewer == null) return;

            var storyboard = new Storyboard
            {
                FillBehavior = FillBehavior.HoldEnd
            };

            var anim = new DoubleAnimation
            {
                To = to,
                Duration = TimeSpan.FromMilliseconds(ms),
                EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(anim, this);
            Storyboard.SetTargetProperty(anim,
                new PropertyPath(AnimationOffsetProperty));

            storyboard.Children.Add(anim);

            storyboard.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
        }
        #endregion
        #endregion

        #region Обработчики событий
        #region OnScrollViewerMouseEnter
        private void OnScrollViewerMouseEnter(object? s, MouseEventArgs e) =>
            FadeScrollbar(1);
        #endregion
        #region OnScrollViewerMouseLeave
        private void OnScrollViewerMouseLeave(object? s, MouseEventArgs e) =>
            FadeScrollbar(0);
        #endregion        
        #region FadeScrollbar
        private void FadeScrollbar(double to)
        {
            if (_verticalBar == null) return;
            AnimationHelper.OpacityAnimation(_verticalBar, to).Begin(_verticalBar, HandoffBehavior.SnapshotAndReplace, true);
        }
        #endregion
        #region OnPreviewMouseWheel
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (_scrollViewer == null) return;

            //e.Handled = true;

            //const double pixelsPerDelta = 150;
            //double target = _scrollViewer.VerticalOffset - e.Delta / 120.0 * pixelsPerDelta;
            //target = Math.Max(0, Math.Min(target, _scrollViewer.ScrollableHeight));

            //double distance = Math.Abs(target - (double)GetValue(AnimationOffsetProperty));
            //double ms = Math.Clamp(distance * 0.2, 100, 400);

            //AnimateToOffset(target, ms);

            const double pxPerDelta = 110;
            _targetOffset -= e.Delta / 120.0 * pxPerDelta;
            _targetOffset = Math.Clamp(_targetOffset, 0, _scrollViewer.ScrollableHeight);

            AnimateToOffset(_targetOffset, 150);
        }
        #endregion
        #region GetScrollBar
        private static ScrollBar? GetScrollBar(DependencyObject? parent, Orientation orientation)
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is ScrollBar sb && sb.Orientation == orientation)
                {
                    return sb;
                }

                var result = GetScrollBar(child, orientation);
                if (result != null) return result;
            }
            return null;
        }
        #endregion
        #endregion
    }
}
