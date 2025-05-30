using Cls;
using Cls.Any;
using Cls.Exceptions;
using Launcher.Any;
using Launcher.Any.GlobalEnums;
using Launcher.Any.UDialogBox;
using Launcher.Api;
using Launcher.Cls;
using Launcher.Components.DialogBox;
using Launcher.Components.PanelChanger;
using Launcher.Components.PayDialogBoxAny;
using Launcher.PanelChanger.Enums;
using Launcher.Settings;
using Launcher.Settings.Enums;
using Launcher.Windows;
using QRCoder;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using EShow = Launcher.Components.PayDialogBoxAny.EShow;

namespace Launcher.Components
{
    namespace PayDialogBoxAny
    {
        public enum EPC_PanelChanger
        {
            Loader,
            PaymentConfirmation,
            PaymentDetails,
            Error,
        }

        public enum EShow
        {
            FailInitPanelChanger,
            FailGetPaymentSystems,
            FailLoadPaymentsToList
        }

        public enum EBuy
        {
            FailExecuteRequest
        }

        public enum EPayDialogBox
        {
            FailProcessingBuy
        }
    }
    /// <summary>
    /// Логика взаимодействия для CPayDialogBox.xaml
    /// </summary>
    public partial class CPayDialogBox : UserControl, IUDialogBox, ITranslatable
    {
        public CPayDialogBox()
        {
            InitializeComponent();

            Application.Current.Windows.OfType<Main>().First().PreviewKeyDown += EKeyDown;
        }

        #region Переменные
        
        private string? PaymentUrl { get; set; }
        private bool UseBonuses { get; set; } = false;
        private bool IsNeedUpdateData { get; set; } = false;
        private static LogBox Pref { get; set; } = new LogBox("Pay Dialog Box");
        private TaskCompletionSource<EDialogResponse> TaskCompletionSource { get; set; }
        private CPanelChanger<EPC_PanelChanger> PanelChanger { get; set; }
        #endregion

        #region Обработчики событий
        #region Background_MouseDown
        private void Background_MouseDown(object sender, MouseButtonEventArgs e) => Application.Current.Windows.OfType<Main>().First().DragMove();
        #endregion
        #region EKeyDown
        private async void EKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Escape)
            {
                e.Handled = true;
                TryClosing();
            }
        }
        #endregion
        #region MG_close_button_MouseLeftButtonDown
        private void MG_close_button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => TryClosing();
        #endregion
        #region MGCCAM_use_bonuses_changer_Clicked
        private void MGCCAM_use_bonuses_changer_Clicked(MainWindow.CFilterChanger sender, bool newValue)
        {
            UseBonuses = sender.IsActive;
            UpdatePaymentConfirmation();
        }
        #endregion
        #region MGCCAM_pay_MouseDown
        private void MGCCAM_pay_MouseDown(object sender, MouseButtonEventArgs e) => _ = Buy();
        #endregion
        #region MGCPI_button_MouseLeftButtonDown
        private void MGCPI_button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(PaymentUrl)) return;

            Process.Start(new ProcessStartInfo { FileName = PaymentUrl, UseShellExecute = true });
        }
        #endregion
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
                animation.Completed += (s, e) => { Panel.SetZIndex(element, -1); tcs.SetResult(null); };
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
            Panel.SetZIndex(element, 1);

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

        #region Функции
        #region Show
        public async Task<UResponse<EDialogResponse>> Show(params object[] p)
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось показать окно оплаты";

            #region try
            try
            {              
                #region Анимация появления
                var fadeInMiddle = AnimationHelper.OpacityAnimationStoryBoard(middleGrid, 1);
                var fadeIn = AnimationHelper.OpacityAnimationStoryBoard(this, 1);
                fadeIn.Completed += (s, e) => { fadeInMiddle.Begin(middleGrid, HandoffBehavior.SnapshotAndReplace, true); };
                fadeIn.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
                #endregion
                #region Создание задачи
                TaskCompletionSource = new();
                #endregion
                #region Лоадер
                MGC_loader_panel.Visibility = Visibility.Visible;
                MGCLP_loader.StartAnimation();
                #endregion
                #region Переключатель панелей
                PanelChanger = new
                (
                    MG_content,
                    [
                        new(EPC_PanelChanger.Loader, MGC_loader_panel),
                        new(EPC_PanelChanger.PaymentConfirmation, MGC_cart_and_method),
                        new(EPC_PanelChanger.PaymentDetails, MGC_payment_info),
                        new(EPC_PanelChanger.Error, MGC_error),
                    ],
                    defaultPanel: EPC_PanelChanger.Loader,
                    defaultState: EPanelState.Showen,
                    IsHitTestMonitor: false
                );
                PanelChanger.ShowElement += PanelChangerShow;
                PanelChanger.HideElement += PanelChangerHide;
                var tryInitPanelChanger = await PanelChanger.Init();
                if (!tryInitPanelChanger.IsSuccess)
                {
                    throw new UExcept(EShow.FailInitPanelChanger, $"Ошибка инициализации панели {nameof(PanelChanger)}");
                }
                #endregion
                #region Обновление языка
                _ = UpdateAllValues();
                #endregion


                #region Загрузка методов оплаты
                var tryGetPaymentMethods = await CApi.GetPaymentSystems();
                if (!tryGetPaymentMethods.IsSuccess)
                {
                    throw new UExcept(EShow.FailGetPaymentSystems, $"Не удалось получить способы оплаты", tryGetPaymentMethods.Error);
                }
                GProp.PaymentSystems = tryGetPaymentMethods.Response;
                #endregion
                #region Проверка и установка
                if (GProp.PaymentSystems.Count is 0)
                {
                    MGCLP_loader.StopAnimation();
                    Error(Dictionary.Translate($"На данный момент нет доступных методов оплаты"));                    
                }
                else
                {
                    MGCCAM_count_value.Text = GProp.Cart.Items.Count().ToString();
                    MGCCAM_sum_value.Text = $"{GProp.Cart.Sum.ToOut()} {GProp.User.Currency.ToUpper()}";

                    var items = new List<CList.CListItem>();
                    foreach (var item in GProp.PaymentSystems)
                    {
                        items.Add
                        (
                            new CList.CListItem
                            (
                                item.Id, 
                                AppSettings.Instance.Language switch
                                {
                                    ELang.En or ELang.ZhCn => item.NameEn,
                                    _ => item.Name  
                                }
                            )
                        );
                    }

                    var tryLoadList = await MGCCAM_methods_list.LoadItems(items);
                    if (!tryLoadList.IsSuccess)
                    {
                        throw new UExcept(EShow.FailLoadPaymentsToList, $"Не удалось установить методы оплаты в список", tryLoadList.Error);
                    }
                    
                    await Task.Run(() => Thread.Sleep(300));
                    MGCLP_loader.StopAnimation();
                    await PanelChanger.ChangePanel(EPC_PanelChanger.PaymentConfirmation);
                }
                #endregion

                #region Ждем результата
                var result = await TaskCompletionSource.Task;
                if (IsNeedUpdateData) _ = GProp.Update(ELauncherUpdate.User | ELauncherUpdate.Subscriptions);
                #endregion

                return new(result);
            }
            #endregion
            #region UExcept
            catch (UExcept ex)
            {
                return new(ex);
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uex = new UExcept(GlobalErrors.Exception, $"Исключение: {ex.Message}", ex);
                Functions.Error(uex, $"{_failinf}: исключение", _proc);
                return new(uex);
            }
            #endregion
        }
        #endregion
        #region Hide
        public async Task Hide()
        {
            var tcs = new TaskCompletionSource<object?>();

            var storyboard = new Storyboard();
            var ease = new PowerEase() { EasingMode = EasingMode.EaseInOut };

            var animationHideGrid = new DoubleAnimation(0, AnimationHelper.AnimationDuration) { EasingFunction = ease };
            Storyboard.SetTarget(animationHideGrid, middleGrid);
            Storyboard.SetTargetProperty(animationHideGrid, new PropertyPath(OpacityProperty));

            var animationHide = new DoubleAnimation(0, AnimationHelper.AnimationDuration) { EasingFunction = ease };
            animationHide.BeginTime = AnimationHelper.AnimationDuration;
            Storyboard.SetTarget(animationHide, this);
            Storyboard.SetTargetProperty(animationHide, new PropertyPath(OpacityProperty));

            storyboard.Children.Add(animationHideGrid);
            storyboard.Children.Add(animationHide);

            storyboard.Completed += (_, __) => { tcs.SetResult(null); };

            storyboard.Begin(middleGrid, HandoffBehavior.SnapshotAndReplace, true);

            await tcs.Task;
        }
        #endregion
        #region Error
        private void Error(string error)
        {
            MGCE_text.Text = error;
            _ = PanelChanger.ChangePanel(EPC_PanelChanger.Error);
        }
        #endregion
        #region UpdateAllValues
        public async Task UpdateAllValues()
        {
            MGCCAM_header.Text = Dictionary.Translate($"Оформление заказа");
            MGCCAM_count.Text = Dictionary.Translate($"Кол-во товаров");
            MGCCAM_use_bonuses.Text = Dictionary.Translate($"Использовать бонусы");
            MGCCAM_sum.Text = Dictionary.Translate($"Сумма");
            MGCCAM_payment_method.Text = Dictionary.Translate($"Метод");
            MGCCAM_pay.Text = Dictionary.Translate($"Оплатить");
            MGCPI_button.Text = Dictionary.Translate($"Открыть в браузере");
        }
        #endregion
        #region UpdatePaymentConfirmation
        private void UpdatePaymentConfirmation()
        {
            var sum = GProp.Cart.Sum;
            if (UseBonuses)
            {
                if (GProp.User.Balance >= sum) sum = 0;
                else sum -= GProp.User.Balance;
            }

            MGCCAM_sum_value.Text = 
                sum > 0 ?
                $"{sum.ToOut()} {GProp.User.Currency.ToUpper()}" :
                Dictionary.Translate("Бесплатно");
        }
        #endregion
        #region Buy
        private async Task Buy()
        {

            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось выполнить покупку";

            #region try
            try
            {
                #region Sentry
                SentryExtensions.BuyTransaction = SentrySdk.StartTransaction("buy", "buying");
                SentrySdk.ConfigureScope(scope => scope.Transaction = SentryExtensions.BuyTransaction);
                var buyTrans = SentryExtensions.BuyTransaction;
                #endregion
                #region Бонусы
                var bonuses =
                    UseBonuses ?
                        GProp.User.Balance >= GProp.Cart.Sum ?
                            GProp.Cart.Sum :
                            GProp.User.Balance
                        : 0;
                #endregion
                #region Прелоадер
                MGCLP_loader.StartAnimation();
                await PanelChanger.ChangePanel(EPC_PanelChanger.Loader);
                #endregion
                #region Покупка
                var buyRequest = buyTrans?.StartChild("send-buy-request");
                var tryBuy = await CApi.Buy(GProp.Cart.Items.ToList(), GProp.PaymentSystems.First(x => x.Id == MGCCAM_methods_list.SelectedItem!.Id), bonuses);
                if (!tryBuy.IsSuccess) throw new UExcept(EBuy.FailExecuteRequest, $"Не удалось выполнить запрос", tryBuy.Error);
                buyRequest?.Finish();
                #endregion
                #region Сразу чистим карзину
                while (GProp.Cart.Items.Count > 0) GProp.Cart.RemoveItem(GProp.Cart.Items.First().Product);
                #endregion
                #region Если не требуется оплата
                if (tryBuy.Response.Status is 2)
                {
                    _ = GProp.Update(ELauncherUpdate.User | ELauncherUpdate.Subscriptions);

                    MGCPI_success_icon.Visibility = Visibility.Visible;
                    MGCPI_qr_code.Visibility = Visibility.Collapsed;
                    MGCPI_button.Visibility = Visibility.Collapsed;

                    MGCPI_text.Text = Dictionary.Translate("Оплата прошла успешно. Можете закрыть данное окно");

                    await Task.Run(() => Thread.Sleep(300));
                    await PanelChanger.ChangePanel(EPC_PanelChanger.PaymentDetails);
                    MGCLP_loader.StopAnimation();
                    buyTrans?.Finish();
                    return;
                }
                PaymentUrl = tryBuy.Response.PaymentUrl;
                #endregion

                #region Код и ссылка
                var genCodeSpan = buyTrans?.StartChild("generating-qr-code");
                using var qrGenerator = new QRCodeGenerator();
                using var qrData = qrGenerator.CreateQrCode(PaymentUrl, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new QRCode(qrData);
                using Bitmap qrBitmap = qrCode.GetGraphic(30);
                MGCPI_qr_code_image.Source = Functions.ConvertBitmapToBitmapImage(qrBitmap);
                MGCPI_text.Text = Dictionary.Translate("Перейдите на сайт платежной системы и завершите платеж. После оплаты закройте данное окно");
                genCodeSpan?.Finish();
                #endregion
                #region Окно оплаты
                IsNeedUpdateData = true;
                await PanelChanger.ChangePanel(EPC_PanelChanger.PaymentDetails);
                MGCLP_loader.StopAnimation();
                #endregion
                buyTrans?.Finish();
            }
            #endregion
            #region UExcept
            catch (UExcept ex)
            {
                Functions.Error(ex, _failinf, _proc);
                Error(Dictionary.Translate($"Не удалось выполнить покупку. Попробуйте позже или обратитесь в поддержку"));
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uex = new UExcept(GlobalErrors.Exception, $"Исключение: {ex.Message}", ex);
                Functions.Error(uex, $"{_failinf}: исключение", _proc);
                Error(Dictionary.Translate($"Не удалось выполнить покупку. Попробуйте позже или обратитесь в поддержку"));
            }
            #endregion
            #region finally
            finally
            {
                SentryExtensions.BuyTransaction = null;
            }
            #endregion
        }
        #endregion
        #region TryClosing
        private void TryClosing()
        {
            Application.Current.Windows.OfType<Main>().First().PreviewKeyDown -= EKeyDown;
            TaskCompletionSource?.TrySetResult(EDialogResponse.Closed);
        }
        #endregion

        #endregion

        
    }
}
