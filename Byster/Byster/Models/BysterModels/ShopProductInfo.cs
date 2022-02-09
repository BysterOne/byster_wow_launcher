using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Byster.Models.BysterModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Byster.Models.BysterModels
{
    public class ShopProductInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string property = "")
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        private int count;
        public int Count
        {
            get
            {
                return count;
            }
            set
            {
                count = value;
                OnPropertyChanged("Count");
                OnPropertyChanged("Price");
            }
        }
        public double Price
        {
            get
            {
                return Count * Product.Price;
            }
        }

        private ShopProduct product;

        public ShopProduct Product
        {
            get
            {
                return product;
            }
            set
            {
                product = value;
                OnPropertyChanged("Product");
            }
        }
        
        public ShopProductInfo()
        {
            Count = 0;
        }
        public ShopProductInfo(ShopProduct shopProduct)
        {
            Product = shopProduct;
            Count = 0;
        }
        public void AddOne()
        {
            Count++;
        }

        public void RemoveOne()
        {
            if (Count > 0) Count--;
        }

        public void RemoveAll()
        {
            Count = 0;
        }
    }
}
