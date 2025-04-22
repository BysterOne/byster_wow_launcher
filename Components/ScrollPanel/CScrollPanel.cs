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
        private ScrollBar? VerticalScrollBar { get; set; } = null;
        #endregion

        #region Функции
        #region ScrollTo
        public void ScrollTo(int y)
        {
            var scroll_viewer = (ScrollViewer)GetTemplateChild("scroll_viewer");
            scroll_viewer.ScrollToVerticalOffset(y);
        }
        #endregion
        #endregion

        #region События
        #region scroll_viewer_MouseEnter
        private void scroll_viewer_MouseEnter(object sender, MouseEventArgs e)
        {
            var scroll_viewer = (ScrollViewer)GetTemplateChild("scroll_viewer");
            VerticalScrollBar = GetScrollBar(scroll_viewer, Orientation.Vertical);
            if (VerticalScrollBar is null) return;

            DoubleAnimation fadeIn = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(200));
            VerticalScrollBar.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }
        #endregion
        #region scroll_viewer_MouseLeave
        private void scroll_viewer_MouseLeave(object sender, MouseEventArgs e) => HideScrollBar();
        #endregion
        #endregion

        #region Доп функции
        #region HideScrollBar
        public void HideScrollBar()
        {
            var scroll_viewer = (ScrollViewer)GetTemplateChild("scroll_viewer");
            VerticalScrollBar = GetScrollBar(scroll_viewer, Orientation.Vertical);
            if (VerticalScrollBar is null) return;

            DoubleAnimation fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));
            VerticalScrollBar.BeginAnimation(UIElement.OpacityProperty, fadeOut);
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
