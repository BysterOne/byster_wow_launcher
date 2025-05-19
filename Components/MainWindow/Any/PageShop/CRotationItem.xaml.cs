using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Any;
using Launcher.Cls;
using Launcher.Components.MainWindow.Any.PageMain;
using Launcher.Settings;
using Launcher.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Rotation = Launcher.Api.Models.Rotation;

namespace Launcher.Components.MainWindow.Any.PageShop
{
    /// <summary>
    /// Логика взаимодействия для CRotationItem.xaml
    /// </summary>
    public partial class CRotationItem : UserControl
    {
        public CRotationItem()
        {
            InitializeComponent();

            this.Cursor = Cursors.Hand;
            this.MouseLeftButtonDown += EMouseLeftButtonDown;
        }       

        #region Переменные
        public static LogBox Pref { get; set; } = new("Rotation Item");
        #endregion

        #region Свойства
        #region Rotation
        public Rotation Rotation
        {
            get => (Rotation)GetValue(RotationProperty);
            set => SetValue(RotationProperty, value);
        }

        public static readonly DependencyProperty RotationProperty =
            DependencyProperty.Register(nameof(Rotation), typeof(Rotation), typeof(CRotationItem), 
                new(null, OnRotationChanged));

        private static void OnRotationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (CRotationItem)d;
            _ = sender.SetRotationData();
        }
        #endregion
        #endregion

        #region Функции
        #region SetRotationData
        private async Task SetRotationData()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось установить данные ротации";

            #region try
            try
            {
                if (Rotation is null) return;
                #region Загрузка изображения
                var image = Rotation.Image;
                if (image is not null)
                {
                    var imageSource = ImageControlHelper.GetImageFromCache(image, (int)Math.Ceiling(this.Width), (int)Math.Ceiling(this.Height));
                    if (imageSource is not null) { CG_image.Source = imageSource; return; }

                    await CG_image_skeleton.ChangeState(true);
                    _ = ImageControlHelper.LoadImageAsync
                    (
                        image,
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
                #endregion
                #region Иконки
                (CGTP_CTR_class.Background as ImageBrush)!.ImageSource = GStatic.GetRotationClassIcon(Rotation.Class);
                (CGTP_CTR_type.Background as ImageBrush)!.ImageSource = GStatic.GetRotationTypeIcon(Rotation.Type);
                (CGTP_CTR_role.Background as ImageBrush)!.ImageSource = GStatic.GetRotationRoleIcon(Rotation.Role);
                #endregion
                #region Название
                CGBPN_value.Text = Rotation.Name;
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
                var uerror = new UError(GlobalErrors.Exception, $"Исключение: {ex.Message}");
                Functions.Error(ex, uerror, $"{_failinf}: исключение", _proc);
            }
            #endregion
        }
        #endregion
        #endregion

        #region Обработчики событий
        #region EMouseLeftButtonDown
        private void EMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => _ = Main.ShowModal(new CRotationInfoDialogBox(), Rotation);        
        #endregion
        #endregion
    }
}
