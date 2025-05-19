using Cls.Any;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Launcher.Any
{
    public enum HoverHintPlacement
    {
        Top,
        Left,
        Right,
        Bottom
    }

    public sealed class HoverHint
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached(
                "Text", typeof(string), typeof(HoverHint),
                new PropertyMetadata(null, OnTextChanged));

        public static string GetText(DependencyObject o) => (string)o.GetValue(TextProperty);
        public static void SetText(DependencyObject o, string v) => o.SetValue(TextProperty, v);

        public static readonly DependencyProperty PlacementProperty =
            DependencyProperty.RegisterAttached(
                "Placement", typeof(HoverHintPlacement), typeof(HoverHint),
                new PropertyMetadata(HoverHintPlacement.Right));

        public static HoverHintPlacement GetPlacement(DependencyObject o) => (HoverHintPlacement)o.GetValue(PlacementProperty);
        public static void SetPlacement(DependencyObject o, HoverHintPlacement value) => o.SetValue(PlacementProperty, value);

        private static readonly DependencyProperty _currentHintProperty =
            DependencyProperty.RegisterAttached("_currentHint", typeof(HintAdorner), typeof(HoverHint));

        private static readonly DependencyProperty _subscribedProperty =
            DependencyProperty.RegisterAttached("_sub", typeof(bool), typeof(HoverHint), new PropertyMetadata(false));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs _)
        {
            if (d is not UIElement ui) return;

            if ((bool)ui.GetValue(_subscribedProperty) == false)
            {
                ui.MouseEnter += OnEnter;
                ui.MouseLeave += OnLeave;
                ui.SetValue(_subscribedProperty, true);
            }
        }

        private static void OnEnter(object? sender, MouseEventArgs e)
        {
            var ui = (UIElement)sender!;
            var text = GetText(ui);
            if (string.IsNullOrEmpty(text)) return;

            if (ui.GetValue(_currentHintProperty) is HintAdorner hExisting)
            {
                hExisting.StopAnimation();
                hExisting.Show();
                return;
            }

            var layer = GetWindowLayer(ui);
            if (layer == null) return;

            var placement = GetPlacement(ui);
            var hint = new HintAdorner(ui, text, placement);
            layer.Add(hint);
            ui.SetValue(_currentHintProperty, hint);

            hint.Show();
        }

        private static void OnLeave(object? sender, MouseEventArgs e)
        {
            var ui = (UIElement)sender!;
            if (ui.GetValue(_currentHintProperty) is not HintAdorner hint) return;

            hint.Hide(() =>
            {
                GetWindowLayer(ui)?.Remove(hint);
                ui.ClearValue(_currentHintProperty);
            });
        }

        private static AdornerLayer? GetWindowLayer(UIElement owner)
        {
            var window = Window.GetWindow(owner);
            return window?.Content is Visual root
                ? AdornerLayer.GetAdornerLayer(root)
                : null;
        }

        private sealed class HintAdorner : Adorner
        {
            private readonly FrameworkElement _bubble;
            private readonly TranslateTransform _shift = new();
            private readonly HoverHintPlacement _placement;

            private const double Gap = 20;
            private const double AnimationOffset = 10;
            private bool _shown = false;

            // Анимации как поля!
            private static readonly Duration Dur = TimeSpan.FromMilliseconds(180);

            private readonly DoubleAnimation _fadeIn = new(1, Dur) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            private readonly DoubleAnimation _fadeOut = new(0, Dur) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn } };
            private readonly DoubleAnimation _slideIn = new() { Duration = Dur, EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            private readonly DoubleAnimation _slideOut = new() { Duration = Dur, EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn } };

            private int _axis; // 0=X, 1=Y

            public HintAdorner(UIElement adorned, string text, HoverHintPlacement placement) : base(adorned)
            {
                _placement = placement;
                _bubble = BuildBubble(text);
                _bubble.RenderTransform = _shift;

                AddVisualChild(_bubble);
                IsHitTestVisible = false;
                Opacity = 0;
            }

            #region Visual Placement
            protected override int VisualChildrenCount => 1;
            protected override Visual GetVisualChild(int _) => _bubble;

            protected override Size MeasureOverride(Size _)
            {
                _bubble.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                var w = AdornedElement.RenderSize.Width + Gap + _bubble.DesiredSize.Width;
                var h = Math.Max(AdornedElement.RenderSize.Height, _bubble.DesiredSize.Height);
                return new Size(w, h);
            }

            private void OnFirstLayout(object? sender, EventArgs e)
            {
                this.LayoutUpdated -= OnFirstLayout;
                _shown = true;
                StartShowAnimation();
            }

            private double _x, _y;

            protected override Size ArrangeOverride(Size fin)
            {
                double x = 0, y = 0;
                switch (_placement)
                {
                    case HoverHintPlacement.Right:
                        x = AdornedElement.RenderSize.Width + Gap;
                        y = (AdornedElement.RenderSize.Height - _bubble.DesiredSize.Height) / 2;
                        _axis = 0;
                        break;
                    case HoverHintPlacement.Left:
                        x = -_bubble.DesiredSize.Width - Gap;
                        y = (AdornedElement.RenderSize.Height - _bubble.DesiredSize.Height) / 2;
                        _axis = 0;
                        break;
                    case HoverHintPlacement.Top:
                        x = (AdornedElement.RenderSize.Width - _bubble.DesiredSize.Width) / 2;
                        y = -_bubble.DesiredSize.Height - Gap;
                        _axis = 1;
                        break;
                    case HoverHintPlacement.Bottom:
                        x = (AdornedElement.RenderSize.Width - _bubble.DesiredSize.Width) / 2;
                        y = AdornedElement.RenderSize.Height + Gap;
                        _axis = 1;
                        break;
                }
                _x = x;
                _y = y;
                _bubble.Arrange(new Rect(new Point(x, y), _bubble.DesiredSize));
                return fin;
            }

            protected override Geometry GetLayoutClip(Size _) => null!;
            #endregion

            #region Animations

            public void Show()
            {
                if (!_shown)
                {
                    this.LayoutUpdated += OnFirstLayout;
                }
                else
                {
                    StartShowAnimation();
                }
            }

            private void StartShowAnimation()
            {
                // Настраиваем анимации для нужного направления
                switch (_placement)
                {
                    case HoverHintPlacement.Right:
                        _slideIn.From = AnimationOffset; _slideIn.To = 0;
                        _slideOut.From = 0; _slideOut.To = AnimationOffset;
                        break;
                    case HoverHintPlacement.Left:
                        _slideIn.From = -AnimationOffset; _slideIn.To = 0;
                        _slideOut.From = 0; _slideOut.To = -AnimationOffset;
                        break;
                    case HoverHintPlacement.Top:
                        _slideIn.From = -AnimationOffset; _slideIn.To = 0;
                        _slideOut.From = 0; _slideOut.To = -AnimationOffset;
                        break;
                    case HoverHintPlacement.Bottom:
                        _slideIn.From = AnimationOffset; _slideIn.To = 0;
                        _slideOut.From = 0; _slideOut.To = AnimationOffset;
                        break;
                }

                BeginAnimation(OpacityProperty, _fadeIn, HandoffBehavior.SnapshotAndReplace);

                if (_axis == 0) // X
                {
                    _shift.BeginAnimation(TranslateTransform.XProperty, _slideIn, HandoffBehavior.SnapshotAndReplace);
                    _shift.BeginAnimation(TranslateTransform.YProperty, null);
                }
                else // Y
                {
                    _shift.BeginAnimation(TranslateTransform.YProperty, _slideIn, HandoffBehavior.SnapshotAndReplace);
                    _shift.BeginAnimation(TranslateTransform.XProperty, null);
                }
            }

            public void Hide(Action onDone)
            {
                _fadeOut.Completed += One;
                BeginAnimation(OpacityProperty, _fadeOut, HandoffBehavior.SnapshotAndReplace);

                if (_axis == 0)
                    _shift.BeginAnimation(TranslateTransform.XProperty, _slideOut, HandoffBehavior.SnapshotAndReplace);
                else
                    _shift.BeginAnimation(TranslateTransform.YProperty, _slideOut, HandoffBehavior.SnapshotAndReplace);

                void One(object? s, System.EventArgs e)
                {
                    _fadeOut.Completed -= One;
                    onDone();
                }
            }

            public void StopAnimation()
            {
                BeginAnimation(OpacityProperty, null);
                _shift.BeginAnimation(TranslateTransform.XProperty, null);
                _shift.BeginAnimation(TranslateTransform.YProperty, null);
            }
            #endregion

            #region Visual Style
            private static FrameworkElement BuildBubble(string text)
            {
                var bubble = new Border
                {
                    Child = new TextBlock
                    {
                        Text = text,
                        FontSize = 18,
                        FontFamily = (FontFamily)Functions.GlobalResources()["fontfamily_main"],
                        Foreground = (Brush)Functions.GlobalResources()["textcolor_main"],
                        Margin = new Thickness(11, 7, 11, 7)
                    },
                    Background = new SolidColorBrush(Color.FromArgb(230, 0, 0, 0)),
                    CornerRadius = new CornerRadius(8)
                };
                return bubble;
            }
            #endregion
        }
    }
}
