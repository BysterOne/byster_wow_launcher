using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using Launcher.Components;
using Launcher.Any;
using System.Windows.Media.Animation;

namespace Launcher.Styles
{
    public partial class GlobalResourceDictionary : ResourceDictionary
    {
        public GlobalResourceDictionary()
        {
            InitializeComponent();
        }

        #region WindowControlButtonStyle_MouseEnter
        private void WindowControlButtonStyle_MouseEnter(object sender, MouseEventArgs e)
        {
            var button = (CButton)sender;    
            (AnimationHelper.OpacityAnimationStoryBoard(button, 1)).Begin(button, HandoffBehavior.SnapshotAndReplace, true);
        }
        #endregion
        #region WindowControlButtonStyle_MouseLeave
        private void WindowControlButtonStyle_MouseLeave(object sender, MouseEventArgs e)
        {
            var button = (CButton)sender;
            (AnimationHelper.OpacityAnimationStoryBoard(button, 0.8)).Begin(button, HandoffBehavior.SnapshotAndReplace, true);
        }
        #endregion
        #region MinimizeWindowControlButtonStyle_MouseDown
        private void MinimizeWindowControlButtonStyle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var window = Window.GetWindow((DependencyObject)sender);
            if (window is not null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }
        #endregion
        #region CloseWindowControlButtonStyle_MouseDown
        private void CloseWindowControlButtonStyle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var window = Window.GetWindow((DependencyObject)sender);
            if (window is not null)
            {
                window.Close();
            }
        }
        #endregion

    }
}
