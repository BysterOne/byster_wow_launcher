using Cls.Any;
using Launcher.Any;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Логика взаимодействия для CToggleSwitch.xaml
    /// </summary>
    public partial class CToggleSwitch : UserControl
    {
        public CToggleSwitch()
        {
            InitializeComponent();

            this.DataContext = this;
            this.Cursor = Cursors.Hand;
            this.SizeChanged += ESizeChanged;
            this.MouseLeftButtonDown += EMouseLeftButtonDown;
        }        

        #region Переменные
        private bool _isFast = false;
        #endregion

        #region События
        public delegate void SelectedIndexChangedDelegate(object sender, int newIndex);
        public event SelectedIndexChangedDelegate SelectedIndexChanged;
        #endregion

        #region Свойства
        #region SelectedIndex
        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(CToggleSwitch),
                new PropertyMetadata(0, OnSelectedIndexChanged));

        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CToggleSwitch sender) sender.UpdateView();
        }
        #endregion
        #region BorderRadius
        public CornerRadius BorderRadius
        {
            get { return (CornerRadius)GetValue(BorderRadiusProperty); }
            set { SetValue(BorderRadiusProperty, value); }
        }

        public static readonly DependencyProperty BorderRadiusProperty =
            DependencyProperty.Register(nameof(BorderRadius), typeof(CornerRadius), typeof(CToggleSwitch),
                new PropertyMetadata(new CornerRadius(15)));
        #endregion
        #region BackColor
        public Brush BackColor
        {
            get { return (Brush)GetValue(BackColorProperty); }
            set { SetValue(BackColorProperty, value); }
        }

        public static readonly DependencyProperty BackColorProperty =
            DependencyProperty.Register(nameof(BackColor), typeof(Brush), typeof(CToggleSwitch),
                new PropertyMetadata(
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString($"#19FFFFFF"))
                ));
        #endregion
        #region ToggleColor
        public Brush ToggleColor
        {
            get { return (Brush)GetValue(ToggleColorProperty); }
            set { SetValue(ToggleColorProperty, value); }
        }

        public static readonly DependencyProperty ToggleColorProperty =
            DependencyProperty.Register(nameof(ToggleColor), typeof(Brush), typeof(CToggleSwitch),
                new PropertyMetadata(
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString($"#19FFFFFF"))
                ));
        #endregion
        #region FirstItem
        public string FirstItem
        {
            get { return (string)GetValue(FirstItemProperty); }
            set { SetValue(FirstItemProperty, value); }
        }

        public static readonly DependencyProperty FirstItemProperty =
            DependencyProperty.Register(nameof(FirstItem), typeof(string), typeof(CToggleSwitch),
                new PropertyMetadata("first"));
        #endregion
        #region SecondItem
        public string SecondItem
        {
            get { return (string)GetValue(SecondItemProperty); }
            set { SetValue(SecondItemProperty, value); }
        }

        public static readonly DependencyProperty SecondItemProperty =
            DependencyProperty.Register(nameof(SecondItem), typeof(string), typeof(CToggleSwitch),
                new PropertyMetadata("second"));
        #endregion
        #region FontSize
        public new int FontSize
        {
            get { return (int)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public static new readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register(nameof(FontSize), typeof(int), typeof(CToggleSwitch),
                new PropertyMetadata(18));
        #endregion
        #endregion

        #region Обработчики событий
        #region EMouseLeftButtonDown
        private void EMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectedIndex = SelectedIndex == 0 ? 1 : 0;
            SelectedIndexChanged?.Invoke(this, SelectedIndex);
        }
        #endregion
        #region ESizeChanged
        private void ESizeChanged(object sender, SizeChangedEventArgs e)
        {
            _isFast = true;
            UpdateView();
        }
        #endregion
        #endregion

        #region Функции
        #region UpdateView
        private void UpdateView()
        {
            var newLeft =
                switch_background.ActualWidth is double.NaN || switch_background.ActualWidth == 0 ? 
                    switch_background.Width is double.NaN ?
                        0 : switch_background.Width
                    : switch_background.ActualWidth;

            var newMargin = SelectedIndex is 0 ? new Thickness(0, 0, 0, 0) : new Thickness(newLeft, 0, 0, 0);
            if (_isFast)
            {
                switch_background.Margin = newMargin;
                _isFast = false;
                return;
            }

            var anim = AnimationHelper.ThicknessAnimationStoryBoard
            (
                switch_background,
                newMargin,
                TimeSpan.FromMilliseconds(200)
            );
            anim.Completed += (_, __) =>
            {
                anim.Children.Clear();
                switch_background.Margin = newMargin;
            };
            anim.Begin(switch_background, HandoffBehavior.SnapshotAndReplace, true);

            
        }
        #endregion
        #region SetSelectedIndexFast
        public void SetSelectedIndexFast(int index)
        {
            _isFast = true;
            SelectedIndex = index;
        } 
        #endregion
        #endregion
    }
}
