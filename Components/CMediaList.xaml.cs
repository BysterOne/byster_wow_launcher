using Launcher.Api.Models;
using Launcher.Components.MainWindow;
using Launcher.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Launcher.Components
{
    /// <summary>
    /// Логика взаимодействия для CMediaList.xaml
    /// </summary>
    public partial class CMediaList : UserControl
    {
        public CMediaList()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        #region Переменные
        public ObservableCollection<Media> Medias { get; } = [];
        #endregion

        #region Обработчики событий
        #region CMediaItem_MouseLeftButtonDown
        private void CMediaItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = (CMediaItem)sender;
            var index = Medias.IndexOf(item.Item);
            _ = Main.ShowMediaView(Medias.ToList(), index);
        }
        #endregion
        #endregion
    }
}
