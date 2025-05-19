using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Any;
using Launcher.Any.GlobalEnums;
using Launcher.Any.UDialogBox;
using Launcher.Api.Models;
using Launcher.Cls;
using Launcher.Components.MainWindow.Any.PageMain.CRotationInfoDialogBoxAny;
using Launcher.Components.MainWindow.ProductItemAny;
using Launcher.Components.PanelChanger;
using Launcher.PanelChanger.Enums;
using Launcher.Settings;
using Launcher.Windows.AnyMain.Enums;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Rotation = Launcher.Api.Models.Rotation;

namespace Launcher.Components.MainWindow.Any.PageMain
{
    namespace CRotationInfoDialogBoxAny
    {
        public enum EShow
        {
            InvalidParameterType,
            ProductTypeNotSupported,
            ProductRotationMissing,
            FailInitPanelChanger
        }

        public enum EPC_PanelChanger
        {
            Description,
            Galery
        }
    }
    /// <summary>
    /// Логика взаимодействия для CRotationInfoDialogBox.xaml
    /// </summary>
    public partial class CRotationInfoDialogBox : UserControl, IUDialogBox, ITranslatable
    {
        public CRotationInfoDialogBox()
        {
            InitializeComponent();
            TranslationHub.Register(this);

            this.Opacity = 0;
            this.middleGrid.Opacity = 0;
        }

        #region Переменные
        public static LogBox Pref { get; set; } = new("Rotation Info Dialog Box");
        private TaskCompletionSource<EDialogResponse> TaskCompletion { get; set; }        
        private CPanelChanger<EPC_PanelChanger> PanelChanger { get; set; }
        public Product Product { get; set; }
        #endregion

        #region Свойства
        #region ViewType
        public EViewType ViewType 
        {
            get => (EViewType)GetValue(ViewTypeProperty);
            set => SetValue(ViewTypeProperty, value);
        }

        public static readonly DependencyProperty ViewTypeProperty =
            DependencyProperty.Register(nameof(ViewType), typeof(EViewType), typeof(CRotationInfoDialogBox),
                new (EViewType.Buy));
        #endregion
        #endregion

        #region Обработчики событий
        #region MG_close_button_MouseLeftButtonDown
        private void MG_close_button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { TaskCompletion.TrySetResult(EDialogResponse.Closed); }
        #endregion
        #region MGRP_description_MouseDown
        private void MGRP_description_MouseDown(object sender, MouseButtonEventArgs e) => ChangePanel(EPC_PanelChanger.Description);
        #endregion
        #region MGRP_galery_MouseDown
        private void MGRP_galery_MouseDown(object sender, MouseButtonEventArgs e) => ChangePanel(EPC_PanelChanger.Galery);
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
        public async Task<UResponse<EDialogResponse>> Show(params object[] pars)
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось инициализировать компонент";

            #region try
            try
            {               
                #region Установка задачи
                TaskCompletion = new ();
                #endregion
                #region Язык
                _ = UpdateAllValues();
                #endregion
                #region Проверка объекта
                var product = pars.OfType<Product>().FirstOrDefault();
                var rotation = pars.OfType<Rotation>().FirstOrDefault();
                if (product is null && rotation is null) throw new UExcept(EShow.InvalidParameterType, $"Ожидается один параметр типа {typeof(Product).Name} или {typeof(Rotation).Name}");

                if (product is not null)
                {
                    if (product.IsBundle) throw new UExcept(EShow.ProductTypeNotSupported, $"Данный компонент не принимает {nameof(Product)} с параметром {nameof(product.IsBundle)} = true");
                    ViewType = EViewType.Buy;
                    Product = product;
                    rotation = product.Rotations.FirstOrDefault();
                    if (rotation is null) throw new UExcept(EShow.ProductRotationMissing, $"У продукта должна быть хотя бы одна ротация");                   
                }
                else if (rotation is not null)
                {
                    ViewType = EViewType.InfoOnly;
                    Product = new()
                    {
                        Name = rotation.Name,
                        NameEn = rotation.Name,
                        ImageUrl = rotation.Image,
                        Rotations = [rotation]
                    };
                }
                #endregion   
                #region Анимация появления
                var fadeInMiddle = AnimationHelper.OpacityAnimationStoryBoard(middleGrid, 1);
                var fadeIn = AnimationHelper.OpacityAnimationStoryBoard(this, 1);
                fadeIn.Completed += (s, e) => { fadeInMiddle.Begin(middleGrid, HandoffBehavior.SnapshotAndReplace, true); };
                fadeIn.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
                #endregion

                #region Установка продукта
                MG_product.Product = Product;
                #endregion
                #region Описание   
                _ = MGRPDP_skeleton.ChangeState(true, false);
                _ = MdHelper.BuildDocMarkdigAsync
                (
                    AppSettings.Instance.Language switch 
                    { 
                        ELang.En => rotation.DescriptionEn,
                        _ => rotation.Description
                    }, 
                    async (doc) =>
                    {
                        MGRPDP_markdown.Document = doc;
                        await MGRPDP_skeleton.ChangeState(false);
                    }
                );
                #endregion

                #region Переключатель панелей
                PanelChanger = new
                (
                    MG_right_panel,
                    [
                        new(EPC_PanelChanger.Description, MGRP_description_panel),
                        new(EPC_PanelChanger.Galery, MGRP_galery_panel)
                    ],
                    defaultPanel: EPC_PanelChanger.Description,
                    defaultState: EPanelState.Showen
                );
                PanelChanger.ShowElement += PanelChangerShow;
                PanelChanger.HideElement += PanelChangerHide;
                var tryInitPanelChanger = await PanelChanger.Init();
                if (!tryInitPanelChanger.IsSuccess)
                {
                    throw new UExcept(EShow.FailInitPanelChanger, $"Ошибка инициализации панели {nameof(PanelChanger)}");
                }
                #endregion

                #region Галерея
                MGRPGP_no_media.Visibility = rotation.Media.Count is 0 ? Visibility.Visible : Visibility.Collapsed;
                MGRPGP_list.Visibility = rotation.Media.Count is 0 ? Visibility.Collapsed : Visibility.Visible;

                if (rotation.Media.Count is not 0)
                {
                    foreach (var media in rotation.Media)
                    {
                        /// Потому что сука для mp4 поставили тип image
                        var ex = media.Url.Split('.').LastOrDefault();
                        if
                        (
                            ex is not null &&
                            ex.Equals("mp4", StringComparison.CurrentCultureIgnoreCase) &&
                            media.Type is EMediaType.Image
                        )
                        {
                            media.Type = EMediaType.Video;
                        }

                        MGRPGP_list.Medias.Add(media);
                    }
                }
                #endregion

                return new(await TaskCompletion.Task);
            }
            #endregion
            #region UExcept
            catch (UExcept ex)
            {
                Functions.Error(ex, _failinf, _proc);
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
        #region UpdateAllValues
        public async Task UpdateAllValues()
        {
            MGRP_description.Text = Dictionary.Translate("ОПИСАНИЕ");
            MGRP_galery.Text = Dictionary.Translate("ГАЛЕРЕЯ");
            MGRPGPNM_text.Text = Dictionary.Translate("Для этого товара нет изображений");
        }
        #endregion        
        #region ChangePanel
        private void ChangePanel(EPC_PanelChanger panel)
        {
            #region Переключатели панелей
            MGRP_description.IsActive = panel is EPC_PanelChanger.Description;
            MGRP_galery.IsActive = panel is EPC_PanelChanger.Galery;
            #endregion

            #region Панели
            _ = PanelChanger.ChangePanel(panel);
            #endregion
        }
        #endregion
        #endregion

    }
}
