using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Byster.Models.BysterModels;

namespace Byster.Models.Services
{
    public class ShopService : INotifyPropertyChanged, IService
    {
        public RestService RestService { get; set; }
        public ObservableCollection<ShopProductInfo> AllProducts { get; set; }
        public ObservableCollection<ShopProductInfo> FilteredProducts { get; set; }

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

        public void BuyCart(int[] ids)
        {

        }

        public void TestProduct(int id)
        {

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
            foreach(var product in AllProducts)
            {
                if(checkProductByFilterOptions(product, FilterOptions)) FilteredProducts.Add(product);
            }
        }

        private bool checkProductByFilterOptions(ShopProductInfo product, Filter filterOptions)
        {
            foreach(var rotation in product.Product.Rotations)
            {
                if ((filterOptions?.FilterClass?.Contains(rotation.RotationClass.EnumWOWClass) ?? true) &&
                    (filterOptions?.FilterType?.Contains(rotation.Type) ?? true)) return true;
            }
            return false;
        }

        private ShopProductInfo getProductById(int id)
        {
            return AllProducts.First(_product => _product.Product.Id == id);
        }

        public ShopService()
        {
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
        }
    }

    public class Filter
    {
        public List<WOWClasses> FilterClass { get; set; }
        public List<string> FilterType { get; set; }
    }
}
