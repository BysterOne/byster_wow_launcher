using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Byster.Models.BysterModels;
using Byster.Models.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

namespace Byster.Models.Services
{
    public class ActiveRotationsService : INotifyPropertyChanged, IService
    {
        public Dispatcher Dispatcher { get; set; }
        public string SessionId { get; set; }
        public RestService RestService { get; set; }
        public ObservableCollection<ActiveRotationViewModel> AllActiveRotations { get; set; }
        public ObservableCollection<ActiveRotationViewModel> FilteredActiveRotations { get; set; }

        private WOWClasses filterClass;
        public WOWClasses FilterClass
        {
            get
            {
                return filterClass;
            }
            set
            {
                filterClass = value;
                OnPropertyChanged("FilterClass");
            }
        }

        public void FilterRotations()
        {
            FilteredActiveRotations.Clear();
            foreach(var rotation in AllActiveRotations)
            {
                if (rotation.RotationClass.EnumWOWClass == FilterClass ||
                    rotation.RotationClass.EnumWOWClass == WOWClasses.ANY ||
                    FilterClass == WOWClasses.ANY)
                {
                    FilteredActiveRotations.Add(rotation);
                }
            }
        }

        public void UpdateData()
        {
            AllActiveRotations = RestService.GetActiveRotationCollection();
            FilterRotations();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            FilterRotations();
        }

        public ActiveRotationsService(RestService service)
        {
            AllActiveRotations = new ObservableCollection<ActiveRotationViewModel>();
            FilteredActiveRotations = new ObservableCollection<ActiveRotationViewModel>();
            if(service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }
            RestService = service;
        }
    }
}