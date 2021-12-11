using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Byster.Models.BysterModels;
using Byster.Models.ViewModels;
using System.Windows.Threading;
using System.Threading;
using System.Windows;

namespace Byster.Models.Services
{
    public class ShopService : INotifyPropertyChanged, IService
    {
        public Dispatcher Dispatcher { get; set; }
        public string SessionId { get; set; }
        private int bonuses = 0;
        public int Bonuses
        {
            get { return bonuses; }
            set
            {
                bonuses = value;
                OnPropertyChanged("Bonuses");
                OnPropertyChanged("ResultSum");
            }
        }

        private double sum = 0;
        public double Sum
        {
            get { return sum; }
            set
            {
                sum = value;
                OnPropertyChanged("Sum");
                OnPropertyChanged("ResultSum");
            }
        }

        public double ResultSum
        {
            get
            {
                if ((Sum-1) - Bonuses < 0)
                    Bonuses = (int)(Sum-1);
                return Sum - Bonuses;
            }
        }
        public RestService RestService { get; set; }
        public ObservableCollection<ShopProductInfoViewModel> AllProducts { get; set; }

        public Action CloseElementAction { get; set; }
        public Func<bool> PreTestElementAction { get; set; }
        public Action TestElementSuccessAction { get; set; }
        public Action TestElementFailAction { get; set; }
        public Func<int> PreBuyCartAction { get; set; }
        public Action<string> BuyCartSuccessAction { get; set; }
        public Action BuyCartFailAction { get; set; }


        public event Action<double> CartProductRemoved;
        public event Action<double> CartProductAdded;
        public event Action CartProductCleared;

        private Filter filterOptions;
        public Filter FilterOptions
        {
            get { return filterOptions; }
            set
            {
                filterOptions = value;
                OnPropertyChanged("FilterOptions");
            }
        }

        public ObservableCollection<FilterClass> AllClasses { get; set; } = FilterClass.GetAllClasses();
        public ObservableCollection<string> AllTypes { get; set; } = new ObservableCollection<string>
        {
            "Bot",
            "Utility",
            "PvE",
            "PvP"
        };

        public void BuyCart()
        {
            Cart cart = createCartProductCollection();
            if (cart.Products.Count == 0) return;
            int paymentSystemId = PreBuyCartAction?.Invoke() ?? 3;
            if (paymentSystemId == -1) return;
            bool status;
            string link;
            (status, link) = RestService.ExecuteBuyRequest(cart, paymentSystemId);
            if(status && !string.IsNullOrEmpty(link))
            {
                BuyCartSuccessAction?.Invoke(link);
            }
            else
            {
                BuyCartFailAction?.Invoke();
            }
        }

        public void TestProduct(int id)
        {
            if (!(PreTestElementAction?.Invoke() ?? true)) return;
            bool status = RestService.ExecuteTestRequest(id);
            if (status)
            {
                TestElementSuccessAction?.Invoke();
            }
            else
            {
                TestElementFailAction?.Invoke();
            }
            
        }
        public void CloseElement()
        {
            CloseElementAction?.Invoke();
        }
        public void ClearCart()
        {
            foreach(var product in AllProducts)
            {
                product.RemoveAll();
            }
            Bonuses = 0;
            CartProductCleared?.Invoke();
        }

        public void AddOneToCountInProduct(int id)
        {
            var changingProduct = getProductById(id);
            CartProductAdded?.Invoke(changingProduct.Product.Price);
            changingProduct.AddOne();
        }

        public void RemoveOneFromCountInProduct(int id)
        {
            var changingProduct = getProductById(id);
            CartProductRemoved?.Invoke(changingProduct.Product.Price);
            changingProduct.RemoveOne();
        }

        public void ClearAllCountFromProduct(int id)
        {
            var changingProduct = getProductById(id);
            CartProductRemoved.Invoke(changingProduct.Product.Price * changingProduct.Count);
            changingProduct.RemoveAll();
        }

        public void UpdateData()
        {
            AllProducts.Clear();
            var newProducts = RestService.GetAllProductCollection();
            foreach (var product in newProducts)
            {
                AllProducts.Add(product);
            } 
            FilterProducts();
            setElementsActions();
        }

        
        private void setElementsActions()
        {
            foreach(var product in AllProducts)
            {
                product.CloseDel = new Action(() => { CloseElement(); });
                product.TestDel = new Action<object>((obj) => { TestProduct(Convert.ToInt32(obj)); });
                product.AddDel = new Action<object>((obj) => { AddOneToCountInProduct(Convert.ToInt32(obj)); });
                product.RemoveDel = new Action<object>((obj) => { RemoveOneFromCountInProduct(Convert.ToInt32(obj)); });
            }
        }

        public void FilterProducts()
        {
            foreach (var product in AllProducts)
            {
                if (checkProductByFilterOptions(product, FilterOptions))
                {
                    product.IsShowingInShop = Visibility.Visible;
                }
                else
                {
                    product.IsShowingInShop = Visibility.Collapsed;
                }
            }
        }

        public List<PaymentSystem> GetAllPaymentSystemsList()
        {
            return RestService.GetAllPaymentSystemList();
        }


        private bool checkProductByFilterOptions(ShopProductInfo product, Filter filterOptions)
        {
            foreach (var rotation in product.Product.Rotations)
            {
                if (filterOptions?.FilterTypes?.Count == 0 ||
                    (filterOptions?.FilterTypes?.Any((ftype) => ftype.ToLower() == rotation.Type.ToLower()) ?? false))
                    if ((rotation.RotationClass.EnumWOWClass == WOWClasses.ANY ||
                        filterOptions?.FilterClasses?.Count == 0 ||
                        (filterOptions?.FilterClasses?.Any((fclass) => fclass.EnumClass == rotation.RotationClass.EnumWOWClass) ?? false)))
                        return true;
            }
            return false;
        }

        private Cart createCartProductCollection()
        {
            double sum = 0;
            List<(int, int)> cartProducts = new List<(int, int)> ();
            foreach (var product in AllProducts)
            {
                if(product.Count > 0)
                {
                    cartProducts.Add((product.Product.Id, product.Count));
                    sum += product.Product.Price * product.Count;
                }
            }
            Cart cart = new Cart()
            {
                Bonuses = Bonuses,
                Products = cartProducts,
                Sum = sum,
            };
            return cart;
        }

        private ShopProductInfo getProductById(int id)
        {
            return AllProducts.First(_product => _product.Product.Id == id);
        }

        public ShopService(RestService restService)
        {
            RestService = restService;
            AllProducts = new ObservableCollection<ShopProductInfoViewModel>();
            FilterOptions = new Filter()
            {
                FilterClasses = new ObservableCollection<FilterClass>()
                { },
                FilterTypes = new ObservableCollection<string>()
                { },
            };
            AllProducts.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler((obj, e) =>
            {
                FilterProducts();
            });
            CartProductAdded += new Action<double>((price) =>
            {
                Sum += price;
            });
            CartProductRemoved += new Action<double>((price) =>
            {
                Sum -= price;
            });
            CartProductCleared += new Action(() =>
            {
                Sum = 0;
            });
            UpdateData();
        }

        public RelayCommand BuyCartCommand
        {
            get
            {
                return new RelayCommand(BuyCart);
            }
        }

        public RelayCommand ClearCartCommand
        {
            get
            {
                return new RelayCommand(ClearCart);
            }
        }

        private RelayCommand closeProduct;
        public RelayCommand CloseProduct
        {
            get
            {
                return closeProduct ??
                    (closeProduct = new RelayCommand((obj) =>
                    {
                        if(Convert.ToBoolean(obj))
                        {
                            CloseElement();
                        }
                    }));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }

    public class Filter : INotifyPropertyChanged
    {
        public ObservableCollection<FilterClass> FilterClasses { get; set; }
        public ObservableCollection<string> FilterTypes { get; set; }
        
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
    
    public class FilterClass
    {
        public string Name { get; set; }
        public WOWClasses EnumClass { get; set; }

        public FilterClass(WOWClasses enClass)
        {
            Name = new ClassWOW(enClass).NameOfClass.Normalize();
            EnumClass = enClass;
        }
        public static ObservableCollection<FilterClass> GetAllClasses()
        {
            ObservableCollection<FilterClass> res = new ObservableCollection<FilterClass>();
            for (int i = 1; i < 11; i++)
            {
                res.Add(new FilterClass((WOWClasses)i));
            }
            return res;
        }
    }
}
