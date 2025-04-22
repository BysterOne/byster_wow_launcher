using Launcher.Api.Models;
using Launcher.Components.MainWindow.Any.PageShop.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Launcher.Any
{
    public class Cart
    {
        public Cart()
        {

        }

        #region События
        public delegate void CartUpdatedDelegate(CCartItem item, ListChangedType changedType);
        public event CartUpdatedDelegate? CartUpdated;

        public delegate void CartSumUpdatedDelegate(double sum);
        public event CartSumUpdatedDelegate? CartSumUpdated;
        #endregion

        #region Переменные
        public double Sum { get; set; } = 0;
        public ObservableCollection<CCartItem> Items { get; } = [];
        #endregion

        #region Обработчики событий

        #endregion

        #region Функции
        #region GetItem
        public CCartItem? GetItem(Product product)
        {
            return Items.FirstOrDefault(x => x.Product.Id == product.Id);
        }
        #endregion
        #region AddItem
        public void AddItem(Product product)
        {
            var item = Items.FirstOrDefault(x => x.Product.Id == product.Id);
            if (item is null)
            {
                item = new CCartItem() { Product = product, Count = 1 };
                Items.Add(item);
                CartUpdated?.Invoke(item, ListChangedType.ItemAdded);
            }

            UpdateSum();
        }
        #endregion
        #region RemoveItem
        public void RemoveItem(Product product)
        {
            var item = Items.FirstOrDefault(x => x.Product.Id == product.Id);
            if (item is null) return;

            Items.Remove(item);
            CartUpdated?.Invoke(item, ListChangedType.ItemDeleted);

            UpdateSum();
        }
        #endregion
        #region ChangeCount
        public void ChangeCount(Product product, int newCount)
        {
            var item = Items.FirstOrDefault(x => x.Product.Id == product.Id);
            if (item is null)
            {
                item = new CCartItem() { Product = product, Count = 1 };
                Items.Add(item);
                CartUpdated?.Invoke(item, ListChangedType.ItemAdded);
            }

            if (newCount <= 0) 
            {
                Items.Remove(item);
                CartUpdated?.Invoke(item, ListChangedType.ItemDeleted);
            }
            else
            {
                if (newCount <= 99)
                {
                    item.Count = newCount;
                    CartUpdated?.Invoke(item, ListChangedType.ItemChanged);
                }                
            }

            UpdateSum();
        }
        #endregion
        #region UpdateSum
        private void UpdateSum()
        {
            var sum = Items.Select(x => x.Product.Price * x.Count).Sum();
            if (Sum != sum) { Sum = sum; CartSumUpdated?.Invoke(Sum); }
        }
        #endregion
        #endregion
    }
}
