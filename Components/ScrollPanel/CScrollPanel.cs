using Launcher.Any;
using Launcher.Components.ScrollPanelAny;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Launcher.Components
{
    namespace ScrollPanelAny
    {
        public enum ScrollOrientation { Vertical, Horizontal }
    }

    public class CScrollPanel : ContentControl
    {
        static CScrollPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CScrollPanel), new FrameworkPropertyMetadata(typeof(CScrollPanel)));
        }

        #region Компоненты
        private ScrollViewer? _scrollViewer;
        private ScrollBar? _scrollBar;
        private double _targetOffset;
        #endregion

        #region Свойства
        #region AnimationOffset
        public double AnimationOffset
        {
            get => (double)GetValue(AnimationOffsetProperty);
            set => SetValue(AnimationOffsetProperty, value);
        }

        public static readonly DependencyProperty AnimationOffsetProperty =
            DependencyProperty.Register(nameof(AnimationOffset), typeof(double), typeof(CScrollPanel),
                new PropertyMetadata(0d, OnAnimationOffsetChanged));
        private static void OnAnimationOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not CScrollPanel p || p._scrollViewer is null) return;

            double val = (double)e.NewValue;
            if (p.Orientation == ScrollOrientation.Vertical) p._scrollViewer.ScrollToVerticalOffset(val);
            else p._scrollViewer.ScrollToHorizontalOffset(val);
        }
        #endregion
        #region Orientation
        public ScrollOrientation Orientation
        {
            get => (ScrollOrientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(nameof(Orientation), typeof(ScrollOrientation), typeof(CScrollPanel),
                new PropertyMetadata(ScrollOrientation.Vertical, OnOrientationChanged));     

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs _)
        {
            if (d is CScrollPanel p) p.UpdateOrientation();
        }
        #endregion
        #endregion

        #region Функции       
        #region ScrollTo
        public void ScrollTo(double offset)
        {
            if (Orientation == ScrollOrientation.Vertical)
                _scrollViewer?.ScrollToVerticalOffset(offset);
            else
                _scrollViewer?.ScrollToHorizontalOffset(offset);
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
            }

            UpdateOrientation();
        }
        #endregion
        #region AnimateToOffset
        private void AnimateToOffset(double to, double ms)
        {
            if (_scrollViewer == null) return;

            var sb = new Storyboard { FillBehavior = FillBehavior.HoldEnd };

            var anim = new DoubleAnimation
            {
                To = to,
                Duration = TimeSpan.FromMilliseconds(ms),
                EasingFunction = new PowerEase { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(anim, this);
            Storyboard.SetTargetProperty(anim, new PropertyPath(AnimationOffsetProperty));

            sb.Children.Add(anim);
            sb.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
        }
        #endregion
        #region UpdateOrientation
        private void UpdateOrientation()
        {
            if (_scrollViewer is null) return;

            if (Orientation == ScrollOrientation.Vertical)
            {
                _scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                _scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                _scrollBar = GetScrollBar(_scrollViewer, System.Windows.Controls.Orientation.Vertical);
            }
            else
            {
                _scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                _scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                _scrollBar = GetScrollBar(_scrollViewer, System.Windows.Controls.Orientation.Horizontal);
            }

            if (_scrollBar != null) _scrollBar.Opacity = 0;
            _targetOffset = 0;
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
            if (_scrollBar == null) return;
            AnimationHelper.OpacityAnimation(_scrollBar, to).Begin(_scrollBar, HandoffBehavior.SnapshotAndReplace, true);
        }
        #endregion
        #region OnPreviewMouseWheel
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (_scrollViewer == null) return;

            e.Handled = true;

            const double pxPerDelta = 80;
            double delta = e.Delta / 120.0 * pxPerDelta;

            if (Orientation == ScrollOrientation.Vertical)
            {
                _targetOffset = Math.Clamp(
                    _targetOffset - delta,
                    0,
                    _scrollViewer.ScrollableHeight);
            }
            else
            {
                _targetOffset = Math.Clamp(
                    _targetOffset - delta,
                    0,
                    _scrollViewer.ScrollableWidth);
            }

            Debug.WriteLine($"{e.Delta} → {_targetOffset}");
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
                    return sb;

                var result = GetScrollBar(child, orientation);
                if (result != null) return result;
            }

            return null;
        }
        #endregion
        #endregion
    }
}
