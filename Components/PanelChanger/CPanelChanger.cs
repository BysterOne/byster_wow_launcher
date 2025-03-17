using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using ControlCenter.PanelChanger.Enums;
using ControlCenter.PanelChanger.Errors;
using ControlCenter.PanelChanger.Models;
using System.Windows;

namespace Launcher.Components.PanelChanger
{
    public class CPanelChanger<T> where T : struct, Enum
    {
        public delegate Task ShowHideElementDelegate(UIElement element, bool UseAnimation = true, bool Pending = true);
        public event ShowHideElementDelegate ShowElement = null!;
        public event ShowHideElementDelegate HideElement = null!;

        private LogBox Pref { get; set; } = new($"Panel Changer");
        public EPanelState State { get; set; }
        public UIElement Parent { get; set; }
        public T? DefaultPanel { get; set; }
        public T SelectedPanel { get; set; }
        public List<CChangerItem<T>> Panels { get; set; } = new();

        public CPanelChanger(UIElement parent, List<CChangerItem<T>> panels, T? defaultPanel = null, EPanelState defaultState = EPanelState.Showen, bool IsHitTestMonitor = true)
        {
            Parent = parent;
            Panels = panels;
            DefaultPanel = defaultPanel;
            State = defaultState;
            Any.OpacityMonitor.Monitor(parent);
            foreach (var panel in panels) Any.OpacityMonitor.Monitor(panel.Element, IsHitTestMonitor);
        }

        #region Init
        public async Task<UResponse> Init()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось выполнить инициацию";

            #region try
            try
            {
                #region Проверка
                if (DefaultPanel is not null)
                {
                    var defaultPanel = Panels.Any(x => x.Panel.Equals(DefaultPanel));
                    if (!defaultPanel) throw new UExcept(EInit.DefaultPanelNotFounded, "Указанная панель по умолчанию не находиться в списке панелей");
                }
                #endregion
                #region Меняем на статус по умолчанию без анимации
                if (State is EPanelState.Showen) { await ShowElement(Parent, UseAnimation: false, Pending: true); }
                else { await HideElement(Parent, UseAnimation: false, Pending: false); }
                #endregion
                #region Показываем блок по умолчанию
                var useAnimation = State is EPanelState.Showen;
                await ChangePanel(DefaultPanel, UseAnimation: useAnimation, ShowThisIfHidden: false);
                #endregion

                return new() { IsSuccess = true };
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
                var uerror = new UError(EInit.Exception, $"Исключение: {ex.Message}");
                Functions.Error(ex, uerror, $"{_failinf}: исключение", _proc);
                return new(uerror);
            }
            #endregion
        }
        #endregion
        #region HidePanels
        public async Task<UResponse> HidePanels(bool UseAnimation = true)
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось изменить состояние блока-родителя";

            #region try
            try
            {
                var hideTasks = new List<Task>();
                foreach (var panelHide in Panels)
                {
                    hideTasks.Add(HideElement(panelHide.Element, UseAnimation));
                }
                await Task.WhenAll(hideTasks);

                return new() { IsSuccess = true };
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
                var uerror = new UError(EChangeParentState.Exception, $"Исключение: {ex.Message}");
                Functions.Error(ex, uerror, $"{_failinf}: исключение", _proc);
                return new(uerror);
            }
            #endregion
        }
        #endregion
        #region ChangePanel
        public async Task<UResponse> ChangePanel(T? panel, bool UseAnimation = true, bool ShowThisIfHidden = true)
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось изменить панель";

            #region try
            try
            {
                #region Наличие панели
                var panelObject = Panels.FirstOrDefault(x => x.Panel.Equals(panel));
                #endregion
                #region Показываем наш блок если надо
                if (ShowThisIfHidden) await ShowElement(Parent, UseAnimation: false, Pending: false);
                #endregion
                #region Скрываем все панели
                var hideTasks = new List<Task>();
                foreach (var panelHide in Panels)
                {
                    if (panel is null || !panelHide.Panel.Equals(panel))
                    {
                        hideTasks.Add(HideElement(panelHide.Element, UseAnimation));
                    }
                }
                await Task.WhenAll(hideTasks);
                #endregion
                #region Показываем нужную
                if (panelObject is not null)
                {
                    await ShowElement(panelObject.Element, UseAnimation: true, Pending: true);
                    SelectedPanel = panelObject.Panel;
                }
                #endregion

                return new() { IsSuccess = true };
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
                var uerror = new UError(EChangePanel.Exception, $"Исключение: {ex.Message}");
                Functions.Error(ex, uerror, $"{_failinf}: исключение", _proc);
                return new(uerror);
            }
            #endregion
        }
        #endregion
    }
}
