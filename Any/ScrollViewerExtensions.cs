using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace Launcher.Any
{
    public class ScrollViewerExtensions
    {
        public static readonly DependencyProperty ScrollFactorProperty =
        DependencyProperty.RegisterAttached(
            "ScrollFactor",
            typeof(double),
            typeof(ScrollViewerExtensions),
            new PropertyMetadata(1.0, OnScrollFactorChanged));

        public static void SetScrollFactor(DependencyObject obj, double value) =>
            obj.SetValue(ScrollFactorProperty, value);
        public static double GetScrollFactor(DependencyObject obj) =>
            (double)obj.GetValue(ScrollFactorProperty);

        private static void OnScrollFactorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer sv)
            {
                sv.PreviewMouseWheel -= Sv_PreviewMouseWheel;
                sv.PreviewMouseWheel += Sv_PreviewMouseWheel;
            }
        }

        private static void Sv_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer sv)
            {
                double factor = GetScrollFactor(sv);
                double lines = e.Delta / Mouse.MouseWheelDeltaForOneLine;
                double offset = sv.VerticalOffset - lines * factor * Mouse.MouseWheelDeltaForOneLine;
                sv.ScrollToVerticalOffset(offset);
                e.Handled = true;
            }
        }
    }
}
