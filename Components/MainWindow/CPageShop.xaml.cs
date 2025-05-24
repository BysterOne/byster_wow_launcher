using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Any;
using Launcher.Api;
using Launcher.Cls;
using Launcher.Components.MainWindow.Any.PageShop.Errors;
using Launcher.Components.MainWindow.Any.PageShop.Models;
using Launcher.Settings;
using Launcher.Windows;
using Launcher.Windows.AnyMain.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Launcher.Components.MainWindow
{
    /// <summary>
    /// Логика взаимодействия для CPageShop.xaml
    /// </summary>
    public partial class CPageShop : UserControl, ITranslatable
    {
        public CPageShop()
        {
            InitializeComponent();
            TranslationHub.Register(this);

            GProp.Cart.CartSumUpdated += ECartSumUpdated;
        }       

        #region Переменные
        public static LogBox Pref { get; set; } = new("Shop Page Component");
        private List<CFilterChanger> FilterClasses { get; set; } = [];        
        #endregion

        #region Функции
        #region Инициализация
        public async Task<UResponse> Initialization()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось выполнить инициализацию";

            #region try
            try
            {
                #region Загрузка списка продуктов
                var tryGetProducts = await CApi.GetProductsList();
                if (!tryGetProducts.IsSuccess)
                {
                    throw new UExcept(EInitialization.FailLoadProductsList, $"Ошибка загрузки продуктов", tryGetProducts.Error);
                }
                GProp.Products = tryGetProducts.Response;
                #endregion
                #region Инициализация компонента списка продуктов
                var tryInitProductList = await MP_products_list.Initialization();
                if (!tryInitProductList.IsSuccess)
                {
                    throw new UExcept(EInitialization.FailInitProductList, $"Ошибка инициализации списка продуктов", tryInitProductList.Error);
                }
                #endregion
                #region Установка переключателей фильтров
                SetupFiltersChangers();
                #endregion
                #region Первая загрузка фильтров
                UpdateFilters(true);
                #endregion
                #region Язык
                _ = UpdateAllValues();
                #endregion


                return new() { IsSuccess = true };
            }
            #endregion
            #region UExcept
            catch (UExcept ex)
            {
                return new(ex.Error);
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uerror = new UError(GlobalErrors.Exception, $"Исключение: {ex.Message}");
                Functions.Error(ex, uerror, $"{_failinf}: исключение", _proc);
                return new(uerror);
            }
            #endregion
        }
        #endregion
        #region SetFilters
        private void SetupFiltersChangers()
        {
            #region Очистка
            FP_stack_panel.Children.Clear();
            FilterClasses.Clear();
            #endregion
            #region Установка маркеров типа
            var hasBotType = GProp.Products.Any(x => x.Rotations.Any(y => y.Type == ERotationType.Bot));
            foreach (var rotation_type in Enum.GetValues<ERotationType>())
            {
                #region Игнорируем если нет хотя бы одного бота
                if (rotation_type is ERotationType.Bot && !hasBotType) { continue; }
                #endregion
                #region Создаем компонент
                var marker = new CFilterChanger()
                {
                    IsActive = false,
                    Text = GStatic.GetRotationTypeName(rotation_type),
                    Value = rotation_type,
                    ChangerType = EFilterChangerType.Marker,
                    Foreground = Functions.GlobalResources()["textcolor_main"] as SolidColorBrush,
                    FontFamily = Functions.GlobalResources()["fontfamily_main"] as FontFamily,
                    FontSize = 18,
                    Margin = new(10, 2, 5, 2),
                };
                marker.Clicked += FilterChanger_Clicked;
                #endregion
                FP_stack_panel.Children.Add(marker);
            }
            #endregion
            #region Полоска пропуска
            FP_stack_panel.Children.Add(FiltersSpaceLine());
            #endregion
            #region Набор
            var marker_bundle = new CFilterChanger()
            {
                IsActive = false,
                Text = Dictionary.Translate("Набор"),
                Value = EProductsType.Bundle,
                ChangerType = EFilterChangerType.Marker,
                Foreground = Functions.GlobalResources()["textcolor_main"] as SolidColorBrush,
                FontFamily = Functions.GlobalResources()["fontfamily_main"] as FontFamily,
                FontSize = 18,
                Margin = new(10, 2, 5, 2),
            };
            marker_bundle.Clicked += FilterChanger_Clicked;
            FP_stack_panel.Children.Add(marker_bundle);
            #endregion
            #region Полоска пропуска
            FP_stack_panel.Children.Add(FiltersSpaceLine());
            #endregion
            #region Фильтры по классу

            foreach (var rotation_class in Enum.GetValues<ERotationClass>().ToList().OrderByDescending(x => x is ERotationClass.Any))
            {
                var changer = new CFilterChanger()
                {
                    IsActive = true,
                    Value = rotation_class,
                    ChangerType = rotation_class is ERotationClass.Any ? EFilterChangerType.Marker : EFilterChangerType.Icon,
                    Icon = rotation_class is ERotationClass.Any ? null : GStatic.GetRotationClassIcon(rotation_class),
                    Text = rotation_class is ERotationClass.Any ? Dictionary.Translate("Все") : GStatic.GetRotationClassName(rotation_class),
                    Foreground = Functions.GlobalResources()["textcolor_main"] as SolidColorBrush,
                    FontFamily = Functions.GlobalResources()["fontfamily_main"] as FontFamily,
                    FontSize = 18,
                    Margin = new(10, 2, 5, 2),
                };
                changer.Clicked += FilterChanger_Clicked;
                FilterClasses.Add(changer);
                FP_stack_panel.Children.Add(changer);
            }
            #endregion
        }
        #endregion
        #region FiltersSpaceLine
        private Rectangle FiltersSpaceLine()
        {
            return new Rectangle()
            {
                Style = (Style)FindResource("RectangleFiltersSpacer")
            };
        }
        #endregion
        #region UpdateFilters
        private void UpdateFilters(bool isFirstLoad = false)
        {
            var Filters = GProp.Filters;

            #region Все переключатели
            var allChangers = FP_stack_panel.Children.OfType<CFilterChanger>().ToList();
            #endregion
            #region Набор или нет
            var bundleChanger = allChangers.FirstOrDefault(x => x.Value is EProductsType.Bundle);
            if (bundleChanger is not null) Filters.IsBundle = bundleChanger.IsActive;
            #endregion
            #region Типы
            var typeChangers = allChangers.Where(x => x.Value is ERotationType).ToList();
            var typeChangersActive = typeChangers.Where(x => x.IsActive).Select(x => (ERotationType)x.Value).ToList();
            Filters.Types = typeChangersActive;
            #endregion
            #region Классы
            var classChangers = allChangers.Where(x => x.Value is ERotationClass && (ERotationClass)x.Value != ERotationClass.Any).ToList();
            var activeClassChangers = classChangers.Where(x => x.IsActive).Select(x => (ERotationClass)x.Value).ToList();
            Filters.Classes = activeClassChangers;
            #endregion

            _ = MP_products_list.ApplyFiltersAndSort(!isFirstLoad);
        }
        #endregion
        #region HandleRotationClassChange
        private void HandleRotationClassChange(CFilterChanger sender, bool newValue)
        {
            var value = (ERotationClass)sender.Value;
            #region Если это "Все"
            if (value is ERotationClass.Any)
            {
                // Все переключатели, не соответствующие новому статусу "Все", должны поменять его на новый статус
                var needChange = FilterClasses.Where(x => (ERotationClass)x.Value != ERotationClass.Any && x.IsActive != newValue);
                foreach (var filter in needChange) filter.IsActive = newValue;
            }
            #endregion
            #region Если класс
            else
            {
                // Меняем статус переключателя "Все" в зависимоти от статуса всех остальных переключателей
                var anyChanger = FilterClasses.First(x => (ERotationClass)x.Value == ERotationClass.Any);
                anyChanger.IsActive = !(FilterClasses.Where(x => (ERotationClass)x.Value != ERotationClass.Any)).Any(x => !x.IsActive);
            }
            #endregion

            #region Обновление фильтров
            UpdateFilters();
            #endregion
        }
        #endregion
        #region UpdateAllValues
        public async Task UpdateAllValues()
        {
            MPBP_buy_button.Text = Dictionary.Translate($"К оплате");
            MPBP_cart.Text = Dictionary.Translate($"Корзина");

            #region Типы
            var types = FP_stack_panel.Children.OfType<CFilterChanger>().Where(x => x.Value.GetType() == typeof(ERotationType));
            foreach (var type in types) type.Text = GStatic.GetRotationTypeName((ERotationType)type.Value);
            #endregion
            #region Набор
            var bundle = FP_stack_panel.Children.OfType<CFilterChanger>().FirstOrDefault(x => x.Value is EProductsType.Bundle);
            if (bundle is not null) bundle.Text = Dictionary.Translate("Набор");
            #endregion
            #region Классы
            var classes = FP_stack_panel.Children.OfType<CFilterChanger>().Where(x => x.Value.GetType() == typeof(ERotationClass));
            foreach (var class_v in classes)
                if ((ERotationClass)class_v.Value is ERotationClass.Any)
                    class_v.Text = Dictionary.Translate("Все");
                else
                    class_v.Text = GStatic.GetRotationClassName((ERotationClass)class_v.Value);
            #endregion
        }
        #endregion
        #endregion

        #region Обработчики событий
        #region FilterChanger_Clicked
        private void FilterChanger_Clicked(CFilterChanger sender, bool newValue)
        {
            var type = sender.Value.GetType();

            switch (type)
            {
                case Type t when t == typeof(ERotationClass):
                    HandleRotationClassChange(sender, newValue);
                    break;
                case Type t when t == typeof(ERotationType):
                    UpdateFilters();
                    break;
                case Type t when t == typeof(EProductsType):
                    UpdateFilters();
                    break;
                default:
                    throw new NotSupportedException($"Необрабатываемый тип фильтра: {type.Name}.{sender.Value}");
            }
        }
        #endregion
        #region ECartSumUpdated
        private void ECartSumUpdated(double sum)
        {
            MPBP_cart_sum.Text = $"{sum.ToOut()} {GProp.User.Currency.ToUpper()}";
        }
        #endregion
        #region MPBP_cart_MouseLeftButtonDown
        private void MPBP_cart_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Main.ChangeCartEditorState(true);
        }
        #endregion
        #region MPBP_buy_button_MouseDown
        private void MPBP_buy_button_MouseDown(object sender, MouseButtonEventArgs e) => Main.GoToPayment();
        #endregion
        #endregion

    }
}
