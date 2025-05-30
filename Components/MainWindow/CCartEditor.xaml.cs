using Cls.Any;
using Launcher.Any;
using Launcher.Any.UDialogBox;
using Launcher.Components.MainWindow.Any.CartEditor;
using Launcher.Components.MainWindow.Any.PageShop.Models;
using Launcher.Components.PanelChanger;
using Launcher.Settings;
using Launcher.Windows;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Launcher.Components.MainWindow
{
    namespace Any.CartEditor
    {
        public enum EPC_CartEditor 
        {
            NoItems,
            Elements
        }
    }
    /// <summary>
    /// Логика взаимодействия для CartEditor.xaml
    /// </summary>
    public partial class CCartEditor : UserControl, ITranslatable
    {
        public CCartEditor()
        {
            InitializeComponent();
            TranslationHub.Register(this);

            GProp.Cart.CartUpdated += ECartUpdated;
            GProp.Cart.CartSumUpdated += ECartSumUpdated;            

            PanelChanger = new CPanelChanger<EPC_CartEditor>
            (
                cart_items_panel,
                [
                    new(EPC_CartEditor.NoItems, CIP_empty),
                    new(EPC_CartEditor.Elements, CIP_elements)
                ],
                EPC_CartEditor.NoItems
            );
            PanelChanger.HideElement += PanelChangerHide;
            PanelChanger.ShowElement += PanelChangerShow;
            _ = PanelChanger.Init();            
        }       

        #region Переменные
        public bool IsShowed { get; set; } = false;

        private CPanelChanger<EPC_CartEditor> PanelChanger { get; set; }
        #endregion

        #region Анимации
        #region PanelChangerHide
        private async Task PanelChangerHide(UIElement element, bool UseAnimation = true, bool Pending = true)
        {
            var duration = AnimationHelper.AnimationDuration;
            var tcs = new TaskCompletionSource<object?>();

            await Dispatcher.InvokeAsync(() =>
            {
                var animation = AnimationHelper.OpacityAnimationStoryBoard((FrameworkElement)element, 0, UseAnimation ? duration : TimeSpan.FromMilliseconds(1));
                animation.Completed += (s, e) => tcs.SetResult(null);
                animation.Begin((FrameworkElement)element, HandoffBehavior.SnapshotAndReplace, true);
            });

            if (Pending) { await tcs.Task; }
        }
        #endregion
        #region PanelChangerShow
        private async Task PanelChangerShow(UIElement element, bool UseAnimation = true, bool Pending = true)
        {
            var duration = AnimationHelper.AnimationDuration;
            var tcs = new TaskCompletionSource<object?>();

            await Dispatcher.InvokeAsync(() =>
            {
                element.Visibility = Visibility.Visible;

                var animation = AnimationHelper.OpacityAnimationStoryBoard((FrameworkElement)element, 1, UseAnimation ? duration : TimeSpan.FromMilliseconds(1));
                animation.Completed += (s, e) => tcs.SetResult(null);
                animation.Begin((FrameworkElement)element, HandoffBehavior.SnapshotAndReplace, true);
            });

            if (Pending) { await tcs.Task; }
        }
        #endregion
        #endregion

        #region Обработчики событий
        #region Background_MouseDown
        private void Background_MouseDown(object sender, MouseButtonEventArgs e) => Application.Current.Windows.OfType<Main>().First().DragMove();
        #endregion
        #region EKeyDown
        public void EKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Escape)
            {
                e.Handled = true;
                Application.Current.Windows.OfType<Main>().First().PreviewKeyDown -= EKeyDown;
                Main.ChangeCartEditorState(false);
            }
        }
        #endregion
        #region ECartSumUpdated
        private void ECartSumUpdated(double sum)
        {
            Dispatcher.Invoke(() => cart_sum.Text = $"{sum.ToOut()} {GProp.User.Currency.ToUpper()}");
        }
        #endregion
        #region ECartUpdated
        private void ECartUpdated(CCartItem item, ListChangedType changedType)
        {
            var countItems = GProp.Cart.Items.Count;
            var needPanel = countItems > 0 ? EPC_CartEditor.Elements : EPC_CartEditor.NoItems;
            if (PanelChanger.SelectedPanel != needPanel) _ = PanelChanger.ChangePanel(needPanel, false);
        }
        #endregion
        #region close_button_MouseLeftButtonDown
        private void close_button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Main.ChangeCartEditorState(false);
        #endregion
        #region buy_button_MouseDown
        private void buy_button_MouseDown(object sender, MouseButtonEventArgs e) => Main.GoToPayment();
        #endregion
        #endregion

        #region Функции
        #region UpdateAllValues
        public async Task UpdateAllValues()
        {
            MPH_text.Text = Dictionary.Translate("Корзина");
            buy_button.Text = Dictionary.Translate("К оплате");
            CIP_value.Text = Dictionary.Translate($"В данный момент корзина пустая");
        }
        #endregion

        #endregion

    }
}
