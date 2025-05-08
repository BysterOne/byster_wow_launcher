using Cls.Any;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Launcher.Any
{
    public sealed class HoverHint
    {
        public static readonly DependencyProperty TextProperty =
        DependencyProperty.RegisterAttached(
            "Text", typeof(string), typeof(HoverHint),
            new PropertyMetadata(null, OnTextChanged));

        public static string GetText(DependencyObject o) => (string)o.GetValue(TextProperty);
        public static void SetText(DependencyObject o, string v) => o.SetValue(TextProperty, v);

        
        private static readonly DependencyProperty _currentHintProperty =
            DependencyProperty.RegisterAttached("_currentHint", typeof(HintAdorner),
                                                typeof(HoverHint));

       
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

            var hint = new HintAdorner(ui, text);
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
        
        private static readonly DependencyProperty _subscribedProperty =
            DependencyProperty.RegisterAttached("_sub", typeof(bool),
                                                typeof(HoverHint), new PropertyMetadata(false));

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

            public HintAdorner(UIElement adorned, string text) : base(adorned)
            {
                _bubble = BuildBubble(text); 
                _bubble.RenderTransform = _shift;

                AddVisualChild(_bubble);
                IsHitTestVisible = false;
                Opacity = 0;
            }

            #region Слой и его отображение
            protected override int VisualChildrenCount => 1;
            protected override Visual GetVisualChild(int _) => _bubble;

            protected override Size MeasureOverride(Size _)
            {
                _bubble.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                var w = AdornedElement.RenderSize.Width + 10 + _bubble.DesiredSize.Width;
                var h = Math.Max(AdornedElement.RenderSize.Height, _bubble.DesiredSize.Height);
                return new Size(w, h);
            }

            protected override Size ArrangeOverride(Size fin)
            {
                double x = AdornedElement.RenderSize.Width + 10;
                double y = (AdornedElement.RenderSize.Height - _bubble.DesiredSize.Height) / 2;
                _bubble.Arrange(new Rect(new Point(x, y), _bubble.DesiredSize));
                return fin;
            }

            protected override Geometry GetLayoutClip(Size _) => null;
            #endregion

            #region Анимации
            private static readonly Duration Dur = TimeSpan.FromMilliseconds(180);

            private readonly DoubleAnimation _fadeIn = new(1, Dur) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            private readonly DoubleAnimation _fadeOut = new(0, Dur) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn } };
            private readonly DoubleAnimation _slideIn = new(0, Dur) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            private readonly DoubleAnimation _slideOut = new(10, Dur) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn } };

            public void Show()
            {
                _slideIn.From = 10;  // справа‑налево
                BeginAnimation(OpacityProperty, _fadeIn, HandoffBehavior.SnapshotAndReplace);
                _shift.BeginAnimation(TranslateTransform.XProperty, _slideIn, HandoffBehavior.SnapshotAndReplace);
            }

            public void Hide(Action onDone)
            {
                _fadeOut.Completed += One;
                BeginAnimation(OpacityProperty, _fadeOut, HandoffBehavior.SnapshotAndReplace);
                _shift.BeginAnimation(TranslateTransform.XProperty, _slideOut, HandoffBehavior.SnapshotAndReplace);

                void One(object? s, EventArgs e)
                {
                    _fadeOut.Completed -= One;
                    onDone();
                }
            }

            public void StopAnimation()
            {
                BeginAnimation(OpacityProperty, null);
                _shift.BeginAnimation(TranslateTransform.XProperty, null);
            }
            #endregion

            #region Конструктор и стили
            private static FrameworkElement BuildBubble(string text)
            {
                var bubble = new Border
                {
                    Child = new TextBlock
                    {
                        Text = text,
                        FontFamily = (FontFamily)Functions.GlobalResources()["fontfamily_main"],
                        FontSize = 18,
                        Foreground = (Brush)Functions.GlobalResources()["textcolor_main"],
                        Margin = new Thickness(10, 6, 10, 6)
                    },
                    Background = new SolidColorBrush(Color.FromArgb(230, 0, 0, 0)),
                    CornerRadius = new CornerRadius(8)
                };

                /* если стрелка не нужна – можно вернуть сразу bubble */
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                Grid.SetColumn(bubble, 1);
                grid.Children.Add(bubble);
                return grid;         // корневой визуал
            }
            #endregion
        }
    }
}