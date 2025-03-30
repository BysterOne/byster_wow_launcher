using System;
using System.Collections.Generic;
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
using Cls.Any;
using Launcher.Any;


namespace Launcher.Components.MainWindow
{
    /// <summary>
    /// Логика взаимодействия для CPageMain.xaml
    /// </summary>
    public partial class CPageMain : UserControl, ITransferable
    {
        public CPageMain()
        {
            InitializeComponent();
        }



        #region Функции


        #region UpdateAllValues
        public async Task UpdateAllValues()
        {
            APE_text_block.Text = Dictionary.Translate("На данный момент у Вас нет ротаций. Их можно приобрести в магазине");

        }
        #endregion
        #endregion
    }
}
