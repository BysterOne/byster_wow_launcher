﻿using System.ComponentModel;
using System.Windows;

namespace Launcher.Any
{
    public class OpacityMonitor
    {
        public static void Monitor(UIElement element, bool isHitTest = true)
        {
            if (element is null) return;

            DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty(UIElement.OpacityProperty, typeof(UIElement));

            descriptor?.AddValueChanged(element, (sender, e) =>
            {
                if (isHitTest)
                {
                    if (element.Opacity == 0) element.IsHitTestVisible = false;
                    else element.IsHitTestVisible = true;
                }                
                else
                {
                    if (element.Opacity == 0) element.Visibility = Visibility.Hidden;
                    //else element.Visibility = Visibility.Visible;
                }
            });
        }
    }
}
