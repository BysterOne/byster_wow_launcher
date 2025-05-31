using Cls.Any;
using Launcher.Any;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Launcher.Components
{
    /// <summary>
    /// Логика взаимодействия для CTextInput.xaml
    /// </summary>
    public partial class CTextInput : UserControl
    {
        public enum EInputType
        {
            Text,
            Password
        }

        public CTextInput()
        {
            InitializeComponent();
            this.Focusable = true;
            this.DataContext = this;
            this.MouseLeave += EMouseLeave;

            textbox.PreviewTextInput += Textbox_PreviewTextInput;
            textbox.GotFocus += Textbox_GotFocus;
            textbox.LostFocus += Textbox_LostFocus;

            passwordbox.PreviewTextInput += Textbox_PreviewTextInput;
            passwordbox.GotFocus += Textbox_GotFocus;
            passwordbox.LostFocus += Textbox_LostFocus;

            this.PreviewMouseLeftButtonDown += EPreviewMouseLeftButtonDown;

            TextChangedTimer.Tick += TimeChangedTextTick;
            TextChangedTimer.Interval = TimeSpan.Zero;
        }

        

        private static Type ElementType = typeof(CTextInput);

        #region Делегаты
        public delegate Task OnTextChangedDelegate(CTextInput sender, string text);
        #endregion

        #region События
        public event OnTextChangedDelegate OnTextChanged;
        #endregion

        #region Переменные
        private bool IsShowPassword { get; set; }
        private bool IsActive { get; set; }
        private DispatcherTimer TextChangedTimer { get; set; } = new DispatcherTimer();
        public TimeSpan TextChangedDelay
        {
            get => TextChangedTimer.Interval;
            set => TextChangedTimer.Interval = value;
        }
        #endregion

        #region Параметры     
        #region InputType        
        public EInputType InputType
        {
            get { return (EInputType)GetValue(InputTypeProperty); }
            set { SetValue(InputTypeProperty, value); }
        }
        public static readonly DependencyProperty InputTypeProperty =
            DependencyProperty.Register("InputType", typeof(EInputType), typeof(CTextInput),
                new PropertyMetadata(EInputType.Text, OnInputTypeChanged));

        private static void OnInputTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (CTextInput)d;

            switch (e.NewValue)
            {
                case EInputType.Text:
                    obj.textbox.Visibility = Visibility.Visible;
                    obj.passwordbox.Visibility = Visibility.Hidden;
                    obj.image.Cursor = null;
                    break;
                case EInputType.Password:
                    obj.textbox.Visibility = Visibility.Hidden;
                    obj.passwordbox.Visibility = Visibility.Visible;
                    obj.IconHeight = 26;
                    obj.image.Cursor = Cursors.Hand;
                    obj.Icon = new BitmapImage(Functions.GetSourceFromResource("Media/view_pass_icon.png"));
                    break;
            }
        }
        #endregion
        #region InputRegex
        public Regex InputRegex
        {
            get { return (Regex)GetValue(InputRegexProperty); }
            set { SetValue(InputRegexProperty, value); }
        }
        public static readonly DependencyProperty InputRegexProperty =
            DependencyProperty.Register("InputRegex", typeof(Regex), typeof(CTextInput),
                new PropertyMetadata(default(Regex), OnInputRegexChanged));

        private static void OnInputRegexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CTextInput)d;

            if (control.InputRegex == null || String.IsNullOrWhiteSpace(control.textbox.Text))
                return;

            if (!control.InputRegex.IsMatch(control.textbox.Text))
            {
                control.textbox.Clear();
            }
        }
        #endregion
        #region BorderRadius
        public double BorderRadius
        {
            get { return (double)GetValue(BorderRadiusProperty); }
            set { SetValue(BorderRadiusProperty, value); }
        }
        public static readonly DependencyProperty BorderRadiusProperty =
            DependencyProperty.Register("BorderRadius", typeof(double), typeof(CTextInput),
                new PropertyMetadata((double)0));
        #endregion
        #region IconHeight
        public double IconHeight
        {
            get { return (double)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }
        public static readonly DependencyProperty IconHeightProperty =
            DependencyProperty.Register("IconHeight", typeof(double), typeof(CTextInput),
                new PropertyMetadata((double)30));
        #endregion
        #region IconWidth
        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }
        public static readonly DependencyProperty IconWidthProperty =
            DependencyProperty.Register("IconWidth", typeof(double), typeof(CTextInput),
                new PropertyMetadata((double)30));
        #endregion
        #region Text
        private bool _isUpdatingInternallyTextProperty = false;
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set 
            {
                _isUpdatingInternallyTextProperty = true;
                SetValue(TextProperty, value);
                ChangedText(true);
                _isUpdatingInternallyTextProperty = false;
            }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(CTextInput),
                new PropertyMetadata("", OnTextPropertyChanged));

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CTextInput)d;
            if (!control._isUpdatingInternallyTextProperty) control.ChangedText();

            if (control.TextChangedDelay != TimeSpan.Zero)
            {
                control.TextChangedTimer.Stop();
                control.TextChangedTimer.Start();
            }
            else
            {
                control.OnTextChanged?.Invoke(control, ((string)e.NewValue).Trim());
            }
        }
        #endregion
        #region Placeholder
        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(CTextInput),
                new PropertyMetadata("placeholder"));
        #endregion
        #region PlaceholderColor
        public Brush PlaceholderColor
        {
            get { return (Brush)GetValue(PlaceholderColorProperty); }
            set { SetValue(PlaceholderColorProperty, value); }
        }
        public static readonly DependencyProperty PlaceholderColorProperty =
            DependencyProperty.Register("PlaceholderColor", typeof(Brush), typeof(CTextInput),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb((byte)(255 * 0.8), 255, 255, 255))));
        #endregion
        #region BackColor
        public Brush BackColor
        {
            get { return (Brush)GetValue(BackColorProperty); }
            set { SetValue(BackColorProperty, value); }
        }
        public static readonly DependencyProperty BackColorProperty =
            DependencyProperty.Register("BackColor", typeof(Brush), typeof(CTextInput),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0, 255, 255, 255))));
        #endregion
        #region Foreground
        public new Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }
        public static new readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(Brush), typeof(CTextInput),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))));
        #endregion
        #region Icon
        public ImageSource? Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set
            {
                image.Visibility = value is null ? Visibility.Collapsed : Visibility.Visible;
                SetValue(IconProperty, value);
            }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), ElementType,
                new PropertyMetadata(null, OnIconChanged));
        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CTextInput)d;
            var newValue = (ImageSource?)e.NewValue;

            control.image.Visibility = newValue == null ? Visibility.Collapsed : Visibility.Visible;
        }
        #endregion
        #endregion

        #region События
        private void N_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var grid = sender as Grid;
            var background_rectangles = grid!.Children.OfType<Rectangle>().ToList();
            if (background_rectangles.Count == 0) return;
            var back_rect = background_rectangles.First();
            AnimationHelper.OpacityAnimationStoryBoard(back_rect, 0).Begin(back_rect, HandoffBehavior.SnapshotAndReplace, true);
        }
        private void N_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var grid = sender as Grid;
            var background_rectangles = grid!.Children.OfType<Rectangle>().ToList();
            if (background_rectangles.Count == 0) return;
            var back_rect = background_rectangles.First();
            AnimationHelper.OpacityAnimationStoryBoard(back_rect, 0.2).Begin(back_rect, HandoffBehavior.SnapshotAndReplace, true);
        }
        private void EPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => ChangeState(true);
        private void Textbox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!textbox.IsFocused) { ChangeState(true); }
        }
        private void Textbox_LostFocus(object sender, RoutedEventArgs e)
        {
            ChangeState(false);
        }
        private void Textbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (InputRegex == null)
                return;
            
            if (!InputRegex.IsMatch(e.Text)) e.Handled = true;
        }

        private void image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (InputType is EInputType.Password && !String.IsNullOrWhiteSpace(Text))
            {
                IsShowPassword = true;                
                ChangePassword();
            }
        }

        private void image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (InputType is EInputType.Password)
            {
                IsShowPassword = false;
                ChangePassword();
            }
        }
        private void EMouseLeave(object sender, MouseEventArgs e)
        {
            if (InputType is EInputType.Password && IsShowPassword)
            {
                IsShowPassword = false;
                ChangePassword();
            }
        }
        #endregion

        #region TimeChangedTextTick
        private void TimeChangedTextTick(object? sender, EventArgs e)
        {
            TextChangedTimer.Stop();
            OnTextChanged?.Invoke(this, Text.Trim());
        }
        #endregion
        #region textbox_TextChanged
        private void textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetValue(TextProperty, textbox.Text.Trim());
        }
        #endregion
        #region passwordbox_PasswordChanged
        private void passwordbox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (InputType is EInputType.Password) SetValue(TextProperty, passwordbox.Password.Trim());
        }
        #endregion
        #region ChangeState
        private void ChangeState(bool toActive)
        {
            //if (IsActive == toActive) return;
            var isFocused = InputType switch { EInputType.Text => textbox.IsFocused, _ => passwordbox.IsFocused };

            var duration = AnimationHelper.AnimationDuration;
            var function = new PowerEase() { EasingMode = EasingMode.EaseInOut };
            var storyboard = new Storyboard();

            #region Переключение на текст
            if (toActive)
            {
                var f = placeholder.Margin;

                switch (InputType)
                {
                    case EInputType.Text:
                        textbox.Visibility = Visibility.Visible;                        
                        textbox.CaretIndex = textbox.Text.Length;
                        break;
                    case EInputType.Password:
                        passwordbox.Visibility = Visibility.Visible;                        
                        break;
                }               

                var animationFadeIn = new DoubleAnimation
                    (
                        InputType switch 
                        { 
                            EInputType.Password => passwordbox.Opacity, 
                            _ => textbox.Opacity 
                        }, 1, duration
                    )
                { EasingFunction = function };

                Storyboard.SetTarget(animationFadeIn, InputType switch { EInputType.Password => passwordbox, _ => textbox });
                Storyboard.SetTargetProperty(animationFadeIn, new PropertyPath(TextBox.OpacityProperty));
                storyboard.Children.Add(animationFadeIn);

                var animationThickness = new ThicknessAnimation(f, new Thickness(25, f.Top, f.Right, f.Bottom), duration) { EasingFunction = function };
                Storyboard.SetTarget(animationThickness, placeholder);
                Storyboard.SetTargetProperty(animationThickness, new PropertyPath(Label.MarginProperty));
                storyboard.Children.Add(animationThickness);

                var animationVisible = new DoubleAnimation(placeholder.Opacity, 0, duration) { EasingFunction = function };
                Storyboard.SetTarget(animationVisible, placeholder);
                Storyboard.SetTargetProperty(animationVisible, new PropertyPath(Label.OpacityProperty));
                storyboard.Children.Add(animationVisible);

                storyboard.Completed += (s, e) => 
                    Dispatcher.Invoke(() =>
                    {
                        placeholder.Visibility = Visibility.Collapsed;
                        if (InputType is EInputType.Text) { if (!isFocused) textbox.Focus(); }
                        else { if (!isFocused) passwordbox.Focus(); }
                    });
            }
            #endregion
            #region Переключение на заставку
            else
            {
                if (String.IsNullOrWhiteSpace(Text))
                {
                    var f = placeholder.Margin;
                    placeholder.Visibility = Visibility.Visible;

                    var animationFadeOut = new DoubleAnimation
                    (
                        InputType switch
                        {
                            EInputType.Password => passwordbox.Opacity,
                            _ => textbox.Opacity
                        }, 0, duration
                    )
                    { EasingFunction = function };
                    Storyboard.SetTarget(animationFadeOut, InputType switch { EInputType.Password => passwordbox, _ => textbox });
                    Storyboard.SetTargetProperty(animationFadeOut, new PropertyPath(TextBox.OpacityProperty));
                    storyboard.Children.Add(animationFadeOut);

                    var animationThickness = new ThicknessAnimation(f, new Thickness(2, f.Top, f.Right, f.Bottom), duration) { EasingFunction = function };
                    Storyboard.SetTarget(animationThickness, placeholder);
                    Storyboard.SetTargetProperty(animationThickness, new PropertyPath(Label.MarginProperty));
                    storyboard.Children.Add(animationThickness);

                    var animationVisible = new DoubleAnimation(placeholder.Opacity, 1, duration) { EasingFunction = function };
                    Storyboard.SetTarget(animationVisible, placeholder);
                    Storyboard.SetTargetProperty(animationVisible, new PropertyPath(Label.OpacityProperty));
                    storyboard.Children.Add(animationVisible);
                }

                RemoveFocus();
            }
            #endregion

            storyboard.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
        }
        #endregion
        #region ChangePassword
        private void ChangePassword()
        {
            textbox.Opacity = IsShowPassword ? 1 : 0;
            textbox.Visibility = IsShowPassword ? Visibility.Visible : Visibility.Hidden;
            passwordbox.Opacity = IsShowPassword ? 0 : 1;
            passwordbox.Visibility = IsShowPassword ? Visibility.Hidden : Visibility.Visible;
        }
        #endregion
        #region ChangedText
        public void ChangedText(bool isInternally = false)
        {
            var f = placeholder.Margin;

            var duration = AnimationHelper.AnimationDuration;
            var function = new PowerEase() { EasingMode = EasingMode.EaseInOut };

            var storyboard = new Storyboard();

            var v = GetValue(TextProperty);
            var vt = Text;

            if (!String.IsNullOrWhiteSpace(Text))
            {
                switch (InputType)
                {
                    case EInputType.Text:
                        textbox.Visibility = Visibility.Visible;
                        break;
                    case EInputType.Password:
                        passwordbox.Visibility = Visibility.Visible;
                        break;
                }

                var animationFadeIn = new DoubleAnimation
                    (
                        InputType switch 
                        { 
                            EInputType.Password => passwordbox.Opacity, 
                            _ => textbox.Opacity 
                        }, 1, duration) 
                { EasingFunction = function };
                Storyboard.SetTarget(animationFadeIn, InputType switch { EInputType.Password => passwordbox, _ => textbox });
                Storyboard.SetTargetProperty(animationFadeIn, new PropertyPath(TextBox.OpacityProperty));
                storyboard.Children.Add(animationFadeIn);

                var animationThickness = new ThicknessAnimation(f, new Thickness(25, f.Top, f.Right, f.Bottom), duration) { EasingFunction = function };
                Storyboard.SetTarget(animationThickness, placeholder);
                Storyboard.SetTargetProperty(animationThickness, new PropertyPath(Label.MarginProperty));
                storyboard.Children.Add(animationThickness);

                var animationVisible = new DoubleAnimation(placeholder.Opacity, 0, duration) { EasingFunction = function };
                Storyboard.SetTarget(animationVisible, placeholder);
                Storyboard.SetTargetProperty(animationVisible, new PropertyPath(Label.OpacityProperty));
                storyboard.Children.Add(animationVisible);
            }
            else if (!textbox.IsFocused || isInternally)
            {
                placeholder.Visibility = Visibility.Visible;

                var animationFadeOut = new DoubleAnimation
                    (
                        InputType switch
                        {
                            EInputType.Password => passwordbox.Opacity,
                            _ => textbox.Opacity
                        }, 0, duration
                    )
                { EasingFunction = function };
                Storyboard.SetTarget(animationFadeOut, InputType switch { EInputType.Password => passwordbox, _ => textbox });
                Storyboard.SetTargetProperty(animationFadeOut, new PropertyPath(TextBox.OpacityProperty));
                storyboard.Children.Add(animationFadeOut);

                var animationThickness = new ThicknessAnimation(f, new Thickness(2, f.Top, f.Right, f.Bottom), duration) { EasingFunction = function };
                Storyboard.SetTarget(animationThickness, placeholder);
                Storyboard.SetTargetProperty(animationThickness, new PropertyPath(Label.MarginProperty));
                storyboard.Children.Add(animationThickness);

                var animationVisible = new DoubleAnimation(placeholder.Opacity, 1, duration) { EasingFunction = function };
                Storyboard.SetTarget(animationVisible, placeholder);
                Storyboard.SetTargetProperty(animationVisible, new PropertyPath(Label.OpacityProperty));
                storyboard.Children.Add(animationVisible);

                if (isInternally) RemoveFocus();
            }

            storyboard.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
        }
        #endregion
        #region RemoveFocus
        public void RemoveFocus()
        {
            this.Focus();
        }
        #endregion        
    }
}
