using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows;

namespace Launcher.Any
{
    public static class AnimationHelper
    {
        #region Переменные
        public static TimeSpan AnimationDuration { get; set; } = TimeSpan.FromMilliseconds(150);
        #endregion

        #region OpacityAnimation
        public static Storyboard OpacityAnimation(FrameworkElement element, double opacity, TimeSpan? duration = null)
        {
            duration ??= AnimationDuration;

            Storyboard storyboard = new Storyboard();

            var animation = new DoubleAnimation(opacity, (TimeSpan)duration) { EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut } };

            Storyboard.SetTarget(animation, element);
            Storyboard.SetTargetProperty(animation, new PropertyPath(UIElement.OpacityProperty));

            storyboard.Children.Add(animation);

            return storyboard;
        }
        #endregion
    }
}
