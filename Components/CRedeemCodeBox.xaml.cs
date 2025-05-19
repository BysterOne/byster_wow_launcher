using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Launcher.Components
{
    /// <summary>
    /// Логика взаимодействия для CRedeemCodeBox.xaml
    /// </summary>
    public partial class CRedeemCodeBox : UserControl
    {
        public CRedeemCodeBox()
        {
            InitializeComponent();
        }

        public delegate void DRedeemCodeEvent(string code);
        public event DRedeemCodeEvent TryRedeemCode;

        public string PlaceHolder
        {
            get => placeholder_.GetValue(TextBlock.TextProperty).ToString();
            set => placeholder_.SetValue(TextBlock.TextProperty, value);            
        }

        public string Coupon
        {
            get => ((string)input_.GetValue(TextBox.TextProperty)).Trim();
            set
            {
                input_.SetValue(TextBox.TextProperty, value);
                SetPlaceholder();
            }
        }

        private async void input_box_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (placeholder_.Opacity != 0)
            {
                HideBlock(placeholder_);
                ShowBlock(input_, 9);
                input_.Focus();
            }
        }

        private void input__LostFocus(object sender, RoutedEventArgs e)
        {
            SetPlaceholder();
        }

        private async void SetPlaceholder()
        {
            if (String.IsNullOrWhiteSpace(Coupon))
            {                
                ShowBlock(placeholder_, 9);
                HideBlock(input_);
                await Dispatcher.BeginInvoke(() => Keyboard.ClearFocus());
            }
            else
            {
                HideBlock(placeholder_);
                ShowBlock(input_, 9);
            }
        }

        private void input__PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!String.IsNullOrWhiteSpace(Coupon)) { TryRedeemCode?.Invoke(Coupon); }
            }
            else { e.Handled = false; }
        }

        private void redeem__PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(Coupon)) { TryRedeemCode?.Invoke(Coupon); }
        }

        private async Task HideBlock(UIElement element)
        {
            element.Visibility = Visibility.Visible;
            ChangeOpacity(element, 0);
            await Task.Run(() => { Thread.Sleep(200); });
            Grid.SetZIndex(element, -1);
            element.Visibility = Visibility.Collapsed;
        }

        private async Task ShowBlock(UIElement element, int index)
        {
            element.Visibility = Visibility.Visible;
            element.Opacity = 0;
            Grid.SetZIndex(element, index);
            ChangeOpacity(element, 1);
            await Task.Run(() => { Thread.Sleep(200); });
        }


        private void PanelButtonMouseLeave(object sender, MouseEventArgs e)
        {
            var el_ = sender as Grid;
            if (el_ == null) return;
            var back = el_.Children.OfType<Rectangle>().First();
            var image = el_.Children.OfType<Image>().First();
            ChangeOpacity(back, 0.05);
            ChangeOpacity(image, 0.5);
        }

        private void PanelButtonMouseEnter(object sender, MouseEventArgs e)
        {
            var el_ = sender as Grid;
            if (el_ == null) return;
            var back = el_.Children.OfType<Rectangle>().First();
            var image = el_.Children.OfType<Image>().First();
            ChangeOpacity(back, 0.15);
            ChangeOpacity(image, 0.7);
        }

        public void ChangeOpacity(UIElement element, double opacity)
        {
            var dur_ = new Duration(TimeSpan.FromMilliseconds(200));
            var ease_ = new PowerEase() { EasingMode = EasingMode.EaseInOut };
            var anim_ = new DoubleAnimation(opacity, dur_) { EasingFunction = ease_ };
            element.BeginAnimation(OpacityProperty, anim_);
        }

        public bool RedeemIsAnimated { get; set; } = false;

        private async void input__TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(Coupon))
            {
                if (!RedeemIsAnimated)
                {
                    var anim = new DoubleAnimationUsingKeyFrames();
                    anim.KeyFrames.Add(new EasingDoubleKeyFrame(0.05, TimeSpan.FromMilliseconds(300)));
                    anim.KeyFrames.Add(new EasingDoubleKeyFrame(0.2, TimeSpan.FromMilliseconds(600)));
                    anim.KeyFrames.Add(new EasingDoubleKeyFrame(0.2, TimeSpan.FromMilliseconds(800)));
                    anim.KeyFrames.Add(new EasingDoubleKeyFrame(0.05, TimeSpan.FromMilliseconds(1100)));

                    anim.RepeatBehavior = RepeatBehavior.Forever;

                    redeem_back.BeginAnimation(OpacityProperty, anim);
                }
                
                RedeemIsAnimated = true;
            }
            else
            {
                var anim = (new DoubleAnimation(0.05, new Duration(TimeSpan.FromMilliseconds(200))) { EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut } });
                redeem_back.BeginAnimation(OpacityProperty, anim);
                await Task.Run(() => { Thread.Sleep(200); });
                RedeemIsAnimated = false;
            }
        }
    }
}
