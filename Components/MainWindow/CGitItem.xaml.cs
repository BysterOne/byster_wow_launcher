using Cls;
using Cls.Any;
using Cls.Exceptions;
using Launcher.Api.Models;
using Launcher.Components.MainWindow.GitItemAny;
using Launcher.Settings;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Launcher.Components.MainWindow
{
    namespace GitItemAny
    {
        public enum EGitItem
        {
            FailUpdateView,
        }
    }
    /// <summary>
    /// Логика взаимодействия для CGitItem.xaml
    /// </summary>
    public partial class CGitItem : UserControl
    {
        public CGitItem()
        {
            InitializeComponent();

            this.Loaded += ELoaded;
        }

        #region Переменные
        private static LogBox Pref { get; set; } = new("Git Item");
        #endregion

        #region Свойства
        #region GitLib
        public CGitDirectory? GitLib
        {
            get => (CGitDirectory)GetValue(GitLibProperty);
            set => SetValue(GitLibProperty, value);
        }

        public static readonly DependencyProperty GitLibProperty =
            DependencyProperty.Register(nameof(GitLib), typeof(CGitDirectory), typeof(CGitItem), 
                new PropertyMetadata(null, OnLibChanged));

        private static void OnLibChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CGitItem sender) _ = sender.UpdateView();            
        }
        #endregion
        #endregion

        #region Обработчики событий
        #region ELoaded
        private void ELoaded(object sender, RoutedEventArgs e)
        {
            //_ = UpdateView();
        }
        #endregion
        #endregion

        #region Функции
        #region UpdateView
        private async Task UpdateView()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось обновить вид компонента";

            #region try
            try
            {
                #region Если пустой
                if (GitLib is null) return;
                #endregion
                #region Установка данных
                CP_name.Text = GitLib.Name;
                CP_path.Text = $"../{GitLib.FilePath}";
                CP_type.Text = GitLib.Type;
                CP_class.Text = GStatic.GetRotationClassName(GitLib.Class);
                #endregion
                #region Обновление статуса синхронизации
                UpdateSyncState();
                #endregion
                #region Async
                await Task.Run(() => { return; });
                #endregion
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uex = new UExcept(EGitItem.FailUpdateView, _failinf, ex);
                Functions.Error(uex, uex.Message, _proc);               
            }
            #endregion
        }
        #endregion
        #region UpdateSyncState
        private void UpdateSyncState()
        {

        }
        #endregion
        #endregion
    }
}
