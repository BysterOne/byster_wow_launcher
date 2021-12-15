using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Byster.Models.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;


namespace Byster.Models.LocalRotationModels
{
    public class LocalRotation : INotifyPropertyChanged
    {
        public ObservableCollection<LocalRotation> Parent;

        private string path;
        private bool isEnabled;
        private Visibility isShowInCollection = Visibility.Visible;
        public string Path
        {
            get { return path; }
            set
            {
                path = value;
                OnPropertyChanged("Path");
                OnPropertyChanged("DisplayedPath");
            }
        }

        public string DisplayedPath
        {
            get
            {
                if(Path.Split('\\').Length > 5)
                {
                    string p = "...";
                    string[] parts = Path.Split('\\');
                    for(int i = parts.Length - 5; i < parts.Length; i++)
                    {
                        p += "\\" + parts[i];
                    }
                    return p;
                }
                return Path;
            }
        }
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;
                OnPropertyChanged("IsEnabled");
            }
        }
        public Visibility IsShowInCollection
        {
            get { return isShowInCollection; }
            set
            {
                isShowInCollection = value;
                OnPropertyChanged("IsShowInCollection");
            }
        }
        private RelayCommand removeCommand;
        public RelayCommand RemoveCommand
        {
            get
            {
                return removeCommand ?? (removeCommand = new RelayCommand(() =>
                {
                    Parent?.Remove(this);
                }));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
