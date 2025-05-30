using Byster.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Byster.Models.Utilities
{
    public static class IEnumerableExtensions
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> ie)
        {
            if (ie == null) return null;
            ObservableCollection<T> collection = new ObservableCollection<T>();
            foreach (T item in ie)
            {
                collection.Add(item);
            }
            return collection;
        }
    }
    public static class BysterWindowExtensions
    {
        public static MainWindowViewModel Model { get; set; }
        public static bool ShowModalDialog(this Window window)
        {
            Model.IsModalWindowOpened = Visibility.Visible;
            bool result = window.ShowDialog() ?? false;
            Model.IsModalWindowOpened = Visibility.Collapsed;
            return result;
        }
    }
}
