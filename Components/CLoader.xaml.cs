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
    /// Логика взаимодействия для CLoader.xaml
    /// </summary>
    public partial class CLoader : UserControl
    {
        private static int CountBlocks { get; set; } = 13;
        private static Size StartBlockSize { get; set; } = new Size(8, 8);
        private static double EndBlockHeight { get; set; } = 30;

        #region Foreground
        public SolidColorBrush ForeColor
        {
            get { return (SolidColorBrush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public static DependencyProperty ForeColorProperty { get; set; } =
            DependencyProperty.Register("ForeColor", typeof(Brush), typeof(CLoader),
                new PropertyMetadata(new SolidColorBrush(Colors.White), OnForegroundChanged));

        private static void OnForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var loader = d as CLoader;

            if (loader is not null)
            {
                var blocks = loader.blocks.Children.OfType<Rectangle>().ToList();
                foreach (var block in blocks)
                {
                    block.Fill = (Brush)e.NewValue;
                }
            }
        }
        #endregion

        public CLoader()
        {
            InitializeComponent();

            for (int i = 0; i < CountBlocks; i++)
            {
                var rect = new Rectangle()
                {
                    Width = StartBlockSize.Width,
                    Height = StartBlockSize.Height,
                    Fill = ForeColor,
                    Margin = new Thickness(2, 0, 2, 0),
                    RadiusX = StartBlockSize.Width / 2,
                    RadiusY = StartBlockSize.Width / 2
                };
                RenderOptions.SetBitmapScalingMode(rect, BitmapScalingMode.HighQuality);
                blocks.Children.Add(rect);
            }
        }

        #region CreateBlockAnimation
        private void CreateBlockAnimation(ref Storyboard board, Rectangle block, double newHeight, TimeSpan delay)
        {
            Storyboard.SetTargetProperty(board, new PropertyPath("Height"));
            Storyboard.SetTarget(board, block);

            var ease = new PowerEase() { EasingMode = EasingMode.EaseInOut };
            var timeline = TimeSpan.Zero;
            var animationTime = TimeSpan.FromMilliseconds(400);

            var heightAnimation = new DoubleAnimationUsingKeyFrames() 
            { 
                BeginTime = delay, 
                RepeatBehavior = RepeatBehavior.Forever 
            };           

            heightAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(StartBlockSize.Height, timeline) { EasingFunction = ease });
            timeline = timeline.Add(animationTime); // Длительность анимации
            heightAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(newHeight, timeline) { EasingFunction = ease });
            timeline = timeline.Add(TimeSpan.FromMilliseconds(80)); // Задержка перед возвратом в обычное состояние
            heightAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(newHeight, timeline) { EasingFunction = ease });
            timeline = timeline.Add(animationTime); // Длительность анимации
            heightAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(StartBlockSize.Height, timeline) { EasingFunction = ease });
            timeline = timeline.Add(TimeSpan.FromMilliseconds(10)); // Ожидание перед след циклом
            heightAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(StartBlockSize.Height, timeline) { EasingFunction = ease });

            board.Children.Add(heightAnimation);

            board.Begin(block, HandoffBehavior.SnapshotAndReplace, true);
        }
        #endregion

        #region CreateBlockStopAnimation
        private void CreateBlockStopAnimation(ref Storyboard board, Rectangle block)
        {
            Storyboard.SetTargetProperty(board, new PropertyPath("Height"));
            Storyboard.SetTarget(board, block);

            var ease = new PowerEase() { EasingMode = EasingMode.EaseInOut };

            var heightAnimation = new DoubleAnimation(StartBlockSize.Height, AnimationHelper.AnimationDuration)
            {
                EasingFunction = ease
            };

            board.Children.Add(heightAnimation);

            board.Begin(block, HandoffBehavior.SnapshotAndReplace, true);
        }
        #endregion

        #region StartAnimation
        public void StartAnimation()
        {
            var blocks = this.blocks.Children.OfType<Rectangle>().ToList();

            var storyboard = new Storyboard();

            for (int i = 0; i < blocks.Count; i++) CreateBlockAnimation(ref storyboard, blocks[i], EndBlockHeight, TimeSpan.FromMilliseconds(i * 20));            
        }
        #endregion

        #region StopAnimation
        public void StopAnimation()
        {
            var blocks = this.blocks.Children.OfType<Rectangle>().ToList();

            var storyboard = new Storyboard();

            for (int i = 0; i < blocks.Count; i++) CreateBlockStopAnimation(ref storyboard, blocks[i]);
        }
        #endregion
    }
}
