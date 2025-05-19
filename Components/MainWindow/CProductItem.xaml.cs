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
using Launcher.Components.MainWindow.ProductItemAny;
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
    namespace ProductItemAny
    {
        public enum EButtonsType
        {
            Dynamic,
            Static,
        }

        public enum EViewType
        {
            Buy,
            InfoOnly
        }
    }
    /// <summary>
    /// Логика взаимодействия для CProductItem.xaml
    /// </summary>
    public partial class CProductItem : UserControl, ITranslatable
    {
        public CProductItem()
        {
            InitializeComponent();
            TranslationHub.Register(this);

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
        #region ViewType
        public EViewType ViewType
        {
            get => (EViewType)GetValue(ViewTypeProperty);
            set => SetValue(ViewTypeProperty, value);
        }

        public static readonly DependencyProperty ViewTypeProperty =
            DependencyProperty.Register(nameof(ViewType), typeof(EViewType), typeof(CProductItem),
                new PropertyMetadata(EViewType.Buy, OnViewTypeChanged));

        private static void OnViewTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (CProductItem)d;
            sender.UpdateView();
            sender.UpdateButtonsPanelView();
        }
        #endregion
        #region ButtonsType
        public EButtonsType ButtonsType
        {
            get => (EButtonsType)GetValue(ButtonsTypeProperty);
            set => SetValue(ButtonsTypeProperty, value);
        }

        public static readonly DependencyProperty ButtonsTypeProperty =
            DependencyProperty.Register(nameof(ButtonsType), typeof(EButtonsType), typeof(CProductItem),
                new PropertyMetadata(EButtonsType.Dynamic, OnButtonsTypeChanged));

        private static void OnButtonsTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (CProductItem)d;
            sender.UpdateView();
            sender.UpdateButtonsPanelView();
        }
        #endregion
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
        #region ButtonsPanelMargin
        public Thickness ButtonsPanelMargin
        {
            get => (Thickness)GetValue(ButtonsPanelMarginProperty);
            set => SetValue(ButtonsPanelMarginProperty, value);
        }

        public static readonly DependencyProperty ButtonsPanelMarginProperty =
            DependencyProperty.Register(nameof(ButtonsPanelMargin), typeof(Thickness), typeof(CProductItem),
                new(new Thickness(0, 0, 0, 5)));
        #endregion
        #region ButtonsMargin
        public Thickness ButtonsMargin
        {
            get => (Thickness)GetValue(ButtonsMarginProperty);
            set => SetValue(ButtonsMarginProperty, value);
        }

        public static readonly DependencyProperty ButtonsMarginProperty =
            DependencyProperty.Register(nameof(ButtonsMargin), typeof(Thickness), typeof(CProductItem),
                new (new Thickness(0, 0, 0, 10)));
        #endregion
        #region CountInCartBlockMargin
        public Thickness CountInCartBlockMargin
        {
            get => (Thickness)GetValue(CountInCartBlockMarginProperty);
            set => SetValue(CountInCartBlockMarginProperty, value);
        }

        public static readonly DependencyProperty CountInCartBlockMarginProperty =
            DependencyProperty.Register(nameof(CountInCartBlockMargin), typeof(Thickness), typeof(CProductItem),
                new(new Thickness(0, 0, 0, 15)));
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
        private void EMouseEnter(object sender, MouseEventArgs e) => ChangeButtonsPanelState();
        #endregion
        #region EMouseLeave
        private void EMouseLeave(object sender, MouseEventArgs e) => ChangeButtonsPanelState();
        #endregion
        #region CGBPBCT_test_PreviewMouseDoubleClick
        private async void CGBPBCT_test_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (!Product.CanTest) return;

            var boxSettings = new BoxSettings
            (
                Dictionary.Translate("Тест"),
                Dictionary.Translate($"Вы действительно хотите взять на тест данный продукт?\nДлительность: ") + Dictionary.HoursCount(GProp.User.TestDuration),
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
            e.Handled = true;
            if (CartItem is null) GProp.Cart.AddItem(Product);            
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
            e.Handled = true;
            if (CartItem is not null) GProp.Cart.ChangeCount(Product, (CartItem.Count - 1));
        }
        #endregion
        #region CGBPBC_add_MouseLeftButtonDown
        private void CGBPBC_add_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (CartItem is not null) GProp.Cart.ChangeCount(Product, (CartItem.Count + 1));
        }
        #endregion
        #endregion

        #region Функции
        #region GetButtonsHeight
        private double GetButtonsHeight()
        {
            if (CGBP_buttons.ActualHeight is not 0) return CGBP_buttons.ActualHeight;
            if (CartItem is not null) return 51;
            else if (Product.CanTest) return 105;
            else return 55;
        }
        #endregion
        #region UpdateView
        private void UpdateView()
        {
            if (Product is null) return;

            CGBP_buttons.Visibility = ViewType is EViewType.Buy ? Visibility.Visible : Visibility.Collapsed;
            CGBP_duration.Visibility = ViewType is EViewType.Buy ? Visibility.Visible : Visibility.Collapsed;
            CGBP_price.Visibility = ViewType is EViewType.Buy ? Visibility.Visible : Visibility.Collapsed;
            //CGBP_name_price.Margin = ViewType is EViewType.Buy ? new (10, 50, 10, 5) : new (10, 50, 10, ButtonsMargin.Bottom + 15);

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
            CGBP_buttons.SetValue
            (  
                MarginProperty, 

                this.IsMouseOver || ButtonsType is EButtonsType.Static ?
                ButtonsPanelMargin :
                new Thickness(0, 5, 0, -GetButtonsHeight())
            );
        }
        #endregion
        #region ChangeButtonsPanelState
        public void ChangeButtonsPanelState()
        {
            if (ButtonsType is EButtonsType.Static) return;
            if (ViewType is not EViewType.Buy) return;

            var newThickness =
                this.IsMouseOver ?
                ButtonsPanelMargin :
                new Thickness(0, 5, 0, -GetButtonsHeight());

            var storyboard = new Storyboard();
            var animation = new ThicknessAnimation(newThickness, AnimationHelper.AnimationDuration)
            {
                EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut },
            };
            
            Storyboard.SetTarget(animation, CGBP_buttons);
            Storyboard.SetTargetProperty(animation, new PropertyPath(Grid.MarginProperty));

            storyboard.Children.Add(animation);
            storyboard.Completed += (_, __) => { storyboard.Stop(CGBP_buttons); CGBP_buttons.Margin = (Thickness)animation.To!;  };

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
                #region Обновление языка
                _ = UpdateAllValues();
                #endregion
                #region Проверка наличия в корзине
                CartItem = GProp.Cart.GetItem(Product);
                GProp.Cart.CartUpdated += ECartUpdated;
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
                    _ = CG_image_skeleton.ChangeState(true, false);
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
        #region UpdateAllValues
        public async Task UpdateAllValues()
        {
            if (Product is not null)
            {
                CGBP_name.Text = AppSettings.Instance.Language switch
                {
                    ELang.En => Product.NameEn,
                    _ => Product.Name
                };
                CGBP_duration.Text = Dictionary.DaysCount(Product.Duration);
            }

            CGBPBCT_test.Text = Dictionary.Translate($"Тест");
            CGBPBCT_buy.Text = Dictionary.Translate($"В корзину");
        }
        #endregion
        #endregion
    }
}
