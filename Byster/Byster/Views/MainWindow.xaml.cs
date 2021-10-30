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
using Byster.Models.BysterWOWModels;

namespace Byster
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<RotationWOW> Rotations { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Rotations = new ObservableCollection<RotationWOW>()
            {
                new RotationWOW()
                {
                    ImageOfRotation = "/Resources/Images/logo.png",
                    Name = "test1",
                    RotationClass = new WOWClass(WOWClass.WOWClasses.Warrior),
                },
                new RotationWOW
                {
                    ImageOfRotation = "/Resources/Images/logo.png",
                    Name = "test2",
                    RotationClass = new WOWClass(WOWClass.WOWClasses.Priest),
                },
                new RotationWOW
                {
                    ImageOfRotation = "/Resources/Images/logo.png",
                    Name = "test3",
                    RotationClass = new WOWClass(WOWClass.WOWClasses.DeathKnight),
                },
                new RotationWOW
                {
                    ImageOfRotation = "/Resources/Images/logo.png",
                    Name = "test4",
                    RotationClass = new WOWClass(WOWClass.WOWClasses.Droid),
                },
            };
            rotationsView.ItemsSource = Rotations;
        }

        private void minimizeBtn_Click(object sender, RoutedEventArgs e)
        {
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
    }
}
