using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Api;
using Launcher.Cls;
using Launcher.Components.MainWindow.Any.PageShop.Errors;
using Launcher.Settings;
using Launcher.Windows.AnyMain.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Launcher.Components.MainWindow
{
    /// <summary>
    /// Логика взаимодействия для CPageShop.xaml
    /// </summary>
    public partial class CPageShop : UserControl
    {
        public CPageShop()
        {
            InitializeComponent();
        }

        #region Переменные
        public static LogBox Pref { get; set; } = new("Shop Page Component");
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
                #region Установка фильтров
                SetFilters();
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
        private void SetFilters()
        {
            #region Очистка
            FP_stack_panel.Children.Clear();
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
                    Margin = new (10, 2, 5, 2),
                };
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
        #endregion
    }
}
