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
using static Byster.Models.Utilities.BysterLogger;

namespace Byster.Models.Services
{
    public class ShopService : INotifyPropertyChanged, IService
    {
        public bool IsInitialized { get; set; } = false;
        public Dispatcher Dispatcher { get; set; }

        private int bonuses = 0;
        public int Bonuses
        {
            get { return bonuses; }
            set
            {
                bonuses = value;
                OnPropertyChanged("Bonuses");
                OnPropertyChanged("ResultSum");
                OnPropertyChanged("IsPaymentSystemRequired");
                OnPropertyChanged("IsPaymentSystemNotRequired");
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
                OnPropertyChanged("IsPaymentSystemRequired");
                OnPropertyChanged("IsPaymentSystemNotRequired");
            }
        }

        private int selectedPaymentSystemId;
        public int SelectedPaymentSystemId
        {
            get { return selectedPaymentSystemId; }
            set
            {
                selectedPaymentSystemId = value;
                OnPropertyChanged("SelectedPaymentSystemId");
            }
        }

        public double ResultSum
        {
            get
            {
                if (Sum - Bonuses < 0)
                    Bonuses = (int)Sum;
                return Sum - Bonuses;
            }
        }
        public Visibility IsPaymentSystemRequired
        {
            get
            {
                return ResultSum > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility IsPaymentSystemNotRequired
        {
            get
            {
                return ResultSum == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }


        public RestService RestService { get; set; }
        public ObservableCollection<ShopProductInfoViewModel> AllProducts { get; set; }

        public Action CloseElementAction { get; set; }
        public Func<bool> PreTestElementAction { get; set; }
        public Action TestElementSuccessAction { get; set; }
        public Action TestElementFailAction { get; set; }
        public Predicate<Cart> PreBuyCartAction { get; set; }
        public Action BuyCartByBonusesSuccessAction { get; set; }
        public Action BuyCartByBonusesFailAction { get; set; }
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

        public void BuyCart()
        {
            Cart cart = createCartProductCollection();
            if (cart.Products.Count == 0) return;
            if (!PreBuyCartAction?.Invoke(null) ?? false) return;
            cart = createCartProductCollection();
            cart.PaymentSystemId = SelectedPaymentSystemId;
            bool status;
            string link;
            (status, link) = RestService.ExecuteBuyRequest(cart);
            if (ResultSum > 0)
            {
                if (status && !string.IsNullOrEmpty(link))
                {
                    BuyCartSuccessAction?.Invoke(link);
                }
                else
                {
                    BuyCartFailAction?.Invoke();
                }
            }
            else
            {
                if (status)
                {
                    BuyCartByBonusesSuccessAction?.Invoke();
                }
                else
                {
                    BuyCartByBonusesFailAction?.Invoke();
                }
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
            foreach (var product in AllProducts)
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
            LogInfo("ShopService", "Обновление данных магазина...");
            IEnumerable<ShopProductInfoViewModel> products = null;
            products = RestService.GetAllProductCollection();
            if (products.ToArray().Length > 0)
            {
                Dispatcher.Invoke(() =>
                {
                    //AllProducts.Clear();
                    foreach (var item in products)
                    {
                        var existedProduct = AllProducts.Where(product => product.Product.Id == item.Product.Id).FirstOrDefault();
                        if (existedProduct == null)
                        {
                            AllProducts.Add(item);
                        }
                        else
                        {
                            existedProduct = item;
                        }
                    }
                    setElementsActions();
                    FilterProducts();
                });
            }
            LogInfo("ShopService", "Обновление данных магазина завершено");
        }

        public string ActivateCoupon(string couponCode)
        {
            return RestService.ExecuteCouponRequest(couponCode) ? "success" : RestService.LastError;
        }
        private void setElementsActions()
        {
            foreach (var product in AllProducts)
            {
                product.CloseDel = new Action(() => { CloseElement(); });
                product.TestDel = new Action<object>((obj) => { TestProduct(Convert.ToInt32(obj)); });
                product.AddDel = new Action<object>((obj) => { AddOneToCountInProduct(Convert.ToInt32(obj)); });
                product.RemoveDel = new Action<object>((obj) => { RemoveOneFromCountInProduct(Convert.ToInt32(obj)); });
                product.RemoveAllDel = new Action<object>((obj) => { ClearAllCountFromProduct(Convert.ToInt32(obj)); });
            }
        }

        public void FilterProducts()
        {
            foreach (var product in AllProducts)
            {
                if (FilterOptions.CheckItem(product))
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
            return RestService.GetAllPaymentSystem().ToList();
        }

        private Cart createCartProductCollection()
        {
            double sum = 0;
            List<(int, int)> cartProducts = new List<(int, int)>();
            foreach (var product in AllProducts)
            {
                if (product.Count > 0)
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
            FilterOptions = Filter.GetDefaultFilter();
            FilterOptions.PropertyChanged += (s, e) =>
            {
                FilterProducts();
            };
            FilterOptions.FilterChanged += () =>
            {
                FilterProducts();
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

        }

        public void Initialize(Dispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            IsInitialized = true;
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
                        if (Convert.ToBoolean(obj))
                        {
                            CloseElement();
                        }
                    }));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public void Dispose()
        {

        }
    }

    public class Filter : INotifyPropertyChanged
    {
        public event Action FilterChanged;
        public void OnFilterChanged()
        {
            FilterChanged?.Invoke();
        }
        public ObservableCollection<IFilterItem<FilterClass>> FilterClasses { get; set; }
        public ObservableCollection<IFilterItem<string>> FilterTypes { get; set; }

        public static Filter GetDefaultFilter()
        {
            var filter = new Filter();
            filter.FilterClasses = FilterItemWowClass.GetAllFilterItems();
            foreach (var item in filter.FilterClasses)
            {
                item.PropertyChanged += (s, e) => filter.OnFilterChanged();
            }
            filter.FilterTypes = FilterItemString.GetAllFilterItems();
            foreach (var item in filter.FilterTypes)
            {
                item.PropertyChanged += (s, e) => filter.OnFilterChanged();
            }
            return filter;
        }


        public bool CheckItem(object obj)
        {
            if (!(obj is ShopProductInfo)) return false;
            var product = obj as ShopProductInfo;

            var selectedFilterClasses = FilterClasses.Where(_ifi => _ifi.IsSelected).ToList();
            var selectedFilterTypes = FilterTypes.Where(_ifi => _ifi.IsSelected).ToList();
            if (selectedFilterTypes.Any(_ifi => _ifi.FilterValue.ToLower().Contains("bundle")) && !product.Product.IsPack) return false;
            if (selectedFilterClasses.Count != 0 && !product.Product.Rotations.Any(_rot => _rot.RotationClass.EnumWOWClass == WOWClasses.ANY) && !selectedFilterClasses.Any(_ifi => product.Product.Rotations.Any(_rot => _rot.RotationClass.EnumWOWClass == _ifi.FilterValue.EnumClass))) return false;
            if (selectedFilterTypes.Where(_ifi => !_ifi.FilterValue.ToLower().Contains("bundle")).Count() != 0 && !selectedFilterTypes.Any(_ifi => product.Product.Rotations.Any(_rot => _rot.Type.Name.ToLower() == _ifi.FilterValue.ToLower()))) return false;

            return true;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
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
        public static List<FilterClass> GetAllClasses()
        {
            List<FilterClass> res = new List<FilterClass>();
            for (int i = 1; i < 11; i++)
            {
                res.Add(new FilterClass((WOWClasses)i));
            }
            return res;
        }
    }

    public class FilterItemWowClass : IFilterItem<FilterClass>
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        public FilterClass FilterValue { get; set; }
        public bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
        public static ObservableCollection<IFilterItem<FilterClass>> GetAllFilterItems()
        {
            var res = new ObservableCollection<IFilterItem<FilterClass>>();
            foreach (var item in FilterClass.GetAllClasses())
            {
                res.Add(new FilterItemWowClass()
                {
                    FilterValue = item,
                    IsSelected = false,
                });
            }
            return res;
        }
    }

    public class FilterItemString : IFilterItem<string>
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        public string FilterValue { get; set; }
        public bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
        public static ObservableCollection<IFilterItem<string>> GetAllFilterItems()
        {
            var res = new ObservableCollection<IFilterItem<string>>();
            string[] types = { "Bot", "Utility", "PvE", "PvP", "Bundle" };
            foreach (var item in types)
            {
                res.Add(new FilterItemString()
                {
                    FilterValue = item,
                    IsSelected = false,
                });
            }
            return res;
        }
    }

    public interface IFilterItem<T> : INotifyPropertyChanged
    {
        bool IsSelected { get; set; }
        T FilterValue { get; set; }
    }
}
