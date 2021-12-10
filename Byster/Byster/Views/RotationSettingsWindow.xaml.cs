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
        public RotationSettingsWindow(MainWindowViewModel model)
        {
            InitializeComponent();
            this.DataContext = new RotationSettingsViewModel(model);
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
        public Branch SelectedBranch { get; set; }
        public ObservableCollection<LocalRotation> Rotations { get; set; }
        private readonly string pathOfConfigDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BysterConfig";
        private readonly string pathOfRotationsFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BysterConfig\\rotations.json";
        public MainWindowViewModel Model { get; set; }

        public RotationSettingsViewModel(MainWindowViewModel model)
        {
            if(!Directory.Exists(pathOfConfigDir)) Directory.CreateDirectory(pathOfConfigDir);
            if(!File.Exists(pathOfRotationsFile)) File.Create(pathOfRotationsFile).Close();
            string rawRotations = File.ReadAllText(pathOfRotationsFile);
            Model = model;
                switch(model.UserInfo.Branch.ToLower())
                {
                    case "dev":
                        SelectedBranch = Branch.AllBranches.First((branch) => branch.BranchType == BranchType.DEVELOPER);
                    break;    
                    case "test":
                        SelectedBranch = Branch.AllBranches.First((branch) => branch.BranchType == BranchType.TEST);
                    break;
                    default:
                    case "master":
                        SelectedBranch = Branch.AllBranches.First((branch) => branch.BranchType == BranchType.MASTER);
                    break;
                }
            Dictionary<string, bool> rotations = JsonConvert.DeserializeObject<Dictionary<string, bool>>(rawRotations);
            Rotations = new ObservableCollection<LocalRotation>();
            foreach( var key in rotations.Keys )
            {
                Rotations.Add(new LocalRotation()
                {
                    IsEnabled = rotations[key],
                    Path = key,
                    Parent = Rotations,
                });
            }
        }

        public void AddRotation(string pathOfRotation)
        {
            if(!Rotations.Any((location) => location.Path == pathOfRotation))
            {
                if(!File.Exists(pathOfRotation))
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
            foreach(var rot in Rotations)
            {
                jsonRotations.Add(rot.Path, rot.IsEnabled);
            }
            File.WriteAllText(pathOfRotationsFile, JsonConvert.SerializeObject(jsonRotations));
            Model.UserInfo.SetBranch(SelectedBranch);
        }

        private RelayCommand addCommand;
        private RelayCommand saveCommand;

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
    }
}
 