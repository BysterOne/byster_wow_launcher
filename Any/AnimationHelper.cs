using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows;
using System.Security.Cryptography;

namespace Launcher.Any
{
    public static class AnimationHelper
    {
        #region Переменные
        public static TimeSpan AnimationDuration { get; set; } = TimeSpan.FromMilliseconds(100);

        public readonly static PowerEase EaseInOut = new PowerEase() { EasingMode = EasingMode.EaseInOut };
        public readonly static PowerEase EaseIn = new PowerEase() { EasingMode = EasingMode.EaseIn };
        #endregion

        #region OpacityAnimation
        public static Storyboard OpacityAnimationStoryBoard(FrameworkElement element, double opacity, TimeSpan? duration = null, IEasingFunction? easing = null)
        {
            duration ??= AnimationDuration;
            easing ??= EaseInOut;

            Storyboard storyboard = new Storyboard();

            var animation = OpacityAnimation(element, opacity, duration, easing);

            storyboard.Children.Add(animation);

            return storyboard;
        }

        public static DoubleAnimation OpacityAnimation(FrameworkElement element, double opacity, TimeSpan? duration = null, IEasingFunction? easing = null) =>
            DoubleAnimation(element, opacity, new PropertyPath(UIElement.OpacityProperty), duration, easing);
        #endregion

        #region ThicknessAnimation
        public static Storyboard ThicknessAnimationStoryBoard(FrameworkElement element, Thickness newThickness, TimeSpan? duration = null, IEasingFunction? easing = null)
        {
            duration ??= AnimationDuration;
            easing ??= EaseInOut;

            Storyboard storyboard = new Storyboard();

            var animation = ThicknessAnimation(element, newThickness, duration, easing);
            storyboard.Children.Add(animation);

            return storyboard;
        }

        public static ThicknessAnimation ThicknessAnimation(FrameworkElement element, Thickness newThickness, TimeSpan? duration = null, IEasingFunction? easing = null)
        {
            duration ??= AnimationDuration;
            easing ??= EaseInOut;

            var animation = new ThicknessAnimation(newThickness, (TimeSpan)duration) { EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut } };

            Storyboard.SetTarget(animation, element);
            Storyboard.SetTargetProperty(animation, new PropertyPath(FrameworkElement.MarginProperty));

            return animation;
        }
        #endregion

        #region DoubleAnimation
        public static Storyboard DoubleAnimationStoryBoard(FrameworkElement element, double newValue, PropertyPath property, TimeSpan? duration = null, IEasingFunction? easing = null)
        {
            duration ??= AnimationDuration;
            easing ??= EaseInOut;

            Storyboard storyboard = new Storyboard();

            var animation = DoubleAnimation(element, newValue, property, duration, easing);
            storyboard.Children.Add(animation);

            return storyboard;
        }

        public static DoubleAnimation DoubleAnimation(FrameworkElement element, double newValue, PropertyPath property, TimeSpan? duration = null, IEasingFunction? easing = null)
        {
            duration ??= AnimationDuration;
            easing ??= EaseInOut;

            var animation = new DoubleAnimation(newValue, (TimeSpan)duration) { EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut } };

            Storyboard.SetTarget(animation, element);
            Storyboard.SetTargetProperty(animation, property);

            return animation;
        }
        #endregion

        #region WidthAnimation
        public static Storyboard WidthAnimationStoryBoard(FrameworkElement element, double newValue, TimeSpan? duration = null, IEasingFunction? easing = null)
            => DoubleAnimationStoryBoard(element, newValue, new PropertyPath(FrameworkElement.WidthProperty), duration, easing);
        public static DoubleAnimation WidthAnimation(FrameworkElement element, double newWidth, TimeSpan? duration = null, IEasingFunction? easing = null)
            => DoubleAnimation(element, newWidth, new PropertyPath(FrameworkElement.WidthProperty), duration, easing);
        #endregion
    }
}
