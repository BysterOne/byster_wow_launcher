using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Byster.Models.ViewModels;
using System.Collections.ObjectModel;

namespace Byster.Models.LocalRotationModels
{
    public class LocalRotation
    {
        public ObservableCollection<LocalRotation> Parent;
        public string Path { get; set; }
        public bool IsEnabled { get; set; }
        private RelayCommand removeCommand;
        public RelayCommand RemoveCommand
        {
            get {
                return removeCommand ?? (removeCommand = new RelayCommand(() =>
                {
                    Parent?.Remove(this);
                }));
            }
        }
    }
}
