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
namespace Byster.Models.Services
{
    public class ShopService : INotifyPropertyChanged, IService
    {
        public Dispatcher Dispatcher { get; set; }
        public string SessionId { get; set; }
        public int Bonuses { get; set; }
        public RestService RestService { get; set; }
        public ObservableCollection<ShopProductInfoViewModel> AllProducts { get; set; }
        public ObservableCollection<ShopProductInfoViewModel> FilteredProducts { get; set; }

        private Filter filterOptions;
        public Filter FilterOptions
        {
            get { return filterOptions; }
            set
            {
                filterOptions = value;
                OnPropertyChanged("FilterOrtions");
            }
        }

        public void BuyCart(Action<string> actionToSuccess, Action actionToFail)
        {
            Cart cart = createCartProductCollection();
            (bool status, string link) = RestService.ExecuteBuyRequest(cart);
            if(status)
            {
                actionToSuccess(link);
            }
            else
            {
                actionToFail();
            }
        }

        public void TestProduct(int id, Action actionToSuccess, Action actionToFail)
        {
            bool status = RestService.ExecuteTestRequest(id);
            if(status)
            {
                actionToSuccess();
            }
            else
            {
                actionToFail();
            }
        }

        public void AddOneToCountInProduct(int id)
        {
            var changingProduct = getProductById(id);
            changingProduct.AddOne();
        }

        public void RemoveOneFromCountInProduct(int id)
        {
            var changingProduct = getProductById(id);
            changingProduct.RemoveOne();
        }

        public void ClearAllCountFromProduct(int id)
        {
            var changingProduct = getProductById(id);
            changingProduct.RemoveAll();
        }

        public void UpdateData()
        {
            AllProducts = RestService.GetAllProductCollection();
        }

        public void FilterProducts()
        {
            FilteredProducts.Clear();
            foreach (var product in AllProducts)
            {
                if (checkProductByFilterOptions(product, FilterOptions)) FilteredProducts.Add(product);
            }
        }

        private bool checkProductByFilterOptions(ShopProductInfo product, Filter filterOptions)
        {
            foreach (var rotation in product.Product.Rotations)
            {
                if ((filterOptions?.FilterClass?.Contains(rotation.RotationClass.EnumWOWClass) ?? true) &&
                    (filterOptions?.FilterType?.Contains(rotation.Type) ?? true)) return true;
            }
            return false;
        }

        private Cart createCartProductCollection()
        {
            List<(int, int)> cartProducts = new List<(int, int)> ();
            foreach (var product in AllProducts)
            {
                if(product.Count > 0)
                {
                    cartProducts.Add((product.Product.Id, product.Count));
                }
            }
            Cart cart = new Cart()
            {
                Bonuses = Bonuses,
                Products = cartProducts,
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
            FilteredProducts = new ObservableCollection<ShopProductInfoViewModel>();
            FilterOptions = new Filter()
            {
                FilterClass = new List<WOWClasses> { WOWClasses.ANY },
                FilterType = new List<string> { }
            };

            AllProducts.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler((obj, e) =>
            {
                FilterProducts();
            });
         }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            if(property == "FilterOptions")
            {
                FilterProducts();
            }
            if(property == "AllProducts")
            {
                FilterProducts();
            }
        }
    }

    public class Filter
    {
        public List<WOWClasses> FilterClass { get; set; }
        public List<string> FilterType { get; set; }
    }
}
