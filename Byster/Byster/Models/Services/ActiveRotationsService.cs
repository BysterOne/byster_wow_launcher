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
using System.Collections.Specialized;
using System.Windows.Threading;
using Byster.Models.Utilities;

namespace Byster.Models.Services
{
    public class ActiveRotationsService : INotifyPropertyChanged, IService
    {
        public bool IsInitialized { get; set; } = false;
        public Dispatcher Dispatcher { get; set; }
        public string SessionId { get; set; }
        public RestService RestService { get; set; }
        public ObservableCollection<ActiveRotationViewModel> AllActiveRotations { get; set; }

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
            foreach(var rotation in AllActiveRotations)
            {
                if (rotation.RotationClass.EnumWOWClass == FilterClass ||
                    rotation.RotationClass.EnumWOWClass == WOWClasses.ANY ||
                    FilterClass == WOWClasses.ANY)
                {
                    rotation.IsVisibleInList = true;
                }
                else
                {
                    rotation.IsVisibleInList = false;
                }
            }
        }

        private void sortAllRotations()
        {
            int count = 0;
            for(int i = count; i < AllActiveRotations.Count; i++)
            {
                if (AllActiveRotations[i].Type.ToLower() == "pvp")
                {
                    count++;
                    for (int j = 0; j < i; j++)
                    {
                        if (AllActiveRotations[j].Type.ToLower() != "pvp")
                        {
                            AllActiveRotations.Move(i, j);
                        }
                    }
                }
            }
            for (int i = count; i < AllActiveRotations.Count; i++)
            {
                if (AllActiveRotations[i].Type.ToLower() == "pve")
                {
                    count++;
                    for (int j = 0; j < i; j++)
                    {
                        if (AllActiveRotations[j].Type.ToLower() != "pve" &&
                            AllActiveRotations[j].Type.ToLower() != "pvp")
                        {
                            AllActiveRotations.Move(i, j);
                        }
                    }
                }
            }
            for (int i = count; i < AllActiveRotations.Count; i++)
            {
                if (AllActiveRotations[i].Type.ToLower() == "bot")
                {
                    count++;
                    for (int j = 0; j < i; j++)
                    {
                        if (AllActiveRotations[j].Type.ToLower() != "pve" &&
                            AllActiveRotations[j].Type.ToLower() != "pvp" &&
                            AllActiveRotations[j].Type.ToLower() != "bot")
                        {
                            AllActiveRotations.Move(i, j);
                        }
                    }
                }
            }
        }

        public void UpdateData()
        {
            Task.Run(() =>
            {
                IEnumerable<ActiveRotationViewModel> products = null;
                products = RestService.GetActiveRotationCollection();
                Dispatcher.Invoke(() =>
                {
                    AllActiveRotations.Clear();
                    foreach (var product in products)
                    {
                        AllActiveRotations.Add(product);
                    }
                    sortAllRotations();
                    FilterRotations();
                });
                
            });
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
            if(service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }
            RestService = service;
        }

        public void Initialize(Dispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            IsInitialized = true;
        }
    }
}