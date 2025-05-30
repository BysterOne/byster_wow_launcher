using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Any;
using Launcher.Any.GlobalEnums;
using Launcher.Any.UDialogBox;
using Launcher.Api.Models;
using Launcher.Cls;
using Launcher.Components.DialogBox;
using Launcher.Components.MainWindow.Any.PageMain.CProductInfoDialogBoxAny;
using Launcher.Components.PanelChanger;
using Launcher.PanelChanger.Enums;
using Launcher.Settings;
using Launcher.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using EShow = Launcher.Components.MainWindow.Any.PageMain.CProductInfoDialogBoxAny.EShow;

namespace Launcher.Components.MainWindow.Any.PageMain
{
    namespace CProductInfoDialogBoxAny
    {
        public enum EPC_Panels
        {
            Description,
            Rotations
        }

        public enum EInitialization
        {
            FailInitPanelChanger
        }

        public enum EShow
        {
            NoObjectOfTheRequiredType,
        }
    }

    /// <summary>
    /// Логика взаимодействия для CProductInfoDialogBox.xaml
    /// </summary>
    public partial class CProductInfoDialogBox : UserControl, IUDialogBox, ITranslatable    
    {
        public CProductInfoDialogBox()
        {
            InitializeComponent();
            TranslationHub.Register(this);

            this.DataContext = this;
            middleGrid.Opacity = 0;

            Application.Current.Windows.OfType<Main>().First().PreviewKeyDown += EKeyDown;
        }

        #region Переменнные
        public static LogBox Pref { get; set; } = new("Product Info Dialog Box");
        private CPanelChanger<EPC_Panels> PanelChanger { get; set; }
        private TaskCompletionSource<EDialogResponse> TaskCompletion { get; set; }
        #endregion

        #region Свойства
        #region Product
        public Product Product
        {
            get => (Product)GetValue(ProductProperty);
            set => SetValue(ProductProperty, value);
        }

        public static readonly DependencyProperty ProductProperty =
            DependencyProperty.Register(nameof(Product), typeof(Product), typeof(CProductInfoDialogBox));

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

        #region Обработка событий
        #region Background_MouseDown
        private void Background_MouseDown(object sender, MouseButtonEventArgs e) => Application.Current.Windows.OfType<Main>().First().DragMove();
        #endregion
        #region EKeyDown
        private void EKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Escape)
            {
                e.Handled = true;
                Application.Current.Windows.OfType<Main>().First().PreviewKeyDown -= EKeyDown;
                TaskCompletion?.TrySetResult(EDialogResponse.Closed);
            }
        }
        #endregion
        #region MGRP_description_MouseDown
        private void MGRP_description_MouseDown(object sender, MouseButtonEventArgs e) => ChangePanel(EPC_Panels.Description);
        #endregion
        #region MGRP_rotations_MouseDown
        private void MGRP_rotations_MouseDown(object sender, MouseButtonEventArgs e) => ChangePanel(EPC_Panels.Rotations);
        #endregion
        #region MG_close_button_MouseLeftButtonDown
        private void MG_close_button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => TaskCompletion.SetResult(EDialogResponse.Closed);
        #endregion
        #endregion

        #region Функци
        #region Show
        public async Task<UResponse<EDialogResponse>> Show(params object[] pars)
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось инициализировать компонент";

            #region try
            try
            {
                #region Проверка объекта
                var product = pars.OfType<Api.Models.Product>().FirstOrDefault();
                if (product is null) throw new UExcept(EShow.NoObjectOfTheRequiredType, $"Требуется объект типа {typeof(Api.Models.Product).FullName}");
                Product = product;
                #endregion
                #region Язык
                _ = UpdateAllValues();
                #endregion
                #region Анимация появления
                var fadeInMiddle = AnimationHelper.OpacityAnimationStoryBoard(middleGrid, 1);
                var fadeIn = AnimationHelper.OpacityAnimationStoryBoard(this, 1);
                fadeIn.Completed += (s, e) => { fadeInMiddle.Begin(middleGrid, HandoffBehavior.SnapshotAndReplace, true); };
                fadeIn.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
                #endregion
                #region Задача
                TaskCompletion = new();
                #endregion
                #region Описание
                MGRPDP_textblock.Text = AppSettings.Instance.Language switch
                {
                    ELang.En or ELang.ZhCn => product.DescriptionEn,
                    _ => product.Description
                };
                #endregion
                #region Переключатель панелей
                PanelChanger = new
                (
                    MG_right_panel,
                    [
                        new(EPC_Panels.Description, MGRP_description_panel),
                        new(EPC_Panels.Rotations, MGRP_rotations_panel)
                    ],
                    defaultPanel: EPC_Panels.Description,
                    defaultState: EPanelState.Showen
                );
                PanelChanger.ShowElement += PanelChangerShow;
                PanelChanger.HideElement += PanelChangerHide;
                var tryInitPanelChanger = await PanelChanger.Init();
                if (!tryInitPanelChanger.IsSuccess)
                {
                    throw new UExcept(EInitialization.FailInitPanelChanger, $"Ошибка инициализации панели {nameof(PanelChanger)}");
                }
                #endregion
                     

                return new(await TaskCompletion.Task);
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
                return new(new UExcept(GlobalErrors.Exception, $"Исключение: {ex.Message}", ex));
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
        #region ChangePanel
        private void ChangePanel(EPC_Panels panel)
        {
            if (!IsInitialized) return;

            #region Переключатели панелей
            MGRP_description.IsActive = panel is EPC_Panels.Description;
            MGRP_rotations.IsActive = panel is EPC_Panels.Rotations;
            #endregion

            #region Панели
            _ = PanelChanger.ChangePanel(panel);
            #endregion
        }
        #endregion
        #region UpdateAllValues
        public async Task UpdateAllValues()
        {
            MGRP_description.Text = Dictionary.Translate("ОПИСАНИЕ");
            MGRP_rotations.Text = Dictionary.Translate("РОТАЦИИ");

            await MG_product.UpdateAllValues();
        }
        #endregion

        #endregion

        
    }
}
