using Cls.Any;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
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

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Debug.WriteLine($"HoverHint.Text={e.NewValue}");
            Debug.WriteLine(AdornerLayer.GetAdornerLayer((UIElement)d) == null ? "NO LAYER" : "OK");
            if (d is not UIElement ui) return;

            ui.MouseEnter += (_, __) =>
            {
                var layer = GetWindowAdornerLayer(ui);
                if (layer == null) return;

                var hint = new HintAdorner(ui, GetText(ui));
                layer.Add(hint);
                ui.SetValue(_currentProperty, hint);
            };

            ui.MouseLeave += (_, __) =>
            {
                if (ui.GetValue(_currentProperty) is HintAdorner hint)
                {
                    GetWindowAdornerLayer(ui)?.Remove(hint);
                    ui.ClearValue(_currentProperty);
                }
            };
        }

        private static AdornerLayer? GetWindowAdornerLayer(UIElement owner)
        {
            var window = Window.GetWindow(owner);
            if (window == null) return null;

            if (window.Content is AdornerDecorator decorator)
                return decorator.AdornerLayer; 
            
            return AdornerLayer.GetAdornerLayer((Visual)window.Content);
        }

        private static readonly DependencyProperty _currentProperty =
            DependencyProperty.RegisterAttached("_current", typeof(Adorner),
                                                typeof(HoverHint));
    }

    sealed class HintAdorner : Adorner
    {
        private readonly FrameworkElement _visual;

        public HintAdorner(UIElement adorned, string text) : base(adorned)
        {
            _visual = BuildBubble(text);            
            AddVisualChild(_visual);
            IsHitTestVisible = false;
        }

        private static FrameworkElement BuildBubble(string text)
        {
            var bubble = new Border
            {
                Child = new TextBlock 
                {
                    Text = text,
                    FontFamily = (FontFamily)(Functions.GlobalResources()["fontfamily_main"]),
                    FontSize = 18,
                    Foreground = (SolidColorBrush)(Functions.GlobalResources()["textcolor_main"]),
                    Margin = new Thickness(10, 6, 10, 6)
                },
                //Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2E323C")),
                Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)),
                CornerRadius = new CornerRadius(8),
            };

            //var arrow = new Path
            //{
            //    Data = Geometry.Parse("M0,5 L8,0 L8,10 Z"),
            //    Fill = bubble.Background,
            //    Width = 8,
            //    Height = 10,
            //    Stretch = Stretch.Fill,
            //    VerticalAlignment = VerticalAlignment.Center
            //};

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            //Grid.SetColumn(arrow, 0);
            Grid.SetColumn(bubble, 1);
            //grid.Children.Add(arrow);
            grid.Children.Add(bubble);

            //grid.Effect = new DropShadowEffect
            //{
            //    BlurRadius = 10,
            //    ShadowDepth = 0,
            //    Color = Colors.Black,
            //    Opacity = 0.5
            //};

            return grid;
        }

        protected override int VisualChildrenCount => 1;
        protected override Visual GetVisualChild(int i) => _visual;
        protected override Geometry GetLayoutClip(Size layoutSlotSize) => null;

        protected override Size MeasureOverride(Size _)
        {
            _visual.Measure(new Size(double.PositiveInfinity,
                                     double.PositiveInfinity));

            double w = AdornedElement.RenderSize.Width + 10 + _visual.DesiredSize.Width;
            double h = Math.Max(AdornedElement.RenderSize.Height,
                                _visual.DesiredSize.Height);

            return new Size(w, h);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double x = AdornedElement.RenderSize.Width + 10;
            double y = (AdornedElement.RenderSize.Height - _visual.DesiredSize.Height) / 2;

            _visual.Arrange(new Rect(new Point(x, y), _visual.DesiredSize));
            return finalSize;
        }
    }
}