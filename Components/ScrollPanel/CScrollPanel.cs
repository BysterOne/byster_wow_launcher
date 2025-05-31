using Launcher.Any;
using Launcher.Components.ScrollPanelAny;
using System.ComponentModel;
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
        #region ScrollOrientation
        public enum ScrollOrientation { Vertical, Horizontal }
        #endregion
        #region ScrollPanelStep
        [TypeConverter(typeof(ScrollPanelStepConverter))]
        public readonly struct ScrollPanelStep
        {
            public double Value { get; }
            public bool IsAuto { get; }

            private ScrollPanelStep(double value, bool isAuto)
            {
                Value = value;
                IsAuto = isAuto;
            }

            public static readonly ScrollPanelStep Auto = new(double.NaN, true);

            public static implicit operator ScrollPanelStep(double val) => new ScrollPanelStep(val, false);

            public static explicit operator double(ScrollPanelStep ml)
            {
                if (ml.IsAuto)
                    throw new InvalidOperationException("Auto не имеет числового значения");
                return ml.Value;
            }
        }

        public sealed class ScrollPanelStepConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext ctx, Type srcType) =>
                srcType == typeof(string) || base.CanConvertFrom(ctx, srcType);

            public override object? ConvertFrom(ITypeDescriptorContext ctx,
                                                System.Globalization.CultureInfo culture,
                                                object value)
            {
                if (value is string s)
                {
                    s = s.Trim();
                    if (string.Equals(s, "Auto", StringComparison.OrdinalIgnoreCase))
                        return ScrollPanelStep.Auto;

                    if (double.TryParse(s.Replace(',', '.'), System.Globalization.NumberStyles.Float,
                                        System.Globalization.CultureInfo.InvariantCulture, out var d))
                        return (ScrollPanelStep)d;

                    throw new FormatException($"Не удалось разобрать ScrollPanelStep из «{s}».");
                }
                return base.ConvertFrom(ctx, culture, value);
            }
        }
        #endregion
    }

    public class CScrollPanel : ContentControl
    {
        static CScrollPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CScrollPanel), new FrameworkPropertyMetadata(typeof(CScrollPanel)));
        }

        #region Переменные
        private ScrollViewer? _scrollViewer;
        private ScrollBar? _scrollBar;
        private double _targetOffset;
        private bool _syncFromScroll;
        private bool _syncFromAnim;
        private double pxPerDelta = 80;
        #endregion

        #region Свойства
        #region ScrollStep
        public ScrollPanelStep ScrollStep
        {
            get => (ScrollPanelStep)GetValue(ScrollStepProperty);
            set => SetValue(ScrollStepProperty, value);
        }

        public static readonly DependencyProperty ScrollStepProperty =
            DependencyProperty.Register(nameof(ScrollStep), typeof(ScrollPanelStep), typeof(CScrollPanel), 
                new (ScrollPanelStep.Auto, OnScrollStepChanged));

        private static void OnScrollStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CScrollPanel p)
            {
                var newValue = (ScrollPanelStep)e.NewValue;
                if (!newValue.IsAuto)
                {
                    var steps = (int)(p.AnimationOffset / newValue.Value);
                    var newOffset = steps * newValue.Value;
                    p.AnimateToOffset(newOffset, 150);
                }
            }
        }
        #endregion
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

            if (p._syncFromScroll) return;

            p._syncFromAnim = true;

            double val = (double)e.NewValue;

            if (p.Orientation == ScrollOrientation.Vertical)
                p._scrollViewer.ScrollToVerticalOffset(val);
            else
                p._scrollViewer.ScrollToHorizontalOffset(val);

            p._syncFromAnim = false;
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
                _scrollViewer.ScrollChanged -= OnScrollViewerScrollChanged;
            }

            _scrollViewer = GetTemplateChild("scroll_viewer") as ScrollViewer;

            if (_scrollViewer != null)
            {
                _scrollViewer.MouseEnter += OnScrollViewerMouseEnter;
                _scrollViewer.MouseLeave += OnScrollViewerMouseLeave;
                _scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            }

            UpdateOrientation();
        }
        #endregion
        #region AnimateToOffset
        private void AnimateToOffset(double to, double ms)
        {
            if (_scrollViewer == null) return;

            var sb = new Storyboard { FillBehavior = FillBehavior.HoldEnd };

            Debug.WriteLine($"Scroll to: {to}");

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
            HookThumbEvents();

            if (_scrollBar != null) _scrollBar.Opacity = 0;
            _targetOffset = 0;
        }
        #endregion
        #region GetScrollBar
        private static ScrollBar? GetScrollBar(DependencyObject? parent, Orientation orientation)
        {
            if (parent == null) return null;

            if (parent is FrameworkElement fe) fe.ApplyTemplate();

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
        #region FindThumb
        private static Thumb? FindThumb(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is Thumb t) return t;

                var found = FindThumb(child);
                if (found != null) return found;
            }
            return null;
        }
        #endregion
        #region HookThumbEvents
        private void HookThumbEvents()
        {
            if (_scrollBar == null) return;

            _scrollBar.RemoveHandler(Thumb.DragCompletedEvent,
                new DragCompletedEventHandler(OnThumbDragCompleted));

            if (VisualTreeHelper.GetChildrenCount(_scrollBar) == 0)
                _scrollBar.ApplyTemplate();

            Thumb? thumb = FindThumb(_scrollBar);
            if (thumb != null)
            {
                thumb.AddHandler(Thumb.DragCompletedEvent,
                    new DragCompletedEventHandler(OnThumbDragCompleted), true);
            }
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
            AnimationHelper.OpacityAnimationStoryBoard(_scrollBar, to).Begin(_scrollBar, HandoffBehavior.SnapshotAndReplace, true);
        }
        #endregion
        #region CalcNewStepOffset
        private static double CalcNewStepOffset(double step, double offset)
        {
            return (int)(offset / step) * step;
        }
        #endregion
        #region ToLastStep
        private static double ToLastStep(double step, double offset)
        {
            return offset - (int)(offset / step) * step;
        }
        #endregion
        #region ToNextStep
        private static double ToNextStep(double step, double offset)
        {
            return offset - ((int)(offset / step) + 1) * step;
        }
        #endregion
        #region OnPreviewMouseWheel
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (_scrollViewer == null) return;
            e.Handled = true;

            double toLastStep = ToLastStep(ScrollStep.Value, _targetOffset);
            double delta = 
                ScrollStep.IsAuto ?
                    e.Delta / 120.0 * pxPerDelta :
                    Math.CopySign(ScrollStep.Value, e.Delta);
            double newTargetOffset =
                ScrollStep.IsAuto ?
                    _targetOffset - delta :
                    toLastStep == 0 ?
                        _targetOffset - delta :
                        _targetOffset - (Math.Sign(e.Delta) is 1 ? toLastStep : delta);

            if (Orientation == ScrollOrientation.Vertical)
            {
                _targetOffset = Math.Clamp(
                    newTargetOffset,
                    0,
                    _scrollViewer.ScrollableHeight);
            }
            else
            {
                _targetOffset = Math.Clamp(
                    newTargetOffset,
                    0,
                    _scrollViewer.ScrollableWidth);
            }

            AnimateToOffset(_targetOffset, 150);
        }
        #endregion        
        #region OnThumbDragCompleted
        private void OnThumbDragCompleted(object? sender, DragCompletedEventArgs e)
        {
            if (_scrollViewer == null) return;

            double currentOffset =
                 Orientation == ScrollOrientation.Vertical ?
                 _scrollViewer.VerticalOffset :
                 _scrollViewer.HorizontalOffset;

            _targetOffset = currentOffset;

            if (ScrollStep.IsAuto) return;

            double toLastStep = ToLastStep(ScrollStep.Value, currentOffset);
            double toNextStep = ToNextStep(ScrollStep.Value, currentOffset);
            var delta = Math.Abs(toNextStep) < toLastStep ? toNextStep : toLastStep;
            double newTargetOffset = currentOffset - delta;

            double limit = Orientation == ScrollOrientation.Vertical
                ? _scrollViewer.ScrollableHeight
                : _scrollViewer.ScrollableWidth;

            if (currentOffset == limit) return;
            _targetOffset = Math.Clamp(newTargetOffset, 0, limit);
            

            AnimateToOffset(_targetOffset, 150);
        }
        #endregion
        #region OnScrollViewerScrollChanged
        private void OnScrollViewerScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            if (_syncFromAnim) return;

            double offset = Orientation == ScrollOrientation.Vertical
                ? e.VerticalOffset
                : e.HorizontalOffset;

            if (!AnimationOffset.Equals(offset))
            {
                _syncFromScroll = true;
                SetCurrentValue(AnimationOffsetProperty, offset);
                _syncFromScroll = false;
            }
        }
        #endregion
        #endregion
    }
}
