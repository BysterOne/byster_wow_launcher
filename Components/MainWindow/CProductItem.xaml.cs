using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Any;
using Launcher.Any.GlobalEnums;
using Launcher.Api;
using Launcher.Api.Models;
using Launcher.Cls;
using Launcher.Components.DialogBox;
using Launcher.Components.MainWindow.Any.PageShop.Models;
using Launcher.Components.MainWindow.Any.ProductItem.Errors;
using Launcher.Settings;
using Launcher.Windows;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Launcher.Components.MainWindow
{
    /// <summary>
    /// Логика взаимодействия для CProductItem.xaml
    /// </summary>
    public partial class CProductItem : UserControl
    {
        public CProductItem()
        {
            InitializeComponent();

            this.MouseEnter += EMouseEnter;
            this.MouseLeave += EMouseLeave;
            this.CGBP_buttons.SizeChanged += EGBP_buttons_SizeChanged;
        }

        #region Переменные
        private bool IsFirstPanelButtonsChange { get; set; } = true;
        public static LogBox Pref { get; set; } = new("Product Item");
        public CCartItem? CartItem { get; set; } = null;
        #endregion

        #region Свойства
        #region Product
        public Product Product 
        { 
            get => (Product)GetValue(ProductProperty);
            set => SetValue(ProductProperty, value);
        }

        public static readonly DependencyProperty ProductProperty =
            DependencyProperty.Register("Product", typeof(Product), typeof(CProductItem), 
                new PropertyMetadata(null, OnProductChanged));

        private static void OnProductChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (d as CProductItem)!;
            if (e.NewValue is not null) _ = sender.SetProductData();
        }
        #endregion
        #endregion

        #region Обработчики событий
        #region EGBP_buttons_SizeChanged
        private void EGBP_buttons_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!IsFirstPanelButtonsChange) return;
            UpdateView();
            UpdateButtonsPanelView();
            if (e.NewSize.Width > 0 && e.NewSize.Height > 0)
                IsFirstPanelButtonsChange = false;
        }
        #endregion
        #region EMouseEnter
        private void EMouseEnter(object sender, MouseEventArgs e) => ChangeButtonsPanelState(true);
        #endregion
        #region EMouseLeave
        private void EMouseLeave(object sender, MouseEventArgs e) => ChangeButtonsPanelState(false);
        #endregion
        #region CGBPBCT_test_PreviewMouseDoubleClick
        private async void CGBPBCT_test_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Product.CanTest) return;

            var boxSettings = new BoxSettings
            (
                Dictionary.Translate("Тест"),
                Dictionary.Translate("Вы действительно хотите взять на тест данный продукт?"),
                [
                    new (EResponse.Yes, Dictionary.Translate("Да")),
                    new (EResponse.Cancel, Dictionary.Translate("Отмена")),
                ]
            );

            var response = await Main.ShowModal(boxSettings);
            if (response.IsSuccess && response.Response is EResponse.Yes)
            {
                await Main.Loader(ELoaderState.Show);  

                var tryRunTest = await CApi.GetTest(Product.Id);
                if (tryRunTest.IsSuccess)
                {
                    #region Обновляем данный продукт
                    Product.CanTest = false;
                    UpdateView();
                    UpdateButtonsPanelView();
                    #endregion
                    #region Обновляем активные подписки
                    GProp.UpdateSubscriptions(tryRunTest.Response);
                    #endregion
                    #region Скрываем прелоадер и показываем ответ
                    await Task.Run(() => Thread.Sleep(300));
                    await Main.Loader(ELoaderState.Hide);
                    Main.Notify(Dictionary.Translate("Отлично! Теперь можете приступить к тестированию"));
                    #endregion
                }
                else
                {
                    #region Скрываем прелоадер
                    await Main.Loader(ELoaderState.Hide);
                    Main.Notify(Dictionary.Translate("Не удалось взять на тест. Попробуйте позже"));
                    #endregion
                }
            }
        }
        #endregion
        #region CGBPBCT_buy_MouseLeftButtonDown
        private void CGBPBCT_buy_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (CartItem is null)
            {
                GProp.Cart.CartUpdated += ECartUpdated;
                GProp.Cart.AddItem(Product);
            } 
        }
        #endregion
        #region ECartUpdated
        private void ECartUpdated(CCartItem item, ListChangedType changedType)
        {
            if (item.Product.Id != Product.Id) return;

            switch (changedType)
            {
                case ListChangedType.ItemAdded:
                    CartItem = item;
                    UpdateView();
                    UpdateButtonsPanelView();
                    break;
                case ListChangedType.ItemChanged:
                    CGBPBC_count.Text = item.Count.ToString();
                    break;
                case ListChangedType.ItemDeleted:
                    CartItem = null;
                    GProp.Cart.CartUpdated -= ECartUpdated;
                    UpdateView();
                    UpdateButtonsPanelView();                    
                    break;
                default:
                    break;
            }
        }
        #endregion
        #region CGBPBC_remove_MouseLeftButtonDown
        private void CGBPBC_remove_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (CartItem is not null) GProp.Cart.ChangeCount(Product, (CartItem.Count - 1));
        }
        #endregion
        #region CGBPBC_add_MouseLeftButtonDown
        private void CGBPBC_add_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (CartItem is not null) GProp.Cart.ChangeCount(Product, (CartItem.Count + 1));
        }
        #endregion
        #endregion

        #region Функции
        #region UpdateView
        private void UpdateView()
        {
            if (CartItem is not null)
            {
                CGBPB_cart_test.Visibility = Visibility.Collapsed;
                CGBPB_count.Visibility = Visibility.Visible;
                CGBPBC_count.Text = CartItem.Count.ToString();
            }
            else
            {
                CGBPB_cart_test.Visibility = Visibility.Visible;
                CGBPB_count.Visibility = Visibility.Collapsed;
                CGBPBCT_test.Visibility = Product.CanTest ? Visibility.Visible : Visibility.Collapsed;
            }
            CGBP_buttons.UpdateLayout();
        }
        #endregion
        #region UpdateButtonsPanelView
        private void UpdateButtonsPanelView()
        {
            CGBP_buttons.Margin =
                this.IsMouseOver ?
                new Thickness(0, -5, 0, 0) :
                new Thickness(5, 0, 0, -100);
        }
        #endregion
        #region ChangeButtonsPanelState
        public void ChangeButtonsPanelState(bool isActive)
        {
            var newThickness =
                this.IsMouseOver ?
                new Thickness(0, -5, 0, 0) :
                new Thickness(5, 0, 0, -100);

            var storyboard = new Storyboard();
            var animation = new ThicknessAnimation(newThickness, AnimationHelper.AnimationDuration)
            {
                EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(animation, CGBP_buttons);
            Storyboard.SetTargetProperty(animation, new PropertyPath(Grid.MarginProperty));

            storyboard.Children.Add(animation);

            storyboard.Begin(CGBP_buttons, HandoffBehavior.SnapshotAndReplace, true);   
        }
        #endregion
        #region ChangeShadow
        private void ChangeShadow(bool isActive)
        {
            var blurRadius = isActive ? 15 : 0;
            //var shadowColor = isActive ? (Color)ColorConverter.ConvertFromString("#1AE8E8E8") : Colors.Transparent;
            var shadowColor = isActive ? Colors.White : Colors.Transparent;

            var storyboard = new Storyboard();

            #region Видимость
            var animation = new DoubleAnimation(blurRadius, AnimationHelper.AnimationDuration)
            {
                EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(animation, MainBorder);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.Effect).(DropShadowEffect.BlurRadius)"));

            storyboard.Children.Add(animation);
            #endregion
            #region Цвет
            var animationColor = new ColorAnimation(shadowColor, AnimationHelper.AnimationDuration)
            {
                EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(animationColor, MainBorder);
            Storyboard.SetTargetProperty(animationColor, new PropertyPath("(UIElement.Effect).(DropShadowEffect.Color)"));

            storyboard.Children.Add(animationColor);
            #endregion

            storyboard.Begin(MainBorder, HandoffBehavior.SnapshotAndReplace, true);
        }
        #endregion
        #region SetProductData
        public async Task SetProductData()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось выполнить инициализацию";

            #region try
            try
            {
                #region Проверка наличия в корзине
                CartItem = GProp.Cart.GetItem(Product);
                if (CartItem is not null) { GProp.Cart.CartUpdated += ECartUpdated; }
                #endregion
                #region Верхняя панель
                CGTP_background.RadiusX = Product.IsBundle ? 14 : 20;
                CGTP_background.RadiusY = Product.IsBundle ? 14 : 20;
                CGTP_bundle.Visibility = Product.IsBundle ? Visibility.Visible : Visibility.Collapsed;
                CGTP_class_type_role.Visibility = Product.IsBundle ? Visibility.Collapsed : Visibility.Visible;
                #region Набор
                if (Product.IsBundle) CGTP_bundle.Text = Dictionary.RotationsCount(Product.Rotations.Count);
                #endregion
                #region Ротация
                else
                {
                    var rotation = Product.Rotations.FirstOrDefault();
                    if (rotation is null) throw new UExcept(EInitialization.NoRotation, $"Продукт не содержит ротаций");

                    (CGTP_CTR_class.Background as ImageBrush)!.ImageSource = GStatic.GetRotationClassIcon(rotation.Class);
                    (CGTP_CTR_type.Background as ImageBrush)!.ImageSource = GStatic.GetRotationTypeIcon(rotation.Type);
                    (CGTP_CTR_role.Background as ImageBrush)!.ImageSource = GStatic.GetRotationRoleIcon(rotation.Role);
                }
                #endregion
                #endregion
                #region Загрузка изображения
                var imageSource = ImageControlHelper.GetImageFromCache(Product.ImageUrl, (int)Math.Ceiling(this.Width), (int)Math.Ceiling(this.Height));
                if (imageSource is null) 
                {
                    await CG_image_skeleton.ChangeState(true);
                    _ = ImageControlHelper.LoadImageAsync
                    (
                        Product.ImageUrl, 
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
                #endregion
                #region Нижняя панель
                #region Данные
                CGBP_name.Text = AppSettings.Instance.Language switch
                {
                    ELang.En => Product.NameEn,
                    _ => Product.Name
                };
                CGBP_duration.Text = Dictionary.DaysCount(Product.Duration);
                CGBP_price.Text = $"{Product.Price.ToOut()} {Product.Currency.ToString().ToUpper()}";
                #endregion
                #region Обновления вида
                UpdateView();
                UpdateButtonsPanelView();
                #endregion
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
    }
}
