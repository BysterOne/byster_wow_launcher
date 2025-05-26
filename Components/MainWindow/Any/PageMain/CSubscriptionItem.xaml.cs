using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Any;
using Launcher.Api.Models;
using Launcher.Cls;
using Launcher.Settings;
using Launcher.Windows.AnyMain.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Launcher.Components.MainWindow.Any.PageMain
{
    /// <summary>
    /// Логика взаимодействия для CSubscriptionItem.xaml
    /// </summary>
    public partial class CSubscriptionItem : UserControl
    {
        public CSubscriptionItem()
        {
            InitializeComponent();
        }

        #region Переменные
        public static LogBox Pref { get; set; } = new("Subscription Item");
        private SolidColorBrush SubExpired { get; set; } = new((Color)ColorConverter.ConvertFromString("#FF6BFF6B"));
        private SolidColorBrush SubExpiredComingSoon { get; set; } = (SolidColorBrush)Functions.GlobalResources()["orange_main"];
        #endregion

        #region Свойства
        #region Subscription
        public Subscription Subscription
        {
            get { return (Subscription)GetValue(SubscriptionProperty); }
            set { SetValue(SubscriptionProperty, value); }
        }

        public static readonly DependencyProperty SubscriptionProperty =
            DependencyProperty.Register("Subscription", typeof(Subscription), typeof(CSubscriptionItem),
                new PropertyMetadata(null, OnSubscriptionChanged));

        private static void OnSubscriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (CSubscriptionItem)d;
            if (e.NewValue is not null) _ = sender.SetProductData();
        }
        #endregion
        #endregion


        #region Функции
        #region SetProductData
        public async Task SetProductData()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось выполнить инициализацию";

            #region try
            try
            {
                var rotation = Subscription.Rotation;
                #region Верхняя панель
                (CGTP_CTR_class.Background as ImageBrush)!.ImageSource = GStatic.GetRotationClassIcon(rotation.Class);
                (CGTP_CTR_type.Background as ImageBrush)!.ImageSource = GStatic.GetRotationTypeIcon(rotation.Type);
                (CGTP_CTR_role.Background as ImageBrush)!.ImageSource = GStatic.GetRotationRoleIcon(rotation.Role);
                #endregion
                #region Загрузка изображения
                var img = Subscription.Rotation.Image;
                if (!String.IsNullOrEmpty(img))
                {
                    var imageSource = ImageControlHelper.GetImageFromCache(img, (int)Math.Ceiling(this.Width), (int)Math.Ceiling(this.Height));
                    if (imageSource is null)
                    {
                        await CG_image_skeleton.ChangeState(true);
                        _ = ImageControlHelper.LoadImageAsync
                        (
                            img,
                            (int)Math.Ceiling(this.Width),
                            (int)Math.Ceiling(this.Height),
                            new CancellationToken(),
                            (imgSource) =>
                            {
                                Dispatcher.Invoke(async () =>
                                {
                                    CG_image.Source = imgSource;
                                    await CG_image_skeleton.ChangeState(false);
                                },
                                DispatcherPriority.Background);
                            }
                        );
                    }
                    else
                    {
                        CG_image.Source = imageSource;
                    }
                }
                #endregion
                #region Нижняя панель                
                CGBP_name.Text = AppSettings.Instance.Language switch { _ => Subscription.Rotation.Name };

                CGBP_expired.Text = Subscription.ExpiredDate.ToLocalTime().ToString("dd.MM.yyyy HH:mm");

                var timeLeft = Subscription.ExpiredDate - DateTime.UtcNow;
                if (timeLeft.TotalHours < 12) CGBP_expired.Foreground = SubExpiredComingSoon;
                else CGBP_expired.Foreground = SubExpired;
                #endregion
            }
            #endregion
            #region UExcept
            catch (UExcept ex)
            {
                Functions.Error(ex, _failinf, _proc);
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uex = new UExcept(GlobalErrors.Exception, $"Исключение: {ex.Message}", ex);
                Functions.Error(uex, $"{_failinf}: исключение", _proc);
            }
            #endregion
        }
        #endregion
        #endregion
    }
}
