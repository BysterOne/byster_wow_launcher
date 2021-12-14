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
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Byster.Models.LocalRotationModels;
using System.IO;
using Newtonsoft.Json;
using Byster.Models.ViewModels;
using Microsoft.Win32;
using Byster.Models.BysterModels;

namespace Byster.Views
{
    /// <summary>
    /// Логика взаимодействия для RotationSettingsWindow.xaml
    /// </summary>
    public partial class RotationSettingsWindow : Window
    {
        public RotationSettingsWindow()
        {
            InitializeComponent();
            this.DataContext = new RotationSettingsViewModel();
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
    }

    public class RotationSettingsViewModel
    {
        public ObservableCollection<LocalRotation> Rotations { get; set; }
        private readonly string pathOfConfigDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BysterConfig";
        private readonly string pathOfRotationsFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BysterConfig\\rotations.json";
        public RotationSettingsViewModel()
        {
            if (!Directory.Exists(pathOfConfigDir)) Directory.CreateDirectory(pathOfConfigDir);
            if (!File.Exists(pathOfRotationsFile)) File.Create(pathOfRotationsFile).Close();
            string rawRotations = File.ReadAllText(pathOfRotationsFile);
            Dictionary<string, bool> rotations = JsonConvert.DeserializeObject<Dictionary<string, bool>>(rawRotations);
            Rotations = new ObservableCollection<LocalRotation>();

            if (rotations != null)
            {
                foreach (var key in rotations.Keys)
                {
                    Rotations.Add(new LocalRotation()
                    {
                        IsEnabled = rotations[key],
                        Path = key,
                        Parent = Rotations,
                    });
                }
            }
        }

        public void AddRotation(string pathOfRotation)
        {
            if (!Rotations.Any((location) => location.Path == pathOfRotation))
            {
                if (!File.Exists(pathOfRotation))
                {
                    return;
                }
                Rotations.Add(new LocalRotation()
                {
                    Path = pathOfRotation,
                    IsEnabled = true,
                    Parent = Rotations,
                });
            }
        }

        public void Save()
        {
            if (!Directory.Exists(pathOfConfigDir)) Directory.CreateDirectory(pathOfConfigDir);
            if (!File.Exists(pathOfRotationsFile)) File.Create(pathOfRotationsFile).Close();
            Dictionary<string, bool> jsonRotations = new Dictionary<string, bool>();
            foreach (var rot in Rotations)
            {
                jsonRotations.Add(rot.Path, rot.IsEnabled);
            }
            File.WriteAllText(pathOfRotationsFile, JsonConvert.SerializeObject(jsonRotations));
        }

        private RelayCommand addCommand;
        private RelayCommand saveCommand;
        private RelayCommand searchCommand;

        public RelayCommand AddCommand
        {
            get
            {
                return addCommand ?? (addCommand = new RelayCommand(() =>
                {
                    OpenFileDialog fileDialog = new OpenFileDialog()
                    {
                        Filter = "Byster Projects|*.toc",
                        CheckFileExists = true,
                        CheckPathExists = true,
                        Title = "Выбор файла",
                    };
                    bool res = fileDialog.ShowDialog() ?? false;
                    if (res)
                    {
                        AddRotation(fileDialog.FileName);
                    }
                }));
            }
        }

        public RelayCommand SaveCommand
        {
            get
            {
                return saveCommand ?? (saveCommand = new RelayCommand(() =>
                {
                    Save();
                }));
            }
        }

        public RelayCommand SearchCommand
        {
            get
            {
                return searchCommand ?? (searchCommand = new RelayCommand((obj) =>
                {
                    string strToSearch = obj as string;
                    strToSearch = strToSearch.ToLower();
                    foreach (var rot in Rotations)
                    {
                        if (string.IsNullOrEmpty(strToSearch) || rot.Path.ToLower().Contains(strToSearch))
                        {
                            rot.IsShowInCollection = Visibility.Visible;
                        }
                        else
                        {
                            rot.IsShowInCollection = Visibility.Collapsed;
                        }
                    }
                }));
            }
        }
    }
}
