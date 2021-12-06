using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Byster.Models.BysterModels;
using System.Windows;

namespace Byster.Models.ViewModels
{
    public class ShopProductInfoViewModel : ShopProductInfo
    {
        public Action<object> TestDel { get; set; }
        public Action CloseDel { get; set; }
        public Action CloseRotationsDel { get; set; }
        public Action<object> AddDel { get; set; }
        public Action<object> RemoveDel { get; set; }

        private Visibility isShowingInShop;
        public Visibility IsShowingInShop
        {
            get { return isShowingInShop; }
            set
            {
                isShowingInShop = value;
                OnPropertyChanged("IsShowingInShop");
            }
        }

        public ShopRotation SelectedRotation;

        public ShopProductInfoViewModel(ShopProduct product, Action<object> testDel = null, Action closeDel = null) : base(product)
        {
            TestDel = testDel;
            CloseDel = closeDel;
            AddDel = null;
            RemoveDel = null;
            IsShowingInShop = Visibility.Collapsed;
        }
        private RelayCommand testCommand;
        public RelayCommand TestCommand
        {
            get
            {
                return testCommand ?? (testCommand = new RelayCommand(TestDel));
            }
        }
        private RelayCommand closeCommand;
        public RelayCommand CloseCommand
        {
            get
            {
                return closeCommand ??
                    (closeCommand = new RelayCommand(CloseDel));
            }
        }
        private RelayCommand addCommand;
        public RelayCommand AddCommand {
            get
            {
                return addCommand ?? (addCommand = new RelayCommand((obj) =>
                {
                    AddDel(obj);
                }));
            }
        }
        private RelayCommand removeCommand;
        public RelayCommand RemoveCommand
        {
            get
            {
                return removeCommand ?? (removeCommand = new RelayCommand((obj) =>
                {
                    RemoveDel(obj);
                }));
            }
        }

        private RelayCommand closeRotationCommand;
        public RelayCommand CloseRotationCommand
        {
            get
            {
                return closeRotationCommand ??
                  (closeRotationCommand = new RelayCommand(() =>
                  {
                      CloseRotationsDel();
                  }));
            }
        }
    }
}
