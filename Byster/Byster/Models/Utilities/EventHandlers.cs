using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Byster.Models.BysterModels;
using System.Windows.Controls;

namespace Byster.Utilities
{
    public partial class BysterEventHandlers
    {
        public void AddProductButton(object sender, RoutedEventArgs e)
        {
            ShopProductInfo shopProductInfo = ((sender as Button).DataContext as ShopProductInfo);
            shopProductInfo.Add();
        }
        public void RemoveProductButton(object sender, RoutedEventArgs e)
        {
            ShopProductInfo shopProductInfo = ((sender as Button).DataContext as ShopProductInfo);
            shopProductInfo.RemoveOne();
        }
        public void TestProductButton(object sender, RoutedEventArgs e)
        {

        }
    }
}
