using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Any;
using Launcher.Any.GlobalEnums;
using Launcher.Cls;
using Launcher.Components.MainWindow.Any.PageShop.Models;
using Launcher.Settings;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Launcher.Components.MainWindow.Any.CartEditor
{
    /// <summary>
    /// Логика взаимодействия для CCartEditorItem.xaml
    /// </summary>
    public partial class CCartEditorItem : UserControl
    {
        public CCartEditorItem()
        {
            InitializeComponent();


        }

        #region Переменные
        public static LogBox Pref { get; set; } = new("Cart Editor Item");
        #endregion

        #region Свойства
        #region Item
        public CCartItem Item 
        {
            get { return (CCartItem)GetValue(GPropProperty); }
            set { SetValue(GPropProperty, value); }
        }

        public static readonly DependencyProperty GPropProperty =
            DependencyProperty.Register("Item", typeof(CCartItem), typeof(CCartEditorItem),
                new PropertyMetadata(null, OnItemChanged));

        private static void OnItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (d as CCartEditorItem)!;
            if (e.NewValue is not null) _ = sender.SetProductData();
        }
        #endregion
        #endregion

        #region Обработчики событий
        #region CG_remove_MouseLeftButtonDown
        private void CG_remove_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GProp.Cart.ChangeCount(Item.Product, (Item.Count - 1));
        }
        #endregion
        #region CG_add_MouseLeftButtonDown
        private void CG_add_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GProp.Cart.ChangeCount(Item.Product, (Item.Count + 1));
        }
        #endregion
        #region remove_button_MouseLeftButtonDown
        private void remove_button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GProp.Cart.RemoveItem(Item.Product);
        }
        #endregion
        #region ECartUpdated
        private void ECartUpdated(CCartItem item, ListChangedType changedType)
        {
            if (changedType is ListChangedType.ItemChanged) UpdateView();            
        }
        #endregion
        #endregion


        #region Функции
        #region SetProductData
        private async Task SetProductData()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось выполнить инициализацию";

            #region try
            try
            {
                #region Подпись на событие изменения
                GProp.Cart.CartUpdated += ECartUpdated;
                #endregion                
                #region Загрузка изображения
                var imageSource = ImageControlHelper.GetImageFromCache(Item.Product.ImageUrl, (int)image_grid.Width, (int)image_grid.Height);
                if (imageSource is null)
                {
                    await image_skeleton.ChangeState(true);
                    _ = ImageControlHelper.LoadImageAsync
                    (
                        Item.Product.ImageUrl,
                        (int)image_grid.Width,
                        (int)image_grid.Height,
                        new CancellationToken(),
                        (imgSource) =>
                        {
                            Dispatcher.Invoke(async () =>
                            {
                                image.Source = imgSource;
                                await image_skeleton.ChangeState(false);
                            },
                            DispatcherPriority.Background);
                        }
                    );
                }
                else
                {
                    image.Source = imageSource;
                }
                #endregion
                #region Данные
                product_name.Text = AppSettings.Instance.Language switch
                {
                    ELang.En or ELang.ZhCn => Item.Product.NameEn,                    
                    _ => Item.Product.Name
                };
                #endregion
                #region Обновления вида
                UpdateView();
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
        #region UpdateView
        private void UpdateView()
        {
            duration.Text = Dictionary.DaysCount(Item.Product.Duration * Item.Count);
            sum.Text = $"{(Item.Product.Price * Item.Count).ToOut()} {Item.Product.Currency.ToString().ToUpper()}";
            CG_count.Text = Item.Count.ToString();
        }
        #endregion
        #endregion        
    }
}
