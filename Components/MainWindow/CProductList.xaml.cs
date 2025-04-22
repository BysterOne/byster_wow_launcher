using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Any.GlobalEnums;
using Launcher.Api.Models;
using Launcher.Cls;
using Launcher.Components.DialogBox;
using Launcher.Settings;
using Launcher.Windows;
using Launcher.Windows.AnyMain.Enums;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Launcher.Components.MainWindow
{
    /// <summary>
    /// Логика взаимодействия для CProductList.xaml
    /// </summary>
    public partial class CProductList : UserControl
    {
        public CProductList()
        {
            InitializeComponent();

            DataContext = this;
        }

        #region Переменные
        public static LogBox Pref { get; set; } = new("Product List");
        public ObservableCollection<Product> ProductList { get; set; } = new();
        #endregion

        #region Функции
        #region Initialization
        public async Task<UResponse> Initialization()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось инициализировать список продуктов";

            #region try
            try
            {
                #region Загрузка ротаций
                foreach (var product in GProp.Products)
                {
                    ProductList.Add(product);
                    await Task.Run(() => Thread.Sleep(10));
                }
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
        #region ApplyFiltersAndSort
        public async Task ApplyFiltersAndSort(bool showLoader)
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось применить фильтры";

            #region try
            try
            {
                if (showLoader) await Main.Loader(ELoaderState.Show);
                #region Время сейчас
                SrollPanel.ScrollTo(0);
                var timeStart = DateTime.Now;
                #endregion
                #region Переменные
                var collection = ProductList;
                #region Порядок сортировки классов
                var classOrder = new List<ERotationClass>();
                foreach (var cls in Enum.GetValues<ERotationClass>()) classOrder.Add(cls);
                #endregion
                #region Порядок сортировки типов
                var typeOrder = new List<ERotationType>();
                foreach (var cls in Enum.GetValues<ERotationType>()) typeOrder.Add(cls);
                #endregion
                #region Порядок сортировки ролей
                var roleOrder = new List<ERotationRole>();
                foreach (var cls in Enum.GetValues<ERotationRole>()) roleOrder.Add(cls);
                #endregion
                #region Фильтры
                var filters = GProp.Filters;
                #endregion
                #endregion
                #region Сортировка и фильтрация
                #region Подготовка словарей
                var classMap = classOrder.Select((c, i) => new { c, i })
                                .ToDictionary(x => x.c, x => x.i);
                var typeMap = typeOrder.Select((t, i) => new { t, i })
                                         .ToDictionary(x => x.t, x => x.i);
                var roleMap = roleOrder.Select((r, i) => new { r, i })
                                         .ToDictionary(x => x.r, x => x.i);
                #endregion
                #region Копия оригинального списка
                var original = ProductList.ToList();
                #endregion
                #region Фильтрация
                var filtered = GProp.Products
                .Where(p =>
                {
                    if (filters.IsBundle && !p.IsBundle) return false;

                    #region Функции для проверки
                    bool CheckType(ERotationType t) => !filters.Types.Any() || filters.Types.Contains(t);
                    bool CheckClass(ERotationClass c) => !filters.Classes.Any() || filters.Classes.Contains(c) || c is ERotationClass.Any;
                    #endregion

                    #region Для набора смотрим все ротации
                    if (p.IsBundle)
                    {
                        if (!p.Rotations.Any(r => CheckType(r.Type))) return false;
                        if (!p.Rotations.Any(r => CheckClass(r.Class))) return false;
                    }
                    #endregion
                    #region Для одиночный ротаций
                    else
                    {
                        var first = p.Rotations[0];
                        if (!CheckType(first.Type)) return false;
                        if (!CheckClass(first.Class)) return false;
                    }
                    #endregion
                    return true;
                });
                #endregion
                #region Сортировка
                var desired = filtered
                .OrderBy(p => p.IsBundle ? 1 : 0)
                #region Для наборов ставим большой ключ и ставим их в конец, остальное по класу типу и роли
                #region Класс
                .ThenBy(p =>
                {
                    if (p.IsBundle) return int.MaxValue;
                    var cls = p.Rotations[0].Class;
                    return classMap.TryGetValue(cls, out var idx) ? idx : int.MaxValue;
                })
                #endregion
                #region Тип
                .ThenBy(p =>
                {
                    if (p.IsBundle) return int.MaxValue;
                    var ty = p.Rotations[0].Type;
                    return typeMap.TryGetValue(ty, out var idx) ? idx : int.MaxValue;
                })
                #endregion
                #region Роль
                .ThenBy(p =>
                {
                    if (p.IsBundle) return int.MaxValue;
                    var rl = p.Rotations[0].Role;
                    return roleMap.TryGetValue(rl, out var idx) ? idx : int.MaxValue;
                })
                #endregion
                .ToList();
                #endregion

                #endregion
                #region Обновление списка
                #region Удаляем не нужные
                foreach (var toRemove in original.Except(desired).ToList())
                    collection.Remove(toRemove);
                #endregion
                #region Перемещаем те, что не на своих местах и добавляем новые
                for (int i = 0; i < desired.Count; i++)
                {
                    var item = desired[i];
                    if (i < collection.Count && ReferenceEquals(collection[i], item))
                        continue;

                    if (collection.Contains(item))
                        collection.Move(collection.IndexOf(item), i);
                    else
                    {
                        collection.Insert(i, item);
                        await Task.Run(() => Thread.Sleep(15));
                    }
                }
                #endregion
                #endregion
                #endregion
                #region Если меньше 500 мс то ждем (что бы не было бликов)
                var span = DateTime.Now - timeStart;
                if (span.TotalMilliseconds < 500)
                    await Task.Run(() => Thread.Sleep(500 - (int)span.TotalMilliseconds));
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
            #region finally
            finally { if (showLoader) await Main.Loader(ELoaderState.Hide); }
            #endregion
        }
        #endregion
        #endregion
    }
}
