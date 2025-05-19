using Launcher.Any;
using Launcher.Api.Models;
using Launcher.Windows.AnyMain.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace Launcher.Components
{
    /// <summary>
    /// Логика взаимодействия для CMediaItem.xaml
    /// </summary>
    public partial class CMediaItem : UserControl
    {
        public CMediaItem()
        {
            InitializeComponent();

            this.Cursor = Cursors.Hand;
            this.MouseEnter += EMouseEnter;
            this.MouseLeave += EMouseLeave;
        }



        #region Свойства
        #region Item

        public Media Item
        {
            get => (Media)GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
        }

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(Media), typeof(CMediaItem),
                new PropertyMetadata(null, OnItemChanged));

        private static void OnItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CMediaItem sender) sender.SetItem();
        }
        #endregion
        #endregion

        #region Обработчики событий
        #region EMouseEnter
        private void EMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Item is null) return;

            if (Item.Type is EMediaType.Image)
            {
                AnimationHelper.OpacityAnimationStoryBoard(ImageHover, 1)
                    .Begin(ImageHover, HandoffBehavior.SnapshotAndReplace, true);
            }
            else if (Item.Type is EMediaType.Video)
            {
                AnimationHelper.WidthAnimationStoryBoard(VideoIcon, 60)
                    .Begin(VideoHover, HandoffBehavior.SnapshotAndReplace, true);
            }
        }
        #endregion
        #region EMouseLeave
        private void EMouseLeave(object sender, MouseEventArgs e)
        {
            if (Item is null) return;

            if (Item.Type is EMediaType.Image)
            {
                AnimationHelper.OpacityAnimationStoryBoard(ImageHover, 0)
                    .Begin(ImageHover, HandoffBehavior.SnapshotAndReplace, true);
            }
            else if (Item.Type is EMediaType.Video)
            {
                AnimationHelper.WidthAnimationStoryBoard(VideoIcon, 50)
                    .Begin(VideoHover, HandoffBehavior.SnapshotAndReplace, true);
            }
        }
        #endregion
        #endregion

        #region Функции
        #region SetItem
        private void SetItem()
        {
            if (Item is null) return;

            ImageHover.Visibility = Item.Type is EMediaType.Image ? Visibility.Visible : Visibility.Collapsed;
            VideoHover.Visibility = Item.Type is EMediaType.Video ? Visibility.Visible : Visibility.Collapsed;
            VideoHover.Opacity = Item.Type is EMediaType.Video ? 1 : 0;

            BackgroundBorder.Effect = Item.Type is EMediaType.Image ? null : new BlurEffect()
            {
                Radius = 30,
                RenderingBias = RenderingBias.Performance,
            };

            if (Item.Type is EMediaType.Image)
            {
                var ex = Item.Url.Split('.').LastOrDefault();
                var exComp = ex is not null ? !ex.Equals("mp4", StringComparison.CurrentCultureIgnoreCase) : false;

                var imageSource = ImageControlHelper.GetImageFromCache(Item.Url, (int)Math.Ceiling(this.Width), (int)Math.Ceiling(this.Height));
                if (imageSource is null && exComp)
                {
                    _ = Skeleton.ChangeState(true, false);
                    _ = ImageControlHelper.LoadImageAsync
                    (
                        Item.Url,
                        (int)Math.Ceiling(this.Width),
                        (int)Math.Ceiling(this.Height),
                        new CancellationToken(),
                        (imgSource) =>
                        {
                            Dispatcher.Invoke(async () =>
                            {
                                BB_image_brush.ImageSource = imgSource;
                                await Skeleton.ChangeState(false);
                            },
                            DispatcherPriority.Background);
                        }
                    );
                }
                else
                {
                    BB_image_brush.ImageSource = imageSource;
                }
            }                        
        }
        #endregion
        #endregion
    }
}
